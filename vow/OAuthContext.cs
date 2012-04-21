using System;
using System.Collections.Generic;
using Owin;
using vow.Extensions;

namespace vow
{
   internal class OAuthContext
   {
      private readonly AppDelegate m_next;
      private readonly IDictionary<string, object> m_env;
      private readonly ResultDelegate m_result;
      private readonly Action<Exception> m_fault;

      public OAuthContext(AppDelegate next, IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
      {
         m_next = next;
         m_env = env;
         m_result = result;
         m_fault = fault;
      }

      public void Next(ResultDelegate callback)
      {
         m_next(m_env, callback, m_fault);
      }

      public void Redirect(string to)
      {
         Redirect(to, new Dictionary<string, IEnumerable<string>>());
      }

      public void Redirect(string to, IDictionary<string, IEnumerable<string>> headers)
      {
         headers.Add("Location", new[] {to});
         m_result("302 Found", headers, m_env.Get<BodyDelegate>(OwinConstants.RequestBody));
      }

      public void Fault(Exception e)
      {
         m_fault(e);
      }
   }
}