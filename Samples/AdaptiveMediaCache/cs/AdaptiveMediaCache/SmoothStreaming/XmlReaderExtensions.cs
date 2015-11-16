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
using System.Xml;

namespace AdaptiveMediaCache.SmoothStreaming
{
    /// <summary>
    /// XmlReader extension methods.
    /// </summary>
     static class XmlReaderExtensions
    {
        /// <summary>
        /// Gets the value of the specified attribute as string.
        /// </summary>
        /// <param name="reader">The XmlReader object.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute's value.</returns>
        public static string GetValue(this XmlReader reader, string name)
        {
            return reader.GetAttribute(name);
        }

        /// <summary>
        /// Gets the value of the specified attribute as boolean.
        /// </summary>
        /// <param name="reader">The XmlReader object.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute's value.</returns>
        public static bool? GetValueAsBool(this XmlReader reader, string name)
        {
            bool result;

            if (bool.TryParse(GetValue(reader, name), out result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Gets the value of the specified attribute as integer.
        /// </summary>
        /// <param name="reader">The XmlReader object.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute's value.</returns>
        public static int? GetValueAsInt(this XmlReader reader, string name)
        {
            int result;

            if (int.TryParse(GetValue(reader, name), out result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Gets the value of the specified attribute as long.
        /// </summary>
        /// <param name="reader">The XmlReader object.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute's value.</returns>
        public static long GetValueAsLong(this XmlReader reader, string name)
        {
            long result;

            if (long.TryParse(GetValue(reader, name), out result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Gets the value of the specified attribute as long.
        /// </summary>
        /// <param name="reader">The XmlReader object.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute's value.</returns>
        public static ulong? GetValueAsULong(this XmlReader reader, string name)
        {
            ulong result;

            if (ulong.TryParse(GetValue(reader, name), out result))
            {
                return result;
            }

            return null;
        }
        /// <summary>
        /// Gets the value of the specified attribute as Uint64long.
        /// </summary>
        /// <param name="reader">The XmlReader object.</param>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute's value.</returns>
        public static UInt64? GetValueAsUInt64(this XmlReader reader, string name)
        {
            UInt64 result;

            if (UInt64.TryParse(GetValue(reader, name), out result))
            {
                return result;
            }

            return null;
        }
    }
}

