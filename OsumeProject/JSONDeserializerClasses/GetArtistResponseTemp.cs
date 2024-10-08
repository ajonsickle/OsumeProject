﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{

    public class GetArtistResponseTemp
    {
        public External_Urls_Artist external_urls { get; set; }
        public Followers followers { get; set; }
        public string[] genres { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public Image_Here[] images { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls_Artist
    {
        public string spotify { get; set; }
    }

    public class Followers
    {
        public object href { get; set; }
        public int total { get; set; }
    }

    public class Image_Here
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

}
