using System;

namespace vow
{
   public class OAuthConfiguration
   {
      public Action<string> TokenCallback { get; set; }
      public string ClientId { get; set; }
      public string ClientSecret { get; set; }
      public string AuthorizeEndpoint { get; set; }
      public string TokenEndpoint { get; set; }
      public string CookieName { get; set; }
   }
}