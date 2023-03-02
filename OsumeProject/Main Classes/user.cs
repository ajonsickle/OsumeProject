using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OsumeProject
{
    public class user
    {

        public string accessToken { get; set; }
        public string username { get; set; }
        public string currentToken { get; set; }
        public string pfpURL { get; set; }
        public string userID { get; set; }
        public string playlistID { get; set; }
        public bool admin = false;

        public user()
        {
            
        }

    }
}