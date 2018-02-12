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

namespace SpeechToTextClient

{
    /// <summary>
    /// class SpeechToTextConfidence 
    /// </summary>
    /// <info>
    /// Used to parse the JSON string coming from the SpeechToText REST API
    /// Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.
    /// </info>

    class SpeechToTextConfidence
    {
        private const string HIGHCONFKey = "HIGHCONF";
        private const string MIDCONFKey = "MIDCONF";
        private const string LOWCONFKey = "LOWCONF";
        private const string requestidKey = "requestid";

        private int HIGHCONF;
        private int MIDCONF;
        private int LOWCONF;
        private string requestid;
        public SpeechToTextConfidence()
        {
            HIGHCONF = 0;
            MIDCONF = 0;
            LOWCONF = 0;
            requestid = string.Empty;
        }
        public SpeechToTextConfidence(JsonObject jsonObject)
        {
            HIGHCONF = 0;
            MIDCONF = 0;
            LOWCONF = 0;
            requestid = string.Empty;
            HIGHCONF = (jsonObject.GetNamedString(HIGHCONFKey,"0") == "1" ? 1 : 0);
            MIDCONF = (jsonObject.GetNamedString(MIDCONFKey,"0") == "1" ? 1 : 0);
            LOWCONF = (jsonObject.GetNamedString(LOWCONFKey,"0") == "1" ? 1 : 0);
            requestid = jsonObject.GetNamedString(requestidKey, "") ;
        }
        public bool IsHIGHCONF() { return (HIGHCONF > 0 ? true : false); }
        public bool IsMIDCONF() { return (MIDCONF > 0 ? true : false); }
        public bool IsLOWCONF() { return (LOWCONF > 0 ? true : false); }
    }
    /// <summary>
    /// class SpeechToTextResult 
    /// </summary>
    /// <info>
    /// Used to parse the JSON string coming from the SpeechToText REST API
    /// Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.
    /// </info>
    class SpeechToTextResult
    {
        private const string confidenceKey = "Confidence";
        private const string lexicalKey = "Lexical";
        private const string ITNKey = "ITN";
        private const string displayKey = "Display";
        private const string maskedITNKey = "MaskedITN";

        private double confidence;
        private string lexical;
        private string ITN;
        private string maskedITN;
        private string display;

        public SpeechToTextResult()
        {
            confidence = 0;
            lexical = string.Empty;
            ITN = string.Empty;
            maskedITN = string.Empty;
            display = string.Empty;
        }

        public SpeechToTextResult(JsonObject jsonObject)
        {
            confidence = jsonObject.GetNamedNumber(confidenceKey, 0);
            lexical = jsonObject.GetNamedString(lexicalKey, "");
            ITN = jsonObject.GetNamedString(ITNKey, "");
            maskedITN = jsonObject.GetNamedString(maskedITNKey, "");
            display = jsonObject.GetNamedString(displayKey, "");
        }
        public string GetLexical() { return lexical; }
        public double GetConfidence() { return confidence; }
        public string GetDisplay() { return display; }
    }
    /// <summary>
    /// class SpeechToTextHeader 
    /// </summary>
    /// <info>
    /// Used to parse the JSON string coming from the SpeechToText REST API
    /// Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.
    /// </info>
    class SpeechToTextHeader
    {
        //{
        //\"scenario\":\"catsearch\",
        //\"name\":\"bing what's the weather like\",
        //\"lexical\":\"bing what's the weather like\",
        //\"confidence\":\"0.879686\",
        //\"properties\":{\"HIGHCONF\":\"1\"}
        //}
        //\"header\":{\"status\":\"success\",
        //            \"scenario\":\"catsearch\",
        //            \"name\":\"bing what's the weather like\",
        //            \"lexical\":\"bing what's the weather like\",
        //            \"properties\":
        //            {\"requestid\":\"f99c9963-ec5f-4168-bcd2-e4e18ebe5113\",\"HIGHCONF\":\"1\"}
        //}

        private const string statusKey = "status";
        private const string scenarioKey = "scenario";
        private const string nameKey = "name";
        private const string lexicalKey = "lexical";
        private const string propertiesKey = "properties";

        private string status;
        private string scenario;
        private string name;
        private string lexical;
        private string HttpError;
        private SpeechToTextConfidence properties;

