using System;
using System.Text;

namespace vow
{
   public class OAuthError : Exception
   {
      public OAuthError(string error)
      {
         Error = error;
      }

      public string Error { get; set; }
      public string ErrorDescription { get; set; }
      public string ErrorUri { get; set; }

      public override string ToString()
      {
         var message = new StringBuilder();

         message.AppendFormat("OAuth Error ({0})", Error);

         if (!string.IsNullOrWhiteSpace(ErrorDescription))
            message.AppendFormat(" {0}", ErrorDescription);

         if (!string.IsNullOrWhiteSpace(ErrorUri))
            message.AppendFormat(" (Visit {0} for more details)", ErrorUri);

         return message.ToString();
      }
   }
}