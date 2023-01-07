using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{

    public class TopArtistsResponseTemp
    {
        public ItemTopArtists[] items { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public string href { get; set; }
        public object previous { get; set; }
        public string next { get; set; }
    }

    public class ItemTopArtists
    {
        public External_UrlsTopArtists external_urls { get; set; }
        public FollowersTopArtists followers { get; set; }
        public string[] genres { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public ImageTopArtists[] images { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_UrlsTopArtists
    {
        public string spotify { get; set; }
    }

    public class FollowersTopArtists
    {
        public object href { get; set; }
        public int total { get; set; }
    }

    public class ImageTopArtists
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

}
