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
        public mainscreenselect()
        {
            Osume = new Osume(new apiClient("5ee7e89013d64c0aad8d6c2fd98213b3", "8c3dff68705f421894419de174db4b10", new HttpClient()), new databaseManager(new SQLiteConnection("Data Source=database.db")));
            InitializeComponent();
            if (Osume.getDatabaseManager().getOpen() == false)
            {
                Osume.getDatabaseManager().getConnection().Open();
                Osume.getDatabaseManager().setOpen(true);
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
