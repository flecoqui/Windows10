
# Universal Windows Platform (UWP) Adaptive Media Cache C# Sample Application

Overview
--------------
This Universal Windows Platform (UWP) Adaptive Media Cache C# Sample Application can play Smooth Streaming, HLS and DASH streams. Moreover, this application can download Smooth Streaming assets to support Download-To-Go and Progressive Download use cases.
The application relies on the Adaptive Media Cache UWP Component which mmanages a local cache where the Smooth Streaming assets are downloaded. 
Once the Smooth Streaming assets is downloaded, the user can play the asset even if the device running Windows 10 is off-line.
Moreover, those media assets can be protected with DRM like PlayReady.
This sample application does support the following containers:<p/>
	-   **Video**: VMV, MP4, MPEG2-TS, MKV, HLS, MPEG-DASH, Smooth Streaming</p> 
	-   **Audio**: WMA, MP3, FLAC</p>
	-   **Picture**: JPG, PNG</p>

Only the Smooth Streaming assets can be downloaded.
When the application is launched, it opens a JSON playlist which contains the list of files to play.
This JSON playlist is by default embedded within the application, the user can select another playlist to play his own content. 

This version of Universal Media Player Windows 10 application is an evolution of the [Anniversary Update (RS1) Universal Media Player Windows 10 application](https://github.com/flecoqui/Windows10/raw/master/Samples/RS1UniversalMediaPlayer/).
This Adaptive Media Cache component has been integrated with several Universal Windows Platform Applications currently deployed.


Installing the application
----------------------------
You can install the application on:<p/>
	- **Personal Computer Platform**: a desktop running Windows 10 Anniversary Update (RS1)</p>
	- **Windows 10 Mobile Platform**: a phone running Windows 10 Anniversary Update (RS1)</p>
	- **IOT Platform**: a IOT device running Windows 10 Anniversary Update (RS1)</p>
	- **XBOX One**: a XBOX One running Windows 10 Anniversary Update (RS1)</p>
	- **Hololens**: an Hololens running Windows 10 Anniversary Update (RS1)</p>


You can install the applications using the packages for x86, x64 and ARM which are available there :
[ZIP file of the application x86, x64, ARM Packages](https://github.com/flecoqui/Windows10/raw/master/Samples/AdaptiveMediaCache/Releases/LatestRelease.zip)


**Installing the application with the application package on Personal Computer:**

1.  Download the ZIP file on your computer harddrive
2.  Unzip the ZIP file
3.  Launch the PowerShell file Add-AppPackage.ps1. The PowerShell script will install the application on your computer running Windows 10


**Installing the application with the application package on Phone:**

1.  Connect the phone running Windows 10 Mobile to your computer with a USB Cable.
2.  After few seconds, you should see the phone storage with Windows Explorer running on your computer
3.  Copy the application packages on your phone storage, for instance inthe Downloads folder
4.  On the phone install a File Explorer application from Windows Store
5.  With File Explorer on the phone navigate to the folder where the packages have been copied
6.  Tap on the file AudioVideoPlayer_1.0.XX.O_x86_x64_arm.cer to install the certificate.
7.  Tap on the file AudioVideoPlayer_1.0.XX.O_x86_x64_arm.appxbundle to install the application


**Installing the application with the application package on IOT:**
<p/>
You can use [WinAppDeployCmd.exe](https://blogs.windows.com/buildingapps/2015/07/09/just-released-windows-10-application-deployment-tool/) to deploy the application on your IOT device.
As the application can play video assets check that the IOT Platform does support Video Hardware acceleration to get a smooth user experience.



Using the application
----------------------------
Once the application is installed on your device, you can launch it and the main page will be displayed after few seconds.

### Main page

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/mainpage.png)

The application is used to play videos, audios and photos. By default you can select in the combo box `Select a stream` the asset you can to play.   

### Selecting the asset
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/listassets.png)

You can also the 2 buttons below to navigate in the list of assets and select a new asset:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/select.png)

### Entering the asset's url
Once the asset is selected the `URL` field is updated with the url associated with the asset. You can update manually this field beofre playing the asset if you want to test a specific url. 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/url.png)


### Fullscreen mode

Once the asset is being played by the application you can change the application display mode:<p/>
	- **Window Mode**: where all the controls are visible on the main page of the application </p>
	- **Fullscreen Mode**: where the MediaElement (player) covers the entire screen </p>
	- **Full Window Mode**: where the MediaElement (player) covers the main page of the application (Desktop only)</p>


