using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Owin;
using vow.Extensions;

namespace vow
{
   internal class OAuthMiddleware
   {
      private readonly OAuthConfiguration m_config;
      private readonly IDictionary<string, object> m_env;
      private readonly ResultDelegate m_result;
      private readonly OAuthContext m_ctx;

      public OAuthMiddleware(
         AppDelegate next, 
         OAuthConfiguration config, 
         IDictionary<string, object> env, 
         ResultDelegate result, 
         Action<Exception> fault)
      {
         m_config = config;
         m_env = env;
         m_result = result;
         
         m_ctx = new OAuthContext(next, m_env, m_result, fault);
      }

      public void Handle()
      {
         if (HandleCode())
            return;

         if (HandleCookie())
            return;

         if (HandleErrors())
            return;

         m_ctx.Next(RedirectIfUnauthorized);
      }

      private void RedirectIfUnauthorized(string status, IDictionary<string, IEnumerable<string>> headers, BodyDelegate body)
      {
         if (status.ToLower() == "401 unauthorized")
         {
            var to = string.Format("{0}?client_id={1}&redirect_url={2}", m_config.AuthorizeEndpoint, m_config.ClientId, m_env.GetUri());
            m_ctx.Redirect(to);
         }
         else
         {
            m_result(status, headers, body);
         }
      }

      private bool HandleCode()
      {
         var code = m_env.GetQueryParameter("code");

         if (string.IsNullOrWhiteSpace(code))
            return false;

         var tokenRequest = string.Format(
            "client_id={0}&client_secret={1}&code={2}", m_config.ClientId, m_config.ClientSecret, code);

         string tokenResponse;
         try
         {
            tokenResponse = new WebClient().UploadString(m_config.TokenEndpoint, tokenRequest);
         }
         catch (Exception e)
         {
            m_ctx.Fault(new TokenRequestFailed(e));
            return true;
         }

         string tokenValue = Query.ParseFormEncodedString(tokenResponse)["access_token"].FirstOrDefault();

         if (string.IsNullOrWhiteSpace(tokenValue))
         {
            m_ctx.Fault(new InvalidTokenResponse(tokenResponse));
            return true;
         }

         var url = m_env.GetUri();

         url.Query = Query.ParseFormEncodedString(url.Query)
            .Where(g => g.Key != "code")
            .ToQueryString();

         m_ctx.Redirect(url.ToString(), new Dictionary<string, IEnumerable<string>> {
               {"Set-Cookie", new [] {string.Format("{0}={1}", m_config.CookieName, tokenValue)}}
            });
         
         return true;
      }

      private bool HandleCookie()
      {
         string oauthCookie = m_env.GetCookie(m_config.CookieName);

         if (oauthCookie != null)
            m_config.TokenCallback(oauthCookie);

         return false;
      }

      private bool HandleErrors()
      {
         var error = m_env.GetQueryParameter("error");

         if (error == null)
            return false;

         var errorDescription = m_env.GetQueryParameter("error_description");
         var errorUri = m_env.GetQueryParameter("error_uri");

         m_ctx.Fault(new OAuthError(error) {
            ErrorDescription = errorDescription,
            ErrorUri = errorUri
         });

         return true;
      }
   }
}