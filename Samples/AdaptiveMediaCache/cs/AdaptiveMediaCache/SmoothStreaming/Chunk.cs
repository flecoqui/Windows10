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

namespace AdaptiveMediaCache.SmoothStreaming
{
    /// <summary>
    /// Represents a chunk.
    /// </summary>
     class Chunk
    {
        /// <summary>
        /// Initializes a Chunk with the specified values.
        /// </summary>
        /// <param name="id">Chunk number.</param>
        /// <param name="time">Chunk timestamp.</param>
        /// <param name="duration">Chunk duration.</param>
        public Chunk(int? id, UInt64? time, ulong? duration)
        {
            this.Id = id;
            this.Time = time;
            this.Duration = duration;
            this.Repeat = 1;
        }

        /// <summary>
        /// Initializes a Chunk with the specified values.
        /// </summary>
        /// <param name="id">Chunk number.</param>
        /// <param name="time">Chunk timestamp.</param>
        /// <param name="duration">Chunk duration.</param>
        /// <param name="duration">Chunk value.</param>
        public Chunk(int? id, UInt64? time, ulong? duration, string value)
            : this(id, time, duration)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the chunk's Id.
        /// </summary>
        public int? Id { get; private set; }

        /// <summary>
        /// Gets the chunk's Time value.
        /// </summary>
        public UInt64? Time { get; internal set; }

        /// <summary>
        /// Gets the chunk's Duration value.
        /// </summary>
        public ulong? Duration { get; private set; }

        /// <summary>
        /// Gets or sets the chunk's Value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the chunk's Repeat value.
        /// </summary>
        public ulong Repeat { get; set; }
    }
}