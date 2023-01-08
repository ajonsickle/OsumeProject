using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{

    public class GetTrackResponseTemp
    {
        public AlbumGetTrack album { get; set; }
        public Artist1GetTrack[] artists { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        [JsonProperty("explicit")]
        public bool Explicit { get; set; }
        public External_IdsGetTrack external_ids { get; set; }
        public External_Urls2GetTrack external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_local { get; set; }
        public bool is_playable { get; set; }
        public Linked_FromGetTrack linked_from { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string preview_url { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class AlbumGetTrack
    {
        public string album_type { get; set; }
        public ArtistGetTrack[] artists { get; set; }
        public External_UrlsGetTrack external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public ImageGetTrack[] images { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public int total_tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_UrlsGetTrack
    {
        public string spotify { get; set; }
    }

    public class ArtistGetTrack
    {
        public External_Urls1 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls1GetTrack
    {
        public string spotify { get; set; }
    }

    public class ImageGetTrack
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    public class External_IdsGetTrack
    {
        public string isrc { get; set; }
    }

    public class External_Urls2GetTrack
    {
        public string spotify { get; set; }
    }

    public class Linked_FromGetTrack
    {
        public External_Urls3GetTrack external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls3GetTrack
    {
        public string spotify { get; set; }
    }

    public class Artist1GetTrack
    {
        public External_Urls4GetTrack external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls4GetTrack
    {
        public string spotify { get; set; }
    }

}
