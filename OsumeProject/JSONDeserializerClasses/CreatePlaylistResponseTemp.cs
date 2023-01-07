using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{

    public class CreatePlaylistResponseTemp
    {
        public bool collaborative { get; set; }
        public string description { get; set; }
        public External_UrlsCreatePlaylist external_urls { get; set; }
        public FollowersCreatePlaylist followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public object[] images { get; set; }
        public string name { get; set; }
        public OwnerCreatePlaylist owner { get; set; }
        public object primary_color { get; set; }
        public bool _public { get; set; }
        public string snapshot_id { get; set; }
        public TracksCreatePlaylist tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_UrlsCreatePlaylist
    {
        public string spotify { get; set; }
    }

    public class FollowersCreatePlaylist
    {
        public object href { get; set; }
        public int total { get; set; }
    }

    public class OwnerCreatePlaylist
    {
        public string display_name { get; set; }
        public External_Urls1CreatePlaylist external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls1CreatePlaylist
    {
        public string spotify { get; set; }
    }

    public class TracksCreatePlaylist
    {
        public string href { get; set; }
        public object[] items { get; set; }
        public int limit { get; set; }
        public object next { get; set; }
        public int offset { get; set; }
        public object previous { get; set; }
        public int total { get; set; }
    }

}
