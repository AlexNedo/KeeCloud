using System;
using System.Net;
using System.Net.Http;

 using Newtonsoft.Json.Linq;

using Dropbox.Api;

namespace KeeCloud.Providers.Dropbox
{
    class Api
    {
        /*
        The consumer key and the secret key included here are dummy keys.
        You should go to http://dropbox.com/developers to create your own application
        and get your own keys.

        This is done to prevent bots from scraping the keys from the source code posted on the web.
         
        Every now and then an accidental checkin of keys may occur, but these are all dummy applications
        created specifically for development that are deleted frequently and limited to the developer,
        never the real production keys.
        */

        /// <summary>
        /// This is the App key provided by Dropbox
        /// </summary>
        const string appKey = "dummy";
        /// <summary>
        /// This is the App secret provided by Dropbox
        /// </summary>
        const string appSecret = "dummy";

        private static HttpClient httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
        {
            // Specify request level timeout which decides maximum time that can be spent on
            // download/upload files.
            Timeout = TimeSpan.FromMinutes(1)
        };

        public static Uri GetAuthorizeUri(string state)
        {
            return DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, Api.appKey, (string)null, tokenAccessType: TokenAccessType.Offline);
        }

        public static string ProcessCodeFlow(string code)
        {
            var task = DropboxOAuth2Helper.ProcessCodeFlowAsync(code, appKey, appSecret, client: httpClient);
            var resp = task.Result;
            return resp.RefreshToken;
        }

        public static DropboxClient AuthenticatedClient(NetworkCredential credential)
        {
            var config = new DropboxClientConfig("KeeCloud")
            {
                HttpClient = httpClient
            };
            var accessToken = GetAccessToken(credential.Password);
            return new DropboxClient(accessToken, config);
        }

        private static string accessToken = null;
        private static string GetAccessToken(string refreshToken){
            if (accessToken != null){
                return accessToken;
            }
            accessToken = GetAccessTokenFromRefreshToken(refreshToken);
            return accessToken;
        }

        /// <summary>
        /// Even though the DropBoxClient can use refreshTokens directly, in all my tries it lead to a deadlock.
        /// So the workaround is to use the refreshToken outside of the DropBoxClient
        /// </summary>
        private static string GetAccessTokenFromRefreshToken(string refreshTok){
            var httpClient = new HttpClient();
            var url = "https://api.dropbox.com/oauth2/token";
            var parameters = new System.Collections.Generic.Dictionary<string, string>
            {
                { "refresh_token", refreshTok} ,
                { "grant_type", "refresh_token" },
                { "client_id", Api.appKey },
                { "client_secret", Api.appSecret },
            };
            var bodyContent = new FormUrlEncodedContent(parameters);
            DropboxProvider.Log("Before OAuth PostAsync");
            var task = httpClient.PostAsync(url, bodyContent);
            var resp = task.GetAwaiter().GetResult();
            DropboxProvider.Log("After OAuth PostAsync: " + resp.StatusCode);

            var task2 = resp.Content.ReadAsStringAsync();
            var resp2 = task2.GetAwaiter().GetResult();
            
            DropboxProvider.Log("Result: " + resp2);

            var json = JObject.Parse(resp2);
            return json["access_token"].ToString();
        }
    }
}
