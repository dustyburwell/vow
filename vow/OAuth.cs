using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Gate;
using Owin;
using vow.Extensions;

namespace vow
{
   /// <summary>
   /// Sets content type in response if none present
   /// </summary>
   public static class OAuth
   {
      public static IAppBuilder UseOAuth(this IAppBuilder builder, OAuthConfiguration configuration)
      {
         return builder.Use(Middleware, configuration);
      }

      public static AppDelegate Middleware(AppDelegate app, OAuthConfiguration config)
      {
         return (env, result, fault) => {
            //HandleToken
            if (HandleCode(env, result, config))
               return;

            //HandleCookie
            if (HandleCookie(env, config))
               return;

            //HandleErrors
            if (HandleErrors(env, result))
               return;

            app(env, (status, headers, body) => {
               if (status.ToLower() == "401 unauthorized")
               {
                  var redirectTo = string.Format(
                     "{0}?client_id={1}&redirect_url={2}",
                     config.AuthorizeEndpoint,
                     config.ClientId,
                     env.GetUri());

                  result(
                     "302 Found",
                     new Dictionary<string, IEnumerable<string>> {
                        { "Location", new [] { redirectTo }}
                     }, body);
               }
               else
                  result(status, headers, body);
             },
             fault);
         };
      }

      private static bool HandleCode(IDictionary<string, object> env, ResultDelegate result, OAuthConfiguration config)
      {
         var code = env.GetQueryParameter("code");

         if (string.IsNullOrWhiteSpace(code))
            return false;

         var tokenRequest = string.Format(
            "client_id={0}&client_secret={1}&code={2}", config.ClientId, config.ClientSecret, code);

         try
         {
            string tokenResponse = 
               new WebClient().UploadString(config.TokenEndpoint, tokenRequest);

            string tokenValue = Query.ParseFormEncodedString(tokenResponse)["access_token"].First();

            // todo: check that the response is well formed.

            var url = env.GetUri();

            url.Query = Query.ParseFormEncodedString(url.Query)
               .Where(g => g.Key != "code")
               .ToQueryString();
            
            result(
               "302 Found", 
               new Dictionary<string, IEnumerable<string>> {
                  {"Location", new [] {url.ToString()}},
                  {"Set-Cookie", new [] {string.Format("{0}={1}", config.CookieName, tokenValue)}}
               }, 
               env.Get<BodyDelegate>(OwinConstants.RequestBody, EmptyBody));

            return true;
         }
         catch (Exception)
         {
            // output errors
            throw;
         }

         return false;
      }

      private static bool HandleCookie(IDictionary<string, object> env, OAuthConfiguration config)
      {
         string oauthCookie = env.GetCookie(config.CookieName);

         if (oauthCookie != null)
         {
            config.TokenCallback(oauthCookie);
         }
         
         return false;
      }

      private static bool HandleErrors(IDictionary<string, object> env, ResultDelegate result)
      {
         return false;
      }

      private static void EmptyBody(
         Func<ArraySegment<byte>, bool> write, 
         Func<Action, bool> flush, 
         Action<Exception> end, 
         CancellationToken cancellationToken)
      {
         end(null);
      }
   }
}
