<!---
  category: AudioVideoAndCamera
  samplefwlink: http://go.microsoft.com/fwlink/p/?LinkId=620563&clcid=0x409
--->

# RS1 Universal Media Player C# Sample Application

Overview
--------------
This RS1 Universal Media Player Windows 10 application can play video files and audio files, display pictures as well.
This sample application does support the following containers:
<p/>
-   **Video**: VMV, MP4, MPEG2-TS, HLS, MPEG-DASH, Smooth Streaming 
-   **Audio**: WMA, MP3, FLAC
-   **Picture**: JPG, PNG

If the MPEG-DASH assets or the Smooth Streaming assets are protected with PlayReady, the application could play those assets. 
When the application is launched, it opens a JSON playlist which contains the list of files to play.
This JSON playlist is by default embedded within the application, the user could select another playlist to play his own content. 

This RS1 Universal Media Player Windows 10 application is an evolution of the [TH2 Universal Media Player Windows 10 application](https://github.com/flecoqui/Windows10/raw/master/Samples/UniversalMediaPlayer/).
This version of the application introduce new features related to Windows 10 Red Stone 1:
- the support of single process background audio
and new features like:
- the support of playback of content stored on a USB devices connected to your desktop or to your XBOX One.
- the support of custom http header while downloading on-line video 



Installing the application
----------------------------
You can install the application on:
<p/>
- **Personal Computer Platform**: a desktop running Windows 10 RS1
- **Windows 10 Mobile Platform**: a phone running Windows 10 RS1
- **IOT Platform**: a IOT device running Windows 10 RS1
- **XBOX One**: a XBOX One running Windows 10 RS1

The applications packages for x86, x64 and ARM are available there :
[ZIP file of the application x86, x64, ARM Packages](https://github.com/flecoqui/Windows10/raw/master/Samples/RS1UniversalMediaPlayer/Releases/LatestRelease.zip)


**Personal Computer installation:**

1.  Download the ZIP file on your computer harddrive
2.  Unzip the ZIP file
3.  Launch the PowerShell file Add-AppPackage.ps1. The PowerShell script will install the application on your computer running Windows 10


**Phone installation:**

1.  Connect the phone running Windows 10 Mobile to your computer with a USB Cable.
2.  After few seconds, you should see the phone storage with Windows Explorer running on your computer
3.  Copy the application packages on your phone storage, for instance inthe Downloads folder
4.  On the phone install a File Explorer application from Windows Store
5.  With File Explorer on the phone navigate to the folder where the packages have been copied
6.  Tap on the file AudioVideoPlayer_1.0.XX.O_x86_x64_arm.cer to install the certificate.
7.  Tap on the file AudioVideoPlayer_1.0.XX.O_x86_x64_arm.appxbundle to install the application


**IOT installation:**
<p/>
You can use [WinAppDeployCmd.exe](https://blogs.windows.com/buildingapps/2015/07/09/just-released-windows-10-application-deployment-tool/) to deploy the application on your IOT device.
As the application can play video assets check that the IOT Platform does support Video Hardware acceleration to get a smooth user experience.



Using the application
----------------------------
Once the application is installed on your device, you can launch it and the main page will be displayed after few seconds.

### Main page

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/Docs/mainpage.png)

The application is used to play videos, audios and photos. By default you can select in the combo box `Select a stream` the asset you can to play.   

### Selecting the asset
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/Docs/listassets.png)

You can also the 2 buttons below to navigate in the list of assets and select a new asset:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/Docs/select.png)

### Entering the asset's url
Once the asset is selected the `URL` field is updated with the url associated with the asset. You can update manually this field beofre playing the asset if you want to test a specific url. 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/Docs/url.png)


### Fullscreen mode

Once the asset is being played by the application you can change the application display mode:
<p/>
- **Window Mode**: where all the controls are visible on the main page of the application
- **Fullscreen Mode**: where the MediaElement (player) covers the entire screen
- **Full Window Mode**: where the MediaElement (player) covers the main page of the application (Desktop only)
<p/>

With the first button below, you can switch to Fullscreen mode. Press a key or double tap to switch back to Window Mode.

With the second button below, you can switch to Full Window mode. Press a key or double tap to switch back to Window Mode.
 
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/Docs/windowmode.png)

### Mute and Audio level

Once the application is playing an audio asset or a video asset it's possible to switch off the audio (`Mute` button) or change the audio output level (`Audio+` and `Audio-` button)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/Docs/audio.png)


### Start over, play, pause and stop 

Once the application is playing an audio asset or a video asset it's possible to:
<p/>
- pause/play the current asset
- start over the current asset
- stop the current asset 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/Docs/playpause.png)

### Bitrate selection
Before playing a Smooth Streaming asset, an HLS asset and an MPEG DASH asset you can select the bitrate using the fields `Bitrate Min` and `Bitrate Max`, the unit is bit per second.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/Docs/bitrate.png)

