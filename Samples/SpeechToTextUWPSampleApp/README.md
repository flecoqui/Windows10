<!---
  category: AudioVideoAndCamera
  samplefwlink: http://go.microsoft.com/fwlink/p/?LinkId=620563&clcid=0x409
--->

# SpeechToTextUWPSampleApp Universal Media Player C# Sample Application

Overview
--------------
This SpeechToTextUWPSampleApp  Universal Media Player Application can play video files and audio files, display pictures as well.
This sample application does support the following containers:
<p/>
-   **Video**: VMV, MP4, MPEG2-TS, HLS, MPEG-DASH, Smooth Streaming 
-   **Audio**: WMA, MP3, FLAC
-   **Picture**: JPG, PNG

If the MPEG-DASH assets or the Smooth Streaming assets are protected with PlayReady, the application could play those assets. 
When the application is launched, it opens a JSON playlist which contains the list of files to play.
This JSON playlist is by default embedded within the application, the user could select another playlist to play his own content. 

This RS1 Universal Media Player Windows 10 application is an evolution of the [TH2 Universal Media Player Windows 10 application](https://github.com/flecoqui/Windows10/raw/master/Samples/SpeechToTextUWPSampleApp/).
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
[ZIP file of the application x86, x64, ARM Packages](https://github.com/flecoqui/Windows10/raw/master/Samples/SpeechToTextUWPSampleApp/Releases/LatestRelease.zip)


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

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/mainpage.png)

The application is used to play videos, audios and photos. By default you can select in the combo box `Select a stream` the asset you can to play.   

### Selecting the asset
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/listassets.png)

You can also the 2 buttons below to navigate in the list of assets and select a new asset:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/select.png)

### Entering the asset's url
Once the asset is selected the `URL` field is updated with the url associated with the asset. You can update manually this field beofre playing the asset if you want to test a specific url. 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/url.png)


### Fullscreen mode

Once the asset is being played by the application you can change the application display mode:
<p/>
- **Window Mode**: where all the controls are visible on the main page of the application
- **Fullscreen Mode**: where the MediaElement (player) covers the entire screen
- **Full Window Mode**: where the MediaElement (player) covers the main page of the application (Desktop only)
<p/>

With the first button below, you can switch to Fullscreen mode. Press a key or double tap to switch back to Window Mode.

With the second button below, you can switch to Full Window mode. Press a key or double tap to switch back to Window Mode.
 
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/windowmode.png)

### Mute and Audio level

Once the application is playing an audio asset or a video asset it's possible to switch off the audio (`Mute` button) or change the audio output level (`Audio+` and `Audio-` button)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/audio.png)


### Start over, play, pause and stop 

Once the application is playing an audio asset or a video asset it's possible to:
<p/>
- pause/play the current asset
- start over the current asset
- stop the current asset 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/playpause.png)

### Bitrate selection
Before playing a Smooth Streaming asset, an HLS asset and an MPEG DASH asset you can select the bitrate using the fields `Bitrate Min` and `Bitrate Max`, the unit is bit per second.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/bitrate.png)

### Auto mode
By default when you launch the application, the application will load the JSON playlist used before closing the application and select the asset selected before closing the application.
If you checked the check box `Auto` before closing the application, the application will automatically start to play the selected asset in the window mode used before closing the application.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/auto.png)


### Selecting another JSON Playlist
By defaut the application uses the [JSON Playlist](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/cs/AudioVideoPlayer/DataModel/MediaData.json) embedded within the application.
However you can select another JSON Playlist by selecting another JSON file stored on the device storage. To select another JSON file click on the button below:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/open.png)

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

Check SystemInformation constructor in the file SystemInformation.cs:

        static SystemInformation()
        {
            // get the system family name
            AnalyticsVersionInfo ai = AnalyticsInfo.VersionInfo;
            SystemFamily = ai.DeviceFamily;

            // get the system version number
            string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (v & 0x000000000000FFFFL);
            SystemVersion = $"{v1}.{v2}.{v3}.{v4}";

            // get the package architecure
            Package package = Package.Current;
            SystemArchitecture = package.Id.Architecture.ToString();

            // get the user friendly app name
            ApplicationName = package.DisplayName;

            // get the app version
            PackageVersion pv = package.Id.Version;
            ApplicationVersion = $"{pv.Major}.{pv.Minor}.{pv.Build}.{pv.Revision}";

            // get the device manufacturer and model name
            EasClientDeviceInformation eas = new EasClientDeviceInformation();
            DeviceManufacturer = eas.SystemManufacturer;
            DeviceModel = eas.SystemProductName;

            // get App Specific Hardware ID
            AppSpecificHardwareID = GetAppSpecificHardwareID();
        }

