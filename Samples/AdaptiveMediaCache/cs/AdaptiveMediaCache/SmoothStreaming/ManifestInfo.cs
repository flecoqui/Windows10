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
    /// Represents the manifest information
    /// </summary>
     class ManifestInfo
    {
        /// <summary>
        /// Initializes a ManifestInfo object with the specified values.
        /// </summary>
        /// <param name="majorVersion">The major manifest version.</param>
        /// <param name="minorVersion">The minor manifest version.</param>
        /// <param name="manifestDuration">The manifest duration.</param>
        /// <param name="streams">The manifest streams.</param>
        public ManifestInfo(int majorVersion, int minorVersion, ulong manifestDuration, IList<StreamInfo> streams, Guid protectionGuid, string protectionData, ulong Timescale)
        {
            this.MajorVersion = majorVersion;
            this.MinorVersion = minorVersion;
            this.ManifestDuration = manifestDuration;
            this.Streams = streams;
            this.ProtectionGuid = protectionGuid;
            this.ProtectionData = protectionData;
            this.Timescale = Timescale;
        }

        /// <summary>
        /// Initializes a ManifestInfo object with the specified values.
        /// </summary>
        /// <param name="majorVersion">The major manifest version.</param>
        /// <param name="minorVersion">The minor manifest version.</param>
        /// <param name="manifestDuration">The manifest duration.</param>
        /// <param name="isLive">Specifies if the manifest is live.</param>
        /// <param name="lookAheadFragmentCount">The look-ahead fragments count.</param>
        /// <param name="dvrWindowLength">The DVR window length.</param>
        /// <param name="streams">The manifest streams.</param>
        public ManifestInfo(int majorVersion, int minorVersion, ulong manifestDuration, bool isLive, int lookAheadFragmentCount, int dvrWindowLength, IList<StreamInfo> streams, Guid protectionGuid, string protectionData, ulong Timescale)
        {
            this.MajorVersion = majorVersion;
            this.MinorVersion = minorVersion;
            this.ManifestDuration = manifestDuration;
            this.IsLive = isLive;
            this.LookAheadFragmentCount = lookAheadFragmentCount;
            this.DvrWindowLength = dvrWindowLength;
            this.Streams = streams;
            this.ProtectionGuid = protectionGuid;
            this.ProtectionData = protectionData;
            this.Timescale = Timescale;

        }

        /// <summary>
        /// Initializes a ManifestInfo object with the specified values.
        /// </summary>
        /// <param name="majorVersion">The major manifest version.</param>
        /// <param name="minorVersion">The minor manifest version.</param>
        protected ManifestInfo(int majorVersion, int minorVersion)
        {
            this.MajorVersion = majorVersion;
            this.MinorVersion = minorVersion;
            this.Streams = new List<StreamInfo>();
        }

        /// <summary>
        /// Initializes a ManifestInfo object with the specified values.
        /// </summary>
        /// <param name="majorVersion">The major manifest version.</param>
        /// <param name="isLive">Specifies if the manifest is live.</param>
        /// <param name="lookAheadFragmentCount">The look-ahead fragments count.</param>
        /// <param name="dvrWindowLength">The DVR window length.</param>
        /// <param name="minorVersion">The minor manifest version.</param>
        protected ManifestInfo(int majorVersion, bool isLive, int lookAheadFragmentCount, int dvrWindowLength, int minorVersion, ulong Timescale )
        {
            this.MajorVersion = majorVersion;
            this.MinorVersion = minorVersion;
            this.IsLive = isLive;
            this.LookAheadFragmentCount = lookAheadFragmentCount;
            this.DvrWindowLength = dvrWindowLength;
            this.Timescale = Timescale;
            this.Streams = new List<StreamInfo>();
        }

        /// <summary>
        /// Gets the major manifest version.
        /// </summary>
        public int MajorVersion { get; protected set; }

        /// <summary>
        /// Gets the minor manifest version.
        /// </summary>
        public int MinorVersion { get; protected set; }

        /// <summary>
        /// Gets the duration of the manifest.
        /// </summary>
        /// <value>The duration of the manifest.</value>
        public virtual ulong ManifestDuration { get; private set; }

        /// <summary>
        /// Gets the TimeScale of the manifest.
        /// </summary>
        /// <value>The TimeScale of the manifest.</value>
        public virtual ulong Timescale { get; private set; }
        /// <summary>
        /// Gets whether the manifest is live.
        /// </summary>
        public bool IsLive { get; protected set; }

        /// <summary>
        /// Gets the look-ahead fragments count.
        /// </summary>
        public int LookAheadFragmentCount { get; protected set; }

        /// <summary>
        /// Get the DVR window length.
        /// </summary>
        public int DvrWindowLength { get; protected set; }

        /// <summary>
        /// Gets the list of streams from the manifest.
        /// </summary>
        public IList<StreamInfo> Streams { get; private set; }

        /// <summary>
        /// Get Protection Guid.
        /// </summary>
        public Guid ProtectionGuid { get; protected set; }

        /// <summary>
        /// Get Protection Data.
        /// </summary>
        public string ProtectionData { get; protected set; }
    }
}
