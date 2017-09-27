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
using Windows.Storage.Streams;
using TestDVDApp.CDReader;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestDVDApp
{


  /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Windows.Media.Playback.MediaPlayer mediaPlayer;
        CDReaderManager cdReaderManager;
        List<CDReaderDevice> ListDeviceInformation;
        CDMetadata currentCD;
        public MainPage()
        {
            this.InitializeComponent();
            currentCD = new CDMetadata();
            if (ListDevices.Items != null)
                ListDevices.Items.Clear();
            ListDeviceInformation = new List<CDReaderDevice>();
            ListDevices.SelectionChanged += ListDevices_SelectionChanged;
            ButtonEjectMedia.IsEnabled = false;
            ButtonReadTable.IsEnabled = false;
            ButtonPlayTrack.IsEnabled = false;
            ButtonExtractTrack.IsEnabled = false;
            ButtonPlayWavFile.IsEnabled = false;
            ButtonStopPlayer.IsEnabled = false;
            ButtonStartDiscover.Visibility = Visibility.Visible;
            ButtonStopDiscover.Visibility = Visibility.Collapsed;
            ListDevices.IsEnabled = false;
            CheckListDevices();
            FillComboTrack();
            bAutoStart = false;
            // Bind player to element
            mediaPlayer = new Windows.Media.Playback.MediaPlayer();
            mediaPlayerElement.SetMediaPlayer(mediaPlayer);
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;



        }

        private async void PlaybackSession_PlaybackStateChanged(Windows.Media.Playback.MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
             () =>
             {

                 if (mediaPlayer.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
                     ButtonStopPlayer.IsEnabled = true;
                 else
                     ButtonStopPlayer.IsEnabled = false;
             });
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
            else
                path = PosterUrl;
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
         private async System.Threading.Tasks.Task<bool> StartPlay(string deviceID, int startSector, int endSector)
        {

            try
            {

                bool result = false;
                if (string.IsNullOrEmpty(deviceID))
                {
                    LogMessage("Empty deviceID");
                    return result;
                }

                // Stop the current stream
                mediaPlayer.Source = null;
                mediaPlayerElement.PosterSource = null;
                mediaPlayer.AutoPlay = true;

                if (result == true)
                {
                    mediaPlayerElement.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mediaPlayerElement.Visibility = Visibility.Visible;
                }
                    
                string contentType = "audio/wav"; 
                mediaPlayer.Source = Windows.Media.Core.MediaSource.CreateFromStream(await CDTrackStream.Create(deviceID, startSector,endSector), contentType);
                mediaPlayer.Play();

                return true;
            }
            catch (Exception ex)
            {
                LogMessage("Exception Playing: " + ex.Message.ToString());
            }
            return false;
        }
        private async System.Threading.Tasks.Task<bool> RecordTrack(string wavFile, string deviceID, int startSector, int endSector)
        {

            try
            {

                bool result = false;
                if (string.IsNullOrEmpty(deviceID))
                {
                    LogMessage("Empty deviceID");
                    return result;
                }

                // Stop the current stream
                mediaPlayer.Source = null;
                mediaPlayerElement.PosterSource = null;
                mediaPlayer.AutoPlay = true;


                CDTrackStream s = await CDTrackStream.Create(deviceID, startSector, endSector);
                if (s != null)
                {
                    Windows.Storage.StorageFile file = await GetFileFromLocalPathUrl(wavFile);
                    if (file != null)
                    {
                        LogMessage("Writing into : " + file.Path);

                        Stream fs = await file.OpenStreamForWriteAsync();
                        if (fs != null)
                        {
                            const int bufferSize = 2352 * 20 * 16;
                            const int WAVHeaderLen = 44;
                            ulong FileSize = s.MaxSize;
                            byte[] array = new byte[bufferSize];
                            ulong currentSize = WAVHeaderLen;
                            for (ulong i = 0; i < FileSize + bufferSize; i += currentSize)
                            {
                                if (i == WAVHeaderLen)
                                    currentSize = bufferSize;
                                var b = await s.ReadAsync(array.AsBuffer(), (uint)currentSize, InputStreamOptions.None);
                                if (b != null)
                                {
                                    fs.Write(array, 0, (int)b.Length);
                                    LogMessage("Writing : " + b.Length.ToString() + " bytes " + ((((ulong)fs.Position+1) * 100) / FileSize).ToString() + "% copied ");
                                    if (currentSize != b.Length)
                                        break;
                                }
                            }
                            fs.Flush();
                            return true;
                        }
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("Exception Recording Track into WAV File : " + ex.Message.ToString());
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
                ButtonPlayTrack.IsEnabled = true;
                ButtonExtractTrack.IsEnabled = true;
                ButtonPlayWavFile.IsEnabled = true;


            }
            else
            {
                ButtonEjectMedia.IsEnabled = false;
                ButtonReadTable.IsEnabled = false;
                ButtonPlayTrack.IsEnabled = false;
                ButtonExtractTrack.IsEnabled = false;
                ButtonPlayWavFile.IsEnabled = false;
                ButtonStopPlayer.IsEnabled = false;
            }
        }



        private async void CDReaderDevice_Removed(CDReaderManager sender, CDReaderDevice args)
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

        private async void CDReaderDevice_Added(CDReaderManager sender, CDReaderDevice args)
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
                     ButtonReadCDMetadata_Click(null, null);
                 }
             });
        }
        private  void ButtonStartDiscover_Click(object sender, RoutedEventArgs e)
        {
            if(ListDevices.Items!=null)
                ListDevices.Items.Clear();
            CheckListDevices();
            string selector = CustomDevice.GetDeviceSelector(new Guid("53f56308-b6bf-11d0-94f2-00a0c91efb8b"));
            IEnumerable<string> additionalProperties = new string[] { "System.Devices.DeviceInstanceId" };
            if (cdReaderManager != null)
            {
                cdReaderManager.StopDiscovery();
                cdReaderManager.CDReaderDeviceAdded -= CDReaderDevice_Added;
                cdReaderManager.CDReaderDeviceRemoved -= CDReaderDevice_Removed;
                cdReaderManager = null;
            }
            cdReaderManager = new CDReaderManager();
            cdReaderManager.CDReaderDeviceAdded += CDReaderDevice_Added;
            cdReaderManager.CDReaderDeviceRemoved += CDReaderDevice_Removed;
            cdReaderManager.StartDiscovery();
            ButtonStartDiscover.Visibility = Visibility.Collapsed;
            ButtonStopDiscover.Visibility = Visibility.Visible;
            ListDevices.IsEnabled = true;


        }
        private void ButtonStopDiscover_Click(object sender, RoutedEventArgs e)
        {
            if (cdReaderManager != null)
            {
                cdReaderManager.StopDiscovery();
                cdReaderManager.CDReaderDeviceAdded -= CDReaderDevice_Added;
                cdReaderManager.CDReaderDeviceRemoved -= CDReaderDevice_Removed;
                cdReaderManager = null;
            }
            ButtonStartDiscover.Visibility = Visibility.Visible;
            ButtonStopDiscover.Visibility = Visibility.Collapsed;
        }
        private void ButtonStopPlayer_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Source = null;
        }
        private async void ButtonPlayWavFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
                filePicker.FileTypeFilter.Add(".wav");
                filePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
                filePicker.SettingsIdentifier = "WavPicker";
                filePicker.CommitButtonText = "Open WAV File to Play";

                var wavFile = await filePicker.PickSingleFileAsync();
                if (wavFile != null)
                {
                    string fileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(wavFile);
                    LogMessage("Selected file: " + wavFile.Path);
                    // Audio or video
                    Windows.Storage.StorageFile file = await GetFileFromLocalPathUrl(wavFile.Path);
                    if (file != null)
                    {
                        mediaPlayer.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                        mediaPlayer.Play();
                    }
                    else
                        LogMessage("Failed to load media file: " + wavFile.Path);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to select WAV file: Exception: " + ex.Message);
            }

        }
        private async void ButtonExtractTrack_Click(object sender, RoutedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                CDReaderDevice device = null;
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
                            if ((ComboTrackNumber.Items != null) &&
                                (ComboTrackNumber.Items.Count > 0))
                            {
                                CDTrackMetadata t = ComboTrackNumber.SelectedItem as CDTrackMetadata;
                                if (t != null)
                                {
                                    LogMessage("Extracting Track " + t.Number.ToString());

                                    var filePicker = new Windows.Storage.Pickers.FileSavePicker();
                                    filePicker.DefaultFileExtension = ".wav";
                                    filePicker.SuggestedFileName = "track"+ t.Number.ToString() + ".wav";
                                    filePicker.FileTypeChoices.Add("WAV files", new List<string>() { ".wav" });
                                    filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
                                    filePicker.SettingsIdentifier = "WavPicker";
                                    filePicker.CommitButtonText = "Save Track into a WAV File";

                                    var wavFile = await filePicker.PickSaveFileAsync();
                                    if (wavFile != null)
                                    {
                                        string fileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(wavFile);
                                        if (await RecordTrack(wavFile.Path, device.Id, t.FirstSector, t.LastSector))
                                        {
                                            LogMessage("Record track in WAV file: " + wavFile.Path.ToString());
                                        }
                                        else
                                            LogMessage("Error while saving record buffer in file: " + wavFile.Path.ToString());
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
        private async void ButtonPlayTrack_Click(object sender, RoutedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                CDReaderDevice device = null;
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


                            if ((ComboTrackNumber.Items != null) &&
                                (ComboTrackNumber.Items.Count > 0))
                            {
                                CDTrackMetadata t = ComboTrackNumber.SelectedItem as CDTrackMetadata;
                                if (t != null)
                                {
                                    LogMessage("Playing Track " + t.Number.ToString());
                                    await StartPlay(device.Id, t.FirstSector, t.LastSector);
                                }
                            }
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        LogMessage("Exception while playing the media: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Exception while playing the media: " + ex.Message);
                    }
                }
            }

        }
 
        private async void ButtonEjectMedia_Click(object sender, RoutedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                CDReaderDevice device = null;
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
                            bool result = await cdReaderManager.EjectMedia(device.Id);
                            if(result==true)
                            { 
                                LogMessage("Media Ejection successful" );
                                if(currentCD.Tracks != null)
                                    currentCD.Tracks.Clear();
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



        void FillComboTrack()
        {
            ComboTrackNumber.Items.Clear();
            if ((currentCD != null)&&
                (currentCD.Tracks.Count > 1))
            {
                ComboTrackNumber.IsEnabled = true;

                for (int i = 0; i < currentCD.Tracks.Count; i++)
                {
                    //string s = currentCD.Tracks[i].ToString();
                    ComboTrackNumber.Items.Add(currentCD.Tracks[i]);
                }
            }
            if (ComboTrackNumber.Items.Count > 0)
            {
                ComboTrackNumber.SelectedIndex = 0;
            }
            else
                ComboTrackNumber.IsEnabled = false;
        }
        private async void ButtonReadCDMetadata_Click(object sender, RoutedEventArgs e)
        {
            string id = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                CDReaderDevice device = null;
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
                        currentCD = await cdReaderManager.ReadMediaMetadata(device.Id);
                        if ((currentCD != null)&&(currentCD.Tracks.Count>1))
                        {
                            FillComboTrack();
                            LogMessage("Get CD Table Map successfull: " + currentCD.Tracks.Count.ToString() + " tracks" );
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
        public  void AutoPlayCD()
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
