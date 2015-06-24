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
        private string _ClientSecret;

        public GoogleWebAuthHelper(string clientId, string clientSecret, string scopes, Action<Uri, Uri> authenticationAction)
            : base(clientId, scopes, authenticationAction)
        {
            _ClientSecret = clientSecret;

            this.Provider = "google";
            this.CallbackUri = new Uri("http://localhost");
        }

        public override void StartWebAuthentication()
        {
            var requestUri = new Uri(string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope={2}&response_type=code",
                _AppID,
                this.CallbackUri.ToString(),
                Uri.EscapeDataString(_Scopes)));

            _AuthenticationAction(requestUri, this.CallbackUri);
        }

        public async override Task<Model.UserInfo> RetrieveForAccessToken(string accessToken)
        {
            //get the email, name, birthday, gender from the provider
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var response = await httpClient.GetAsync(new Uri("https://www.googleapis.com/plus/v1/people/me"));
                var json = await response.Content.ReadAsStringAsync();

                var anonObj = new { birthday = "", displayName = "", id = "", gender = "", emails = new[] { new { value = "", type = "" } } };

                anonObj = await Task.Factory.StartNew(() => JsonConvert.DeserializeAnonymousType(json, anonObj));

                return new UserInfo
                {
                    Id = anonObj.id,
                    Birthday = anonObj.birthday,
                    Email = anonObj.emails.First().value,
                    Gender = anonObj.gender,
                    Name = anonObj.displayName
                };
            }
        }

        public async new Task<string> ContinueWebAuthentication(WebAuthenticationResult result)
        {
            var needle = "code=";
            var tokenStartIndex = result.ResponseData.IndexOf(needle) + needle.Length;
            var tokenEndIndex = result.ResponseData.IndexOf("&", tokenStartIndex);
            var code = result.ResponseData.Substring(tokenStartIndex, tokenEndIndex - tokenStartIndex);

            if (string.IsNullOrEmpty(code))
            {
                throw new Exception("Code not found");
            }

            using (var httpClient = new HttpClient())
            {
                var httpContent = new HttpFormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                   new KeyValuePair<string, string>("client_id", _AppID),
                   new KeyValuePair<string, string>("client_secret", _ClientSecret),
                   new KeyValuePair<string, string>("code", code),
                   new KeyValuePair<string, string>("redirect_uri", this.CallbackUri.ToString()),
                   new KeyValuePair<string, string>("grant_type", "authorization_code"),
                });

                var accessTokenResult = await httpClient.PostAsync(new Uri("https://accounts.google.com/o/oauth2/token"), httpContent);

                var resultString = await accessTokenResult.Content.ReadAsStringAsync();
                var anonResult = new { access_token = "" };

                anonResult = await Task.Factory.StartNew(() => JsonConvert.DeserializeAnonymousType(resultString, anonResult));

                return anonResult.access_token;
            }

            throw new Exception("Authentication failed");
        }
    }
}
