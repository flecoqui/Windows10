<!---
  category: AudioVideoAndCamera
  samplefwlink: http://go.microsoft.com/fwlink/p/?LinkId=620563&clcid=0x409
--->

# Auto Start UWP (Universal Windows Plaftorm) Sample Application

Overview
--------------
This sample UWP application is automatically launched when the user is logged.
This application uses Desktop Bridge Extension to include a Win32 application (Launcher) in the application package.
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

The applications packages for x86, x64 are available there :
[ZIP file of the application x86, x64, ARM Packages](https://github.com/flecoqui/Windows10/raw/master/Samples/RS1UniversalMediaPlayer/Releases/LatestRelease.zip)


**Personal Computer installation:**

1.  Download the ZIP file on your computer harddrive
2.  Unzip the ZIP file
3.  Launch the PowerShell file Add-AppPackage.ps1. The PowerShell script will install the application on your computer running Windows 10


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
 




