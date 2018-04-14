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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Windows.Data.Json;

namespace VisionClient

{
 

    /// <summary>
    /// class VisionResponse 
    /// </summary>
    /// <info>
    /// Used to parse the JSON string coming from the Vision REST API
    /// Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.
    /// </info>
    public sealed class VisionResponse
    {

        private string HttpError;
        private string displayString;
        public override string ToString()
        {
            if(!string.IsNullOrEmpty(displayString))
                return displayString;
            return string.Empty;
        }

        public VisionResponse()
        {
            HttpError = string.Empty;
            displayString = string.Empty;
        }

        public VisionResponse(string jsonString, string httpError )
        {

            displayString = jsonString;
            if(!string.IsNullOrEmpty(httpError))
            {
                HttpError = httpError;
            }
        }
        public string Result()
        {
            return displayString;
        }

        public string GetHttpError()
        {
            return HttpError;
        }

    }
}
