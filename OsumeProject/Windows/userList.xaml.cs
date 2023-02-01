using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
    /// Interaction logic for addUser.xaml
    /// </summary>
    public partial class userList : Window
    {
        public Osume Osume;
        public userList(ref Osume Osume)
        {
            this.Osume = Osume;
            InitializeComponent();
            loadUserList();

        }

        private void profilePictureClick(object sender, RoutedEventArgs e)
        {
            string uri = Convert.ToString(((System.Windows.Controls.Image)sender).Tag);
            Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });
        }
        private void backButtonClick(object sender, RoutedEventArgs e)
        {
            settings settingsWindow = new settings(ref Osume);
            settingsWindow.Show();
            this.Close();
        }

        private void removeButtonClick(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Name[12].ToString();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM userAccount WHERE NOT (username = @username) ORDER BY username DESC", Osume.databaseManager.connection);
            command.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable data = Osume.databaseManager.returnSearchedTable(command);
            DataRow row = data.Rows[Convert.ToInt32(name)];
            SQLiteCommand removeSong = new SQLiteCommand("DELETE FROM userAccount WHERE username = @username", Osume.databaseManager.connection);
            removeSong.Parameters.AddWithValue("@username", row[0]);
            removeSong.ExecuteNonQuery();
            loadUserList();
        }

        private async void loadUserList()
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(factory.getSingleton().pfpURL));
            usersList.Children.Clear();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM userAccount WHERE NOT (username = @username) ORDER BY username DESC", Osume.databaseManager.connection);
            command.Parameters.AddWithValue("@username", factory.getSingleton().username);
            DataTable data = Osume.databaseManager.returnSearchedTable(command);
            int rectangleTopMargin = 0;
            int number = 0;
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    string username = row[0].ToString();
                    string imageURI = factory.getSingleton().pfpURL;
                    var response = await Osume.getApiClient().client.GetAsync(imageURI);
                    var stream = await response.Content.ReadAsStreamAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    Bitmap image = new Bitmap(memoryStream);
                    int[] rgbValues = Osume.getAvgColor(image);
                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle()
                    {
                        Width = 600,
                        Height = 100,
                        RadiusX = 20,
                        RadiusY = 20,
                        Margin = new Thickness(30, rectangleTopMargin, 0, 0),
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)rgbValues[0], (byte)rgbValues[1], (byte)rgbValues[2])),
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 5
                    };
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                    img.Source = new BitmapImage(new Uri(imageURI));
                    img.Width = 75;
                    img.Height = 75;
                    img.Margin = new Thickness(65, 12 + rectangleTopMargin, 0, 0);
                    img.MouseUp += new MouseButtonEventHandler(profilePictureClick);
                    img.Tag = "https://open.spotify.com/user/" + row[4].ToString();
                    System.Windows.Controls.TextBlock text = new System.Windows.Controls.TextBlock();
                    text.Text = username;
                    text.Margin = new Thickness(180, 40 + rectangleTopMargin, 0, 0);
                    text.FontSize = 15;
                    System.Windows.Controls.Button button = new System.Windows.Controls.Button();
                    button.Content = "X";
                    button.Click += new RoutedEventHandler(removeButtonClick);
                    button.Margin = new Thickness(500, 40 + rectangleTopMargin, 0, 0);
                    button.Height = 20;
                    button.Width = 20;
                    button.Name = "removeButton" + number.ToString();
                    usersList.Children.Add(rectangle);
                    usersList.Children.Add(img);
                    usersList.Children.Add(text);
                    usersList.Children.Add(button);
                    rectangleTopMargin += 120;
                    if (number >= 5)
                    {
                        usersList.Height += 150;
                    }
                    number++;

                }
                catch (Exception err)
                {
                    Trace.WriteLine(err);
                }

            }
        }

    }
}
