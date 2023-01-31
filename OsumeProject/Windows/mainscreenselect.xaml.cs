using NAudio.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OsumeProject
{
    /// <summary>
    /// Interaction logic for mainscreenselect.xaml
    /// </summary>
    public partial class mainscreenselect : Window
    {
        public Osume Osume;
        public mainscreenselect(ref Osume Osume)
        {
            Osume = this.Osume;
            if (Osume == null) Osume = new Osume(new apiClient("5ee7e89013d64c0aad8d6c2fd98213b3", "8c3dff68705f421894419de174db4b10", new HttpClient());, new databaseManager(new SQLiteConnection("Data Source=database.db")));
            InitializeComponent();
            if (Osume.databaseManager.open == false)
            {
                Osume.databaseManager.connection.Open();
                Osume.databaseManager.open = true;
                /*
                SQLiteCommand deleteEntity = new SQLiteCommand("DROP TABLE genre", databaseManager.connection);
                deleteEntity.ExecuteNonQuery();
                SQLiteCommand createEntity = new SQLiteCommand("CREATE TABLE genre (genreName TEXT NOT NULL, username TEXT REFERENCES userAccount (username) NOT NULL, numOfLikedSongs REAL NOT NULL, numOfDislikedSongs REAL NOT NULL, FOREIGN KEY (username) REFERENCES userAccount (username), UNIQUE (genreName, username))", databaseManager.connection);
                createEntity.ExecuteNonQuery();
                SQLiteCommand deleteEntity2 = new SQLiteCommand("DROP TABLE audioFeature", databaseManager.connection);
                deleteEntity2.ExecuteNonQuery();
                SQLiteCommand createEntity2 = new SQLiteCommand("CREATE TABLE audioFeature (username TEXT NOT NULL,count INTEGER,danceabilityTotal REAL,energyTotal REAL,speechinessTotal REAL,acousticnessTotal REAL,instrumentalnessTotal REAL,livenessTotal REAL,valenceTotal REAL,FOREIGN KEY(username) REFERENCES userAccount(username))", databaseManager.connection);
                createEntity2.ExecuteNonQuery();
                SQLiteCommand deleteEntity3 = new SQLiteCommand("DROP TABLE userAccount", databaseManager.connection);
                deleteEntity3.ExecuteNonQuery();
                SQLiteCommand createEntity3 = new SQLiteCommand("CREATE TABLE userAccount (username TEXT NOT NULL PRIMARY KEY, hashedPassword TEXT NOT NULL, accessToken TEXT NOT NULL, playlistID TEXT NOT NULL, spotifyID TEXT NOT NULL)", databaseManager.connection);
                createEntity3.ExecuteNonQuery();
                SQLiteCommand deleteEntity4 = new SQLiteCommand("DROP TABLE userSettings", databaseManager.connection);
                deleteEntity4.ExecuteNonQuery();
                SQLiteCommand createEntity4 = new SQLiteCommand("CREATE TABLE userSettings (explicitTracks INTEGER NOT NULL, recommendationStrength INTEGER NOT NULL, username TEXT REFERENCES userAccount (username) NOT NULL)", databaseManager.connection);
                createEntity4.ExecuteNonQuery();
                SQLiteCommand deleteEntity5 = new SQLiteCommand("DROP TABLE savedSong", databaseManager.connection);
                deleteEntity5.ExecuteNonQuery();
                SQLiteCommand createEntity5 = new SQLiteCommand("CREATE TABLE savedSong (songID TEXT NOT NULL PRIMARY KEY, timeSaved DATETIME, username TEXT NOT NULL REFERENCES userAccount(username), FOREIGN KEY (username) REFERENCES userAccount(username))", databaseManager.connection);
                createEntity5.ExecuteNonQuery();
                SQLiteCommand deleteEntity6 = new SQLiteCommand("DROP TABLE blockList", databaseManager.connection);
                deleteEntity6.ExecuteNonQuery();
                SQLiteCommand createEntity6 = new SQLiteCommand("CREATE TABLE blockList (artistID TEXT NOT NULL PRIMARY KEY, timeSaved DATETIME, username TEXT NOT NULL REFERENCES userAccount (username), FOREIGN KEY (username) REFERENCES userAccount (username))", databaseManager.connection);
                createEntity6.ExecuteNonQuery(); */
            }
            
        }
        private void loginButtonClick(object sender, RoutedEventArgs e)
        {
            login loginWindow = new login(ref Osume);
            loginWindow.Show();
            this.Close();
        }
        private void registerButtonClick(object sender, RoutedEventArgs e)
        {
            register registerWindow = new register(ref Osume);
            registerWindow.Show();
            this.Close();
        }

    }
}
