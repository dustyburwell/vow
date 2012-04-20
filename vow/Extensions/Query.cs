using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Owin;

namespace vow.Extensions
{
   public static class Query
   {
      public static string GetQueryParameter(this IDictionary<string, object> env, string name)
      {
         return GetQueryParameters(env, name).FirstOrDefault();
      }

      public static IEnumerable<string> GetQueryParameters(this IDictionary<string, object> env, string name)
      {
         return GetQueryLookup(env)[name];
      }

      public static ILookup<string, string> GetQueryLookup(this IDictionary<string, object> env)
      {
         var queryString = env.Get(OwinConstants.RequestQueryString, string.Empty);

         return ParseFormEncodedString(queryString);
      }

      public static ILookup<string, string> ParseFormEncodedString(string value)
      {
         return Enumerable.ToLookup(value.Split(new[] { '&', '?' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(qp => qp.Split('='))
                         .Select(a => new { Key = a[0], Value = a[1] }), a => a.Key, a => a.Value);
      }

      public static string ToQueryString(this IEnumerable<IGrouping<string, string>> lookup)
      {
         return lookup
            .SelectMany(grouping => grouping.Select(i => new { Key = grouping.Key, Value = i }))
            .Aggregate(new StringBuilder(), (sb, a) => sb.AppendFormat("{0}={1}&", a.Key, a.Value))
            .ToString();
      }
   }
}