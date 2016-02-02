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
using AdaptiveMediaCache.SmoothStreaming;
using System.IO;
using System.Xml;
using Windows.Foundation.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using AdaptiveMediaCache.PlayReady;
namespace AdaptiveMediaCache
{
    [DataContract(Name ="ManifestCache")]
     class ManifestCache : IDisposable
    {

        public const ulong TimeUnit = 10000000;


        /// <summary>
        /// ManifestUri 
        /// Uri of the manifest associated with this asset.
        /// </summary>
        [DataMember]
        public Uri ManifestUri { get; private set; }
        /// <summary>
        /// RedirectUri 
        /// Redirect Uri if an http redirection is used on the server side.
        /// </summary>
        [DataMember]
        public Uri RedirectUri { get; private set; }
        /// <summary>
        /// DownloadToGo 
        /// if the value is true, the user could play this asset once the whole asset will be downloaded.When downloading the chunks, if the number of http error reach this value, the download thread is terminated.
        /// if the value is false, the user could play this asset once a percentage of the asset is downloaded.
        /// </summary>
        [DataMember]
        public bool DownloadToGo { get;  set; }

        /// <summary>
        /// MaxError 
        /// When downloading the chunks, if the number of http error reach this value, the download thread is terminated.
        /// </summary>
        [DataMember]
        public uint MaxError { get; set; }
        /// <summary>
        /// MaxMemoryBufferSize 
        /// When the amount of audio and video chunks in memory is over this value, they are stored on disk and removed from memory 
        /// </summary>
        [DataMember]
        public ulong MaxMemoryBufferSize { get; set; }
        /// <summary>
        /// IsLive 
        /// True if Live manifest (Download To go is not supported for Live streams) 
        /// </summary>
        [DataMember]
        public bool IsLive { get; private set; }
        /// <summary>
        /// BaseUrl 
        /// Base Url of the asset 
        /// </summary>
        [DataMember]
        public string BaseUrl { get; set; }
        /// <summary>
        /// RedirectBaseUrl 
        /// Redirect Base Url if a redirection is used on the server side 
        /// </summary>
        [DataMember]
        public string RedirectBaseUrl { get; set; }
        /// <summary>
        /// StoragePath 
        /// Folder name where the asset will be stored on disk  
        /// </summary>
        [DataMember]
        public string StoragePath { get; private set; }
        /// <summary>
        /// Duration 
        /// Duration of the asset (unit: 100 ns)  
        /// </summary>
        [DataMember]
        public ulong Duration { get; set; }
        /// <summary>
        /// TimeScale 
        /// TimeScale defined in the manifest  
        /// </summary>
        [DataMember]
        public ulong TimeScale { get; set; }
        /// <summary>
        /// AudioBitrate 
        /// Audio bitrate of the asset  
        /// </summary>
        [DataMember]
        public ulong AudioBitrate { get; set; }
        /// <summary>
        /// VideoBitrate 
        /// Video bitrate of the asset  
        /// </summary>
        [DataMember]
        public ulong VideoBitrate { get; set; }

        /// <summary>
        /// ExpectedMediaSize 
        /// The estimated asset size in bytes based on the duration, the audio bitrate and video bitrate  
        /// </summary>
        public ulong ExpectedMediaSize { get { return GetExpectedSize();  } }
        /// <summary>
        /// CurrentMediaSize 
        /// The number of audio and video bytes stored on disk 
        /// </summary>
        public ulong CurrentMediaSize { get { return AudioSavedBytes + VideoSavedBytes; } }

        /// <summary>
        /// AudioChunks 
        /// The number of audio chunks for this asset
        /// </summary>
        [DataMember]
        public ulong AudioChunks { get; set; }
        /// <summary>
        /// VideoChunks 
        /// The number of video chunks for this asset
        /// </summary>
        [DataMember]
        public ulong VideoChunks { get ; set; }
        /// <summary>
        /// AudioDownloadedChunks 
        /// The number of audio chunks downloaded
        /// </summary>
        [DataMember]
        public ulong AudioDownloadedChunks { get; set; }
        /// <summary>
        /// VideoDownloadedChunks 
        /// The number of video chunks downloaded
        /// </summary>
        [DataMember]
        public ulong VideoDownloadedChunks { get; set; }
        /// <summary>
        /// AudioSavedChunks 
        /// The number of audio chunks saved on disk
        /// </summary>
        [DataMember]
        public ulong AudioSavedChunks { get; set; }
        /// <summary>
        /// VideoSavedChunks 
        /// The number of video chunks saved on disk
        /// </summary>
        [DataMember]
        public ulong VideoSavedChunks { get; set; }
        /// <summary>
        /// AudioDownloadedBytes 
        /// The number of audio bytes downloaded
        /// </summary>
        [DataMember]
        public ulong AudioDownloadedBytes { get; set; }
        /// <summary>
        /// VideoDownloadedBytes 
        /// The number of video bytes downloaded
        /// </summary>
        [DataMember]
        public ulong VideoDownloadedBytes { get; set; }
        /// <summary>
        /// AudioSavedBytes 
        /// The number of audio bytes stored on disk
        /// </summary>
        [DataMember]
        public ulong AudioSavedBytes { get; set; }
        /// <summary>
        /// VideoSavedBytes 
        /// The number of video bytes stored on disk
        /// </summary>
        [DataMember]
        public ulong VideoSavedBytes { get; set; }
        /// <summary>
        /// AudioTemplateUrl 
        /// The Url template to download  the audio chunks
        /// </summary>
        [DataMember]
        public string AudioTemplateUrl { get; set; }
        /// <summary>
        /// AudioTemplateUrlType 
        /// Type in the Url template to download  the audio chunks
        /// </summary>
        [DataMember]
        public string AudioTemplateUrlType { get; set; }
        /// <summary>
        /// AudioChunkList 
        /// List of the audio chunk to download  
        /// </summary>
        [DataMember]
        public List<ChunkCache> AudioChunkList { get; set; }
        /// <summary>
        /// VideoTemplateUrl 
        /// The Url template to download  the video chunks
        /// </summary>
        [DataMember]
        public string VideoTemplateUrl { get; set; }
        /// <summary>
        /// VideoTemplateUrlType 
        /// Type in the Url template to download  the video chunks
        /// </summary>
        [DataMember]
        public string VideoTemplateUrlType { get; set; }
        /// <summary>
        /// VideoChunkList 
        /// List of the video chunk to download  
        /// </summary>
        [DataMember]
        public List<ChunkCache> VideoChunkList { get; set; }
        /// <summary>
        /// DownloadThreadStartTime 
        /// Download thread start time  
        /// </summary>
        [DataMember]
        public DateTime DownloadThreadStartTime { get; set; }
        /// <summary>
        /// DownloadThreadAudioCount 
        /// Number of audio chunks downloaded since the download thread is running 
        /// </summary>
        [DataMember]
        public ulong DownloadThreadAudioCount { get; set; }
        /// <summary>
        /// DownloadThreadVideoCount 
        /// Number of video chunks downloaded since the download thread is running 
        /// </summary>
        [DataMember]
        public ulong DownloadThreadVideoCount { get; set; }
        /// <summary>
        /// manifestBuffer 
        /// Buffer where the manifest is stored 
        /// </summary>
        [DataMember]
        public byte[] manifestBuffer{ get; set; }
        /// <summary>
        /// Get Protection Guid.
        /// </summary>
        [DataMember]
        public Guid ProtectionGuid { get; protected set; }
        /// <summary>
        /// Get Protection Data.
        /// </summary>
        [DataMember]
        public string ProtectionData { get; protected set; }
        /// <summary>
        /// IsPlayReadyLicenseAcquired 
        /// true if PlayReady License has been acquired
        /// </summary>
        [DataMember]
        public bool IsPlayReadyLicenseAcquired { get; set; }
        /// <summary>
        /// MinBitrate 
        /// Minimum video bitrate of the video track to select 
        /// </summary>
        [DataMember]
        public ulong MinBitrate { get; set; }
        /// <summary>
        /// MaxBitrate 
        /// Maximum video bitrate of the video track to select 
        /// </summary>
        [DataMember]
        public ulong MaxBitrate { get; set; }

