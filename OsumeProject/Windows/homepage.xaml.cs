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

namespace OsumeProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class homepage : Window
    {
        DateTime timeStamp;
        private Thread playMP3;
        public OsumeTrack currentSong;
        OStack<OsumeTrack> songsPlayed = new OStack<OsumeTrack>();
        OStack<bool> previousSongsLiked = new OStack<bool>();
        
        bool undone = false;

        public homepage()
        {
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
            library libraryWindow = new library();
            libraryWindow.Show();
            playMP3.Interrupt();
            playMP3 = null;
            this.Close();
        }
        private void settingsButtonClick(object sender, RoutedEventArgs e)
        {
            settings settingsWindow = new settings();
            settingsWindow.Show();
            playMP3.Interrupt();
            playMP3 = null;
            this.Close();
        }

        private async void undo(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - timeStamp).Ticks < 10000000) return;
            timeStamp = DateTime.Now;
            if (songsPlayed.getLength() > 0)
            {
                OsumeTrack song = songsPlayed.pop();
                bool liked = previousSongsLiked.pop();
                if (playMP3 != null)
                {
                    playMP3.Interrupt();
                    playMP3 = null;
                    await undoChanges(song, liked);
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
                    SQLiteCommand command = new SQLiteCommand("INSERT INTO savedSong (songID, timeSaved, username) VALUES (?, ?, ?)", databaseManager.connection);
                    command.Parameters.AddWithValue("songID", currentSong.id);
                    command.Parameters.AddWithValue("timeSaved", DateTime.Now);
                    command.Parameters.AddWithValue("username", factory.getSingleton().username);
                    command.ExecuteNonQuery();
                    factory.getSingleton().apiClient.addToPlaylist(factory.getSingleton().playlistID, currentSong.id);
                    await updateAudioFeatures(currentSong, false);
                    await updateGenres(currentSong, true, false);
                    songsPlayed.push(currentSong);
                    previousSongsLiked.push(true);
                    await loadSong();
                }
            }
            catch (Exception err)
            {
                Trace.WriteLine(err);
                await loadSong();
            }
        }
        private async Task undoChanges(OsumeTrack track, bool liked)
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
                await updateGenres(track, false, true);
            } else
            {
                // clicked like
                await updateGenres(track, true, true);
                await updateAudioFeatures(track, true);
                SQLiteCommand deleteFromLibrary = new SQLiteCommand("DELETE FROM savedSong WHERE songID = @songID AND username = @user", databaseManager.connection);
                deleteFromLibrary.Parameters.AddWithValue("@songID", track.id);
                deleteFromLibrary.Parameters.AddWithValue("@user", factory.getSingleton().username);
                deleteFromLibrary.ExecuteNonQuery();
            }

        }
        private async Task updateGenres(OsumeTrack track, bool like, bool undo)
        {
            OList<string> addedGenres = new OList<string>();
            foreach (OsumeArtist artist in track.artists)
            {
                foreach (string genre in artist.genres)
                {
                    if (!addedGenres.contains(genre))
                    {
                        addedGenres.add(genre);
                        await factory.getSingleton().apiClient.updateGenres(genre, like, undo);
                    }
                }
            }
        }

        private async Task updateAudioFeatures(OsumeTrack track, bool undo)
        {
            SQLiteCommand getCurrentFeatures = new SQLiteCommand("SELECT * FROM audioFeature WHERE username = @user", databaseManager.connection);
            getCurrentFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
            DataTable data = databaseManager.returnSearchedTable(getCurrentFeatures);
            await factory.getSingleton().apiClient.updateAudioFeatures(track, data, undo);
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
                    await updateGenres(currentSong, false, false);
                    songsPlayed.push(currentSong);
                    previousSongsLiked.push(true);
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
        
        private void toggleRecommendationStrength(object sender, RoutedEventArgs e)
        {
            SQLiteCommand checkRecSettings = new SQLiteCommand("SELECT recommendationStrength FROM userSettings WHERE username = @username", databaseManager.connection);
            checkRecSettings.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = databaseManager.returnSearchedTable(checkRecSettings);
            int strength = Convert.ToInt32(result.Rows[0][0]);
            SQLiteCommand changeRecStrengthSettings = new SQLiteCommand("UPDATE userSettings SET recommendationStrength = @strength WHERE username = @username", databaseManager.connection);
            changeRecStrengthSettings.Parameters.AddWithValue("@username", factory.getSingleton().username);
            if (strength == 0)
            {
                changeRecStrengthSettings.Parameters.AddWithValue("@strength", 1);
                ChangeRecButton.Content = "Normal Recommendations";
                ChangeRecButton.Template = (ControlTemplate)this.Resources["purpleButton"];
            } else
            {
                changeRecStrengthSettings.Parameters.AddWithValue("@strength", 0);
                ChangeRecButton.Content = "Expanding Taste";
                ChangeRecButton.Template = (ControlTemplate)this.Resources["pinkButton"];
            }
            changeRecStrengthSettings.ExecuteNonQuery();
        }

        public static void PlayMp3FromUrl(string url)
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
        private async Task setThread(string playbackURL)
        {
            playMP3 = new Thread(() => PlayMp3FromUrl(playbackURL));
            playMP3.IsBackground = true;
        }
        private async void loadWindow()
        {
            SQLiteCommand checkRecSettings = new SQLiteCommand("SELECT recommendationStrength FROM userSettings WHERE username = @username", databaseManager.connection);
            checkRecSettings.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = databaseManager.returnSearchedTable(checkRecSettings);
            int strength = Convert.ToInt32(result.Rows[0][0]);
            if (strength == 0)
            {
                ChangeRecButton.Content = "Expanding Taste";
                ChangeRecButton.Template = (ControlTemplate)this.Resources["pinkButton"];
            }
            await factory.getSingleton().getRefreshToken();
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
            Dictionary<string, double> recommendations = null;
            Random rand = new Random();
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(factory.getSingleton().pfpURL));
            profilePicture.Fill = brush;
            bool validSong = false;
            int index = 24;
            if (songToPlay == null)
            {
                recommendations = factory.getSingleton().apiClient.generateRecommendations();
            }
            do
            {
                try
                {
                    OsumeTrack song = null;
                    if (songToPlay == null)
                    {
                        Trace.WriteLine("Hi");
                        song = await factory.getSingleton().apiClient.getTrack(recommendations.ElementAt(rand.Next(0, index)).Key);
                        undone = false;
                    }
                    else
                    {
                        song = songToPlay;
                        undone = true;
                    }

                    bool allowed = false;

                    if (song.isExplicit)
                    {
                        SQLiteCommand checkExplicitSetting = new SQLiteCommand("SELECT explicitTracks FROM userSettings WHERE username = @username", databaseManager.connection);
                        checkExplicitSetting.Parameters.AddWithValue("@username", factory.getSingleton().username);
                        DataTable table0 = databaseManager.returnSearchedTable(checkExplicitSetting);
                        if (Convert.ToInt32(table0.Rows[0][0]) == 0)
                        {
                            allowed = false;
                        }
                        else allowed = true;
                    } else
                    {
                        allowed = true;
                    }
                    SQLiteCommand searchBlockList = new SQLiteCommand("SELECT * FROM blockList WHERE username = @username AND artistID = @artistID", databaseManager.connection);
                    searchBlockList.Parameters.AddWithValue("@username", factory.getSingleton().username);
                    if (song.artists.Length > 0)
                    {
                        searchBlockList.Parameters.AddWithValue("@artistID", song.artists[0].id);
                    }
                    else
                    {
                        searchBlockList.Parameters.AddWithValue("@artistID", "null");
                    }
                    DataTable table1 = databaseManager.returnSearchedTable(searchBlockList);
                    SQLiteCommand searchSavedSong = new SQLiteCommand("SELECT * FROM savedSong WHERE username = @username AND songID = @songID", databaseManager.connection);
                    searchSavedSong.Parameters.AddWithValue("@username", factory.getSingleton().username);
                    searchSavedSong.Parameters.AddWithValue("@songID", song.id);
                    DataTable table2 = databaseManager.returnSearchedTable(searchSavedSong);
                    if (table1.Rows.Count == 0 && table2.Rows.Count == 0 && allowed == true)
                    {
                        validSong = true;
                        if (song.album.cover.images[300] != null)
                        {
                            albumCover.Source = new BitmapImage(new Uri(song.album.cover.images[300]));
                        }
                        else
                        {
                            albumCover.Source = null;
                        }
                        songTitle.Text = "🎵 " + song.name;
                        if (song.artists.Length > 0) {
                            artistName.Text = "🧑‍🎤" + song.artists[0].name;
                            yearReleased.Text = "📅 " + song.album.release_date;
                            OsumeArtist genreSearch = await factory.getSingleton().apiClient.getArtist(song.artists[0].id);
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
                            await setThread(song.previewURL);
                            playMP3.Start();
                        }
                    } else
                    {
                        recommendations.Remove(recommendations.ElementAt(rand.Next(0, index)).Key);
                        index--;
                    }
                }
                catch (Exception err)
                {
                    Trace.WriteLine(err);
                    if (recommendations != null)
                    {
                        recommendations.Remove(recommendations.ElementAt(rand.Next(0, index)).Key);
                        index--;
                    }
                }
            } while (validSong == false);
        }
        
    }
}
