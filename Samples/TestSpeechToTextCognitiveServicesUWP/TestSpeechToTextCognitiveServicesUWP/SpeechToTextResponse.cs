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

namespace TestSpeechToTextCognitiveServicesUWP
{
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
    }
    class SpeechToTextResult
    {
        //{
        //\"scenario\":\"catsearch\",
        //\"name\":\"bing what's the weather like\",
        //\"lexical\":\"bing what's the weather like\",
        //\"confidence\":\"0.879686\",
        //\"properties\":{\"HIGHCONF\":\"1\"}
        //}
        private const string scenarioKey = "scenario";
        private const string nameKey = "name";
        private const string lexicalKey = "lexical";
        private const string confidenceKey = "confidence";
        private const string propertiesKey = "properties";

        private string scenario;
        private string name;
        private string lexical;
        private double confidence;
        private SpeechToTextConfidence properties;

        public SpeechToTextResult()
        {
            scenario = "";
            name = "";
            lexical = "";
            confidence = 0;
            properties = new SpeechToTextConfidence();
        }

        public SpeechToTextResult(JsonObject jsonObject)
        {
            scenario = jsonObject.GetNamedString(scenarioKey, "");
            name = jsonObject.GetNamedString(nameKey, "");
            lexical = jsonObject.GetNamedString(lexicalKey, "");
            scenario = jsonObject.GetNamedString(scenarioKey, "");
            string s = jsonObject.GetNamedString(confidenceKey, "");
            double.TryParse(s, out confidence);
            JsonObject propertiesObject = jsonObject.GetNamedObject(propertiesKey, null);
            if (propertiesObject != null)
                properties = new SpeechToTextConfidence(propertiesObject);
        }
    }
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

    public class SpeechToTextResponse
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
        private const string versionKey = "version";
        private const string headerKey = "header";
        private const string resultsKey = "results";

        private string version;
        private SpeechToTextHeader header;
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
            version = "3.0";
            header = null;
            results = new ObservableCollection<SpeechToTextResult>();
        }

        public SpeechToTextResponse(string jsonString, string httpError = null)
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                JsonObject jsonObject = JsonObject.Parse(jsonString);
                Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);
                displayString = obj.ToString();
                version = jsonObject.GetNamedString(versionKey, "");
                header = new SpeechToTextHeader(jsonObject.GetNamedObject(headerKey, null));

                results = new ObservableCollection<SpeechToTextResult>();
                if (results != null)
                {
                    foreach (IJsonValue jsonValue in jsonObject.GetNamedArray(resultsKey, new JsonArray()))
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
            if (header == null)
                return string.Empty;

            return header.Result();
        }
        public string Status()
        {
            if (header == null)
                return string.Empty;

            return header.Status();
        }
        public string GetHttpError()
        {
            return HttpError;
        }

    }
}
