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
        Osume Osume;
        DateTime timeStamp;
        private Thread playMP3;
        public OsumeTrack currentSong;
        OStack<OsumeTrack> songsPlayed = new OStack<OsumeTrack>();
        OStack<OsumeTrack> songsToPlay = new OStack<OsumeTrack>();
        OStack<bool> previousSongsLiked = new OStack<bool>();
        
        bool undone = false;

        public homepage(ref Osume Osume)
        {
            InitializeComponent();
            loadWindow();
            this.Osume = Osume;
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
            settings settingsWindow = new settings();
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
            if (songsPlayed.getLength() > 0)
            {
                OsumeTrack song = songsPlayed.pop();
                bool liked = previousSongsLiked.pop();
                if (playMP3 != null)
                {
                    playMP3.Interrupt();
                    playMP3 = null;
                    songsToPlay.push(currentSong);
                    await undoChanges(song, liked);
                    await loadSong(song);
                }
            }
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
            SQLiteCommand checkRecSettings = new SQLiteCommand("SELECT recommendationStrength FROM userSettings WHERE username = @username", Osume.databaseManager.connection);
            checkRecSettings.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = Osume.databaseManager.returnSearchedTable(checkRecSettings);
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

        public double[] getAudioFeatureTasteVector()
        {
            OList<double> vector = new OList<double>();
            SQLiteCommand getAudioFeaturesForUser = new SQLiteCommand("SELECT (danceabilityTotal / count), (energyTotal / count), (speechinessTotal / count), (acousticnessTotal / count), (instrumentalnessTotal / count), (livenessTotal / count), (valenceTotal / count) FROM audioFeature WHERE username = @username", Osume.databaseManager.connection);
            getAudioFeaturesForUser.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = Osume.databaseManager.returnSearchedTable(getAudioFeaturesForUser);
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
            SQLiteCommand getAllGenresForUser = new SQLiteCommand("SELECT genreName, (numOfLikedSongs / numOfDislikedSongs) FROM genre WHERE username = @username", Osume.databaseManager.connection);
            getAllGenresForUser.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = Osume.databaseManager.returnSearchedTable(getAllGenresForUser);
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
                    SQLiteCommand command = new SQLiteCommand("INSERT INTO savedSong (songID, timeSaved, username) VALUES (?, ?, ?)", Osume.databaseManager.connection);
                    command.Parameters.AddWithValue("songID", currentSong.id);
                    command.Parameters.AddWithValue("timeSaved", DateTime.Now);
                    command.Parameters.AddWithValue("username", factory.getSingleton().username);
                    command.ExecuteNonQuery();
                    factory.getSingleton().apiClient.addToPlaylist(factory.getSingleton().playlistID, currentSong.id);
                    await updateAudioFeatures(currentSong, false);
                    updateGenres(currentSong, true, false);
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
                SQLiteCommand getCurrentFeatures = new SQLiteCommand("SELECT * FROM audioFeature WHERE username = @user", Osume.databaseManager.connection);
                getCurrentFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
                featureData = Osume.databaseManager.returnSearchedTable(getCurrentFeatures);
            }
            if (featureData == null)
            {
                // clicked dislike
                updateGenres(track, false, true);
            } else
            {
                // clicked like
                updateGenres(track, true, true);
                await updateAudioFeatures(track, true);
                SQLiteCommand deleteFromLibrary = new SQLiteCommand("DELETE FROM savedSong WHERE songID = @songID AND username = @user", Osume.databaseManager.connection);
                deleteFromLibrary.Parameters.AddWithValue("@songID", track.id);
                deleteFromLibrary.Parameters.AddWithValue("@user", factory.getSingleton().username);
                deleteFromLibrary.ExecuteNonQuery();
                factory.getSingleton().apiClient.removeFromPlaylist(factory.getSingleton().playlistID, new string[]{ track.id });
            }

        }
        private void updateGenres(OsumeTrack track, bool like, bool undo)
        {
            OList<string> addedGenres = new OList<string>();
            foreach (OsumeArtist artist in track.artists)
            {
                foreach (string genre in artist.genres)
                {
                    if (!addedGenres.contains(genre))
                    {
                        addedGenres.add(genre);
                        Osume.databaseManager.updateGenres(genre, like, undo);
                    }
                }
            }
        }

        private async Task updateAudioFeatures(OsumeTrack track, bool undo)
        {
            SQLiteCommand getCurrentFeatures = new SQLiteCommand("SELECT * FROM audioFeature WHERE username = @user", Osume.databaseManager.connection);
            getCurrentFeatures.Parameters.AddWithValue("@user", factory.getSingleton().username);
            DataTable data = Osume.databaseManager.returnSearchedTable(getCurrentFeatures);
            Dictionary<string, double> audioFeatures = await Osume.getApiClient().getAudioFeatures(track.id);
            Osume.databaseManager.updateAudioFeatures(track, data, undo, audioFeatures);
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
                    updateGenres(currentSong, false, false);
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
            SQLiteCommand checkRecSettings = new SQLiteCommand("SELECT recommendationStrength FROM userSettings WHERE username = @username", Osume.databaseManager.connection);
            checkRecSettings.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = Osume.databaseManager.returnSearchedTable(checkRecSettings);
            int strength = Convert.ToInt32(result.Rows[0][0]);
            SQLiteCommand changeRecStrengthSettings = new SQLiteCommand("UPDATE userSettings SET recommendationStrength = @strength WHERE username = @username", Osume.databaseManager.connection);
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
        private void setThread(string playbackURL)
        {
            playMP3 = new Thread(() => PlayMp3FromUrl(playbackURL));
            playMP3.IsBackground = true;
        }
        private async void loadWindow()
        {
            SQLiteCommand checkRecSettings = new SQLiteCommand("SELECT recommendationStrength FROM userSettings WHERE username = @username", Osume.databaseManager.connection);
            checkRecSettings.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = Osume.databaseManager.returnSearchedTable(checkRecSettings);
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
            int index = 14;
            if (songToPlay == null)
            {
                recommendations = generateRecommendations();
            }
            do
            {
                try
                {
                    OsumeTrack song = null;
                    if (songToPlay == null)
                    {
                        if (songsToPlay.getLength() == 0)
                        {
                            song = await factory.getSingleton().apiClient.getTrack(recommendations.ElementAt(rand.Next(0, index)).Key);
                        } else
                        {
                            song = songsToPlay.pop();
                        }
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
                        SQLiteCommand checkExplicitSetting = new SQLiteCommand("SELECT explicitTracks FROM userSettings WHERE username = @username", Osume.databaseManager.connection);
                        checkExplicitSetting.Parameters.AddWithValue("@username", factory.getSingleton().username);
                        DataTable table0 = Osume.databaseManager.returnSearchedTable(checkExplicitSetting);
                        if (Convert.ToInt32(table0.Rows[0][0]) == 0)
                        {
                            allowed = false;
                        }
                        else allowed = true;
                    } else
                    {
                        allowed = true;
                    }
                    SQLiteCommand searchBlockList = new SQLiteCommand("SELECT * FROM blockList WHERE username = @username AND artistID = @artistID", Osume.databaseManager.connection);
                    searchBlockList.Parameters.AddWithValue("@username", factory.getSingleton().username);
                    if (song.artists.Length > 0)
                    {
                        searchBlockList.Parameters.AddWithValue("@artistID", song.artists[0].id);
                    }
                    else
                    {
                        searchBlockList.Parameters.AddWithValue("@artistID", "null");
                    }
                    DataTable table1 = Osume.databaseManager.returnSearchedTable(searchBlockList);
                    SQLiteCommand searchSavedSong = new SQLiteCommand("SELECT * FROM savedSong WHERE username = @username AND songID = @songID", Osume.databaseManager.connection);
                    searchSavedSong.Parameters.AddWithValue("@username", factory.getSingleton().username);
                    searchSavedSong.Parameters.AddWithValue("@songID", song.id);
                    DataTable table2 = Osume.databaseManager.returnSearchedTable(searchSavedSong);
                    if (table1.Rows.Count == 0 && table2.Rows.Count == 0 && allowed == true)
                    {
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
                            setThread(song.previewURL);
                            playMP3.Start();
                        }
                    } else
                    {
                        if (recommendations != null)
                        {
                            recommendations.Remove(recommendations.ElementAt(rand.Next(0, index)).Key);
                        }
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