With the first button below, you can switch to Fullscreen mode. Press a key or double tap to switch back to Window Mode.

With the second button below, you can switch to Full Window mode. Press a key or double tap to switch back to Window Mode.
 
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/windowmode.png)

### Mute and Audio level

Once the application is playing an audio asset or a video asset it's possible to switch off the audio (`Mute` button) or change the audio output level (`Audio+` and `Audio-` button)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/audio.png)


### Start over, play, pause and stop 

Once the application is playing an audio asset or a video asset it's possible to:
<p/>
	- pause/play the current asset</p>
	- start over the current asset</p>
	- stop the current asset </p>

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/playpause.png)

### Bitrate selection
Before playing a Smooth Streaming asset, an HLS asset and an MPEG DASH asset you can select the bitrate using the fields `Bitrate Min` and `Bitrate Max`, the unit is bit per second.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/bitrate.png)

### Auto mode
By default when you launch the application, the application will load the JSON playlist used before closing the application and select the asset selected before closing the application.
If you checked the check box `Auto` before closing the application, the application will automatically start to play the selected asset in the window mode used before closing the application.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/auto.png)


### Selecting another JSON Playlist
By defaut the application uses the [JSON Playlist](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/cs/AudioVideoPlayer/DataModel/MediaData.json) embedded within the application.
However you can select another JSON Playlist by selecting another JSON file stored on the device storage. To select another JSON file click on the button below:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/open.png)

Below the format of the items in the JSON Playlist: </p>
	- **UniqueId**: a unique ID associated with the item</p>
	- **Comment**: a comment associated with the item</p>
	- **Title**: the title of the item which will be displayed</p>
	- **ImagePath**: the path to the image which describes the type of item, for instance: MP4, HLS, .... It can be an http uri or a local uri with the prefix: "ms-appx://" for instance: "ms-appx:///Assets/WMV.png".</p>
	- **Description**: the description of this item</p>
	- **Content**: the path to the item, it can an http uri, a local file uri for instance: "picture://myfolder/myposter.jpg" for a jpg file in the picture folder of your device. You can use the following prefixes: "file://",  "picture://", "music://", "video://".</p>  
	- **PosterContent**: the path to an image associated with the item. For instance, if the content is an audio file, a radio uri the poster will be displayed while playing the audio item. </p> 
	- **Start**: the start position to play the video or audio item in milliseconds.</p>
	- **Duration**: the play duration for the item in milliseconds.</p>
	- **HttpHeaders**: this field can define the http header with the following syntax: "{[HttpHeader1]: [HttpHeaderValue1]},{[HttpHeader2]: [HttpHeaderValue2]},...,{[HttpHeaderN]: [HttpHeaderValueN]}"</p>
	                    Moreover, for Azure Media Service SWT Token or JWT Token you can use the following syntax:</p>
						"{Authorization: Bearer:[JWT/SWT Token]}"</p>
	- **PlayReadyUrl**: the PlayReady license acquisition url if the content is protected with PlayReady.</p>
	- **PlayReadyCustomData**: the PlayReady custom data if the content is protected with PlayReady.</p>
	- **BackgroundAudio**: "true" if the audio item must be played in background audio mode (not used)</p>


Sample item for a Smooth Streaming video protected with PlayReady over HTTP and specific Http Headers:

        {
          "UniqueId": "video_playready_http_1",
          "Comment": "#101",
          "Title": "SMOOTH PlayReady VC1 Video (Expiration Date)",
          "ImagePath": "ms-appx:///Assets/SMOOTH.png",
          "Description": "SMOOTH Video over HTTP 1",
          "Content": "http://playready.directtaps.net/smoothstreaming/TTLSS720VC1PR/To_The_Limit_720.ism/Manifest",
          "PosterContent": "",
          "Start": "0",
          "Duration": "0",
          "HttpHeaders": "{test: testHLSHeader},{test2: TestHLSHeader}",
          "PlayReadyUrl": " http://playready.directtaps.net/pr/svc/rightsmanager.asmx?PlayRight=1&FirstPlayExpiration=600",
          "PlayReadyCustomData": "",
          "BackgroundAudio": false
        },

### Download-To-Go scenario 
The key feature of this sample Universal Windows Platform (UWP) application is the support of Download-To-Go scenario for Smooth Streaming assets.</p>
**Playing assets while your device if offline**</p>
This scenario consists in downloading the Smooth Streaming asset, once the asset is downloaded you can play the asset even if the network connection is not available.</p>
**Playing HD content when your Internet connection doesn't support high bandwidth**</p>
This scenario consists in downloading the highest resolution track of the Smooth Streaming asset when your Internet connection doesn't support such a bandwidth. With this approach you can play high quality video with low bitrate Internet connection.

