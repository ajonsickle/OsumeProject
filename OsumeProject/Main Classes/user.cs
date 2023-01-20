﻿using System;
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
        public apiClient apiClient;

        public user()
        {
            apiClient = new apiClient("5ee7e89013d64c0aad8d6c2fd98213b3", "8c3dff68705f421894419de174db4b10", new HttpClient());
        }
        public async Task getRefreshToken()
        {
            var getRefreshedToken = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            getRefreshedToken.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                  {"grant_type", "refresh_token"},
                  {"refresh_token", accessToken},
            });
            getRefreshedToken.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(apiClient.clientID + ":" + apiClient.clientSecret)));
            var token = await apiClient.client.SendAsync(getRefreshedToken);
            token.EnsureSuccessStatusCode();
            using var stream = await token.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<RefreshTokenResponseTemp>(stream);
            currentToken = result.access_token;
        }
    }
}