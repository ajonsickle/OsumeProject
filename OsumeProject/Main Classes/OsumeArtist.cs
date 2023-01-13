using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class OsumeArtist : IComparable
    {
        public string spotifyURL { get; set; }
        public string image { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string[] genres {get; set;}
        public OsumeArtist(string spotifyURL, string id, string name, string[] genres, string image)
        {
            this.spotifyURL = spotifyURL;
            this.id = id;
            this.name = name;
            this.genres = genres;
            this.image = image;
        }
        public OsumeArtist(string spotifyURL, string id, string name, string image)
        {
            this.spotifyURL = spotifyURL;
            this.id = id;
            this.name = name;
            this.image = image; 
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
