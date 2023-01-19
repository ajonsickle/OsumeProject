﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace OsumeProject
{
    /// <summary>
    /// Interaction logic for settings.xaml
    /// </summary>
    public partial class settings : Window
    {
        public settings()
        {
            InitializeComponent();
            loadSettings();

        }

        private async void viewUsersClicked(object sender, RoutedEventArgs e)
        {
            if (factory.getSingleton().admin != true) return;
            else
            {
                userList userlistWindow = new userList();
                userlistWindow.Show();
                this.Close();
            }
        }
        private void explicitTracksToggleClicked(object sender, RoutedEventArgs e)
        {
            SQLiteCommand checkExplicit = new SQLiteCommand("SELECT explicitTracks FROM userSettings WHERE username = @username", databaseManager.connection);
            checkExplicit.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = databaseManager.returnSearchedTable(checkExplicit);
            SQLiteCommand changeSetting = new SQLiteCommand("UPDATE userSettings SET explicitTracks = @toggle WHERE username = @username", databaseManager.connection);
            changeSetting.Parameters.AddWithValue("@username", factory.getSingleton().username);
            if (Convert.ToInt32(result.Rows[0][0]) == 1)
            {
                changeSetting.Parameters.AddWithValue("@toggle", 0);
                explicitTracksToggle.Content = "Explicit Tracks Off";
            } else
            {
                changeSetting.Parameters.AddWithValue("@toggle", 1);
                explicitTracksToggle.Content = "Explicit Tracks On";
            }
            changeSetting.ExecuteNonQuery();
        }
        private async void loadSettings()
        {
            if (factory.getSingleton().admin == true) viewUsersAdmin.Visibility = Visibility.Visible;
            else viewUsersAdmin.Visibility = Visibility.Hidden;
            SQLiteCommand checkExplicit = new SQLiteCommand("SELECT explicitTracks FROM userSettings WHERE username = @username", databaseManager.connection);
            checkExplicit.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable result = databaseManager.returnSearchedTable(checkExplicit);
            if (Convert.ToInt32(result.Rows[0][0]) == 0) {
                explicitTracksToggle.Content = "Explicit Tracks Off";
            }
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(factory.getSingleton().pfpURL));
            profilePicture.Fill = brush;
            blockedArtists.Children.Clear();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM blockList WHERE username = @username ORDER BY timeSaved DESC", databaseManager.connection);
            command.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable data = databaseManager.returnSearchedTable(command);
            int rectangleTopMargin = 10;
            int number = 0;
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    OsumeArtist artist = await factory.getSingleton().apiClient.getArtist(row[0].ToString());
                    string imageURI = artist.image;
                    var response = await factory.getSingleton().apiClient.client.GetAsync(imageURI);
                    var stream = await response.Content.ReadAsStreamAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    Bitmap image = new Bitmap(memoryStream);
                    int[] rgbValues = (Application.Current.MainWindow as library).getAvgColor(image);
                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle()
                    {
                        Width = 450,
                        Height = 75,
                        RadiusX = 25,
                        RadiusY = 25,
                        Margin = new Thickness(30, rectangleTopMargin, 0, 0),
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)rgbValues[0], (byte)rgbValues[1], (byte)rgbValues[2])),
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 5
                    };
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                    img.Source = new BitmapImage(new Uri(imageURI));
                    img.Width = 35;
                    img.Height = 35;
                    img.Margin = new Thickness(65, 20 + rectangleTopMargin, 0, 0);
                    System.Windows.Controls.TextBlock text = new System.Windows.Controls.TextBlock();
                    text.Text = artist.name;
                    text.Margin = new Thickness(180, 23 + rectangleTopMargin, 0, 0);
                    text.FontSize = 20;
                    System.Windows.Controls.Button button = new System.Windows.Controls.Button();
                    button.Content = "X";
                    button.Click += new RoutedEventHandler(removeButtonClick);
                    button.Margin = new Thickness(400, 28 + rectangleTopMargin, 0, 0);
                    button.Height = 20;
                    button.Width = 20;
                    button.Name = "removeButton" + number.ToString();
                    blockedArtists.Children.Add(rectangle);
                    blockedArtists.Children.Add(img);
                    blockedArtists.Children.Add(text);
                    blockedArtists.Children.Add(button);
                    rectangleTopMargin += 120;
                    if (number >= 4)
                    {
                        blockedArtists.Height += 150;
                    }
                    number++;

                }
                catch (Exception err)
                {
                    Trace.WriteLine(err);
                }

            }
        }
        private void removeButtonClick(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Name[12].ToString();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM blockList WHERE username = @username ORDER BY timeSaved DESC", databaseManager.connection);
            command.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable data = databaseManager.returnSearchedTable(command);
            DataRow row = data.Rows[Convert.ToInt32(name)];
            SQLiteCommand removeArtist = new SQLiteCommand("DELETE FROM blockList WHERE artistID = @id", databaseManager.connection);
            removeArtist.Parameters.AddWithValue("@id", row[0]);
            removeArtist.ExecuteNonQuery();
            loadSettings();
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
        private async void blockArtistButtonClick(object sender, RoutedEventArgs e)
        {
            OsumeArtist artist = await factory.getSingleton().apiClient.getArtistByName(blockArtistText.Text);
            if (artist == null) return;
            SQLiteCommand comm = new SQLiteCommand("INSERT INTO blockList (artistID, timeSaved, username) VALUES (?, ?, ?)", databaseManager.connection);
            comm.Parameters.AddWithValue("@artistID", artist.id);
            comm.Parameters.AddWithValue("@timeSaved", DateTime.Now);
            comm.Parameters.AddWithValue("@username", factory.getSingleton().username);
            comm.ExecuteNonQuery();
            loadSettings();
        }
        private void homeButtonClick(object sender, RoutedEventArgs e)
        {
            homepage homeWindow = new homepage();
            homeWindow.Show();
            this.Close();
        }
        private void libraryButtonClick(object sender, RoutedEventArgs e)
        {
            library libraryWindow = new library();
            libraryWindow.Show();
            this.Close();
        }
        private void logout(object sender, RoutedEventArgs e)
        {
            factory.deleteSingleton();
            mainscreenselect win = new mainscreenselect();
            win.Show();
            this.Close();
        }
    }
}
