<!---
  category: AudioVideoAndCamera
  samplefwlink: http://go.microsoft.com/fwlink/p/?LinkId=620563&clcid=0x409
--->

# Translator-Text UWP Sample Application

Overview
--------------
This Translator-Text UWP Sample Application  can:

- **Get Languages**: get the list of supported languages by the Cognitive Services backend, 
- **Detect the current language**: detect the language of a text,
- **Translate**: Translate a text from one language to another one.

In order to use the application you need a Cognitive Services Translator-Text subscription Key.
You can sign up [here](https://www.microsoft.com/cognitive-services/en-us/sign-up)  


Installing the application
----------------------------
You can install the application on:

- **Personal Computer Platform**: a desktop running Windows 10 Anniversary Update
- **Windows 10 Mobile Platform**: a phone running Windows 10 Anniversary Update

The applications packages for x86, x64 and ARM are available there :
[ZIP file of the application x86, x64, ARM Packages](https://github.com/flecoqui/Windows10/raw/master/Samples/TranslatorTextUWPSampleApp/Releases/LatestRelease.zip)


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
6.  Tap on the file TranslatorTextUWPSampleApp_1.0.XX.O_x86_x64_arm.cer to install the certificate.
7.  Tap on the file TranslatorTextUWPSampleApp_1.0.XX.O_x86_x64_arm.appxbundle to install the application


Using the application
----------------------------
Once the application is installed on your device, you can launch it and the main page will be displayed after few seconds.

### Main page

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/TranslatorTextUWPSampleApp/Docs/main.png)

The application is used to translate text after getting the list of supported languages. The application can also detect the language of a text.

### Entering your subscription Key
Once the application is launched, you can enter your subscription key which will be used for the communication with Translator-Text Cognitive Services.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/TranslatorTextUWPSampleApp/Docs/subscriptionkey.png)

### Getting the list of supported languages
With the application you can get the list of supported languages.
Click on the button "Get Languages" to get the list of supported languages.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/TranslatorTextUWPSampleApp/Docs/getlanguages.png)

Once the list is downloaded, you can translate the input text. 
Once the text is filled in the edit box, you can either select the input language using the combo box language or detect the input language by clicking on button "Detect Language"

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/TranslatorTextUWPSampleApp/Docs/detect.png)

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/TranslatorTextUWPSampleApp/Docs/inputlanguage.png)

Once the input language is set, you can translate the input text into a text in another language. You need to select the output language using the combo box language.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/TranslatorTextUWPSampleApp/Docs/outputlanguage.png)

Click on the button "Translate".

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/TranslatorTextUWPSampleApp/Docs/translate.png) 

After few seconds the result is displayed on the page.

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/TranslatorTextUWPSampleApp/Docs/result.png) 

![](https://raw.githubusercontent.com/flecoqui/Windows10/master/Samples/TranslatorTextUWPSampleApp/Docs/mainpage.png)


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

The Translator-Text UWP Sample Applicaton could be improved to support the following features:
<p/>

1. Support Speech translator






