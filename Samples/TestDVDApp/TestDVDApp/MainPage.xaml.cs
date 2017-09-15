using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Custom;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestDVDApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Windows.Media.Playback.MediaPlayer mediaPlayer;
        DeviceWatcher dw;
        List<DeviceInformation> ListDeviceInformation;
        public MainPage()
        {
            this.InitializeComponent();
            if (ListDevices.Items != null)
                ListDevices.Items.Clear();
            ListDeviceInformation = new List<DeviceInformation>();
            ListDevices.SelectionChanged += ListDevices_SelectionChanged;
            ButtonEjectMedia.IsEnabled = false;
            ButtonReadTable.IsEnabled = false;
            ButtonReadTableEx.IsEnabled = false;
            ButtonReadRaw.IsEnabled = false;
            ButtonStartDiscover.Visibility = Visibility.Visible;
            ButtonStopDiscover.Visibility = Visibility.Collapsed;
            ListDevices.IsEnabled = false;
            CheckListDevices();
            SectorArray = null;
            FillComboTrack();
            bAutoStart = false;
            // Bind player to element
            mediaPlayer = new Windows.Media.Playback.MediaPlayer();
            mediaPlayerElement.SetMediaPlayer(mediaPlayer);
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        }
        #region Media
        /// <summary>
        /// GetFileFromLocalPathUrl
        /// Return the StorageFile associated with the url  
        /// </summary>
        /// <param name="PosterUrl">Url string of the content</param>
        /// <returns>StorageFile</returns>
        private async System.Threading.Tasks.Task<Windows.Storage.StorageFile> GetFileFromLocalPathUrl(string PosterUrl)
        {
            string path = null;
            Windows.Storage.StorageFolder folder = null;
            if (PosterUrl.ToLower().StartsWith("picture://"))
            {
                folder = Windows.Storage.KnownFolders.PicturesLibrary;
                path = PosterUrl.Replace("picture://", "");
            }
            else if (PosterUrl.ToLower().StartsWith("music://"))
            {
                folder = Windows.Storage.KnownFolders.MusicLibrary;
                path = PosterUrl.Replace("music://", "");
            }
            else if (PosterUrl.ToLower().StartsWith("video://"))
            {
                folder = Windows.Storage.KnownFolders.VideosLibrary;
                path = PosterUrl.Replace("video://", "");
            }
            else if (PosterUrl.ToLower().StartsWith("file://"))
            {
                path = PosterUrl.Replace("file://", "");
            }
            Windows.Storage.StorageFile file = null;
            try
            {
                if (folder != null)
                {
                    string ext = System.IO.Path.GetExtension(path);
                    string filename = System.IO.Path.GetFileName(path);
                    string directory = System.IO.Path.GetDirectoryName(path);
                    while (!string.IsNullOrEmpty(directory))
                    {

                        string subdirectory = directory;
                        int pos = -1;
                        if ((pos = directory.IndexOf('\\')) > 0)
                            subdirectory = directory.Substring(0, pos);
                        folder = await folder.GetFolderAsync(subdirectory);
                        if (folder != null)
                        {
                            if (pos > 0)
                                directory = directory.Substring(pos + 1);
                            else
                                directory = string.Empty;
                        }
                    }
                    if (folder != null)
                        file = await folder.GetFileAsync(filename);
                }
                else
                    file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
            }
            catch (Exception e)
            {
                LogMessage("Exception while opening file: " + PosterUrl + " exception: " + e.Message);
            }
            return file;
        }

        /// <summary>
        /// Mute method 
        /// </summary>
        private void mute_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Toggle Mute");
            mediaPlayer.IsMuted = !mediaPlayer.IsMuted;
        }
        /// <summary>
        /// Volume Up method 
        /// </summary>
        private void volumeUp_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Volume Up");
            mediaPlayer.Volume = (mediaPlayer.Volume + 0.10 <= 1 ? mediaPlayer.Volume + 0.10 : 1);
        }
        /// <summary>
        /// Volume Down method 
        /// </summary>
        private void volumeDown_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Volume Down");
            mediaPlayer.Volume = (mediaPlayer.Volume - 0.10 >= 0 ? mediaPlayer.Volume - 0.10 : 0);
        }
        private async System.Threading.Tasks.Task<bool> StartPlay(string content)
        {

            try
            {

                bool result = false;
                if (string.IsNullOrEmpty(content))
                {
                    LogMessage("Empty Uri");
                    return result;
                }

                // Stop the current stream
                mediaPlayer.Source = null;
                mediaPlayerElement.PosterSource = null;
                mediaPlayer.AutoPlay = true;
                // if a picture will be displayed
                // display or not popup
                if (result == true)
                {
                    mediaPlayerElement.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mediaPlayerElement.Visibility = Visibility.Visible;
                }
                // Audio or video
                Windows.Storage.StorageFile file = await GetFileFromLocalPathUrl("file://" + content);
                if (file != null)
                {
                    mediaPlayer.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                    mediaPlayer.Play();
                    return true;
                }
                else
                    LogMessage("Failed to load media file: " + Content);
            }
            catch (Exception ex)
            {
                LogMessage("Exception Playing: " + ex.Message.ToString());
            }
            return false;
        }

        private void MediaPlayer_MediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            LogMessage("MediaPlayer media opened event");

        }

        private void MediaPlayer_MediaFailed(Windows.Media.Playback.MediaPlayer sender, Windows.Media.Playback.MediaPlayerFailedEventArgs args)
        {
            LogMessage("MediaPlayer media failed event");
        }

        private void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            LogMessage("MediaPlayer media ended event");
        }
        #endregion Media
        void CheckListDevices()
        {
            if (ListDevices.Items != null) 
                if(ListDevices.Items.Count == 0)
                    ListDevices.Items.Add("None");
            if (ListDevices.Items.Count > 1)
                    ListDevices.Items.Remove("None");

            if (ListDevices.Items.Count > 0)
                ListDevices.SelectedIndex = 0;
        }
        private void ListDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                ButtonEjectMedia.IsEnabled = true;
                ButtonReadTable.IsEnabled = true;
                ButtonReadTableEx.IsEnabled = true;
                ButtonReadRaw.IsEnabled = true;
            }
            else
            {
                ButtonEjectMedia.IsEnabled = false;
                ButtonReadTable.IsEnabled = false;
                ButtonReadTableEx.IsEnabled = false;
                ButtonReadRaw.IsEnabled = false;
            }
        }

        private void Dw_EnumerationCompleted(DeviceWatcher sender, object args)
        {
        }

        private async void Dw_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
             () =>
             {
                 if ((ListDeviceInformation != null)&& (ListDeviceInformation.Count>0))
                 {
                     foreach(var d in ListDeviceInformation)
                     {
                         if (d.Id == args.Id)
                         {
                             ListDeviceInformation.Remove(d);
                             break;
                         }
                     }
                 }
                 ListDevices.Items.Remove(args.Id);
                 CheckListDevices();
             });
        }

        private async void Dw_Added(DeviceWatcher sender, DeviceInformation args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
             () =>
             {
                 if(ListDeviceInformation!=null)
                     ListDeviceInformation.Add(args);
                 ListDevices.Items.Add(args.Id);
                 CheckListDevices();
                 if (bAutoStart == true)
                 {
                     bAutoStart = false;
                     ButtonReadTable_Click(null, null);
                 }
             });
        }
        private void ButtonStartDiscover_Click(object sender, RoutedEventArgs e)
        {
            if(ListDevices.Items!=null)
                ListDevices.Items.Clear();
            CheckListDevices();
            string selector = CustomDevice.GetDeviceSelector(new Guid("53f56308-b6bf-11d0-94f2-00a0c91efb8b"));
            //string selector = CustomDevice.GetDeviceSelector(new Guid("53f56308-b6bf-11d0-94f2-00a0c91efb8b"));
            IEnumerable<string> additionalProperties = new string[] { "System.Devices.DeviceInstanceId" };
            if (dw != null)
            {
                dw.Stop();
                dw.Added -= Dw_Added;
                dw.Removed -= Dw_Removed;
                dw.EnumerationCompleted -= Dw_EnumerationCompleted;
                dw = null;
            }
            dw = DeviceInformation.CreateWatcher(selector, additionalProperties);
            dw.Added += Dw_Added;
            dw.Removed += Dw_Removed;
            dw.EnumerationCompleted += Dw_EnumerationCompleted;
            dw.Start();
            ButtonStartDiscover.Visibility = Visibility.Collapsed;
            ButtonStopDiscover.Visibility = Visibility.Visible;
            ListDevices.IsEnabled = true;

        }
        private void ButtonStopDiscover_Click(object sender, RoutedEventArgs e)
        {
            if (dw != null)
            {
                dw.Stop();
                dw.Added -= Dw_Added;
                dw.Removed -= Dw_Removed;
                dw.EnumerationCompleted -= Dw_EnumerationCompleted;
                dw = null;
            }
            ButtonStartDiscover.Visibility = Visibility.Visible;
            ButtonStopDiscover.Visibility = Visibility.Collapsed;
        }

        private async void ButtonReadRaw_Click(object sender, RoutedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                DeviceInformation device = null;
                if (ListDeviceInformation != null)
                {
                    foreach (var d in ListDeviceInformation)
                    {
                        if (d.Id == id)
                            device = d;
                    }
                }
                if (device != null)
                {
                    try
                    {
                        {
                            LogMessage("Device Name: " + device.Name);

                            foreach (var p in device.Properties)
                            {
                                LogMessage("Property: " + p.Key + " value: " + (p.Value != null ? p.Value.ToString() : "null"));
                            }
                            if ((ComboTrackNumber.Items != null) &&
                                (ComboTrackNumber.Items.Count > 0))
                            {
                                int i = ComboTrackNumber.SelectedIndex;
                                if (i < SectorArray.Length)
                                {
                                    string path = await GetTrackBuffer(device.Id, SectorArray, i);
                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        LogMessage("Track " + i.ToString() + " saved under " + path);
                                        await StartPlay(path);
                                    }
                                }
                            }
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        LogMessage("Exception while ejecting the media: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Exception while ejecting the media: " + ex.Message);
                    }
                }
            }

        }
        /*
        IOControlCode^ Fx2Driver::GetSevenSegmentDisplay = ref new IOControlCode(DeviceType,
                                                                         FunctionBase + 7,
                                                                         IOControlAccessMode::Read,
                                                                         IOControlBufferingMethod::Buffered);

        IOControlCode^ Fx2Driver::SetSevenSegmentDisplay = ref new IOControlCode(DeviceType,
                                                                                 FunctionBase + 8,
                                                                                 IOControlAccessMode::Write,
                                                                                 IOControlBufferingMethod::Buffered);

        IOControlCode^ Fx2Driver::ReadSwitches = ref new IOControlCode(DeviceType,
                                                                       FunctionBase + 6,
                                                                       IOControlAccessMode::Read,
                                                                       IOControlBufferingMethod::Buffered);

        IOControlCode^ Fx2Driver::GetInterruptMessage = ref new IOControlCode(DeviceType,
                                                                              FunctionBase + 9,
                                                                              IOControlAccessMode::Read,
                                                                              IOControlBufferingMethod::DirectOutput);

        IOControlCode^ Fx2Driver::EjectMedia = ref new IOControlCode(IOCTL_STORAGE_BASE, 0x0202, IOControlAccessMode::Read, IOControlBufferingMethod::Buffered);

        IOControlCode^ Fx2Driver::LoadMedia = ref new IOControlCode(IOCTL_STORAGE_BASE, 0x0203, IOControlAccessMode::Read, IOControlBufferingMethod::Buffered);
        */
        const UInt16 DeviceType = 0x5500;
        const UInt16 FunctionBase = 0x800;
        //FILE_DEVICE_CD_ROM              0x00000002
        const ushort FILE_DEVICE_CD_ROM = 0x00000002;
        const ushort FILE_DEVICE_MASS_STORAGE = 0x0000002d;
        const int MAXIMUM_NUMBER_TRACKS = 100;
        /*
        struct TRACK_DATA
        {
            byte Reserved;
            byte Control;
                //: 4;
            //char Adr : 4;
            byte TrackNumber;
            byte Reserved1;
            //4
            byte[] Address;
        };
        struct CDROM_TOC
        {
            //2
            byte[] Length;
            byte FirstTrack;
            byte LastTrack;
            //MAXIMUM_NUMBER_TRACKS
            TRACK_DATA[] TrackData;
        };
        */
        IOControlCode ejectMedia = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0202, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode loadMedia = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0203, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode reserveMedia = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0204, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode releaseMedia = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0205, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode readTable = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0000, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode readTableEx = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0015, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode readRaw = new IOControlCode(FILE_DEVICE_CD_ROM, 0x000F, IOControlAccessMode.Read, IOControlBufferingMethod.DirectOutput);
        private async void ButtonEjectMedia_Click(object sender, RoutedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                DeviceInformation device = null;
                if (ListDeviceInformation != null)
                {
                    foreach (var d in ListDeviceInformation)
                    {
                        if (d.Id == id)
                            device = d;
                    }
                }
                if (device != null)
                {
                    try
                    {
                        {
                            LogMessage("Device Name: " + device.Name);

                            foreach (var p in device.Properties)
                            {
                                LogMessage("Property: " + p.Key + " value: " + (p.Value != null ? p.Value.ToString() : "null"));
                            }
                            var customDevice = await Windows.Devices.Custom.CustomDevice.FromIdAsync(device.Id, DeviceAccessMode.ReadWrite,
                                DeviceSharingMode.Exclusive);

                            if (customDevice != null)
                            {
                                var r = await customDevice.SendIOControlAsync(
                                       ejectMedia,
                                    null, null);
                                LogMessage("Media Ejection successful: " + r.ToString());
                                SectorArray = null;
                                FillComboTrack();
                            }

                        }

                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        LogMessage("Exception while ejecting the media: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Exception while ejecting the media: " + ex.Message);
                    }
                }
            }
        }
        private async void ButtonLoadMedia_Click(object sender, RoutedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                DeviceInformation device = null;
                if (ListDeviceInformation != null)
                {
                    foreach (var d in ListDeviceInformation)
                    {
                        if (d.Id == id)
                            device = d;
                    }
                }
                if (device != null)
                {
                    try
                    {
                        {
                            LogMessage("Device Name: " + device.Name);

                            foreach (var p in device.Properties)
                            {
                                LogMessage("Property: " + p.Key + " value: " + (p.Value != null ? p.Value.ToString() : "null"));
                            }
                            var customDevice = await Windows.Devices.Custom.CustomDevice.FromIdAsync(device.Id, DeviceAccessMode.ReadWrite,
                                DeviceSharingMode.Exclusive);

                            var inputBuffer = new byte[1];
                            var outputBuffer = new byte[1];
                            //      IOControlCode ^ Fx2Driver::EjectMedia = ref new IOControlCode(IOCTL_STORAGE_BASE, 0x0202, IOControlAccessMode::Read, IOControlBufferingMethod::Buffered);

                            var r = await customDevice.SendIOControlAsync(
                                    reserveMedia,
                                null, null);


                        }

                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        LogMessage("Exception: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Exception: " + ex.Message);
                    }
                }
            }
        }
        static  int MSF_TO_LBA2(byte min, byte sec, byte frame)
        {
            return (int)(frame + 75 * (sec - 2 + 60 * min));
        }
        async System.Threading.Tasks.Task<int[]> GetCDSectorArray(Windows.Devices.Custom.CustomDevice device)
        {
            int[] Array = null;
            if(device!=null)
            {
                var outputBuffer = new byte[MAXIMUM_NUMBER_TRACKS * 8 + 4];

                uint r = await device.SendIOControlAsync(
                       readTable,
                    null, outputBuffer.AsBuffer());

                if (r > 0)
                {
                    int i_tracks = outputBuffer[3] - outputBuffer[2] + 1;

                    Array = new int[i_tracks+1];
                    if (Array != null)
                    {
                        for (int i = 0; (i <= i_tracks) && (4 + i * 8 + 4 + 3 < r); i++)
                        {
                            int sectors = MSF_TO_LBA2(
                                outputBuffer[4 + i * 8 + 4 + 1],
                                outputBuffer[4 + i * 8 + 4 + 2],
                                outputBuffer[4 + i * 8 + 4 + 3]);
                            Array[i] = sectors;
                            LogMessage("track number: " + i.ToString() + " sectors: " + sectors.ToString());
                        }
                    }
                }

            }
            return Array;
        }
        public  async System.Threading.Tasks.Task<bool> WriteIntoFile(string filename, byte[] array)
        {
            try
            {
                Windows.Storage.StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                if (folder != null)
                {
                    Windows.Storage.StorageFile file = await folder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                    if (file != null)
                    {
                        LogMessage("Writing into : " + file.Path);
                        Stream s = await file.OpenStreamForWriteAsync();
                        if (s != null)
                        {
                            s.Write(array, 0, array.Length);
                            s.Flush();
                            return true;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while copying file: " + Ex.Message);
            }
            return false;
        }
        async System.Threading.Tasks.Task<byte[]> GetCDText(string id)
        {
            byte[] Array = null;
            var device = await Windows.Devices.Custom.CustomDevice.FromIdAsync(id, DeviceAccessMode.Read,DeviceSharingMode.Shared);

            if (device != null)
            {
                var inputBuffer = new byte[4];
                inputBuffer[0] = 0x05;
                var outputBuffer = new byte[4];

                uint r = await device.SendIOControlAsync(
                       readTableEx,
                    inputBuffer.AsBuffer(), outputBuffer.AsBuffer());

                if (r > 0)
                {
                    int i_text = 2 + (outputBuffer[0] << 8) + outputBuffer[1];
                    if (i_text > 4)
                    {

                        Array = new byte[i_text];

                        r = await device.SendIOControlAsync(
                                   readTableEx,
                                inputBuffer.AsBuffer(), Array.AsBuffer());
                        if (r > 0)
                        {
                            LogMessage("CD Text: " + Array.ToString());
                            await WriteIntoFile("test.bin", Array);
                        }
                    }

                }

            }
            return Array;
        }
        int GetCDInfo(int[] SectorArray, byte[] TextArray)
        {
            int Result = 0;
            int i_track_last = 0;
            string[] ArrayTrackInfo = new string[99];
            int Count = (TextArray.Length - 4) / 18;
            for (int i = 0; i < Count; i++)
            {


                int i_pack_type = TextArray[4 + 18 * i];
                if (i_pack_type < 0x80 || i_pack_type > 0x8f)
                    continue;

                int i_track_number = (TextArray[4 + 18 * i + 1] >> 0) & 0x7f;
                int i_extension_flag = (TextArray[4 + 18 * i + 1] >> 7) & 0x01;
                if (i_extension_flag != 0)
                    continue;

                int i_track = i_track_number;

                int indexTrack = 4 + 18 * i + 4;
                int indexTrackMax = indexTrack + 12;
                while (i_track <= 127 && indexTrack < indexTrackMax)
                {
                    //fprintf( stderr, "t=%d psz_track=%p end=%p", i_track, (void *)psz_track, (void *)&psz_text[12] );
                    if (TextArray[4 + 18 * i + 1]!=0)
                    {
                        //astrcat(&pppsz_info[i_track][i_pack_type - 0x80], psz_track);
                        byte[] resArray = new byte[12];
                        int k = 0;
                        for (k = 0; k < 12; k++)
                        {
                            resArray[k] = TextArray[4 + 18 * i + 4 + k];
                            if (resArray[k] == 0x00)
                                break;
                        }
                        string str = System.Text.Encoding.UTF8.GetString(resArray);
                        ArrayTrackInfo[i_track] += str;
                        LogMessage("Text: " + str);
                        indexTrack += k;
                        i_track_last = (i_track_last > i_track ? i_track_last : i_track);
                    }

                    i_track++;
                    indexTrack += 1 + 12;
                }


            }
            for (int l = 0; l < 99; l++)
            {
                if (!string.IsNullOrEmpty(ArrayTrackInfo[l]))
                    LogMessage("Track : " + l.ToString() + " Info: " + ArrayTrackInfo[l]);
            }
            return Result;
        }
        //[Bloc décrivant le format audio]
        //   FormatBlocID(4 octets) : Identifiant «fmt »  (0x66,0x6D, 0x74,0x20)
        //   BlocSize(4 octets) : Nombre d'octets du bloc - 16  (0x10)

        //   AudioFormat(2 octets) : Format du stockage dans le fichier(1: PCM, ...)
        //   NbrCanaux(2 octets) : Nombre de canaux(de 1 à 6, cf.ci-dessous)
        //   Frequence(4 octets) : Fréquence d'échantillonnage (en hertz) [Valeurs standardisées : 11 025, 22 050, 44 100 et éventuellement 48 000 et 96 000]
        //   BytePerSec(4 octets) : Nombre d'octets à lire par seconde (c.-à-d., Frequence * BytePerBloc).
        //   BytePerBloc(2 octets) : Nombre d'octets par bloc d'échantillonnage(c.-à-d., tous canaux confondus : NbrCanaux* BitsPerSample/8).
        //   BitsPerSample(2 octets) : Nombre de bits utilisés pour le codage de chaque échantillon(8, 16, 24)
        //                            nChannels = BitConverter.ToUInt16(fmt.data, 2);
        //                            nSamplesPerSec = BitConverter.ToUInt32(fmt.data, 4);
        //                            nAvgBytesPerSec = BitConverter.ToUInt32(fmt.data, 8);
        //                            nBlockAlign = BitConverter.ToUInt16(fmt.data, 12);
        //                            wBitsPerSample = BitConverter.ToUInt16(fmt.data, 14);

        public byte[] CreateWAVHeaderBuffer(uint Len)
        {
            uint headerLen = 4 + 16 + 8 + 8 + 8;
            byte[] updatedBuffer = new byte[headerLen];
            if (updatedBuffer != null)
            {
                System.Text.UTF8Encoding.UTF8.GetBytes("RIFF").CopyTo(0, updatedBuffer.AsBuffer(), 0, 4);
                BitConverter.GetBytes(4 + 16 + 8 + Len + 8).CopyTo(0, updatedBuffer.AsBuffer(), 4, 4);
                System.Text.UTF8Encoding.UTF8.GetBytes("WAVE").CopyTo(0, updatedBuffer.AsBuffer(), 8, 4);
                System.Text.UTF8Encoding.UTF8.GetBytes("fmt ").CopyTo(0, updatedBuffer.AsBuffer(), 12, 4);
           //     BitConverter.GetBytes(fmt.length).CopyTo(0, updatedBuffer.AsBuffer(), 16, 4);
                BitConverter.GetBytes((uint)16).CopyTo(0, updatedBuffer.AsBuffer(), 16, 4);
                //                fmt.data.CopyTo(0, updatedBuffer.AsBuffer(), 20, (int)fmt.length);
                BitConverter.GetBytes(1).CopyTo(0, updatedBuffer.AsBuffer(), 20, 2);
                BitConverter.GetBytes((ushort)2).CopyTo(0, updatedBuffer.AsBuffer(), 22, 2);
                BitConverter.GetBytes((uint)44100).CopyTo(0, updatedBuffer.AsBuffer(), 24, 4);
                BitConverter.GetBytes((uint)176400).CopyTo(0, updatedBuffer.AsBuffer(), 28, 4);
                BitConverter.GetBytes((UInt16)4).CopyTo(0, updatedBuffer.AsBuffer(), 32, 2);
                BitConverter.GetBytes((UInt16)16).CopyTo(0, updatedBuffer.AsBuffer(), 34, 2);

                System.Text.UTF8Encoding.UTF8.GetBytes("data").CopyTo(0, updatedBuffer.AsBuffer(), 20 + 16, 4);
                BitConverter.GetBytes(Len).CopyTo(0, updatedBuffer.AsBuffer(), 24 + 16, 4);
            }
            return updatedBuffer;
        }
        const uint CD_RAW_SECTOR_SIZE = 2352;
        const uint CD_SECTOR_SIZE = 2048;
        async System.Threading.Tasks.Task<string> GetTrackBuffer(string Id, int[] SectorArray, int track)
        {
            string filename = "test" + track.ToString() + ".wav";
            Windows.Storage.StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            if (folder != null)
            {
                Windows.Storage.StorageFile file = await folder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                if (file != null)
                {
                    LogMessage("Writing into : " + file.Path);
                    Stream s = await file.OpenStreamForWriteAsync();
                    if (s != null)
                    {
                        var customDevice = await Windows.Devices.Custom.CustomDevice.FromIdAsync(Id, DeviceAccessMode.ReadWrite,
                        DeviceSharingMode.Exclusive);
                        if ((SectorArray!=null)&&(track+1 < SectorArray.Length))
                        {

                            int startSector = SectorArray[track];
                            int endSector = SectorArray[track+1];
                            int numberSector = 20;
                            var inputBuffer = new byte[8 + 4 + 4];
                            var outputBuffer = new byte[CD_RAW_SECTOR_SIZE * numberSector];
                            int k = startSector;
                            while (k < endSector)
                            {
                                long firstSector = k * CD_SECTOR_SIZE;
                                byte[] array = BitConverter.GetBytes(firstSector);
                                for (int i = 0; i < array.Length; i++)
                                    inputBuffer[i] = array[i];
                                byte[] intarray = BitConverter.GetBytes(numberSector);
                                for (int i = 0; i < intarray.Length; i++)
                                    inputBuffer[8 + i] = intarray[i];
                                intarray = BitConverter.GetBytes((int)2);
                                for (int i = 0; i < intarray.Length; i++)
                                    inputBuffer[12 + i] = intarray[i];
                                uint r = 0; ;
                                r = await customDevice.SendIOControlAsync(
                                       readRaw, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());
                                //if (customDevice.InputStream != null)
                                //{
                                //    var stream = customDevice.InputStream;
                                //    if(stream!=null)
                                //    {
                                //        //stream.Seek((ulong)firstSector);
                                //        var b = await stream.ReadAsync(outputBuffer.AsBuffer(), (uint) (numberSector * 2048), Windows.Storage.Streams.InputStreamOptions.None);
                                //        if (b != null)
                                //        {
                                //            r = b.Length;
                                //            outputBuffer = b.ToArray();
                                //        }


                                //    }
                                //}
                                if (r > 0)
                                {
                                    //bool bFound = false;
                                    //for (int j = 0; j < r; j++)
                                    //    if (outputBuffer[j] != 0)
                                    //    {
                                    //        bFound = true;
                                    //        break;
                                    //    }
                                    //if (bFound == true)
                                    if (k == startSector)
                                    {
                                        byte[] header = CreateWAVHeaderBuffer((uint)(100 * (numberSector * CD_RAW_SECTOR_SIZE)));
                                        s.Write(header, 0, header.Length);
                                    }
                                    if(r==(numberSector * CD_RAW_SECTOR_SIZE))
                                        s.Write(outputBuffer, 0, (int) (numberSector * CD_RAW_SECTOR_SIZE));
                                    else
                                        s.Write(outputBuffer, 0, (int)r);
                                    s.Flush();
                                    LogMessage("Read " + numberSector.ToString() + " sectors from " + k.ToString());
                                    
                                    //else
                                    //    LogMessage("Read " + numberSector.ToString() + " null sectors from " + k.ToString());
                                    k += numberSector;
                                    if (k > (startSector + 100 * numberSector))
                                        break;
                                }
                            }
                            return file.Path;
                        }
                    }
                }
            }
            return null;
        }
        void FillComboTrack()
        {
            ComboTrackNumber.Items.Clear();
            if ((SectorArray != null)&&
                (SectorArray.Length>1))
            {
                ComboTrackNumber.IsEnabled = true;
                for (int i = 0; i < (SectorArray.Length - 1); i++)
                    ComboTrackNumber.Items.Add(i.ToString());
            }
            if (ComboTrackNumber.Items.Count > 0)
            {
                ComboTrackNumber.SelectedIndex = 0;
            }
            else
                ComboTrackNumber.IsEnabled = false;
        }
        int[] SectorArray = null;
        private async void ButtonReadTable_Click(object sender, RoutedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                DeviceInformation device = null;
                if (ListDeviceInformation != null)
                {
                    foreach (var d in ListDeviceInformation)
                    {
                        if (d.Id == id)
                            device = d;
                    }
                }
                if (device != null)
                {
                    try
                    {
                        LogMessage("Device Name: " + device.Name);

                        foreach (var p in device.Properties)
                        {
                            LogMessage("Property: " + p.Key + " value: " + (p.Value != null ? p.Value.ToString() : "null"));
                        }
                        var customDevice = await Windows.Devices.Custom.CustomDevice.FromIdAsync(device.Id, DeviceAccessMode.ReadWrite,
                            DeviceSharingMode.Exclusive);
                        
                        SectorArray = await GetCDSectorArray(customDevice);
                        if (SectorArray != null)
                        {
                            LogMessage("Get CD Table successfull: " + SectorArray.Count().ToString() + " tracks" );
                            FillComboTrack();
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        LogMessage("Exception while reading Table: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Exception while reading Table: " + ex.Message);
                    }
                }
            }
        }

        private async void ButtonReadTableEx_Click(object sender, RoutedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                DeviceInformation device = null;
                if (ListDeviceInformation != null)
                {
                    foreach (var d in ListDeviceInformation)
                    {
                        if (d.Id == id)
                            device = d;
                    }
                }
                if (device != null)
                {
                    try
                    {
                        LogMessage("Device Name: " + device.Name);

                        foreach (var p in device.Properties)
                        {
                            LogMessage("Property: " + p.Key + " value: " + (p.Value != null ? p.Value.ToString() : "null"));
                        }
                        if (SectorArray != null)
                        {
                            byte[] TextArray = await GetCDText(device.Id);
                            if (TextArray != null)
                            {
                                GetCDInfo(SectorArray, TextArray);
                            }
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        LogMessage("Exception: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Exception: " + ex.Message);
                    }
                }
            }
        }






        #region Logs
        void PushMessage(string Message)
        {
            App app = Windows.UI.Xaml.Application.Current as App;
            if (app != null)
                app.MessageList.Enqueue(Message);
        }
        bool PopMessage(out string Message)
        {
            Message = string.Empty;
            App app = Windows.UI.Xaml.Application.Current as App;
            if (app != null)
                return app.MessageList.TryDequeue(out Message);
            return false;
        }
        /// <summary>
        /// Display Message on the application page
        /// </summary>
        /// <param name="Message">String to display</param>
        async void LogMessage(string Message)
        {
            string Text = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " " + Message + "\n";
            PushMessage(Text);
            System.Diagnostics.Debug.WriteLine(Text);
            await DisplayLogMessage();
        }
        /// <summary>
        /// Display Message on the application page
        /// </summary>
        /// <param name="Message">String to display</param>
        async System.Threading.Tasks.Task<bool> DisplayLogMessage()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {

                    string result;
                    while (PopMessage(out result))
                    {
                        logs.Text += result;
                        if (logs.Text.Length > 16000)
                        {
                            string LocalString = logs.Text;
                            while (LocalString.Length > 12000)
                            {
                                int pos = LocalString.IndexOf('\n');
                                if (pos == -1)
                                    pos = LocalString.IndexOf('\r');


                                if ((pos >= 0) && (pos < LocalString.Length))
                                {
                                    LocalString = LocalString.Substring(pos + 1);
                                }
                                else
                                    break;
                            }
                            logs.Text = LocalString;
                        }
                    }
                }
            );
            return true;
        }
        /// <summary>
        /// This method is called when the content of the Logs TextBox changed  
        /// The method scroll to the bottom of the TextBox
        /// </summary>
        void Logs_TextChanged(object sender, TextChangedEventArgs e)
        {
            //  logs.Focus(FocusState.Programmatic);
            // logs.Select(logs.Text.Length, 0);
            var tbsv = GetFirstDescendantScrollViewer(logs);
            tbsv.ChangeView(null, tbsv.ScrollableHeight, null, true);
        }
        /// <summary>
        /// Retrieve the ScrollViewer associated with a control  
        /// </summary>
        ScrollViewer GetFirstDescendantScrollViewer(DependencyObject parent)
        {
            var c = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < c; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var sv = child as ScrollViewer;
                if (sv != null)
                    return sv;
                sv = GetFirstDescendantScrollViewer(child);
                if (sv != null)
                    return sv;
            }

            return null;
        }
        #endregion

        bool bAutoStart = false;
        public async void AutoPlayCD()
        {
            LogMessage("AutoPlayCD Method");
            bAutoStart = true;
            ButtonStartDiscover_Click(null, null);
        }
        public void AutoPlayDVD()
        {
            LogMessage("AutoPlayDVD Method");
            bAutoStart = true;
            ButtonStartDiscover_Click(null, null);
        }
        public void AutoPlayBD()
        {
            LogMessage("AutoPlayBD Method");
            bAutoStart = true;
            ButtonStartDiscover_Click(null, null);
        }




    }
}
