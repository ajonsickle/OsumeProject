using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.IO;
using NAudio.Wave;
using System.Net;
using System.Web;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace OsumeProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class homepage : Window
        {
        public Osume Osume;
        DateTime timeStamp;
        public Thread playMP3 { get; set; }
        public OsumeTrack currentSong { get; set; }
       
        public homepage(ref Osume Osume)
        {
            this.Osume = Osume;
            InitializeComponent();
            loadWindow();

        }

        private void ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var uri = "https://open.spotify.com/user/" + factory.getSingleton().userID;
            Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });
        }
        private void libraryButtonClick(object sender, RoutedEventArgs e)
        {
            Osume.songsToPlay.push(currentSong);
            library libraryWindow = new library(ref Osume);
            libraryWindow.Show();
            if (playMP3 != null)
            {
                playMP3.Interrupt();
                playMP3 = null;
            }
            this.Close();
        }
        private void settingsButtonClick(object sender, RoutedEventArgs e)
        {
            Osume.songsToPlay.push(currentSong);
            settings settingsWindow = new settings(ref Osume);
            settingsWindow.Show();
            if (playMP3 != null)
            {
                playMP3.Interrupt();
                playMP3 = null;
            }
            this.Close();
        }
        private async void undo(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - timeStamp).Ticks < 10000000) return;
            timeStamp = DateTime.Now;
            if (Osume.songsPlayed.getLength() > 0)
            {
                OsumeTrack song = Osume.songsPlayed.pop();
                bool liked = Osume.previousSongsLiked.pop();
                if (playMP3 != null)
                {
                    playMP3.Interrupt();
                    playMP3 = null;
                    Osume.songsToPlay.push(currentSong);
                    await Osume.undoChanges(song, liked);
                    await loadSong(song);
                }
            }
        }
        private async void likeButtonClick(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - timeStamp).Ticks < 10000000) return;
            timeStamp = DateTime.Now;
            try
            {
                if (playMP3 != null)
                {
                    playMP3.Interrupt();
                    playMP3 = null;
                    await Osume.makeSongChoice(currentSong, true);
                    await loadSong();
                }
            }
            catch (Exception err)
            {
                Trace.WriteLine(err);
                await loadSong();
            }
        }
        private async void dislikeButtonClick(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - timeStamp).Ticks < 10000000) return;
            timeStamp = DateTime.Now;
            try
            {
                if (playMP3 != null)
                {
                    playMP3.Interrupt();
                    playMP3 = null;
                    await Osume.makeSongChoice(currentSong, false);
                    await loadSong();
                }
            }
            catch (NullReferenceException err)
            {
                Trace.WriteLine(err);
                await loadSong();
                throw err;
            }
        }
        private void toggleRecommendationStrengthClicked(object sender, RoutedEventArgs e)
        {
            int strength = Osume.getRecommendationStrength();
            SQLiteCommand changeRecStrengthSettings = new SQLiteCommand("UPDATE userSettings SET recommendationStrength = @strength WHERE username = @username", Osume.getDatabaseManager().getConnection());
            changeRecStrengthSettings.Parameters.AddWithValue("@username", factory.getSingleton().username);
            if (strength == 0)
            {
                changeRecStrengthSettings.Parameters.AddWithValue("@strength", 1);
                ChangeRecButton.Content = "Normal Recommendations";
                ChangeRecButton.Template = (ControlTemplate)this.Resources["purpleButton"];
            }
            else
            {
                changeRecStrengthSettings.Parameters.AddWithValue("@strength", 0);
                ChangeRecButton.Content = "Expanding Taste";
                ChangeRecButton.Template = (ControlTemplate)this.Resources["pinkButton"];
            }
            changeRecStrengthSettings.ExecuteNonQuery();
        }
        private void setThread(string playbackURL)
        {
            playMP3 = new Thread(() => Osume.playMP3FromUrl(playbackURL));
            playMP3.IsBackground = true;
        }
        private async void loadWindow()
        {
            int strength = Osume.getRecommendationStrength();
            if (strength == 0)
            {
                ChangeRecButton.Content = "Expanding Taste";
                ChangeRecButton.Template = (ControlTemplate)this.Resources["pinkButton"];
            }
            await Osume.getApiClient().refreshToken();
            try
            {
                await loadSong();
            }
            catch (Exception err)
            {
                Trace.WriteLine(err);
            }
        }
        private async Task loadSong(OsumeTrack songToPlay = null)
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(factory.getSingleton().pfpURL));
            profilePicture.Fill = brush;
            Dictionary<string, double> recommendations = null;
            bool validSong = false;
            Random rand = new Random();
            int index = 9;
            if (songToPlay == null)
            {
                recommendations = Osume.generateRecommendations();
            }
            do
            {
                int random = rand.Next(0, index);
                try
                {
                    OsumeTrack song = await Osume.getRecommendation(songToPlay, recommendations, random);
                    if (song != null) { 
                        validSong = true;
                        if (song.album.coverImages[300] != null)
                        {
                            albumCover.Source = new BitmapImage(new Uri(song.album.coverImages[300]));
                        }
                        else
                        {
                            albumCover.Source = null;
                        }
                        songTitle.Text = "🎵 " + song.name;
                        if (song.artists.Length > 0) {
                            artistName.Text = "🧑‍🎤" + song.artists[0].name;
                            yearReleased.Text = "📅 " + song.album.release_date;
                            OsumeArtist genreSearch = await Osume.getApiClient().getArtist(song.artists[0].id);
                            if (genreSearch.genres.Length > 0)
                            {
                                genre.Text = "🏷 " + Regex.Replace(genreSearch.genres[0], @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                            }
                            else
                            {
                                genre.Text = "🏷 " + "None";
                            }
                            albumTitle.Text = "💿 " + song.album.name;
                            currentSong = song;
                            setThread(song.previewURL);
                            playMP3.Start();
                        }
                    } else
                    {
                        if (recommendations != null)
                        {
                            recommendations.Remove(recommendations.ElementAt(random).Key);
                        }
                        index--;
                    }
                }
                catch (Exception err)
                {
                    Trace.WriteLine(err);
                    if (recommendations != null)
                    {
                        recommendations.Remove(recommendations.ElementAt(random).Key);
                        index--;
                    }
                }
            } while (validSong == false);
        }
        
    }
}
