using System;
using System.Collections.Generic;
using System.Linq;
using Gate;
using Owin;

namespace vow.Extensions
{
   public static class Cookie
   {
      public static string GetCookie(this IDictionary<string, object> env, string name)
      {
         string value;
         GetCookies(env).TryGetValue(name, out value);
         return value;
      }

      public static IDictionary<string, string> GetCookies(this IDictionary<string, object> env)
      {
         var headers = Get<IDictionary<string, IEnumerable<string>>>(env, OwinConstants.RequestHeaders, null);
         return headers.GetCookies();
      }

      public static string GetCookie(this IDictionary<string, IEnumerable<string>> headers, string name)
      {
         string value;
         GetCookies(headers).TryGetValue(name, out value);
         return value;
      }

      public static IDictionary<string, string> GetCookies(this IDictionary<string, IEnumerable<string>> headers)
      {
         if (headers == null || !headers.HasHeader("Cookie"))
            return new Dictionary<string, string>();

         var cookiesString = headers["Cookie"].First();
         var cookies = cookiesString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

         return cookies
            .Select(s => s.Trim().Split('='))
            .Select(c => new { Key = c[0], Value = c[1] })
            .ToDictionary(c => c.Key, c => c.Value);
      }

      public static T Get<T>(this IDictionary<string, object> env, string key)
      {
         return env.Get(key, default(T));
      }

      public static T Get<T>(this IDictionary<string, object> env, string key, T defaultValue)
      {
         object value;
         return env.TryGetValue(key, out value) && value is T ? (T)value : defaultValue;
      }

      public static UriBuilder GetUri(this IDictionary<string, object> env)
      {
         return new UriBuilder {
            Scheme = env.Get(OwinConstants.RequestScheme, "http"),
            Host = env.Get<string>("server.SERVER_NAME"),
            Port = Int32.Parse(env.Get<string>("server.SERVER_PORT")),
            Path = env.Get<string>(OwinConstants.RequestPath),
            Query = env.Get<string>(OwinConstants.RequestQueryString)
         };
      }
   }
}