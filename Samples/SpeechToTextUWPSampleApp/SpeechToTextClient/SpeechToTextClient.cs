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
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using System.Xml.Linq;

namespace SpeechToTextClient
{
    class WebSocketMessage
    {
        /* #region Public Properties */
        public string Body { get; private set; }
        public System.Collections.Generic.Dictionary<string, string> Headers { get; private set; }
        public string Path
        {
            get
            {
                return this.GetHeader("Path");
            }
        }
        public string RequestId
        {
            get
            {
                return this.GetHeader("X-RequestId");
            }
        }
        public string GetHeader(string key)
        {
            if (Headers.ContainsKey(key))
                return Headers[key];
            return string.Empty;
        }
        static public WebSocketMessage CreateSocketMessage(string message)
        {
            WebSocketMessage webSocketMessage = new WebSocketMessage();
            if (webSocketMessage!=null)
            {
                webSocketMessage.Headers = new System.Collections.Generic.Dictionary<string, string>();
                StringReader str = new StringReader(message);
                bool wasPreviousLineEmpty = true;
                string line = null;
                do
                {
                    line = str.ReadLine();
                    if (line == string.Empty)
                    {
                        if (wasPreviousLineEmpty) break;
                    }
                    else
                    {
                        var colonIndex = line.IndexOf(':');
                        if (colonIndex > -1)
                        {
                            webSocketMessage.Headers.Add(line.Substring(0, colonIndex), line.Substring(colonIndex + 1));
                        }
                    }

                } while (line != null);
                webSocketMessage.Body = str.ReadToEnd();
            }
            return webSocketMessage;
        }
    }

    /// <summary>
    /// Event which returns the position of the buffer ready to be sent 
    /// This event is fired with continuous recording
    /// </summary>
    public delegate void BufferReadyEventHandler(SpeechToTextClient sender);


    /// <summary>
    /// Event which returns the Audio Level of the audio samples
    /// being stored in the audio buffer
    /// </summary>
    public delegate void AudioLevelEventHandler(SpeechToTextClient sender, double level);


    /// <summary>
    /// Event which returns the Audio Capture Errors while 
    /// a recording is in progress
    /// </summary>
    /// <returns>true if successful</returns>
    public delegate void AudioCaptureErrorEventHandler(SpeechToTextClient sender, string message);

    /// <summary>
    /// class SpeechToTextClient: SpeechToText UWP Client
    /// </summary>
    /// <info>
    /// Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.
    /// </info>
    public sealed class SpeechToTextClient
    {
        private string SubscriptionKey;
        private string Token;
        private SpeechToTextMainStream STTStream;
        private const string SpeechAuthUrl = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private string AuthUrl = SpeechAuthUrl;
//        private const string CustomSpeechAuthUrl = "https://westus.api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private const string CustomSpeechAuthUrl = "https://northeurope.api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private const string SpeechUrl = "https://{0}/speech/recognition/{1}/cognitiveservices/v1";

        private const string WSSSpeechUrl = "wss://{0}/speech/recognition/{1}/cognitiveservices/v1";
        private const string endPointID = "d1ed3aff-18e7-4539-ad2f-d95e3129a842";
        // westus.stt.speech.microsoft.com
//        private const string WSSSpeechUrl = "wss://{0}/speech/recognition/{1}/cognitiveservices/v1?cid={2}"; 

        private string WebSocketRequestID;
        private bool bUseWebSocket;
        private Windows.Networking.Sockets.MessageWebSocket webSocket;
        private bool bWebSocketReady = false;
        private System.Threading.AutoResetEvent WebSocketEvent;

