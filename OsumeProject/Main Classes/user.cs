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

        public string accessToken;
        public string username;
        public string currentToken;
        public string pfpURL;
        public string userID;
        public string playlistID;
        public bool admin = false;

        public user()
        {
            
        }

    }
}