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

namespace OsumeProject
{
    /// <summary>
    /// Interaction logic for library.xaml
    /// </summary>
    public partial class library : Window
    {
        public Osume Osume;
        public library(ref Osume Osume)    
        {
            this.Osume = Osume;
            InitializeComponent();
            loadLibrary();
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

        private async void loadLibrary()
        {
            scrollView.ScrollToTop();
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(factory.getSingleton().pfpURL));
            profilePicture.Fill = brush;
            songsList.Children.Clear();
            DataTable data = Osume.getSavedSongs();
            int rectangleTopMargin = 0;
            int number = 0;
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    OsumeTrack song = await Osume.getApiClient().getTrack(row[0].ToString());
                    string imageURI = song.album.coverImages[64];
                    var response = await Osume.getApiClient().client.GetAsync(imageURI);
                    var stream = await response.Content.ReadAsStreamAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    Bitmap image = new Bitmap(memoryStream);
                    int[] rgbValues = Osume.getAvgColor(image);
                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle()
                    {
                        Width = 1200,
                        Height = 100,
                        RadiusX = 50,
                        RadiusY = 50,
                        Margin = new Thickness(30, rectangleTopMargin, 0, 0),
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)rgbValues[0], (byte)rgbValues[1], (byte)rgbValues[2])),
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 5
                    };
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                    img.Source = new BitmapImage(new Uri(song.album.coverImages[64]));
                    img.Width = 75;
                    img.Height = 75;
                    img.Margin = new Thickness(65, 12 + rectangleTopMargin, 0, 0);
                    System.Windows.Controls.TextBlock text = new System.Windows.Controls.TextBlock();
                    string artistString = song.artists[0].name;
                    foreach (var artist in song.artists)
                    {
                        if (artist != null)
                        {
                            if (!artistString.Contains(artist.name))
                            {
                                artistString += ", " + artist.name;
                            }
                        }
                    }
                    text.Text = artistString + " - " + song.name;
                    text.Margin = new Thickness(180, 40 + rectangleTopMargin, 0, 0);
                    text.FontSize = 15;
                    System.Windows.Controls.Button button = new System.Windows.Controls.Button();
                    button.Content = "X";
                    button.Click += new RoutedEventHandler(removeButtonClick);
                    button.Margin = new Thickness(1130, 40 + rectangleTopMargin, 0, 0);
                    button.Height = 20;
                    button.Width = 20;
                    button.Name = "removeButton" + number.ToString();
                    songsList.Children.Add(rectangle);
                    songsList.Children.Add(img);
                    songsList.Children.Add(text);
                    songsList.Children.Add(button);
                    rectangleTopMargin += 120;
                    if (number >= 8)
                    {
                        songsList.Height += 150;
                    }
                    number++;

                } catch (Exception err)
                {
                    Trace.WriteLine(err);
                }
                
            }
        }
        private void removeButtonClick(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Name.ToString();
            string substring = name.Substring(12, name.Length - 12);
            Osume.removeFromLibrary(substring);
            loadLibrary();
        }
        private void homeButtonClick(object sender, RoutedEventArgs e)
        {
            homepage homeWindow = new homepage(ref Osume);
            homeWindow.Show();
            this.Close();
        }
        private void settingsButtonClick(object sender, RoutedEventArgs e)
        {
            settings settingsWindow = new settings(ref Osume);
            settingsWindow.Show();
            this.Close();
        }
    }
}
