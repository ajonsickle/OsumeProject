using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

namespace OsumeProject
{
    public class apiClient
    {
        public string clientID;
        public string clientSecret;
        public HttpClient client;
        public apiClient(string clientID, string clientSecret, HttpClient client)
        {
            this.clientID = clientID;
            this.clientSecret = clientSecret;
            this.client = client;
        }
        public async Task<OsumeArtist> getArtistByName(string artist)
        {
            string x = artist.Replace(" ", "%20");
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/search?q=artist%3A" + x + "&type=artist&limit=1");
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<SearchResponseTemp>(stream);
            if (result.artists.items.Length == 0) return null;
            return new OsumeArtist(result.artists.items[0].uri, result.artists.items[0].id, result.artists.items[0].name, result.artists.items[0].genres, result.artists.items[0].images[0].url);
        }

        public async Task<OsumeArtist> getArtist(string id)
        {
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/artists/" + id);
            if (stream == null) return null;
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<GetArtistResponseTemp>(stream);
            string image = null;
            if (result.images.Length > 0) image = result.images[0].url;
            return new OsumeArtist(result.external_urls.spotify, result.id, result.name, result.genres, image);
        }

        public async Task<OsumeTrack> getTrack(string id)
        {
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/tracks/" + id + "?market=GB");
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<GetTrackResponseTemp>(stream);
            OList<OsumeArtist> artists = new OList<OsumeArtist>();
            foreach (var artist in result.artists)
            {
                artists.add(await getArtist(artist.id));
            }
            OList<OsumeArtist> albumArtists = new OList<OsumeArtist>();
            foreach (var artist in result.album.artists)
            {
                artists.add(await getArtist(artist.id));
            }
            Dictionary<string, double> audioFeatures = await getAudioFeatures(result.id);
            return new OsumeTrack(artists.convertToArray(), result.external_urls.spotify, result.id, result.preview_url, result.name, result.Explicit, audioFeatures, new OsumeAlbum(albumArtists.convertToArray(), result.album.external_urls.spotify, result.album.id, result.album.name, result.album.release_date, new Dictionary<int, string>() { { result.album.images[0].width, result.album.images[0].url }, { result.album.images[1].width, result.album.images[1].url }, { result.album.images[2].width, result.album.images[2].url } }));
        }

        public async Task<string> createPlaylist(string userID)
        {
            CreatePlaylistBody messageBody = new CreatePlaylistBody("Osume");
            var stream = await genericHTTPRequest("post", "https://api.spotify.com/v1/users/" + userID + "/playlists", messageBody);
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<CreatePlaylistResponseTemp>(stream);
            return result.id;
        }

        public async void addToPlaylist(string playlistID, string trackID)
        {
            await genericHTTPRequest("post", "https://api.spotify.com/v1/playlists/" + playlistID + "/tracks?uris=spotify:track:" + trackID);
        }

        public async void removeFromPlaylist(string playlistID, string[] trackIDs)
        {
            OList<RemoveFromPlaylistBody.tracksBody> items = new OList<RemoveFromPlaylistBody.tracksBody>();
            foreach (var id in trackIDs)
            {
                items.add(new RemoveFromPlaylistBody.tracksBody("spotify:track:" + id));
            }
            RemoveFromPlaylistBody messageBody = new RemoveFromPlaylistBody(items.convertToArray());
            await genericHTTPRequest("delete", "https://api.spotify.com/v1/playlists/" + playlistID + "/tracks", messageBody);
        }

        public async Task<string> getCurrentUserID()
        {
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/me");
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<GetProfileResponseTemp>(stream);
            return result.id;
        }

        public async Task<string> getCurrentUserPFP()
        {
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/me");
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<GetProfileResponseTemp>(stream);
            if (result.images.Length > 0) return result.images[0].url;
            else return "https://i.scdn.co/image/ab6761610000e5eb18bd995e53ed8e1e78cdce67";
        }

