using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsumeProject
{
    public class Osume
    {
        private apiClient apiClient;
        public databaseManager databaseManager;
        public Osume(apiClient apiClient, databaseManager databaseManager)
        {
            this.apiClient = apiClient;
            this.databaseManager = databaseManager;
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
    }
}
