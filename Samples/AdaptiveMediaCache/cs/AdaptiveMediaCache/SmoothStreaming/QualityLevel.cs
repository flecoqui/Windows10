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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a quality level.
    /// </summary>
     class QualityLevel
    {
        /// <summary>
        /// Initializes a QualityLevel object.
        /// </summary>
        public QualityLevel()
        {
            this.Attributes = new Dictionary<string, string>();
            this.CustomAttributes = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the QualityLevel attributes.
        /// </summary>
        public IDictionary<string, string> Attributes { get; private set; }

        /// <summary>
        /// Gets the QualityLevel custom attributes.
        /// </summary>
        public IDictionary<string, string> CustomAttributes { get; private set; }

        /// <summary>
        /// Adds an attribute to the quality level.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public void AddAttribute(string attribute, string value)
        {
            this.Attributes.Add(attribute, value);
        }

        /// <summary>
        /// Adds a custom attribute to the quality level.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public void AddCustomAttribute(string attribute, string value)
        {
            this.CustomAttributes.Add(attribute, value);
        }

        /// <summary>
        /// Creates a clone instance of this QualityLevel object.
        /// </summary>
        /// <returns>A copy of the QualityLevel object.</returns>
        public QualityLevel Clone()
        {
            QualityLevel clone = new QualityLevel();

            foreach (KeyValuePair<string, string> attributeValuePair in this.Attributes)
            {
                clone.AddAttribute(attributeValuePair.Key, attributeValuePair.Value);
            }

            foreach (KeyValuePair<string, string> customAttributeValuePair in this.CustomAttributes)
            {
                clone.AddCustomAttribute(customAttributeValuePair.Key, customAttributeValuePair.Value);
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
            if(this.Attributes.TryGetValue(attribute, out result))
            {
                return uint.TryParse(result,out value);
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