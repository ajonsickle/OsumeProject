using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OsumeProject
{
    public class user
    {

        public string accessToken;
        public string username;
        public string currentToken;
        public string pfpURL;
        public string userID;
        public string playlistID;
        public bool admin = false;
        public apiClient apiClient = new apiClient("af2ec11dcf36457e95de0ca70d46fd85", "bea950062ca34a87acaa11e8211ae513", new HttpClient());

        public user()
        {

        }
        public async Task getRefreshToken()
        {
            var getRefreshedToken = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            getRefreshedToken.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                  {"grant_type", "refresh_token"},
                  {"refresh_token", accessToken},
            });
            getRefreshedToken.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(apiClient.getClientID() + ":" + apiClient.getClientSecret())));
            var token = await apiClient.client.SendAsync(getRefreshedToken);
            token.EnsureSuccessStatusCode();
            using var stream = await token.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<RefreshTokenResponseTemp>(stream);
            currentToken = result.access_token;
        }
    }
}