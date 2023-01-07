using System;
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
    public static class databaseManager
    {
        public static SQLiteConnection connection = new SQLiteConnection("Data Source=database.db");
        public static bool open = false;

        public static DataTable returnSearchedTable(SQLiteCommand command)
        {
            var reader = command.ExecuteReader();
            DataTable data = new DataTable();
            data.Load(reader);
            return data;
        }
    }
}