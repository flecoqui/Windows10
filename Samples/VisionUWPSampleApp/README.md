<!---
  category: AudioVideoAndCamera
  samplefwlink: http://go.microsoft.com/fwlink/p/?LinkId=620563&clcid=0x409
--->

# Cognitive Services Vision UWP Sample Application

Overview
--------------
This Vision UWP Sample Application  can:

- **Take a picture**: take a picture with your Windows 10 device camera, 
- **Open a picture**: open a picture stored on your Windows 10 device,
- **Analyse a picture**: Analyse a picture with Cognitive Service Computer Vision and/or Custom Vision,

In order to use the application you need a Cognitive Services Computer Vision key or a Custom Vision Key.
You can sign up [here](https://www.microsoft.com/cognitive-services/en-us/sign-up)  


Installing the application
----------------------------
You can install the application on:

- **Personal Computer Platform**: a desktop running Windows 10 RS1
- **Windows 10 Mobile Platform**: a phone running Windows 10 RS1

The applications packages for x86, x64 and ARM are available there :
[ZIP file of the application x86, x64, ARM Packages](https://github.com/flecoqui/Windows10/raw/master/Samples/VisionUWPSampleApp/Releases/LatestRelease.zip)


**Personal Computer installation:**

1.  Download the ZIP file on your computer harddrive
2.  Unzip the ZIP file
3.  Launch the PowerShell file Add-AppPackage.ps1. The PowerShell script will install the application on your computer running Windows 10


**Phone installation:**

1.  Connect the phone running Windows 10 Mobile to your computer with a USB Cable.
2.  After few seconds, you should see the phone storage with Windows Explorer running on your computer
3.  Copy the application packages on your phone storage, for instance in the Downloads folder
4.  On the phone install a File Explorer application from Windows Store
5.  With File Explorer on the phone navigate to the folder where the packages have been copied
6.  Tap on the file VisionUWPSampleApp_1.0.XX.O_x86_x64_arm.cer to install the certificate.
7.  Tap on the file VisionUWPSampleApp_1.0.XX.O_x86_x64_arm.appxbundle to install the application


Using the application with Computer Vision Cognitive Service
----------------------------
Once the application is installed on your device, you can launch it and the main page will be displayed after few seconds.

### Main page

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/main.png)

The application is used to take picture, open picture and analyze the picture with Cognitive Services.
As you use the application with Computer Vision service, verify the Check Box Custom Vision is unchecked. 

### Entering your subscription Key
Then you can enter the subscription key associated with the Computer Vision Cognitive Service which will be used for the communication Computer Vision Cognitive Services.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/subscriptionkey.png)


### Entering the hostname
Then you can enter the hostname associated with the url of your Computer Vision Cognitive Service. The format is usually region.api.cognitive.microsoft.com.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/hostname.png)

### Selecting the options
Then you can select the options before calling the service. For instance, if you want to analyze faces from celebrities, select the Visula Features "Faces"  and the details "Celebreties".

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/options.png)


### Taking a picture
Now you can either take a picture with the camera installed on your device running Windows 10 or open a picture stored on your computer.
Tap on the Video button to launch the video preview

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/video.png)

You can also select the resolution of your picture, select a resolution which will create a picture with a size below 4 MB.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/comboresolution.png)

When the preview control is displaying the picture you want to take, Tap on the Camera button to capture the picture associated with the video preview

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/camera.png)

The picture is displayed in the preview control.

### Opening a picture
You can also open an existing picture.
Tap on the Open button to select the picture on your harddrive

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/open.png)

The selected picture is displayed in the preview control.

### Analyzing the picture
Tap on the Send button to send the current picture to the Cognitive Services

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/send.png)

After few seconds, the result is displayed in the Log field:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/result.png)


Using the application with Custom Vision Cognitive Service
----------------------------
Once the application is installed on your device, you can launch it and the main page will be displayed after few seconds.

### Main page

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/maincustom.png)

As you will use the application with Custom Vision service, verify the Check Box Custom Vision is checked. 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/customcheck.png)

### Entering your subscription Key
Then you can enter the subscription key associated with the Computer Vision Cognitive Service which will be used for the communication Computer Vision Cognitive Services.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/subscriptionkeycustom.png)


### Entering the hostname
Then you can enter the hostname associated with the url of your Computer Vision Cognitive Service. The format is usually region.api.cognitive.microsoft.com.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/hostnamecustom.png)

### Entering the ProjectID and optionnally the IterationID
Then you can enter the Project ID associated with the Custom Vision Model and optionnally the Iteration ID of the model. If you leave this field empty the service will use the latest model.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/optionscustom.png)


### Taking a picture
Now you can either take a picture with the camera installed on your device running Windows 10 or open a picture stored on your computer.
Tap on the Video button to launch the video preview

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/video.png)

You can also select the resolution of your picture, select a resolution which will create a picture with a size below 4 MB.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/comboresolution.png)

When the preview control is displaying the picture you want to take, Tap on the Camera button to capture the picture associated with the video preview

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/camera.png)

The picture is displayed in the preview control.

### Opening a picture
You can also open an existing picture.
Tap on the Open button to select the picture on your harddrive

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/open.png)

The selected picture is displayed in the preview control.

### Analyzing the picture
Tap on the Send button to send the current picture to the Custom Cognitive Services

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/send.png)

After few seconds, the result is displayed in the Log field:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/VisionUWPSampleApp/Docs/resultcustom.png)




Building the application
----------------

1. If you download the samples ZIP, be sure to unzip the entire archive, not just the folder with the sample you want to build. 
2. Ensure the Red Stone 1 (RS1) Windows 10 SDK is installed on your machine
3. Start Microsoft Visual Studio 2015 and select **File** \> **Open** \> **Project/Solution**.
3. Starting in the folder where you unzipped the samples, go to the Samples subfolder, then the subfolder for this specific sample, then the subfolder for your preferred language (C++, C#, or JavaScript). Double-click the Visual Studio 2015 Solution (.sln) file.
4. Press Ctrl+Shift+B, or select **Build** \> **Build Solution**.


**Deploying and running the sample**
1.  To debug the sample and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the sample without debugging, press Ctrl+F5 or select**Debug** \> **Start Without Debugging**.

Next steps
--------------

The Vision UWP Sample Applicaton could be improved to support the following features:
<p/>

1. Better Camera usage
2. Display result over the picture 






