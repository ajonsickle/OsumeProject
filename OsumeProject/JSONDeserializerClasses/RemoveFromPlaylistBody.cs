using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class RemoveFromPlaylistBody
    {
        public tracksBody[] tracks;
        public class tracksBody : IComparable
        {
            public string uri;
            public tracksBody(string uri)
            {
                this.uri = uri;
            }

            public int CompareTo(object obj)
            {
                throw new NotImplementedException();
            }
        }
        public RemoveFromPlaylistBody(tracksBody[] tracks)
        {
            this.tracks = tracks;
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
