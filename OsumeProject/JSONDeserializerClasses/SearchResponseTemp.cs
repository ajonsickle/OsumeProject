using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
        public class SearchResponseTemp
        {
            public Artists artists { get; set; }
        }

        public class Artists
        {
            public string href { get; set; }
            public ItemSearchResponse[] items { get; set; }
            public int limit { get; set; }
            public string next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
        }

        public class ItemSearchResponse
        {
            public External_UrlsSearchResponse external_urls { get; set; }
            public FollowersSearchResponse followers { get; set; }
            public string[] genres { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public ImageSearchResponse[] images { get; set; }
            public string name { get; set; }
            public int popularity { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }

        public class External_UrlsSearchResponse
        {
            public string spotify { get; set; }
        }

        public class FollowersSearchResponse
    {
            public object href { get; set; }
            public int total { get; set; }
        }

        public class ImageSearchResponse
    {
            public int height { get; set; }
            public string url { get; set; }
            public int width { get; set; }
        }
}
