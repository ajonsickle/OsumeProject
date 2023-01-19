using System;
using System.Collections.Generic;
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

namespace OsumeProject.Windows
{
    /// <summary>
    /// Interaction logic for genresView.xaml
    /// </summary>
    public partial class genresView : Window
    {
        OList<string> genres = new OList<string>();
        bool ascending = true;
        public genresView()
        {
            InitializeComponent();
            StreamReader sr = new StreamReader("genresList.txt");
            while (sr.Peek() != -1)
            {
                genres.add(sr.ReadLine());
            }
            sr.Close();
            loadList();
        }
        private void backButtonClick(object sender, RoutedEventArgs e)
        {
            settings settingsWindow = new settings();
            settingsWindow.Show();
            this.Close();
        }
        private void toggleSort(object sender, RoutedEventArgs e)
        {
            ascending = ascending == true ? false : true;
            genres = genres.sort(genres, ascending);
            loadList();
        }
        private void loadList()
        {
            genresList.Children.Clear();
            int topMargin = 0;
            for (int i = 0; i < genres.getLength(); i++)
            {
                TextBlock txt = new TextBlock();
                txt.Text = (i + 1) + ". " + genres.getByIndex(i);
                txt.FontSize = 20;
                txt.Margin = new Thickness(65, 12 + topMargin, 0, 0);
                txt.Foreground = new SolidColorBrush(Colors.White);
                genresList.Children.Add(txt);
                if (i >= 5)
                {
                    genresList.Height += 150;
                }
                topMargin += 50;
            }
        }
    }
}
