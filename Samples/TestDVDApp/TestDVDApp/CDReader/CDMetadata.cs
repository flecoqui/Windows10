using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDVDApp.CDReader
{
    public class CDMetadata
    {
        // Type = 0
        public string AlbumTitle { get; set; }
        // Type = 1
        public string Artist { get; set; }
        // Type = 5
        public string Message { get; set; }
        // Type = 7
        public string Genre { get; set; }
        // Type = 6
        public string DiscID { get; set; }
        // Type = 14
        public string ISrc { get; set; }

        public List<CDTrackMetadata> Tracks { get; set; }

        public CDMetadata()
        {
            Tracks = new List<CDTrackMetadata>();
        }
    }

}