        public SpeechToTextHeader()
        {
            status = string.Empty;
            scenario = string.Empty;
            name = string.Empty;
            lexical = string.Empty;
            HttpError = string.Empty;
            properties = new SpeechToTextConfidence();
        }

        public SpeechToTextHeader(JsonObject jsonObject)
        {
            status = jsonObject.GetNamedString(statusKey, "");
            scenario = jsonObject.GetNamedString(scenarioKey, "");
            name = jsonObject.GetNamedString(nameKey, "");
            lexical = jsonObject.GetNamedString(lexicalKey, "");
            JsonObject propertiesObject = jsonObject.GetNamedObject(propertiesKey, null);
            if (propertiesObject != null)
                properties = new SpeechToTextConfidence(propertiesObject);
        }
        public string Result()
        {
            if (string.IsNullOrEmpty(lexical))
                return string.Empty;
            return (lexical.StartsWith("bing ")? lexical.Substring(5):lexical);
        }
        public string Status()
        {
            return status;
        }
    }
    /// <summary>
    /// class SpeechToTextResponse 
    /// </summary>
    /// <info>
    /// Used to parse the JSON string coming from the SpeechToText REST API
    /// Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.
    /// </info>
    public sealed class SpeechToTextResponse
    {
        //{\"version\":\"3.0\",
        //\"header\":{\"status\":\"success\",\"scenario\":\"catsearch\",\"name\":\"bing what's the weather like\",\"lexical\":\"bing what's the weather like\",
        //          \"properties\":
        //               {\"requestid\":\"dd2f0028-b001-4d23-94e3-24fd6521da35\",\"HIGHCONF\":\"1\"}
        //           },
        //\"results\":
        //[{
        //\"scenario\":\"catsearch\",
        //\"name\":\"bing what's the weather like\",
        //\"lexical\":\"bing what's the weather like\",
        //\"confidence\":\"0.879686\",
        //\"properties\":{\"HIGHCONF\":\"1\"}
        //}]
        //}
        private const string RecognitionStatusKey = "RecognitionStatus";
        private const string OffsetKey = "Offset";
        private const string DurationKey = "Duration";
        private const string NBestKey = "NBest";
        private const string DisplayTextKey = "DisplayText";

        private string recognitionStatus;
        private  double offset;
        private double duration;
        private string displayText;


        //{
        //"RecognitionStatus": "Success",
        //"Offset": 22500000,
        //"Duration": 21000000,
        //"NBest": [{
        //    "Confidence": 0.941552162,
        //    "Lexical": "find a funny movie to watch",
        //    "ITN": "find a funny movie to watch",
        //    "MaskedITN": "find a funny movie to watch",
        //    "Display": "Find a funny movie to watch."
        //}]
        //}

        private ObservableCollection<SpeechToTextResult> results;
        private string HttpError;
        private string displayString;
        public override string ToString()
        {
            if(!string.IsNullOrEmpty(displayString))
                return displayString;
            return string.Empty;
        }

        public SpeechToTextResponse()
        {
            HttpError = string.Empty;
            displayText = string.Empty;
            results = new ObservableCollection<SpeechToTextResult>();
        }

        public SpeechToTextResponse(string jsonString, string httpError )
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                JsonObject jsonObject = JsonObject.Parse(jsonString);
                Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);
                displayString = obj.ToString();
                recognitionStatus = jsonObject.GetNamedString(RecognitionStatusKey, "");
                offset = jsonObject.GetNamedNumber(OffsetKey, 0);
                duration = jsonObject.GetNamedNumber(DurationKey, 0);
                displayText = jsonObject.GetNamedString(DisplayTextKey, "");
                results = new ObservableCollection<SpeechToTextResult>();
                if (results != null)
                {
                    foreach (IJsonValue jsonValue in jsonObject.GetNamedArray(NBestKey, new JsonArray()))
                    {
                        if (jsonValue.ValueType == JsonValueType.Object)
                        {
                            results.Add(new SpeechToTextResult(jsonValue.GetObject()));
                        }
                    }
                }
            }
            if(!string.IsNullOrEmpty(httpError))
            {
                HttpError = httpError;
            }
        }
        public string Result()
        {
            if ((results == null) ||  (results.Count == 0))
            {
                if (string.IsNullOrEmpty(displayText))
                    return string.Empty;
                return displayText;
            }

            return results[0].GetDisplay();
        }
        public string Status()
        {
            return recognitionStatus;
        }
        public string GetHttpError()
        {
            return HttpError;
        }

    }
}
