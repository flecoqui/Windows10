<!---
  samplefwlink: http://go.microsoft.com/fwlink/p/?LinkId=619979&clcid=0x409
--->

# Windows 10 Media Application Samples

This repository contains the Windows 10 application samples that demonstrate the Media API usage patterns for the Universal Windows Platform (UWP) in the Windows Software Development Kit (SDK) for Windows 10. These code samples were created with the Universal Windows templates available in Visual Studio, and are designed to run on desktop, mobile, and future devices that support the Universal Windows Platform.  

## Universal Windows Platform development

These samples require Visual Studio 2015 and the Windows Software Development Kit (SDK) for Windows 10 to build, test, and deploy your Universal Windows apps. 

   [Get a free copy of Visual Studio 2015 Community Edition with support for building Universal Windows apps](http://go.microsoft.com/fwlink/?LinkID=280676)


## Using the samples

The easiest way to use these samples without using Git is to download the zip file containing the current version (using the link below or by clicking the "Download ZIP" button on the repo page). You can then unzip the entire archive and use the samples in Visual Studio 2015.

   [Download the samples ZIP](../../archive/master.zip)

   **Notes:** 
   * Before you unzip the archive, right-click it, select Properties, and then select Unblock.
   * In Visual Studio 2015, the platform target defaults to ARM, so be sure to change that to x64 or x86 if you want to test on a non-ARM device. 
   
**Reminder:** If you unzip individual samples, they will not build due to references to other portions of the ZIP file that were not unzipped. You must unzip the entire archive if you intend to build the samples.

For more info about the programming models, platforms, languages, and APIs demonstrated in these samples, please refer to the guidance, tutorials, and reference topics provided in the Windows 10 documentation available in the [Windows Developer Center](https://dev.windows.com). These samples are provided as-is in order to indicate or demonstrate the functionality of the programming models and feature APIs for Windows.


## See also

For additional Windows samples, see [Windows Universal Samples on GitHub](https://github.com/Microsoft/Windows-universal-samples/). 

## Samples by category

<table>
 <tr>
  <th colspan="3" align="left">Media Applications</th>
 </tr>
 <tr>
  <td><a href="Samples/UniversalMediaPlayer">Universal Media Player</a></td>
  <td><a href="Samples/AdaptiveMediaCache">Adaptive Media Cache</a></td>
 </tr>
</table>
