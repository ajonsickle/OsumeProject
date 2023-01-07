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
        public string currentSongID;
        public homepage()
        {
            InitializeComponent();
            loadWindow();
            factory.getSingleton().apiClient.cosineSimilarityForRecommendation();
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
        private async void likeButtonClick(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - timeStamp).Ticks < 10000000) return;
            timeStamp = DateTime.Now;
            try
            {
                playMP3.Interrupt();
                playMP3 = null;
                SQLiteCommand command = new SQLiteCommand("INSERT INTO savedSong (songID, timeSaved, username) VALUES (?, ?, ?)", databaseManager.connection);
                command.Parameters.AddWithValue("songID", currentSongID);
                command.Parameters.AddWithValue("timeSaved", DateTime.Now);
                command.Parameters.AddWithValue("username", factory.getSingleton().username);
                command.ExecuteNonQuery();
                factory.getSingleton().apiClient.addToPlaylist(factory.getSingleton().playlistID, currentSongID);
                OsumeTrack current = await factory.getSingleton().apiClient.getTrack(currentSongID);
                await updateAudioFeatures(current);
                await updateGenres(current, true);
                await loadSong();
            }
            catch (Exception err)
            {
                Trace.WriteLine(err);
                await loadSong();
            }
        }
        private async Task updateGenres(OsumeTrack track, bool like)
        {
            List<string> addedGenres = new List<string>();
            foreach (OsumeArtist artist in track.artists)
            {
                foreach (string genre in artist.genres)
                {
                    if (!addedGenres.Contains(genre))
                    {
                        addedGenres.Add(genre);
                        await factory.getSingleton().apiClient.updateGenres(genre, like);
                    }
                }
            }
        }
        private async Task updateAudioFeatures(OsumeTrack track)
        {
            SQLiteCommand getCurrentFeatures = new SQLiteCommand("SELECT * FROM audioFeature WHERE username = @user", databaseManager.connection);
            getCurrentFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
            DataTable data = databaseManager.returnSearchedTable(getCurrentFeatures);
            await factory.getSingleton().apiClient.updateAudioFeatures(track, data);
        }
        private async void dislikeButtonClick(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - timeStamp).Ticks < 10000000) return;
            timeStamp = DateTime.Now;
            try
            {
                playMP3.Interrupt();
                playMP3 = null;
                OsumeTrack current = await factory.getSingleton().apiClient.getTrack(currentSongID);
                await updateAudioFeatures(current);
                await updateGenres(current, false);
                await loadSong();
            }
            catch (NullReferenceException err)
            {
                Trace.WriteLine(err);
                await loadSong();
                throw err;
            }
        }
        private async void loadWindow()
        {
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
        private async Task loadSong()
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(factory.getSingleton().pfpURL));
            profilePicture.Fill = brush;
            bool validSong = false;
            do
            {
                try
                {
                    OsumeTrack song = await factory.getSingleton().apiClient.getRandomTopTrack("short_term", 50);
                    SQLiteCommand searchBlockList = new SQLiteCommand("SELECT * FROM blockList WHERE username = @username AND artistID = @artistID", databaseManager.connection);
                    searchBlockList.Parameters.AddWithValue("@username", factory.getSingleton().username);
                    searchBlockList.Parameters.AddWithValue("@artistID", song.artists[0].id);
                    DataTable table = databaseManager.returnSearchedTable(searchBlockList);
                    if (table.Rows.Count == 0)
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
                        artistName.Text = "🧑‍🎤" + song.artists[0].name;
                        yearReleased.Text = "📅 " + song.album.release_date;
                        OsumeArtist genreSearch = await factory.getSingleton().apiClient.getArtist(song.artists[0].id);
                        genre.Text = "🏷 " + Regex.Replace(genreSearch.genres[0], @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                        albumTitle.Text = "💿 " + song.album.name;
                        currentSongID = song.id;
                        await setThread(song.previewURL);
                        playMP3.Start();
                    }
                }
                catch (Exception err)
                {
                    throw err;
                }
            } while (validSong == false);
        }
        public static void PlayMp3FromUrl(string url)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, "song.mp3");
                try
                {
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
    }
}
