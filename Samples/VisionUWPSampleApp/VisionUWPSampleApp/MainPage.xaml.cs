//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using VisionClient;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace VisionUWPSampleApp
{
    /// <summary>
    /// Main page for the application.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static public MainPage Current = null;
        VisionClient.VisionClient client;
        const string defaultVisionHostname = "northeurope.api.cognitive.microsoft.com";
        const string defaultCustomVisionHostname = "southcentralus.api.cognitive.microsoft.com";
        string VisionHostname = defaultVisionHostname;
        string CustomVisionHostname = defaultCustomVisionHostname;
        string VisionSubscriptionKey = string.Empty;
        string CustomVisionSubscriptionKey = string.Empty;

        bool isPreviewingVideo = false;
        // Object to manage access to camera devices
        private MediaCapturePreviewer _previewer = null;
        // Folder in which the captures will be stored (initialized in InitializeCameraButton_Click)
        private StorageFolder _captureFolder = null;
        // Current Picture path
        string currentPicturePath = null;
        string[] LanguageArray = 
            {"en","zh"  };
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            _previewer = new MediaCapturePreviewer(PreviewControl, Dispatcher);
        }
        protected override  void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            StopCamera();
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            LogMessage("MainPage OnNavigatedTo");
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size
            {
                Height = 436,
                Width = 320
            });
            // Hide Systray on phone
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                // Hide Status bar
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }

            // Logs event to refresh the TextBox
            logs.TextChanged += Logs_TextChanged;
            // Check Event
            CustomVisionCheck.Checked += CustomVisionCheck_Checked;
            CustomVisionCheck.Unchecked += CustomVisionCheck_Checked;

            // ComboResolution Changed Event
            ComboResolution.SelectionChanged += ComboResolution_Changed;

            backgroundVideo.SizeChanged += BackgroundVideo_SizeChanged;

            // Fill Combobox API
            ComboVisualFeatures.Items.Clear();
            ComboVisualFeatures.Items.Add("Categories");
            ComboVisualFeatures.Items.Add("Tags");
            ComboVisualFeatures.Items.Add("Description");
            ComboVisualFeatures.Items.Add("Faces");
            ComboVisualFeatures.Items.Add("ImageType");
            ComboVisualFeatures.Items.Add("Color");
            ComboVisualFeatures.Items.Add("Adult");
            ComboVisualFeatures.SelectedIndex = 0;

            ComboDetails.Items.Clear();
            ComboDetails.Items.Add("Celebrities");
            ComboDetails.Items.Add("Landmarks");
            ComboDetails.SelectedIndex = 0;



            // Get Subscription ID from the local settings
            ReadSettingsAndState();
            // Display Picture
            await SetPictureUrl(currentPicturePath);
            PreviewControl.Visibility = Visibility.Collapsed;
            pictureElement.Visibility = Visibility.Visible;

            // Update control and play first video
            UpdateControls();

            
            // Register Suspend/Resume
            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
            
            // Display OS, Device information
            LogMessage(VisionClient.SystemInformation.GetString());
            
            // Create Cognitive Service Vision Client
            client = new VisionClient.VisionClient();



        }

        private void CustomVisionCheck_Checked(object sender, RoutedEventArgs e)
        {
            if(CustomVisionCheck.IsChecked == true)
            {
                subscriptionKey.Text = CustomVisionSubscriptionKey;
                Hostname.Text = CustomVisionHostname;
            }
            else
            {
                subscriptionKey.Text = VisionSubscriptionKey;
                Hostname.Text = VisionHostname;
            }
            UpdateControls();
        }






        /// <summary>
        /// Method OnNavigatedFrom
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            LogMessage("MainPage OnNavigatedFrom");
            // Unregister Suspend/Resume
            Application.Current.Suspending -= Current_Suspending;
            Application.Current.Resuming -= Current_Resuming;
        }
        /// <summary>
        /// This method is called when the application is resuming
        /// </summary>
        async void Current_Resuming(object sender, object e)
        {
            LogMessage("Resuming");
            ReadSettingsAndState();
            // Display Picture
            await SetPictureUrl(currentPicturePath);
            PreviewControl.Visibility = Visibility.Collapsed;
            pictureElement.Visibility = Visibility.Visible;


            //Update Controls
            UpdateControls();
        }
        /// <summary>
        /// This method is called when the application is suspending
        /// </summary>
        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            LogMessage("Suspending");
            var deferal = e.SuspendingOperation.GetDeferral();
            SaveSettingsAndState();
            if (isPreviewingVideo)
            {
                LogMessage("Stop Camera...");
                StopCamera();
                isPreviewingVideo = false;
            }
            deferal.Complete();
        }

        #region Settings
        const string keyVisionHostname = "VisionHostnameKey";
        const string keyCustomVisionHostname = "CustomVisionHostnameKey";
        const string keySubscriptionKey = "SubscriptionKey";
        const string keyCustomSubscriptionKey = "CustomSubscriptionKey";
        const string keyCurrentPicturePath = "CurrentPicturePath";
        const string keyIsCustom = "isCustomKey";
        const string keyVisionVisualFeatures = "VisionVisualFeaturesKey";
        const string keyVisionDetails = "VisionDetailsKey";
        const string keyProjectID = "ProjectIDKey";
        const string keyIterationID = "IterationIDKey";
        /// <summary>
        /// Function to save all the persistent attributes
        /// </summary>
        public bool SaveSettingsAndState()
        {
            if (CustomVisionCheck.IsChecked == true)
            {
                CustomVisionSubscriptionKey = subscriptionKey.Text;
                CustomVisionHostname = Hostname.Text;
            }
            else
            {
                VisionSubscriptionKey = subscriptionKey.Text;
                VisionHostname = Hostname.Text;
            }
            SaveSettingsValue(keyCustomVisionHostname, CustomVisionHostname);
            SaveSettingsValue(keyCustomSubscriptionKey, CustomVisionSubscriptionKey);
            SaveSettingsValue(keyVisionHostname, VisionHostname);
            SaveSettingsValue(keySubscriptionKey, VisionSubscriptionKey);
            SaveSettingsValue(keyCurrentPicturePath, currentPicturePath);
            SaveSettingsValue(keyIsCustom, CustomVisionCheck.IsChecked.ToString());
            SaveSettingsValue(keyVisionVisualFeatures, (string) ComboVisualFeatures.SelectedItem);
            SaveSettingsValue(keyVisionDetails, (string)ComboDetails.SelectedItem);
            SaveSettingsValue(keyProjectID, (string)projectID.Text);
            SaveSettingsValue(keyIterationID, (string)iterationID.Text);
            return true;
        }
        /// <summary>
        /// Function to read all the persistent attributes
        /// </summary>
        public bool ReadSettingsAndState()
        {
            string s = ReadSettingsValue(keyCustomSubscriptionKey) as string;
            if (!string.IsNullOrEmpty(s))
                CustomVisionSubscriptionKey = s;
            s = ReadSettingsValue(keySubscriptionKey) as string;
            if (!string.IsNullOrEmpty(s))
                VisionSubscriptionKey = s;


            s = ReadSettingsValue(keyCurrentPicturePath) as string;
            if (!string.IsNullOrEmpty(s))
                currentPicturePath = s;

            s = ReadSettingsValue(keyVisionHostname) as string;
            if (!string.IsNullOrEmpty(s))
                VisionHostname = s;
            else
                VisionHostname = defaultVisionHostname;

            s = ReadSettingsValue(keyCustomVisionHostname) as string;
            if (!string.IsNullOrEmpty(s))
                CustomVisionHostname = s;
            else
                CustomVisionHostname = defaultCustomVisionHostname;

            bool bresult = false;
            s = ReadSettingsValue(keyIsCustom) as string;
            if (!string.IsNullOrEmpty(s))
                bool.TryParse(s, out bresult);
            CustomVisionCheck.IsChecked = bresult;
            if (CustomVisionCheck.IsChecked == true)
            {
                Hostname.Text = CustomVisionHostname;
                subscriptionKey.Text = CustomVisionSubscriptionKey;
            }
            else
            {
                Hostname.Text = VisionHostname;
                subscriptionKey.Text = VisionSubscriptionKey;
            }


            s = ReadSettingsValue(keyVisionDetails) as string;
            if (!string.IsNullOrEmpty(s))
                ComboDetails.SelectedItem = s;
            else
                ComboDetails.SelectedItem = "Categories";

            s = ReadSettingsValue(keyVisionVisualFeatures) as string;
            if (!string.IsNullOrEmpty(s))
                ComboVisualFeatures.SelectedItem = s;
            else
                ComboVisualFeatures.SelectedItem = "Celebrities";

            s = ReadSettingsValue(keyProjectID) as string;
            if (!string.IsNullOrEmpty(s))
                projectID.Text = s;
            else
                projectID.Text = "";

            s = ReadSettingsValue(keyIterationID) as string;
            if (!string.IsNullOrEmpty(s))
                iterationID.Text = s;
            else
                iterationID.Text = "";
            return true;
        }
        /// <summary>
        /// Function to read a setting value and clear it after reading it
        /// </summary>
        public static object ReadSettingsValue(string key)
        {
            if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return null;
            }
            else
            {
                var value = Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
                Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove(key);
                return value;
            }
        }

        /// <summary>
        /// Save a key value pair in settings. Create if it doesn't exist
        /// </summary>
        public static void SaveSettingsValue(string key, object value)
        {
            if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
            else
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }
        #endregion Settings

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
        public async void LogMessage(string Message)
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

        #region Media
        /// <summary>
        /// Populates the given combo box with all possible combinations of the given stream type settings returned by the camera driver
        /// </summary>
        /// <param name="streamType"></param>
        /// <param name="comboBox"></param>
        private void PopulateComboBox(MediaStreamType streamType, ComboBox comboBox, bool showFrameRate = true)
        {
            // Query all preview properties of the device 
            IEnumerable<StreamResolution> allStreamProperties = _previewer.MediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(streamType).Select(x => new StreamResolution(x));
            // Order them by resolution then frame rate
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);
            comboBox.Items.Clear();
            // Populate the combo box with the entries
            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(showFrameRate);
                comboBoxItem.Tag = property;
                comboBox.Items.Add(comboBoxItem);
            }
        }
        /// <summary>
        /// Event handler for Photo settings combo box. Updates stream resolution based on the selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ComboResolution_Changed(object sender, RoutedEventArgs e)
        {
            if (_previewer.IsPreviewing)
            {
                var selectedItem = (sender as ComboBox).SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    var encodingProperties = (selectedItem.Tag as StreamResolution).EncodingProperties;
                    await _previewer.MediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, encodingProperties);

                }
            }
        }
        private async void StartCamera()
        {


            await _previewer.InitializeCameraAsync();

            if (_previewer.IsPreviewing)
            {

                
                PopulateComboBox(MediaStreamType.Photo, ComboResolution, false);

            }

            var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            // Fall back to the local app storage if the Pictures Library is not available
            _captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
        }
        private async void StopCamera()
        {
            
            await _previewer.CleanupCameraAsync();
        }
        private async System.Threading.Tasks.Task<string> CapturePhoto()
        {
            string path = string.Empty;
            if (_previewer.IsPreviewing)
            {

                var stream = new InMemoryRandomAccessStream();

                try
                {
                    // Take and save the photo
                    var file = await _captureFolder.CreateFileAsync("VisionPhoto.jpg", CreationCollisionOption.GenerateUniqueName);
                    await _previewer.MediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
                    LogMessage("Photo taken, saved to: " + file.Path);
                    path = file.Path;
                }
                catch (Exception ex)
                {
                    // File I/O errors are reported as exceptions.
                    LogMessage("Exception when taking a photo: " + ex.Message);
                }


            }
            return path;
        }

        /// <summary>
        /// Mute method 
        /// </summary>
        private async void startCamera_Click(object sender, RoutedEventArgs e)
        {
            isPreviewingVideo = !isPreviewingVideo;
            previewButton.IsEnabled = false;
            try

            {
                if (isPreviewingVideo)
                {
                    LogMessage("Start Camera");
                    StartCamera();
                    PreviewControl.Visibility = Visibility.Visible;
                    pictureElement.Visibility = Visibility.Collapsed;
                    SetPreviewSize();
                }
                else
                {
                    LogMessage("Capture Photo");
                    string path = await CapturePhoto();
                    if (!string.IsNullOrEmpty(path))
                    {
                        LogMessage("Display photo: " + path);
                        await SetPictureUrl("file://" + path);
                    }
                    LogMessage("StopCamera");

                    StopCamera();

                    PreviewControl.Visibility = Visibility.Collapsed;
                    pictureElement.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                LogMessage("Exeption while taking photo: " + ex.Message);
            }
            finally
            {

                previewButton.IsEnabled = true;
            }
            UpdateControls();
        }
        /// <summary>
        /// open method which select a WAV file on disk
        /// </summary>
        private async void openPicture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
                filePicker.FileTypeFilter.Add(".jpg");
                filePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                filePicker.SettingsIdentifier = "JPGPicker";
                filePicker.CommitButtonText = "Open JPG File to Process";

                var jpgFile = await filePicker.PickSingleFileAsync();
                if (jpgFile != null)
                {
                    string fileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(jpgFile);

                    LogMessage("Selected file: " + jpgFile.Path);
                    await SetPictureUrl("file://" + jpgFile.Path);
                    PreviewControl.Visibility = Visibility.Collapsed;
                    pictureElement.Visibility = Visibility.Visible;
                    UpdateControls();
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to select WAV file: Exception: " + ex.Message);
            }
        }
        private string ParseString(string jsonString)
        {
            return jsonString.Replace(",", ",\n").Replace("{", "{\n").Replace("}", "\n}");
        }
        /// <summary>
        /// sendPicture_Click method 
        /// </summary>
        private async void sendPicture_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Send current picture to Cognitive Services: " + currentPicturePath);
            if (!string.IsNullOrEmpty(currentPicturePath))
            {
                sendPictureButton.IsEnabled = false;
                try
                {
                    Windows.Storage.StorageFile file = await GetFileFromLocalPathUrl(currentPicturePath);
                    if (file != null)
                    {
                        // Cognitive Service Vision GetToken 
                        if (!string.IsNullOrEmpty(subscriptionKey.Text))
                        {
                            VisionResponse response = null;

                            if(CustomVisionCheck.IsChecked==true)
                                response = await client.SendCustomVisionPicture(subscriptionKey.Text.ToString(), Hostname.Text, projectID.Text, iterationID.Text, file);
                            else
                                response = await client.SendVisionPicture(subscriptionKey.Text.ToString(), Hostname.Text, ComboVisualFeatures.SelectedItem.ToString(), ComboDetails.SelectedItem.ToString(), LanguageArray[0], file);
                            if (response!=null)
                            {
                                string error = response.GetHttpError();
                                if(!string.IsNullOrEmpty(error))
                                {
                                    LogMessage("Error from Cognitive Services: " + error);
                                }
                                else
                                {
                                    SaveSettingsAndState();
                                    LogMessage("Response from Cognitive Services: " + ParseString(response.Result()));
                                    
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // File I/O errors are reported as exceptions.
                    LogMessage("Exception when sending photo: " + ex.Message);
                }
                finally
                {
                    sendPictureButton.IsEnabled = true;
                }
            }
            else
            {
                LogMessage("Sending picture: Path not defined");
            }
            UpdateControls();
        }

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



        #endregion Media

        #region ui
        /// <summary>
        /// BackgroundVideo picture Resize Event
        /// </summary>        
        void BackgroundVideo_SizeChanged(System.Object sender, SizeChangedEventArgs e)
        {
            SetPictureElementSize();
            SetPreviewSize();
        }
        /// <summary>
        /// Set the source for the picture : windows  
        /// </summary>
        void SetPictureSource(Windows.UI.Xaml.Media.Imaging.BitmapImage b)
        {
            // Set picture source for windows element
            pictureElement.Source = b;
        }
        /// <summary>
        /// Resize the pictureElement to match with the BackgroundVideo size
        /// </summary>      
        void SetPictureElementSize()
        {
            Windows.UI.Xaml.Media.Imaging.BitmapImage b = pictureElement.Source as Windows.UI.Xaml.Media.Imaging.BitmapImage;
            if (b != null)
            {
                int nWidth;
                int nHeight;
                double ratioBackground = backgroundVideo.ActualWidth / backgroundVideo.ActualHeight;
                double ratioPicture = ((double)b.PixelWidth / (double)b.PixelHeight);
                if (ratioPicture > ratioBackground)
                {
                    nWidth = (int)backgroundVideo.ActualWidth;
                    nHeight = (int)(nWidth / ratioPicture);
                }
                else
                {
                    nHeight = (int)backgroundVideo.ActualHeight;
                    nWidth = (int)(nHeight * ratioPicture);

                }
                pictureElement.Width = nWidth;
                pictureElement.Height = nHeight;
            }
        }
        /// <summary>
        /// Resize the preview to match with the BackgroundVideo size
        /// </summary>      
        void SetPreviewSize()
        {
            if (isPreviewingVideo)
            {
                int nWidth;
                int nHeight;
                double ratioBackground = backgroundVideo.ActualWidth / backgroundVideo.ActualHeight;
                if (PreviewControl.ActualHeight == 0)
                {
                    PreviewControl.Width = (int)backgroundVideo.ActualWidth;
                    PreviewControl.Height = (int)backgroundVideo.ActualHeight;
                }
                else
                {
                    double ratioPicture = ((double)PreviewControl.ActualWidth / (double)PreviewControl.ActualHeight);
                    if (ratioPicture > ratioBackground)
                    {
                        nWidth = (int)backgroundVideo.ActualWidth;
                        nHeight = (int)(nWidth / ratioPicture);
                    }
                    else
                    {
                        nHeight = (int)backgroundVideo.ActualHeight;
                        nWidth = (int)(nHeight * ratioPicture);

                    }
                    PreviewControl.Width = nWidth;
                    PreviewControl.Height = nHeight;
                }

            }
        }
        private async System.Threading.Tasks.Task<bool> SetDefaultPicture()
        {
            var uri = new System.Uri("ms-appx:///Assets/Photo.png");
            Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            if (file != null)
            {
                using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    if (fileStream != null)
                    {
                        Windows.UI.Xaml.Media.Imaging.BitmapImage b = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                        if (b != null)
                        {
                            b.SetSource(fileStream);
                            SetPictureSource(b);
                            SetPictureElementSize();
                            return true;
                        }
                    }
                }
            }
            return false;

        }
        /// <summary>
        /// This method set the poster source for the MediaElement 
        /// </summary>
        private async System.Threading.Tasks.Task<bool> SetPictureUrl(string PosterUrl)
        {
            try
            {

                currentPicturePath = PosterUrl;
                Windows.Storage.StorageFile file = await GetFileFromLocalPathUrl(PosterUrl);
                if (file != null)
                {
                    using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        if (fileStream != null)
                        {
                            Windows.UI.Xaml.Media.Imaging.BitmapImage b = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                            if (b != null)
                            {
                                b.SetSource(fileStream);
                                SetPictureSource(b);
                                SetPictureElementSize();

                                return true;
                            }
                        }
                    }
                }
                else
                {
                    await SetDefaultPicture();
                    LogMessage("Failed to load poster: " + PosterUrl);
                    return true;
                }

            }
            catch (Exception e)
            {
                LogMessage("Exception while loading poster: " + PosterUrl + " - " + e.Message);
            }

            return false;
        }

        /// <summary>
        /// UpdateControls Method which update the controls on the page  
        /// </summary>
        async void UpdateControls()
        {

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                 () =>
                 {
                     {
                         previewButton.IsEnabled = true;
                         openPictureButton.IsEnabled = true;
                         
                         if (CustomVisionCheck.IsChecked == true)
                         {
                             VisionPanel.Visibility = Visibility.Collapsed;
                             CustomVisionPanel.Visibility = Visibility.Visible;

                         }
                         else
                         {
                             VisionPanel.Visibility = Visibility.Visible;
                             CustomVisionPanel.Visibility = Visibility.Collapsed;

                         }
                         if (isPreviewingVideo)
                         {
                             previewButton.Content = "\xE722";
                             ComboResolution.IsEnabled = true;
                             sendPictureButton.IsEnabled = false;
                             openPictureButton.IsEnabled = false;
                         }
                         else
                         {
                             previewButton.Content = "\xE714";
                             ComboResolution.IsEnabled = false;
                             openPictureButton.IsEnabled = true;
                             if (!string.IsNullOrEmpty(currentPicturePath))
                                sendPictureButton.IsEnabled = true;
                             else
                                sendPictureButton.IsEnabled = false;
                         }
                     }
                 });
        }








        #endregion ui


    }
}
