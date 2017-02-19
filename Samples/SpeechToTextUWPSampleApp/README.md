<!---
  category: AudioVideoAndCamera
  samplefwlink: http://go.microsoft.com/fwlink/p/?LinkId=620563&clcid=0x409
--->

# Speech-To-Text UWP Sample Application

Overview
--------------
This Speech-To-Text UWP Sample Application  can:
<p/>
-   **Record**: record spoken audio into a WAV file, 
-   **Play**: play the WAV files stored on the local disk,
-   **Convert WAV file**: Convert the WAV file to text,
-   **Convert live audio**: Convert live audio to text.

The spoken audio is recorded in WAV file in the following format:
<p/>
-   **Number of Channels**: one channel, 
-   **Samples per second**: 16000,
-   **Bits per sample**: 16 bits,
-   **Average Bytes per second**: 256 kbit/s.

In order to use the application you need a Cognitive Services Speech-To-Text subscription Key.
You can sign up [here](https://www.microsoft.com/cognitive-services/en-us/sign-up)  


Installing the application
----------------------------
You can install the application on:
<p/>
- **Personal Computer Platform**: a desktop running Windows 10 RS1
- **Windows 10 Mobile Platform**: a phone running Windows 10 RS1

The applications packages for x86, x64 and ARM are available there :
[ZIP file of the application x86, x64, ARM Packages](https://github.com/flecoqui/Windows10/raw/master/Samples/SpeechToTextUWPSampleApp/Releases/LatestRelease.zip)


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
6.  Tap on the file SpeechToTextUWPSampleApp_1.0.XX.O_x86_x64_arm.cer to install the certificate.
7.  Tap on the file SpeechToTextUWPSampleApp_1.0.XX.O_x86_x64_arm.appxbundle to install the application


Using the application
----------------------------
Once the application is installed on your device, you can launch it and the main page will be displayed after few seconds.

### Main page

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/mainpage.png)

The application is used to record spoken audio into a WAV file, play the WAV files stored on the local disk, convert the WAV file to text, convert live audio to text.

### Entering your subscription Key
Once the application is launched, you can enter your subscription key which will be used for the communication with Speech-To-Text Cognitive Services.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/subscriptionkey.png)

### Recording Spoken Audio into a WAV file
With the application you can record the spoken audio. 
Click on the button "Record" 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/record.png)

to start the recording.

Now you can speak, after few seconds click on the same button "Stop Record" 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/stoprecord.png)

to stop the recording.
And then select the WAV file where you want to store the recording.
The path of the WAV file is displayed in the Edit box "Path" 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/openfile.png) 

to select a WAV file.

### Playing a WAV file
In order to play a WAV file click on the button "Open WAV File" 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/openfile.png) 

to select a WAV file.

### Mute and Audio level
Once the application is playing an audio file it's possible to switch off the audio (`Mute` button) or change the audio output level (`Audio+` and `Audio-` button)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/audio.png)


### Start over, play, pause and stop 

Once the application is playing an audio file it's possible to:
<p/>
- pause/play the current asset
- start over the current asset
- stop the current asset 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/playpause.png)


### Converting Spoken Audio WAV file to Text
With you can convert to text the WAV file you have just recorded. 
First, select the language with the "Language" Combo Box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/upload.png)

Then click on the button "Upload" 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/upload.png) 

to start the recording.
After less than one second, the result is displayed in the "Result" Edit box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/result.png)

### Converting Live Spoken Audio to Text
You can also directly convert the live Spoken Audio to text. 
First, select the language with the "Language" Combo Box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/upload.png).

Then click on the button "Convert" 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/convert.png) to start the recording of Live Spoken Audio.

Now you can speak, after few seconds click on the same button "Stop Convert" 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/stoprecord.png) to stop the recording and transmit the audio stream.

After less than one second, the result is displayed in the "Result" Edit box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/result.png)

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

The Speech-To-Text UWP Sample Applicaton could be improved to support the following features:
<p/>
1.  Continuous recording and spoken audio conversion to text
 




