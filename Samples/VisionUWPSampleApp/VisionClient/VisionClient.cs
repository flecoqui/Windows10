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
using Windows.Web.Http.Headers;

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

        private const string VisionUrl = "https://{0}/vision/v1.0/analyze?visualFeatures={1}&details={2}&language={3}";
        private const string CustomVisionUrl = "https://{0}/customvision/v1.1/Prediction/{1}/image?iterationId={2}";


        /// <summary>
        /// class VisionClient constructor
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        public VisionClient()
        {

        }


        private void ProgressHandler(Windows.Web.Http.HttpProgress progress)
        {
            System.Diagnostics.Debug.WriteLine("Http progress: " + progress.Stage.ToString() + " " + progress.BytesSent.ToString() + "/" + progress.TotalBytesToSend.ToString());
        }
        /// <summary>
        /// SendVisionPicture method
        /// </summary>
        /// <param name="subscriptionKey">service key
        /// </param>
        /// <param name="hostname">hostname associated with the service url
        /// </param>
        /// <param name="visualFeatures">visual features for the request: tags, ...
        /// </param>
        /// <param name="details">detail information for the request:
        /// </param>
        /// <param name="lang">language for the response chinese or english so far
        /// </param>
        /// <param name="pictureFile">StorageFile associated with the picture file which 
        /// will be sent to the Vision Services.
        /// </param>
        /// <return>The result of the Vision REST API.
        /// </return>
        public IAsyncOperation<VisionResponse> SendVisionPicture(string subscriptionKey, string hostname, string visualFeatures, string details, string lang, Windows.Storage.StorageFile pictureFile)
        {
            return Task.Run<VisionResponse>(async () =>
            {
                VisionResponse r = null;

                try
                {
                       string visionUrl = string.Format(VisionUrl, hostname, visualFeatures, details, lang);
                        Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();

                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Ocp-Apim-Subscription-Key", subscriptionKey);
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "application/octet-stream");

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
                                content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/octet-stream");

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

            return r;
             }).AsAsyncOperation<VisionResponse>();
        }

        /// <summary>
        /// SendCustomVisionPicture method
        /// </summary>
        /// <param name="subscriptionKey">service key
        /// </param>
        /// <param name="hostname">hostname associated with the service url
        /// </param>
        /// <param name="visualFeatures">visual features for the request: tags, ...
        /// </param>
        /// <param name="details">detail information for the request:
        /// </param>
        /// <param name="lang">language for the response chinese or english so far
        /// </param>
        /// <param name="pictureFile">StorageFile associated with the picture file which 
        /// will be sent to the Vision Services.
        /// </param>
        /// <return>The result of the Vision REST API.
        /// </return>
        public IAsyncOperation<VisionResponse> SendCustomVisionPicture(string subscriptionKey, string hostname, string projectID, string iterationID, Windows.Storage.StorageFile pictureFile)
        {
            return Task.Run<VisionResponse>(async () =>
            {
                VisionResponse r = null;

                    try
                    {
                        string customVisionUrl = string.Format(CustomVisionUrl, hostname, projectID, iterationID);
                        //                        string visionUrl = string.Format(VisionUrl, hostname, visualFeatures);
                        Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();

                        hc.DefaultRequestHeaders.TryAppendWithoutValidation("Ocp-Apim-Subscription-Key", subscriptionKey);
                        hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "application/octet-stream");

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
                                    content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/octet-stream");

                                    hrm = await hc.PostAsync(new Uri(customVisionUrl), content).AsTask(cts.Token, progress);

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
                                        r = new VisionResponse(result, null);
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
                return r;
            }).AsAsyncOperation<VisionResponse>();
        }

    }

}
