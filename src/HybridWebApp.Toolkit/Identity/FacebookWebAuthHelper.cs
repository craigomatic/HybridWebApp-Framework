using HybridWebApp.Toolkit.Identity.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Web.Http;

namespace HybridWebApp.Toolkit.Identity
{
    public class FacebookWebAuthHelper : WebAuthHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="scopes"></param>
        /// <param name="authenticationAction">First param is the request URI, second the callback URI</param>
        public FacebookWebAuthHelper(string appId, string scopes, Action<Uri, Uri> authenticationAction)
            :base(appId, scopes, authenticationAction)
        {
            this.Provider = "facebook";
        }

        public override void StartWebAuthentication()
        {
            var callbackUri = new Uri("https://www.facebook.com/connect/login_success.html");
            var requestUri = new Uri(string.Format("https://m.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&response_type=token&scope={2}", _AppID, callbackUri.ToString(), _Scopes));

            _AuthenticationAction(requestUri, callbackUri);
        }

        public override async Task<UserInfo> RetrieveForAccessToken(string accessToken)
        {
            return await _FetchUserInfo(new Uri(string.Format("https://graph.facebook.com/me?access_token={0}", accessToken)));
        }
    }
}
