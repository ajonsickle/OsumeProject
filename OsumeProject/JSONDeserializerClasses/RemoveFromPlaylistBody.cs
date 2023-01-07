using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class RemoveFromPlaylistBody
    {
        public tracksBody[] tracks;
        public class tracksBody
        {
            public string uri;
            public tracksBody(string uri)
            {
                this.uri = uri;
            }
        }
        public RemoveFromPlaylistBody(tracksBody[] tracks)
        {
            this.tracks = tracks;
        }
    }
}
