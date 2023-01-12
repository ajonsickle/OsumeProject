﻿using Newtonsoft.Json;
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

namespace OsumeProject
{
    public class apiClient
    {

        private string clientID;
        private string clientSecret;
        public HttpClient client;
        public apiClient(string clientID, string clientSecret, HttpClient client)
        {
            this.clientID = clientID;
            this.clientSecret = clientSecret;
            this.client = client;
        }
        public string getClientID()
        {
            return clientID;
        }
        public string getClientSecret()
        {
            return clientSecret;
        }

        public Dictionary<string, double> generateRecommendations()
        {
            Dictionary<string, double> recommendations = new Dictionary<string, double>();
            double[] audioFeatureTasteVector = getAudioFeatureTasteVector();
            double[] genreTasteVector = getGenreTasteVector();
            double[] finalTasteVector = audioFeatureTasteVector.Concat(genreTasteVector).ToArray();
            string[] lines = File.ReadAllLines("tracks.csv");
            Random r = new Random();
            int i = 0;
            while (i < 100) { 
                int index = r.Next(1, 19893);
                string row = lines[index];
                TextFieldParser parser = new TextFieldParser(new StringReader(row));
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(",");
                string[] separated = new string[22];
                while (!parser.EndOfData)
                {
                    separated = parser.ReadFields();
                };
                parser.Close();
                if (!recommendations.ContainsKey(separated[0]))
                {
                    OList<double> audioFeatureVector = new OList<double>();
                    audioFeatureVector.add(Math.Round(Convert.ToDouble(separated[8]), 1));
                    audioFeatureVector.add(Math.Round(Convert.ToDouble(separated[9]), 1));
                    audioFeatureVector.add(Math.Round(Convert.ToDouble(separated[13]), 1));
                    audioFeatureVector.add(Math.Round(Convert.ToDouble(separated[14]), 1));
                    audioFeatureVector.add(Math.Round(Convert.ToDouble(separated[15]), 1));
                    audioFeatureVector.add(Math.Round(Convert.ToDouble(separated[16]), 1));
                    audioFeatureVector.add(Math.Round(Convert.ToDouble(separated[17]), 1));
                    double[] songAudioFeatureVector = audioFeatureVector.convertToArray();
                    double[] songGenreVector = separated[21].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray();
                    double[] finalSongVector = songAudioFeatureVector.Concat(songGenreVector).ToArray();
                    /*double audioFeatureAngle = calculateCosineSimilarity(audioFeatureTasteVector, songAudioFeatureVector);
                    double genreAngle = calculateCosineSimilarity(genreTasteVector, songGenreVector); */
                    recommendations.Add(separated[0], calculateCosineSimilarity(finalTasteVector, finalSongVector));
                    i++;
                }
            }
            SQLiteCommand checkRecSettings = new SQLiteCommand("SELECT recommendationStrength FROM userSettings WHERE username = @username", databaseManager.connection);
            checkRecSettings.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = databaseManager.returnSearchedTable(checkRecSettings);
            int strength = Convert.ToInt32(result.Rows[0][0]);
            Dictionary<string, double> sortedRecs = new Dictionary<string, double>();
            if (strength == 0)
            {
                sortedRecs = recommendations.OrderBy(key => key.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            } else
            {
                sortedRecs = recommendations.OrderByDescending(key => key.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            return sortedRecs;
        }

        public double[] getAudioFeatureTasteVector()
        {
            OList<double> vector = new OList<double>();
            SQLiteCommand getAudioFeaturesForUser = new SQLiteCommand("SELECT (danceabilityTotal / count), (energyTotal / count), (speechinessTotal / count), (acousticnessTotal / count), (instrumentalnessTotal / count), (livenessTotal / count), (valenceTotal / count) FROM audioFeature WHERE username = @username", databaseManager.connection);
            getAudioFeaturesForUser.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = databaseManager.returnSearchedTable(getAudioFeaturesForUser);
            for (int i = 0; i < 7; i++)
            {
                vector.add(Math.Round(Convert.ToDouble(result.Rows[0][i]), 1));
            }
            return vector.convertToArray();
        }

        public double[] getGenreTasteVector()
        {
            OList<double> vector = new OList<double>();
            StreamReader sr = new StreamReader("genresList.txt");
            SQLiteCommand getAllGenresForUser = new SQLiteCommand("SELECT genreName, (numOfLikedSongs / numOfDislikedSongs) FROM genre WHERE username = @username", databaseManager.connection);
            getAllGenresForUser.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = databaseManager.returnSearchedTable(getAllGenresForUser);
            while (sr.Peek() != -1)
            {
                string stapleGenre = Convert.ToString(sr.ReadLine());
                double affinity = 0.0;
                foreach (DataRow row in result.Rows)
                {
                    string name = Convert.ToString(row[0]);
                    if (name == stapleGenre)
                    {
                        affinity = Convert.ToDouble(row[1]);
                        if (affinity >= 1) affinity = 1.0;
                        affinity = Math.Round(affinity, 1);
                    }
                }
                vector.add(affinity);
            }
            return vector.convertToArray();
        }

        public double calculateCosineSimilarity(double[] A, double[] B)
        {
            var dotProduct = calculateDotProduct(A, B);
            var magnitudeA = calculateMagnitude(A);
            var magnitudeB = calculateMagnitude(B);

            return dotProduct / (magnitudeA * magnitudeB);
        }

        public double calculateDotProduct(double[] A, double[] B)
        {
            double result = 0;
            for (var i = 0; i < A.Length; i++)
            {
                result += (A[i] * B[i]);
            }
            return result;
        }

        public double calculateMagnitude(double[] x)
        {
            double total = 0;
            foreach (var val in x)
            {
                total += (val * val);
            }
            return Math.Sqrt(total);
        }

        public async Task<OsumeTrack> getCurrentlyPlayingTrack()
        {
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/me/player/currently-playing");
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<CurrentlyPlayingTrackJSONTemp>(stream);
            OList<OsumeArtist> artists = new OList<OsumeArtist>();
            foreach (var artist in result.item.artists)
            {
                artists.add(await getArtist(artist.id));
            }
            OList<OsumeArtist> albumartists = new OList<OsumeArtist>();
            foreach (var artist in result.item.album.artists)
            {
                albumartists.add(await getArtist(artist.id));
            }
            Dictionary<string, double> audioFeatures = await getAudioFeatures(result.item.id);
            
            OsumeTrack song = new OsumeTrack(artists.convertToArray(), result.item.external_urls.spotify, result.item.id, result.item.preview_url, result.item.name, result.item.Explicit, audioFeatures, new OsumeAlbum(albumartists.convertToArray(), result.item.album.external_urls.spotify, result.item.album.id, result.item.album.name, result.item.album.release_date, new OsumeAlbum.OsumeAlbumArt(new Dictionary<int, string>() { { result.item.album.images[0].width, result.item.album.images[0].url }, { result.item.album.images[1].width, result.item.album.images[1].url }, { result.item.album.images[2].width, result.item.album.images[2].url } })));
            return song;
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
            return new OsumeTrack(artists.convertToArray(), result.external_urls.spotify, result.id, result.preview_url, result.name, result.Explicit, audioFeatures, new OsumeAlbum(albumArtists.convertToArray(), result.album.external_urls.spotify, result.album.id, result.album.name, result.album.release_date, new OsumeAlbum.OsumeAlbumArt(new Dictionary<int, string>() { { result.album.images[0].width, result.album.images[0].url }, { result.album.images[1].width, result.album.images[1].url }, { result.album.images[2].width, result.album.images[2].url } })));
        }

        public async Task<string> createPlaylist(string userID)
        {
            CreatePlaylistBody messageBody = new CreatePlaylistBody("Osume");
            var stream = await genericHTTPRequest("post", "https://api.spotify.com/v1/users/" + userID + "/playlists", messageBody);
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<CreatePlaylistResponseTemp>(stream);
            return result.id;
        }

        public async Task<GetPlaylistResponseTemp> getPlaylist(string playlistID)
        {
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/playlists/" + playlistID);
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<GetPlaylistResponseTemp>(stream);
            return result;
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

        public async Task<OsumeTrack> getRandomTopTrack(string range, int limit)
        {
            bool valid = true;
            var stream = await genericHTTPRequest("get", "https://api.spotify.com/v1/me/top/tracks?time_range=" + range + "&limit=" + limit);
            if (stream == null) return await getTrack("3D0UBEEE8f3PNruc1dJ6Rs");
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<TopTracksResponseTemp>(stream);
            do
            {
                Random rnd = new Random();
                int num = rnd.Next(result.items.Length);
                var item = result.items[num];
                OList<OsumeArtist> trackArtists = new OList<OsumeArtist>();
                foreach (var artist in item.artists)
                {
                    OsumeArtist x = await getArtist(artist.id);
                    trackArtists.add(x);
                }
                OList<OsumeArtist> albumArtists = new OList<OsumeArtist>();
                foreach (var artist in item.album.artists)
                {
                    OsumeArtist x = await getArtist(artist.id);
                    albumArtists.add(x);
                }
                Dictionary<int, string> albumCovers = new Dictionary<int, string>();
                foreach (var image in item.album.images)
                {
                    albumCovers.Add(image.width, image.url);
                }
                Dictionary<string, double> audioFeatures = await getAudioFeatures(item.id);
                if (albumCovers != null) return new OsumeTrack(trackArtists.convertToArray(), item.external_urls.spotify, item.id, item.preview_url, item.name, item.Explicit, audioFeatures, new OsumeAlbum(albumArtists.convertToArray(), item.album.external_urls.spotify, item.album.id, item.album.name, item.album.release_date, new OsumeAlbum.OsumeAlbumArt(albumCovers)));
                else valid = false;
            } while (valid == false);
            return await getTrack("3D0UBEEE8f3PNruc1dJ6Rs");
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
                if (albumCovers != null) tracksList.add(new OsumeTrack(trackArtists.convertToArray(), item.external_urls.spotify, item.id, item.preview_url, item.name, item.Explicit, audioFeatures, new OsumeAlbum(albumArtists.convertToArray(), item.album.external_urls.spotify, item.album.id, item.album.name, item.album.release_date, new OsumeAlbum.OsumeAlbumArt(albumCovers))));
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

        public float calculateNewAverage(int size, float oldAverage, float newValue)
        {
            return oldAverage + ((newValue - oldAverage) / size);
        }

        public async Task updateGenres(string genre, bool like, bool undo)
        {
            try
            {
                SQLiteCommand updateGenre = new SQLiteCommand("INSERT INTO genre (genreName, username, numOfLikedSongs, numOfDislikedSongs) VALUES (?, ?, ?, ?)", databaseManager.connection);
                updateGenre.Parameters.AddWithValue("genreName", genre);
                updateGenre.Parameters.AddWithValue("username", factory.getSingleton().username);
                updateGenre.Parameters.AddWithValue("numOfLikedSongs", 1);
                updateGenre.Parameters.AddWithValue("numOfDislikedSongs", 1);
                updateGenre.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                if (undo)
                {
                    SQLiteCommand decrementGenreValues;
                    if (like)
                    {
                        decrementGenreValues = new SQLiteCommand("UPDATE genre SET numOfLikedSongs = numOfLikedSongs - 1 WHERE genreName = @genreName AND username = @user", databaseManager.connection);
                    }
                    else
                    {
                        decrementGenreValues = new SQLiteCommand("UPDATE genre SET numOfDislikedSongs = numOfDislikedSongs - 1 WHERE genreName = @genreName AND username = @user", databaseManager.connection);
                    }
                    decrementGenreValues.Parameters.AddWithValue("@genreName", genre);
                    decrementGenreValues.Parameters.AddWithValue("@user", factory.getSingleton().username);
                    decrementGenreValues.ExecuteNonQuery();
                }
                else
                {
                    SQLiteCommand incrementGenreValues;
                    if (like)
                    {
                        incrementGenreValues = new SQLiteCommand("UPDATE genre SET numOfLikedSongs = numOfLikedSongs + 1 WHERE genreName = @genreName AND username = @user", databaseManager.connection);
                    }
                    else
                    {
                        incrementGenreValues = new SQLiteCommand("UPDATE genre SET numOfDislikedSongs = numOfDislikedSongs + 1 WHERE genreName = @genreName AND username = @user", databaseManager.connection);
                    }
                    incrementGenreValues.Parameters.AddWithValue("@genreName", genre);
                    incrementGenreValues.Parameters.AddWithValue("@user", factory.getSingleton().username);
                    incrementGenreValues.ExecuteNonQuery();
                }
            }
        }

        public async Task updateAudioFeatures(OsumeTrack track, DataTable data, bool undo)
        {
            Dictionary<string, double> audioFeatures = await getAudioFeatures(track.id);
            if (audioFeatures == null) return;
            double totalDanceability = 0;
            double totalEnergy = 0;
            double totalSpeechiness = 0;
            double totalAcousticness = 0;
            double totalInstrumentalness = 0;
            double totalLiveness = 0;
            double totalValence = 0;
            int count = Convert.ToInt32(data.Rows[0][1]);
            SQLiteCommand updateFeatures = new SQLiteCommand("UPDATE audioFeature SET count = @count, danceabilityTotal = @danceabilityTotal, " +
    "energyTotal = @energyTotal, speechinessTotal = @speechinessTotal, acousticnessTotal = @acousticnessTotal, " +
    "instrumentalnessTotal = @instrumentalnessTotal, livenessTotal = @livenessTotal, valenceTotal = @valenceTotal " +
    "WHERE username = @user", databaseManager.connection);

            if (undo)
            {
                totalDanceability = Convert.ToDouble(data.Rows[0][2]) - audioFeatures["danceability"];
                totalEnergy = Convert.ToDouble(data.Rows[0][3]) - audioFeatures["energy"];
                totalSpeechiness = Convert.ToDouble(data.Rows[0][4]) - audioFeatures["speechiness"];
                totalAcousticness = Convert.ToDouble(data.Rows[0][5]) - audioFeatures["acousticness"];
                totalInstrumentalness = Convert.ToDouble(data.Rows[0][6]) - audioFeatures["instrumentalness"];
                totalLiveness = Convert.ToDouble(data.Rows[0][7]) - audioFeatures["liveness"];
                totalValence = Convert.ToDouble(data.Rows[0][8]) - audioFeatures["valence"];
                count--;
            } else
            {
                totalDanceability = Convert.ToDouble(data.Rows[0][2]) + audioFeatures["danceability"];
                totalEnergy = Convert.ToDouble(data.Rows[0][3]) + audioFeatures["energy"];
                totalSpeechiness = Convert.ToDouble(data.Rows[0][4]) + audioFeatures["speechiness"];
                totalAcousticness = Convert.ToDouble(data.Rows[0][5]) + audioFeatures["acousticness"];
                totalInstrumentalness = Convert.ToDouble(data.Rows[0][6]) + audioFeatures["instrumentalness"];
                totalLiveness = Convert.ToDouble(data.Rows[0][7]) + audioFeatures["liveness"];
                totalValence = Convert.ToDouble(data.Rows[0][8]) + audioFeatures["valence"];
                count++;
            }
            updateFeatures.Parameters.AddWithValue("@count", count);
            updateFeatures.Parameters.AddWithValue("@danceabilityTotal", totalDanceability);
            updateFeatures.Parameters.AddWithValue("@energyTotal", totalEnergy);
            updateFeatures.Parameters.AddWithValue("@speechinessTotal", totalSpeechiness);
            updateFeatures.Parameters.AddWithValue("@acousticnessTotal", totalAcousticness);
            updateFeatures.Parameters.AddWithValue("@instrumentalnessTotal", totalInstrumentalness);
            updateFeatures.Parameters.AddWithValue("@livenessTotal", totalLiveness);
            updateFeatures.Parameters.AddWithValue("@valenceTotal", totalValence);
            updateFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
            updateFeatures.ExecuteNonQuery();

        }

        public async Task analyseListeningHistory()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(register))
                {
                    (window as register).errorMessageBox.Visibility = Visibility.Hidden;
                    (window as register).analyseProgressBar.Visibility = Visibility.Visible;
                    (window as register).analyseText.Visibility = Visibility.Visible;
                }
            }
            OsumeTrack[] recentTopTracks = await getTopTracks("short_term", 50);
            OsumeArtist[] recentTopArtists = await getTopArtists("short_term", 50);
            SQLiteCommand searchFeatures = new SQLiteCommand("SELECT * FROM audioFeature WHERE username = @user", databaseManager.connection);
            searchFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
            foreach (OsumeArtist artist in recentTopArtists)
            {
                foreach (string genre in artist.genres)
                {
                    await updateGenres(genre, true, false);
                }
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(register))
                    {
                        (window as register).analyseProgressBar.Value = (window as register).analyseProgressBar.Value + 1;
                    }
                }
            }
            foreach (OsumeTrack track in recentTopTracks)
            {
                DataTable data = databaseManager.returnSearchedTable(searchFeatures);
                if (data.Rows.Count >= 0)
                {
                    await updateAudioFeatures(track, data, false);
                }
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(register))
                    {
                        (window as register).analyseProgressBar.Value = (window as register).analyseProgressBar.Value + 1;
                    }
                }
            }

        }
        public int[] getAvgColor(Bitmap bmp)
        {
            int totalRed = 0;
            int totalBlue = 0;
            int totalGreen = 0;
            for (int i = 0; i < bmp.Width - 1; i++)
            {
                for (int j = 0; j < bmp.Height - 1; j++)
                {
                    System.Drawing.Color colour = bmp.GetPixel(i, j);
                    totalRed += colour.R;
                    totalBlue += colour.B;
                    totalGreen += colour.G;
                }
            }
            int avgRed = (int)Math.Round((double)totalRed / (bmp.Height * bmp.Width));
            int avgBlue = (int)Math.Round((double)totalBlue / (bmp.Height * bmp.Width));
            int avgGreen = (int)Math.Round((double)totalGreen / (bmp.Height * bmp.Width));
            int[] arr = { avgRed, avgBlue, avgGreen };
            return arr;
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
