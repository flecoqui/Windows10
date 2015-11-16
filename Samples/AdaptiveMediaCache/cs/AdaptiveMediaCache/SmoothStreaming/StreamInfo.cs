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
    /// Represents the information of a Stream in a manifest.
    /// </summary>
     class StreamInfo
    {
        /// <summary>
        /// Initializes a StreamInfo object with the specified type.
        /// </summary>
        /// <param name="type">The stream type.</param>
        public StreamInfo(string type)
        {
            this.Attributes = new Dictionary<string, string>();
            this.QualityLevels = new List<QualityLevel>();
            this.Chunks = new List<Chunk>();
            this.StreamType = type;
        }

        /// <summary>
        /// Gets the stream type.
        /// </summary>
        public string StreamType { get; private set; }

        /// <summary>
        /// Gets the list of quality levels.
        /// </summary>
        public IList<QualityLevel> QualityLevels { get; internal set; }

        /// <summary>
        /// Gets the list of chunks in the stream.
        /// </summary>
        public IList<Chunk> Chunks { get; private set; }

        /// <summary>
        /// Gets a dictionary with the stream attributes.
        /// </summary>
        public IDictionary<string, string> Attributes { get; private set; }

        /// <summary>
        /// Gets whether the stream is sparse (not video or audio).
        /// </summary>
        public bool IsSparseStream
        {
            get { return this.StreamType.ToUpper() != "VIDEO" && this.StreamType.ToUpper() != "AUDIO"; }
        }

        /// <summary>
        /// Gets the parent clip.
        /// </summary>
        public Clip ParentClip { get; set; }

        /// <summary>
        /// Adds an attribute.
        /// </summary>
        /// <param name="key">The attribute key.</param>
        /// <param name="value">The attribute value.</param>
        public void AddAttribute(string key, string value)
        {
            this.Attributes.Add(key, value);
        }

        /// <summary>
        /// Creates a clone instance of this StreamInfo object.
        /// </summary>
        /// <returns>A copy of the StreamInfo object.</returns>
        public StreamInfo Clone()
        {
            StreamInfo clone = new StreamInfo(this.StreamType);

            clone.ParentClip = this.ParentClip;

            foreach (QualityLevel qualityLevel in this.QualityLevels)
            {
                clone.QualityLevels.Add(qualityLevel.Clone());
            }

            foreach (Chunk chunk in this.Chunks)
            {
                clone.Chunks.Add(chunk);
            }

            foreach (KeyValuePair<string, string> attributeValuePair in this.Attributes)
            {
                clone.AddAttribute(attributeValuePair.Key, attributeValuePair.Value);
            }

            return clone;
        }
        public bool TryGetAttributeValueAsString(string attribute, out string value)
        {
            return this.Attributes.TryGetValue(attribute, out value);
        }
        public bool TryGetAttributeValueAsUint(string attribute, out uint value)
        {
            string result;
            value = 0;
            if (this.Attributes.TryGetValue(attribute, out result))
            {
                return uint.TryParse(result, out value);
            }
            return false;
        }
        public bool TryGetAttributeValueAsUlong(string attribute, out ulong value)
        {
            string result;
            value = 0;
            if (this.Attributes.TryGetValue(attribute, out result))
            {
                return ulong.TryParse(result, out value);
            }
            return false;
        }
    }
}
