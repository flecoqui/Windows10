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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using AudioVideoPlayer.DataModel;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using System.Reflection;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AudioVideoPlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Attributes
        // Collection of smooth streaming urls 
        private ObservableCollection<MediaItem> defaultViewModel = new ObservableCollection<MediaItem>();
        public ObservableCollection<MediaItem> DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }
        // Timer used to automatically skip to the next stream
        private DispatcherTimer timer ;
        // AutoSkip
        // When the media ended, skip automatically to the next stream in the list 
        private bool bAutoSkip = false;
        // Popup used to display picture in fullscreen mode
        private Popup picturePopup = null;
        enum WindowMediaState
        {
            WindowMode = 0,
            FullWindow,
            FullScreen
        };
        private WindowMediaState windowState;
        // WindowMode define the way the Media is displayed: Window, Full Window, Full Screen mode
        private WindowMediaState WindowState
        {
            get { return windowState; }
            set
            {
                if (windowState != value)
                {
                    windowState = value;
                    LogMessage("Media in " + windowState.ToString() + " state");
                }
            }
        }
        // attribute used to register Smooth Streaming component
        private Windows.Media.MediaExtensionManager extension = null;
        // attribute used to play HLS and DASH component
        private Windows.Media.Streaming.Adaptive.AdaptiveMediaSource adaptiveMediaSource = null; //ams represents the AdaptiveMedaSource used throughout this sample
        // attribute used to register Smooth Streaming component
        private Microsoft.Media.AdaptiveStreaming.AdaptiveSourceManager smoothStreamingManager = null;

        // Url of the current playing media 
        private string CurrentMediaUrl;
        // Url of the poster of the current playing media 
        private string CurrentPosterUrl;
        // Duration for the current playing media 
        private TimeSpan CurrentDuration;
        // Start up position of the current playing media 
        private TimeSpan CurrentStartPosition;
        // Time when the application is starting to play a picture 
        private DateTime StartPictureTime;

        // Default values for MinBitRate and MaxBitRate
        private uint MinBitRate = 100000;
        private uint MaxBitRate = 1000000;

        // Constant Keys used to store parameter in the isolate storage
        private const string keyAutoSkip = "bAutoSkip";
        private const string keyMinBitRate = "MinBitRate";
        private const string keyMaxBitRate = "MaxBitRate";
        private const string keyMediaDataPath = "MediaDataPath";
        private const string keyMediaDataIndex = "MediaDataIndex";
        private const string keyMediaUri = "MediaUri";
        private const string keyWindowState = "WindowState";
        #endregion

        #region Initialization
        /// <summary>
        /// MainPage constructor 
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            LogMessage("Application MainPage Initialized");
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            LogMessage("MainPage OnNavigatedTo");
            await  ReadSettings();

            if (e.NavigationMode != NavigationMode.New)
                RestoreState();

            // Register Suspend/Resume
            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;


            // Register Smooth Streaming component
            RegisterSmoothStreaming();
            // Register PlayReady component
            RegisterPlayReady();

            // Register HLS Library
            RegisterHLSPlugins(extension);


            // Register UI components and events
            await RegisterUI();

            // Load Data
            if (string.IsNullOrEmpty(MediaDataSource.MediaDataPath))
            {
                LogMessage("MainPage Loading Data...");
                await LoadingData(string.Empty);
            }

            // Update control and play first video
            UpdateControls();

            // Stat to play the first asset
            if (bAutoSkip)
                PlayCurrentUrl();

            // Display OS, Device information
            LogMessage(Information.SystemInformation.GetString());

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

            // Register Smooth Streaming component
            UnregisterSmoothStreaming();
            // Register PlayReady component
            UnregisterPlayReady();
            // Unregister HLS library
            RegisterHLSPlugins(extension);

            // Unregister UI components and events
            UnregisterUI();

            SaveState();
        }
        /// <summary>
        /// This method Register the UI components .
        /// </summary>
        public async System.Threading.Tasks.Task<bool> RegisterUI()
        {
            bool bResult = false;
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                SystemControls = Windows.Media.SystemMediaTransportControls.GetForCurrentView();
                if (SystemControls != null)
                    SystemControls.ButtonPressed += SystemControls_ButtonPressed;
            }
            // DisplayInformation used to detect orientation changes
            displayInformation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
            if (displayInformation != null)
            {
                // Register for orientation change
                displayInformation.OrientationChanged += displayInformation_OrientationChanged;
            }
            else
                return false;
            // Set Minimum size for the view
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size
            {
                Height = 240,
                Width = 320
            });

            // Hide Systray on phone
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                // Hide Status bar
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
                // Hide fullWindow button
                fullwindowButton.Visibility = Visibility.Collapsed;
            }

            // Register Size Changed and Key Down
            Window.Current.SizeChanged += OnWindowResize;
            backgroundVideo.SizeChanged += BackgroundVideo_SizeChanged;
            this.KeyDown += OnKeyDown;
            this.DoubleTapped += doubleTapped;

            // Create the popup used to display the pictures in fullscreen
            if (picturePopup == null)
                CreatePicturePopup();
            /*
            if (IsFullScreen())
                WindowMode = WindowMediaState.FullScreen;
            else if (IsFullWindow())
                WindowMode = WindowMediaState.FullWindow;
            else
                WindowMode = WindowMediaState.WindowMode;
                */

            // Initialize MediaElement events
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            mediaElement.MediaFailed += MediaElement_MediaFailed;
            mediaElement.MediaEnded += MediaElement_MediaEnded;
            mediaElement.CurrentStateChanged += MediaElement_CurrentStateChanged;
            mediaElement.DoubleTapped += doubleTapped;
            IsFullWindowToken = mediaElement.RegisterPropertyChangedCallback(MediaElement.IsFullWindowProperty, new DependencyPropertyChangedCallback(IsFullWindowChanged));



            // Combobox event
            comboStream.SelectionChanged += ComboStream_SelectionChanged;
            // Logs event to refresh the TextBox
            logs.TextChanged += Logs_TextChanged;

            // Set AutoSkip mode 
            AutoSkip.IsChecked = bAutoSkip;

            // Initialize minBitrate and maxBitrate TextBox
            minBitrate.Text = MinBitRate.ToString();
            maxBitrate.Text = MaxBitRate.ToString();
            maxBitrate.TextChanged += BitrateTextChanged;
            minBitrate.TextChanged += BitrateTextChanged;

            // Start timer
            timer = new DispatcherTimer();
            if (timer != null)
            {
                timer.Interval = TimeSpan.FromMilliseconds(1000);
                timer.Tick += Timer_Tick;
                timer.Start();
                bResult = true;
            }
            return bResult;
        }
        /// <summary>
        /// This method Unregister the UI components .
        /// </summary>
        public bool UnregisterUI()
        {
            bool bResult = false;
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                SystemControls = Windows.Media.SystemMediaTransportControls.GetForCurrentView();
                if (SystemControls != null)
                    SystemControls.ButtonPressed -= SystemControls_ButtonPressed;
            }
            // DisplayInformation used to detect orientation changes
            displayInformation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
            if (displayInformation != null)
            {
                // Unregister for orientation change
                displayInformation.OrientationChanged -= displayInformation_OrientationChanged;
            }
            else
                return false;

            // Register Size Changed and Key Down
            Window.Current.SizeChanged -= OnWindowResize;
            backgroundVideo.SizeChanged -= BackgroundVideo_SizeChanged;
            this.KeyDown -= OnKeyDown;
            this.DoubleTapped -= doubleTapped;

            // Remove the popup used to display the pictures in fullscreen
            RemovePicturePopup();

            // Initialize MediaElement events
            mediaElement.MediaOpened -= MediaElement_MediaOpened;
            mediaElement.MediaFailed -= MediaElement_MediaFailed;
            mediaElement.MediaEnded -= MediaElement_MediaEnded;
            mediaElement.CurrentStateChanged -= MediaElement_CurrentStateChanged;
            mediaElement.DoubleTapped -= doubleTapped;
            mediaElement.UnregisterPropertyChangedCallback(MediaElement.IsFullWindowProperty, IsFullWindowToken);



            // Combobox event
            comboStream.SelectionChanged -= ComboStream_SelectionChanged;
            // Logs event to refresh the TextBox
            logs.TextChanged -= Logs_TextChanged;

            // Initialize minBitrate and maxBitrate TextBox
            maxBitrate.TextChanged -= BitrateTextChanged;
            minBitrate.TextChanged -= BitrateTextChanged;

            // Stop timer
            if (timer != null)
            {
                timer.Tick -= Timer_Tick;
                timer.Stop();
                bResult = true;
            }
            return bResult;
        }
        /// <summary>
        /// Method LoadingData which loads the JSON playlist file
        /// </summary>
        async System.Threading.Tasks.Task<bool> LoadingData(string path)
        {

            MediaDataGroup audio_video = null;
            string oldPath = MediaDataSource.MediaDataPath;

            try
            {
                MediaDataSource.Clear();
                LogMessage(string.IsNullOrEmpty(path) ? "Loading default playlist" : "Loading playlist :" + path);
                audio_video = await MediaDataSource.GetGroupAsync(path, "audio_video_picture");
                if ((audio_video != null) && (audio_video.Items.Count > 0))
                {
                    LogMessage("MainPage Loading Data successful");
                    TestTitle.Text = audio_video.Title;
                    try
                    {
                        TestLogo.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(audio_video.ImagePath));
                    }
                    catch
                    {

                    }
                    this.defaultViewModel = audio_video.Items;
                    comboStream.DataContext = this.DefaultViewModel;
                    comboStream.SelectedIndex = 0;
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMessage("MainPage Loading Data failed: " + ex.Message);

            }
            return false;
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Invoked when there is a change in the display orientation.
        /// </summary>
        /// <param name="sender">
        /// DisplayInformation object from which the new Orientation can be determined
        /// </param>
        /// <param name="e"></param>
        void displayInformation_OrientationChanged(Windows.Graphics.Display.DisplayInformation sender, object args)
        {
            LogMessage("Orientation Changed: " + sender.CurrentOrientation.ToString());
        }

        #endregion

        #region MediaElement
        // Displayinformation 
        Windows.Graphics.Display.DisplayInformation displayInformation;

        // Media Element FullWindow token
        private long IsFullWindowToken;

        // Create this variable at a global scope. Set it to null.
        private Windows.System.Display.DisplayRequest dispRequest = null;
        /// <summary>
        /// This method request the display to prevent screen saver while the MediaElement is playing a video.
        /// </summary>
        public void RequestDisplay()
        {
            if (dispRequest == null)
            {

                // Activate a display-required request. If successful, the screen is 
                // guaranteed not to turn off automatically due to user inactivity.
                dispRequest = new Windows.System.Display.DisplayRequest();
                dispRequest.RequestActive();
            }
        }
        /// <summary>
        /// This method release the display to allow screen saver.
        /// </summary>
        public void ReleaseDisplay()
        {
            // Insert your own code here to stop the video.
            if (dispRequest != null)
            {
                // Deactivate the display request and set the var to null.
                dispRequest.RequestRelease();
                dispRequest = null;
            }
        }
        /// <summary>
        /// This method is called when the Media State changed .
        /// </summary>
        private void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            MediaElement m;
            if ((sender != null) &&
                ((m = (MediaElement)sender) != null))
            {
                LogMessage("Media CurrentState Changed: " + m.CurrentState.ToString());
                if ((m.CurrentState == MediaElementState.Stopped) ||
                    (m.CurrentState == MediaElementState.Paused))
                    ReleaseDisplay();
                if (m.CurrentState == MediaElementState.Playing)
                    RequestDisplay();
            }
            else
                LogMessage("Media CurrentState Changed: ");
            UpdateControls();

        }
        /// <summary>
        /// This method is called when the Media is opened.
        /// </summary>
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            LogMessage("Media opened");
            if (mediaElement.CanSeek)
            {
                mediaElement.Position = CurrentStartPosition;
            }
            UpdateControls();
        }
        /// <summary>
        /// This method is called when the Media ended.
        /// </summary>
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            LogMessage("Media ended");
            ReleaseDisplay();
            UpdateControls();
            if (bAutoSkip)
            {
                LogMessage("Skipping to next Media on media end...");
                plus_Click(null, null);
            }
        }

        /// <summary>
        /// This method is called when the Media failed.
        /// </summary>
        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            LogMessage("Media failed: " + e.ErrorMessage);
            CurrentMediaUrl = string.Empty;
            CurrentPosterUrl = string.Empty;
            ReleaseDisplay();
            UpdateControls();
        }
        /// <summary>
        /// This method Register the Smooth Streaming component .
        /// </summary>
        public bool RegisterSmoothStreaming()
        {
            bool bResult = false;
            // Smooth Streaming initialization
            // Init SMOOTH Manager
            if(smoothStreamingManager != null)
            {
                smoothStreamingManager.ManifestReadyEvent -= SmoothStreamingManager_ManifestReadyEvent;
                smoothStreamingManager.AdaptiveSourceStatusUpdatedEvent -= SmoothStreamingManager_AdaptiveSourceStatusUpdatedEvent;
                smoothStreamingManager = null;
            }
            smoothStreamingManager = Microsoft.Media.AdaptiveStreaming.AdaptiveSourceManager.GetDefault() as Microsoft.Media.AdaptiveStreaming.AdaptiveSourceManager;
            extension = new Windows.Media.MediaExtensionManager();
            if ((smoothStreamingManager != null) &&
                (extension != null))
            {
                PropertySet ssps = new PropertySet();
                ssps["{A5CE1DE8-1D00-427B-ACEF-FB9A3C93DE2D}"] = smoothStreamingManager;


                extension.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "text/xml", ssps);
                extension.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "application/vnd.ms-sstr+xml", ssps);
                extension.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".isml", "text/xml", ssps);
                extension.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".isml", "application/vnd.ms-sstr+xml", ssps);


                extension.RegisterSchemeHandler("Microsoft.Media.AdaptiveStreaming.SmoothSchemeHandler", "ms-sstr:", ssps);
                extension.RegisterSchemeHandler("Microsoft.Media.AdaptiveStreaming.SmoothSchemeHandler", "ms-sstrs:", ssps);

                smoothStreamingManager.ManifestReadyEvent += SmoothStreamingManager_ManifestReadyEvent;
                smoothStreamingManager.AdaptiveSourceStatusUpdatedEvent += SmoothStreamingManager_AdaptiveSourceStatusUpdatedEvent;
                bResult = true;
            }
            return bResult;
        }
        /// <summary>
        /// This method Unregister the Smooth Streaming component .
        /// </summary>
        public bool UnregisterSmoothStreaming()
        {
            bool bResult = false;
            if (smoothStreamingManager != null)
            {
                smoothStreamingManager.ManifestReadyEvent -= SmoothStreamingManager_ManifestReadyEvent;
                smoothStreamingManager.AdaptiveSourceStatusUpdatedEvent -= SmoothStreamingManager_AdaptiveSourceStatusUpdatedEvent;
                smoothStreamingManager = null;
            }
            return bResult;
        }

        /// <summary>
        /// This method Register the PlayReady component .
        /// </summary>
        public bool RegisterPlayReady()
        {
            bool bResult = false;
            // PlayReady
            // Init PlayReady Protection Manager
            if(protectionManager!=null)
            {
                protectionManager.ComponentLoadFailed -= ProtectionManager_ComponentLoadFailed;
                protectionManager.ServiceRequested -= ProtectionManager_ServiceRequested;
                protectionManager = null;
            }
            protectionManager = new Windows.Media.Protection.MediaProtectionManager();
            if (protectionManager != null)
            {
                Windows.Foundation.Collections.PropertySet cpSystems = new Windows.Foundation.Collections.PropertySet();
                //Indicate to the MF pipeline to use PlayReady's TrustedInput
                cpSystems.Add("{F4637010-03C3-42CD-B932-B48ADF3A6A54}", "Windows.Media.Protection.PlayReady.PlayReadyWinRTTrustedInput");
                //            protectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionSystemIdMapping", cpSystems);

                //            cpSystems.Add("{F4637010-03C3-42CD-B932-B48ADF3A6A54}", "Microsoft.Media.PlayReadyClient.PlayReadyWinRTTrustedInput"); //Playready
                protectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionSystemIdMapping", cpSystems);
                //Use by the media stream source about how to create ITA InitData.
                //See here for more detai: https://msdn.microsoft.com/en-us/library/windows/desktop/aa376846%28v=vs.85%29.aspx
                protectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionSystemId", "{F4637010-03C3-42CD-B932-B48ADF3A6A54}");
                // Setup the container GUID that's in the PPSH box
                protectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionContainerGuid", "{9A04F079-9840-4286-AB92-E65BE0885F95}");

                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                // Check if the platform does support hardware DRM 
                LogMessage((IsHardwareDRMSupported() == true ? "Hardware DRM is supported on this platform" : "Hardware DRM is not supported on this platform"));

                // Associate the MediaElement with the protection manager
                mediaElement.ProtectionManager = protectionManager;
                mediaElement.ProtectionManager.ComponentLoadFailed += ProtectionManager_ComponentLoadFailed;
                mediaElement.ProtectionManager.ServiceRequested += ProtectionManager_ServiceRequested;
                bResult = true;
            }
            return bResult;
        }
        /// <summary>
        /// This method Unregister the PlayReady component .
        /// </summary>
        public bool UnregisterPlayReady()
        {
            bool bResult = false;
            if (protectionManager != null)
            {
                protectionManager.ComponentLoadFailed -= ProtectionManager_ComponentLoadFailed;
                protectionManager.ServiceRequested -= ProtectionManager_ServiceRequested;
                protectionManager = null;
                bResult = true;
            }
            return bResult;
        }

        /// <summary>
        /// This method is called every second.
        /// </summary>
        private void Timer_Tick(object sender, object e)
        {
            if (CurrentDuration != TimeSpan.Zero)
            {
                if (!IsPicture(CurrentMediaUrl))
                {
                    TimeSpan t = mediaElement.Position - CurrentStartPosition;
                    if (t > CurrentDuration)
                    {
                        if (bAutoSkip)
                        {
                            LogMessage("Skipping to next Media on timer tick...");
                            plus_Click(null, null);
                        }
                    }
                }
                else
                {
                    TimeSpan t = DateTime.Now - StartPictureTime;
                    if (t > CurrentDuration)
                    {
                        if (bAutoSkip)
                        {
                            LogMessage("Skipping to next Media on timer tick...");
                            plus_Click(null, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the instance of the SystemMediaTransportControls being used.
        /// </summary>
        public Windows.Media.SystemMediaTransportControls SystemControls { get; private set; }
        async void SystemControls_ButtonPressed(Windows.Media.SystemMediaTransportControls sender, Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case Windows.Media.SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LogMessage("Pause from SystemMediaTransportControls");
                        mediaElement.Pause();
                    });
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LogMessage("Play from SystemMediaTransportControls");
                        mediaElement.Play();
                    });
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Stop:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LogMessage("Stop from SystemMediaTransportControls");
                        mediaElement.Stop();
                    });
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Previous:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LogMessage("Previous from SystemMediaTransportControls");
                    });
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Next:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LogMessage("Next from SystemMediaTransportControls");
                    });
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Rewind:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LogMessage("Rewind from SystemMediaTransportControls");
                    });
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.FastForward:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LogMessage("FastForward from SystemMediaTransportControls");
                    });
                    break;
            }
        }
        #endregion

        #region SuspendResume



        /// <summary>
        /// This method saves the MediaElement position and the media state 
        /// it also saves the MediaCache
        /// </summary>
        void SaveState()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["PlayerPosition"] = mediaElement.Position;
            int i = (int)mediaElement.CurrentState;
            localSettings.Values["PlayerState"] = i;
            LogMessage("SaveState - Position: " + mediaElement.Position.ToString() + " State: " + mediaElement.CurrentState.ToString());
            mediaElement.Pause();
        }
        /// <summary>
        /// This method restores the MediaElement position and the media state 
        /// it also restores the MediaCache
        /// </summary>
        void RestoreState()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object value = localSettings.Values["PlayerPosition"];
            if (value != null)
            {
                TimeSpan t = (TimeSpan)value;
                if (t != null)
                {
                    mediaElement.Position = t;
                }
            }
            value = localSettings.Values["PlayerState"];
            if (value != null)
            {
                int i = (int)value;
                MediaElementState t = (MediaElementState)i;
                if (t != MediaElementState.Paused)
                    mediaElement.Play();
                else
                    mediaElement.Pause();
            }
            LogMessage("RestoreState - Position: " + mediaElement.Position.ToString() + " State: " + mediaElement.CurrentState.ToString());
        }
        /// <summary>
        /// This method is called when the application is resuming
        /// </summary>
        void Current_Resuming(object sender, object e)
        {
            LogMessage("Resuming");
            //await ReadSettings();
            RestoreState();
            // Register for orientation change
            displayInformation.OrientationChanged += displayInformation_OrientationChanged;

            // Resotre Playback Rate
            if (mediaElement.PlaybackRate != mediaElement.DefaultPlaybackRate)
                mediaElement.PlaybackRate = mediaElement.DefaultPlaybackRate;
        }
        /// <summary>
        /// This method is called when the application is suspending
        /// </summary>
        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            LogMessage("Suspending");
            var deferal = e.SuspendingOperation.GetDeferral();
            // Register for orientation change
            displayInformation.OrientationChanged -= displayInformation_OrientationChanged;
            SaveSettings();
            SaveState();
            deferal.Complete();
        }
        #endregion

        #region Logs
        System.Collections.Concurrent.ConcurrentQueue<String> MessageList;
        /// <summary>
        /// Display Message on the application page
        /// </summary>
        /// <param name="Message">String to display</param>
        async void LogMessage(string Message)
        {
            if (MessageList == null)
                MessageList = new System.Collections.Concurrent.ConcurrentQueue<string>();
            string Text = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " " + Message + "\n";
            MessageList.Enqueue(Text);
            System.Diagnostics.Debug.WriteLine(Text);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {

                    string result;
                    //  double h = logs.ActualHeight;
                    while (MessageList.TryDequeue(out result))
                    {
                        logs.Text += result;
                        if (logs.Text.Length > 16000)
                        {
                            string LocalString = logs.Text;
                            while (LocalString.Length > 12000)
                            {
                                int pos = LocalString.IndexOf('\n');
                                if ((pos > 0) && (pos < LocalString.Length))
                                {
                                    LocalString = LocalString.Substring(pos + 1);
                                }
                            }
                            logs.Text = LocalString;
                        }
                    }
                    //if (logs.ActualHeight > h)
                    //  logs.Height = h;

                }
            );
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

        #region UIEvents 
        /// <summary>
        /// Check if the bitrates are consistent 
        /// </summary>
        private bool CheckBitrates(string smin, string smax)
        {
            uint max, min;
            if (uint.TryParse(smin, out min))
                if (uint.TryParse(smax, out max))
                    if (min <= max)
                        return true;
            return false;
        }
        /// <summary>
        /// This method is called when the content of the minBitrate and maxBitrate TextBox changed  
        /// </summary>
        void BitrateTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                uint n;
                if (!uint.TryParse(tb.Text, out n))
                {
                    if (tb == minBitrate)
                        tb.Text = MinBitRate.ToString();
                    if (tb == maxBitrate)
                        tb.Text = MaxBitRate.ToString();
                }
                else
                {

                    if (tb == minBitrate)
                    {
                        if (CheckBitrates(tb.Text, maxBitrate.Text))
                            MinBitRate = n;
                        else
                            tb.Text = MinBitRate.ToString();
                    }
                    if (tb == maxBitrate)
                    {
                        if (CheckBitrates(minBitrate.Text, tb.Text))
                            MaxBitRate = n;
                        else
                            tb.Text = MaxBitRate.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// UpdateControls Method which update the controls on the page  
        /// </summary>
        void UpdateControls()
        {
            if (comboStream.Items.Count > 0)
            {
                playButton.IsEnabled = true;


                minusButton.IsEnabled = true;
                plusButton.IsEnabled = true;
                muteButton.IsEnabled = true;
                volumeDownButton.IsEnabled = true;
                volumeUpButton.IsEnabled = true;

                playPauseButton.IsEnabled = false;
                pausePlayButton.IsEnabled = false;
                stopButton.IsEnabled = false;


                if (IsPicture(CurrentMediaUrl))
                {
                    fullscreenButton.IsEnabled = true;
                    fullwindowButton.IsEnabled = true;
                }
                else
                {
                    if (mediaElement.CurrentState == MediaElementState.Opening)
                    {
                        fullscreenButton.IsEnabled = true;
                        fullwindowButton.IsEnabled = true;
                    }
                    else if (mediaElement.CurrentState == MediaElementState.Playing)
                    {
                        if (string.Equals(mediaUri.Text, CurrentMediaUrl))
                        {
                            playPauseButton.IsEnabled = false;
                            pausePlayButton.IsEnabled = true;
                            stopButton.IsEnabled = true;
                        }
                    }
                    else if (mediaElement.CurrentState == MediaElementState.Paused)
                    {
                        playPauseButton.IsEnabled = true;
                        stopButton.IsEnabled = true;
                    }
                    else if ((mediaElement.CurrentState == MediaElementState.Stopped) ||
                        (mediaElement.CurrentState == MediaElementState.Closed))
                    {
                       // mediaElement.AreTransportControlsEnabled = false;
                        fullscreenButton.IsEnabled = false;
                        fullwindowButton.IsEnabled = false;
                    }
                }
                // Volume buttons control
                if (mediaElement.IsMuted)
                    muteButton.Content = "\xE767";
                else
                    muteButton.Content = "\xE74F";
                if (mediaElement.Volume == 0)
                {
                    volumeDownButton.IsEnabled = false;
                    volumeUpButton.IsEnabled = true;
                }
                else if (mediaElement.Volume >= 1)
                {
                    volumeDownButton.IsEnabled = true;
                    volumeUpButton.IsEnabled = false;
                }
                else
                {
                    volumeDownButton.IsEnabled = true;
                    volumeUpButton.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Play method which plays the video with the MediaElement from position 0
        /// </summary>
        private void play_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayCurrentUrl();
            }
            catch (Exception ex)
            {
                LogMessage("Failed to play: " + mediaUri.Text + " Exception: " + ex.Message);
            }
        }
        /// <summary>
        /// Stop method which stops the video currently played by the MediaElement
        /// </summary>
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((!string.IsNullOrEmpty(CurrentMediaUrl)) &&
                    (!string.IsNullOrEmpty(mediaUri.Text)) &&
                    (string.Equals(mediaUri.Text, CurrentMediaUrl)))
                {
                    LogMessage("Stop " + CurrentMediaUrl.ToString());
                    mediaElement.Stop();
                    mediaElement.Source = null;
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to stop: " + mediaUri.Text + " Exception: " + ex.Message);
            }
        }
        /// <summary>
        /// Play method which plays the video currently paused by the MediaElement
        /// </summary>
        private void playPause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((!string.IsNullOrEmpty(CurrentMediaUrl)) &&
                    (!string.IsNullOrEmpty(mediaUri.Text)) &&
                    (string.Equals(mediaUri.Text, CurrentMediaUrl)))
                {
                    LogMessage("Play " + CurrentMediaUrl.ToString());
                    mediaElement.Play();
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to play: " + mediaUri.Text + " Exception: " + ex.Message);
            }
        }
        /// <summary>
        /// Pause method which pauses the video currently played by the MediaElement
        /// </summary>
        private void pausePlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((!string.IsNullOrEmpty(CurrentMediaUrl)) &&
                    (!string.IsNullOrEmpty(mediaUri.Text)) &&
                    (string.Equals(mediaUri.Text, CurrentMediaUrl)))
                {
                    LogMessage("Play " + CurrentMediaUrl.ToString());
                    mediaElement.Pause();
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to play: " + mediaUri.Text + " Exception: " + ex.Message);
            }
        }


        /// <summary>
        /// Playlist method which loads another JSON playlist for the application 
        /// </summary>
        private async void playlist_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
            filePicker.FileTypeFilter.Add(".json");
            filePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            filePicker.SettingsIdentifier = "PlaylistPicker";
            filePicker.CommitButtonText = "Open JSON Playlist File to Process";

            var file = await filePicker.PickSingleFileAsync();
            if(file!=null)
            {
                string fileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                LogMessage("Stopping MediaElement before Loading playlist");
                try
                {
                    mediaElement.Stop();
                    mediaElement.Source = null;
                }
                catch (Exception)
                {

                }
                if (await LoadingData(file.Path) == false)
                    await LoadingData(string.Empty);
                //Update control and play first video
                UpdateControls();
                PlayCurrentUrl();
            }
        }


        /// <summary>
        /// Channel up method 
        /// </summary>
        private void plus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int Index = comboStream.SelectedIndex;
                Index = (Index+1>=comboStream.Items.Count? 0: ++Index);
                comboStream.SelectedIndex = Index;
                MediaItem ms = comboStream.SelectedItem as MediaItem;
                mediaUri.Text = ms.Content;
                PlayCurrentUrl();

            }
            catch (Exception ex)
            {
                LogMessage("Failed to to play: " + mediaUri.Text + " Exception: " + ex.Message);
            }
        }
        /// <summary>
        /// Channel down method 
        /// </summary>
        private void minus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int Index = comboStream.SelectedIndex;
                Index = (Index - 1 >= 0 ? --Index : comboStream.Items.Count-1);
                comboStream.SelectedIndex = Index;
                MediaItem ms = comboStream.SelectedItem as MediaItem;
                mediaUri.Text = ms.Content;
                PlayCurrentUrl();
            }
            catch (Exception ex)
            {
                LogMessage("Failed to to play: " + mediaUri.Text + " Exception: " + ex.Message);
            }
        }


        /// <summary>
        /// Mute method 
        /// </summary>
        private void mute_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Toggle Mute");
            mediaElement.IsMuted = !mediaElement.IsMuted;
            UpdateControls();
        }
        /// <summary>
        /// Volume Up method 
        /// </summary>
        private void volumeUp_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Volume Up");
            mediaElement.Volume = (mediaElement.Volume + 0.10 <= 1 ? mediaElement.Volume + 0.10 : 1) ;
            UpdateControls();
        }
        /// <summary>
        /// Volume Down method 
        /// </summary>
        private void volumeDown_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Volume Down");
            mediaElement.Volume = (mediaElement.Volume - 0.10 >= 0 ? mediaElement.Volume - 0.10 : 0);
            UpdateControls();
        }
        /// <summary>
        /// This method is called when the AutoSkip is unchecked
        /// </summary>
        private void AutoSkip_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AutoSkip.IsChecked == true)
                bAutoSkip = true;
            else
                bAutoSkip = false;
        }
        /// <summary>
        /// This method is called when the AutoSkip is checked 
        /// </summary>
        private void AutoSkip_Checked(object sender, RoutedEventArgs e)
        {
            if (AutoSkip.IsChecked == true)
                bAutoSkip = true;
            else
                bAutoSkip = false;
        }
        /// <summary>
        /// This method is called when the ComboStream selection changes 
        /// </summary>
        private void ComboStream_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboStream.SelectedItem != null)
            {
                MediaItem ms = comboStream.SelectedItem as MediaItem;
                mediaUri.Text = ms.Content;

                try
                {
                    streamLogo.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(ms.ImagePath));
                }
                catch
                {
                }

                // Update PlayReady URLs and Custom Data
                PlayReadyLicenseUrl = ms.PlayReadyUrl;
                PlayReadyChallengeCustomData = ms.PlayReadyCustomData;
                if (!string.Equals(CurrentMediaUrl, mediaUri.Text))
                    mediaElement.Stop();
                UpdateControls();
            }

        }
        #endregion

        #region WindowMode
        /// <summary>
        /// IsFullScreen
        /// This method return true if the application is running in Full Screen mode
        /// </summary>
        bool IsFullScreen()
        {
            var view = ApplicationView.GetForCurrentView();
            if (((picturePopup != null) && (picturePopup.IsOpen == true)) ||
                ((mediaElement.IsFullWindow) &&
                (mediaElement.AreTransportControlsEnabled == false)) ||
                ((view != null) && (view.IsFullScreenMode)))
                return true;
            return false;
        }
        /// <summary>
        /// IsFullScreen
        /// This method return true if the application is running in Full Window mode
        /// </summary>
        bool IsFullWindow()
        {
            if ((mediaElement.IsFullWindow) &&
                (mediaElement.AreTransportControlsEnabled == false))
                return true;
            return false;
        }
        /// <summary>
        /// IsFullWindowChanged
        /// This method is called when MediaElement property IsFullWindow changed 
        /// </summary>
        private void IsFullWindowChanged(DependencyObject obj, DependencyProperty prop)
        {
            if (mediaElement.IsFullWindow)
            {
                if (mediaElement.AreTransportControlsEnabled == true)
                {
                    WindowState = WindowMediaState.FullScreen;
                    LogMessage("Media is in Full Screen mode");
                }
                else
                {
                    WindowState = WindowMediaState.FullWindow;
                    LogMessage("Media is in Full Window mode");
                }
            }
            else
            {
                mediaElement.AreTransportControlsEnabled = false;
                WindowState = WindowMediaState.WindowMode;
                SetWindowMode(WindowState);
                LogMessage("Media is in Window mode");
            }
        }


        /// <summary>
        /// Full screen method 
        /// </summary>
        private void fullscreen_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Switch to fullscreen");
            SetWindowMode(WindowMediaState.FullScreen);
        }
        /// <summary>
        /// Create picturePopup
        /// ¨Popup used to display picture in fullscreen
        /// </summary>
        bool CreatePicturePopup()
        {
            picturePopup = new Popup()
            {
                Child = new StackPanel
                {
                    Background = new SolidColorBrush(Windows.UI.Colors.Black),
                }
            };
            if (picturePopup != null)
            {
                StackPanel c = picturePopup.Child as StackPanel;
                if (c != null)
                {
                    c.DoubleTapped += doubleTapped;
                    c.Children.Add(
                        new Image
                        {
                            Stretch = Stretch.Uniform,
                        }
                        );
                    return true;
                };
            }
            return false;
        }
        /// <summary>
        /// Remove picturePopup
        /// </summary>
        bool RemovePicturePopup()
        {
            if (picturePopup != null)
            {
                StackPanel c = picturePopup.Child as StackPanel;
                if (c != null)
                {
                    c.DoubleTapped -= doubleTapped;
                    c.Children.Clear();
                };
                picturePopup.Child = null;
                picturePopup = null;
            }
            return false;
        }
        /// <summary>
        /// Display picturePopup
        /// ¨Diplay or hide picturePopup
        /// </summary>
        bool DisplayPicturePopup(bool bDisplay)
        {
            if (picturePopup != null)
            {
                picturePopup.IsOpen = bDisplay;
                return true;
            }
            return false;
        }

        /// <summary>
        /// SetFullWindow for MediaElement, PictureElement and picturePopup
        /// </summary>
        bool SetWindowMode(WindowMediaState state)
        {
            if (state == WindowMediaState.FullWindow)
            {
                // if playing a picture or a video or audio with poster                
                if (pictureElement.Visibility == Visibility.Visible)
                {
                    DisplayPicturePopup(true);
                    ResizePicturePopup(Window.Current.Bounds.Width, Window.Current.Bounds.Height);
                }
                else
                {
                    if (mediaElement.AreTransportControlsEnabled == true)
                        mediaElement.AreTransportControlsEnabled = false;
                    if (mediaElement.IsFullWindow == false)
                        mediaElement.IsFullWindow = true;
                    DisplayPicturePopup(false);
                }
                WindowState = WindowMediaState.FullWindow;

            }
            else if (state == WindowMediaState.FullScreen)
            {

                // if playing a picture or a video or audio with poster                
                if (pictureElement.Visibility == Visibility.Visible)
                {
                    var view = ApplicationView.GetForCurrentView();
                    if (!view.IsFullScreenMode)
                        view.TryEnterFullScreenMode();
                    DisplayPicturePopup(true);
                }
                else
                {
                    if (mediaElement.AreTransportControlsEnabled == false)
                        mediaElement.AreTransportControlsEnabled = true;
                    if (mediaElement.IsFullWindow == false)
                        mediaElement.IsFullWindow = true;
                    DisplayPicturePopup(false);
                }
                WindowState = WindowMediaState.FullScreen;
            }
            else
            {

                var view = ApplicationView.GetForCurrentView();
                if ((view.IsFullScreenMode) || (view.IsFullScreen))
                    view.ExitFullScreenMode();
                DisplayPicturePopup(false);
                if (mediaElement.IsFullWindow == true)
                    mediaElement.IsFullWindow = false;
                if (mediaElement.AreTransportControlsEnabled == true)
                    mediaElement.AreTransportControlsEnabled = false;
                WindowState = WindowMediaState.WindowMode;
            }
            return true;
        }

        /// <summary>
        /// Full window method 
        /// </summary>
        private void fullwindow_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Switch to fullwindow");
            SetWindowMode(WindowMediaState.FullWindow);
        }
        /// <summary>
        /// Set the source for the picture : windows and fullscreen 
        /// </summary>
        void SetPictureSource(Windows.UI.Xaml.Media.Imaging.BitmapImage b)
        {
            // Set picture source for windows element
            pictureElement.Source = b;
            // If doesn't exist create picture popup for fullscreen display
            if (picturePopup == null)
                CreatePicturePopup();
            // Set picture source for fullscreen element
            if (picturePopup != null)
            {
                StackPanel c = picturePopup.Child as StackPanel;
                if (c != null)
                {
                    Image im = c.Children.First() as Image;
                    if (im != null)
                    {
                        im.Source = b;
                    }
                }
            }
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
        /// BackgroundVideo picture Resize Event
        /// </summary>        
        void BackgroundVideo_SizeChanged(System.Object sender, SizeChangedEventArgs e)
        {
            SetPictureElementSize();
        }
        /// <summary>
        /// Resize the picturePopup
        /// </summary>
        void ResizePicturePopup(double Width, double Height)
        {
            if (picturePopup != null)
            {
                StackPanel c = picturePopup.Child as StackPanel;
                if (c != null)
                {
                    c.Width = Width;
                    c.Height = Height;

                    Image im = c.Children.First() as Image;
                    if (im != null)
                    {
                        im.Width = Width;
                        im.Height = Height;
                    }
                }
            }

        }
        /// <summary>
        /// Windows Resize Event
        /// </summary>
        void OnWindowResize(object sender, WindowSizeChangedEventArgs e)
        {
            // Resize the picture popup accordingly 
            ResizePicturePopup(e.Size.Width, e.Size.Height);
            // Update Controls
            UpdateControls();
        }
        /// <summary>
        /// KeyDown event to exit full screen mode
        /// </summary>
        void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                SetWindowMode(WindowMediaState.WindowMode);
            }
        }

        /// <summary>
        /// Method called when the , picturePopup stackpanel or page  is double Tapped
        /// </summary>
        private void doubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (IsFullScreen())
                SetWindowMode(WindowMediaState.WindowMode);
        }

        #endregion

        #region Media


        /// <summary>
        /// This method checks if the url is a picture url 
        /// </summary>
        private bool IsPicture(string url)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(url))
            {
                if ((url.ToLower().EndsWith(".jpg")) ||
                    (url.ToLower().EndsWith(".png")) ||
                    (url.ToLower().EndsWith(".bmp")) ||
                    (url.ToLower().EndsWith(".ico")) ||
                    (url.ToLower().EndsWith(".gif")))
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// This method checks if the url is the url of a local file 
        /// </summary>
        private bool IsLocalFile(string url)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(url))
            {
                if ((url.ToLower().StartsWith("file://")) ||
                    (url.ToLower().StartsWith("picture://")) ||
                    (url.ToLower().StartsWith("video://")) ||
                    (url.ToLower().StartsWith("music://")))
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// This method checks if the url is a SMOOTH, HLS or DASH url
        /// </summary>
        private bool IsAdaptiveStreaming(string url)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(url))
            {
                if (url.ToLower().Contains("/manifest") ||
                    url.ToLower().Contains(".m3u8") ||
                    url.ToLower().Contains(".mpd")
                    )
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// This method checks if the url is a smooth streaming url
        /// </summary>
        private bool IsSmoothStreaming(string url)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(url))
            {
                if(url.ToLower().EndsWith("/manifest"))
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// This method prepare the MediaElement to play any content (video, audio, pictures): SMOOTH, DASH, HLS, MP4, WMV, MPEG2-TS, JPG, PNG,...
        /// </summary>
        private async void PlayCurrentUrl()
        {

            MediaItem item = comboStream.SelectedItem as MediaItem;
            if (item != null)
            {
                await StartPlay(mediaUri.Text, item.PosterContent, item.Start, item.Duration);
                UpdateControls();
            }
        }
        /// <summary>
        /// This method set the poster source for the MediaElement 
        /// </summary>
        private async System.Threading.Tasks.Task<bool> SetPosterUrl(string PosterUrl)
        {
            if (IsPicture(PosterUrl))
            {
                if (IsLocalFile(PosterUrl))
                {
                    try
                    {
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
                            LogMessage("Failed to load poster: " + PosterUrl);

                    }
                    catch (Exception e)
                    {
                        LogMessage("Exception while loading poster: " + PosterUrl + " - " + e.Message);
                    }
                }
                else
                {
                    try
                    {
                        
                        // Load the bitmap image over http
                        Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
                        Windows.Storage.Streams.InMemoryRandomAccessStream ras = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                        using (var stream = await httpClient.GetInputStreamAsync(new Uri(PosterUrl)))
                        {
                            if (stream != null)
                            {
                                await stream.AsStreamForRead().CopyToAsync(ras.AsStreamForWrite());
                                ras.Seek(0);
                                var b = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                                if (b != null)
                                {
                                    await b.SetSourceAsync(ras);
                                    SetPictureSource(b);
                                    SetPictureElementSize();
                                    return true;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                    }
                }
            }

            return false;
        }
        
        /// <summary>
        /// StartPlay
        /// Start to play pictures, audio content or video content
        /// </summary>
        /// <param name="content">Url string of the content to play </param>
        /// <param name="poster">Url string of the poster associated with the content to play </param>
        /// <param name="start">start position of the content to play in milliseconds</param>
        /// <param name="duration">duration of the content to play in milliseconds</param>
        /// <returns>true if success</returns>
        private async System.Threading.Tasks.Task<bool> StartPlay(string content, string poster, long start, long duration)
        {

            try
            {

                bool result = false;
                if (string.IsNullOrEmpty(content))
                {
                    LogMessage("Empty Uri");
                    return result;
                }
                LogMessage("Start to play: " + content + (string.IsNullOrEmpty(poster)?"" : " with poster: " + poster) + (start>0?" from " + start.ToString() + "ms":"") + (start > 0 ? " during " + duration.ToString() + "ms" : "")) ;
                // Display the PlayReady expiration date for this video (if protected)
                if (PlayReadyUrlKeyIdDictionary.ContainsKey(content))
                {
                    // Get the expiration date of PlayReady license
                    DateTime d = GetLicenseExpirationDate(PlayReadyUrlKeyIdDictionary[content]);
                    if (d != DateTime.MinValue)
                    {
                        LogMessage("Video: " + content + " is protected with PlayReady and the license Expiration Date is: " + d.ToString());
                    }
                }

                // The application does restore the default DRM configuration hardware DRM or software DRM
                // before playing the asset
                // if the content to play is based on VC1 codec and the hardware DRM is enabled,
                // the software DRM will be enabled on event Manifest Ready
                LogMessage("Restoring the DRM configuration: " + (IsHardwareDRMSupported() == true ? "Hardware DRM" : "Software DRM"));
                EnableSoftwareDRM(!IsHardwareDRMSupported());

                // Stop the current stream
                mediaElement.Source = null;
                mediaElement.PosterSource = null;
                mediaElement.AutoPlay = true;
                CurrentMediaUrl = string.Empty;
                CurrentPosterUrl = string.Empty;
                CurrentStartPosition = new TimeSpan(0);
                CurrentDuration = new TimeSpan(0);
                StartPictureTime = DateTime.MinValue;
                if (IsPicture(content))
                    result = await SetPosterUrl(content);
                else if (IsPicture(poster))
                    result = await SetPosterUrl(poster);
                // if a picture will be displayed
                // display or not popup
                if (result == true)
                {
                    pictureElement.Visibility = Visibility.Visible;
                    StartPictureTime = DateTime.Now;
                }
                else
                {
                    SetPictureSource(null);
                    pictureElement.Visibility = Visibility.Collapsed;
                }
                // Audio or video
                if (!IsPicture(content))
                {
                    result = await SetAudioVideoUrl(content);
                    if (result == true)
                        mediaElement.Play();
                }
                if (result == true)
                {
                    CurrentStartPosition = new TimeSpan(start * 10000);
                    CurrentDuration = new TimeSpan(duration * 10000);
                    CurrentMediaUrl = content;
                    CurrentPosterUrl = poster;
                    // Set Window Mode
                    SetWindowMode(WindowState);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMessage("Exception Playing: " + ex.Message.ToString());
                CurrentMediaUrl = string.Empty;
                CurrentPosterUrl = string.Empty;
            }
            return false;
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
            else
                path = PosterUrl.Replace("file://", "");
            Windows.Storage.StorageFile file = null;
            if (folder != null)
                file = await folder.GetFileAsync(path);
            else
                file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);

            return file;
        }

        /// <summary>
        /// SetAudioVideoUrl
        /// Prepare the MediaElement to play audio or video content 
        /// </summary>
        /// <param name="content">Url string of the content to play </param>
        /// <returns>true if success</returns>
        private async System.Threading.Tasks.Task<bool> SetAudioVideoUrl(string Content)
        {
            try
            {
                if (IsLocalFile(Content))
                {
                    Windows.Storage.StorageFile file = await GetFileFromLocalPathUrl(Content);
                    if (file != null)
                    {
                        var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        if (fileStream != null)
                        {
                            mediaElement.SetSource(fileStream, file.FileType);
                            return true;
                        }
                    }
                    else
                        LogMessage("Failed to load media file: " + Content);
                }
                else if (!IsAdaptiveStreaming(Content))
                {
                    mediaElement.Source = new Uri(Content);
                    return true;
                }
                else
                {

                    // If SMOOTH stream
                    if (IsSmoothStreaming(Content))
                    {
                        mediaElement.Source = new Uri(Content);
                        return true;
                    }
                    else
                    {
                        // If HLS content uses legacy HLS Libray
                        if (IsHLSUrl(Content))
                        {
                            Content = UpdateHLSUrl(Content);
                            mediaElement.Source = new Uri(Content);
                            return true;

                        }
                        else
                        {
                            // If DASH content
                            // Create the AdaptiveMediaSource
                            Windows.Media.Streaming.Adaptive.AdaptiveMediaSourceCreationResult result = await Windows.Media.Streaming.Adaptive.AdaptiveMediaSource.CreateFromUriAsync(new Uri(Content));
                            if (result.Status == Windows.Media.Streaming.Adaptive.AdaptiveMediaSourceCreationStatus.Success)
                            {
                                if (adaptiveMediaSource != null)
                                {
                                    adaptiveMediaSource.DownloadBitrateChanged -= AdaptiveMediaSource_DownloadBitrateChanged;
                                    adaptiveMediaSource.DownloadCompleted -= AdaptiveMediaSource_DownloadCompleted;
                                    adaptiveMediaSource.DownloadFailed -= AdaptiveMediaSource_DownloadFailed;
                                    adaptiveMediaSource.DownloadRequested -= AdaptiveMediaSource_DownloadRequested;
                                    adaptiveMediaSource.PlaybackBitrateChanged -= AdaptiveMediaSource_PlaybackBitrateChanged;
                                }
                                adaptiveMediaSource = result.MediaSource;
                                adaptiveMediaSource.DownloadBitrateChanged += AdaptiveMediaSource_DownloadBitrateChanged;
                                adaptiveMediaSource.DownloadCompleted += AdaptiveMediaSource_DownloadCompleted;
                                adaptiveMediaSource.DownloadFailed += AdaptiveMediaSource_DownloadFailed;
                                adaptiveMediaSource.DownloadRequested += AdaptiveMediaSource_DownloadRequested;
                                adaptiveMediaSource.PlaybackBitrateChanged += AdaptiveMediaSource_PlaybackBitrateChanged;

                                LogMessage("Available bitrates: ");
                                uint startupBitrate = 0;
                                foreach (var b in adaptiveMediaSource.AvailableBitrates)
                                {
                                    LogMessage("bitrate: " + b.ToString() + " b/s ");
                                    if ((startupBitrate == 0) &&
                                        (b >= MinBitRate) &&
                                        (b <= MaxBitRate))
                                        startupBitrate = b;
                                }
                                // Set bitrate range for HLS and DASH
                                if (startupBitrate > 0)
                                    adaptiveMediaSource.InitialBitrate = startupBitrate;
                                adaptiveMediaSource.DesiredMaxBitrate = MaxBitRate;
                                adaptiveMediaSource.DesiredMinBitrate = MinBitRate;

                                mediaElement.SetMediaStreamSource(adaptiveMediaSource);
                                return true;
                            }
                            else
                                LogMessage("Failed to create AdaptiveMediaSource: " + result.Status.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("Exception Playing: " + ex.Message.ToString());
                CurrentMediaUrl = string.Empty;
                CurrentPosterUrl = string.Empty;
            }
            return false;
        }
        #endregion

        #region SmoothStreaming
        /// <summary>
        /// Called when the SMOOTH manifest has been downloaded and parsed
        /// If the asset is in the cache don't restrick track: the MediaCache will select the correct audio and video track
        /// </summary>
        private void SmoothStreamingManager_ManifestReadyEvent(Microsoft.Media.AdaptiveStreaming.AdaptiveSource sender, Microsoft.Media.AdaptiveStreaming.ManifestReadyEventArgs args)
        {
            // VC1 Codec flag: true if VC1 codec is used for this Smooth Streaming content
            bool bVC1CodecDetected = false;
            LogMessage("Manifest Ready for uri: " + sender.Uri.ToString());
            foreach (var stream in args.AdaptiveSource.Manifest.SelectedStreams)
            {

                if (stream.Type == Microsoft.Media.AdaptiveStreaming.MediaStreamType.Video)
                {
                    foreach (var track in stream.SelectedTracks)
                    {
                        LogMessage("  Bitrate: " + track.Bitrate.ToString() + " Width: " + track.MaxWidth.ToString() + " Height: " + track.MaxHeight.ToString() + " FourCC: " + track.FourCC);
                        if ((bVC1CodecDetected == false) && ((track.FourCC == 0x31435657/*WVC1*/) || (track.FourCC == 0x31435641 /*AVC1*/)))
                            bVC1CodecDetected = true;
                    }

                    IReadOnlyList<Microsoft.Media.AdaptiveStreaming.IManifestTrack> list = null;

                    // if asset not in the Cache the application can restrick track
                    if ((MinBitRate > 0) && (MaxBitRate > 0))
                    {
                        list = stream.AvailableTracks.Where(t => (t.Bitrate > MinBitRate) && (t.Bitrate <= MaxBitRate)).ToList();
                        if ((list != null) && (list.Count > 0))
                            stream.RestrictTracks(list);
                    }
                    else if (MinBitRate > 0)
                    {
                        list = stream.AvailableTracks.Where(t => (t.Bitrate > MinBitRate)).ToList();
                        if ((list != null) && (list.Count > 0))
                            stream.RestrictTracks(list);
                    }
                    else if (MaxBitRate > 0)
                    {
                        list = stream.AvailableTracks.Where(t => (t.Bitrate < MaxBitRate)).ToList();
                        if ((list != null) && (list.Count > 0))
                            stream.RestrictTracks(list);
                    }
                    if ((list != null) && (list.Count > 0))
                    {
                        LogMessage("Select Bitrate between: " + MinBitRate.ToString() + " and " + MaxBitRate.ToString());
                        foreach (var track in stream.SelectedTracks)
                        {
                            LogMessage("  Bitrate: " + track.Bitrate.ToString() + " Width: " + track.MaxWidth.ToString() + " Height: " + track.MaxHeight.ToString());

                        }
                    }
                }
            }
            // if the platform does support Hardware DRM and VC1 codec is used by this content
            // the application will force the Software DRM  as current Hardware DRM implementation doesn't support VC1 codec 
            if ((bVC1CodecDetected == true) && (IsHardwareDRMEnabled()))
            {
                LogMessage("Enable Software DRM as VC1 content has been detected");
                EnableSoftwareDRM(true);
            }
        }
        /// <summary>
        /// Called when the bitrate changed for SMOOTH streams
        /// </summary>
        private void SmoothStreamingManager_AdaptiveSourceStatusUpdatedEvent(Microsoft.Media.AdaptiveStreaming.AdaptiveSource sender, Microsoft.Media.AdaptiveStreaming.AdaptiveSourceStatusUpdatedEventArgs args)
        {
            //LogMessage("AdaptiveSourceStatusUpdatedEvent for uri: " + sender.Uri.ToString());
            if (args != null)
            {
                if (args.UpdateType == Microsoft.Media.AdaptiveStreaming.AdaptiveSourceStatusUpdateType.BitrateChanged)
                {

                    LogMessage("Bitrate changed for uri: " + sender.Uri.ToString());
                    foreach (var stream in args.AdaptiveSource.Manifest.SelectedStreams)
                    {
                        if (stream.Type == Microsoft.Media.AdaptiveStreaming.MediaStreamType.Video)
                        {
                            if (!string.IsNullOrEmpty(args.AdditionalInfo))
                            {
                                int pos = args.AdditionalInfo.IndexOf(';');
                                if (pos > 0)
                                {
                                    try
                                    {

                                        var newBitrate = uint.Parse(args.AdditionalInfo.Substring(0, pos));
                                        foreach (var track in stream.SelectedTracks)
                                        {
                                            if (track.Bitrate == newBitrate)
                                            {
                                                LogMessage("  Bitrate: " + track.Bitrate.ToString() + " Width: " + track.MaxWidth.ToString() + " Height: " + track.MaxHeight.ToString());
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
                else if (args.UpdateType == Microsoft.Media.AdaptiveStreaming.AdaptiveSourceStatusUpdateType.StartEndTime)
                {
                    RunOnProtectedUIThread(
                        () =>
                        {
                            LogMessage("Smooth Streaming Time changed - Start " + (new TimeSpan(args.StartTime)).ToString() + " End: " + (new TimeSpan(Math.Max(args.EndTime, args.StartTime + args.AdaptiveSource.Manifest.Duration))).ToString() + " Live: " + (new TimeSpan(args.EndTime)).ToString() + " Position: " + mediaElement.Position.ToString());
                        });
                }

            }
        }
        #endregion

        #region DASH_HLS

        /// <summary>
        /// Called when the bitrate changed for DASH or HLS streams
        /// </summary>
        private void AdaptiveMediaSource_PlaybackBitrateChanged(Windows.Media.Streaming.Adaptive.AdaptiveMediaSource sender, Windows.Media.Streaming.Adaptive.AdaptiveMediaSourcePlaybackBitrateChangedEventArgs args)
        {
            LogMessage("PlaybackBitrateChanged from " + args.OldValue + " to " + args.NewValue);
        }
        /// <summary>
        /// Called when the download of a DASH or HLS chunk is requested
        /// </summary>

        private async void AdaptiveMediaSource_DownloadRequested(Windows.Media.Streaming.Adaptive.AdaptiveMediaSource sender, Windows.Media.Streaming.Adaptive.AdaptiveMediaSourceDownloadRequestedEventArgs args)
        {
//            LogMessage("DownloadRequested for uri: " + args.ResourceUri.ToString());
            
            var deferral = args.GetDeferral();
            if (deferral != null)
            {
                args.Result.ResourceUri = args.ResourceUri;
                args.Result.ContentType = args.ResourceType.ToString();
                var filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
                filter.CacheControl.WriteBehavior = Windows.Web.Http.Filters.HttpCacheWriteBehavior.NoCache;
                using (var httpClient = new Windows.Web.Http.HttpClient(filter))
                {
                    try
                    {
                        Windows.Web.Http.HttpRequestMessage request = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, args.Result.ResourceUri);
                        Windows.Web.Http.HttpResponseMessage response = await httpClient.SendRequestAsync(request);
                       // args.Result.ExtendedStatus = (uint)response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            //args.Result.ExtendedStatus = (uint)response.StatusCode;
                            args.Result.InputStream = await response.Content.ReadAsInputStreamAsync();

                        }
                        else
                            LogMessage("DownloadRequested for uri: " + args.ResourceUri.ToString() + " error: " + response.StatusCode.ToString());
                    }
                    catch (Exception e)
                    {
                        LogMessage("DownloadRequested for uri: " + args.ResourceUri.ToString() + " exception: " + e.Message);

                    }
//                    LogMessage("DownloadRequested for uri: " + args.ResourceUri.ToString() + " done");
                    deferral.Complete();
                }
            }
            
        }

        /// <summary>
        /// Called when the download of a DASH or HLS chunk failed
        /// </summary>
        private void AdaptiveMediaSource_DownloadFailed(Windows.Media.Streaming.Adaptive.AdaptiveMediaSource sender, Windows.Media.Streaming.Adaptive.AdaptiveMediaSourceDownloadFailedEventArgs args)
        {
            LogMessage("DownloadRequested failed for uri: " + args.ResourceUri.ToString());
        }

        /// <summary>
        /// Called when the download is completed for a DASH or HLS chunk
        /// </summary>
        private void AdaptiveMediaSource_DownloadCompleted(Windows.Media.Streaming.Adaptive.AdaptiveMediaSource sender, Windows.Media.Streaming.Adaptive.AdaptiveMediaSourceDownloadCompletedEventArgs args)
        {
           // LogMessage("DownloadRequested completed for uri: " + args.ResourceUri.ToString());
        }
        /// <summary>
        /// Called when the bitrate change for DASH or HLS 
        /// </summary>
        private void AdaptiveMediaSource_DownloadBitrateChanged(Windows.Media.Streaming.Adaptive.AdaptiveMediaSource sender, Windows.Media.Streaming.Adaptive.AdaptiveMediaSourceDownloadBitrateChangedEventArgs args)
        {
            LogMessage("DownloadBitrateChangedfrom " + args.OldValue + " to " + args.NewValue);

        }

        #endregion

        #region PLAYREADY
        // Dictionary used to store the KeyId associated with the video asset
        // This dictionary is used to retrieve the PlayReady license Expiration date from the keyId
        Dictionary<String, Guid> PlayReadyUrlKeyIdDictionary = new Dictionary<string, Guid>();
        Windows.Media.Protection.MediaProtectionManager protectionManager;
        private const int MSPR_E_CONTENT_ENABLING_ACTION_REQUIRED = -2147174251;
        public const int DRM_E_NOMORE_DATA = -2147024637; //( 0x80070103 )
        public const int MSPR_E_NEEDS_INDIVIDUALIZATION = -2147174366; // (0x8004B822)
        private string PlayReadyLicenseUrl;
        private string PlayReadyChallengeCustomData;
        /// <summary>
        /// HardwareDRMInitialized
        /// True if HardwareDRMSupported has been set following a call to MediaHelpers.PlayReadyHelper.IsHardwareDRMSupported()
        /// </summary>
        private bool HardwareDRMInitialized = false;
        /// <summary>
        /// True if hardware DRM is supported on the platform
        /// This variable is initialized when the ProtectionManager is initialized
        /// </summary>
        private bool HardwareDRMSupported = false;
        /// <summary>
        /// Return true if the Windows 10 platform does support Hardware DRM 
        /// </summary>
        bool IsHardwareDRMSupported()
        {
            if (HardwareDRMInitialized == false)
            {
                HardwareDRMSupported = MediaHelpers.PlayReadyHelper.IsHardwareDRMSupported();
                HardwareDRMInitialized = true;
            }
            return HardwareDRMSupported;
        }
        /// <summary>
        /// Return true if the application is configured to support Hardware DRM
        /// Return true if the Windows 10 platform does support Hardware DRM 
        /// </summary>
        bool IsHardwareDRMEnabled()
        {
            bool bResult = false;
            try
            {
                bResult = Windows.Media.Protection.PlayReady.PlayReadyStatics.CheckSupportedHardware(Windows.Media.Protection.PlayReady.PlayReadyHardwareDRMFeatures.HardwareDRM);
            }
            catch (Exception e)
            {
                LogMessage("Exception in IsHardwareDRMEnabled: " + e.Message);
            }
            return bResult;
        }
        /// <summary>
        /// Enable or Disable Software DRM
        /// Software DRM must be enabled when the MediaElement is playing VC1 protected content on platform supporting Hardware DRM
        /// </summary>
        bool EnableSoftwareDRM(bool bEnable)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            // Force Software DRM useful for VC1 content which doesn't support Hardware DRM
            try
            {
                if (!localSettings.Containers.ContainsKey("PlayReady"))
                    localSettings.CreateContainer("PlayReady", Windows.Storage.ApplicationDataCreateDisposition.Always);
                localSettings.Containers["PlayReady"].Values["SoftwareOverride"] = (bEnable == true ? 1 : 0);
            }
            catch (Exception e)
            {
                LogMessage("Exception while forcing software DRM: " + e.Message);
            }
            //Setup Software Override based on app setting
            //By default, PlayReady uses Hardware DRM if the machine support it. However, in case the app still want
            //software behavior, they can set localSettings.Containers["PlayReady"].Values["SoftwareOverride"]=1. 
            //This code tells MF to use software override as well
            if (localSettings.Containers.ContainsKey("PlayReady") &&
                localSettings.Containers["PlayReady"].Values.ContainsKey("SoftwareOverride"))
            {
                int UseSoftwareProtectionLayer = (int)localSettings.Containers["PlayReady"].Values["SoftwareOverride"];

                if (protectionManager.Properties.ContainsKey("Windows.Media.Protection.UseSoftwareProtectionLayer"))
                    protectionManager.Properties["Windows.Media.Protection.UseSoftwareProtectionLayer"] = (UseSoftwareProtectionLayer == 1 ? true : false);
                else
                    protectionManager.Properties.Add("Windows.Media.Protection.UseSoftwareProtectionLayer", (UseSoftwareProtectionLayer == 1 ? true : false));
            }
            return true;
        }
        /// <summary>
        /// Invoked when the Protection Manager can't load some components
        /// </summary>
        void ProtectionManager_ComponentLoadFailed(Windows.Media.Protection.MediaProtectionManager sender, Windows.Media.Protection.ComponentLoadFailedEventArgs e)
        {
            LogMessage("ProtectionManager ComponentLoadFailed");
            e.Completion.Complete(false);
        }
        /// <summary>
        /// Invoked to acquire the PlayReady License
        /// </summary>
        async System.Threading.Tasks.Task<bool> LicenseAcquisitionRequest(Windows.Media.Protection.PlayReady.PlayReadyLicenseAcquisitionServiceRequest licenseRequest, Windows.Media.Protection.MediaProtectionServiceCompletion CompletionNotifier, string Url, string ChallengeCustomData)
        {
            bool bResult = false;
            string ExceptionMessage = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(Url))
                {
                    LogMessage("ProtectionManager PlayReady Manual License Acquisition Service Request in progress - URL: " + Url);

                    if (!string.IsNullOrEmpty(ChallengeCustomData))
                    {
                        // disable Base64String encoding
                        //System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                        //byte[] b = encoding.GetBytes(ChallengeCustomData);
                        //licenseRequest.ChallengeCustomData = Convert.ToBase64String(b, 0, b.Length);
                        licenseRequest.ChallengeCustomData = ChallengeCustomData;
                    }

                    Windows.Media.Protection.PlayReady.PlayReadySoapMessage soapMessage = licenseRequest.GenerateManualEnablingChallenge();

                    byte[] messageBytes = soapMessage.GetMessageBody();
                    Windows.Web.Http.IHttpContent httpContent = new Windows.Web.Http.HttpBufferContent(messageBytes.AsBuffer());

                    IPropertySet propertySetHeaders = soapMessage.MessageHeaders;
                    foreach (string strHeaderName in propertySetHeaders.Keys)
                    {
                        string strHeaderValue = propertySetHeaders[strHeaderName].ToString();

                        // The Add method throws an ArgumentException try to set protected headers like "Content-Type"
                        // so set it via "ContentType" property
                        if (strHeaderName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                            httpContent.Headers.ContentType = Windows.Web.Http.Headers.HttpMediaTypeHeaderValue.Parse(strHeaderValue);
                        else
                            httpContent.Headers.TryAppendWithoutValidation(strHeaderName.ToString(), strHeaderValue);
                    }
                    CommonLicenseRequest licenseAcquision = new CommonLicenseRequest();
                    
                    Windows.Web.Http.IHttpContent responseHttpContent = await licenseAcquision.AcquireLicense(new Uri(Url), httpContent);
                    if (responseHttpContent != null)
                    {
                        //string res = await responseHttpContent.ReadAsStringAsync();
                        var buffer = await responseHttpContent.ReadAsBufferAsync();
                        Exception exResult = licenseRequest.ProcessManualEnablingResponse(buffer.ToArray());
                        if (exResult != null)
                        {
                            throw exResult;
                        }
                        bResult = true;
                    }
                    else
                        ExceptionMessage = licenseAcquision.GetLastErrorMessage();
                }
                else
                {
                    LogMessage("ProtectionManager PlayReady License Acquisition Service Request in progress - URL: " + licenseRequest.Uri.ToString());
                    await licenseRequest.BeginServiceRequest();
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                ExceptionMessage = e.Message;
            }

            if (bResult == true)
                LogMessage(!string.IsNullOrEmpty(Url) ? "ProtectionManager Manual PlayReady License Acquisition Service Request successful" :
                    "ProtectionManager PlayReady License Acquisition Service Request successful");
            else
                LogMessage(!string.IsNullOrEmpty(Url) ? "ProtectionManager Manual PlayReady License Acquisition Service Request failed: " + ExceptionMessage :
                    "ProtectionManager PlayReady License Acquisition Service Request failed: " + ExceptionMessage);
            if (CompletionNotifier != null)
                CompletionNotifier.Complete(bResult);
            return bResult;
        }
        /// <summary>
        /// Proactive Individualization Request 
        /// </summary>
        async System.Threading.Tasks.Task<bool> ProActiveIndivRequest()
        {
            Windows.Media.Protection.PlayReady.PlayReadyIndividualizationServiceRequest indivRequest = new Windows.Media.Protection.PlayReady.PlayReadyIndividualizationServiceRequest();
            bool bResultIndiv = await ReactiveIndivRequest(indivRequest, null);
            return bResultIndiv;

        }
        /// <summary>
        /// Invoked to send the Individualization Request 
        /// </summary>
        async System.Threading.Tasks.Task<bool> ReactiveIndivRequest(Windows.Media.Protection.PlayReady.PlayReadyIndividualizationServiceRequest IndivRequest, Windows.Media.Protection.MediaProtectionServiceCompletion CompletionNotifier)
        {
            bool bResult = false;
            Exception exception = null;
            LogMessage("ProtectionManager PlayReady Individualization Service Request in progress...");
            try
            {
                await IndivRequest.BeginServiceRequest();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (exception == null)
                {
                    bResult = true;
                }
                else
                {
                    System.Runtime.InteropServices.COMException comException = exception as System.Runtime.InteropServices.COMException;
                    if (comException != null && comException.HResult == MSPR_E_CONTENT_ENABLING_ACTION_REQUIRED)
                    {
                        IndivRequest.NextServiceRequest();
                    }
                }
            }
            if (bResult == true)
                LogMessage("ProtectionManager PlayReady Individualization Service Request successful");
            else
                LogMessage("ProtectionManager PlayReady Individualization Service Request failed");
            if (CompletionNotifier != null) CompletionNotifier.Complete(bResult);
            return bResult;

        }

        /// <summary>
        /// Invoked to send a PlayReady request (Individualization or License request)
        /// </summary>
        private async void ProtectionManager_ServiceRequested(Windows.Media.Protection.MediaProtectionManager sender, Windows.Media.Protection.ServiceRequestedEventArgs e)
        {
            LogMessage("ProtectionManager ServiceRequested - Current DRM Configuration: " + (IsHardwareDRMEnabled() ? "Hardware" : "Software"));
            if (e.Request is Windows.Media.Protection.PlayReady.PlayReadyIndividualizationServiceRequest)
            {
                Windows.Media.Protection.PlayReady.PlayReadyIndividualizationServiceRequest IndivRequest = e.Request as Windows.Media.Protection.PlayReady.PlayReadyIndividualizationServiceRequest;
                bool bResultIndiv = await ReactiveIndivRequest(IndivRequest, e.Completion);
            }
            else if (e.Request is Windows.Media.Protection.PlayReady.PlayReadyLicenseAcquisitionServiceRequest)
            {
                Windows.Media.Protection.PlayReady.PlayReadyLicenseAcquisitionServiceRequest licenseRequest = e.Request as Windows.Media.Protection.PlayReady.PlayReadyLicenseAcquisitionServiceRequest;
                bool result = await LicenseAcquisitionRequest(licenseRequest, e.Completion, PlayReadyLicenseUrl, PlayReadyChallengeCustomData);
                if (result == true)
                {
                    // Store the keyid of the current video
                    // if the user wants to retrieve subsequently the PlayReady license Expiration date
                    if (!PlayReadyUrlKeyIdDictionary.ContainsKey(CurrentMediaUrl))
                        PlayReadyUrlKeyIdDictionary.Add(CurrentMediaUrl, licenseRequest.ContentHeader.KeyId);
                    // Get the expiration date of PlayReady license
                    DateTime d = GetLicenseExpirationDate(licenseRequest.ContentHeader.KeyId);
                    if (d != DateTime.MinValue)
                    {

                        LogMessage("PlayReady license Expiration Date: " + d.ToString());
                    }
                }
            }
        }
        /// <summary>
        /// Retrieve the PlayReady license expiration date based onthe video KeyID
        /// This method uses the Windows Runtime library MediaHelpers to get the expiration date
        /// The use of this library is a turn around to a PlayReady issue with .Net Native.
        /// </summary>
        private DateTime GetLicenseExpirationDate(Guid videoId)
        {

            var keyIdString = Convert.ToBase64String(videoId.ToByteArray());
            try
            {
                var contentHeader = new Windows.Media.Protection.PlayReady.PlayReadyContentHeader(
                    videoId,
                    keyIdString,
                    Windows.Media.Protection.PlayReady.PlayReadyEncryptionAlgorithm.Aes128Ctr,
                    null,
                    null,
                    string.Empty,
                    new Guid());
                Windows.Media.Protection.PlayReady.IPlayReadyLicense[] licenses = new Windows.Media.Protection.PlayReady.PlayReadyLicenseIterable(contentHeader, true).ToArray();
                foreach (var lic in licenses)
                {
                    DateTimeOffset? d = MediaHelpers.PlayReadyHelper.GetLicenseExpirationDate(lic);
                    if ((d != null) && (d.HasValue))
                        return d.Value.DateTime;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("GetLicenseExpirationDate Exception: " + e.Message);
                return DateTime.MinValue;
            }
            return DateTime.MinValue;
        }
        #endregion

        #region Settings



        /// <summary>
        /// Function to read all the persistent attributes
        /// </summary>
        public async System.Threading.Tasks.Task<bool> ReadSettings()
        {
            string s = ReadSettingsValue(keyAutoSkip) as string;
            if (!string.IsNullOrEmpty(s))
                bool.TryParse(s, out bAutoSkip);

            s = ReadSettingsValue(keyMinBitRate) as string;
            if (!string.IsNullOrEmpty(s))
                uint.TryParse(s, out MinBitRate);

            s = ReadSettingsValue(keyMaxBitRate) as string;
            if (!string.IsNullOrEmpty(s))
                uint.TryParse(s, out MaxBitRate);

            // Restore PlayList path and index in the local settings
            s = ReadSettingsValue(keyMediaDataPath) as string;
            if (!string.IsNullOrEmpty(s))
            {
                LogMessage("MainPage Loading Data for path: " + s);
                if (await LoadingData(s) == true)
                {
                    s = ReadSettingsValue(keyMediaDataIndex) as string;
                    if (!string.IsNullOrEmpty(s))
                    {
                        int index;
                        if (int.TryParse(s, out index))
                        {

                            comboStream.SelectedIndex = index;
                            MediaItem ms = comboStream.SelectedItem as MediaItem;
                            if (ms != null)
                            {
                                mediaUri.Text = ms.Content;
                                PlayReadyLicenseUrl = ms.PlayReadyUrl;
                                PlayReadyChallengeCustomData = ms.PlayReadyCustomData;
                            }

                        }
                    }
                    s = ReadSettingsValue(keyMediaUri) as string;
                    if (!string.IsNullOrEmpty(s))
                    {
                        mediaUri.Text = s;
                    }
                }
                else
                {
                    await LoadingData(string.Empty);
                    comboStream.SelectedIndex = 0;
                    MediaItem ms = comboStream.SelectedItem as MediaItem;
                    if (ms != null)
                    {
                        mediaUri.Text = ms.Content;
                        PlayReadyLicenseUrl = ms.PlayReadyUrl;
                        PlayReadyChallengeCustomData = ms.PlayReadyCustomData;
                    }
                }
            }
            // Restore WindowState
            s = ReadSettingsValue(keyWindowState) as string;
            if (!string.IsNullOrEmpty(s))
            {
                int state;
                if (int.TryParse(s, out state))
                {
                    if (state == 0)
                        WindowState = WindowMediaState.WindowMode;
                    else if ((state == 1) && (bAutoSkip == true))
                        WindowState = WindowMediaState.FullWindow;
                    else if ((state == 2) && (bAutoSkip == true))
                        WindowState = WindowMediaState.FullScreen;
                    else
                        WindowState = WindowMediaState.WindowMode;

                    SetWindowMode(WindowState);
                }
            }
            return true;
        }

        /// <summary>
        /// Function to save all the persistent attributes
        /// </summary>
        public bool SaveSettings()
        {
            SaveSettingsValue(keyAutoSkip, bAutoSkip.ToString());
            SaveSettingsValue(keyMinBitRate, MinBitRate.ToString());
            SaveSettingsValue(keyMaxBitRate, MaxBitRate.ToString());

            // Save PlayList path and index in the local settings
            SaveSettingsValue(keyMediaDataPath, MediaDataSource.MediaDataPath);
            SaveSettingsValue(keyMediaDataIndex, comboStream.SelectedIndex.ToString());
            SaveSettingsValue(keyMediaUri, mediaUri.Text);

            // Save WindowState
            int state = (int)WindowState;
            SaveSettingsValue(keyWindowState, state.ToString());
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
        #endregion


        #region LegacyHLS
        // LegacyHLS Component

        Microsoft.HLSClient.HLSControllerFactory HLSControllerFactory;
        Microsoft.HLSClient.IHLSController HLSController;

        /// <summary>
        /// Check whether it's a HLS Url
        /// </summary>
        /// <param name="url">Url </param>
        bool IsHLSUrl(string url)
        {
            url = url.ToLower();
            if (url.EndsWith(".m3u8") == true)
                return true;
            else if (url.IndexOf("manifest(format=m3u8") > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Update HLS url and return an url associated with the HLS Plugin
        /// </summary>
        /// <param name="url">HLS Url </param>
        /// <return >New HLS Url </param>
        string UpdateHLSUrl(string url)
        {
            if (url.StartsWith("http:"))
                url = url.Replace("http:", "ms-hls:");
            else if (url.StartsWith("https:"))
                url = url.Replace("https:", "ms-hls-s:");
            return url;
        }



        /// <summary>
        /// Register Legacy HLS Plugin 
        /// </summary>
        void RegisterHLSPlugins(Windows.Media.MediaExtensionManager HLSMediaManager)
        {
            LogMessage("Register plugins");
            // HLS registration

            if (HLSControllerFactory != null)
            {
                HLSControllerFactory.HLSControllerReady -= ControllerFactory_HLSControllerReady;
                HLSControllerFactory = null;
            }
            HLSControllerFactory = new Microsoft.HLSClient.HLSControllerFactory();
            HLSControllerFactory.HLSControllerReady += ControllerFactory_HLSControllerReady;

            PropertySet hlsps = new PropertySet();
            hlsps.Add("MimeType", "application/x-mpegurl");
            hlsps.Add("ControllerFactory", HLSControllerFactory);

            HLSMediaManager.RegisterSchemeHandler("Microsoft.HLSClient.HLSPlaylistHandler", "ms-hls:", hlsps);
            HLSMediaManager.RegisterSchemeHandler("Microsoft.HLSClient.HLSPlaylistHandler", "ms-hls-s:", hlsps);
            HLSMediaManager.RegisterByteStreamHandler("Microsoft.HLSClient.HLSPlaylistHandler", ".m3u8", "application/x-mpegurl", hlsps);
            HLSMediaManager.RegisterByteStreamHandler("Microsoft.HLSClient.HLSPlaylistHandler", ".ism/manifest(format=m3u8-aapl)", "application/x-mpegurl", hlsps);



        }
        /// <summary>
        /// Unregister Legacy HLS Plugin 
        /// </summary>
        void UnregisterHLSPlugins()
        {
            LogMessage("Unregister plugins");

            if (HLSControllerFactory != null)
            {
                HLSControllerFactory.HLSControllerReady -= ControllerFactory_HLSControllerReady;
                HLSControllerFactory = null;
            }
        }

        /// <summary>
        /// HLSControllerReady event raised when the HLS Controller is ready 
        /// </summary>
        void ControllerFactory_HLSControllerReady(Microsoft.HLSClient.IHLSControllerFactory sender, Microsoft.HLSClient.IHLSController args)
        {
            if ((HLSController != null) && (HLSController.IsValid == true) && (HLSController.Playlist != null))
            {
                HLSController.Playlist.StreamSelectionChanged -= Playlist_StreamSelectionChanged;
                HLSController.Playlist.BitrateSwitchCompleted -= Playlist_BitrateSwitchCompleted;
                HLSController.Playlist.SlidingWindowChanged -= Playlist_SlidingWindowChanged;
                HLSController = null;
            }
            HLSController = args;
            if ((HLSController != null) && (HLSController.IsValid == true) && (HLSController.Playlist != null))
            {

                HLSController.EnableAdaptiveBitrateMonitor = true;

                HLSController.PrefetchDuration = TimeSpan.FromMilliseconds(6000);
                HLSController.MinimumBufferLength = TimeSpan.FromMilliseconds(30000);
                HLSController.MinimumLiveLatency = TimeSpan.FromMilliseconds(24000);

                HLSController.UseTimeAveragedNetworkMeasure = false;
                HLSController.BitrateChangeNotificationInterval = TimeSpan.FromMilliseconds(5000);
                HLSController.SegmentTryLimitOnBitrateSwitch = 2;

                HLSController.ForceKeyFrameMatchOnSeek = true;
                HLSController.AllowSegmentSkipOnSegmentFailure = true;
                HLSController.MaximumToleranceForBitrateDownshift = (20 / 100 <= 1 ? 20 / 100 : 0);
                HLSController.MinimumPaddingForBitrateUpshift = (30 / 100 <= 1 ? 30 / 100 : 0);

                HLSController.UseTimeAveragedNetworkMeasure = false;


                HLSController.Playlist.StreamSelectionChanged += Playlist_StreamSelectionChanged;
                HLSController.Playlist.BitrateSwitchCompleted += Playlist_BitrateSwitchCompleted;
                HLSController.Playlist.SlidingWindowChanged += Playlist_SlidingWindowChanged;
                LogMessage("HLS PlayList Ready for uri: " + HLSController.Playlist.Url.ToString());

                var Streams = HLSController.Playlist.GetVariantStreams();
                uint startupBitrate = 0;
                if (Streams != null)
                {
                    foreach (var track in Streams)
                    {
                        if ((startupBitrate == 0) &&
                            (track.Bitrate >= MinBitRate) &&
                            (track.Bitrate <= MaxBitRate))
                            startupBitrate = track.Bitrate;
                        if (track.HasResolution == true)
                            LogMessage("  Bitrate: " + track.Bitrate.ToString() + " Width: " + track.HorizontalResolution.ToString() + " Height: " + track.VerticalResolution.ToString());
                        else
                            LogMessage("  Bitrate: " + track.Bitrate.ToString());
                    }
                }
                if (HLSController.Playlist.IsMaster)
                {

                    // Set bitrate range for HLS 
                    if (startupBitrate > 0)
                    {
                        HLSController.Playlist.StartBitrate = startupBitrate;
                        LogMessage("  StartupBitrate: " + startupBitrate.ToString());
                    }
                    if (MaxBitRate != 0 && MaxBitRate >= HLSController.Playlist.GetVariantStreams().First().Bitrate)
                    {
                        HLSController.Playlist.MaximumAllowedBitrate = MaxBitRate;
                        LogMessage("  MaxBitrate: " + MaxBitRate.ToString());
                    }
                    if (MinBitRate != 0 && MinBitRate <= HLSController.Playlist.GetVariantStreams().Last().Bitrate)
                    {
                        HLSController.Playlist.MinimumAllowedBitrate = MinBitRate;
                        LogMessage("  MinBitrate: " + MinBitRate.ToString());
                    }

                }
            }

            /*
                        var settings = App.ViewModel.hlsSettings;
                        //Controller.MinimumBufferLength = settings.MinimumBufferLength;
                        Controller.EnableAdaptiveBitrateMonitor = settings.EnableAdaptiveBitrateMonitor;
                        //Controller.MinimumBufferLength = settings.MinimumBufferLength;
                        Controller.UseTimeAveragedNetworkMeasure = settings.UseTimeAveragedNetworkMeasure;
                        //          Controller.BitrateChangeNotificationInterval = settings.BitrateChangeNotificationInterval;
                        //Controller.KeyFrameMatchTryLimitOnBitrateSwitch = settings.KeyFrameMatchTryLimitOnBitrateSwitch;
                        Controller.SegmentTryLimitOnBitrateSwitch = settings.SegmentTryLimitOnBitrateSwitch;
                        //Controller.BitrateSwitchOnSegmentBoundaryOnly = settings.BitrateSwitchOnSegmentBoundaryOnly;
                        //Controller.AllowParallelDownloadsForBitrateSwitch = settings.AllowParallelDownloadsForBitrateSwitch;
                        Controller.Playlist.StreamSelectionChanged += Playlist_StreamSelectionChanged;
                        Controller.Playlist.SegmentSwitched += Playlist_SegmentSwitched;
                        Controller.Playlist.SegmentDataLoaded += Playlist_SegmentDataLoaded;
                        Controller.ForceKeyFrameMatchOnSeek = settings.ForceKeyframeMatchOnSeek;
                        Controller.AllowSegmentSkipOnSegmentFailure = settings.AllowSegmentSkipOnSegmentFailure;
                        //Controller.BitrateToleranceMarginInPercentage = settings.BitrateToleranceMarginInPercentage;
                        Controller.UseTimeAveragedNetworkMeasure = settings.UseTimeAveragedNetworkMeasure;
                        if (Controller.Playlist.IsMaster)
                        {
                            if (settings.StartBitrate != 0 && settings.StartBitrate * 1024 <= Controller.Playlist.GetVariantStreams().Last().Bitrate && settings.StartBitrate * 1024 >= Controller.Playlist.GetVariantStreams().First().Bitrate)
                                Controller.Playlist.StartBitrate = settings.StartBitrate * 1024;
                            if (settings.MaximumBitrate != 0 && settings.MaximumBitrate * 1024 >= Controller.Playlist.GetVariantStreams().First().Bitrate)
                                Controller.Playlist.MaximumAllowedBitrate = settings.MaximumBitrate * 1024;
                            if (settings.MinimumBitrate != 0 && settings.MinimumBitrate * 1024 <= Controller.Playlist.GetVariantStreams().Last().Bitrate)
                                Controller.Playlist.MinimumAllowedBitrate = settings.MinimumBitrate * 1024;
                        }
            */


        }
        /// <summary>
        /// RunOnProtectedUIThread method used to run action on the UI Thread 
        /// </summary>
        private async void RunOnProtectedUIThread(Action action)
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled && Dispatcher != null)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    action();
                });
            }
        }
        /// <summary>
        /// Playlist_SlidingWindowChanged event raised when the Sliding Window changed (for Live streams) 
        /// </summary>
        private void Playlist_SlidingWindowChanged(Microsoft.HLSClient.IHLSPlaylist sender, Microsoft.HLSClient.IHLSSlidingWindow args)
        {

            RunOnProtectedUIThread(
                () =>
                {
                    LogMessage("HLS Time changed - Start " + args.StartTimestamp.ToString() + " End: " + args.EndTimestamp.ToString() + " Live: " + args.LivePosition.ToString() + " Position: " + mediaElement.Position.ToString());
                });
        }
        /// <summary>
        /// Playlist_StreamSelectionChanged event raised when the Stream selection changed  
        /// </summary>

        void Playlist_StreamSelectionChanged(Microsoft.HLSClient.IHLSPlaylist sender, Microsoft.HLSClient.IHLSStreamSelectionChangedEventArgs args)
        {
            LogMessage("Stream Selection changed for uri: " + sender.Url.ToString());
        }

        /// <summary>
        /// Playlist_BitrateSwitchCompleted event raised when the bitrate changed 
        /// </summary>
        void Playlist_BitrateSwitchCompleted(Microsoft.HLSClient.IHLSPlaylist sender, Microsoft.HLSClient.IHLSBitrateSwitchEventArgs args)
        {
            LogMessage("Bitrate changed for uri: " + sender.Url.ToString());
            LogMessage(" New bitrate: " + args.ToBitrate.ToString());
        }

        #endregion

    }

}
