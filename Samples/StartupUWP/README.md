<!---
  category: AudioVideoAndCamera
  samplefwlink: http://go.microsoft.com/fwlink/p/?LinkId=620563&clcid=0x409
--->

# Auto Start UWP (Universal Windows Plaftorm) Sample Application

Overview
--------------
This sample UWP application is automatically launched when the user is logged.
This application uses Desktop Bridge Extension to include a Win32 application (Launcher) in the application package.
The Win32 StartupHelper.exe application will launch the UWP Application StartupTask while the user is logging in.
This feature is suported with the Anniversary Update of Windows 10 (10.0.14393.X) x86 and x64 flavor.
In order to include the Win32 application in the UWP package, the Application manifest needs to be updated:

The application requires the runFullTrust capability:

  <Capabilities>
     .
     .
    <rescap:Capability Name="runFullTrust" />
     .
     .
  </Capabilities>

Moreover, the desktop Extension category "windows.startupTask" will define the location of the Win32 application in the package.
The Win32 application must be stored in a subfolder, below in the subfolder "Win32".

  <Extensions>
        <desktop:Extension Category="windows.startupTask" Executable="Win32\StartupHelper.exe" EntryPoint="Windows.FullTrustApplication">
          <desktop:StartupTask TaskId="MyStartupTask" Enabled="true" DisplayName="My Startup Helper" />
        </desktop:Extension>
  </Extensions>


Installing the application
----------------------------
You can install the application on:
<p/>
- **Personal Computer Platform**: a desktop running Windows 10 RS1

The applications packages for x86, x64 are available there :
[ZIP file of the application x86, x64, ARM Packages](https://github.com/flecoqui/Windows10/raw/master/Samples/StartupUWP/Releases/LatestRelease.zip)


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


**Publishing the application**


### Entering the asset's url
Once the asset is selected the `URL` field is updated with the url associated with the asset. You can update manually this field beofre playing the asset if you want to test a specific url. 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/appname.png)



![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/createapppackages.png)


![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/createapppackagespage.png)



![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/createapppackagespagecompleted.png)



![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/storepackage.png)


The appxbundle inside the appxupload contains the CoreCLR assemblies, not the .NET native ones.

You should have a folder named (in your example) at the same directory level as your appxupload

StartUpTask_1.1.4.0_Test

Pick the appxsym and appxbundle from that folder to create the new appxupload.
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files1.png)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files2.png)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files3.png)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files4.png)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files5.png)
 





