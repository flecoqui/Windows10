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

```xml
  <Capabilities>
     .
     .
    <rescap:Capability Name="runFullTrust" />
     .
     .
  </Capabilities>
```

Moreover, the desktop Extension category "windows.startupTask" will define the location of the Win32 application in the package.
The Win32 application must be stored in a subfolder, below in the subfolder "Win32".
```xml
  <Extensions>
        <desktop:Extension Category="windows.startupTask" Executable="Win32\StartupHelper.exe" EntryPoint="Windows.FullTrustApplication">
          <desktop:StartupTask TaskId="MyStartupTask" Enabled="true" DisplayName="My Startup Helper" />
        </desktop:Extension>
  </Extensions>
```

Installing the application
----------------------------
You can install the application on:
<p/>
- **Personal Computer Platform**: a desktop running Windows 10 Anniversary Update (RS1)

The applications packages for x86, x64 are available there :
[ZIP file of the application x86, x64 Packages](https://github.com/flecoqui/Windows10/raw/master/Samples/StartupUWP/Releases/LatestRelease.zip)


**Personal Computer installation:**

1.  Download the ZIP file on your computer harddrive
2.  Unzip the ZIP file
3.  Launch the PowerShell file Add-AppPackage.ps1. The PowerShell script will install the application on your computer running Windows 10


Building the application
----------------

**Prerequisite: Windows Smooth Streaming Client SDK**

1. If you download the samples ZIP, be sure to unzip the entire archive, not just the folder with the sample you want to build. 
2. Ensure the Anniversary Update (RS1) Windows 10 SDK is installed on your machine
3. Start Microsoft Visual Studio 2015 and select **File** \> **Open** \> **Project/Solution**.
3. Starting in the folder where you unzipped the samples, go to the Samples subfolder, then the subfolder for this specific sample, then the subfolder for your preferred language (C++, C#, or JavaScript). Double-click the Visual Studio 2015 Solution (.sln) file.
4. Press Ctrl+Shift+B, or select **Build** \> **Build Solution**.


**Deploying and running the sample**
1.  To debug the sample and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the sample without debugging, press Ctrl+F5 or select**Debug** \> **Start Without Debugging**.


**Publishing the application**

The publication of these packages on Windows Store will require a manual generation of the file appxupload.
To generate the new appxupload file follow those steps:

1. Select the project associated with the UWP Application, right-click on the project, select "Store" on the popup menu and "Create App package...":<br />
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/createapppackages.png)
2. Select "Yes (using a new name)" on the first "Create App Package" page <br />
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/storepackage.png)
3. Create or Select the name of the new application <br />
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/appname.png)
4. Select "Release" configuration and x86, x64 flavor and click on Create button<br />
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/createapppackagespage.png)
5. After few seconds, the packages are ready and click on the Output location link.<br />
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/createapppackagespagecompleted.png)
6. The File Explorer opens the folder which contains the new appxupload file and the folder ending with "_test" associated to the new pachage<br />
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files1.png)<br />
Unfortunately, the appxbundle inside the new appxupload files contains the CoreCLR assemblies, not the .NET native ones.
We need to create a new appxupload with .Net native. The appxbundle file inside the folder ending with "_test" does contains the .Net native assemblies.
7. Navigate into the folder ending with "_test" <br />
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files2.png)
8. Select the files appxbundle and appxsym and create a zip file with those files. <br /> 
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files3.png)
9. Rename the new zip file (newpackages.zip) into appxupload file. <br />
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files4.png)
10. The new appxupload file can be uplaoded on the Windows Store to deploy your UWP Application. <br />
![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/StartupUWP/Docs/files5.png) <br />
 