### PlayReady: Getting license expiration date 

Check method GetLicenseExpirationDate in the file MainPage.xaml.cs:

        private DateTime GetLicenseExpirationDate(Guid videoId)
        {
            
            var keyIdString = Convert.ToBase64String(videoId.ToByteArray());
            try
            {
                var contentHeader = new Windows.Media.Protection.PlayReady.PlayReadyContentHeader(
                    videoId,
                    keyIdString,
                    Windows.Media.Protection.PlayReady.PlayReadyEncryptionAlgorithm.Aes128Ctr,
                    null,
                    null,
                    string.Empty,
                    new Guid());
                Windows.Media.Protection.PlayReady.IPlayReadyLicense[] licenses = new Windows.Media.Protection.PlayReady.PlayReadyLicenseIterable(contentHeader, true).ToArray();
                foreach (var lic in licenses)
                {
                    DateTimeOffset? d = MediaHelpers.PlayReadyHelper.GetLicenseExpirationDate(lic);
                    if((d!=null)&&(d.HasValue))
                            return d.Value.DateTime;
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("GetLicenseExpirationDate Exception: " + e.Message);
                return DateTime.MinValue;
            }
            return DateTime.MinValue;
        }


### PlayReady: Forcing Software DRM

By default the UWP App using PlayReady do support Hardware DRM if the platform does support DRM.
Unfortunately, Hardware DRM is not supported for VC-1 codec. If your UWP application needs to play a VC-1 stream protected with PlayReady the hardware DRM needs to be disable for this stream.
Moreover, the hardware DRM needs to be enabled if the content is an H.264 or H.265 stream.

Check method EnableSoftwareDRM in the file MAinPage.xaml.cs:

        bool EnableSoftwareDRM(bool bEnable)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            // Force Software DRM useful for VC1 content which doesn't support Hardware DRM
            try
            {
                if (!localSettings.Containers.ContainsKey("PlayReady"))
                    localSettings.CreateContainer("PlayReady", Windows.Storage.ApplicationDataCreateDisposition.Always);
                localSettings.Containers["PlayReady"].Values["SoftwareOverride"] = (bEnable==true ? 1: 0);
            }
            catch (Exception e)
            {
                LogMessage("Exception while forcing software DRM: " + e.Message);
            }
            //Setup Software Override based on app setting
            //By default, PlayReady uses Hardware DRM if the machine support it. However, in case the app still want
            //software behavior, they can set localSettings.Containers["PlayReady"].Values["SoftwareOverride"]=1. 
            //This code tells MF to use software override as well
            if (localSettings.Containers.ContainsKey("PlayReady") &&
                localSettings.Containers["PlayReady"].Values.ContainsKey("SoftwareOverride"))
            {
                int UseSoftwareProtectionLayer = (int)localSettings.Containers["PlayReady"].Values["SoftwareOverride"];

                if(protectionManager.Properties.ContainsKey("Windows.Media.Protection.UseSoftwareProtectionLayer"))
                    protectionManager.Properties["Windows.Media.Protection.UseSoftwareProtectionLayer"] = (UseSoftwareProtectionLayer == 1? true : false);
                else  
                    protectionManager.Properties.Add("Windows.Media.Protection.UseSoftwareProtectionLayer", (UseSoftwareProtectionLayer == 1 ? true : false));
            }
            return true;
        }


Building the application
----------------

**Prerequisite: Windows Smooth Streaming Client SDK**
This version is based on the latest [Universal Smooth Streaming Client SDK](https://visualstudiogallery.msdn.microsoft.com/1e7d4700-7fa8-49b6-8a7b-8d8666685459)

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
 