### Auto mode
By default when you launch the application, the application will load the JSON playlist used before closing the application and select the asset selected before closing the application.
If you checked the check box `Auto` before closing the application, the application will automatically start to play the selected asset in the window mode used before closing the application.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/cs/Docs/auto.png)


### Selecting another JSON Playlist
By defaut the application uses the [JSON Playlist](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/cs/AudioVideoPlayer/DataModel/MediaData.json) embedded within the application.
However you can select another JSON Playlist by selecting another JSON file stored on the device storage. To select another JSON file click on the button below:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/UniversalMediaPlayer/Docs/open.png)

<p/>
- **UniqueId** : a unique ID associated with the item
- **Comment** : a comment associated with the item
- **Title** : the title of the item which will be displayed
- **ImagePath** : the path to the image which describes the type of item, for instance: MP4, HLS, .... It can be an http uri or a local uri with the prefix: "ms-appx://" for instance: "ms-appx:///Assets/WMV.png".
- **Description** : the description of this item
- **Content** : the path to the item, it can an http uri, a local file uri for instance: "picture://myfolder/myposter.jpg" for a jpg file in the picture folder of your device. You can use the following prefixes: "file://",  "picture://", "music://", "video://".   
- **PosterContent** : the path to an image associated with the item. For instance, if the content is an audio file, a radio uri the poster will be displayed while playing the audio item.  
- **Start** : the start position to play the video or audio item in milliseconds.
- **Duration** : the play duration for the item in milliseconds.
- **PlayReadyUrl** : the PlayReady license acquisition url if the content is protected with PlayReady.
- **PlayReadyCustomData** : the PlayReady custom data if the content is protected with PlayReady.
- **BackgroundAudio** : "true" if the audio item must be played in background audio mode (not implemented yet)

Sample item for a WMV video over HTTP:

        {
          "UniqueId": "video_wmv_http_1",
          "Comment": "#1",
          "Title": "WMV Video over HTTP 1",
          "ImagePath": "ms-appx:///Assets/WMV.png",
          "Description": "WMV Video over HTTP 1",
          "Content": "http://VMBasicA1.cloudapp.net/testwmv/test1.wmv",
          "PosterContent": "",
          "Start": "0",
          "Duration": "0",
		  "HttpHeaders": "",
          "PlayReadyUrl": "",
          "PlayReadyCustomData": "",
          "BackgroundAudio": false
        },

Sample item for a Smooth Streaming video protected with PlayReady over HTTP:

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

Sample item for a photo over HTTP which will be displayed during 10 seconds:

        {
          "UniqueId": "photo_http_2",
          "Comment": "#112",
          "Title": "Photo over HTTP 2",
          "ImagePath": "ms-appx:///Assets/PHOTO.png",
          "Description": "Photo over HTTP 2",
          "Content": "http://ia.media-imdb.com/images/M/MV5BMzc5NTUzNTgzMF5BMl5BanBnXkFtZTcwODcwMzQ5Mw@@._V1_SY317_CR5,0,214,317_AL_.jpg",
          "PosterContent": "",
          "Start": "0",
          "Duration": "10000",
		  "HttpHeaders": "",
          "PlayReadyUrl": "",
          "PlayReadyCustomData": "",
          "BackgroundAudio": false
        },

Sample item for a photo stored in the MyPictures folder on the device running Windows 10:

        {
          "UniqueId": "photo_file_3",
          "Comment": "#113",
          "Title": "Photo local file",
          "ImagePath": "ms-appx:///Assets/PHOTO.png",
          "Description": "Photo over HTTP 3",
          "Content": "picture://MyFolder\\poster1.jpg",
          "PosterContent": "",
          "Start": "0",
          "Duration": "10000",
		  "HttpHeaders": "",
          "PlayReadyUrl": "",
          "PlayReadyCustomData": "",
          "BackgroundAudio": false
        }
Under the Surface
----------------

### Getting System Information

to be completed

### PlayReady: Getting license expiration date 

to be completed

### PlayReady: Forcing Software DRM

to be completed



Building the application
----------------

**Prerequisite: Windows Smooth Streaming Client SDK**

1. If you download the samples ZIP, be sure to unzip the entire archive, not just the folder with the sample you want to build. 
2. Ensure the Red Stone 1 (RS1) Windows 10 SDK is installed on your machine
3. Start Microsoft Visual Studio 2015 and select **File** \> **Open** \> **Project/Solution**.
3. Starting in the folder where you unzipped the samples, go to the Samples subfolder, then the subfolder for this specific sample, then the subfolder for your preferred language (C++, C#, or JavaScript). Double-click the Visual Studio 2015 Solution (.sln) file.
4. Press Ctrl+Shift+B, or select **Build** \> **Build Solution**.


**Deploying and running the sample**
1.  To debug the sample and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the sample without debugging, press Ctrl+F5 or select**Debug** \> **Start Without Debugging**.




Next steps
--------------

The Universal Media Player C# Sample Applicaton could be improved to support the following features:
<p/>
1.  Support of Azure Media 
 



