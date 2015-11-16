//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace AdaptiveMediaCache
{
    [DataContract(Name = "ChunkCache")]
     class ChunkCache : IDisposable
    {


        /// <summary>
        /// Gets the chunk's Time value.
        /// </summary>
        [DataMember]
        public UInt64 Time { get; internal set; }

        /// <summary>
        /// Gets the chunk's Duration value.
        /// </summary>
        [DataMember]
        public ulong Duration { get; private set; }

        public byte[] chunkBuffer;
        // 
        //public Windows.Storage.Streams.IInputStream inputStream;

        public uint GetLength()
        {
            
            if (chunkBuffer != null)
                return (uint)chunkBuffer.Length;
            return 0;
        }
        public bool IsChunkDownloaded()
        {
            return ((chunkBuffer != null) && (chunkBuffer.Length>0)) ? true : false;
        }

        public ChunkCache() { }

        public ChunkCache(UInt64 time, ulong duration)
        {
            Time = time;
            Duration = duration;
            chunkBuffer = null;
        }

        public void Dispose()
        {
            chunkBuffer = null;
            Time = 0;
            Duration = 0;
        }
    }
}
