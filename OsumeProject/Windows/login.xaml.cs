using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
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

        public string hash(string input)
        {
            long h0 = 0x67452301;
            long h1 = 0xEFCDAB89;
            long h2 = 0x98BADCFE;
            long h3 = 0x10325476;
            long h4 = 0xC3D2E1F0;
            string binaryString = string.Join(" ", Encoding.GetEncoding("ASCII").GetBytes(input).Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
            binaryString += '1';
            while (binaryString.Length % 512 != 448)
            {
                binaryString += '0';
            }
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            var endianInteger = BitConverter.ToUInt64(bytes);
            string endianString = Convert.ToString(endianInteger);
            binaryString += endianString;
            IEnumerable<string> chunks = Enumerable.Range(0, binaryString.Length / 512).Select(i => binaryString.Substring(i * 512, 512));
            foreach (var chunk in chunks)
            {
                IEnumerable<string> words = Enumerable.Range(0, chunk.Length / 32).Select(j => chunk.Substring(j * 32, 32));
                string[] wordArray = words.ToArray();
                byte[] byteArray = wordArray.Select(byte.Parse).ToArray();
                for (int i = 16; i < 79; i++)
                {
                    byteArray[i] = (byte)BitOperations.RotateLeft((uint)(byteArray[i - 3] ^ byteArray[i - 8] ^ byteArray[i - 14] ^ byteArray[i - 16]), 1);
                }
                long a = h0;
                long b = h1;
                long c = h2;
                long d = h3;
                long e = h4;
                long f = 0;
                long k = 0;
                for (int j = 0; j < 79; j++)
                {
                    if (j >= 0 && j <= 19)
                    {
                        f = (b & c) | ((~b) & d);
                        k = 0x5A827999;
                    }
                    else if (j >= 20 && j <= 39)
                    {
                        f = b ^ c ^ d;
                        k = 0x6ED9EBA1;
                    }
                    else if (j >= 40 && j <= 59)
                    {
                        f = (b & c) | (b & d) | (c & d);
                        k = 0x8F1BBCDC;
                    }
                    else if (j >= 60 && j <= 70)
                    {
                        f = b ^ c ^ d;
                        k = 0xCA62C1D6;
                    }
                    long temp = (BitOperations.RotateLeft((uint)a, 5)) + f + e + k + byteArray[j];
                    e = d;
                    d = c;
                    c = BitOperations.RotateLeft((uint)b, 30);
                    b = a;
                    a = temp;
                }
                h0 = h0 + a;
                h1 = h1 + b;
                h2 = h2 + c;
                h3 = h3 + d;
                h4 = h4 + e;
            }
            string finalHash = Convert.ToString(h0) + Convert.ToString(h1) + Convert.ToString(h2) + Convert.ToString(h3) + Convert.ToString(h4);
            return finalHash;
        }

        public string sha1(string input)
        {
            string output = "";
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            SHA1 hasher = SHA1.Create();
            byte[] computedHash = hasher.ComputeHash(inputBytes);
            foreach (var hashedByte in computedHash)
            {
                output += hashedByte.ToString("X2");
            }
            if (string.IsNullOrEmpty(output)) return "Error!";
            else return output;
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
                    string hashedPassword = hash(passwordInput.Password);
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