To test this feature, you can follow the steps below:</p>

1. Select a Smooth Streaming Asset in the Combo Box "Select a stream" or enter the url of a Smooth Streaming asset in the Edit Box "URL:"

    ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download1.png)

2. Check the Check Box "Download-To-Go" to enable the Download-To-Go scenario.
3. You can also select the video track you want to download if you enter the minimum and maximum bitrate. The application will select the track with the highest bitrate in the range of bitrate you defined.
4. Now you can click on Download button to launch the download.

    ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download2.png)

5. In the logs you can see the selected video track and the selected audio track and the progress of the download. 

    ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download3.png)

6. Once the download is completed, the buttons Delete, PlayReady Acquisition and Play are enabled.

    ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download4.png)

7. If the content is protected with PlayReady, you can acquire manually the PlayReady license when clicking on PlayReady Acquisition button.

    ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download5.png)

8. Once the asset is downloaded and the PlayReady license acquired, you can play the asset even if your PC is not connected to Internet. Click on the Play button:

    ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download6.png)

9. As the downloaded asset is stored on your local hard drive, you can still remove the asset from the hard drive when clicking on the button Remove:

    ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download7.png)

### Progressive Download scenario 
The Progressive Download scenario is applicable when the bandwidth of your Internet connection is too low to support high quality video. With this approach the user can play high quality video track once a sufficent amount of video/audio chunks are downloaded to start to play the asset locally.</p>
**Playing HD content when your Internet connection doesn't support high bandwidth**</p>
This scenario consists in downloading the highest resolution track of the Smooth Streaming asset when your Internet connection doesn't support such a bandwidth. With this approach you can play high quality video with low bitrate Internet connection.

To test this feature, you can follow the steps below:

1. Select a Smooth Streaming Asset in the Combo Box "Select a stream" or enter the url of a Smooth Streaming asset in the Edit Box "URL:"

   ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download8.png)

2. Uncheck the Check Box "Download-To-Go" to enable the Progressive Download scenario.
3. You can also select the video track you want to download if you enter the minimum and maximum bitrate. The application will select the track with the highest bitrate in the range of bitrate you defined.
4. Now you can click on Download button to launch the download.
5. In the logs you can see the selected video track and the selected audio track and the progress of the download. 
6. Once a sufficent number of audio/video chunks is downloaded, the buttons Delete, PlayReady Acquisition and Play are enabled.
7. If the content is protected with PlayReady, you can acquire manually the PlayReady license when clicking on PlayReady Acquisition button.

   ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download5.png)

8. You can play the asset, your PC needs to be connected to Internet. Click on the Play button:

   ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download6.png)

9. You can also remove the asset from the hard drive when clicking on the button Remove:

   ![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/AdaptiveMediaCache/Docs/download7.png)


Under the Surface
----------------

### Adaptive Media Cache: Parsing the manifest 

When the application is playing audio over http in background mode (Single Process Background Audio), the network may be not available after few minutes preventing the application from downloading the next audio file.
In order to keep the network when the application is in background mode, you need to use the MediaBinder and the Binding event.
Check method BookNetworkForBackground in the file MainPage.xaml.cs:

        Windows.Media.Playback.MediaPlayer localMediaPlayer = null;
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

The method ParseDashManifest() needs to be implemented to support Download-To-Go with Dash assets:

        public async Task<bool> ParseDashManifest()
        {
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
            }
            return false;
        }

The method ParseHLSManifest() needs to be implemented to support Download-To-Go with HLS assets:

        public async Task<bool> ParseHLSManifest()
        {
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
            }
            return false;
        }


### Adaptive Media Cache: Cache implementation
The cache is implemented in the AdaptiveMediaCache WinRT component.
The cache which could be used to store several assets in a specific folder under the Application Data folder (Windows.Storage.ApplicationData.Current).

The audio and video chunks of a video asset are stored in a subfolder under the Application Data folder (Windows.Storage.ApplicationData.Current).

    <Application Data folder>
        <ROOT folder>
              <ASSET 1 folder>
              <ASSET 2 folder>
              <ASSET 3 folder>
              <ASSET 4 folder>
			      <Manifest file>
			      <AudioIndex file>
			      <AudioContent file>
			      <VideoIndex file>
			      <VideoContent file>
              <ASSET N-1 folder>
              <ASSET N folder>

