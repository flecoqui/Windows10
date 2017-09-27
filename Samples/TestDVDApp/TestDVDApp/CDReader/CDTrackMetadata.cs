using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDVDApp.CDReader
{
    public class CDTrackMetadata
    {
        public int Number { get; set; }
        // Type = 0
        public string Title { get; set; }
        // Type = 14
        public string Poster { get; set; }
        public string ISrc { get; set; }
        public TimeSpan Duration { get; set; }
        public int FirstSector { get; set; }
        public int LastSector { get; set; }

        public override string ToString()
        {
            return string.Format("Track: {0} Title: {1} ID: {2} Duration: {3}", Number, string.IsNullOrEmpty(Title) ? "Unknown" : Title, string.IsNullOrEmpty(ISrc) ? "Unknown" : ISrc, Duration);
        }

    }
}
