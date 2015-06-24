using HybridWebApp.Toolkit.Identity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace HybridWebApp.Toolkit.Identity
{
    public interface IWebAuthHelper
    {
        void StartWebAuthentication();

        string ContinueWebAuthentication(WebAuthenticationResult result);

        Task<UserInfo> RetrieveForAccessToken(string accessToken);
    }
}
