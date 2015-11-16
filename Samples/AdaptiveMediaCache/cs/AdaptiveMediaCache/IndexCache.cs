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

namespace AdaptiveMediaCache
{
    class IndexCache
    {
        public ulong Time;
        public ulong Offset;
        public UInt32 Size;

        /// <summary>Construct the data by giving the parameters. This is for writing index data to storage</summary>
        public IndexCache(ulong time, ulong offset, UInt32 size)
        {
            Time = time;
            Offset = offset;
            Size = size;
        }

        /// <summary>Construct the data by giving the Byte Array data. This is for reading index data from storage</summary>
        public IndexCache(Byte[] Data)
        {
            Time = BitConverter.ToUInt64(Data, 0);
            Offset = BitConverter.ToUInt64(Data, 8);
            Size = BitConverter.ToUInt32(Data, 16);
        }

        /// <summary> Convert all content index information to Byte Array in order to save it to file</summary>
        /// <returns> Byte array of each chunk index information.</returns>
        public Byte[] GetByteData()
        {
            Byte[] Data = new Byte[IndexCacheSize];
            BitConverter.GetBytes(Time).CopyTo(Data, 0);
            BitConverter.GetBytes(Offset).CopyTo(Data, 8);
            BitConverter.GetBytes(Size).CopyTo(Data, 16);

            return Data;
        }

        public const int IndexCacheSize = 20;
    }
}
