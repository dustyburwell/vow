using System;

namespace vow
{
   public class InvalidTokenResponse : Exception
   {
      public InvalidTokenResponse(string responseString)
      {
         ResponseString = responseString;
      }

      public string ResponseString { get; set; }
   }
}