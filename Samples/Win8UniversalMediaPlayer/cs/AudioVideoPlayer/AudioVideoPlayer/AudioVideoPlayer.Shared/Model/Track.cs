using System;
using System.Collections.Generic;
using System.Text;

namespace AudioVideoPlayer.Model
{
    class Track
    {
        string Name;
        UInt32 TrackID;
        UInt32 Duration;
        UInt32 KeyAlbum;
        UInt32 KeyArtist;
        UInt32 KeyMediaFile;

        static public UInt32 Save(Dictionary<UInt32, Track> list, string path)
        {
            return 0;
        }
        static public Dictionary<UInt32, Track> Restore(string path)
        {
            Dictionary<UInt32, Track> list = null;
            return list;
        }

    }
}
