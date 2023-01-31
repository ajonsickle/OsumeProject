using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OsumeProject
{
    /// <summary>
    /// Interaction logic for register.xaml
    /// </summary>
    public partial class register : Window
    {
        public Osume Osume;
        public register(ref Osume Osume)
        {
            this.Osume = Osume;
            InitializeComponent();
        }

        private void adminBoxChecked(object sender, RoutedEventArgs e)
        {
            adminKeyInput.Visibility = Visibility.Visible;
            adminKeyLabel.Visibility = Visibility.Visible;
        }
        private void adminBoxUnchecked(object sender, RoutedEventArgs e)
        {
            adminKeyInput.Visibility = Visibility.Hidden;
            adminKeyLabel.Visibility = Visibility.Hidden;
        }

        private void backButtonClick(object sender, RoutedEventArgs e)
        {
            mainscreenselect mss = new mainscreenselect(ref Osume);
            mss.Show();
            this.Close();
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
            OsumeTrack[] recentTopTracks = await factory.getSingleton().apiClient.getTopTracks("short_term", 50);
            OsumeArtist[] recentTopArtists = await factory.getSingleton().apiClient.getTopArtists("short_term", 50);
            SQLiteCommand searchFeatures = new SQLiteCommand("SELECT * FROM audioFeature WHERE username = @user", Osume.databaseManager.connection);
            searchFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
            foreach (OsumeArtist artist in recentTopArtists)
            {
                foreach (string genre in artist.genres)
                {
                    Osume.databaseManager.updateGenres(genre, true, false);
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
                DataTable data = Osume.databaseManager.returnSearchedTable(searchFeatures);
                if (data.Rows.Count >= 0)
                {
                    Dictionary<string, double> audioFeatures = await Osume.getApiClient().getAudioFeatures(track.id);
                    Osume.databaseManager.updateAudioFeatures(track, data, false, audioFeatures);
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

        private async void registerButtonClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(usernameInput.Text))
            {
                errorMessageBox.Text = "Error! Empty username field!";
                return;
            }
            else if (usernameInput.Text.Length < 5)
            {
                errorMessageBox.Text = "Error! Username must be 5 characters or more!";
                return;
            }
            else if (usernameInput.Text.Length > 15)
            {
                errorMessageBox.Text = "Error! Username cannot exceed 15 characters!";
                return;
            }
            else if (String.IsNullOrEmpty(passwordInput.Password) || String.IsNullOrEmpty(confirmPasswordInput.Password))
            {
                errorMessageBox.Text = "Error! A password box is empty!";
                return;
            }
            else if (passwordInput.Password.Length < 8)
            {
                errorMessageBox.Text = "Error! Password must be 8 characters or more!";
                return;
            }
            else if (passwordInput.Password.Length > 50)
            {
                errorMessageBox.Text = "Error! Password cannot exceed 50 characters!";
                return;
            }
            else if (passwordInput.Password != confirmPasswordInput.Password)
            {
                errorMessageBox.Text = "Error! Passwords do not match!";
                return;
            }
            SQLiteCommand countCommand = new SQLiteCommand("SELECT COUNT(hashedPassword) FROM userAccount WHERE username = @user", Osume.databaseManager.connection);
            countCommand.Parameters.AddWithValue("@user", usernameInput.Text);
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM userAccount WHERE username = @user", Osume.databaseManager.connection);
            command.Parameters.AddWithValue("@user", usernameInput.Text);
            try
            {
                int count = Convert.ToInt32(countCommand.ExecuteScalar());
                if (count > 0)
                {
                    errorMessageBox.Text = "Error! Username is already taken!";
                    return;
                }
                else
                {
                    string hashedPassword = sha1(passwordInput.Password);
                    if (hashedPassword == "Error!")
                    {
                        errorMessageBox.Text = "Error while storing password!";
                        return;
                    }
                    else
                    {
                        bool admin = false;
                        if (adminCheckbox.IsChecked == true)
                        {
                            if (adminKeyInput.Password == "538561") admin = true;
                            else
                            {
                                errorMessageBox.Text = "Incorrect admin passkey!";
                                return;
                            }
                        }
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
                        getAccessToken.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(factory.getSingleton().apiClient.clientID + ":" + factory.getSingleton().apiClient.clientSecret)));
                        var token = await factory.getSingleton().apiClient.client.SendAsync(getAccessToken);
                        if (!token.IsSuccessStatusCode)
                        {
                            factory.deleteSingleton();
                            return;
                        }
                        using (var stream = await token.Content.ReadAsStreamAsync())
                        {
                            var result = await JsonSerializer.DeserializeAsync<TokenResponseTemp>(stream);
                            factory.getSingleton().accessToken = result.refresh_token;
                            factory.getSingleton().currentToken = result.access_token;
                            factory.getSingleton().username = usernameInput.Text;
                            await factory.getSingleton().getRefreshToken();
                            string userID = await factory.getSingleton().apiClient.getCurrentUserID();
                            string pfpURL = await factory.getSingleton().apiClient.getCurrentUserPFP();
                            factory.getSingleton().pfpURL = pfpURL;
                            factory.getSingleton().userID = userID;
                            string playlistID = await factory.getSingleton().apiClient.createPlaylist(userID);
                            factory.getSingleton().playlistID = playlistID;
                            SQLiteCommand insertUserAccountRow = new SQLiteCommand("INSERT INTO userAccount (username, hashedPassword, accessToken, playlistID, spotifyID) VALUES (?, ?, ?, ?, ?)", Osume.databaseManager.connection);
                            insertUserAccountRow.Parameters.AddWithValue("username", usernameInput.Text);
                            insertUserAccountRow.Parameters.AddWithValue("hashedPassword", hashedPassword);
                            insertUserAccountRow.Parameters.AddWithValue("accessToken", result.refresh_token);
                            insertUserAccountRow.Parameters.AddWithValue("playlistID", playlistID);
                            insertUserAccountRow.Parameters.AddWithValue("spotifyID", userID);
                            insertUserAccountRow.ExecuteNonQuery();
                            SQLiteCommand insertFeaturesRow = new SQLiteCommand("INSERT INTO audioFeature (username, count, danceabilityTotal, energyTotal, speechinessTotal, acousticnessTotal, instrumentalnessTotal, livenessTotal, valenceTotal) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", Osume.databaseManager.connection);
                            insertFeaturesRow.Parameters.AddWithValue("username", usernameInput.Text);
                            insertFeaturesRow.Parameters.AddWithValue("count", 0);
                            insertFeaturesRow.Parameters.AddWithValue("danceabilityTotal", 0);
                            insertFeaturesRow.Parameters.AddWithValue("energyTotal", 0);
                            insertFeaturesRow.Parameters.AddWithValue("speechinessTotal", 0);
                            insertFeaturesRow.Parameters.AddWithValue("acousticnessTotal", 0);
                            insertFeaturesRow.Parameters.AddWithValue("instrumentalnessTotal", 0);
                            insertFeaturesRow.Parameters.AddWithValue("livenessTotal", 0);
                            insertFeaturesRow.Parameters.AddWithValue("valenceTotal", 0);
                            insertFeaturesRow.ExecuteNonQuery();
                            SQLiteCommand insertUserSettingsRow = new SQLiteCommand("INSERT INTO userSettings (explicitTracks, recommendationStrength, username) VALUES (?, ?, ?)", Osume.databaseManager.connection);
                            insertUserSettingsRow.Parameters.AddWithValue("explicitTracks", true);
                            insertUserSettingsRow.Parameters.AddWithValue("recommendationStrength", 1);
                            insertUserSettingsRow.Parameters.AddWithValue("username", usernameInput.Text);
                            insertUserSettingsRow.ExecuteNonQuery();
                            await analyseListeningHistory();
                        }

                    }
                }
            }
            catch (Exception err)
            {
                Trace.WriteLine(err);
            }
            Thread.Sleep(5000);
            homepage homescreen = new homepage(ref Osume);
            homescreen.Show();
            this.Close();
        }
    }
}