Below the names of the different files used by the application:

        private const string manifestFileName = "manifest.xml";
        private const string audioIndexFileName = "AudioIndex";
        private const string videoIndexFileName = "VideoIndex";
        private const string audioContentFileName = "Audio";
        private const string videoContentFileName = "Video";

Below the source code of the method which reads the asset in the cache on the local hard drive:

        public async Task<List<ManifestCache>> RestoreAllAssets(string pattern)
        {
            List<ManifestCache> downloads = new List<ManifestCache>();
            List<string> dirs = await GetDirectoryNames(root);
            if (dirs != null)
            {
                for (int i = 0; i < dirs.Count; i++)
                {
                    string path = Path.Combine(root, dirs[i]);
                    if (!string.IsNullOrEmpty(path))
                    {
                        string file = Path.Combine(path, manifestFileName);
                        if (!string.IsNullOrEmpty(file))
                        {
                            ManifestCache de = await GetObjectByType(file, typeof(ManifestCache)) as ManifestCache;
                            if (de != null)
                            {
                                // Sanity check are the manifest file and chunk files consistent
                                if ((de.AudioSavedChunks == (ulong)(await GetFileSize(Path.Combine(path, audioIndexFileName)) / indexSize)) &&
                                    (de.VideoSavedChunks == (ulong)(await GetFileSize(Path.Combine(path, videoIndexFileName)) / indexSize)))
                                {
                                    downloads.Add(de);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache - RestoreAllAssets - Manifest and Chunk file not consistent for path: " + path.ToString());
                                }
                            }
                        }
                    }
                }
            }
            return downloads;
        }


The audio or the video index files are file which contains a table of a structure which includes:</p>
- The timestamp of the audio/video chunks (8 bytes)</p>
- The offset of the chunk associated in the audio/video content file (8 bytes)</p>
- The size of the chunk (4 bytes)</p>

Below the structure of the index file:

        public ulong Time;
        public ulong Offset;
        public UInt32 Size;

Below the method used to retrieve the audio/video chunk: 

        private async Task<byte[]> GetChunkBuffer(bool isVideo, string path, ulong time)
        {
            byte[] buffer = null;
            string dir = Path.Combine(root, path);
            if (!string.IsNullOrEmpty(dir))
            {
                string indexFile = Path.Combine(dir, (isVideo == true ? videoIndexFileName : audioIndexFileName));
                string contentFile = Path.Combine(dir, (isVideo == true ? videoContentFileName : audioContentFileName));
                if ((!string.IsNullOrEmpty(contentFile))&&
                    (!string.IsNullOrEmpty(indexFile)))
                {

                    using (var releaser = (isVideo == true ? await internalVideoDiskLock.ReaderLockAsync(): await internalAudioDiskLock.ReaderLockAsync()))
                    {
                        ulong offset = 0;
                        ulong size = 20;
                        ulong fileSize = await GetFileSize(indexFile);
                        while (offset < fileSize)
                        {
                            byte[] b = await Restore(indexFile, offset, size);
                            IndexCache ic = new IndexCache(b);
                            if (ic != null)
                            {
                                if (ic.Time == time)
                                {
                                    buffer = await Restore(contentFile, ic.Offset, ic.Size);
                                    break;
                                }
                            }
                            offset += size;
                        }
                    }
                }
            }
            return buffer;
        }


Building the application
----------------

**Prerequisite: Windows Smooth Streaming Client SDK**
This version is based on the latest [Universal Smooth Streaming Client SDK](https://visualstudiogallery.msdn.microsoft.com/1e7d4700-7fa8-49b6-8a7b-8d8666685459)

1. If you download the samples ZIP, be sure to unzip the entire archive, not just the folder with the sample you want to build. 
2. Ensure the Anniversary Update (RS1) Windows 10 SDK is installed on your machine
3. Start Microsoft Visual Studio 2015 and select **File** \> **Open** \> **Project/Solution**.
3. Starting in the folder where you unzipped the samples, go to the Samples subfolder, then the subfolder for this specific sample, then the subfolder for your preferred language (C++, C#, or JavaScript). Double-click the Visual Studio 2015 Solution (.sln) file.
4. Press Ctrl+Shift+B, or select **Build** \> **Build Solution**.


**Deploying and running the sample**
1.  To debug the sample and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the sample without debugging, press Ctrl+F5 or select **Debug** \> **Start Without Debugging**.



Next steps
--------------

The Universal Media Player C# Sample Applicaton could be improved to support the following features:</p>
1.  Support of HLS Download-To-Go and Progressive Download use case</p>
2.  Support of DASH Download-To-Go and Progressive Download use case</p>

 