        /// <summary>
        /// SelectVideoTrackIndex 
        /// Index of the video track to select 
        /// </summary>
        [DataMember]
        public int SelectVideoTrackIndex { get; protected set; }
        /// <summary>
        /// SelectAudioTrackIndex 
        /// Index of the audio track to select 
        /// </summary>
        [DataMember]
        public int SelectAudioTrackIndex { get; protected set; }
        /// <summary>
        /// SelectedVideoTrackIndex 
        /// Index of the video track selected
        /// </summary>
        [DataMember]
        public int SelectedVideoTrackIndex { get; protected set; }
        /// <summary>
        /// SelectedAudioTrackIndex 
        /// Index of the audio track selected 
        /// </summary>
        [DataMember]
        public int SelectedAudioTrackIndex { get; protected set; }

        private const string audioString = "audio";
        private const string videoString = "video";
        private List<AudioTrack> ListAudioTracks;
        private List<VideoTrack> ListVideoTracks;
        private AssetStatus mStatus;
        private DiskCache DiskCache = null;
        private System.Threading.Tasks.Task downloadTask;
        private System.Threading.CancellationTokenSource downloadTaskCancellationtoken;
        private bool downloadTaskRunning = false;


        /// <summary>
        /// Initialize 
        /// Initialize the Manifest Cache parameters 
        /// </summary>
        private void Initialize()
        {
            DownloadToGo = false;
            ManifestUri = null;
            RedirectUri = null;
            StoragePath = string.Empty;
            MinBitrate = 0;
            MaxBitrate = 0;
            MaxError = 20;
            MaxMemoryBufferSize = 256000;
            VideoChunkList = new List<ChunkCache>();
            AudioChunkList = new List<ChunkCache>();
            AudioDownloadedBytes = 0;
            AudioDownloadedChunks = 0;
            AudioSavedBytes = 0;
            VideoDownloadedBytes = 0;
            VideoDownloadedChunks = 0;
            VideoSavedBytes = 0;
            AudioTemplateUrl = string.Empty;
            VideoTemplateUrl = string.Empty;

            BaseUrl = string.Empty;
            RedirectBaseUrl = string.Empty;

            ListAudioTracks = new List<AudioTrack>();
            ListVideoTracks = new List<VideoTrack>();
            SelectVideoTrackIndex = -1;
            SelectAudioTrackIndex = -1;
            SelectedVideoTrackIndex = -1;
            SelectedAudioTrackIndex = -1;

            DownloadedPercentage = 0;
            VideoTemplateUrlType = videoString;
            AudioTemplateUrlType = audioString;
            IsPlayReadyLicenseAcquired = false;
            mStatus = AssetStatus.Initialized;

        }
        /// <summary>
        /// ManifestCache 
        /// ManifestCache contructor 
        /// </summary>
        public ManifestCache() {
            Initialize();
        }
        /// <summary>
        /// ManifestCache 
        /// ManifestCache contructor 
        /// </summary>
        public ManifestCache(Uri manifestUri, bool downloaddToGo, ulong minBitrate, ulong maxBitrate, int AudioIndex, ulong maxMemoryBufferSize, uint maxError)
        {
            Initialize();
            ManifestUri = manifestUri;
            StoragePath = ComputeHash(ManifestUri.AbsoluteUri.ToLower());
            DownloadToGo = downloaddToGo;
            MinBitrate = minBitrate;
            MaxBitrate = maxBitrate;
            MaxMemoryBufferSize = maxMemoryBufferSize;
            MaxError = maxError;
            SelectVideoTrackIndex = -1;
            SelectAudioTrackIndex = AudioIndex;
        }
        /// <summary>
        /// ManifestCache 
        /// ManifestCache contructor 
        /// </summary>
        public ManifestCache(Uri manifestUri, bool downloaddToGo, int VideoIndex, int AudioIndex, ulong maxMemoryBufferSize, uint maxError)
        {
            Initialize();
            ManifestUri = manifestUri;
            StoragePath = ComputeHash(ManifestUri.AbsoluteUri.ToLower());
            DownloadToGo = downloaddToGo;
            MinBitrate = 0;
            MaxBitrate = 0;
            MaxMemoryBufferSize = maxMemoryBufferSize;
            MaxError = maxError;
            SelectVideoTrackIndex = VideoIndex;
            SelectAudioTrackIndex = AudioIndex;
        }
        /// <summary>
        /// Convert manifest timescale times to HNS for reporting
        /// </summary>
        /// <param name="tsTime">time in timescale units</param>
        /// <returns>time in HNS units</returns>
        public ulong TimescaleToHNS(ulong tsTime)
        {
            ulong hnsTime = tsTime;
            if (TimeScale != TimeUnit)
            {
                double scale = TimeUnit / TimeScale;
                hnsTime = (ulong)(tsTime * scale);
            }
            return hnsTime;
        }
        /// <summary>
        /// RestoreStatus
        /// Restore the manifest status based on the chunks downloaded or saved on disk
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if success</returns>
        public bool RestoreStatus()
        {
            mStatus = AssetStatus.Initialized;
            if((this.VideoBitrate>0)&&
                (this.AudioBitrate > 0))
                mStatus = AssetStatus.ManifestDownloaded;
            if ((this.VideoSavedChunks > 0) ||
                (this.AudioSavedChunks > 0))
                mStatus = AssetStatus.DownloadingChunks;
            if ((this.VideoSavedChunks == this.VideoChunks) &&
                (this.AudioSavedChunks == this.AudioChunks))
                mStatus = AssetStatus.ChunksDownloaded;            
            return true;
        }
        /// <summary>
        /// IsAssetProtected
        /// Return true if the asset is protected with PlayReady
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if protected</returns>
        public bool IsAssetProtected()
        {
            if (!string.IsNullOrEmpty(this.ProtectionData) &&
                (this.ProtectionGuid != Guid.Empty))
                return true;
            return false;
        }

