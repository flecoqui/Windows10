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
    /// Represents a clip.
    /// </summary>
     class Clip
    {
        /// <summary>
        /// Initializes a Clip object with the specified values.
        /// </summary>
        /// <param name="url">The clip's URL.</param>
        /// <param name="clipBegin">The clip begin position.</param>
        /// <param name="clipEnd">The clip end position.</param>
        /// <param name="isGap">Specifies if the clip is a empty gap.</param>
        public Clip(Uri url, ulong clipBegin, ulong clipEnd, bool isGap)
            : this(url, clipBegin, clipEnd, isGap, new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Initializes a Clip object with the specified values.
        /// </summary>
        /// <param name="url">The clip's URL.</param>
        /// <param name="clipBegin">The clip begin position.</param>
        /// <param name="clipEnd">The clip end position.</param>
        /// <param name="isGap">Specifies if the clip is a empty gap.</param>
        /// <param name="customAttributes">Specifies additional custom attributes.</param>
        public Clip(Uri url, ulong clipBegin, ulong clipEnd, bool isGap, IDictionary<string, string> customAttributes)
            : this(url, clipBegin, clipEnd, isGap, customAttributes, new List<StreamInfo>())
        {
        }

        /// <summary>
        /// Initializes a Clip object with the specified values.
        /// </summary>
        /// <param name="url">The clip's URL.</param>
        /// <param name="clipBegin">The clip begin position.</param>
        /// <param name="clipEnd">The clip end position.</param>
        /// <param name="isGap">Specifies if the clip is a empty gap.</param>
        /// <param name="customAttributes">Specifies additional custom attributes.</param>
        /// <param name="streams">Specifies a collection of StreamInfo objects of the clip.</param>
        public Clip(Uri url, ulong clipBegin, ulong clipEnd, bool isGap, IDictionary<string, string> customAttributes, IList<StreamInfo> streams)
        {
            this.Url = url;
            this.ClipBegin = clipBegin;
            this.ClipEnd = clipEnd;
            this.IsGap = isGap;
            this.Streams = streams;

            this.CustomAttributes = customAttributes ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the Url value.
        /// </summary>
        public Uri Url { get; private set; }

        /// <summary>
        /// Gets or sets the clip begin position.
        /// </summary>
        public ulong ClipBegin { get; set; }

        /// <summary>
        /// Gets or sets the clip end position.
        /// </summary>
        public ulong ClipEnd { get; set; }

        /// <summary>
        /// Gets the clip streams.
        /// </summary>
        public IList<StreamInfo> Streams { get; private set; }

        /// <summary>
        /// Gets the clip custom attributes.
        /// </summary>
        public IDictionary<string, string> CustomAttributes { get; private set; }

        /// <summary>
        /// Gets or sets whether the clip is a gap.
        /// </summary>
        public bool IsGap { get; set; }

        /// <summary>
        /// Gets the clip duration.
        /// </summary>
        public ulong Duration
        {
            get { return this.ClipEnd - this.ClipBegin; }
        }
    }
}

