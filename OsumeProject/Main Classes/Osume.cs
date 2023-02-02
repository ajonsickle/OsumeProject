using Microsoft.VisualBasic.FileIO;
using NAudio.Wave;
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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OsumeProject
{
    public class Osume
    {
        private apiClient apiClient;
        private databaseManager databaseManager;
        public OStack<OsumeTrack> songsPlayed;
        public OStack<OsumeTrack> songsToPlay;
        public OStack<bool> previousSongsLiked;
        public Osume(apiClient apiClient, databaseManager databaseManager)
        {
            this.apiClient = apiClient;
            this.databaseManager = databaseManager;
            this.songsPlayed = new OStack<OsumeTrack>();
            this.songsToPlay = new OStack<OsumeTrack>();
            this.previousSongsLiked = new OStack<bool>();
        }
        public apiClient getApiClient()
        {
            return this.apiClient;
        }
        public databaseManager getDatabaseManager()
        {
            return this.databaseManager;
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
        public Dictionary<string, double> generateRecommendations()
        {
            Dictionary<string, double> recommendations = new Dictionary<string, double>();
            double[] audioFeatureTasteVector = getAudioFeatureTasteVector();
            double[] genreTasteVector = getGenreTasteVector();
            double[] finalTasteVector = audioFeatureTasteVector.Concat(genreTasteVector).ToArray();
            string[] lines = File.ReadAllLines("tracks.csv");
            Random r = new Random();
            int i = 0;
            while (i < 100)
            {
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
            }
            else
            {
                sortedRecs = recommendations.OrderByDescending(key => key.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            return sortedRecs;
        }
        public async Task updateAudioFeatures(OsumeTrack track, bool undo)
        {
            SQLiteCommand getCurrentFeatures = new SQLiteCommand("SELECT * FROM audioFeature WHERE username = @user", databaseManager.connection);
            getCurrentFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
            DataTable data = databaseManager.returnSearchedTable(getCurrentFeatures);
            Dictionary<string, double> audioFeatures = await getApiClient().getAudioFeatures(track.id);
            databaseManager.updateAudioFeatures(track, data, undo, audioFeatures);
        }
        public void updateGenres(OsumeTrack track, bool like, bool undo)
        {
            OList<string> addedGenres = new OList<string>();
            foreach (OsumeArtist artist in track.artists)
            {
                foreach (string genre in artist.genres)
                {
                    if (!addedGenres.contains(genre))
                    {
                        addedGenres.add(genre);
                        databaseManager.updateGenres(genre, like, undo);
                    }
                }
            }
        }
        public async Task undoChanges(OsumeTrack track, bool liked)
        {
            OList<string> addedGenres = new OList<string>();
            foreach (OsumeArtist artist in track.artists)
            {
                foreach (string genre in artist.genres)
                {
                    if (!addedGenres.contains(genre))
                    {
                        addedGenres.add(genre);
                    }
                }
            }
            DataTable featureData = null;
            if (liked)
            {
                SQLiteCommand getCurrentFeatures = new SQLiteCommand("SELECT * FROM audioFeature WHERE username = @user", databaseManager.connection);
                getCurrentFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
                featureData = databaseManager.returnSearchedTable(getCurrentFeatures);
            }
            if (featureData == null)
            {
                // clicked dislike
                updateGenres(track, false, true);
            }
            else
            {
                // clicked like
                updateGenres(track, true, true);
                await updateAudioFeatures(track, true);
                SQLiteCommand deleteFromLibrary = new SQLiteCommand("DELETE FROM savedSong WHERE songID = @songID AND username = @user", databaseManager.connection);
                deleteFromLibrary.Parameters.AddWithValue("@songID", track.id);
                deleteFromLibrary.Parameters.AddWithValue("@user", factory.getSingleton().username);
                deleteFromLibrary.ExecuteNonQuery();
                getApiClient().removeFromPlaylist(factory.getSingleton().playlistID, new string[] { track.id });
            }

        }
        public void PlayMp3FromUrl(string url)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, "song.mp3");
                    var reader = new Mp3FileReader("song.mp3");
                    var waveOut = new WaveOut();
                    waveOut.Init(reader);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        try
                        {
                            Thread.Sleep(100);
                        }
                        catch (ThreadInterruptedException e)
                        {
                            Trace.WriteLine(e);
                            reader.Close();
                            return;
                        }
                    }
                    reader.Close();
                }
                catch (Exception err)
                {
                    Trace.WriteLine(err);
                    return;
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
        public string sha1(string input)
        {
            string output = "";
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            SHA1 hasher = SHA1.Create();
            byte[] computedHash = hasher.ComputeHash(inputBytes);
            foreach (var hashedByte in computedHash)
            {
                output += hashedByte.ToString("X2");
            }
            if (string.IsNullOrEmpty(output)) return "Error!";
            else return output;
        }
        public async Task register(bool admin, string usernameInput, string hashedPassword)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/callback/");
            listener.Start();
            string accessToken = "";
            var uri = "https://accounts.spotify.com/authorize?client_id=5ee7e89013d64c0aad8d6c2fd98213b3&response_type=code&scope=user-read-playback-state%20user-read-currently-playing%20user-read-recently-played%20user-top-read%20playlist-read-private%20playlist-modify-private%20playlist-modify-public%20user-library-read%20user-library-modify&show_dialog=true&redirect_uri=http://localhost:8888/callback";
            Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            var callbackURL = Convert.ToString(request.Url);
            var indexOfEquals = callbackURL.LastIndexOf('=');
            accessToken = callbackURL.Substring(indexOfEquals + 1);
            string responseString = "<HTML><BODY>You may now return to the application.</BODY></HTML>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            listener.Stop();
            var getAccessToken = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            getAccessToken.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                        {
                            {"grant_type", "authorization_code"},
                            {"code", accessToken},
                            {"redirect_uri", "http://localhost:8888/callback"}
                        });
            if (admin == true) factory.createSingleton(true);
            else factory.createSingleton(false);
            getAccessToken.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(getApiClient().clientID + ":" + getApiClient().clientSecret)));
            var token = await getApiClient().client.SendAsync(getAccessToken);
            if (!token.IsSuccessStatusCode)
            {
                factory.deleteSingleton();
                return;
            }
            var stream = await token.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<TokenResponseTemp>(stream);
            factory.getSingleton().accessToken = result.refresh_token;
            factory.getSingleton().currentToken = result.access_token;
            factory.getSingleton().username = usernameInput;
            await getApiClient().getRefreshToken();
            string userID = await getApiClient().getCurrentUserID();
            string pfpURL = await getApiClient().getCurrentUserPFP();
            factory.getSingleton().pfpURL = pfpURL;
            factory.getSingleton().userID = userID;
            string playlistID = await getApiClient().createPlaylist(userID);
            factory.getSingleton().playlistID = playlistID;
            SQLiteCommand insertUserAccountRow = new SQLiteCommand("INSERT INTO userAccount (username, hashedPassword, accessToken, playlistID, spotifyID) VALUES (?, ?, ?, ?, ?)", databaseManager.connection);
            insertUserAccountRow.Parameters.AddWithValue("username", usernameInput);
            insertUserAccountRow.Parameters.AddWithValue("hashedPassword", hashedPassword);
            insertUserAccountRow.Parameters.AddWithValue("accessToken", result.refresh_token);
            insertUserAccountRow.Parameters.AddWithValue("playlistID", playlistID);
            insertUserAccountRow.Parameters.AddWithValue("spotifyID", userID);
            insertUserAccountRow.ExecuteNonQuery();
            SQLiteCommand insertFeaturesRow = new SQLiteCommand("INSERT INTO audioFeature (username, count, danceabilityTotal, energyTotal, speechinessTotal, acousticnessTotal, instrumentalnessTotal, livenessTotal, valenceTotal) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", databaseManager.connection);
            insertFeaturesRow.Parameters.AddWithValue("username", usernameInput);
            insertFeaturesRow.Parameters.AddWithValue("count", 0);
            insertFeaturesRow.Parameters.AddWithValue("danceabilityTotal", 0);
            insertFeaturesRow.Parameters.AddWithValue("energyTotal", 0);
            insertFeaturesRow.Parameters.AddWithValue("speechinessTotal", 0);
            insertFeaturesRow.Parameters.AddWithValue("acousticnessTotal", 0);
            insertFeaturesRow.Parameters.AddWithValue("instrumentalnessTotal", 0);
            insertFeaturesRow.Parameters.AddWithValue("livenessTotal", 0);
            insertFeaturesRow.Parameters.AddWithValue("valenceTotal", 0);
            insertFeaturesRow.ExecuteNonQuery();
            SQLiteCommand insertUserSettingsRow = new SQLiteCommand("INSERT INTO userSettings (explicitTracks, recommendationStrength, username) VALUES (?, ?, ?)", databaseManager.connection);
            insertUserSettingsRow.Parameters.AddWithValue("explicitTracks", true);
            insertUserSettingsRow.Parameters.AddWithValue("recommendationStrength", 1);
            insertUserSettingsRow.Parameters.AddWithValue("username", usernameInput);
            insertUserSettingsRow.ExecuteNonQuery();
        }
        public async Task makeSongChoice(OsumeTrack currentSong, bool like)
        {
            if (like)
            {
                SQLiteCommand command = new SQLiteCommand("INSERT INTO savedSong (songID, timeSaved, username) VALUES (?, ?, ?)", getDatabaseManager().connection);
                command.Parameters.AddWithValue("songID", currentSong.id);
                command.Parameters.AddWithValue("timeSaved", DateTime.Now);
                command.Parameters.AddWithValue("username", factory.getSingleton().username);
                command.ExecuteNonQuery();
                getApiClient().addToPlaylist(factory.getSingleton().playlistID, currentSong.id);
                await updateAudioFeatures(currentSong, false);
            }
            updateGenres(currentSong, like, false);
            songsPlayed.push(currentSong);
            previousSongsLiked.push(true);
        }
        public int getRecommendationStrength()
        {
            SQLiteCommand checkRecSettings = new SQLiteCommand("SELECT recommendationStrength FROM userSettings WHERE username = @username", getDatabaseManager().connection);
            checkRecSettings.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = getDatabaseManager().returnSearchedTable(checkRecSettings);
            int strength = Convert.ToInt32(result.Rows[0][0]);
            return strength;
        }
        public async Task<OsumeTrack> getRecommendation(OsumeTrack songToPlay, Dictionary<string, double> recommendations, int random)
        {
            try
            {
                OsumeTrack song = null;
                if (songToPlay == null)
                {
                    if (songsToPlay.getLength() == 0)
                    {
                        song = await getApiClient().getTrack(recommendations.ElementAt(random).Key);
                    }
                    else
                    {
                        song = songsToPlay.pop();
                    }
                }
                else
                {
                    song = songToPlay;
                }

                bool allowed = false;

                if (song.isExplicit)
                {
                    SQLiteCommand checkExplicitSetting = new SQLiteCommand("SELECT explicitTracks FROM userSettings WHERE username = @username", getDatabaseManager().connection);
                    checkExplicitSetting.Parameters.AddWithValue("@username", factory.getSingleton().username);
                    DataTable table0 = getDatabaseManager().returnSearchedTable(checkExplicitSetting);
                    if (Convert.ToInt32(table0.Rows[0][0]) == 0)
                    {
                        allowed = false;
                    }
                    else allowed = true;
                }
                else
                {
                    allowed = true;
                }
                SQLiteCommand searchBlockList = new SQLiteCommand("SELECT * FROM blockList WHERE username = @username AND artistID = @artistID", getDatabaseManager().connection);
                searchBlockList.Parameters.AddWithValue("@username", factory.getSingleton().username);
                if (song.artists.Length > 0)
                {
                    searchBlockList.Parameters.AddWithValue("@artistID", song.artists[0].id);
                }
                else
                {
                    searchBlockList.Parameters.AddWithValue("@artistID", "null");
                }
                DataTable table1 = getDatabaseManager().returnSearchedTable(searchBlockList);
                SQLiteCommand searchSavedSong = new SQLiteCommand("SELECT * FROM savedSong WHERE username = @username AND songID = @songID", getDatabaseManager().connection);
                searchSavedSong.Parameters.AddWithValue("@username", factory.getSingleton().username);
                searchSavedSong.Parameters.AddWithValue("@songID", song.id);
                DataTable table2 = getDatabaseManager().returnSearchedTable(searchSavedSong);
                if (table1.Rows.Count == 0 && table2.Rows.Count == 0 && allowed == true)
                {
                    return song;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception err)
            {
                Trace.WriteLine(err);
                return null;
            }
        }
        public DataTable getSavedSongs()
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM savedSong WHERE username = @username ORDER BY timeSaved DESC", getDatabaseManager().connection);
            command.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable data = getDatabaseManager().returnSearchedTable(command);
            return data;
        }
        public void removeFromLibrary(string name)
        {
            DataTable data = getSavedSongs();
            DataRow row = data.Rows[Convert.ToInt32(name)];
            getApiClient().removeFromPlaylist(factory.getSingleton().playlistID, new string[] { row[0].ToString() });
            SQLiteCommand removeSong = new SQLiteCommand("DELETE FROM savedSong WHERE songID = @id", getDatabaseManager().connection);
            removeSong.Parameters.AddWithValue("@id", row[0]);
            removeSong.ExecuteNonQuery();
        }
        public DataTable getBlockedArtists()
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM blockList WHERE username = @username ORDER BY timeSaved DESC", getDatabaseManager().connection);
            command.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable data = getDatabaseManager().returnSearchedTable(command);
            return data;
        }
        public void addToBlockedArtists(OsumeArtist artist)
        {
            SQLiteCommand comm = new SQLiteCommand("INSERT INTO blockList (artistID, timeSaved, username) VALUES (?, ?, ?)", getDatabaseManager().connection);
            comm.Parameters.AddWithValue("@artistID", artist.id);
            comm.Parameters.AddWithValue("@timeSaved", DateTime.Now);
            comm.Parameters.AddWithValue("@username", factory.getSingleton().username);
            comm.ExecuteNonQuery();
        }
        public void removeFromBlockedArtists(string name)
        {
            DataTable data = getBlockedArtists();
            DataRow row = data.Rows[Convert.ToInt32(name)];
            SQLiteCommand removeArtist = new SQLiteCommand("DELETE FROM blockList WHERE artistID = @id", getDatabaseManager().connection);
            removeArtist.Parameters.AddWithValue("@id", row[0]);
            removeArtist.ExecuteNonQuery();
        }
        public int getExplicitTracks()
        {
            SQLiteCommand checkExplicit = new SQLiteCommand("SELECT explicitTracks FROM userSettings WHERE username = @username", getDatabaseManager().connection);
            checkExplicit.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = getDatabaseManager().returnSearchedTable(checkExplicit);
            return Convert.ToInt32(result.Rows[0][0]);
        }
        public DataTable getAllUserAccounts()
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM userAccount WHERE NOT (username = @username) ORDER BY username DESC", getDatabaseManager().connection);
            command.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable data = getDatabaseManager().returnSearchedTable(command);
            return data;
        }
        public void removeUserAccount(string name)
        {
            DataTable data = getAllUserAccounts();
            DataRow row = data.Rows[Convert.ToInt32(name)];
            SQLiteCommand removeSong = new SQLiteCommand("DELETE FROM userAccount WHERE username = @username", getDatabaseManager().connection);
            removeSong.Parameters.AddWithValue("@username", row[0]);
            removeSong.ExecuteNonQuery();
        }
    }
}
