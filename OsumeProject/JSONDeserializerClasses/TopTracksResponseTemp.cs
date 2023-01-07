using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{



    public class TopTracksResponseTemp
    {
        public ItemTopTracks[] items { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public string href { get; set; }
        public object previous { get; set; }
        public object next { get; set; }
    }

    public class ItemTopTracks
    {
        public AlbumTopTracks album { get; set; }
        public Artist1TopTracks[] artists { get; set; }
        public string[] available_markets { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        public bool _explicit { get; set; }
        public External_IdsTopTracks external_ids { get; set; }
        public External_Urls2TopTracks external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_local { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string preview_url { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class AlbumTopTracks
    {
        public string album_type { get; set; }
        public ArtistTopTracks[] artists { get; set; }
        public string[] available_markets { get; set; }
        public External_UrlsTopTracks external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public ImageTopTracks[] images { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public int total_tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_UrlsTopTracks
    {
        public string spotify { get; set; }
    }

    public class ArtistTopTracks
    {
        public External_Urls1TopTracks external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls1TopTracks
    {
        public string spotify { get; set; }
    }

    public class ImageTopTracks
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    public class External_IdsTopTracks
    {
        public string isrc { get; set; }
    }

    public class External_Urls2TopTracks
    {
        public string spotify { get; set; }
    }

    public class Artist1TopTracks
    {
        public External_Urls3TopTracks external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls3TopTracks
    {
        public string spotify { get; set; }
    }


}
