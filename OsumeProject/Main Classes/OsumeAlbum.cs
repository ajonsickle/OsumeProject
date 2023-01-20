using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class OsumeAlbum : IComparable
    {
        public Dictionary<int, string> coverImages;
        public OsumeArtist[] artists { get; set; }
        public string spotifyURL { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }

        public OsumeAlbum(OsumeArtist[] artists, string spotifyURL, string id, string name, string release_date, Dictionary<int, string> coverImages)
        {
            this.artists = artists;
            this.spotifyURL = spotifyURL;
            this.id = id;
            this.name = name;
            this.coverImages = coverImages;
            this.release_date = release_date;
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
