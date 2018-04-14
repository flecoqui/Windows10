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

namespace VisionClient
{







    /// <summary>
    /// class VisionClient: Vision UWP Client
    /// </summary>
    /// <info>
    /// Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.
    /// </info>
    public sealed class VisionClient
    {
        private string SubscriptionKey;
        private string Token;

        private const string AuthUrl = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private const string VisionUrl = "https://{0}/vision/v1.0/analyze?visualFeatures={1}&details={2}&language={3}";

        /// <summary>
        /// class VisionClient constructor
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        public VisionClient()
        {
            SubscriptionKey = string.Empty;
            Token = string.Empty;
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
        /// <param name="subscriptionKey">SubscriptionKey associated with the Vision 
        /// Cognitive Service subscription.
        /// </param>
        /// <return>Token which is used for all calls to the Vision REST API.
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
        /// <return>Token which is used to all the calls to the Vision REST API.
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
        /// HasToken method
        /// </summary>
        /// <param>Check if a Token has been acquired
        /// </param>
        /// <return>true if a Token has been acquired to use the Vision REST API.
        /// </return>
        public bool HasToken()
        {
            if (string.IsNullOrEmpty(Token))
                return false;
            return true;
        }
        private void ProgressHandler(Windows.Web.Http.HttpProgress progress)
        {
            System.Diagnostics.Debug.WriteLine("Http progress: " + progress.Stage.ToString() + " " + progress.BytesSent.ToString() + "/" + progress.TotalBytesToSend.ToString());
        }
        /// <summary>
        /// SendStorageFile method
        /// </summary>
        /// <param name="wavFile">StorageFile associated with the audio file which 
        /// will be sent to the Vision Services.
        /// </param>
        /// <param name="locale">language associated with the current buffer/recording.
        /// for instance en-US, fr-FR, pt-BR, ...
        /// </param>
        /// <return>The result of the Vision REST API.
        /// </return>
        public  IAsyncOperation<VisionResponse> SendVisionPicture(string hostname, string visualFeatures, string details, string lang, Windows.Storage.StorageFile pictureFile)
        {
            return Task.Run<VisionResponse>(async () =>
            {
                VisionResponse r = null;
            int loop = 1;

            while (loop-- > 0)
            {
                try
                {
                    string visionUrl = string.Format(VisionUrl, hostname, visualFeatures, details, lang);
                    Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();

                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", Token);
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "application/octet-stream");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "application/json;text/xml");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Transfer-Encoding", "chunked");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Expect", "100-continue");
                    Windows.Web.Http.HttpResponseMessage hrm = null;

                    Windows.Storage.StorageFile file = pictureFile;
                    if (file != null)
                    {
                        using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                        {
                            if (fileStream != null)
                            {

                                Windows.Web.Http.HttpStreamContent content = new Windows.Web.Http.HttpStreamContent(fileStream.AsStream().AsInputStream());
                                System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                                IProgress<Windows.Web.Http.HttpProgress> progress = new Progress<Windows.Web.Http.HttpProgress>(ProgressHandler);
                                hrm = await hc.PostAsync(new Uri(visionUrl), content).AsTask(cts.Token, progress);
                                
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
                                    r = new VisionResponse(result,null);
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
                                r = new VisionResponse(string.Empty, HttpError);
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
             }).AsAsyncOperation<VisionResponse>();
        }


    }

}
