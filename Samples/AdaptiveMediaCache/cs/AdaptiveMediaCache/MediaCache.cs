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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Media.AdaptiveStreaming;
using Windows.Foundation;
using AdaptiveMediaCache.SmoothStreaming;
using System.IO;


namespace AdaptiveMediaCache
{

    /// <summary>
    /// delegate DownloadProgressEventHandler
    /// Parameter: Current MediaCache
    /// Parameter: The manifestUri
    /// Parameter: percentage of downloaded chunks 
    /// </summary>
    public delegate void DownloadProgressEventHandler(MediaCache mc, Uri manifestUri, uint percent);

    /// <summary>
    /// delegate StatusProgressEventHandler
    /// Parameter: Current MediaCache
    /// Parameter: The manifestUri
    /// Parameter: The status of the asset: 
    /// - Initialized
    /// - DownloadingManifest
    /// - ManifestDownloaded
    /// - DownloadingChunks
    /// - AssetPlayable (for Progressive Download only - Not applicable for DownloadToGo)
    /// - ChunksDownloaded
    /// - Errorxxxxx
    /// </summary>
    public delegate void StatusProgressEventHandler(MediaCache mc, Uri manifestUri, AssetStatus Status);

    public enum AssetStatus
    {
        Initialized = 0,
        DownloadingManifest,
        ManifestDownloaded,
        DownloadingChunks,
        AssetPlayable,
        ChunksDownloaded,
        ErrorManifestAlreadyInCache,
        ErrorManifestCreationError,
        ErrorDownloadAssetLimit,
        ErrorManifestStorage,
        ErrorManifestNotInCache,
        ErrorManifestDownload,
        ErrorChunksDownload,
        ErrorDownloadSessionLimit,
        ErrorStorageLimit,
        ErrorParameters,
    }
    public sealed class AudioTrack
    {
        public int Index { get; set; }
        public int Bitrate { get; set; }
        public string FourCC { get; set; }
    }
    public sealed class VideoTrack
    {
        public int Index { get; set; }
        public int Bitrate { get; set; }
        public string FourCC { get; set; }
        public int MaxHeight { get; set; }
        public int MaxWidth { get; set; }
    }
    public sealed class MediaCache : IDownloaderPlugin, IDisposable
    {

        #region Properties
        public bool IsInitialized { get; private set; }
        public uint MaxError { get; private set; }
        public ulong MaxMemoryBufferSizePerSession { get; private set; }
        public uint MaxDownloadSessions { get; private set; }
        public uint MaxDownloadedAssets { get; private set; }
        public uint ActiveDownloads { get
            {
                uint count = 0;
                foreach (var mc in ManifestCacheList) {
                    if (mc.Value.IsDownlaodTaskRunning() == true)
                        count++;
                }
                return count;
            } }
        public bool AutoStartDownload { get; private set; }
        #endregion


        #region Events
        /// <summary>
        /// This event is used to track the download progress
        /// The parameter is DownloadProgressEventArgs
        /// </summary>
        public event DownloadProgressEventHandler DownloadProgressEvent;


        /// <summary>
        /// This event is used to track the status progress
        /// The parameter are StatusProgressEventArgs 
        /// Current MediaCache
        /// ManifestUri uri
        /// Status AssetStatus
        /// </summary>
        public event StatusProgressEventHandler StatusProgressEvent;
        #endregion

        #region CacheMethods
        /// <summary>
        /// Method: constructor
        /// <summary>
        public MediaCache()
        {
            IsInitialized = false;
        }
        /// <summary>
        /// Method: Initialize 
        /// Initialize the cache
        /// Parameter: container is a string defining the folder name where the cache will be stored on disk under folder
        /// \Users\<UserName>\AppData\Local\Packages\<PackageID>\LocalState
        /// Parameter: bDownloadToGo if true DownloadToGo scenario (offline playback), if false ProgressiveDownload scenario
        /// Parameter: maxDownloadSession maximum number of simultaneous download sessions
        /// Parameter: maxDownloadSession maximum number of simultaneous download sessions
        /// Parameter: maxMemoryBufferSizePerSession maximum buffer size per session, when the size if over this value, the chunks will be saved on disk and freed from memory.
        /// Parameter: maxDownloadedAssets maximum number of asset on disk
        /// Parameter: maxError maximum number of http error while downloading the chunks associated with an asset. When this value is reached the download thread will be cancelled
        /// Parameter: bAutoStartDownload if true after a resume the cache will start automatically the download 
        /// </summary>
        public IAsyncOperation<bool> Initialize(string container, uint maxDownloadSession, ulong maxMemoryBufferSizePerSession, uint maxDownloadedAssets, uint maxError, bool bAutoStartDownload)
        {
            return Task.Run<bool>(async () =>
            {
                if (ManifestCacheList != null)
                    Uninitialize();
                Container = container;


                MaxError = maxError;
                MaxMemoryBufferSizePerSession = maxMemoryBufferSizePerSession;
                MaxDownloadSessions = maxDownloadSession;
                MaxDownloadedAssets = maxDownloadedAssets;
                AutoStartDownload = bAutoStartDownload;

                // SMOOTH
                // Init Adaptative Manager
                AdaptiveSrcManager = AdaptiveSourceManager.GetDefault();
                AdaptiveSrcManager.SetDownloaderPlugin(this);
                AdaptiveSrcManager.AdaptiveSourceOpenedEvent += AdaptiveSrcManager_AdaptiveSourceOpenedEvent;
                AdaptiveSrcManager.AdaptiveSourceClosedEvent += AdaptiveSrcManager_AdaptiveSourceClosedEvent;


                // SMOOTH
                // Init SMOOTH Manager
                SmoothStreamingManager = Microsoft.Media.AdaptiveStreaming.AdaptiveSourceManager.GetDefault() as Microsoft.Media.AdaptiveStreaming.AdaptiveSourceManager;
                Extension = new Windows.Media.MediaExtensionManager();
                if ((SmoothStreamingManager != null) &&
                    (Extension != null))
                {
                    Windows.Foundation.Collections.PropertySet ssps = new Windows.Foundation.Collections.PropertySet();
                    ssps["{A5CE1DE8-1D00-427B-ACEF-FB9A3C93DE2D}"] = SmoothStreamingManager;


                    Extension.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "text/xml", ssps);
                    Extension.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "application/vnd.ms-sstr+xml", ssps);
                    Extension.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".isml", "text/xml", ssps);
                    Extension.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".isml", "application/vnd.ms-sstr+xml", ssps);