        private bool isRecordingInitialized;
        private bool isRecording;
        private ulong maxStreamSizeInBytes;
        private UInt16 thresholdDuration;
        private UInt16 thresholdLevel;
        private Windows.Media.Capture.MediaCapture mediaCapture;
        private string apiString = "interactive";
        private string apiSynthesizeString = "synthesize";
        private string hostnameString = "speech.platform.bing.com";
        /// <summary>
        /// class SpeechToTextClient constructor
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        public SpeechToTextClient()
        {
            SubscriptionKey = string.Empty;
            Token = string.Empty;
            isRecordingInitialized = false;
            isRecording = false;
            thresholdDuration = 0;
            thresholdLevel = 0;
            maxStreamSizeInBytes = 0;
            bUseWebSocket = false;
        }
        /// <summary>
        /// SetAPI method
        /// </summary>
        /// <param name="APIstring">String associated with the endpoint url (interactive, conversation, dictation) 
        /// 
        /// </param>
        /// <return>True if successfull 
        /// </return>
        public bool SetAPI(string HostnameString, string APIstring, bool bWebSocket )
        {
            bool result = false;
            if (string.Equals(APIstring, "interactive") ||
                string.Equals(APIstring, "conversation") ||
                string.Equals(APIstring, "dictation"))
            {
                apiString = APIstring;
                if (!string.IsNullOrEmpty(HostnameString))
                {
                    hostnameString = HostnameString;
                    bUseWebSocket = bWebSocket;
                    if(bUseWebSocket==true)
                    {
                        if (webSocket != null)
                        {
                            if (bWebSocketReady == true)
                            {
                                webSocket.MessageReceived -= WebSocket_MessageReceived;
                                webSocket.Closed -= WebSocket_Closed;
                                webSocket.Dispose();
                            }
                            webSocket = null;
                        }
                        if (webSocket == null)
                        {
                            webSocket = new Windows.Networking.Sockets.MessageWebSocket();
                        }
                        if (webSocket != null)
                        {
                            webSocket.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Binary;

                            webSocket.MessageReceived += WebSocket_MessageReceived;
                            webSocket.Closed += WebSocket_Closed;
                        }
                    }
                    else
                    {
                        if (webSocket != null)
                        {

                            webSocket.MessageReceived -= WebSocket_MessageReceived;
                            webSocket.Closed -= WebSocket_Closed;
                            webSocket.Dispose();
                            webSocket = null;
                        }

                    }
                    if (string.Equals(hostnameString, "speech.platform.bing.com"))
                        AuthUrl = SpeechAuthUrl;
                    else
                        AuthUrl = CustomSpeechAuthUrl;
                }
                result = true;
            }
            return result;
        }
        private void WebSocket_MessageReceived(Windows.Networking.Sockets.MessageWebSocket sender, Windows.Networking.Sockets.MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                if (bWebSocketReady)
                {
                    using (Windows.Storage.Streams.DataReader dataReader = args.GetDataReader())
                    {
                        dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        string message = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                        System.Diagnostics.Debug.WriteLine("Message received from MessageWebSocket: " + message);
                        WebSocketMessage wsm = WebSocketMessage.CreateSocketMessage(message);
                        if (wsm != null)
                        {
                            switch (wsm.Path.ToLower())
                            {
                                case "turn.start":
                                    WebSocketEvent.Set();
                                    break;
                                case "turn.end":
                                    bWebSocketReady = false;
                                    break;
                                case "speech.enddetected":
                                    break;
                                case "speech.phrase":
                                    break;
                                case "speech.hypothesis":
                                    break;
                                case "speech.startdetected":
                                    break;
                                case "speech.fragment":
                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Windows.Web.WebErrorStatus webErrorStatus = Windows.Networking.Sockets.WebSocketError.GetStatus(ex.GetBaseException().HResult);
                // Add additional code here to handle exceptions.
            }
        }

        private void WebSocket_Closed(Windows.Networking.Sockets.IWebSocket sender, Windows.Networking.Sockets.WebSocketClosedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("WebSocket_Closed; Code: " + args.Code + ", Reason: \"" + args.Reason + "\"");
            // Add additional code here to handle the WebSocket being closed.
            webSocket.Dispose();
            webSocket = null;
        }
        /// <summary>
        /// ClearToken method
        /// </summary>
        /// <return>true.
        /// </return>
        public bool ClearToken()
        {
            Token = String.Empty;
            return true;
        }

        /// <summary>
        /// GetToken method
        /// </summary>
        /// <param name="subscriptionKey">SubscriptionKey associated with the SpeechToText 
        /// Cognitive Service subscription.
        /// </param>
        /// <return>Token which is used for all calls to the SpeechToText REST API.
        /// </return>
        public IAsyncOperation<string> GetToken(string subscriptionKey )
        {
            return Task.Run<string>( async () =>
            {
                if (string.IsNullOrEmpty(subscriptionKey))
                    return string.Empty;
                SubscriptionKey = subscriptionKey;
                try
                {
                    Token = string.Empty;
                    Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Ocp-Apim-Subscription-Key", SubscriptionKey);
                    Windows.Web.Http.HttpStringContent content = new Windows.Web.Http.HttpStringContent(String.Empty);

                    Windows.Web.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(AuthUrl), content);
                    if (hrm != null)
                    {
                        switch (hrm.StatusCode)
                        {
                            case Windows.Web.Http.HttpStatusCode.Ok:
                                var b = await hrm.Content.ReadAsBufferAsync();
                                string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                                if (!string.IsNullOrEmpty(result))
                                {
                                    Token = "Bearer " + result;
                                    return Token;
                                }
                                break;

                            default:
                                System.Diagnostics.Debug.WriteLine("Http Response Error:" + hrm.StatusCode.ToString() + " reason: " + hrm.ReasonPhrase.ToString());
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while getting the token: " + ex.Message);
                }
                return string.Empty;
            }).AsAsyncOperation<string>();
        }
        /// <summary>
        /// RenewToken method
        /// </summary>
        /// <param>
        /// </param>
        /// <return>Token which is used to all the calls to the SpeechToText REST API.
        /// </return>
        public IAsyncOperation<string> RenewToken()
        {
            return Task.Run<string>(async () =>
            {
            if (string.IsNullOrEmpty(SubscriptionKey))
                return string.Empty;
            try
            {
                Token = string.Empty;
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                hc.DefaultRequestHeaders.TryAppendWithoutValidation("Ocp-Apim-Subscription-Key", SubscriptionKey);
                Windows.Web.Http.HttpStringContent content = new Windows.Web.Http.HttpStringContent(String.Empty);
                Windows.Web.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(AuthUrl), content);
                if (hrm != null)
                {
                    switch (hrm.StatusCode)
                    {
                        case Windows.Web.Http.HttpStatusCode.Ok:
                            var b = await hrm.Content.ReadAsBufferAsync();
                            string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                            if (!string.IsNullOrEmpty(result))
                            {
                                Token = "Bearer  " + result;
                                return Token;
                            }
                            break;

                        default:
                            System.Diagnostics.Debug.WriteLine("Http Response Error:" + hrm.StatusCode.ToString() + " reason: " + hrm.ReasonPhrase.ToString());
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while getting the token: " + ex.Message);
            }
            return string.Empty;
            }).AsAsyncOperation<string>();
        }
        /// <summary>
        /// Generates SSML.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="name">The voice name.</param>
        /// <param name="text">The text input.</param>
        private string GenerateSsml(string locale, string gender, string name, string text)
        {
            var ssmlDoc = new XDocument(
                              new XElement("speak",
                                  new XAttribute("version", "1.0"),
                                  new XAttribute(XNamespace.Xml + "lang", "en-US"),
                                  new XElement("voice",
                                      new XAttribute(XNamespace.Xml + "lang", locale),
                                      new XAttribute(XNamespace.Xml + "gender", gender),
                                      new XAttribute("name", name),
                                      text)));
            return ssmlDoc.ToString();
        }

        string GetVoiceName(string lang, string gender)
        {
            string voiceName = "Microsoft Server Speech Text to Speech Voice (en-US, BenjaminRUS)";

            switch (lang.ToLower())
            {
                case "ar-eg":
                    voiceName = "Microsoft Server Speech Text to Speech Voice (ar-EG, Hoda)";
                    break;
                case "de-de":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (de-DE, Hedda)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (de-DE, Stefan, Apollo)";
                    break;
                case "en-au":
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-AU, Catherine)";
                    break;
                case "en-ca":
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-CA, Linda)";
                    break;
                case "en-gb":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-GB, Susan, Apollo)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-GB, George, Apollo)";
                    break;
                case "en-in":
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-IN, Ravi, Apollo)";
                    break;
                case "en-us":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-US, BenjaminRUS)";
                    break;
                case "es-es":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (es-ES, Laura, Apollo)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (es-ES, Pablo, Apollo)";
                    break;
                case "es-mx":
                    voiceName = "Microsoft Server Speech Text to Speech Voice (es-MX, Raul, Apollo)";
                    break;
                case "fr-ca":
                    voiceName = "Microsoft Server Speech Text to Speech Voice (fr-CA, Caroline)";
                    break;
                case "fr-fr":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (fr-FR, Julie, Apollo)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (fr-FR, Paul, Apollo)";
                    break;
                case "it-it":
                    voiceName = "Microsoft Server Speech Text to Speech Voice (it-IT, Cosimo, Apollo)";
                    break;
                case "ja-jp":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (ja-JP, Ayumi, Apollo)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (ja-JP, Ichiro, Apollo)";
                    break;
                case "pt-br":
                    voiceName = "Microsoft Server Speech Text to Speech Voice (pt-BR, Daniel, Apollo)";
                    break;
                case "ru-ru":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (ru-RU, Irina, Apollo)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (ru-RU, Pavel, Apollo)";
                    break;
                case "zh-cn":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-CN, HuihuiRUS)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-CN, Kangkang, Apollo)";
                    break;
                case "zh-hk":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-HK, Tracy, Apollo)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-HK, Danny, Apollo)";
                    break;
                case "zh-tw":
                    if(gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-TW, Yating, Apollo)";
                    else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-TW, Zhiwei, Apollo)";
                    break;
            }
            return voiceName;

        }
        /// <summary>
        /// TextToSpeech method
        /// </summary>
        /// <param>
        /// text to convert to speech
        /// lang language of the text ("en-us", "fr-fr")
        /// gender for the voice ('female" or "male")
        /// </param>
        /// <return>The audio stream
        /// </return>
        public IAsyncOperation<Windows.Storage.Streams.IInputStream> TextToSpeech(string text, string lang, string gender)
        {
            return Task.Run<Windows.Storage.Streams.IInputStream>(async () =>
            {
                if (!HasToken())
                    return null;
                try
                {
                    Windows.Web.Http.Filters.HttpBaseProtocolFilter httpBaseProtocolFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
                    httpBaseProtocolFilter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;
                    httpBaseProtocolFilter.CacheControl.WriteBehavior = Windows.Web.Http.Filters.HttpCacheWriteBehavior.NoCache;

                    httpBaseProtocolFilter.CookieUsageBehavior = Windows.Web.Http.Filters.HttpCookieUsageBehavior.NoCookies;
                    httpBaseProtocolFilter.AutomaticDecompression = false;
                    

                    Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient(httpBaseProtocolFilter);


                    string speechUrl = "https://" + hostnameString + "/" + apiSynthesizeString;
                    System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                    hc.DefaultRequestHeaders.Clear();
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", Token);
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("X-Search-AppId", "07D3234E49CE426DAA29772419F436CA");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("X-Search-ClientID", "1ECFAE91408841A480F00935DC390960");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("User-Agent", "TTSClient");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Expect", "100-continue");

                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm");
                    hc.DefaultRequestHeaders.Connection.Clear();
                    


                   // string TextToSpeechContent = "<speak version=\"1.0\" xml:lang=\"{0}\"><voice xml:lang=\"{1}\" xml:gender=\"{2}\" name=\"Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)\">{3}</voice></speak>";
                  //  string contentString = String.Format(TextToSpeechContent, lang, lang, gender, text);
                    string contentString = GenerateSsml( lang, gender, GetVoiceName(lang,gender),text);
                    Windows.Web.Http.HttpStringContent content = new Windows.Web.Http.HttpStringContent(contentString);
                    content.Headers.Clear();
                    content.Headers.Remove("Content-Type");
                    content.Headers.TryAppendWithoutValidation("Content-Type", "application/ssml+xml");



                    Windows.Web.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(speechUrl), content);
                    if (hrm != null)
                    {
                        switch (hrm.StatusCode)
                        {
                            case Windows.Web.Http.HttpStatusCode.Ok:
                                return await hrm.Content.ReadAsInputStreamAsync();
                            default:
                                System.Diagnostics.Debug.WriteLine("Http Response Error:" + hrm.StatusCode.ToString() + " reason: " + hrm.ReasonPhrase.ToString());
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while getting speech from text: " + ex.Message);
                }
                return null;
            }).AsAsyncOperation<Windows.Storage.Streams.IInputStream>();
        }

        /// <summary>
        /// HasToken method
        /// </summary>
        /// <param>Check if a Token has been acquired
        /// </param>
        /// <return>true if a Token has been acquired to use the SpeechToText REST API.
        /// </return>
        public bool HasToken()
        {
            if (string.IsNullOrEmpty(Token))
                return false;
            return true;
        }
        /// <summary>
        /// GetAudioStream method
        /// This method return the audio buffer (stream) which has been acquired while the client is continuously recording the audio.
        /// </summary>
        /// <param>
        /// </param>
        /// <return>The SpeechToTextAudioStream in the queue, null if the queue is empty.
        /// </return>
        public SpeechToTextAudioStream GetAudioStream()
        {
            return STTStream.GetAudioStream();
        }
        /// <summary>
        /// SendBuffer method
        /// This method sends the current audio buffer towards Cognitive Services REST API
        /// </summary>
        /// <param name="locale">language associated with the current buffer/recording.
        /// for instance en-US, fr-FR, pt-BR, ...
        /// </param>
        /// <return>The result of the SpeechToText REST API.
        /// </return>
        public IAsyncOperation<SpeechToTextResponse> SendBuffer(string locale, string resulttype)
        {
            return Task.Run<SpeechToTextResponse>(async () =>
            {
                SpeechToTextResponse r = null;
            int loop = 1;

            while (loop-- > 0)
            {
                try
                {
                    string speechUrl = string.Format(SpeechUrl, hostnameString, apiString) + "?language=" + locale + "&format=" + resulttype;
                    Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                    System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", Token);
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "application/json;text/xml");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Transfer-Encoding", "chunked");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Expect", "100-continue");

                    Windows.Web.Http.HttpResponseMessage hrm = null;
                    Windows.Web.Http.HttpStreamContent content = null;
                    if (STTStream != null)
                    {
                        content = new Windows.Web.Http.HttpStreamContent(STTStream.AsStream().AsInputStream());
                        //content.Headers.ContentLength = STTStream.GetLength();
                        //System.Diagnostics.Debug.WriteLine("REST API Post Content Length: " + content.Headers.ContentLength.ToString());
                        content.Headers.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                        IProgress<Windows.Web.Http.HttpProgress> progress = new Progress<Windows.Web.Http.HttpProgress>(ProgressHandler);
                        hrm = await hc.PostAsync(new Uri(speechUrl), content).AsTask(cts.Token, progress);
                    }
                    if (hrm != null)
                    {
                        switch (hrm.StatusCode)
                        {
                            case Windows.Web.Http.HttpStatusCode.Ok:
                                var b = await hrm.Content.ReadAsBufferAsync();
                                string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                                if (!string.IsNullOrEmpty(result))
                                    r = new SpeechToTextResponse(result,null);
                                break;

                            case Windows.Web.Http.HttpStatusCode.Forbidden:
                                string token = await RenewToken();
                                if (string.IsNullOrEmpty(token))
                                {
                                    loop++;
                                }
                                break;

                            default:
                                int code = (int)hrm.StatusCode;
                                string HttpError = "Http Response Error: " + code.ToString() + " reason: " + hrm.ReasonPhrase.ToString();
                                System.Diagnostics.Debug.WriteLine(HttpError);
                                r = new SpeechToTextResponse(string.Empty, HttpError);
                                break;
                        }
                    }
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("http POST canceled");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("http POST exception: " + ex.Message);
                }
                finally
                {
                    System.Diagnostics.Debug.WriteLine("http POST done");
                }
            }
            return r;
            }).AsAsyncOperation<SpeechToTextResponse>();
        }
        /// <summary>
        /// SendAudioStream method
        /// This method sends a SpeechToTextAudioStream towards Cognitive Services REST API.
        /// Usually, the method GetAudioStream returns the SpeechToTextAudioStream (if available) then the method SendAudioStream 
        /// sends a SpeechToTextAudioStream towards Cognitive Services REST API.
        /// </summary>
        /// <param name="locale">language associated with the current buffer/recording.
        /// for instance en-US, fr-FR, pt-BR, ...
        /// </param>
        /// <param name="stream">AudioStream which will be forwarded to REST API.
        /// </param>
        /// <return>The result of the SpeechToText REST API.
        /// </return>
        public IAsyncOperation<SpeechToTextResponse> SendAudioStream(string locale, string resulttype, SpeechToTextAudioStream stream)
        {
            return Task.Run<SpeechToTextResponse>(async () =>
            {
                SpeechToTextResponse r = null;
            int loop = 1;

            while (loop-- > 0)
            {
                try
                {
                    string speechUrl = string.Format(SpeechUrl, hostnameString, apiString) + "?language=" + locale + "&format=" + resulttype;

                    Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                    System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", Token);
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "application/json;text/xml");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Transfer-Encoding", "chunked");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Expect", "100-continue");

                    Windows.Web.Http.HttpResponseMessage hrm = null;
                    Windows.Web.Http.HttpStreamContent content = null;
                    content = new Windows.Web.Http.HttpStreamContent(stream.GetInputStreamAt(0));
                    //content.Headers.ContentLength = (ulong)stream.Size;
                    if ((content != null) && (stream.Size > 0))
                    {
                        System.Diagnostics.Debug.WriteLine("REST API Post Content Length: " + content.Headers.ContentLength.ToString());
                        content.Headers.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                        IProgress<Windows.Web.Http.HttpProgress> progress = new Progress<Windows.Web.Http.HttpProgress>(ProgressHandler);
                        hrm = await hc.PostAsync(new Uri(speechUrl), content).AsTask(cts.Token, progress);
                    }
                    if (hrm != null)
                    {
                        switch (hrm.StatusCode)
                        {
                            case Windows.Web.Http.HttpStatusCode.Ok:
                                var b = await hrm.Content.ReadAsBufferAsync();
                                string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                                if (!string.IsNullOrEmpty(result))
                                    r = new SpeechToTextResponse(result,null);
                                break;
                            case Windows.Web.Http.HttpStatusCode.Forbidden:
                                string token = await RenewToken();
                                if (string.IsNullOrEmpty(token))
                                {
                                    loop++;
                                }
                                break;

                            default:
                                int code = (int)hrm.StatusCode;
                                string HttpError = "Http Response Error: " + code.ToString() + " reason: " + hrm.ReasonPhrase.ToString();
                                System.Diagnostics.Debug.WriteLine(HttpError);
                                r = new SpeechToTextResponse(string.Empty, HttpError);
                                break;
                        }
                    }
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("http POST canceled");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("http POST exception: " + ex.Message);
                }
                finally
                {
                    System.Diagnostics.Debug.WriteLine("http POST done");
                }
            }
            return r;
        }).AsAsyncOperation<SpeechToTextResponse>();
        }
        char GetChar(byte b)
        {
            if ((b >= 32) && (b < 127))
                return (char)b;
            return '.';
        }
        string DumpHex(byte[] data)
        {
            string result = string.Empty;
            string resultHex = " ";
            string resultASCII = " ";
            int Len = ((data.Length % 16 == 0) ? (data.Length / 16) : (data.Length / 16) + 1) * 16;
            for (int i = 0; i < Len; i++)
            {
                if (i < data.Length)
                {
                    resultASCII += string.Format("{0}", GetChar(data[i]));
                    resultHex += string.Format("{0:X2} ", data[i]);
                }
                else
                {
                    resultASCII += " ";
                    resultHex += "   ";
                }
                if (i % 16 == 15)
                {
                    result += string.Format("{0:X8} ", i - 15) + resultHex + resultASCII + "\r\n";
                    resultHex = " ";
                    resultASCII = " ";
                }
            }
            return result;
        }
        async System.Threading.Tasks.Task<bool> SendSpeechConfig()
        {
            bool result = false;

            try
            {


                //Create a request id that is unique for this 
                webSocket.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Utf8;

                // RequestID for the session
                WebSocketRequestID = Guid.NewGuid().ToString("N");

                //Send the first message after connecting to the websocket with required headers
                string payload =
                 "Path: speech.config" + Environment.NewLine +
                 "x-timestamp: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK") + Environment.NewLine +
                 "content-type: application/json; charset=utf-8" + Environment.NewLine + Environment.NewLine +
                 "{ \"context\":{ \"system\":{ \"version\":\"1.0.00000\"},\"os\":{ \"platform\":\"Unity\",\"name\":\"Unity\",\"version\":\"\"},\"device\":{ \"manufacturer\":\"SpeechSample\",\"model\":\"UnitySpeechSample\",\"version\":\"1.0.00000\"} } }";

                //"{\"context\": {\"system\": { \"version\":  \"2.0.12341\"},\"os\": {\"platform\": \"Windows\",\"name\": \"Windows 10\",\"version\": \"10.0.0.0\" },\"device\": {\"manufacturer\": \"Contoso\",\"model\": \"Fabrikan\",\"version\": \"7.341\"}}}}";

                System.Diagnostics.Debug.Write(payload);
                using (var dataWriter = new Windows.Storage.Streams.DataWriter(webSocket.OutputStream))
                {
                    dataWriter.WriteString(payload);
                    await dataWriter.StoreAsync();
                    dataWriter.DetachStream();
                }
                result = true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while sending Speech Config: " + ex.Message);
            }
            return result;
        }
        byte[] CreateAudioWebSocketHeader()
        {
            var outputBuilder = new System.Text.StringBuilder();
            outputBuilder.Append("path:audio\r\n");
            outputBuilder.Append("x-requestid:" + WebSocketRequestID + "\r\n");
            outputBuilder.Append("x-timestamp:" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK") + "\r\n");
            outputBuilder.Append("content-type:audio/x-wav\r\n");

            return System.Text.Encoding.ASCII.GetBytes(outputBuilder.ToString());
        }
        byte[] CreateAudioWebSocketHeaderLength(byte[] header)
        {
            var str = "0x" + (header.Length).ToString("X");
            var headerHeadBytes = BitConverter.GetBytes((UInt16)header.Length);
            var isBigEndian = !BitConverter.IsLittleEndian;
            return !isBigEndian ? new byte[] { headerHeadBytes[1], headerHeadBytes[0] } : new byte[] { headerHeadBytes[0], headerHeadBytes[1] };
        }
        async System.Threading.Tasks.Task<bool> SendSpeechFile(Windows.Storage.StorageFile wavFile)
        {
            bool result = false;
            try
            {
                using (var fileStream = await wavFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    int Len = GetWAVHeaderLength(fileStream);
                    if (Len > 0)
                    {
                        byte[] byteArray = new byte[fileStream.Size - (ulong)Len];
                        if (byteArray != null)
                        {

                            fileStream.Seek((ulong)Len);
                            fileStream.ReadAsync(byteArray.AsBuffer(), (uint)(fileStream.Size-(ulong)Len), Windows.Storage.Streams.InputStreamOptions.Partial).AsTask().Wait();

                            int index = 0;
                            while (index < (int)(fileStream.Size - (ulong)Len))
                            {
                                var headerBytes = CreateAudioWebSocketHeader();
                                var headerHead = CreateAudioWebSocketHeaderLength(headerBytes);

                                var length = Math.Min(4096 * 2 - headerBytes.Length - 8, byteArray.Length - index); //8bytes for the chunk header

                                var chunkHeader = System.Text.Encoding.ASCII.GetBytes("data").Concat(BitConverter.GetBytes((UInt32)length)).ToArray();

                                byte[] dataArray = new byte[length];
                                Array.Copy(byteArray, index, dataArray, 0, length);

                                index += length;

                                var arr = headerHead.Concat(headerBytes).Concat(chunkHeader).Concat(dataArray).ToArray();


                                if (webSocket != null)
                                {
                                    //Create a request id that is unique for this 
                                    webSocket.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Binary;
                                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(webSocket.OutputStream))
                                    {
                                      //  System.Diagnostics.Debug.Write(DumpHex(arr));
                                        dataWriter.WriteBytes(arr);
                                        await dataWriter.StoreAsync();
                                        await dataWriter.FlushAsync();
                                        dataWriter.DetachStream();
                                    }
                                }
                                else
                                    break;
                            }
                        }
                    }
                }
                {
                    var headerBytes = CreateAudioWebSocketHeader();
                    var headerHead = CreateAudioWebSocketHeaderLength(headerBytes);
                    var arr = headerHead.Concat(headerBytes).ToArray();

                    if (webSocket != null)
                    {
                        //Create a request id that is unique for this 
                        webSocket.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Binary;
                        using (var dataWriter = new Windows.Storage.Streams.DataWriter(webSocket.OutputStream))
                        {
                            dataWriter.WriteBytes(arr);
                            await dataWriter.StoreAsync();
                            await dataWriter.FlushAsync();
                            dataWriter.DetachStream();
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while sending Speech Config: " + ex.Message);
            }
            return result;
        }
        int GetWAVHeaderLength(Windows.Storage.Streams.IRandomAccessStream fileStream)
        {
            int Len = 0;
            byte[] array = new byte[1024];
            if (array != null)
            {
                fileStream.Seek(0);
                fileStream.ReadAsync(array.AsBuffer(), (uint)1024, Windows.Storage.Streams.InputStreamOptions.Partial).AsTask().Wait();

                byte[] IntBuffer = new byte[4];
                if ((array[0] == 'R') &&
                    (array[1] == 'I') &&
                    (array[2] == 'F') &&
                    (array[3] == 'F'))
                {
                    Buffer.BlockCopy(array, 4, IntBuffer, 0, 4);
                    int Index = 8;
                    int FileLen = BitConverter.ToInt32(IntBuffer, 0);
                    if ((array[Index] == 'W') &&
                        (array[Index + 1] == 'A') &&
                        (array[Index + 2] == 'V') &&
                        (array[Index + 3] == 'E'))
                    {
                        Index += 4;
                        while (Index < array.Length)
                        {
                            if ((array[Index] == 'd') &&
                                (array[Index + 1] == 'a') &&
                                (array[Index + 2] == 't') &&
                                (array[Index + 3] == 'a'))
                            {
                                Len = Index;
                                break;
                            }
                            else
                            {
                                Index += 4;
                                Buffer.BlockCopy(array, Index, IntBuffer, 0, 4);
                                Index += 4;
                                Index += BitConverter.ToInt32(IntBuffer, 0);
                            }
                        }
                    }
                }
            }
            return Len;
        }
        async System.Threading.Tasks.Task<bool> SendSpeechFileHeader(Windows.Storage.StorageFile wavFile)
        {
            bool result = false;
            try
            {
                using (var fileStream = await wavFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    int Len = GetWAVHeaderLength(fileStream);
                    if (Len > 0)
                    {
                        byte[] header = new byte[Len];
                        if (header != null)
                        {
                            fileStream.Seek(0);
                            fileStream.ReadAsync(header.AsBuffer(), (uint)Len, Windows.Storage.Streams.InputStreamOptions.Partial).AsTask().Wait();

                            var headerBytes = CreateAudioWebSocketHeader();
                            var headerHead = CreateAudioWebSocketHeaderLength(headerBytes);

                            var arr = headerHead.Concat(headerBytes).Concat(header).ToArray();


                            if (webSocket != null)
                            {
                                //Create a request id that is unique for this 
                                webSocket.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Binary;
                            //    System.Diagnostics.Debug.Write(DumpHex(arr));

                                using (var dataWriter = new Windows.Storage.Streams.DataWriter(webSocket.OutputStream))
                                {
                                    dataWriter.WriteBytes(arr);
                                    await dataWriter.StoreAsync();
                                    await dataWriter.FlushAsync();
                                    dataWriter.DetachStream();
                                }
                            }

                        }

                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while sending Speech Config: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// SendStorageFile method
        /// </summary>
        /// <param name="wavFile">StorageFile associated with the audio file which 
        /// will be sent to the SpeechToText Services.
        /// </param>
        /// <param name="locale">language associated with the current buffer/recording.
        /// for instance en-US, fr-FR, pt-BR, ...
        /// </param>
        /// <return>The result of the SpeechToText REST API.
        /// </return>
        public IAsyncOperation<SpeechToTextResponse> SendStorageFile(Windows.Storage.StorageFile wavFile, string locale,string resulttype)
        {
            return Task.Run<SpeechToTextResponse>(async () =>
            {
            if(bUseWebSocket==true)
            {
                    try
                    {
                        string speechUrl = string.Empty;
                        if (string.Equals(hostnameString, "speech.platform.bing.com"))
                            speechUrl = string.Format(WSSSpeechUrl, hostnameString, apiString) + "?language=" + locale + "&format=" + resulttype;
                        else
  //                          speechUrl = string.Format(WSSSpeechUrl, hostnameString, apiString, endPointID) ;
                        speechUrl = string.Format(WSSSpeechUrl, hostnameString, apiString, endPointID) + "?language=" + locale + "&format=" + resulttype;
                        webSocket.SetRequestHeader("Authorization", Token);
                        webSocket.SetRequestHeader("X-ConnectionId", Guid.NewGuid().ToString("N"));
                        await webSocket.ConnectAsync(new Uri(speechUrl));
                        // ResetEvent for synchronization
                        if (WebSocketEvent == null)
                            WebSocketEvent = new System.Threading.AutoResetEvent(false);
                        else
                            WebSocketEvent.Reset();
                        System.Diagnostics.Debug.WriteLine("Sending Speech Config");
                        bWebSocketReady = true;
                        if (await SendSpeechConfig() == true)
                        {
                            System.Diagnostics.Debug.WriteLine("Sending Speech Header");
                            if (await SendSpeechFileHeader(wavFile) == true)
                            {


                                // wait for the reception of Start message before sending the Wav file
                                if (WebSocketEvent.WaitOne(10000))
                                {
                                    System.Diagnostics.Debug.WriteLine("Sending Speech File");
                                    await SendSpeechFile(wavFile);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("Message Start not received after 5000 ms");
                                    return null;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception while sending Speech Config and WAV file: " + ex.Message);
                    }
                    return null;
            }
            SpeechToTextResponse r = null;
            int loop = 1;

            while (loop-- > 0)
            {
                try
                {
                    string speechUrl = string.Format(SpeechUrl, hostnameString, apiString) + "?language=" + locale + "&format=" + resulttype;
                    Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();

                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", Token);
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "application/json;text/xml");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Transfer-Encoding", "chunked");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Expect", "100-continue");
                    Windows.Web.Http.HttpResponseMessage hrm = null;

                    Windows.Storage.StorageFile file = wavFile;
                    if (file != null)
                    {
                        using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                        {
                            if (STTStream != null)
                            {
                                STTStream.AudioLevel -= STTStream_AudioLevel;
                                STTStream.Dispose();
                                STTStream = null;
                            }
                            STTStream = SpeechToTextMainStream.Create(0,0,0);
                            if (STTStream != null)
                            {
                                byte[] byteArray = new byte[fileStream.Size];
                                fileStream.ReadAsync(byteArray.AsBuffer(), (uint)fileStream.Size, Windows.Storage.Streams.InputStreamOptions.Partial).AsTask().Wait();
                                STTStream.WriteAsync(byteArray.AsBuffer()).AsTask().Wait();

                                Windows.Web.Http.HttpStreamContent content = new Windows.Web.Http.HttpStreamContent(STTStream.AsStream().AsInputStream());
                //                content.Headers.ContentLength = STTStream.GetLength();
                  //              System.Diagnostics.Debug.WriteLine("REST API Post Content Length: " + content.Headers.ContentLength.ToString() + " bytes");
                                System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                                IProgress<Windows.Web.Http.HttpProgress> progress = new Progress<Windows.Web.Http.HttpProgress>(ProgressHandler);
                                hrm = await hc.PostAsync(new Uri(speechUrl), content).AsTask(cts.Token, progress);
                                
                            }
                        }
                    }
                    if (hrm != null)
                    {
                        switch (hrm.StatusCode)
                        {
                            case Windows.Web.Http.HttpStatusCode.Ok:
                                var b = await hrm.Content.ReadAsBufferAsync();
                                string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                                if (!string.IsNullOrEmpty(result))
                                    r = new SpeechToTextResponse(result,null);
                                break;

                            case Windows.Web.Http.HttpStatusCode.Forbidden:
                                string token = await RenewToken();
                                if (string.IsNullOrEmpty(token))
                                {
                                    loop++;
                                }
                                break;

                            default:
                                int code = (int)hrm.StatusCode;
                                string HttpError = "Http Response Error: " + code.ToString() + " reason: " + hrm.ReasonPhrase.ToString();
                                System.Diagnostics.Debug.WriteLine(HttpError);
                                r = new SpeechToTextResponse(string.Empty, HttpError);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while sending the audio file:" + ex.Message);
                }
            }
            return r;
             }).AsAsyncOperation<SpeechToTextResponse>();
        }
        /// <summary>
        /// SaveBuffer method
        /// </summary>
        /// <param name="wavFile">StorageFile where the audio buffer 
        /// will be stored.
        /// </param>
        /// <param name="start">the position in the buffer of the first byte to save in a file. 
        /// by default the value is 0.
        /// </param>
        /// <param name="end">the position in the buffer of the last byte to save in a file
        /// by default the value is 0, if the value is 0 the whole buffer will be stored in a a file
        /// </param>
        /// <return>true if successful.
        /// </return>
        public IAsyncOperation<bool> SaveBuffer(Windows.Storage.StorageFile wavFile, UInt64 start, UInt64 end)
        {
            return Task.Run<bool>(async () =>
            {
                bool bResult = false;
            if (wavFile != null)
            {
                try
                {
                    using (Stream stream = await wavFile.OpenStreamForWriteAsync())
                    {
                        if ((stream != null) && (STTStream != null))
                        {
                            stream.SetLength(0);
                            if ((start == 0) && (end == 0))
                            {
                                await STTStream.AsStream().CopyToAsync(stream);
                                System.Diagnostics.Debug.WriteLine("Audio Stream stored in: " + wavFile.Path);
                                bResult = true;
                            }
                            else if ((start >= 0) && (end > start))
                            {
                                var headerBuffer = STTStream.CreateWAVHeaderBuffer((uint)(end - start));
                                if (headerBuffer != null)
                                {
                                    byte[] buffer = new byte[headerBuffer.Length + (uint)(end - start)];
                                    if (buffer != null)
                                    {
                                        headerBuffer.CopyTo(buffer, headerBuffer.Length);
                                        ulong pos = STTStream.Position;
                                        STTStream.Seek(start);
                                        STTStream.ReadAsync(buffer.AsBuffer((int)headerBuffer.Length, (int)(end - start)), (uint)(end - start), Windows.Storage.Streams.InputStreamOptions.None).AsTask().Wait();
                                        STTStream.Seek(pos);
                                        MemoryStream bufferStream = new MemoryStream(buffer);
                                        await bufferStream.CopyToAsync(stream);
                                        System.Diagnostics.Debug.WriteLine("Audio Stream stored in: " + wavFile.Path);
                                        bResult = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while saving the Audio Stream stored in: " + wavFile.Path + " Exception: " + ex.Message);

                }
            }
            return bResult;
        }).AsAsyncOperation<bool>();
        }
        /// <summary>
        /// IsRecording method
        /// </summary>
        /// <return>Return true if the Client is currently recording
        /// </return>
        public bool IsRecording()
        {
            return isRecording;
        }
        /// <summary>
        /// GetBufferLength method
        /// Return the length of the current audio buffer
        /// </summary>
        /// <return>the length of the WAV buffer in ulong.
        /// </return>
        public ulong GetBufferLength()
        {
            if (STTStream != null)
            {
                return STTStream.GetLength();
            }
            return 0;
        }
        /// <summary>
        /// StartRecording method
        /// Start to record audio using the microphone.
        /// The audio stream in stored in memory with no limit of size.
        /// </summary>
        /// <param name="MaxStreamSizeInBytes">
        /// This parameter defines the max size of the buffer in memory. When the size of the buffer is over this limit, the 
        /// client create another stream and remove the previouw stream. 
        /// By default the value is 0, in that case the audio stream in stored in memory with no limit of size.
        /// </param>
        /// <return>return true if successful.
        /// </return>
        public IAsyncOperation<bool> StartRecording(ulong MaxStreamSizeInBytes)
        {
            return Task.Run<bool>(async () =>
            {
                return await StartContinuousRecording(MaxStreamSizeInBytes,0,0);
            }).AsAsyncOperation<bool>();
        }
        /// <summary>
        /// StartRecording method
        /// Start to record audio using the microphone.
        /// The audio stream in stored in memory with no limit of size.
        /// </summary>
        /// <param name="MaxStreamSizeInBytes">
        /// This parameter defines the max size of the buffer in memory. When the size of the buffer is over this limit, the 
        /// client create another stream and remove the previouw stream. 
        /// By default the value is 0, in that case the audio stream in stored in memory with no limit of size.
        /// </param>
        /// <param name="ThresholdDuration">
        /// The duration in milliseconds for the calculation of the average audio level. 
        /// With this parameter you define the period during which the average level is measured. 
        /// If the value is 0, no buffer will be sent to Cognitive Services.
        /// </param>
        /// <param name="ThresholdLevel">
        /// The minimum audio level average necessary to trigger the recording, 
        /// it's a value between 0 and 65535. You can tune this value after several microphone tests.
        /// If the value is 0, no buffer will be sent to Cognitive Services.
        /// </param>
        /// <return>return true if successful.
        /// </return>
        public IAsyncOperation<bool> StartContinuousRecording(ulong MaxStreamSizeInBytes, UInt16 ThresholdDuration, UInt16 ThresholdLevel)
        {
            return Task.Run<bool>(async () =>
            {
                thresholdDuration = ThresholdDuration;
            thresholdLevel = ThresholdLevel;
            bool bResult = false;
            maxStreamSizeInBytes = MaxStreamSizeInBytes;
            if (isRecordingInitialized != true)
                await InitializeRecording();
            if(STTStream != null)
            {
                STTStream.BufferReady -= STTStream_BufferReady;
                STTStream.AudioLevel -= STTStream_AudioLevel;
                STTStream.Dispose();
                STTStream = null;
            }
            STTStream = SpeechToTextMainStream.Create(maxStreamSizeInBytes, thresholdDuration, thresholdLevel);
            STTStream.AudioLevel += STTStream_AudioLevel;
            STTStream.BufferReady += STTStream_BufferReady;

            if ((STTStream != null) && (isRecordingInitialized == true))
            {
                try
                {
                    Windows.Media.MediaProperties.MediaEncodingProfile MEP = Windows.Media.MediaProperties.MediaEncodingProfile.CreateWav(Windows.Media.MediaProperties.AudioEncodingQuality.Auto);
                    if (MEP != null)
                    {
                        if (MEP.Audio != null)
                        {
                            uint framerate = 16000;
                            uint bitsPerSample = 16;
                            uint numChannels = 1;
                            uint bytespersecond = 32000;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_SAMPLES_PER_SECOND] = framerate;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_NUM_CHANNELS] = numChannels;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_BITS_PER_SAMPLE] = bitsPerSample;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_AVG_BYTES_PER_SECOND] = bytespersecond;
                            foreach (var Property in MEP.Audio.Properties)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: " + Property.Key.ToString());
                                System.Diagnostics.Debug.WriteLine("Value: " + Property.Value.ToString());
                                if (Property.Key == new Guid("5faeeae7-0290-4c31-9e8a-c534f68d9dba"))
                                    framerate = (uint)Property.Value;
                                if (Property.Key == new Guid("f2deb57f-40fa-4764-aa33-ed4f2d1ff669"))
                                    bitsPerSample = (uint)Property.Value;
                                if (Property.Key == new Guid("37e48bf5-645e-4c5b-89de-ada9e29b696a"))
                                    numChannels = (uint)Property.Value;

                            }
                        }
                        if (MEP.Container != null)
                        {
                            foreach (var Property in MEP.Container.Properties)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: " + Property.Key.ToString());
                                System.Diagnostics.Debug.WriteLine("Value: " + Property.Value.ToString());
                            }
                        }
                    }
                    await mediaCapture.StartRecordToStreamAsync(MEP, STTStream);
                    bResult = true;
                    isRecording = true;
                    System.Diagnostics.Debug.WriteLine("Recording in audio stream...");
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while recording in audio stream:" + e.Message);
                }
            }
            return bResult;
            }).AsAsyncOperation<bool>();
        }


        /// <summary>
        /// StopRecording method
        /// </summary>
        /// <param>
        /// Stop to record audio .
        /// The audio stream is still in stored in memory
        /// </param>
        /// <return>return true if successful.
        /// </return>
        public IAsyncOperation<bool> StopRecording()
        {
            return Task.Run<bool>(async () =>
            {
                // Stop recording and dispose resources
                if (mediaCapture != null)
            {
                if (isRecording == true)
                {
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;
                }
            }
            return true;
        }).AsAsyncOperation<bool>();
        }

        /// <summary>
        /// Cleans up the microphone resources and the stream and unregisters from MediaCapture events
        /// </summary>
        /// <returns>true if successful</returns>
        public IAsyncOperation<bool> CleanupRecording()
        {
            return Task.Run<bool>(async () =>
            {
                if (isRecordingInitialized)
            {
                // If a recording is in progress during cleanup, stop it to save the recording
                if (isRecording)
                {
                    await StopRecording();
                }
                isRecordingInitialized = false;
            }

            if (mediaCapture != null)
            {
                mediaCapture.RecordLimitationExceeded -= mediaCapture_RecordLimitationExceeded;
                mediaCapture.Failed -= mediaCapture_Failed;
                mediaCapture.Dispose();
                mediaCapture = null;
            }
            if (STTStream != null)
            {
                STTStream.BufferReady -= STTStream_BufferReady;
                STTStream.AudioLevel -= STTStream_AudioLevel;
                STTStream.Dispose();
                STTStream = null;
            }
            return true;
        }).AsAsyncOperation<bool>();
        }
        /// <summary>
        /// Event which returns the position of the buffer ready to be sent 
        /// This event is fired with continuous recording
        /// </summary>
        public event BufferReadyEventHandler BufferReady;
        
        /// <summary>
        /// Event which returns the Audio Level of the audio samples
        /// being stored in the audio buffer
        /// </summary>
        public event AudioLevelEventHandler AudioLevel;

        /// <summary>
        /// Event which returns the Audio Capture Errors while 
        /// a recording is in progress
        /// </summary>
        /// <returns>true if successful</returns>
        public event AudioCaptureErrorEventHandler AudioCaptureError;
        #region private
        private async System.Threading.Tasks.Task<bool> InitializeRecording()
        {
            isRecordingInitialized = false;
            try
            {
                // Initialize MediaCapture
                mediaCapture = new Windows.Media.Capture.MediaCapture();

                await mediaCapture.InitializeAsync(new Windows.Media.Capture.MediaCaptureInitializationSettings
                {
                    //VideoSource = screenCapture.VideoSource,
                    //      AudioSource = screenCapture.AudioSource,
                    StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio,
                    MediaCategory = Windows.Media.Capture.MediaCategory.Other,
                    AudioProcessing = Windows.Media.AudioProcessing.Raw

                });
                mediaCapture.RecordLimitationExceeded += mediaCapture_RecordLimitationExceeded;
                mediaCapture.Failed += mediaCapture_Failed;
                System.Diagnostics.Debug.WriteLine("Device Initialized Successfully...");
                isRecordingInitialized = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while initializing the device: " + e.Message);
            }
            return isRecordingInitialized;
        }
        async void mediaCapture_Failed(Windows.Media.Capture.MediaCapture sender, Windows.Media.Capture.MediaCaptureFailedEventArgs errorEventArgs)
        {
            System.Diagnostics.Debug.WriteLine("Fatal Error " + errorEventArgs.Message);
            await StopRecording();
            if (AudioCaptureError != null)
                AudioCaptureError(this, errorEventArgs.Message);
        }

        async void mediaCapture_RecordLimitationExceeded(Windows.Media.Capture.MediaCapture sender)
        {
            System.Diagnostics.Debug.WriteLine("Stopping Record on exceeding max record duration");
            await StopRecording();
            if (AudioCaptureError != null)
                AudioCaptureError(this, "Error Media Capture: Record Limitation Exceeded");
        }
        private  void STTStream_AudioLevel(object sender, double level)
        {
            //System.Diagnostics.Debug.WriteLine("STTStream_AmplitudeReading")
            if (AudioLevel != null)
                AudioLevel(this, level);
        }
        private void STTStream_BufferReady(object sender)
        {
            if (BufferReady != null)
                BufferReady(this);
        }


        private void ProgressHandler(Windows.Web.Http.HttpProgress progress)
        {
            System.Diagnostics.Debug.WriteLine("Http progress: " + progress.Stage.ToString() + " " + progress.BytesSent.ToString() + "/" + progress.TotalBytesToSend.ToString());
        }
        #endregion private
    }

}
