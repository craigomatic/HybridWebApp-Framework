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
    public abstract class WebAuthHelper : IWebAuthHelper
    {
        public string Provider { get; protected set; }

        protected string _AppID;
        protected string _Scopes;
        protected Action<Uri, Uri> _AuthenticationAction;

        public WebAuthHelper(string appId, string scopes, Action<Uri, Uri> authenticationAction)
        {
            _AppID = appId;
            _Scopes = scopes;
            _AuthenticationAction = authenticationAction;
        }

        public abstract void StartWebAuthentication();


        /// <summary>
        /// Continues the authentication and returns the access_token
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public string ContinueWebAuthentication(WebAuthenticationResult result)
        {
            if (result.ResponseStatus == Windows.Security.Authentication.Web.WebAuthenticationStatus.Success)
            {
                var needle = "access_token=";
                var tokenStartIndex = result.ResponseData.IndexOf(needle) + needle.Length;
                var tokenEndIndex = result.ResponseData.IndexOf("&", tokenStartIndex);
                var accessToken = result.ResponseData.Substring(tokenStartIndex, tokenEndIndex - tokenStartIndex);

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access Token not found");
                }

                return accessToken;
            }

            throw new Exception("ResponseStatus does not indicate success.");
        }

        public abstract Task<Model.UserInfo> RetrieveForAccessToken(string accessToken);

        protected async Task<UserInfo> _FetchUserInfo(Uri graphUri)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(graphUri);
                var json = await response.Content.ReadAsStringAsync();

                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<UserInfo>(json));
            }
        }
    }
}
