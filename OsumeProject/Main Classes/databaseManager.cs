﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Data.SQLite;
using System.Data;
using System.Threading.Tasks;

namespace OsumeProject
{
    public class databaseManager
    {
        private SQLiteConnection connection;
        private bool open;

        public databaseManager(SQLiteConnection connection)
        {
            this.connection = connection;
            open = false;
        }

        public SQLiteConnection getConnection()
        {
            return this.connection;
        }

        public void setConnection(SQLiteConnection connection)
        {
            this.connection = connection;
        }

        public bool getOpen()
        {
            return this.open;
        }

        public void setOpen(bool open)
        {
            this.open = open;
        }

        public DataTable returnSearchedTable(SQLiteCommand command)
        {
            var reader = command.ExecuteReader();
            DataTable data = new DataTable();
            data.Load(reader);
            return data;
        }

        public void insertRowIntoGenre(string genreName)
        {
            SQLiteCommand updateGenre = new SQLiteCommand("INSERT INTO genre (genreName, username, numOfLikedSongs, numOfDislikedSongs) VALUES (?, ?, ?, ?)", this.connection);
            updateGenre.Parameters.AddWithValue("genreName", genreName);
            updateGenre.Parameters.AddWithValue("username", factory.getSingleton().username);
            updateGenre.Parameters.AddWithValue("numOfLikedSongs", 1);
            updateGenre.Parameters.AddWithValue("numOfDislikedSongs", 1);
            updateGenre.ExecuteNonQuery();
        }

        public void incrementGenreLikedSongs(string genreName, bool like, bool undo)
        {
            if (undo)
            {
                SQLiteCommand decrementGenreValues;
                if (like)
                {
                    decrementGenreValues = new SQLiteCommand("UPDATE genre SET numOfLikedSongs = numOfLikedSongs - 1 WHERE genreName = @genreName AND username = @user", this.connection);
                }
                else
                {
                    decrementGenreValues = new SQLiteCommand("UPDATE genre SET numOfDislikedSongs = numOfDislikedSongs - 1 WHERE genreName = @genreName AND username = @user", this.connection);
                }
                decrementGenreValues.Parameters.AddWithValue("@genreName", genreName);
                decrementGenreValues.Parameters.AddWithValue("@user", factory.getSingleton().username);
                decrementGenreValues.ExecuteNonQuery();
            }
            else
            {
                SQLiteCommand incrementGenreValues;
                if (like)
                {
                    incrementGenreValues = new SQLiteCommand("UPDATE genre SET numOfLikedSongs = numOfLikedSongs + 1 WHERE genreName = @genreName AND username = @user", this.connection);
                }
                else
                {
                    incrementGenreValues = new SQLiteCommand("UPDATE genre SET numOfDislikedSongs = numOfDislikedSongs + 1 WHERE genreName = @genreName AND username = @user", this.connection);
                }
                incrementGenreValues.Parameters.AddWithValue("@genreName", genreName);
                incrementGenreValues.Parameters.AddWithValue("@user", factory.getSingleton().username);
                incrementGenreValues.ExecuteNonQuery();
            }
        }

        public void updateGenres(string genre, bool like, bool undo)
        {
            try
            {
                insertRowIntoGenre(genre);
            }
            catch (Exception err)
            {
                incrementGenreLikedSongs(genre, like, undo);
            }
        }

        public void updateAudioFeatures(OsumeTrack track, DataTable data, bool undo, Dictionary<string, double> audioFeatures)
        {
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
    "WHERE username = @user", this.connection);

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
            }
            else
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

    }
}