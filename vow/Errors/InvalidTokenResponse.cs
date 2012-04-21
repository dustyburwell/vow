using System;

namespace vow
{
   public class InvalidTokenResponse : Exception
   {
      public InvalidTokenResponse(string responseString)
      {
         ResponseString = responseString;
      }

      public override string Message
      {
         get { return ResponseString; }
      }

      public string ResponseString { get; set; }
   }
}