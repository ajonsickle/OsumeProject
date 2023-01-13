using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class OsumeAlbum 
    {
        public class OsumeAlbumArt
        {
            public Dictionary<int, string> images = new Dictionary<int, string>();
            public OsumeAlbumArt(Dictionary<int, string> images)
            {
                this.images = images;
            }
            public Dictionary<int, string> getImages()
            {
                return this.images;
            }
        };
        public OsumeArtist[] artists { get; set; }
        public string spotifyURL { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public OsumeAlbumArt cover { get; set; }

        public OsumeAlbum(OsumeArtist[] artists, string spotifyURL, string id, string name, string release_date, OsumeAlbumArt cover)
        {
            this.artists = artists;
            this.spotifyURL = spotifyURL;
            this.id = id;
            this.name = name;
            this.cover = cover;
            this.release_date = release_date;
        }
    }
}
