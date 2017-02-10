using System;
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
using TestUWPCognitiveServices.Information;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestUWPCognitiveServices
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Windows.Media.Playback.MediaPlayer mediaPlayer;
        string[] ArrayLanguage = 
            {"ca-ES","de-DE","zh-TW", "zh-HK","ru-RU","es-ES", "ja-JP","ar-EG", "da-DK","en-AU" ,"en-CA","en-GB" ,"en-IN", "en-US" , "en-NZ","es-MX","fi-FI",
              "fr-FR","fr-CA" ,"it-IT","ko-KR" , "nb-NO","nl-NL","pt-BR" ,"pt-PT"  ,             
              "pl-PL"  ,"sv-SE", "zh-CN"  };
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
            // Bind player to element
            mediaPlayer = new Windows.Media.Playback.MediaPlayer();
            mediaPlayerElement.SetMediaPlayer(mediaPlayer);
            language.Items.Clear();
            foreach(var l in ArrayLanguage)
                language.Items.Add(l);
            language.SelectedItem = "en-US";
            subscriptionID.Text = "5917b1caa4d04adc8e438a74240cfe4d";
            // Update control and play first video
            UpdateControls();


            // Display OS, Device information
            LogMessage(Information.SystemInformation.GetString());

        }

        public async System.Threading.Tasks.Task<string> GetToken(string authUrl, string subscriptionKey)
    {
        try
        {
                //Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                            System.Net.Http.HttpClient hc = new System.Net.Http.HttpClient();
                //hc.DefaultRequestHeaders.TryAppendWithoutValidation("Ocp-Apim-Subscription-Key", subscriptionKey);
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", subscriptionKey);
                //Windows.Web.Http.HttpStringContent content = new Windows.Web.Http.HttpStringContent(String.Empty);
                               System.Net.Http.HttpContent content = new System.Net.Http.StringContent(string.Empty);
               // Windows.Web.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(authUrl), content);
                System.Net.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(authUrl), content);
            if (hrm != null)
            {
                switch (hrm.StatusCode)
                {
//                        case Windows.Web.Http.HttpStatusCode.Ok:
                        case System.Net.HttpStatusCode.OK:
                        //IEnumerable<string> result;
                        string result = await hrm.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(result))
                        //                            if (hrm.Headers.TryGetValues("Authorization",out result)==true)
                        {
                            string base64Token = "Bearer  " + result;
                            return base64Token;
                        }
                        break;

                    default:
                        break;
                }
            }
        }
        catch (Exception e)
        {
        }
        return string.Empty;
    }
    public async System.Threading.Tasks.Task<string> SpeechToText(string token, string wavFile, string locale)
    {

        //            POST / recognize ? scenarios = catsearch & appid = f84e364c - ec34 - 4773 - a783 - 73707bd9a585 & locale = en - US & device.os = wp7 & version = 3.0 & format = xml & requestid = 1d4b6030 - 9099 - 11e0 - 91e4 - 0800200c9a66 & instanceid = 1d4b6030 - 9099 - 11e0 - 91e4 - 0800200c9a66 HTTP/ 1.1
        //Host: speech.platform.bing.com
        //Content - Type: audio / wav; samplerate = 16000
        //Authorization: Bearer[Base64 access_token]
        try
        {
            string os = "Windows";
            string deviceid = "b2c95ede-97eb-4c88-81e4-80f32d6aee54";
            string speechUrl = "https://speech.platform.bing.com/recognize?scenarios=catsearch&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5&version=3.0&device.os=" + os + "&locale=" + locale + "&format=json&requestid=" + Guid.NewGuid().ToString() + "&instanceid=" + deviceid + "&result.profanitymarkup=1&maxnbest=4";
            System.Net.Http.HttpClient hc = new System.Net.Http.HttpClient();

            hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
            hc.DefaultRequestHeaders.TryAddWithoutValidation("ContentType", "audio/wav; samplerate=16000");
                System.Net.Http.HttpResponseMessage hrm = null;
                System.Net.Http.HttpContent content;
                Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(wavFile);
                if (file != null)
                {
//                    System.IO.FileStream fileStream = new System.IO.FileStream(wavFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        content = new System.Net.Http.StreamContent(fileStream.AsStream());
                        hrm = await hc.PostAsync(new Uri(speechUrl), content);
                    }
                }

                
//            System.Net.Http.HttpContent content = new System.Net.Http.StreamContent(fileStream);
//            System.Net.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(speechUrl), content);
            if (hrm != null)
            {
                switch (hrm.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        //IEnumerable<string> result;
                        string result = await hrm.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(result))


                            //                            {\"version\":\"3.0\",
                            //\"header\":{\"status\":\"success\",\"scenario\":\"catsearch\",\"name\":\"bing what's the weather like\",\"lexical\":\"bing what's the weather like\",
                            //          \"properties\":
                            //               {\"requestid\":\"dd2f0028-b001-4d23-94e3-24fd6521da35\",\"HIGHCONF\":\"1\"}
                            //           },
                            //\"results\":
                            //[{
                            //\"scenario\":\"catsearch\",
                            //\"name\":\"bing what's the weather like\",
                            //\"lexical\":\"bing what's the weather like\",
                            //\"confidence\":\"0.879686\",
                            //\"properties\":{\"HIGHCONF\":\"1\"}
                            //}]
                            //}
                            //     

                 //           "{\"version\":\"3.0\",\"header\":{\"status\":\"success\",\"scenario\":\"catsearch\",\"name\":\"bing what's the weather like\",\"lexical\":\"bing what's the weather like\",\"properties\":{\"requestid\":\"f99c9963-ec5f-4168-bcd2-e4e18ebe5113\",\"HIGHCONF\":\"1\"}},\"results\":[{\"scenario\":\"catsearch\",\"name\":\"bing what's the weather like\",\"lexical\":\"bing what's the weather like\",\"confidence\":\"0.879686\",\"properties\":{\"HIGHCONF\":\"1\"}}]}""
                            //                            if (hrm.Headers.TryGetValues("Authorization",out result)==true)
                            {
                            //   Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(result);
                            string base64Token = "toto";
                            return base64Token;

                        }
                        break;

                    default:
                        break;
                }
            }
        }
        catch (Exception e)
        {
        }
        return string.Empty;
    }
        public async System.Threading.Tasks.Task<Response> StorageFileSpeechToText(string token, Windows.Storage.StorageFile wavFile, string locale)
        {

            //            POST / recognize ? scenarios = catsearch & appid = f84e364c - ec34 - 4773 - a783 - 73707bd9a585 & locale = en - US & device.os = wp7 & version = 3.0 & format = xml & requestid = 1d4b6030 - 9099 - 11e0 - 91e4 - 0800200c9a66 & instanceid = 1d4b6030 - 9099 - 11e0 - 91e4 - 0800200c9a66 HTTP/ 1.1
            //Host: speech.platform.bing.com
            //Content - Type: audio / wav; samplerate = 16000
            //Authorization: Bearer[Base64 access_token]
            try
            {
                string os = "Windows";
                string deviceid = "b2c95ede-97eb-4c88-81e4-80f32d6aee54";
                string speechUrl = "https://speech.platform.bing.com/recognize?scenarios=ulm&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5&version=3.0&device.os=" + os + "&locale=" + locale + "&format=json&requestid=" + Guid.NewGuid().ToString() + "&instanceid=" + deviceid + "&result.profanitymarkup=1&maxnbest=3";
                System.Net.Http.HttpClient hc = new System.Net.Http.HttpClient();

                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
                hc.DefaultRequestHeaders.TryAddWithoutValidation("ContentType", "audio/wav; samplerate=16000");
                System.Net.Http.HttpResponseMessage hrm = null;
                System.Net.Http.HttpContent content;
                Windows.Storage.StorageFile file = wavFile;
                if (file != null)
                {
                    //                    System.IO.FileStream fileStream = new System.IO.FileStream(wavFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        content = new System.Net.Http.StreamContent(fileStream.AsStream());
                        hrm = await hc.PostAsync(new Uri(speechUrl), content);
                    }
                }


                //            System.Net.Http.HttpContent content = new System.Net.Http.StreamContent(fileStream);
                //            System.Net.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(speechUrl), content);
                if (hrm != null)
                {
                    switch (hrm.StatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:
                            //IEnumerable<string> result;
                            string result = await hrm.Content.ReadAsStringAsync();
                            if (!string.IsNullOrEmpty(result))


                            //                            {\"version\":\"3.0\",
                            //\"header\":{\"status\":\"success\",\"scenario\":\"catsearch\",\"name\":\"bing what's the weather like\",\"lexical\":\"bing what's the weather like\",
                            //          \"properties\":
                            //               {\"requestid\":\"dd2f0028-b001-4d23-94e3-24fd6521da35\",\"HIGHCONF\":\"1\"}
                            //           },
                            //\"results\":
                            //[{
                            //\"scenario\":\"catsearch\",
                            //\"name\":\"bing what's the weather like\",
                            //\"lexical\":\"bing what's the weather like\",
                            //\"confidence\":\"0.879686\",
                            //\"properties\":{\"HIGHCONF\":\"1\"}
                            //}]
                            //}
                            //     

                            //           "{\"version\":\"3.0\",\"header\":{\"status\":\"success\",\"scenario\":\"catsearch\",\"name\":\"bing what's the weather like\",\"lexical\":\"bing what's the weather like\",\"properties\":{\"requestid\":\"f99c9963-ec5f-4168-bcd2-e4e18ebe5113\",\"HIGHCONF\":\"1\"}},\"results\":[{\"scenario\":\"catsearch\",\"name\":\"bing what's the weather like\",\"lexical\":\"bing what's the weather like\",\"confidence\":\"0.879686\",\"properties\":{\"HIGHCONF\":\"1\"}}]}""
                            //                            if (hrm.Headers.TryGetValues("Authorization",out result)==true)
                            {
                                //dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                                Response r = new Response(result);                                
                                return r;
                                //Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(result);
                                //System.Diagnostics.Debug.WriteLine("Result: " + obj.ToString());
                                //if (obj != null)
                                //{
                                //    Response r = new Response(result);
                                //    Newtonsoft.Json.Linq.JToken res = obj.GetValue("results");
                                //    foreach(var r in res)
                                //    {
                                //        if (r.Count() == 1)
                                //        {
                                //          //  r.Value[""];
                                //        }
                                //    }
                                //    string base64Token = "toto";
                                //    return base64Token;
                                //}

                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
            }
            return null;
        }
        public MainPage()
        {
            this.InitializeComponent();
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


        /// <summary>
        /// UpdateControls Method which update the controls on the page  
        /// </summary>
        async void UpdateControls()
        {

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                 () =>
                 {
                     {
                         openButton.IsEnabled = true;
                         convertAudioButton.IsEnabled = true;
                         stopConvertAudioButton.IsEnabled = true;
                         mediaUri.IsEnabled = true;

                         if (!string.IsNullOrEmpty(mediaUri.Text))
                         {
                             convertWAVButton.IsEnabled = true;

                             playButton.IsEnabled = true;


                             muteButton.IsEnabled = true;
                             volumeDownButton.IsEnabled = true;
                             volumeUpButton.IsEnabled = true;

                             playPauseButton.IsEnabled = false;
                             pausePlayButton.IsEnabled = false;
                             stopButton.IsEnabled = false;


                            if (mediaPlayer.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
                            {
                                //if (string.Equals(mediaUri.Text, CurrentMediaUrl))
                                //{
                                playPauseButton.IsEnabled = false;
                                pausePlayButton.IsEnabled = true;
                                stopButton.IsEnabled = true;
                                //}
                            }
                            else if (mediaPlayer.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Paused)
                            {
                                playPauseButton.IsEnabled = true;
                                stopButton.IsEnabled = true;
                            }
                        }
                        // Volume buttons control
                        if (mediaPlayer.IsMuted)
                            muteButton.Content = "\xE767";
                        else
                            muteButton.Content = "\xE74F";
                        if (mediaPlayer.Volume == 0)
                        {
                            volumeDownButton.IsEnabled = false;
                            volumeUpButton.IsEnabled = true;
                        }
                        else if (mediaPlayer.Volume >= 1)
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
                 });
        }


        /// <summary>
        /// Mute method 
        /// </summary>
        private void mute_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Toggle Mute");
            mediaPlayer.IsMuted = !mediaPlayer.IsMuted;
            UpdateControls();
        }
        /// <summary>
        /// Volume Up method 
        /// </summary>
        private void volumeUp_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Volume Up");
            mediaPlayer.Volume = (mediaPlayer.Volume + 0.10 <= 1 ? mediaPlayer.Volume + 0.10 : 1);
            UpdateControls();
        }
        /// <summary>
        /// Volume Down method 
        /// </summary>
        private void volumeDown_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Volume Down");
            mediaPlayer.Volume = (mediaPlayer.Volume - 0.10 >= 0 ? mediaPlayer.Volume - 0.10 : 0);
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

        /// <summary>
        /// StartPlay
        /// Start to play pictures, audio content or video content
        /// </summary>
        /// <param name="content">Url string of the content to play </param>
        /// <returns>true if success</returns>
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
                LogMessage("Start to play: " + content);
                // Stop the current stream
                mediaPlayer.Source = null;
                mediaPlayerElement.PosterSource = null;
                mediaPlayer.AutoPlay = true;
                // if a picture will be displayed
                // display or not popup
                if (result == true)
                {
                    pictureElement.Visibility = Visibility.Visible;
                    mediaPlayerElement.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pictureElement.Visibility = Visibility.Collapsed;
                    mediaPlayerElement.Visibility = Visibility.Visible;
                }
                // Audio or video
                Windows.Storage.StorageFile file = await GetFileFromLocalPathUrl(content);
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

        /// <summary>
        /// This method prepare the MediaElement to play any content (video, audio, pictures): SMOOTH, DASH, HLS, MP4, WMV, MPEG2-TS, JPG, PNG,...
        /// </summary>
        private async void PlayCurrentUrl()
        {

            await StartPlay(mediaUri.Text);
            UpdateControls();
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
                if (!string.IsNullOrEmpty(mediaUri.Text)) 
                {
                    LogMessage("Stop " + mediaUri.Text.ToString());
                    //          mediaPlayer.Stop();
                    mediaPlayer.Source = null;
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
                if (!string.IsNullOrEmpty(mediaUri.Text))
                {
                    LogMessage("Pause/Play " + mediaUri.Text.ToString());
                    mediaPlayer.Play();
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
                if (!string.IsNullOrEmpty(mediaUri.Text))
                {
                    LogMessage("Play " + mediaUri.Text.ToString());
                    mediaPlayer.Pause();
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to play: " + mediaUri.Text + " Exception: " + ex.Message);
            }
        }
        /// <summary>
        /// convertAudio method which plays the video with the MediaElement from position 0
        /// </summary>
        private void convertAudio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogMessage("Failed to convertAudio: Exception: " + ex.Message);
            }
        }
        /// <summary>
        /// stopConvertAudio method which plays the video with the MediaElement from position 0
        /// </summary>
        private void stopConvertAudio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogMessage("Failed to stopConvertAudio: Exception: " + ex.Message);
            }
        }
        /// <summary>
        /// open method which select WAV file on disk
        /// </summary>
        private async void open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
                filePicker.FileTypeFilter.Add(".wav");
                filePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
                filePicker.SettingsIdentifier = "WavPicker";
                filePicker.CommitButtonText = "Open WAV File to Process";

                var wavFile = await filePicker.PickSingleFileAsync();
                if (wavFile != null)
                {
                    string fileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(wavFile);
                    mediaUri.Text = "file://" + wavFile.Path;
                    LogMessage("Selected file: " + mediaUri.Text);
                    UpdateControls();
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to select WAV file: Exception: " + ex.Message);
            }
        }
        /// <summary>
        /// convertWAV method which plays the video with the MediaElement from position 0
        /// </summary>
        private async void convertWAV_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Converting file: " + mediaUri.Text + " into text");
            Response result = null;
            try
            {
                resultText.Text = string.Empty;
                if (!string.IsNullOrEmpty(mediaUri.Text))
                {
                    string authUrl = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
                    string subscriptionKey = subscriptionID.Text;
                    string token = await GetToken(authUrl, subscriptionKey);
                    if (!string.IsNullOrEmpty(token))
                    {
                        string locale = language.SelectedItem.ToString();
                        var file = await GetFileFromLocalPathUrl(mediaUri.Text);
                        if (file != null)
                        {
                            string convertedText = string.Empty;
                            result = await StorageFileSpeechToText(token, file, locale);
                            resultText.Text = result.Result();
                            LogMessage("Result: " + result.ToString());
                        }
                    }
                    UpdateControls();
                }

            }
            catch (Exception ex)
            {
                LogMessage("Failed to convertAudio: Exception: " + ex.Message);
            }
        }
    }
}