        public async Task<OsumeTrack[]> getTopTracks(string range, int limit)
        {
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/me/top/tracks?time_range=" + range + "&limit=" + limit);
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<TopTracksResponseTemp>(stream);
            OList<OsumeTrack> tracksList = new OList<OsumeTrack>();
            foreach (var item in result.items)
            {
                OList<OsumeArtist> trackArtists = new OList<OsumeArtist>();
                foreach (var artist in item.artists)
                {
                    OsumeArtist x = await getArtist(artist.id);
                    if (x != null) trackArtists.add(x);
                }
                OList<OsumeArtist> albumArtists = new OList<OsumeArtist>();
                foreach (var artist in item.album.artists)
                {
                    OsumeArtist x = await getArtist(artist.id);
                    if (x != null) albumArtists.add(x);
                }
                Dictionary<int, string> albumCovers = new Dictionary<int, string>();
                foreach (var image in item.album.images)
                {
                    albumCovers.Add(image.width, image.url);
                }
                Dictionary<string, double> audioFeatures = await getAudioFeatures(item.id);
                if (albumCovers != null) tracksList.add(new OsumeTrack(trackArtists.convertToArray(), item.external_urls.spotify, item.id, item.preview_url, item.name, item.Explicit, audioFeatures, new OsumeAlbum(albumArtists.convertToArray(), item.album.external_urls.spotify, item.album.id, item.album.name, item.album.release_date, albumCovers)));
            }
            return tracksList.convertToArray();
        }

        public async Task<Dictionary<string, double>> getAudioFeatures(string trackID)
        {
            try
            {
                var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/audio-features/" + trackID);
                var result = await System.Text.Json.JsonSerializer.DeserializeAsync<GetAudioFeatureResponseTemp>(stream);
                Dictionary<string, double> features = new Dictionary<string, double>()
                    {
                        {"danceability", result.danceability},
                        {"energy", result.energy},
                        {"speechiness", result.speechiness},
                        {"acousticness", result.acousticness},
                        {"instrumentalness", result.instrumentalness},
                        {"liveness", result.liveness},
                        {"valence", result.valence},
                    };
                return features;
            }
            catch (Exception err)
            {
                return null;
            }
        }

        public async Task<OsumeArtist[]> getTopArtists(string range, int limit)
        {
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/me/top/artists?time_range=" + range + "&limit=" + limit);
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<TopArtistsResponseTemp>(stream);
            OList<OsumeArtist> artistList = new OList<OsumeArtist>();
            foreach (var item in result.items)
            {
                string image = null;
                if (item.images.Length > 0) image = item.images[0].url;
                artistList.add(new OsumeArtist(item.uri, item.id, item.name, item.genres, image));
            }
            return artistList.convertToArray();
        }
        public async Task getRefreshToken()
        {
            var getRefreshedToken = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            getRefreshedToken.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                  {"grant_type", "refresh_token"},
                  {"refresh_token", factory.getSingleton().accessToken},
            });
            getRefreshedToken.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(clientID + ":" + clientSecret)));
            var token = await client.SendAsync(getRefreshedToken);
            token.EnsureSuccessStatusCode();
            using var stream = await token.Content.ReadAsStreamAsync();
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<RefreshTokenResponseTemp>(stream);
            factory.getSingleton().currentToken = result.access_token;
        }

        public async Task<Stream> genericHTTPRequest(string method, string uri, object bodyParameters = null)
        {
            bool success = false;
            do
            {
                HttpRequestMessage request = null;
                if (method == "get") request = new HttpRequestMessage(HttpMethod.Get, uri);
                else if (method == "post") request = new HttpRequestMessage(HttpMethod.Post, uri);
                else if (method == "delete") request = new HttpRequestMessage(HttpMethod.Delete, uri);
                else return null;
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", factory.getSingleton().currentToken);
                if (bodyParameters != null)
                {
                    string json = JsonConvert.SerializeObject(bodyParameters);
                    StringContent obj = new StringContent(json, Encoding.UTF8, "application/json");
                    request.Content = obj;
                }
                var sendRequest = await client.SendAsync(request);
                if (sendRequest.IsSuccessStatusCode)
                {
                    success = true;
                    var stream = await sendRequest.Content.ReadAsStreamAsync();
                    return stream;
                }
                else
                {
                    string x = await sendRequest.Content.ReadAsStringAsync();
                    Trace.WriteLine(x);
                    await Task.Delay(5000);
                }
            } while (success == false);
            return null;
        }
    }
}
