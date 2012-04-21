using Gate;
using Owin;

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
         return (env, result, fault) => new OAuthMiddleware(app, config, env, result, fault).Handle();
      }
   }
}
