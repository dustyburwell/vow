﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Gate;
using Owin;
using vow.Extensions;

namespace vow
{
   /// <summary>
   /// Initiates the OAuth 2.0 protocol on a 401 status code.
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
            if (HandleCode(env, result, fault, config))
               return;

            if (HandleCookie(env, config))
               return;

            if (HandleErrors(env, fault))
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
               {
                  result(status, headers, body);
               }
             },
             fault);
         };
      }

      private static bool HandleCode(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault, OAuthConfiguration config)
      {
         var code = env.GetQueryParameter("code");

         if (string.IsNullOrWhiteSpace(code))
            return false;

         var tokenRequest = string.Format(
            "client_id={0}&client_secret={1}&code={2}", config.ClientId, config.ClientSecret, code);

         string tokenResponse;
         try
         {
            tokenResponse = new WebClient().UploadString(config.TokenEndpoint, tokenRequest);
         }
         catch (Exception e)
         {
            fault(new TokenRequestFailed(e));
            return true;
         }

         string tokenValue = Query.ParseFormEncodedString(tokenResponse)["access_token"].FirstOrDefault();

         if (string.IsNullOrWhiteSpace(tokenValue))
         {
            fault(new InvalidTokenResponse(tokenResponse));
            return true;
         }

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
            env.Get<BodyDelegate>(OwinConstants.RequestBody));

         return true;
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

      private static bool HandleErrors(IDictionary<string, object> env, Action<Exception> fault)
      {
         var error = env.GetQueryParameter("error");

         if (error == null)
            return false;

         var errorDescription = env.GetQueryParameter("error_description");
         var errorUri = env.GetQueryParameter("error_uri");

         fault(new OAuthError(error) {
            ErrorDescription = errorDescription,
            ErrorUri = errorUri
         });

         return true;
      }
   }
}
