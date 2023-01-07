using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{

    public class GetProfileResponseTemp
    {
        public string country { get; set; }
        public string display_name { get; set; }
        public string email { get; set; }
        public Explicit_ContentGetProfile explicit_content { get; set; }
        public External_UrlsGetProfile external_urls { get; set; }
        public FollowersGetProfile followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public ImageGetProfile[] images { get; set; }
        public string product { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class Explicit_ContentGetProfile
    {
        public bool filter_enabled { get; set; }
        public bool filter_locked { get; set; }
    }

    public class External_UrlsGetProfile
    {
        public string spotify { get; set; }
    }

    public class FollowersGetProfile
    {
        public object href { get; set; }
        public int total { get; set; }
    }

    public class ImageGetProfile
    {
        public object height { get; set; }
        public string url { get; set; }
        public object width { get; set; }
    }

}
