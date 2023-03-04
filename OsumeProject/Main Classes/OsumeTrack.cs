using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class OsumeTrack : IComparable
    {
        public OsumeArtist[] artists { get; set; }
        public string spotifyURL { get; set; }  
        public string id { get; set; }
        public string previewURL { get; set; }
        public string name { get; set; }
        public bool isExplicit { get; set; }
        public Dictionary<string, double> features { get; set; }
        public OsumeAlbum album { get; set; }

        public OsumeTrack(OsumeArtist[] artists, string spotifyURL, string id, string previewURL, string name, bool isExplicit, Dictionary<string, double> features, OsumeAlbum album)
        {
            this.artists = artists;
            this.spotifyURL = spotifyURL;
            this.id = id;
            this.previewURL = previewURL;
            this.name = name;
            this.isExplicit = isExplicit;
            this.album = album;
            this.features = features;
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