                    Extension.RegisterSchemeHandler("Microsoft.Media.AdaptiveStreaming.SmoothSchemeHandler", "ms-sstr:", ssps);
                    Extension.RegisterSchemeHandler("Microsoft.Media.AdaptiveStreaming.SmoothSchemeHandler", "ms-sstrs:", ssps);

                    SmoothStreamingManager.ManifestReadyEvent += SmoothStreamingManager_ManifestReadyEvent;
                    SmoothStreamingManager.SetDownloaderPlugin(this);
                }

                if (diskCache == null)
                {
                    diskCache = new DiskCache();
                    if (diskCache != null)
                    {
                        bool bResult = false;
                        bResult = await diskCache.Initialize(Container);
                        if (bResult != true)
                        {
                            System.Diagnostics.Debug.WriteLine("Can't initialize DiskCache");
                            return false;
                        }
                    }
                }
                if (ManifestCacheList == null)
                    ManifestCacheList = new ConcurrentDictionary<Uri, ManifestCache>();
                if (ManifestCacheList != null)
                    IsInitialized = true;
                return IsInitialized;
            }).AsAsyncOperation<bool>();
        }

        /// <summary>
        /// Method: Uninitialize 
        /// Uninitialize the cache
        /// </summary>
        public bool Uninitialize()
        {
            bool result = false;
            if (IsInitialized)
            {
                // Stop all the download threads
                Suspend();
                AdaptiveSrcManager.AdaptiveSourceOpenedEvent -= AdaptiveSrcManager_AdaptiveSourceOpenedEvent;
                AdaptiveSrcManager.AdaptiveSourceClosedEvent -= AdaptiveSrcManager_AdaptiveSourceClosedEvent;
                AdaptiveSrcManager = null;
                SmoothStreamingManager.ManifestReadyEvent -= SmoothStreamingManager_ManifestReadyEvent;
                SmoothStreamingManager = null;
                IsInitialized = false;
                if (ManifestCacheList != null)
                    ManifestCacheList = null;
                Container = string.Empty;
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Method: Dispose
        /// Release the cache
        /// </summary>
        public void Dispose()
        {
            if (ManifestCacheList != null)
            {
                foreach (var mc in ManifestCacheList)
                {
                    mc.Value.Dispose();
                }
            }
        }

        #endregion

        #region AssetMethods
        /// <summary>
        /// Method: GetSourceUri 
        /// Return the Uri to use to initialize the MediaElement Source
        /// In order to prevent the MediaElement from sending http request when offline
        /// we need to replace http:// with ms-sstr::// prefix
        /// Parameter: manifestUri
        /// </summary>
        public Uri GetSourceUri(Uri manifestUri)
        {
            if (this.IsAssetInCache(manifestUri))
            {
                try
                {
                    string newUrl = GetSmoothStreamingUrl(manifestUri.ToString());
                    return new Uri(newUrl);
                }
                catch (Exception)
                {

                }
            }
            return null;
        }
        /// <summary>
        /// Method: Add 
        /// Add the asset associated with the manifestUri in the list of Assets managed by the cache 
        /// Return type enum AssetStatus, if the value is AssetStatus.Initialized the asset has been added correctly in the list
        /// Parameter: manifestUri Uri of the manifest (Smooth Streaming manifest so far)
        /// Parameter: bDownloadToGo Download Scenario: Download-To-Go if true or Progressive Download if false
        /// Parameter: minBitrate minimum bitrate for the video track
        /// Parameter: maxBitrate maximum bitrate for the video track
        /// Parameter: audioIndex index of the audio track to select (usually = 0)
        /// Parameter: Download Method: 0 Auto: The cache will create if necessary several threads to download audio and video chunks  
        ///                             1 Default: The cache will download the audio and video chunks step by step in one single thread
        ///                             N The cache will create N parallel threads to download the audio chunks and N parallel threads to downlaod video chunks 
        /// </summary>
        public AssetStatus Add(Uri manifestUri, bool bDownloadToGo, ulong minBitrate, ulong maxBitrate, int   audioIndex, int downloadMethod)
        {
            if ((ManifestCacheList != null) &&
                (ManifestCacheList.Count + 1 > MaxDownloadedAssets))
                return AssetStatus.ErrorDownloadAssetLimit;
            if (minBitrate > maxBitrate)
                return AssetStatus.ErrorParameters;
            if (!ManifestCacheList.ContainsKey(manifestUri))
            {
                
                ManifestCache mc = ManifestCache.CreateManifestCache(manifestUri, bDownloadToGo, minBitrate, maxBitrate, audioIndex, MaxMemoryBufferSizePerSession, MaxError, downloadMethod);
                if (mc != null)
                {
                    if (ManifestCacheList.TryAdd(manifestUri, mc))
                    {
                        mc.SetDiskCache(diskCache);
                        mc.DownloadProgress += ManifestCache_DownloadProgress;
                        mc.StatusProgress += ManifestCache_StatusProgress;
                       // mc.SaveProgress += ManifestCache_SaveProgress;
                        return mc.ManifestStatus;
                    }
                }
                return AssetStatus.ErrorManifestCreationError;
            }
            return AssetStatus.ErrorManifestAlreadyInCache;
        }
        /// <summary>
        /// Method: Add 
        /// Add the asset associated with the manifestUri in the list of Assets managed by the cache 
        /// Return type enum AssetStatus, if the value is AssetStatus.Initialized the asset has been added correctly in the list
        /// Parameter: manifestUri Uri of the manifest (Smooth Streaming manifest so far)
        /// Parameter: bDownloadToGo Download Scenario: Download-To-Go if true or Progressive Download if false
        /// Parameter: videoIndex index of the video track to select (usually = 0)
        /// Parameter: audioIndex index of the audio track to select (usually = 0)
        /// Parameter: Download Method: 0 Auto: The cache will create if necessary several threads to download audio and video chunks  
        ///                             1 Default: The cache will download the audio and video chunks step by step in one single thread
        ///                             N The cache will create N parallel threads to download the audio chunks and N parallel threads to downlaod video chunks 
        /// </summary>
        public AssetStatus Add(Uri manifestUri, bool bDownloadToGo, int videoIndex, int audioIndex, int downloadMethod)
        {
            if ((ManifestCacheList != null) &&
                (ManifestCacheList.Count + 1 > MaxDownloadedAssets))
                return AssetStatus.ErrorDownloadAssetLimit;
            if (!ManifestCacheList.ContainsKey(manifestUri))
            {

                ManifestCache mc = ManifestCache.CreateManifestCache(manifestUri, bDownloadToGo, videoIndex, audioIndex, MaxMemoryBufferSizePerSession, MaxError, downloadMethod);
                if (mc != null)
                {
                    if (ManifestCacheList.TryAdd(manifestUri, mc))
                    {
                        mc.SetDiskCache(diskCache);
                        mc.DownloadProgress += ManifestCache_DownloadProgress;
                        mc.StatusProgress += ManifestCache_StatusProgress;
                        return mc.ManifestStatus;
                    }
                }
                return AssetStatus.ErrorManifestCreationError;
            }
            return AssetStatus.ErrorManifestAlreadyInCache;
        }
        /// <summary>
        /// Method: GetCount 
        /// Return the number of assets in the cache
        /// </summary>
        public int GetCount()
        {
            if (ManifestCacheList != null)
                return ManifestCacheList.ToArray().Count();
            return 0;
        }
        /// <summary>
        /// Method: GetUri
        /// Return the Uri at Index position 
        /// Parameter: Index of the Uri
        /// </summary>
        public Uri GetUri(int Index)
        {
            if (ManifestCacheList != null)
            {
                if (Index < ManifestCacheList.ToArray().Count())
                {
                    return ManifestCacheList.ToArray()[Index].Value.ManifestUri;
                }
            }
            return null;
        }



        /// <summary>
        /// Method: Delete 
        /// Delete the asset 
        /// Parameter: manifest Uri 
        /// </summary>
        public IAsyncOperation<bool> Delete(Uri manifestUri)
        {
            return Task.Run<bool>(async  () =>
           {
               if (ManifestCacheList.ContainsKey(manifestUri))
               {
                   ManifestCache mc;
                   if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                   {
                       if (mc != null)
                       {
                            // Remove the asset from the disk 
                            await RemoveAsset(mc.ManifestUri);
                       }
                   }
                    // Remove the asset from the List 
                    if (ManifestCacheList.TryRemove(manifestUri, out mc))
                   {
                       if (mc != null)
                       {
                           mc.DownloadProgress -= ManifestCache_DownloadProgress;
                           mc.StatusProgress -= ManifestCache_StatusProgress;
                           //mc.SaveProgress -= ManifestCache_SaveProgress;

                           mc.Dispose();
                       }
                   }
                   return true;
               }
               return false;
           }).AsAsyncOperation<bool>();
        }
        #endregion

        #region InfoMethods

        /// <summary>
        /// Method: GetAudioBitrate 
        /// return the Audio bitrate selected in bit/s 
        /// Parameter: manifest Uri 
        /// </summary>
        public ulong GetAudioBitrate(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri].AudioBitrate;
            }
            return 0;
        }
        /// <summary>
        /// Method: GetVideoBitrate 
        /// return the Video bitrate selected in bit/s 
        /// Parameter: manifest Uri 
        /// </summary>
        public ulong GetVideoBitrate(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri].VideoBitrate;
            }
            return 0;
        }
        /// <summary>
        /// Method: GetDuration
        /// return asset duration (unit 100 ns) 
        /// Parameter: manifest Uri 
        /// </summary>
        public ulong GetDuration(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri].TimescaleToHNS(ManifestCacheList[manifestUri].Duration);
            }
            return 0;
        }
        /// <summary>
        /// Method: GetAudioChunksCount 
        /// return the number of audio chunks for the asset
        /// Parameter: manifest Uri 
        /// </summary>
        public ulong GetAudioChunksCount(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri].AudioChunks;
            }
            return 0;
        }
        /// <summary>
        /// Method: GetSavedAudioChunksCount 
        /// return the number of video chunks for the asset
        /// Parameter: manifest Uri 
        /// </summary>
        public ulong GetSavedAudioChunksCount(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri].AudioSavedChunks;
            }
            return 0;
        }

        /// <summary>
        /// Method: GetVideoChunksCount 
        /// return the number of video chunks for the asset
        /// Parameter: manifest Uri 
        /// </summary>
        public ulong GetVideoChunksCount(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri].VideoChunks;
            }
            return 0;
        }
        /// <summary>
        /// Method: GetSavedVideoChunksCount 
        /// return the number of video chunks for the asset
        /// Parameter: manifest Uri 
        /// </summary>
        public ulong GetSavedVideoChunksCount(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri].VideoSavedChunks;
            }
            return 0;
        }
        /// <summary>
        /// Method: GetAssetStatus 
        /// return the status of the asset
        /// Parameter: manifest Uri 
        /// </summary>
        public AssetStatus GetAssetStatus(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri].GetAssetStatus();
            }
            return AssetStatus.ErrorManifestNotInCache;
        }
        /// <summary>
        /// Method: IsAssetDowloaded 
        /// return true is the asset is downloaded
        /// Parameter: manifest Uri 
        /// </summary>
        public bool IsAssetEnoughDowloaded(Uri manifestUri, bool DownloadToGo)
        {
            bool bResult = false;
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                bResult = ManifestCacheList[manifestUri].IsAssetEnoughDownloaded(DownloadToGo);
            }
            return bResult;
        }
        /// <summary>
        /// Method: GetDownloadedProgress 
        /// return the percentage of chunks downloaded
        /// Parameter: manifest Uri 
        /// </summary>
        public uint GetDownloadedProgress(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri].DownloadedPercentage;
            }
            return 0;
        }

        /// <summary>
        /// Method: IsAssetInCache 
        /// return true is the asset is in the cache
        /// Parameter: manifest Uri 
        /// </summary>
        public bool IsAssetInCache(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method: IsAssetReadyToPlay
        /// return true if the asset is ready to be played:protected with PlayReady 
        /// - Downloaded for unencrypted content
        /// - Downloaded and license Acquired for encrypted content
        /// Parameter: manifest Uri 
        /// </summary>
        public bool IsAssetReadyToPlay(Uri manifestUri)
        {
            bool bResult = false;
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                bResult = ManifestCacheList[manifestUri].IsAssetReadyToPlay();
            }
            return bResult;
        }
        #endregion

        #region PlayReadyMethods
        /// <summary>
        /// Method: IsAssetProtected
        /// return true if the asset is protected with PlayReady 
        /// Parameter: manifest Uri 
        /// </summary>
        public bool IsAssetProtected(Uri manifestUri)
        {
            bool bResult = false;
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                bResult = ManifestCacheList[manifestUri].IsAssetProtected();
            }
            return bResult;
        }
        /// <summary>
        /// Method: GetPlayReadyExpirationDate
        /// return the Expiration Date for the PlayReady license (return DateTimeOffset.MinValue if not available)
        /// Parameter: manifest Uri 
        /// </summary>
        public DateTimeOffset GetPlayReadyExpirationDate(Uri manifestUri)
        {
            DateTimeOffset d = DateTimeOffset.MinValue;
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                d = ManifestCacheList[manifestUri].GetPlayReadyExpirationDate();
            }
            return d;
        }
        /// <summary>
        /// Method: GetPlayReadyLicense
        /// Get the PlayReady license for the asset 
        /// Parameter: manifest Uri 
        /// Parameter: override PlayReady License Uri 
        /// Parameter: PlayReady Challenge Custom Data
        /// </summary>
        public IAsyncOperation<bool> GetPlayReadyLicense(Uri manifestUri,Uri PlayReadyLicenseUri, string PlayReadyChallengeCustomData)
        {
            return Task.Run<bool>(async () =>
            {
                bool bResult = false;
                if (ManifestCacheList.ContainsKey(manifestUri))
                {
                    bResult = await ManifestCacheList[manifestUri].GetPlayReadyLicense(PlayReadyLicenseUri, PlayReadyChallengeCustomData);
                }
                return bResult;
            }).AsAsyncOperation<bool>();
        }
        #endregion 

        #region DownloadMethods
        /// <summary>
        /// Method: StartDownload 
        /// Start the download of the asset: 
        ///      Download the manifest
        ///      if the asset is protected, Acquire the PlayReady license
        ///      Start the Thread to Download the chunks
        /// Parameter: manifest Uri 
        /// Parameter: override PlayReady License Uri 
        /// Parameter: PlayReady Challenge Custom Data
        ///          
        /// </summary>

        public IAsyncOperation<AssetStatus> StartDownload(Uri manifestUri, Uri PlayReadyLicenseUri, string PlayReadyChallengeCustomData)
        {
            return Task.Run<AssetStatus>(async () =>
            {
                if (ManifestCacheList.ContainsKey(manifestUri))
                {
                    ManifestCache mc;
                    if(ManifestCacheList.TryGetValue(manifestUri,out mc))
                    {
                        // Remove Asset from disk
                        await RemoveAsset(manifestUri);
                        if (await mc.DownloadManifest())
                        {
                            ulong availableSizeOnDisk = await GetAvailableSize();
                            ulong expectedAssetSize = GetExpectedSize(manifestUri);
                            // Check if enough Storage on Disk
                            if (expectedAssetSize * 2 < availableSizeOnDisk)
                            {
                                if (mc.IsAssetProtected())
                                    await mc.GetPlayReadyLicense(PlayReadyLicenseUri, PlayReadyChallengeCustomData);
                                // Save Asset On Disk
                                if (await SaveManifest(manifestUri))
                                {
                                    // Start download thread
                                    if (ActiveDownloads + 1 <= MaxDownloadSessions)
                                    {
                                        if ((await mc.StartDownloadChunks()) == true)
                                            return AssetStatus.DownloadingChunks;
                                    }
                                    return AssetStatus.ErrorDownloadSessionLimit;

                                }
                                return AssetStatus.ErrorManifestStorage;
                            }
                            return AssetStatus.ErrorStorageLimit;
                        }
                        return AssetStatus.ErrorManifestDownload;
                    }
                }
                return AssetStatus.ErrorManifestNotInCache;
            }).AsAsyncOperation<AssetStatus>();

        }
        /// <summary>
        /// Method: DownloadManifest 
        /// Start the download of the asset: 
        ///      Download the manifest
        /// Parameter: manifest Uri                  
        /// </summary>

        public IAsyncOperation<AssetStatus> DownloadManifest(Uri manifestUri)
        {
            return Task.Run<AssetStatus>(async () =>
            {
                if (ManifestCacheList.ContainsKey(manifestUri))
                {
                    ManifestCache mc;
                    if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                    {
                        // Remove Asset from disk
                        await RemoveAsset(manifestUri);
                        if (await mc.DownloadManifest())
                        {
                            ulong availableSizeOnDisk = await GetAvailableSize();
                            ulong expectedAssetSize = GetExpectedSize(manifestUri);
                            if (expectedAssetSize * 2 < availableSizeOnDisk)
                            {

                                // Save Asset On Disk
                                if (await SaveManifest(manifestUri))
                                {
                                    return AssetStatus.ManifestDownloaded;
                                }
                                return AssetStatus.ErrorManifestStorage;
                            }
                            return AssetStatus.ErrorStorageLimit;
                        }
                        return AssetStatus.ErrorManifestDownload;
                    }
                }
                return AssetStatus.ErrorManifestNotInCache;
            }).AsAsyncOperation<AssetStatus>();
        }
        /// <summary>
        /// Method: GetAudioTracks
        /// Return the list of video tracks in the manifest  
        /// DownloadManifest or StartDownload needs to be called before
        /// Parameter: manifest Uri         
        /// </summary>
        public IReadOnlyList<AudioTrack> GetAudioTracks(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.GetAudioTracks();
                }
            }
            return null;
        }
        /// <summary>
        /// Method: GetVideoTracks
        /// Return the list of video tracks in the manifest  
        /// DownloadManifest or StartDownload needs to be called before
        /// Parameter: manifest Uri         
        /// </summary>
        public IReadOnlyList<VideoTrack> GetVideoTracks(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.GetVideoTracks();
                }
            }
            return null;
        }
        /// <summary>
        /// Method: GetSelectedAudioIndex
        /// Return the selected audio index 
        /// DownloadManifest or StartDownload needs to be called before
        /// Parameter: manifest Uri         
        /// </summary>
        public int GetSelectedAudioTrackIndex(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.SelectedAudioTrackIndex;
                }
            }
            return -1;
        }
        /// <summary>
        /// Method: GetVideoTracks
        /// Return the selected video index 
        /// DownloadManifest or StartDownload needs to be called before
        /// Parameter: manifest Uri         
        /// </summary>
        public int GetSelectedVideoTrackIndex(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.SelectedVideoTrackIndex;
                }
            }
            return -1;
        }


        /// <summary>
        /// Method: GetExpectedSize
        /// Return the expected size in bytes of the asset to be downloaded 
        /// Parameter: manifest Uri         
        /// </summary>
        public ulong GetExpectedSize(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.GetExpectedSize();
                }
            }
            return 0;
        }
        /// <summary>
        /// Method: GetRemainingDowloadTime
        /// Return the estimated remaining downloading time (in seconds) for the asset be downloaded 
        /// Parameter: manifest Uri         
        /// </summary>
        public ulong GetRemainingDowloadTime(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.GetRemainingDowloadTime();
                }
            }
            return 0;
        }
        /// <summary>
        /// GetCurrentBitrate
        /// return the estimated download bitrate.
        /// </summary>
        /// <param name=""></param>
        /// <returns>return the estimated download bitrate in bit/seconds</returns>
        public double GetCurrentBitrate(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.GetCurrentBitrate();
                }
            }
            return 0;
        }
        /// <summary>
        /// Method: GetCurrentAudioCacheSize
        /// Return the current size of the saved audio chunks on disk for the asset 
        /// Parameter: manifest Uri         
        /// </summary>
        public ulong GetCurrentAudioCacheSize(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.AudioSavedBytes;
                }
            }
            return 0;
        }
        /// <summary>
        /// Method: GetCurrentVideoCacheSize
        /// Return the current size of the saved video chunks on disk for the asset 
        /// Parameter: manifest Uri         
        /// </summary>
        public ulong GetCurrentVideoCacheSize(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.VideoSavedBytes;
                }
            }
            return 0;
        }
        /// <summary>
        /// Method: GetCurrentVideoCacheSize
        /// Return the current size of the saved video and audio chunks on disk for the asset 
        /// Parameter: manifest Uri         
        /// </summary>
        public ulong GetCurrentMediaCacheSize(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.VideoSavedBytes + mc.AudioSavedBytes;
                }
            }
            return 0;
        }
        /// <summary>
        /// Method: DownloadChunks 
        /// Start the download the chunks: 
        ///      Download the manifest
        ///      if the asset is protected, Acquire the PlayReady license
        ///      Start the Thread to Download the chunks
        /// Parameter: manifest Uri 
        /// Parameter: override PlayReady License Uri 
        /// Parameter: PlayReady Challenge Custom Data
        ///          
        /// </summary>

        public IAsyncOperation<AssetStatus> StartDownloadChunks(Uri manifestUri)
        {
            return Task.Run<AssetStatus>(async () =>
            {
                if (ManifestCacheList.ContainsKey(manifestUri))
                {
                    ManifestCache mc;
                    if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                    {
                        // Start download thread
                        if (ActiveDownloads + 1 <= MaxDownloadSessions)
                        {
                            if (await mc.StartDownloadChunks() == true)
                                return AssetStatus.DownloadingChunks;
                        }
                        return AssetStatus.ErrorDownloadSessionLimit;
                    }
                }
                return AssetStatus.ErrorManifestNotInCache;
            }).AsAsyncOperation<AssetStatus>();
        }
        /// <summary>
        /// Method: StopDownload 
        /// Stop the download of the asset 
        /// Parameter: manifest Uri 
        /// </summary>

        public bool StopDownload(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.StopDownloadChunks();
                }
            }
            return false;
        }
        /// <summary>
        /// Method: PauseDownload 
        /// Pause the download of the asset 
        /// Parameter: manifest Uri 
        /// </summary>
        public bool PauseDownload(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.PauseDownloadChunks();
                }
            }
            return false;
        }

        /// <summary>
        /// Method: ResumeDownload 
        /// Resume the download of the asset 
        /// Parameter: manifest Uri 
        /// </summary>
        public bool ResumeDownload(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.ResumeDownloadChunks();
                }
            }
            return false;
        }
        /// <summary>
        /// Method: IsDownloadRunning 
        /// Return true if the download thread is running, otherwise false.
        /// Parameter: manifest Uri 
        /// </summary>
        public bool IsDownloadRunning(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                ManifestCache mc;
                if (ManifestCacheList.TryGetValue(manifestUri, out mc))
                {
                    return mc.IsDownlaodTaskRunning();
                }
            }
            return false;
        }
        #endregion

        #region DiskMethods
        /// <summary>
        /// Method: GetAvailableSize
        /// Return the available size on disk
        /// </summary>
        public IAsyncOperation<ulong> GetAvailableSize()
        {
            return Task.Run<ulong>(async () =>
            {
                if (diskCache != null)
                return await diskCache.GetAvailableSize();
                return 0;
            }).AsAsyncOperation<ulong>();
        }
        /// <summary>
        /// Method: GetCacheSize
        /// Return the size used on disk
        /// </summary>
        public IAsyncOperation<ulong> GetCacheSize()
        {
            return Task.Run<ulong>(async () =>
            {
                if (diskCache != null)
                    return await diskCache.GetCacheSize();
                return 0;
            }).AsAsyncOperation<ulong>();
        }
        
        /// <summary>
        /// Method: Suspend
        /// Suspend the current download and Save the assets on disk
        /// </summary>
        public bool Suspend()
        {

            bool bResult = false;
            if (ManifestCacheList != null)
            {
                bResult = true;
                foreach (var value in ManifestCacheList)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}" , DateTime.Now) + " Suspending Asset cache for Uri: " + value.Value.ManifestUri.ToString());
                    //bool res = await diskCache.SaveAsset(value.Value);
                    if (value.Value.IsDownlaodTaskRunning())
                    {
                        value.Value.StopDownloadChunks();
                    }
                }
            }
            return bResult;
        }
        /// <summary>
        /// Method: Resume
        /// Restore the list of assets from the files and directories on disk 
        /// Resume the download of chunks if necesary
        /// </summary>
        public IAsyncOperation<bool> Resume()
        {
            return Task.Run<bool>(async () =>
            {
                await Restore();

                bool bResult = false;
                if (ManifestCacheList != null)
                {
                    bResult = true;
                    // if AutoStartDownload == true
                    // Start the downloadthread to complete the download of assets
                    if (AutoStartDownload)
                    {
                        foreach (var val in ManifestCacheList)
                        {
                            var cache = val.Value;
                            if (cache != null)
                            {
                                if (((cache.VideoSavedChunks > 0) && (cache.VideoSavedChunks < cache.VideoChunks)) ||
                                    ((cache.AudioSavedChunks > 0) && (cache.AudioSavedChunks < cache.AudioChunks)))
                                    cache.ResumeDownloadChunks();
                            }
                        }
                    }
                }
                return bResult;
            }).AsAsyncOperation<bool>();
        }

        /// <summary>
        /// Method: Clear
        /// Initialize the list of assets  
        /// Remove all the assets from the cache on disk
        /// </summary>
        public IAsyncOperation<bool> Clear()
        {
            return Task.Run<bool>(async () =>
            {

                bool bResult = false;
                if (ManifestCacheList == null)
                    ManifestCacheList = new ConcurrentDictionary<Uri, ManifestCache>();
                if (ManifestCacheList != null)
                {
                    bResult = true;
                    ManifestCacheList.Clear();
                    await diskCache.RemoveDirectory(Container);
                }
                return bResult;
            }).AsAsyncOperation<bool>();
        }


        #endregion

        #region PrivateMethods
        private string GetSmoothStreamingUrl(string url)
        {
            string result = url;
            if (!string.IsNullOrEmpty(url))
            {
                result = url.Replace("https://", "ms-sstrs://");
                result = result.Replace("http://", "ms-sstr://");
            }
            return result;
        }
        private async Task<bool> Remove()
        {
            bool bResult = false;
            if (ManifestCacheList != null)
            {
                bResult = true;
                foreach (var value in ManifestCacheList)
                {
                    bool res = await diskCache.RemoveAsset(value.Value);
                    if (res != true)
                        bResult = false;
                }
            }
            return bResult;
        }
        private async Task<bool> RemoveAsset(Uri uri)
        {
            bool bResult = false;
            if (ManifestCacheList != null)
            {
                if (ManifestCacheList.ContainsKey(uri))
                {
                    bResult = await diskCache.RemoveAsset(ManifestCacheList[uri]);
                }
            }
            return bResult;
        }
        private async Task<bool> SaveAsset(Uri uri)
        {
            bool bResult = false;
            if (ManifestCacheList != null)
            {
                if (ManifestCacheList.ContainsKey(uri))
                {
                    bResult = await diskCache.SaveAsset(ManifestCacheList[uri]);
                }
            }
            return bResult;
        }
        private async Task<bool> RemoveManifest(Uri uri)
        {
            bool bResult = false;
            if (ManifestCacheList != null)
            {
                if (ManifestCacheList.ContainsKey(uri))
                {
                    bResult = await diskCache.RemoveManifest(ManifestCacheList[uri]);
                }
            }
            return bResult;
        }
        private async Task<bool> SaveManifest(Uri uri)
        {
            bool bResult = false;
            if (ManifestCacheList != null)
            {
                if (ManifestCacheList.ContainsKey(uri))
                {
                    bResult = await diskCache.SaveManifest(ManifestCacheList[uri]);
                }
            }
            return bResult;
        }
        private async Task<bool> SaveChunks(Uri uri)
        {
            bool bResult = false;
            if (ManifestCacheList != null)
            {
                if (ManifestCacheList.ContainsKey(uri))
                {
                    bResult = await diskCache.SaveAudioChunks(ManifestCacheList[uri]);
                    if (bResult)
                        bResult = await diskCache.SaveVideoChunks(ManifestCacheList[uri]);
                }
            }
            return bResult;
        }
        private async Task<bool> RemoveChunks(Uri uri)
        {
            bool bResult = false;
            if (ManifestCacheList != null)
            {
                if (ManifestCacheList.ContainsKey(uri))
                {
                    bResult = await diskCache.RemoveAudioChunks(ManifestCacheList[uri]);
                    if (bResult)
                        bResult = await diskCache.RemoveVideoChunks(ManifestCacheList[uri]);
                }
            }
            return bResult;
        }

        private void ManifestCache_StatusProgress(object sender, AssetStatus e)
        {
            if (StatusProgressEvent != null)
            {
                ManifestCache mc = sender as ManifestCache;
                if (mc != null)
                {
                    StatusProgressEvent(this, mc.ManifestUri,e);
                }
            }
        }

        private void ManifestCache_DownloadProgress(object sender, uint e)
        {
            if (DownloadProgressEvent != null)
            {
                ManifestCache mc = sender as ManifestCache;
                if (mc != null)
                {
                    DownloadProgressEvent(this, mc.ManifestUri, e);
                }
            }
        }
        private ManifestCache GetFromBaseUri(Uri baseUri)
        {
            string refstring = baseUri.ToString().ToLower();
            foreach (var mc in ManifestCacheList)
            {
                // fix potential issue 
                if (!string.IsNullOrEmpty(mc.Value.BaseUrl))
                {
                    if (refstring.StartsWith(mc.Value.BaseUrl, StringComparison.OrdinalIgnoreCase))
                        return mc.Value;
                }
            }
            return null;
        }
        private ManifestCache Get(Uri manifestUri)
        {
            if (ManifestCacheList.ContainsKey(manifestUri))
            {
                return ManifestCacheList[manifestUri];
            }
            return null;
        }
        private async void SmoothStreamingManager_ManifestReadyEvent(AdaptiveSource sender, ManifestReadyEventArgs args)
        {
            // Retrieve the ManifestCache from the MediaCache
            if (this.IsAssetInCache(sender.Uri))
            {
                // Select the Audio Stream
                List<Microsoft.Media.AdaptiveStreaming.IManifestStream> listStream = args.AdaptiveSource.Manifest.AvailableStreams.Where(t => (t.Type == MediaStreamType.Video)).ToList(); ;
                if (listStream!=null)
                {
                    int audioIndex = 0;
                    int selectedAudioIndex = this.GetSelectedAudioTrackIndex(sender.Uri);
                    foreach (var stream in args.AdaptiveSource.Manifest.AvailableStreams)
                    {
                        if (stream.Type == Microsoft.Media.AdaptiveStreaming.MediaStreamType.Audio)
                        {
                            if ((selectedAudioIndex == -1)||(selectedAudioIndex == audioIndex))
                            {
                                listStream.Add(stream);
                                await args.AdaptiveSource.Manifest.SelectStreamsAsync(listStream);
                                break;
                            }
                        }
                    }
                }
                // Select the video track with the expected bitrate
                foreach (var stream in args.AdaptiveSource.Manifest.SelectedStreams)
                {

                    if (stream.Type == Microsoft.Media.AdaptiveStreaming.MediaStreamType.Video)
                    {
                        // Restrict the bitrate to the cached bitrate
                        // Select the bitrate of the track which has been downloaded
                        IReadOnlyList<Microsoft.Media.AdaptiveStreaming.IManifestTrack> list = stream.AvailableTracks.Where(t => (t.Bitrate == this.GetVideoBitrate(sender.Uri))).ToList();
                        if ((list != null) && (list.Count > 0))
                            stream.RestrictTracks(list);
                    }
                }
            }
        }
        /// <summary>
        /// Method: Save
        /// Save the list of assets on disk
        /// </summary>
        private IAsyncOperation<bool> Save()
        {
            return Task.Run<bool>(async () =>
            {
                bool bResult = false;
                if (diskCache == null)
                    return false;
                if (ManifestCacheList != null)
                {
                    bResult = true;
                    foreach (var value in ManifestCacheList)
                    {
                        bool res = await diskCache.SaveAsset(value.Value);
                        if (res != true)
                            bResult = false;
                    }
                }
                return bResult;
            }).AsAsyncOperation<bool>();
        }

        /// <summary>
        /// Method: Restore
        /// Initialize the list of assets from the files and directories on disk
        /// </summary>
        private IAsyncOperation<bool> Restore()
        {
            return Task.Run<bool>(async () =>
            {
                bool bResult = false;
                if (diskCache == null)
                    return false;
                List<ManifestCache> list = await diskCache.RestoreAllAssets("*");
                if (list != null)
                {
                    if (ManifestCacheList == null)
                        ManifestCacheList = new ConcurrentDictionary<Uri, ManifestCache>();
                    if (ManifestCacheList != null)
                    {
                        bResult = true;
                        ManifestCacheList.Clear();
                        foreach (var cache in list)
                        {
                            cache.VideoDownloadedChunks = cache.VideoSavedChunks;
                            cache.VideoDownloadedBytes = cache.VideoSavedBytes;
                            cache.AudioDownloadedChunks = cache.AudioSavedChunks;
                            cache.AudioDownloadedBytes = cache.AudioSavedBytes;
                            bool res = ManifestCacheList.TryAdd(cache.ManifestUri, cache);
                            if (res != true)
                                bResult = false;
                            else
                            {
                                cache.SetDiskCache(diskCache);
                                cache.RestoreStatus();
                                cache.IsPlayReadyLicenseAcquired = false;
                                cache.DownloadProgress += ManifestCache_DownloadProgress;
                                cache.StatusProgress += ManifestCache_StatusProgress;
                            }
                        }
                    }
                }
                return bResult;
            }).AsAsyncOperation<bool>();
        }

        #endregion

        #region PrivateAttributes
        private string Container;
        private ConcurrentDictionary<Uri, ManifestCache> ManifestCacheList;
        private IAdaptiveSourceManager AdaptiveSrcManager;
        private Microsoft.Media.AdaptiveStreaming.AdaptiveSourceManager SmoothStreamingManager;
        private Windows.Media.MediaExtensionManager Extension = null;
        private DiskCache diskCache = null;
        #endregion

        #region IDownloaderPlugin

        private bool GetTimeAndType(Uri uri, out string type, out ulong time)
        {
            string[] url = uri.LocalPath.ToLower().Split('/');

            time = 0;
            type = string.Empty;
            DateTime time1 = DateTime.Now;
            try
            {
                if (uri.LocalPath.ToLower().Contains("/fragments("))
                {
                    //url = "fragments(audio=0)"
                    string[] keys = { "(", "=", ")" };
                    url = url[url.Length - 1].Split(keys, StringSplitOptions.RemoveEmptyEntries);

                    time = 0;
                    type = url[url.Length - 2];
                    return ulong.TryParse(url[url.Length - 1], out time);
                }
            }
            catch (Exception) {
            }

            return false;
        }
        /*
        // Not used
        public void CompleteTask(
          IAsyncOperation<DownloaderResponse> asyncInfo,
          AsyncStatus asyncStatus
        )
        {
            System.Diagnostics.Debug.WriteLine("CompleteTask");
            // System.Diagnostics.Debug.WriteLine("Request url completed ");
        }
        */
        public IAsyncOperation<DownloaderResponse> RequestAsync(DownloaderRequest pDownloaderRequest)
        {
            // This is a basic implimentation. The downloader plugin has multiple functions. In high level it has two functions. 
            // When you register for a downloader plugin all the out going http requests first send to the downloader plugin.
            // Downloader plugin can take two actions at that moment; response with data to the request or return a new URL or same URL.
            // When there is a data resposne, SDK uses this data and doesn't go to network. 
            // If the downloader respsonse with null data and with a URL (optionally it can include http headers and cookies) SDK uses the URl and goes to network and tries to download the data by own.
            // At that moment there will be a responsedata and this will be send to plugin. Plugin can use the resposne data to store for future usage.

            // The below sample has the basics just show how the plugin interface works. 
            // It simply gets the requests and return the excat URL back. When you get the request, you can download the data by yourself using any protocol or serve it from cache (disk/memory)including  and send the data back.
            // The API also alows you send different data to different quality requests and signal that you changed the data. 
            // DownloaderRequest Class AlternativeUris will identify which quality levels can be returned back to the request http://msdn.microsoft.com/en-us/library/jj822798(v=vs.90).aspx 
            // DownloaderResponse Class Uri-when you modify the quality you need to signal it using the Uri. You can also bypass the BypassBandwidthMeasurement  http://msdn.microsoft.com/en-us/library/jj822671(v=vs.90).aspx

            //System.Diagnostics.Debug.WriteLine("Smooth Streming Request Url: " + pDownloaderRequest.RequestUri.ToString());
            System.Diagnostics.Debug.WriteLine("RequestAsync");

            ManifestCache mc = this.Get(pDownloaderRequest.RequestUri);
            // Is Manifest request
            if ((mc != null)&&(mc.manifestBuffer!=null)&&(mc.manifestBuffer.Length>0))
            {
                System.Diagnostics.Debug.WriteLine("Cache received manifest request for Url : " + pDownloaderRequest.RequestUri.ToString());
                TaskCompletionSource <DownloaderResponse> respTmp = new TaskCompletionSource<DownloaderResponse>();
                if (respTmp.TrySetResult(new DownloaderResponse(pDownloaderRequest.RequestUri, (new MemoryStream(mc.manifestBuffer)).AsInputStream(), (ulong)mc.manifestBuffer.Length, "text/xml", null, true)) == true)
                {
                    Windows.Foundation.IAsyncOperation<DownloaderResponse> resp = respTmp.Task.AsAsyncOperation();
                  //  resp.Completed = CompleteTask;
                    System.Diagnostics.Debug.WriteLine("Manifest found in the cache, data: " + mc.manifestBuffer.Length.ToString() + " bytes for Url: " + pDownloaderRequest.RequestUri.ToString());
                    return resp;
                }
                System.Diagnostics.Debug.WriteLine("Manifest not found in the cache");
            }
            else
            {
                // Is Chunk request
                string BaseUrl = ManifestCache.GetBaseUri(pDownloaderRequest.RequestUri.ToString());
                mc = this.GetFromBaseUri(new Uri(BaseUrl));
                if (mc !=null)
                {
                    string type = string.Empty;
                    ulong time = 0;
                    if (GetTimeAndType(pDownloaderRequest.RequestUri, out type, out time))
                    {
                        System.Diagnostics.Debug.WriteLine("Cache received Chunk Request for time: " + time.ToString() + " url: " + pDownloaderRequest.RequestUri.ToString());
                        ChunkCache cc = null;
                        if(type.Equals(mc.VideoTemplateUrlType))
                            cc = mc.GetVideoChunkCache(time);
                        else if (type.Equals(mc.AudioTemplateUrlType))
                            cc = mc.GetAudioChunkCache(time);
                        if ((cc != null)&&(cc.GetLength()>0))
                        {
                            TaskCompletionSource<DownloaderResponse> respTmp = new TaskCompletionSource<DownloaderResponse>();
                            System.Collections.Generic.Dictionary<string, string> d = new System.Collections.Generic.Dictionary<string, string>();
                            d.Add("Content-Type", "video/mp4");
                            d.Add("Content-Length", cc.GetLength().ToString());

                            if (respTmp.TrySetResult(new DownloaderResponse(pDownloaderRequest.RequestUri, (new MemoryStream(cc.chunkBuffer)).AsInputStream(), (ulong)cc.GetLength(), "video/mp4", d, true)) == true)
                            {
                                Windows.Foundation.IAsyncOperation<DownloaderResponse> respb = respTmp.Task.AsAsyncOperation();
                             //   respb.Completed = CompleteTask;
                                System.Diagnostics.Debug.WriteLine("Chunk found in the cache, data: " + cc.GetLength().ToString() + " bytes for Url: " + pDownloaderRequest.RequestUri.ToString());
                                return respb;
                            }
                        }
                        else
                        {
                            byte[] buffer = null;
                            if (type.Equals(mc.VideoTemplateUrlType))
                                buffer = diskCache.GetVideoChunkBuffer(mc.StoragePath,time);
                            else if (type.Equals(mc.AudioTemplateUrlType))
                                buffer = diskCache.GetAudioChunkBuffer(mc.StoragePath, time);
                            if((buffer!=null)&&(buffer.Length>0))
                            {
                                TaskCompletionSource<DownloaderResponse> respTmp = new TaskCompletionSource<DownloaderResponse>();
                                System.Collections.Generic.Dictionary<string, string> d = new System.Collections.Generic.Dictionary<string, string>();
                                d.Add("Content-Type", "video/mp4");
                                d.Add("Content-Length", cc.GetLength().ToString());

                                if (respTmp.TrySetResult(new DownloaderResponse(pDownloaderRequest.RequestUri, (new MemoryStream(buffer)).AsInputStream(), (ulong)buffer.Length, "video/mp4", d, true)) == true)
                                {
                                    Windows.Foundation.IAsyncOperation<DownloaderResponse> respb = respTmp.Task.AsAsyncOperation();
                                    //   respb.Completed = CompleteTask;
                                    System.Diagnostics.Debug.WriteLine("Chunk found in the cache, data: " + buffer.Length.ToString() + " bytes for Url: " + pDownloaderRequest.RequestUri.ToString());
                                    return respb;
                                }
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("Chunk not found in the cache");
                }
            }
            System.Diagnostics.Debug.WriteLine("Default behavior (no cache) for url: " + pDownloaderRequest.RequestUri.ToString());
            TaskCompletionSource<DownloaderResponse> respTmpb = new TaskCompletionSource<DownloaderResponse>();
            DownloaderResponse dr = new DownloaderResponse(pDownloaderRequest.RequestUri, null, null);
            respTmpb.TrySetResult(dr);
            Windows.Foundation.IAsyncOperation<DownloaderResponse> respc = respTmpb.Task.AsAsyncOperation();
            //  respc.Completed = CompleteTask;
            return respc;
        }

        public void ResponseData(DownloaderRequest pDownloaderRequest, DownloaderResponse pDownloaderResponse)
        {
            System.Diagnostics.Debug.WriteLine("ResponseData");
            // you can use the return data and store it in the disk.

            // Application might hold the instance of ResponseStream for a while and dispose it later, it is up to Application’s implementation.
            // Once app knows it doesn’t need the response stream any more, it can add below code to dispose the ResponseStream;

            if (pDownloaderResponse != null && pDownloaderResponse.ResponseStream != null)
            {
                System.Diagnostics.Debug.WriteLine("Response for url: " + pDownloaderResponse.Uri + " Content: " + pDownloaderResponse.ContentType.ToString() + " length: " + pDownloaderResponse.ContentLength.ToString());
               // pDownloaderResponse.ResponseStream.Dispose();
                //System.Diagnostics.Debug.WriteLine("Response for url: ");
            }


        }




        private void AdaptiveSrcManager_AdaptiveSourceOpenedEvent(AdaptiveSource sender, AdaptiveSourceOpenedEventArgs args)
        {
            // Not used yet
        }

        private void AdaptiveSrcManager_AdaptiveSourceClosedEvent(AdaptiveSource sender, AdaptiveSourceClosedEventArgs args)
        {
            // Not used yet
        }
        #endregion

    }
}