        /// <summary>
        /// GetPlayReadyExpirationDate
        /// Return PlayReady Expiration date (return DateTimeOffset.MinValue if not available)
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if protected</returns>
        public DateTimeOffset GetPlayReadyExpirationDate()
        {
            DateTimeOffset d = DateTimeOffset.MinValue;
            if (!string.IsNullOrEmpty(this.ProtectionData) &&
                (this.ProtectionGuid != Guid.Empty))
            {
                Uri DefaultLicenseAcquistionUri;
                Guid DefaultContentKeyId;
                string DefaultContentKeyIdString;
                Guid DefaultDomainServiceId;

                if (GetPlayReadyParameters(this.ProtectionData, out DefaultLicenseAcquistionUri, out DefaultContentKeyId, out DefaultContentKeyIdString, out DefaultDomainServiceId))
                {
                    var keyIdString = Convert.ToBase64String(DefaultContentKeyId.ToByteArray());
                    try
                    {
                        var contentHeader = new Windows.Media.Protection.PlayReady.PlayReadyContentHeader(
                            DefaultContentKeyId,
                            keyIdString,
                            Windows.Media.Protection.PlayReady.PlayReadyEncryptionAlgorithm.Aes128Ctr,
                            null,
                            null,
                            string.Empty,
                            new Guid());
                        Windows.Media.Protection.PlayReady.IPlayReadyLicense[] licenses = new Windows.Media.Protection.PlayReady.PlayReadyLicenseIterable(contentHeader, true).ToArray();
                        foreach (var lic in licenses)
                        {
                            DateTimeOffset? dto = MediaHelpers.PlayReadyHelper.GetLicenseExpirationDate(lic);
                            if ((dto != null) && (dto.HasValue))
                                return dto.Value.DateTime;
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("GetPlayReadyExpirationDate Exception: " + e.Message);
                        return DateTimeOffset.MinValue;
                    }
                } 
                return d;
            }
            return d;
        }
        /// <summary>
        /// Get KID and LicenseAcquisition Uri from the Protection Header
        /// </summary>
        private bool GetPlayReadyParameters(string ProtectionData, out Uri LicenseAcquisitionUri, out Guid ContentKeyId, out string ContentKeyIdString, out Guid DomainServiceId)
        {
            byte[] protectionData = System.Convert.FromBase64String(ProtectionData);
            LicenseAcquisitionUri = null;
            ContentKeyId = Guid.Empty;
            ContentKeyIdString = String.Empty;
            DomainServiceId = Guid.Empty;
            if ((ProtectionData != null))
            {
                if (ProtectionData.Length > 10)
                {
                    System.Text.UnicodeEncoding UTF16encoding = new System.Text.UnicodeEncoding();
                    string xmlstring = UTF16encoding.GetString(protectionData, 10, protectionData.Length - 10);
                    XmlReader xmlReader = XmlReader.Create(new StringReader(UTF16encoding.GetString(protectionData, 10, protectionData.Length - 10)));
                    while (xmlReader.Read())
                    {
                        if ((xmlReader.Name == "WRMHEADER") && (xmlReader.NodeType == XmlNodeType.EndElement))
                            return true;
                        if ((xmlReader.Name == "KID") &&
                                    (xmlReader.NodeType == XmlNodeType.Element))
                        {
                            string s = xmlReader.ReadElementContentAsString();
                            ContentKeyIdString = s.Trim();
                            byte[] base64EncodedBytes = System.Convert.FromBase64String(ContentKeyIdString);
                            if (base64EncodedBytes.Length == 16)
                            {
                                ContentKeyId = new Guid(base64EncodedBytes);
                            }
                        }
                        if ((xmlReader.Name == "LA_URL") &&
                                    (xmlReader.NodeType == XmlNodeType.Element))
                        {
                            string s = xmlReader.ReadElementContentAsString();
                            s = s.Trim();
                            LicenseAcquisitionUri = new Uri(s);
                        }
                        if ((xmlReader.Name == "DS_ID") &&
                                    (xmlReader.NodeType == XmlNodeType.Element))
                        {
                            string s = xmlReader.ReadElementContentAsString();
                            s = s.Trim();
                            byte[] base64EncodedBytes = System.Convert.FromBase64String(s);
                            if (base64EncodedBytes.Length == 16)
                            {
                                DomainServiceId = new Guid(base64EncodedBytes);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public const int MSPR_E_CONTENT_ENABLING_ACTION_REQUIRED = -2147174251;
        public const int DRM_E_NOMORE_DATA = -2147024637; //( 0x80070103 )
        public const int MSPR_E_NEEDS_INDIVIDUALIZATION = -2147174366; // (0x8004B822)
        /// <summary>
        /// Proactive Individualization Request 
        /// </summary>
        async Task<bool> ProActiveIndivRequest()
        {
            Windows.Media.Protection.PlayReady.PlayReadyIndividualizationServiceRequest indivRequest = new Windows.Media.Protection.PlayReady.PlayReadyIndividualizationServiceRequest();
            bool bResultIndiv = await ReactiveIndivRequest(indivRequest, null);
            return bResultIndiv;

        }
        /// <summary>
        /// Invoked to send the Individualization Request 
        /// </summary>
        async System.Threading.Tasks.Task<bool> ReactiveIndivRequest(Windows.Media.Protection.PlayReady.PlayReadyIndividualizationServiceRequest IndivRequest, Windows.Media.Protection.MediaProtectionServiceCompletion CompletionNotifier)
        {
            bool bResult = false;
            Exception exception = null;
            try
            {
                await IndivRequest.BeginServiceRequest();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (exception == null)
                {
                    bResult = true;
                }
                else
                {
                    System.Runtime.InteropServices.COMException comException = exception as System.Runtime.InteropServices.COMException;
                    if (comException != null && comException.HResult == MSPR_E_CONTENT_ENABLING_ACTION_REQUIRED)
                    {
                        IndivRequest.NextServiceRequest();
                    }
                }
            }
            if (bResult == true)
            if (CompletionNotifier != null) CompletionNotifier.Complete(bResult);
            return bResult;
        }
        /// <summary>
        /// GetCachePlayReadyLicense
        /// Return true if get the PlayReady license successfully 
        /// </summary>
        /// <param name="PlayReadyLicenseUri">PlayReady server Uri</param>
        /// <param name="PlayReadyChallengeCustomData">PlayReady custom Data</param>
        /// <param name="DefaultContentKeyId">Content KeyId</param>
        /// <param name="DefaultContentKeyIdString">Content KeyId string </param>
        /// <param name="DefaultDomainServiceId">Domain Service Id </param>
        /// <returns>true if license acquired</returns>
        protected async Task<bool> GetCachePlayReadyLicense(Uri PlayReadyLicenseUri, string PlayReadyChallengeCustomData, Guid DefaultContentKeyId, string DefaultContentKeyIdString, Guid DefaultDomainServiceId )
        {
            bool bResult = false;
            Windows.Media.Protection.PlayReady.PlayReadyLicenseAcquisitionServiceRequest licenseRequest = new Windows.Media.Protection.PlayReady.PlayReadyLicenseAcquisitionServiceRequest();
            if (licenseRequest != null)
            {
                if (!string.IsNullOrEmpty(PlayReadyChallengeCustomData))
                {
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    byte[] b = encoding.GetBytes(PlayReadyChallengeCustomData);
                    licenseRequest.ChallengeCustomData = Convert.ToBase64String(b, 0, b.Length);
                }
                licenseRequest.ContentHeader = new Windows.Media.Protection.PlayReady.PlayReadyContentHeader(
                                            DefaultContentKeyId,
                                            DefaultContentKeyIdString,
                                            Windows.Media.Protection.PlayReady.PlayReadyEncryptionAlgorithm.Aes128Ctr,
                                            PlayReadyLicenseUri,
                                            null,
                                            String.Empty,
                                            DefaultDomainServiceId);
                Windows.Media.Protection.PlayReady.PlayReadySoapMessage soapMessage = licenseRequest.GenerateManualEnablingChallenge();

                byte[] messageBytes = soapMessage.GetMessageBody();
                Windows.Web.Http.IHttpContent httpContent = new Windows.Web.Http.HttpBufferContent(messageBytes.AsBuffer());
                IPropertySet propertySetHeaders = soapMessage.MessageHeaders;

                foreach (string strHeaderName in propertySetHeaders.Keys)
                {
                    string strHeaderValue = propertySetHeaders[strHeaderName].ToString();

                    // The Add method throws an ArgumentException try to set protected headers like "Content-Type"
                    // so set it via "ContentType" property
                    if (strHeaderName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        httpContent.Headers.ContentType = Windows.Web.Http.Headers.HttpMediaTypeHeaderValue.Parse(strHeaderValue);
                    else
                        httpContent.Headers.TryAppendWithoutValidation(strHeaderName.ToString(), strHeaderValue);
                }

                CommonLicenseRequest licenseAcquision = new CommonLicenseRequest();
                Windows.Web.Http.IHttpContent responseHttpContent = await licenseAcquision.AcquireLicense(PlayReadyLicenseUri, httpContent);

                if (responseHttpContent != null)
                {
                    var buffer = await responseHttpContent.ReadAsBufferAsync();
                    Exception exResult = licenseRequest.ProcessManualEnablingResponse(buffer.ToArray());
                    if (exResult == null)
                    {
                        bResult = true;
                    }
                    else
                        throw exResult;
                }
            }
            return bResult;
        }
        public AssetStatus GetAssetStatus()
        {
            return mStatus;
        }
        /// <summary>
        /// Get PlayReady license associated with this manifest
        /// </summary>
        /// <param name="manifestUri">Uri to check for</param>
        /// <returns>true if the license is downloaded</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.IO.IsolatedStorage.IsolatedStorageException"></exception>
        public async Task<bool> GetPlayReadyLicense(Uri PlayReadyLicenseUri, string PlayReadyChallengeCustomData)
        {
            bool bResult = false;
            IsPlayReadyLicenseAcquired = false;
            if ((this.ProtectionGuid != Guid.Empty) && (!string.IsNullOrEmpty(this.ProtectionData)))
            {
                int AttemptCount = 2;
                while ((AttemptCount-- > 0)&&(bResult == false))
                {
                    try
                    {
                        Uri DefaultLicenseAcquistionUri;
                        Guid DefaultContentKeyId;
                        string DefaultContentKeyIdString;
                        Guid DefaultDomainServiceId;

                        if (GetPlayReadyParameters(this.ProtectionData, out DefaultLicenseAcquistionUri, out DefaultContentKeyId, out DefaultContentKeyIdString, out DefaultDomainServiceId))
                        {
                            if (PlayReadyLicenseUri == null)
                                PlayReadyLicenseUri = DefaultLicenseAcquistionUri;

                            bResult = await GetCachePlayReadyLicense(PlayReadyLicenseUri, PlayReadyChallengeCustomData, DefaultContentKeyId, DefaultContentKeyIdString, DefaultDomainServiceId);
                            if(bResult== true)
                                IsPlayReadyLicenseAcquired = true;
                        }
                    }
                    catch (Exception e)
                    {
                        bResult = false;
                        System.Diagnostics.Debug.WriteLine("GetPlayReadyLicense Exception: " + e.Message);
                        if (e.HResult == MSPR_E_NEEDS_INDIVIDUALIZATION)
                        {
                            System.Diagnostics.Debug.WriteLine("GetPlayReadyLicense Individualisation required ");
                            if (await ProActiveIndivRequest() == true)
                                // Let's try to get the license again
                                System.Diagnostics.Debug.WriteLine("GetPlayReadyLicense Individualisation successfull");
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("GetPlayReadyLicense Individualisation failed");
                                break;
                            }
                        }
                        else
                            break;
                    }
                }
            }
            return bResult;
        }
        /// <summary>
        /// DownloadManifestAsync
        /// Downloads a manifest asynchronously.
        /// </summary>
        /// <param name="forceNewDownload">Specifies whether to force a new download and avoid cached results.</param>
        /// <returns>A byte array</returns>
        public async Task<byte[]> DownloadManifestAsync(bool forceNewDownload)
        {
            Uri manifestUri = this.ManifestUri;
            System.Diagnostics.Debug.WriteLine("Download Manifest: " + manifestUri.ToString() + " start at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now));

            var client = new Windows.Web.Http.HttpClient();
            try
            {
                if (forceNewDownload)
                {
                    string modifier = manifestUri.AbsoluteUri.Contains("?") ? "&" : "?";
                    string newUriString = string.Concat(manifestUri.AbsoluteUri, modifier, "ignore=", Guid.NewGuid());
                    manifestUri = new Uri(newUriString);
                }

                Windows.Web.Http.HttpResponseMessage response = await client.GetAsync(manifestUri, Windows.Web.Http.HttpCompletionOption.ResponseContentRead);

                response.EnsureSuccessStatusCode();
                /*
                foreach ( var v in response.Content.Headers)
                {
                    System.Diagnostics.Debug.WriteLine("Content Header key: " + v.Key + " value: " + v.Value.ToString());
                }
                foreach (var v in response.Headers)
                {
                    System.Diagnostics.Debug.WriteLine("Header key: " + v.Key + " value: " + v.Value.ToString());
                }
                */
                var buffer = await response.Content.ReadAsBufferAsync();
                if (buffer != null)
                {

                    if ((response.Headers.Location != null) && (response.Headers.Location != manifestUri))
                    {
                        this.RedirectUri = response.Headers.Location;
                        this.RedirectBaseUrl = GetBaseUri(RedirectUri.AbsoluteUri);
                    }
                    else
                    {
                        this.RedirectBaseUrl = string.Empty;
                        this.RedirectUri = null;
                    }
                    this.BaseUrl = GetBaseUri(manifestUri.AbsoluteUri);

                    uint val = buffer.Length;
                    System.Diagnostics.Debug.WriteLine("Download " + val.ToString() + " Bytes Manifest: " + manifestUri.ToString() + " done at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now));
                    return buffer.ToArray();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// ParseDashManifest
        /// Parse DASH manifest (not implemented).
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>
        public async Task<bool> ParseDashManifest()
        {
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
            }
            return false;
        }
        /// <summary>
        /// ParseHLSManifest
        /// Parse HLS manifest (not implemented).
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>
        public async Task<bool> ParseHLSManifest()
        {
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
            }
            return false;
        }
        /// <summary>
        /// GetBaseUri
        /// Get Base Uri of the input source url.
        /// </summary>
        /// <param name="source">Source url</param>
        /// <returns>string base Uri</returns>
        public static string GetBaseUri(string source)
        {
            int manitestPosition = source.LastIndexOf(@"/manifest",StringComparison.OrdinalIgnoreCase);
            if (manitestPosition < 0)
                manitestPosition = source.LastIndexOf(@"/qualitylevels",StringComparison.OrdinalIgnoreCase);
            return manitestPosition < 0 ?
                                source :
                                source.Substring(0, manitestPosition);
        }
        /// <summary>
        /// GetType
        /// Get Type from the url template.
        /// </summary>
        /// <param name="source">Source url</param>
        /// <returns>string Type included in the source url</returns>
        private string GetType(string Template)
        {
            string[] url = Template.ToLower().Split('/');

            string type = string.Empty;
            try
            {
                if (Template.ToLower().Contains("/fragments("))
                {
                    //url = "fragments(audio=0)"
                    string[] keys = { "(", "=", ")" };
                    url = url[url.Length - 1].Split(keys, StringSplitOptions.RemoveEmptyEntries);

                    type = url[url.Length - 2];
                }
            }
            catch (Exception)
            {
            }

            return type;
        }
        /// <summary>
        /// GetAudioTracks
        /// Get the audio tracks .
        /// </summary>
        /// <param name=""></param>
        /// <returns>List of audio tracks</returns>
        public IReadOnlyList<AudioTrack> GetAudioTracks()
        {
            return ListAudioTracks;
        }
        /// <summary>
        /// GetVideoTracks
        /// Get the video tracks .
        /// </summary>
        /// <param name=""></param>
        /// <returns>List of video tracks</returns>
        public IReadOnlyList<VideoTrack> GetVideoTracks()
        {
            return ListVideoTracks ;
        }

        /// <summary>
        /// InitializeChunks
        /// Initialize the chunks parameter based on the information in the manifest.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="selected"></param>
        /// <returns>true if success</returns>
        bool InitializeChunks(StreamInfo stream, QualityLevel selected)
        {
            ulong Bitrate = 0;
            string UrlTemplate = string.Empty;
            if ((selected != null)&&(stream!= null))
            {
                selected.TryGetAttributeValueAsUlong("Bitrate", out Bitrate);
                if (Bitrate > 0)
                {
                    if (stream.TryGetAttributeValueAsString("Url", out UrlTemplate))
                    {
                        UrlTemplate = UrlTemplate.Replace("{bitrate}", Bitrate.ToString());
                        UrlTemplate = UrlTemplate.Replace("{start time}", "{start_time}");
                        UrlTemplate = UrlTemplate.Replace("{CustomAttributes}", "timeScale=10000000");
                        bool bAudio = false;
                        bool bVideo = false;
                        if (stream.StreamType.ToLower() == "audio")
                        {
                            ulong index = 0;
                            if (SelectAudioTrackIndex < 0)
                            {
                                selected.TryGetAttributeValueAsUlong("Index", out index);
                                SelectedAudioTrackIndex = (int)index;
                            }
                            else
                                SelectedAudioTrackIndex = SelectAudioTrackIndex;
                            bAudio = true;
                        }
                        if (stream.StreamType.ToLower() == "video")
                        {
                            ulong index = 0;
                            if (SelectVideoTrackIndex < 0)
                            {
                                selected.TryGetAttributeValueAsUlong("Index", out index);
                                SelectedVideoTrackIndex = (int)index;
                            }
                            else
                                SelectedVideoTrackIndex = SelectVideoTrackIndex;
                            bVideo = true;
                        }
                        if ((bAudio) || (bVideo))
                        {
                            UInt64 time = 0;
                            ulong duration = 0;
                            foreach (var chunk in stream.Chunks)
                            {
                                if (chunk.Duration != null)
                                    duration = (ulong)chunk.Duration;
                                else
                                    duration = 0;
                                if (chunk.Time != null)
                                    time = (UInt64)chunk.Time;

                                ChunkCache cc = new ChunkCache(time, duration);
                                time += (ulong)duration;
                                if (cc != null)
                                {
                                    if (bAudio)
                                        this.AudioChunkList.Add(cc);
                                    if (bVideo)
                                        this.VideoChunkList.Add(cc);
                                }
                            }
                            if (bAudio)
                            {
                                this.AudioBitrate = Bitrate;
                                this.AudioChunks = (ulong)this.AudioChunkList.Count;
                                this.AudioTemplateUrl = UrlTemplate;
                                this.AudioTemplateUrlType = GetType(UrlTemplate);
                            }
                            if (bVideo)
                            {
                                this.VideoBitrate = Bitrate;
                                this.VideoChunks = (ulong)this.VideoChunkList.Count;
                                this.VideoTemplateUrl = UrlTemplate;
                                this.VideoTemplateUrlType = GetType(UrlTemplate);
                            }
                        }
                        return true;
                    };
                }

            }
            return false;
        }
        /// <summary>
        /// ParseSmoothManifest
        /// Parse Smooth Streaming manifest 
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>
        private async Task<bool> ParseSmoothManifest()
        {
            bool bResult = false;
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
                try
                { 
                SmoothStreamingManifest parser = new SmoothStreamingManifest(manifestBuffer);
                    if ((parser != null) && (parser.ManifestInfo != null))
                    {
                        ManifestInfo mi = parser.ManifestInfo;

                        ulong Duration = mi.ManifestDuration;
                        string UrlTemplate = string.Empty;

                        this.Duration = Duration;
                        this.TimeScale = mi.Timescale;
                        this.IsLive = mi.IsLive;



                        this.ProtectionGuid = mi.ProtectionGuid;
                        this.ProtectionData = mi.ProtectionData;

                        // We don't support multiple audio. Therefore, we need to download the first audio track. 

                        ListVideoTracks.Clear();
                        ListAudioTracks.Clear();
                        int audioIndex = 0;

                        QualityLevel audioselected = null;
                        QualityLevel videoselected = null;
                        StreamInfo audiostream = null;
                        StreamInfo videostream = null;

                        foreach (StreamInfo stream in mi.Streams)
                        {
                            if (stream.StreamType.ToUpper() == "VIDEO")
                            {

                                ulong Dummy = 0;

                                foreach (QualityLevel track in stream.QualityLevels)
                                {
                                    ulong currentBitrate = 0;
                                    ulong currentIndex = 0;
                                    string currentFourCC = string.Empty;
                                    ulong currentWidth = 0;
                                    ulong currentHeight = 0;
                                    track.TryGetAttributeValueAsUlong("Index", out currentIndex);
                                    track.TryGetAttributeValueAsUlong("Bitrate", out currentBitrate);
                                    if (!track.TryGetAttributeValueAsString("FourCC", out currentFourCC))
                                        currentFourCC = string.Empty;
                                    track.TryGetAttributeValueAsUlong("MaxWidth", out currentWidth);
                                    track.TryGetAttributeValueAsUlong("MaxHeight", out currentHeight);
                                    ListVideoTracks.Add(new VideoTrack
                                    {
                                        Index = (int)currentIndex,
                                        Bitrate = (int)currentBitrate,
                                        FourCC = currentFourCC,
                                        MaxHeight = (int)currentHeight,
                                        MaxWidth = (int)currentWidth
                                    });
                                    if (SelectVideoTrackIndex == -1)
                                    {
                                        if (((this.MinBitrate == 0) || (currentBitrate >= (ulong)this.MinBitrate)) &&
                                            ((this.MaxBitrate == 0) || (currentBitrate <= (ulong)this.MaxBitrate)))
                                        {
                                            if (videoselected != null)
                                            {
                                                if (currentBitrate > (ulong)(videoselected.TryGetAttributeValueAsUlong("Bitrate", out Dummy) ? Dummy : 0))
                                                {
                                                    videoselected = track;
                                                    videostream = stream;
                                                }
                                            }
                                            else
                                            {
                                                videoselected = track;
                                                videostream = stream;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (SelectVideoTrackIndex == (int)currentIndex)
                                        {
                                            videoselected = track;
                                            videostream = stream;
                                        }
                                    }

                                }
                                if (videoselected == null)
                                {
                                    videoselected = stream.QualityLevels.First();
                                    videostream = stream;
                                }
                            }
                            if (stream.StreamType.ToUpper() == "AUDIO")
                            {
                                if (audioIndex == 0)
                                {
                                    audioselected = stream.QualityLevels.First();
                                    audiostream = stream;
                                }
                                foreach (QualityLevel track in stream.QualityLevels)
                                {
                                    ulong currentBitrate = 0;
                                    string currentFourCC = string.Empty;
                                    track.TryGetAttributeValueAsUlong("Bitrate", out currentBitrate);
                                    if (!track.TryGetAttributeValueAsString("FourCC", out currentFourCC))
                                        currentFourCC = string.Empty;
                                    ListAudioTracks.Add(new AudioTrack
                                    {
                                        Index = (int)audioIndex,
                                        Bitrate = (int)currentBitrate,
                                        FourCC = currentFourCC,
                                    });
                                    if ((SelectAudioTrackIndex != -1) &&
                                         ((int)audioIndex++ == SelectAudioTrackIndex))
                                    {
                                        audioselected = track;
                                        audiostream = stream;
                                    }
                                }

                            }
                        }
                        VideoChunkList.Clear();
                        AudioChunkList.Clear();
                        InitializeChunks(videostream, videoselected);
                        InitializeChunks(audiostream, audioselected);
                        if (this.manifestBuffer == null)
                            this.manifestBuffer = manifestBuffer;
                        bResult = true;
                    }                
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Exception while parsing Smooth Streaming manifest : " + ex.Message);
                    bResult = false;
                }
            }
            return bResult;
        }
        /// <summary>
        /// DownloadManifest
        /// Download and parse the manifest  
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>        
        public async Task<bool> DownloadManifest()
        {
            bool bResult = false;
            // load the stream associated with the HLS, SMOOTH or DASH manifest
            this.ManifestStatus = AssetStatus.DownloadingManifest;
            bResult = await this.ParseSmoothManifest();
            if (bResult != true)
            {
                bResult = await this.ParseDashManifest();
                if (bResult != true)
                {
                    bResult = await this.ParseHLSManifest();
                }
            }
            if (bResult == true)
                this.ManifestStatus = AssetStatus.ManifestDownloaded;
            else
                this.ManifestStatus = AssetStatus.ErrorManifestDownload;
            return bResult;
        }
        /// <summary>
        /// SetDiskCache
        /// Associate a Disk cache with the manifest.
        /// The DiskCache will be used to store the manifest on disk
        /// </summary>
        /// <param name="cache">DiskCache</param>
        /// <returns>true if successful</returns>
        public bool SetDiskCache(DiskCache cache)
        {
            if (cache != null)
            {
                DiskCache = cache;
                return true;
            }
            return false;
        }

        /// <summary>
        /// CreateManifestCache
        /// Create a manifest cache.
        /// 
        /// </summary>
        /// <param name="manifestUri">manifest Uri</param>
        /// <param name="downloadToGo">true if Download To Go experience, Progressive Downlaoad experience  otherwise</param>
        /// <param name="minBitrate">mininmum bitrate for the video track</param>
        /// <param name="maxBitrate">maximum bitrate for the video track</param>
        /// <param name="audioIndex">Index of the audio track to select (usually 0)</param>
        /// <param name="maxMemoryBufferSize">maximum memory buffer size</param>
        /// <param name="maxError">maximum http error while downloading the chunks</param>
        /// <returns>return a ManifestCache (null if not successfull)</returns>
        public static ManifestCache CreateManifestCache(Uri manifestUri, bool downloadToGo, ulong minBitrate, ulong maxBitrate, int audioIndex, ulong maxMemoryBufferSize, uint maxError)
        {
            // load the stream associated with the HLS, SMOOTH or DASH manifest
            ManifestCache mc = new ManifestCache(manifestUri, downloadToGo, minBitrate, maxBitrate, audioIndex, maxMemoryBufferSize, maxError);
            if (mc != null)
            {
                mc.ManifestStatus = AssetStatus.Initialized;
            }
            return mc;
        }
        /// <summary>
        /// CreateManifestCache
        /// Create a manifest cache.
        /// 
        /// </summary>
        /// <param name="manifestUri">manifest Uri</param>
        /// <param name="downloadToGo">true if Download To Go experience, Progressive Downlaoad experience  otherwise</param>
        /// <param name="audioIndex">Index of the video track to select </param>
        /// <param name="audioIndex">Index of the audio track to select (usually 0)</param>
        /// <param name="maxMemoryBufferSize">maximum memory buffer size</param>
        /// <param name="maxError">maximum http error while downloading the chunks</param>
        /// <returns>return a ManifestCache (null if not successfull)</returns>
        public static ManifestCache CreateManifestCache(Uri manifestUri, bool downloadToGo, int videoIndex, int audioIndex, ulong maxMemoryBufferSize, uint maxError)
        {
            // load the stream associated with the HLS, SMOOTH or DASH manifest
            ManifestCache mc = new ManifestCache(manifestUri, downloadToGo, videoIndex, audioIndex, maxMemoryBufferSize, maxError);
            if (mc != null)
            {
                mc.ManifestStatus = AssetStatus.Initialized;
            }
            return mc;
        }
        /// <summary>
        /// ComputeHash
        /// Convert the manifest Uri into a unique string which will be the folder name where the asset will be stored.
        /// </summary>
        /// <param name="message">string to hash</param>
        /// <returns>string</returns>
        private string ComputeHash(string message)
        {
            // Convert the message string to binary data.
            Windows.Storage.Streams.IBuffer buffUtf8Msg = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(message, Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
            Windows.Security.Cryptography.Core.HashAlgorithmProvider sha1 = Windows.Security.Cryptography.Core.HashAlgorithmProvider.OpenAlgorithm("SHA1");
            Windows.Storage.Streams.IBuffer hash = sha1.HashData(buffUtf8Msg);
            byte[] array = new byte[hash.Length];
            Windows.Storage.Streams.DataReader dr = Windows.Storage.Streams.DataReader.FromBuffer(hash);
            dr.ReadBytes(array);
            string[] hex = new string[array.Length];
            for (int i = 0; i < array.Length; i++)
                hex[i] = array[i].ToString("X2");
            return string.Concat(hex);
        }
        /// <summary>
        /// IsDownlaodTaskRunning
        /// return true if the download thread is still running.
        /// </summary>
        /// <param name=""></param>
        /// <returns>return true if the download thread is still running</returns>
        public bool IsDownlaodTaskRunning()
        {
            return downloadTaskRunning;
        }
        /// <summary>
        /// IsAssetReadyToPlay
        /// return true if the asset is ready to play for both scenarios Download To Go and Progressive Download.
        /// </summary>
        /// <param name=""></param>
        /// <returns>return true if asset can be played</returns>
        public bool IsAssetReadyToPlay()
        {
            if(IsAssetEnoughDownloaded(DownloadToGo))
            {
                if (IsAssetProtected())
                {
                    if (IsPlayReadyLicenseAcquired)
                        return true;
                    return false;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Method: GetExpectedSize
        /// Return the expected size in byte of the asset to be downloaded 
        /// </summary>
        public ulong GetExpectedSize()
        {
            //ulong expectedSize = ((this.AudioBitrate + this.VideoBitrate) * this.TimescaleToHNS(this.Duration)) / (8 * ManifestCache.TimeUnit);
            ulong duration = this.TimescaleToHNS(this.Duration) / (ManifestCache.TimeUnit);
            ulong expectedSize = ((this.AudioBitrate + this.VideoBitrate) * duration) / 8 ;
            return expectedSize;
        }
        /// <summary>
        /// GetRemainingDowloadTime
        /// return the estimated remaining time before the end of the download.
        /// </summary>
        /// <param name=""></param>
        /// <returns>return the remaining time in seconds</returns>
        public ulong GetRemainingDowloadTime()
        {
            if ((VideoSavedChunks >= VideoChunks) &&
                (AudioSavedChunks >= AudioChunks))
                return 0;
            ulong expectedMediaSize = GetExpectedSize();
            ulong currentMediaSize = CurrentMediaSize;
            if (expectedMediaSize > currentMediaSize)
            {
                DateTime time = DateTime.Now;
                double currentBitrate = (this.DownloadThreadVideoCount + this.DownloadThreadAudioCount) * 8 / (time - DownloadThreadStartTime).TotalSeconds;
                ulong remainingSize = expectedMediaSize - CurrentMediaSize;
                return ((currentBitrate!=0)? (remainingSize * 8) / ((ulong)currentBitrate):0) + 1;
            }
            return 0;
        }
        /// <summary>
        /// IsAssetEnoughDownloaded
        /// return true if the asset is ready to play for both scenarios Download To Go and Progressive Download.
        /// </summary>
        /// <param name="DownloadToGo"> True if Downlaod To Go</param>
        /// <returns>return true if asset ready to play</returns>
        public bool IsAssetEnoughDownloaded(bool DownloadToGo)
        {
            if ((AudioChunks != 0) &&
                (AudioChunks == AudioSavedChunks) &&
                (VideoChunks != 0) &&
                (VideoChunks == VideoSavedChunks))
                return true;
            if ((AudioChunks < AudioSavedChunks) ||
                (VideoChunks < VideoSavedChunks))
                return true;
            if(DownloadToGo == false)
            {
                if (downloadTaskRunning == true)
                {
                    ulong duration = TimescaleToHNS(Duration) / TimeUnit;
                    // 10% margin for the remaining time 
                    if ((GetRemainingDowloadTime() * 1.1) < duration)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// DownloadChunkAsync
        /// Download asynchronously a chunk.
        /// </summary>
        /// <param name="chunkUri">Uri of the chunk to download </param>
        /// <param name="forceNewDownload">if true force the downlaod (adding a guid at the end of the uri) </param>
        /// <returns>return a byte array containing the chunk</returns>
        public virtual async Task<byte[]> DownloadChunkAsync(Uri chunkUri, bool forceNewDownload = true)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DownloadChunk start for chunk: " + chunkUri.ToString() );

            var client = new Windows.Web.Http.HttpClient();
            try
            {
                if (forceNewDownload)
                {
                    string modifier = chunkUri.AbsoluteUri.Contains("?") ? "&" : "?";
                    string newUriString = string.Concat(chunkUri.AbsoluteUri, modifier, "ignore=", Guid.NewGuid());
                    chunkUri = new Uri(newUriString);
                }
                Windows.Web.Http.HttpResponseMessage response = await client.GetAsync(chunkUri, Windows.Web.Http.HttpCompletionOption.ResponseContentRead).AsTask(downloadTaskCancellationtoken.Token);

                response.EnsureSuccessStatusCode();
                Windows.Storage.Streams.IBuffer buffer = await response.Content.ReadAsBufferAsync();
                if(buffer!=null)
                {
                    uint val = buffer.Length;
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DownloadChunk done for chunk: " + chunkUri.ToString());
                    return buffer.ToArray();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DownloadChunk exception: " + e.Message + " for chunk: " + chunkUri.ToString());
            }

            return null;
        }


        /// <summary>
        /// DownloadCurrentVideoChunk
        /// Download the current video chunk .
        /// </summary>
        /// <param name=""></param>
        /// <returns>return the lenght of the downloaded video chunk</returns>
        async Task<ulong> DownloadCurrentVideoChunk()
        {
            ulong len = 0;
            int i = (int)VideoDownloadedChunks;
            string url = (string.IsNullOrEmpty(RedirectBaseUrl) ?  BaseUrl : RedirectBaseUrl) + "/" + VideoTemplateUrl.Replace("{start_time}", VideoChunkList[i].Time.ToString());
            VideoChunkList[i].chunkBuffer = await DownloadChunkAsync(new Uri(url));
            if (VideoChunkList[i].IsChunkDownloaded())
            {
                len = VideoChunkList[i].GetLength();
                VideoDownloadedBytes += len;
                VideoDownloadedChunks++;
            }
            return len;
        }
        /// <summary>
        /// DownloadCurrentAudioChunk
        /// Download the current audio chunk .
        /// </summary>
        /// <param name=""></param>
        /// <returns>return the length of the downloaded audio chunk</returns>
        async Task<ulong> DownloadCurrentAudioChunk()
        {
            ulong len = 0;
            int i = (int)AudioDownloadedChunks;
            string url = (string.IsNullOrEmpty(RedirectBaseUrl) ? BaseUrl : RedirectBaseUrl) + "/" + AudioTemplateUrl.Replace("{start_time}", AudioChunkList[i].Time.ToString());
            AudioChunkList[i].chunkBuffer = await DownloadChunkAsync(new Uri(url));
            if (AudioChunkList[i].IsChunkDownloaded())
            {
                len = AudioChunkList[i].GetLength();
                AudioDownloadedBytes += len;
                AudioDownloadedChunks++;
            }
            return len;
        }
        /// <summary>
        /// downloadThread
        /// Download thread 
        /// This thread download one by one the audio and video chunks
        /// When the amount of chunks in memory is over a defined limit, the chunks are stored on disk and disposed
        /// When the download is completed, the thread exits 
        /// </summary>
        /// <param name=""></param>
        /// <returns>return the length of the downloaded audio chunk</returns>
        public async  Task<bool> downloadThread()
        {

            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread started for Uri: " + ManifestUri.ToString());
            downloadTaskRunning = true;
            // Were we already canceled?
            if (downloadTaskCancellationtoken != null)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Cancellation Token throw for Uri: " + ManifestUri.ToString());
                downloadTaskCancellationtoken.Token.ThrowIfCancellationRequested();
            }


            DownloadThreadStartTime = DateTime.Now;
            DownloadThreadAudioCount = 0;
            DownloadThreadVideoCount = 0;

            VideoDownloadedChunks = VideoSavedChunks;
            AudioDownloadedChunks = AudioSavedChunks;
            VideoDownloadedBytes = VideoSavedBytes;
            AudioDownloadedBytes = AudioSavedBytes;

            ManifestStatus = AssetStatus.DownloadingChunks;
            int error = 0;
            while (downloadTaskRunning)
            {
                // Poll on this property if you have to do
                // other cleanup before throwing.
                if ((downloadTaskCancellationtoken != null) && (downloadTaskCancellationtoken.Token.IsCancellationRequested))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Cancellation Token throw for Uri: " + ManifestUri.ToString());
                    // Clean up here, then...
                    downloadTaskCancellationtoken.Token.ThrowIfCancellationRequested();
                }

                bool result = false;
                // Download Video Chunks
                if((VideoChunkList!= null)&&(VideoChunkList.Count>0))
                {
                    // Something to download
                    if(VideoDownloadedChunks < VideoChunks)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading video chunks for Uri: " + ManifestUri.ToString());
                        ulong len = await DownloadCurrentVideoChunk();
                        if (len > 0)
                        {
                            DownloadThreadVideoCount += len;
                            result = true;
                        }
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading video chunks done for Uri: " + ManifestUri.ToString());

                    }
                }

                if (downloadTaskRunning == false)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloadTaskRunning == false for Uri: " + ManifestUri.ToString());
                    break;
                }

                // Download Audio Chunks
                if ((AudioChunkList != null) && (AudioChunkList.Count > 0))
                {
                    // Something to download
                    if (AudioDownloadedChunks < AudioChunks)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading audio chunks for Uri: " + ManifestUri.ToString());
                        ulong len = await DownloadCurrentAudioChunk();
                        if (len > 0)
                        {
                            DownloadThreadAudioCount += len;
                            result = true;
                        }
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading video chunks done for Uri: " + ManifestUri.ToString());

                    }
                }
                if (downloadTaskRunning == false)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloadTaskRunning == false for Uri: " + ManifestUri.ToString());
                    break;
                }

                if (result == false)
                {
                    error++;
                    if (error > MaxError)
                    {
                        DateTime time = DateTime.Now;
                        System.Diagnostics.Debug.WriteLine("Download stopped too many error at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", time));
                        System.Diagnostics.Debug.WriteLine("Current Media Size: " + this.CurrentMediaSize.ToString() + " Bytes");
                        double bitrate = (this.DownloadThreadVideoCount + this.DownloadThreadAudioCount) * 8 / (time - DownloadThreadStartTime).TotalSeconds;
                        System.Diagnostics.Debug.WriteLine("Download bitrate for the current session: " + bitrate.ToString() + " bps");
                        ManifestStatus = AssetStatus.ErrorChunksDownload;
                        downloadTaskRunning = false;
                    }
                }
                if ((VideoDownloadedChunks >= VideoChunks) &&
                    (AudioDownloadedChunks >= AudioChunks))
                {
                    DateTime time = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine("Download done at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", time));
                    System.Diagnostics.Debug.WriteLine("Current Media Size: " + this.CurrentMediaSize.ToString() + " Bytes");
                    double bitrate = (this.DownloadThreadVideoCount + this.DownloadThreadAudioCount) * 8 / (time - DownloadThreadStartTime).TotalSeconds;
                    System.Diagnostics.Debug.WriteLine("Download bitrate for the current session: " + bitrate.ToString() + " bps");
                    System.Diagnostics.Debug.WriteLine("Download Thread Saving Asset");
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset for Uri: " + ManifestUri.ToString());
                    if (DiskCache != null)
                        await DiskCache.SaveAsset(this);
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset done for Uri: " + ManifestUri.ToString());
                    DownloadedPercentage = (uint)(((VideoSavedChunks + AudioSavedChunks) * 100) / (VideoChunks + AudioChunks));
                    ManifestStatus = AssetStatus.ChunksDownloaded;
                    downloadTaskRunning = false;
                    break;
                }
                ulong s = GetBufferSize();
                if (s > MaxMemoryBufferSize)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset for Uri: " + ManifestUri.ToString());
                    if (DiskCache != null)
                      await DiskCache.SaveAsset(this);
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset done for Uri: " + ManifestUri.ToString());

                    if ((DownloadToGo == false)&&(ManifestStatus != AssetStatus.AssetPlayable))
                    {
                        if(IsAssetEnoughDownloaded(DownloadToGo))
                            ManifestStatus = AssetStatus.AssetPlayable;
                    }
                }

                DownloadedPercentage = (uint)(((VideoSavedChunks + AudioSavedChunks) * 100) / (VideoChunks + AudioChunks));
            }

            downloadTaskRunning = false;
            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread ended for Uri: " + ManifestUri.ToString());
            return true;
        }
        /// <summary>
        /// GetBufferSize
        /// Return the amount of audio and video chunk in memory
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return the amount of audio and video chunk in memory</returns>
        private ulong GetBufferSize()
        {
            ulong audioLength = 0;
            for (int i = 0; i < AudioChunkList.Count; i++)
            {
                if (AudioChunkList[i].chunkBuffer != null)
                    audioLength += (ulong) AudioChunkList[i].chunkBuffer.Length;
                else
                {
                    if (audioLength > 0)
                        break;
                }
            }
            ulong videoLength = 0;
            for (int i = 0; i < VideoChunkList.Count; i++)
            {
                if (VideoChunkList[i].chunkBuffer != null)
                    videoLength += (ulong)VideoChunkList[i].chunkBuffer.Length;
                else
                {
                    if (videoLength > 0)
                        break;
                }
            }
            return audioLength + videoLength;
        }
        /// <summary>
        /// InitializeDownloadParameters
        /// Initialize the download parameters
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        private async Task<bool> InitializeDownloadParameters()
        {
            AudioDownloadedBytes = 0;
            AudioDownloadedChunks = 0;
            AudioSavedBytes = 0;
            VideoDownloadedBytes = 0;
            VideoDownloadedChunks = 0;
            VideoSavedBytes = 0;
            await DiskCache.SaveManifest(this);
            return true;
        }
        /// <summary>
        /// StopDownloadThread
        /// Stop the download parameters
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        private bool StopDownloadThread()
        {
            if ((downloadTask != null) && (downloadTaskRunning == true))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread for Uri: " + ManifestUri.ToString());
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask downloadTaskRunning = false for Uri: " + ManifestUri.ToString());
                downloadTaskRunning = false;
                if (!downloadTask.IsCompleted)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask Cancel token for Uri: " + ManifestUri.ToString());
                    downloadTaskCancellationtoken.Cancel();
                }
                try
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread Waiting end of thread for Uri: " + ManifestUri.ToString());
                    downloadTask.Wait(500);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread Exception for Uri: " + ManifestUri.ToString() + " exception: " + e.Message);
                }

                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread completed for Uri: " + ManifestUri.ToString());
                downloadTask = null;
            }
            return true;
        }

        /// <summary>
        /// PauseDownloadChunks
        /// Pause the download of chunks
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        public bool PauseDownloadChunks()
        {
            return StopDownloadThread();
        }
        /// <summary>
        /// StopDownloadChunks
        /// Stop the download of chunks
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        public bool StopDownloadChunks()
        {
            return StopDownloadThread();
        }
        /// <summary>
        /// StartDownloadThread
        /// Start the downlaod thread
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        private bool StartDownloadThread()
        {
            if (downloadTask == null)
            {
                downloadTaskRunning = false;
                downloadTaskCancellationtoken = new System.Threading.CancellationTokenSource();
                downloadTask = Task.Run(async () => { await downloadThread(); }, downloadTaskCancellationtoken.Token);
                if (downloadTask != null)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// StartDownloadChunks
        /// Start the download of chunks
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        public async Task<bool> StartDownloadChunks()
        {

            // stop the download thread if running
            StopDownloadThread();
            // Initialize Download paramters
            await InitializeDownloadParameters();
            return StartDownloadThread();
        }

        /// <summary>
        /// ResumeDownloadChunks
        /// Resume the download of chunks
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        public bool ResumeDownloadChunks()
        {
            // stop the download thread if running
            StopDownloadThread();
            return StartDownloadThread();
        }

        /// <summary>
        /// GetAudioChunkCache
        /// Return a ChunkCache based on time
        /// </summary>
        /// <param name="time"></param>
        /// <returns>Return a ChunkCache </returns>
        public ChunkCache GetAudioChunkCache(ulong time)
        {
            for (int i = 0; i < AudioChunkList.Count; i++)
            {
                if (AudioChunkList[i].Time == time)
                    return AudioChunkList[i];
            }
            return null;
        }
        /// <summary>
        /// GetVideoChunkCache
        /// Return a ChunkCache based on time
        /// </summary>
        /// <param name="time"></param>
        /// <returns>Return a ChunkCache </returns>
        public ChunkCache GetVideoChunkCache(ulong time)
        {
            for (int i = 0; i < VideoChunkList.Count; i++)
            {
                if (VideoChunkList[i].Time == time)
                    return VideoChunkList[i];
            }
            return null;
        }

        #region Events
        /// <summary>
        /// ManifestStatus
        /// Return a ManifestStatus 
        /// </summary>
        public AssetStatus ManifestStatus {
            get
            {
                return mStatus;
            }
            private set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    if (StatusProgress!=null)
                        StatusProgress(this, mStatus);
                }
            }
        }
        private uint downloadedPercentage;
        /// <summary>
        /// DownloadedPercentage
        /// Return a downloaded Percentage based on the number of audio and video chunks downloaded
        /// </summary>
        public uint DownloadedPercentage {
            get
            {
                downloadedPercentage = (uint)(((VideoSavedChunks + AudioSavedChunks) * 100) / (VideoChunks + AudioChunks));
                return downloadedPercentage;
            }
            set
            {
                if(downloadedPercentage != value)
                {
                    downloadedPercentage = value;
                    if (DownloadProgress != null)
                        DownloadProgress(this, downloadedPercentage);
                } 
            }
        }


        /// <summary>
        /// This event is used to track the download progress
        /// The parameter is an int between 0 and 100
        /// </summary>
        public event System.EventHandler<uint> DownloadProgress;
        /// <summary>
        /// This event is used to track the status progress
        /// The parameter is an int between 0 and 100
        /// </summary>
        public event System.EventHandler<AssetStatus> StatusProgress;

        #endregion
        public void Dispose()
        {
            manifestBuffer = null;
            if(VideoChunkList!= null)
            {
                foreach( var c in VideoChunkList)
                {
                    c.Dispose();                    
                }
            }
            if (AudioChunkList != null)
            {
                foreach (var c in AudioChunkList)
                {
                    c.Dispose();
                }
            }
        }

    }
}
