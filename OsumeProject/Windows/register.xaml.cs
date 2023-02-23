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
            mainscreenselect mss = new mainscreenselect();
            mss.Show();
            this.Close();
        }

        public async Task analyseListeningHistory()
        {
            errorMessageBox.Visibility = Visibility.Hidden;
            analyseProgressBar.Visibility = Visibility.Visible;
            analyseText.Visibility = Visibility.Visible;
            OsumeTrack[] recentTopTracks = await Osume.getApiClient().getTopTracks("short_term", 50);
            OsumeArtist[] recentTopArtists = await Osume.getApiClient().getTopArtists("short_term", 50);
            SQLiteCommand searchFeatures = new SQLiteCommand("SELECT * FROM audioFeature WHERE username = @user", Osume.getDatabaseManager().connection);
            searchFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
            foreach (OsumeArtist artist in recentTopArtists)
            {
                foreach (string genre in artist.genres)
                {
                    Osume.getDatabaseManager().updateGenres(genre, true, false);
                }
                analyseProgressBar.Value = analyseProgressBar.Value + 1;
            }
            foreach (OsumeTrack track in recentTopTracks)
            {
                DataTable data = Osume.getDatabaseManager().returnSearchedTable(searchFeatures);
                if (data.Rows.Count >= 0)
                {
                    Dictionary<string, double> audioFeatures = await Osume.getApiClient().getAudioFeatures(track.id);
                    Osume.getDatabaseManager().updateAudioFeatures(track, data, false, audioFeatures);
                }
                analyseProgressBar.Value = analyseProgressBar.Value + 1;
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
            SQLiteCommand countCommand = new SQLiteCommand("SELECT COUNT(hashedPassword) FROM userAccount WHERE username = @user", Osume.getDatabaseManager().connection);
            countCommand.Parameters.AddWithValue("@user", usernameInput.Text);
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM userAccount WHERE username = @user", Osume.getDatabaseManager().connection);
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
                    string hashedPassword = Osume.md5(passwordInput.Password);
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
                        await Osume.register(admin, usernameInput.Text, hashedPassword);
                        await analyseListeningHistory();
                        
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
