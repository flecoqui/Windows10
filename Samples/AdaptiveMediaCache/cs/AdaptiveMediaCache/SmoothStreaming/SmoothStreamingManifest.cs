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
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using SmoothStreaming;
    /// <summary>
    /// Parses a Smooth Streaming Manifest.
    /// </summary>
     class SmoothStreamingManifest    {
        // tick = hundred nano second = 10^7
        public const int TicksPerSecond = 10000000;

        /// <summary>
        /// Defines the Manifest SmoothStreamingMedia element.
        /// </summary>
        private const string ManifestSmoothStreamingMediaElement = "SmoothStreamingMedia";

        /// <summary>
        /// Defines the Manifest MajorVersion attribute.
        /// </summary>
        private const string ManifestMajorVersionAttribute = "MajorVersion";

        /// <summary>
        /// Defines the Manifest MinorVersion attribute.
        /// </summary>
        private const string ManifestMinorVersionAttribute = "MinorVersion";

        /// <summary>
        /// Defines the Manifest Duration attribute.
        /// </summary>
        private const string ManifestDurationAttribute = "Duration";

        /// <summary>
        /// Defines the Manifest Duration attribute.
        /// </summary>
        private const string TimeScaleAttribute = "TimeScale";

        /// <summary>
        /// Defines the Manifest IsLive attribute.
        /// </summary>
        private const string ManifestIsLiveAttribute = "IsLive";

        /// <summary>
        /// Defines the Manifest LookAheadFragmentCount attribute.
        /// </summary>
        private const string ManifestLookAheadFragmentCountAttribute = "LookAheadFragmentCount";

        /// <summary>
        /// Defines the Manifest DVRWindowLength attribute.
        /// </summary>
        private const string ManifestDvrWindowLengthAttribute = "DVRWindowLength";

        /// <summary>
        /// Defines the Manifest StreamIndex element.
        /// </summary>
        private const string ManifestStreamIndexElement = "StreamIndex";
        /// <summary>
        /// Defines the Manifest Protection element.
        /// </summary>
        private const string ManifestProtectionElement = "Protection";
        /// <summary>
        /// Defines the Manifest Protection element.
        /// </summary>
        private const string ManifestProtectionHeaderElement = "ProtectionHeader";
        /// <summary>
        /// Defines the Manifest Protection SystemIDelement.
        /// </summary>
        private const string ManifestProtectionSystemIDElement = "SystemID";

        /// <summary>
        /// Defines the Manifest StreamIndex Type attribute.
        /// </summary>
        private const string ManifestStreamIndexTypeAttribute = "Type";

        /// <summary>
        /// Initializes a new instance of the <see cref="SmoothStreamingManifestParser"/> class.
        /// </summary>
        /// <param name="manifestStream">The stream of the manifest being parsed.</param>
        public SmoothStreamingManifest(Stream manifestStream)
        {
            this.ParseManifest(manifestStream);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SmoothStreamingManifestParser"/> class.
        /// </summary>
        /// <param name="manifestStream">The buffer of the manifest being parsed.</param>
        public SmoothStreamingManifest(byte[] manifestBuffer)
        {
            using (var manifestStream = new MemoryStream(manifestBuffer))
            {
                this.ParseManifest(manifestStream);
            }
        }

        /// <summary>
        /// Gets the <see cref="ManifestInfo"/> of the parsed stream.
        /// </summary>
        /// <value>The manifest information containing all the need it information.</value>
        public ManifestInfo ManifestInfo { get; private set; }

        /// <summary>
        /// Adds attributes to the stream info.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <param name="streamInfo">The stream info.</param>
        private static void AddAttributes(XmlReader reader, StreamInfo streamInfo)
        {
            if (reader.HasAttributes && reader.MoveToFirstAttribute())
            {
                do
                {
                    streamInfo.AddAttribute(reader.Name, reader.Value);
                }
                while (reader.MoveToNextAttribute());
                reader.MoveToFirstAttribute();
            }
        }

        /// <summary>
        /// Adds attributes to the quality level.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <param name="qualityLevel">The quality level.</param>
        private static void AddAttributes(XmlReader reader, QualityLevel qualityLevel)
        {
            if (reader.HasAttributes && reader.MoveToFirstAttribute())
            {
                do
                {
                    qualityLevel.AddAttribute(reader.Name, reader.Value);
                }
                while (reader.MoveToNextAttribute());
                reader.MoveToElement();
            }
        }

        /// <summary>
        /// Adds custom attributes to the quality level.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <param name="qualityLevel">The quality level.</param>
        private static void AddCustomAttributes(XmlReader reader, QualityLevel qualityLevel)
        {
            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    if (reader.Name == "CustomAttributes" && reader.NodeType == XmlNodeType.Element)
                    {
                        while (reader.Read())
                        {
                            if ((reader.Name == "Attribute") && (reader.NodeType == XmlNodeType.Element))
                            {
                                string attribute = reader.GetAttribute("Name");

                                if (!string.IsNullOrEmpty(attribute))
                                {
                                    qualityLevel.AddCustomAttribute(attribute, reader.GetAttribute("Value"));
                                }
                            }

                            if ((reader.Name == "CustomAttributes") && (reader.NodeType == XmlNodeType.EndElement))
                            {
                                return;
                            }
                        }
                    }

                    if (reader.Name == "QualityLevel" && reader.NodeType == XmlNodeType.EndElement)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Parses the manifest stream.
        /// </summary>
        /// <param name="manifestStream">The manifest stream being parsed.</param>
        private void ParseManifest(Stream manifestStream)
        {
            
            using (XmlReader reader = XmlReader.Create(manifestStream))
            {
                if (reader.Read() && reader.IsStartElement(ManifestSmoothStreamingMediaElement))
                {
                    
                    int majorVersion = reader.GetValueAsInt(ManifestMajorVersionAttribute).GetValueOrDefault();
                    int minorVersion = reader.GetValueAsInt(ManifestMinorVersionAttribute).GetValueOrDefault();
                    bool isLive = reader.GetValueAsBool(ManifestIsLiveAttribute).GetValueOrDefault();

                    int lookAheadFragmentCount = 0;
                    int dvrWindowLength = 0;

                    if (isLive)
                    {
                        lookAheadFragmentCount = reader.GetValueAsInt(ManifestLookAheadFragmentCountAttribute).GetValueOrDefault();
                        dvrWindowLength = reader.GetValueAsInt(ManifestDvrWindowLengthAttribute).GetValueOrDefault();
                    }
                    ulong Timescale = reader.GetValueAsULong(TimeScaleAttribute).GetValueOrDefault(TicksPerSecond);
                    ulong manifestDuration = reader.GetValueAsULong(ManifestDurationAttribute).GetValueOrDefault();
                    Guid protectionGuid = Guid.Empty;
                    string protectionData = string.Empty;

                    List<StreamInfo> streams = new List<StreamInfo>();

                    while (reader.Read())
                    {
                        if (reader.Name == ManifestProtectionElement && reader.NodeType == XmlNodeType.Element)
                        {
                            reader.Read();
                            if (reader.Name == ManifestProtectionHeaderElement && reader.NodeType == XmlNodeType.Element)
                            {
                                protectionGuid = new Guid(reader.GetValue(ManifestProtectionSystemIDElement));
                                protectionData = reader.ReadElementContentAsString();

                            }

                        }
                        if (reader.Name == ManifestStreamIndexElement && reader.NodeType == XmlNodeType.Element)
                        {
                            string type = reader.GetValue(ManifestStreamIndexTypeAttribute);

                            StreamInfo streamInfo = new StreamInfo(type);

                            AddAttributes(reader, streamInfo);

                            while (reader.Read())
                            {
                                if (reader.Name == ManifestStreamIndexElement && reader.NodeType == XmlNodeType.EndElement)
                                {
                                    break;
                                }

                                if ((reader.Name == "QualityLevel") && (reader.NodeType == XmlNodeType.Element))
                                {
                                    QualityLevel qualityLevel = new QualityLevel();

                                    AddAttributes(reader, qualityLevel);

                                    AddCustomAttributes(reader, qualityLevel);

                                    streamInfo.QualityLevels.Add(qualityLevel);
                                }

                                if ((reader.Name == "c") && (reader.NodeType == XmlNodeType.Element))
                                {
                                    int? chunkId = reader.GetValueAsInt("n");
                                    UInt64? time = reader.GetValueAsUInt64("t");
                                    ulong? duration = reader.GetValueAsULong("d");
                                    ulong? repetitions = reader.GetValueAsULong("r");

                                    for (ulong i = 0; i < (repetitions.HasValue ? repetitions.Value : 1); i++)
                                    {
                                        Chunk chunk = new Chunk(chunkId, i == 0 ? time : null, duration);

                                        if (((!reader.IsEmptyElement && reader.Read()) && (reader.IsStartElement("f") && reader.Read())) && (reader.NodeType == XmlNodeType.Text))
                                        {
                                            chunk.Value = reader.Value;
                                        }

                                        streamInfo.Chunks.Add(chunk);
                                    }
                                }
                            }

                            streams.Add(streamInfo);
                        }
                    }

                    streams.ToArray();

                    if (!isLive)
                    {
                        this.ManifestInfo = new ManifestInfo(majorVersion, minorVersion, manifestDuration, streams, protectionGuid, protectionData,Timescale);
                    }
                    else
                    {
                        this.ManifestInfo = new ManifestInfo(majorVersion, minorVersion, manifestDuration, isLive, lookAheadFragmentCount, dvrWindowLength, streams, protectionGuid, protectionData,Timescale);
                    }
                }
            }
        }
    }
}
