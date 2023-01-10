using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OsumeProject
{

    public class GetPlaylistResponseTemp
    {
        public bool collaborative { get; set; }
        public string description { get; set; }
        public External_UrlsGetPlaylist external_urls { get; set; }
        public FollowersGetPlaylist followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public Image1GetPlaylist[] images { get; set; }
        public string name { get; set; }
        public OwnerGetPlaylist owner { get; set; }
        public object primary_color { get; set; }
        public bool _public { get; set; }
        public string snapshot_id { get; set; }
        public TracksGetPlaylist tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_UrlsGetPlaylist
    {
        public string spotify { get; set; }
    }

    public class FollowersGetPlaylist
    {
        public object href { get; set; }
        public int total { get; set; }
    }

    public class OwnerGetPlaylist
    {
        public string display_name { get; set; }
        public External_Urls1GetPlaylist external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls1GetPlaylist
    {
        public string spotify { get; set; }
    }

    public class TracksGetPlaylist
    {
        public string href { get; set; }
        public ItemGetPlaylist[] items { get; set; }
        public int limit { get; set; }
        public object next { get; set; }
        public int offset { get; set; }
        public object previous { get; set; }
        public int total { get; set; }
    }

    public class ItemGetPlaylist
    {
        public DateTime added_at { get; set; }
        public Added_ByGetPlaylist added_by { get; set; }
        public bool is_local { get; set; }
        public object primary_color { get; set; }
        public TrackGetPlaylist track { get; set; }
        public Video_ThumbnailGetPlaylist video_thumbnail { get; set; }
    }

    public class Added_ByGetPlaylist
    {
        public External_Urls2GetPlaylist external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls2GetPlaylist
    {
        public string spotify { get; set; }
    }

    public class TrackGetPlaylist
    {
        public AlbumGetPlaylist album { get; set; }
        public Artist1GetPlaylist[] artists { get; set; }
        public string[] available_markets { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        public bool episode { get; set; }
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }
        public External_IdsGetPlaylist external_ids { get; set; }
        public External_Urls5GetPlaylist external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_local { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string preview_url { get; set; }
        public bool track { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class AlbumGetPlaylist
    {
        public string album_type { get; set; }
        public ArtistGetPlaylist[] artists { get; set; }
        public string[] available_markets { get; set; }
        public External_Urls3GetPlaylist external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public ImageGetPlaylist[] images { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public int total_tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls3GetPlaylist
    {
        public string spotify { get; set; }
    }

    public class ArtistGetPlaylist
    {
        public External_Urls4GetPlaylist external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls4GetPlaylist
    {
        public string spotify { get; set; }
    }

    public class ImageGetPlaylist
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    public class External_IdsGetPlaylist
    {
        public string isrc { get; set; }
    }

    public class External_Urls5GetPlaylist
    {
        public string spotify { get; set; }
    }

    public class Artist1GetPlaylist
    {
        public External_Urls6GetPlaylist external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class External_Urls6GetPlaylist
    {
        public string spotify { get; set; }
    }

    public class Video_ThumbnailGetPlaylist
    {
        public object url { get; set; }
    }

    public class Image1GetPlaylist
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

}
