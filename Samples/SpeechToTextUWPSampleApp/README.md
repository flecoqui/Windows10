<!---
  category: AudioVideoAndCamera
  samplefwlink: http://go.microsoft.com/fwlink/p/?LinkId=620563&clcid=0x409
--->

# Speech-To-Text and Text-To-Speech UWP Sample Application

Overview
--------------
This Speech-To-Text and Text-To-Speech UWP Sample Application  can:

- **Record**: record spoken audio into a WAV file, 
- **Play**: play the WAV files stored on the local disk,
- **Convert WAV file**: Convert the WAV file to text with Cognitive Services,
- **Convert live audio**: Convert live audio to text with Cognitive Services, the audio buffer is sent to Cogntive Services at the end of the recording session.
- **Convert continuously live audio**: Convert continuously live audio to text with Cognitive Services, in that case, the audio buffers are sent to Cognitive Services if the audio level is sufficient during a configurable period.
- **Convert Text**: Convert a text into a WAV stream associated with the current language,

The spoken audio is recorded into a WAV file in the following format:

- **Number of Channels**: one channel, 
- **Samples per second**: 16000,
- **Bits per sample**: 16 bits,
- **Average Bytes per second**: 256 kbit/s.

In order to use the application you need a Cognitive Services Speech-To-Text subscription Key.
You can sign up [here](https://www.microsoft.com/cognitive-services/en-us/sign-up)  


Installing the application
----------------------------
You can install the application on:

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

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/main.png)

The application is used to record spoken audio into a WAV file, play the WAV files stored on the local disk, convert the WAV file to text, convert live audio to text and convert continuously live audio to text .

### Entering your subscription Key
Once the application is launched, you can enter your subscription key which will be used for the communication with Speech-To-Text Cognitive Services.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/subscriptionkey.png)

### Recording Spoken Audio into a WAV file
With the application you can record the spoken audio. 
Click on the button "Record" to start the recording.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/record.png)

Now you can speak, you can see the audio level in cyan

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/stopliverecord.png)

after few seconds click on the same button "Stop Record" to stop the recording.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/stoprecordinginfile.png)

And then select the WAV file where you want to store the recording.
The path of the WAV file is automatically copied in the Edit box "Path" used to select a WAV file.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/path.png) 


### Playing a WAV file
In order to play a WAV file click on the button "Open WAV File" to select a WAV file.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/openfile.png) 

Then click on the "Play" button.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/play.png) 

#### Start over, play, pause and stop 

Once the application is playing an audio file it's possible to:
<p/>
- pause/play the current asset
- start over the current asset
- stop the current asset 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/playpause.png)


#### Mute and Audio level
Once the application is playing an audio file it's possible to switch off the audio (`Mute` button) or change the audio output level (`Audio+` and `Audio-` button)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/audio.png)


### Converting Spoken Audio WAV file to Text (Speech-To-Text)
With the application, you can convert to text the WAV file you have just recorded. 
First, check the path of the your audio file is correct in the Path Edit box,
then select the language in the "Language" Combo Box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/language.png)

You can select the conversation model: interactive, conversation, dictation with "Cognitive Services Speech Recognition API"  Combo Box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/mode.png)

You can also select the result type: simle or detailed  with "Cognitive Services Speech Result type"  Combo Box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/resulttype.png)


Finally click on the button "Upload" to upload the file towards the Cognitive Services.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/upload.png) 

Once the file is uploaded, after less than one second, the result is displayed in the "Result" Edit box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/result.png)

### Converting Live Spoken Audio to Text (Speech-To-Text)
You can also directly convert the live Spoken Audio to text. 
First, select the language in the "Language" Combo Box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/language.png).

Then click on the button "Convert" to start the recording of Live Spoken Audio.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/liverecord.png) 

Now you can speak, you can see the audio level in cyan

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/stopliverecord.png)

after few seconds click on the same button "Stop Convert" to stop the recording and transmit the audio buffer to Cognitive Services.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/stopliverecordbutton.png) 

After less than one second, the result is displayed in the "Result" Edit box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/result.png)


### Converting continuously Live Spoken Audio to Text (Speech-To-Text)
You can also directly convert the live Spoken Audio continuously to text. 
First, select the language in the "Language" Combo Box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/language.png).

For continuous recording, you can define the two following parameters:

1. The minimum audio level average necessary to trigger the recording, it's a value between 0 and 65535. By default the value is 300. You can tune this value after several microphone tests.
2. The duration in milliseconds for the calculation of the audio level average . With this parameter you define the period during which the audio level is measured. By default the value is 1000 ms.  

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/levelduration.png).

As soon as the audio level average is over the Level, all the audio samples will be recorded till the audio level average becomes below the same level.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/graph.png).

Then click on the button "Continuous Record" to start the recording of Live Spoken Audio.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/continuousrecord.png) 

Now you can speak, you can see the audio level in cyan

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/audiolevel.png)

once the audio level is sufficient the Live Spoken Audio is recorded till the audio level become too low. Then the audio buffer is sent to Cognitive Services.
The result is displayed, a green rectangle is displayed, if the conversion is successul:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/result_ok.png) 

a red rectangle is displayed, if the conversion failed:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/result_error.png) 

Moreover, if the application is suspended, the continuous recording is stopped. When the application will resume, the continous recording will start automatically.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/resuming.png) 

IF you want to stop the continuous recording click on the same button:

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/stopcontinuousrecord.png) 

### Converting text to speech  (Text-To-Speech)
With the application, you can also convert a text into speech . 
First, enter your text in the Result text box, 
then select the language in the "Language" Combo Box: 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/language.png)

You can select the speech gender Male or Female with the "Gender" Combo Box:  

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/texttospeech.png)

Then click on the TextToSpeech button to get the WAV stream associated with the text and the current language:  

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/SpeechToTextUWPSampleApp/Docs/texttospeechbutton.png)






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

1. Integration with LUIS Cognitive Services for continuous recording
2. Support of MP3, AAC, WMA audio files 






