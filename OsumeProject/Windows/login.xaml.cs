using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

namespace OsumeProject
{
    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class login : Window
    {
        public login()
        {
            InitializeComponent();
        }

        private void adminBoxChecked(object sender, RoutedEventArgs e)
        {
            adminKeyLabel.Visibility = Visibility.Visible;
            adminKeyInput.Visibility = Visibility.Visible;
        }
        private void adminBoxUnchecked(object sender, RoutedEventArgs e)
        {
            adminKeyLabel.Visibility = Visibility.Hidden;
            adminKeyInput.Visibility = Visibility.Hidden;
        }

        private void backButtonClick(object sender, RoutedEventArgs e)
        {
            mainscreenselect mss = new mainscreenselect();
            mss.Show();
            this.Close();
        }
        private async void loginButtonClick(object sender, RoutedEventArgs e)
        {
            var username = usernameInput.Text;
            SQLiteCommand countCommand = new SQLiteCommand("SELECT COUNT(hashedPassword) FROM userAccount WHERE username = @user", databaseManager.connection);
            countCommand.Parameters.AddWithValue("@user", username);
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM userAccount WHERE username = @user", databaseManager.connection);
            command.Parameters.AddWithValue("@user", username);
            try
            {
                int count = Convert.ToInt32(countCommand.ExecuteScalar());
                if (count <= 0)
                {
                    errorMessageBox.Text = "Username does not exist!";
                    return;
                } else
                {
                    bool admin = false;
                    if (adminCheckbox.IsChecked == true)
                    {
                        if (adminKeyInput.Password != "538561")
                        {
                            errorMessageBox.Text = "Incorrect admin key!";
                            return;
                        }
                        else admin = true;
                    }
                    string hashedPassword = Hasher.sha1(passwordInput.Password);
                    DataTable data = databaseManager.returnSearchedTable(command);
                    bool validPassword = false;
                    if ((string)data.Rows[0][1] == hashedPassword)
                    {
                        validPassword = true;
                    }
                    if (validPassword)
                    {
                        if (admin == true)
                        {
                            factory.createSingleton(true);
                        }
                        else factory.createSingleton(false);
                        factory.getSingleton().accessToken = (string)data.Rows[0][2];
                        factory.getSingleton().username = username;
                        await factory.getSingleton().getRefreshToken();
                        string userID = await factory.getSingleton().apiClient.getCurrentUserID();
                        string pfpURL = await factory.getSingleton().apiClient.getCurrentUserPFP();
                        factory.getSingleton().pfpURL = pfpURL;
                        factory.getSingleton().userID = userID;
                        factory.getSingleton().playlistID = (string)data.Rows[0][3];
                        await factory.getSingleton().getRefreshToken();
                    } else
                    {
                        errorMessageBox.Text = "Incorrect password!";
                        return;
                    }
                }
            } catch (Exception err)
            {
                Trace.WriteLine(err);
                
            }
            homepage homepageWindow = new homepage();
            homepageWindow.Show();
            this.Close();
        }
    }
}
