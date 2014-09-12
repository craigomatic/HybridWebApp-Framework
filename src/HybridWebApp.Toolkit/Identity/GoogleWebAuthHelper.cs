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
    public class GoogleWebAuthHelper : WebAuthHelper
    {
        public GoogleWebAuthHelper(string appId, string scopes, Action<Uri, Uri> authenticationAction)
            :base(appId, scopes, authenticationAction)
        {
            this.Provider = "google";
        }

        public override void StartWebAuthentication()
        {
            var callbackUri = new Uri("http://localhost");
            var requestUri = new Uri(string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope={2}&response_type=code", _AppID, callbackUri.ToString(), _Scopes));

            _AuthenticationAction(requestUri, callbackUri);
        }

        public async override Task<Model.UserInfo> RetrieveForAccessToken(string accessToken)
        {
            //get the email, name, birthday, gender from the provider
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var response = await httpClient.GetAsync(new Uri("https://www.googleapis.com/plus/v1/people/me"));
                var json = await response.Content.ReadAsStringAsync();

                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<UserInfo>(json));
            }
        }
    }
}
