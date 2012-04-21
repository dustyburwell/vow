using System;

namespace vow
{
   public class TokenRequestFailed : Exception
   {
      public TokenRequestFailed(Exception innerException)
         : base("Token request failed", innerException)
      {
      }
   }
}