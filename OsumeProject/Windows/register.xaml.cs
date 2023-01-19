﻿using System;
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

namespace OsumeProject
{
    /// <summary>
    /// Interaction logic for register.xaml
    /// </summary>
    public partial class register : Window
    {
        public register()
        {
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
            mainscreenselect mss = new mainscreenselect();
            mss.Show();
            this.Close();
        }
        public string hash(string input)
        {
            long h0 = 0x67452301;
            long h1 = 0xEFCDAB89;
            long h2 = 0x98BADCFE;
            long h3 = 0x10325476;
            long h4 = 0xC3D2E1F0;
            string binaryString = string.Join(" ", Encoding.GetEncoding("ASCII").GetBytes(input).Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
            binaryString += '1';
            while (binaryString.Length % 512 != 448)
            {
                binaryString += '0';
            }
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            var endianInteger = BitConverter.ToUInt64(bytes);
            string endianString = Convert.ToString(endianInteger);
            binaryString += endianString;
            IEnumerable<string> chunks = Enumerable.Range(0, binaryString.Length / 512).Select(i => binaryString.Substring(i * 512, 512));
            foreach(var chunk in chunks)
            {
                IEnumerable<string> words = Enumerable.Range(0, chunk.Length / 32).Select(j => chunk.Substring(j * 32, 32));
                string[] wordArray = words.ToArray();
                byte[] byteArray = wordArray.Select(byte.Parse).ToArray();
                for (int i = 16; i < 79; i++)
                {
                    byteArray[i] = (byte)BitOperations.RotateLeft((uint)(byteArray[i - 3] ^ byteArray[i - 8] ^ byteArray[i - 14] ^ byteArray[i - 16]), 1);
                }
                long a = h0;
                long b = h1;
                long c = h2;
                long d = h3;
                long e = h4;
                long f = 0;
                long k = 0;
                for (int j = 0; j < 79; j++)
                {
                    if (j >= 0 && j <= 19)
                    {
                        f = (b & c) | ((~b) & d);
                        k = 0x5A827999;
                    } else if (j >= 20 && j <= 39)
                    {
                        f = b ^ c ^ d;
                        k = 0x6ED9EBA1;
                    } else if (j >= 40 && j <= 59)
                    {
                        f = (b & c) | (b & d) | (c & d);
                        k = 0x8F1BBCDC;
                    } else if (j >= 60 && j <= 70)
                    {
                        f = b ^ c ^ d;
                        k = 0xCA62C1D6;
                    }
                    long temp = (BitOperations.RotateLeft((uint)a, 5)) + f + e + k + byteArray[j];
                    e = d;
                    d = c;
                    c = BitOperations.RotateLeft((uint)b, 30);
                    b = a;
                    a = temp;
                }
                h0 = h0 + a;
                h1 = h1 + b;
                h2 = h2 + c;
                h3 = h3 + d;
                h4 = h4 + e;
            }
            string finalHash = Convert.ToString(h0) + Convert.ToString(h1) + Convert.ToString(h2) + Convert.ToString(h3) + Convert.ToString(h4);
            return finalHash;
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
            SQLiteCommand countCommand = new SQLiteCommand("SELECT COUNT(hashedPassword) FROM userAccount WHERE username = @user", databaseManager.connection);
            countCommand.Parameters.AddWithValue("@user", usernameInput.Text);
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM userAccount WHERE username = @user", databaseManager.connection);
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
                    string hashedPassword = hash(passwordInput.Password);
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
                        getAccessToken.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(factory.getSingleton().apiClient.getClientID() + ":" + factory.getSingleton().apiClient.getClientSecret())));
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
                            SQLiteCommand insertUserAccountRow = new SQLiteCommand("INSERT INTO userAccount (username, hashedPassword, accessToken, playlistID, spotifyID) VALUES (?, ?, ?, ?, ?)", databaseManager.connection);
                            insertUserAccountRow.Parameters.AddWithValue("username", usernameInput.Text);
                            insertUserAccountRow.Parameters.AddWithValue("hashedPassword", hashedPassword);
                            insertUserAccountRow.Parameters.AddWithValue("accessToken", result.refresh_token);
                            insertUserAccountRow.Parameters.AddWithValue("playlistID", playlistID);
                            insertUserAccountRow.Parameters.AddWithValue("spotifyID", userID);
                            insertUserAccountRow.ExecuteNonQuery();
                            SQLiteCommand insertFeaturesRow = new SQLiteCommand("INSERT INTO audioFeature (username, count, danceabilityTotal, energyTotal, speechinessTotal, acousticnessTotal, instrumentalnessTotal, livenessTotal, valenceTotal) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", databaseManager.connection);
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
                            SQLiteCommand insertUserSettingsRow = new SQLiteCommand("INSERT INTO userSettings (explicitTracks, recommendationStrength, username) VALUES (?, ?, ?)", databaseManager.connection);
                            insertUserSettingsRow.Parameters.AddWithValue("explicitTracks", true);
                            insertUserSettingsRow.Parameters.AddWithValue("recommendationStrength", 1);
                            insertUserSettingsRow.Parameters.AddWithValue("username", usernameInput.Text);
                            insertUserSettingsRow.ExecuteNonQuery();
                            await factory.getSingleton().apiClient.analyseListeningHistory();
                        }

                    }
                }
            }
            catch (Exception err)
            {
                Trace.WriteLine(err);
            }
            Thread.Sleep(5000);
            homepage homescreen = new homepage();
            homescreen.Show();
            this.Close();
        }
    }
}
