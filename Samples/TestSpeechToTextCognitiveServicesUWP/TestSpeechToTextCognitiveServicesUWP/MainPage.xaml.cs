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
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestSpeechToTextCognitiveServicesUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Windows.Media.Playback.MediaPlayer mediaPlayer;
        string[] LanguageArray = 
            {"ca-ES","de-DE","zh-TW", "zh-HK","ru-RU","es-ES", "ja-JP","ar-EG", "da-DK","en-AU" ,"en-CA","en-GB" ,"en-IN", "en-US" , "en-NZ","es-MX","fi-FI",
              "fr-FR","fr-CA" ,"it-IT","ko-KR" , "nb-NO","nl-NL","pt-BR" ,"pt-PT"  ,             
              "pl-PL"  ,"sv-SE", "zh-CN"  };
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
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

            // Logs event to refresh the TextBox
            logs.TextChanged += Logs_TextChanged;

            // Bind player to element
            mediaPlayer = new Windows.Media.Playback.MediaPlayer();
            mediaPlayerElement.SetMediaPlayer(mediaPlayer);
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;

            language.Items.Clear();
            foreach(var l in LanguageArray)
                language.Items.Add(l);
            language.SelectedItem = "en-US";

            // Get Subscription ID from the local settings
            subscriptionKey.Text = GetSavedSubscriptionKey();
            
            // Update control and play first video
            UpdateControls();
            convertAudioButton.Focus(FocusState.Programmatic);

            // Display OS, Device information
            LogMessage(Information.SystemInformation.GetString());

        }
#region Settings
        private string GetSavedSubscriptionKey()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object value = localSettings.Values["subscriptionKey"];
            string s = string.Empty;
            if (value != null)
                s = (string)value;
            return s;
        }
        private void SaveSubscriptionKey(string ID)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["subscriptionKey"] = ID;
        }
#endregion Settings

        public async System.Threading.Tasks.Task<string> GetToken(string authUrl, string subscriptionKey)
        {
        try
        {
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                //            Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                hc.DefaultRequestHeaders.TryAppendWithoutValidation("Ocp-Apim-Subscription-Key", subscriptionKey);
                //hc.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", subscriptionKey);
                Windows.Web.Http.HttpStringContent content = new Windows.Web.Http.HttpStringContent(String.Empty);
                //               Windows.Web.Http.HttpContent content = new Windows.Web.Http.StringContent(string.Empty);
                Windows.Web.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(authUrl), content);
               // Windows.Web.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(authUrl), content);
            if (hrm != null)
            {
                switch (hrm.StatusCode)
                {
                        case Windows.Web.Http.HttpStatusCode.Ok:
                 //       case Windows.Web.HttpStatusCode.OK:
                        //IEnumerable<string> result;
                        var b = await hrm.Content.ReadAsBufferAsync();
                        string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
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

        public async System.Threading.Tasks.Task<SpeechToTextResponse> StreamSpeechToText(string token, SpeechToTextStream stream , string locale)
        {

            try
            {
                string os = "Windows";
                string deviceid = "b2c95ede-97eb-4c88-81e4-80f32d6aee54";
                string speechUrl = "https://speech.platform.bing.com/recognize?scenarios=ulm&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5&version=3.0&device.os=" + os + "&locale=" + locale + "&format=json&requestid=" + Guid.NewGuid().ToString() + "&instanceid=" + deviceid + "&result.profanitymarkup=1&maxnbest=3";
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                System.Threading.CancellationTokenSource cts =  new System.Threading.CancellationTokenSource();
                hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", token);
                Windows.Web.Http.HttpResponseMessage hrm = null;
                Windows.Web.Http.HttpStreamContent content;
                if (stream != null)
                {
                    content = new Windows.Web.Http.HttpStreamContent(stream.AsStream().AsInputStream());
                    content.Headers.ContentLength = stream.GetLength();
                    LogMessage("REST API Post Content Length: " + content.Headers.ContentLength.ToString());
                    content.Headers.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                    IProgress<Windows.Web.Http.HttpProgress> progress = new Progress<Windows.Web.Http.HttpProgress>(ProgressHandler);
                    hrm = await hc.PostAsync(new Uri(speechUrl), content).AsTask(cts.Token, progress);
                    
                }


                if (hrm != null)
                {
                    switch (hrm.StatusCode)
                    {
                        case Windows.Web.Http.HttpStatusCode.Ok:
                            //IEnumerable<string> result;
                            var b = await hrm.Content.ReadAsBufferAsync();
                            string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                            if (!string.IsNullOrEmpty(result))
                            {
                                SpeechToTextResponse r = new SpeechToTextResponse(result);
                                return r;
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                LogMessage("http POST canceled");
            }
            catch (Exception ex)
            {
                LogMessage("http POST exception: " + ex.Message);
            }
            finally
            {
                //LogMessage("http POST done" );
            }
            return null;
        }
        private void ProgressHandler(Windows.Web.Http.HttpProgress progress)
        {
            //LogMessage("Http progress: " + progress.Stage.ToString() + " " + progress.BytesSent.ToString() + "/" + progress.TotalBytesToSend.ToString());
        }

        public async System.Threading.Tasks.Task<SpeechToTextResponse> StorageFileSpeechToText(string token, Windows.Storage.StorageFile wavFile, string locale)
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
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();

                hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", token);
                hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                Windows.Web.Http.HttpResponseMessage hrm = null;

                Windows.Storage.StorageFile file = wavFile;
                if (file != null)
                {
                    //                    System.IO.FileStream fileStream = new System.IO.FileStream(wavFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        AudioStream = SpeechToTextStream.Create();
                        byte[] byteArray = new byte[fileStream.Size];
                        fileStream.ReadAsync(byteArray.AsBuffer(), (uint)fileStream.Size, Windows.Storage.Streams.InputStreamOptions.Partial).AsTask().Wait();
                        AudioStream.WriteAsync(byteArray.AsBuffer()).AsTask().Wait();

                        Windows.Web.Http.HttpStreamContent content = new Windows.Web.Http.HttpStreamContent(AudioStream.AsStream().AsInputStream());
                        content.Headers.ContentLength = AudioStream.GetLength();
                        LogMessage("REST API Post Content Length: " + content.Headers.ContentLength.ToString() + " bytes");
                        hrm = await hc.PostAsync(new Uri(speechUrl), content);
                    }
                }


                //            Windows.Web.Http.HttpContent content = new Windows.Web.Http.StreamContent(fileStream);
                //            Windows.Web.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(speechUrl), content);
                if (hrm != null)
                {
                    switch (hrm.StatusCode)
                    {
                        case Windows.Web.Http.HttpStatusCode.Ok:
                            //IEnumerable<string> result;
                            
                            //                            string result = await hrm.Content.ReadAsStringAsync();
                            var b = await hrm.Content.ReadAsBufferAsync();
                            string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
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
                                SpeechToTextResponse r = new SpeechToTextResponse(result);                                
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
                         if (_isRecording)
                         {
                             convertAudioButton.Content = "\xE778";
                             recordAudioButton.Content = "\xE78C";
                         }
                         else
                         {
                             convertAudioButton.Content = "\xE717";
                             recordAudioButton.Content = "\xE720";
                         }
                         openButton.IsEnabled = true;
                         convertAudioButton.IsEnabled = true;
                         recordAudioButton.IsEnabled = true;
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

        #region Media
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

        private void MediaPlayer_MediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            LogMessage("MediaPlayer media opened event");
            UpdateControls();

        }

        private void MediaPlayer_MediaFailed(Windows.Media.Playback.MediaPlayer sender, Windows.Media.Playback.MediaPlayerFailedEventArgs args)
        {
            LogMessage("MediaPlayer media failed event");
            UpdateControls();
        }

        private void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            LogMessage("MediaPlayer media ended event" );
            UpdateControls();
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
                LogMessage("Start to play " + mediaUri.Text.ToString());
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
                    UpdateControls();

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
                    LogMessage("Play " + mediaUri.Text.ToString());
                    mediaPlayer.Play();
                    UpdateControls();

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
                    LogMessage("Pause " + mediaUri.Text.ToString());
                    mediaPlayer.Pause();
                    UpdateControls();
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to play: " + mediaUri.Text + " Exception: " + ex.Message);
            }
        }

#endregion Media
        
         
        Windows.Media.Capture.MediaCapture mediaCapture;
        private SpeechToTextStream AudioStream;
        private async System.Threading.Tasks.Task<bool> StopRecording()
        {
            // Stop recording and dispose resources
            if (mediaCapture != null)
            {
                await mediaCapture.StopRecordAsync();
            }
            return true;
        }
        DateTime LastAmplitudeTime = DateTime.Now;
        double maxValue = 0;
        async void AudioStream_AmplitudeReading(object sender, double reading)
        {
            if ((DateTime.Now - LastAmplitudeTime).TotalMilliseconds > 200)
            {
                LastAmplitudeTime = DateTime.Now;
                if (maxValue == 0)
                {
                    maxValue = 1;
                    return;
                }
                //LogMessage("Amplitude: " + reading.ToString());

                double value = reading > 32768 ? 32768 : reading;
                if (value > maxValue)
                    maxValue = value;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        DrawLevel(value);
                    });
            }
        }
        void DrawLevel(double value)
        {
            if (CanvasGraph.Children.Count > 0) CanvasGraph.Children.Clear();
            Windows.UI.Xaml.Shapes.Line line = new Windows.UI.Xaml.Shapes.Line() { X1 = 0, Y1 = CanvasGraph.Height / 2, X2 = ((value * CanvasGraph.Width) / maxValue), Y2 = CanvasGraph.Height / 2 };
            line.StrokeThickness = CanvasGraph.Height;
            line.Stroke = new SolidColorBrush(Windows.UI.Colors.Cyan);
            CanvasGraph.Children.Add(line);
        }
        void ClearLevel()
        {
            if (CanvasGraph.Children.Count > 0) CanvasGraph.Children.Clear();
        }
        async void mediaCapture_Failed(Windows.Media.Capture.MediaCapture sender, Windows.Media.Capture.MediaCaptureFailedEventArgs errorEventArgs)
        {
            LogMessage("Fatal Error " + errorEventArgs.Message);
            await StopRecording();
            ClearLevel();
        }

        async void mediaCapture_RecordLimitationExceeded(Windows.Media.Capture.MediaCapture sender)
        {
            LogMessage("Stopping Record on exceeding max record duration");
            await StopRecording();
            ClearLevel();
            LogMessage("Record stopped");
        }
        private async System.Threading.Tasks.Task<bool> StartRecording(SpeechToTextStream stream)
        {
            bool bResult = false;
            if ((stream != null) && (_isInitialized == true))
            {
                try
                {
                    Windows.Media.MediaProperties.MediaEncodingProfile MEP = Windows.Media.MediaProperties.MediaEncodingProfile.CreateWav(Windows.Media.MediaProperties.AudioEncodingQuality.Auto);
                    if (MEP != null)
                    {
                        if (MEP.Audio != null)
                        {
                            uint framerate = 16000;
                            uint bitsPerSample = 16;
                            uint numChannels = 1;
                            uint bytespersecond = 32000;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_SAMPLES_PER_SECOND] = framerate;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_NUM_CHANNELS] = numChannels;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_BITS_PER_SAMPLE] = bitsPerSample;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_AVG_BYTES_PER_SECOND] = bytespersecond;
                            foreach (var Property in MEP.Audio.Properties)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: " + Property.Key.ToString());
                                System.Diagnostics.Debug.WriteLine("Value: " + Property.Value.ToString());
                                if (Property.Key == new Guid("5faeeae7-0290-4c31-9e8a-c534f68d9dba"))
                                    framerate = (uint)Property.Value;
                                if (Property.Key == new Guid("f2deb57f-40fa-4764-aa33-ed4f2d1ff669"))
                                    bitsPerSample = (uint)Property.Value;
                                if (Property.Key == new Guid("37e48bf5-645e-4c5b-89de-ada9e29b696a"))
                                    numChannels = (uint)Property.Value;

                            }
                        }
                        if (MEP.Container != null)
                        {
                            foreach (var Property in MEP.Container.Properties)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: " + Property.Key.ToString());
                                System.Diagnostics.Debug.WriteLine("Value: " + Property.Value.ToString());
                            }
                        }
                    }
                    await mediaCapture.StartRecordToStreamAsync(MEP, AudioStream);
                    bResult = true;
                    LogMessage("Recording in audio stream...");
                }
                catch(Exception e)
                {
                    LogMessage("Exception while recording in audio stream:" + e.Message);
                }
            }
            return bResult;
        }
        private async System.Threading.Tasks.Task<bool> InitializeRecording()
        {
            _isInitialized = false;
            try
            {
                // Initialize MediaCapture
                mediaCapture = new Windows.Media.Capture.MediaCapture();

                await mediaCapture.InitializeAsync(new Windows.Media.Capture.MediaCaptureInitializationSettings
                {
                    //VideoSource = screenCapture.VideoSource,
                    //      AudioSource = screenCapture.AudioSource,
                    StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio,
                    MediaCategory = Windows.Media.Capture.MediaCategory.Other,
                    AudioProcessing = Windows.Media.AudioProcessing.Raw

                });
                mediaCapture.RecordLimitationExceeded += mediaCapture_RecordLimitationExceeded;
                mediaCapture.Failed += mediaCapture_Failed;
                LogMessage("Device Initialized Successfully...");
                _isInitialized = true;
            }
            catch(Exception e)
            {
                LogMessage("Exception while initializing the device: " + e.Message);
            }

            return _isInitialized;
        }
        /// <summary>
        /// Cleans up the camera resources (after stopping any video recording and/or preview if necessary) and unregisters from MediaCapture events
        /// </summary>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<bool> CleanupRecording()
        {
            if (_isInitialized)
            {
                // If a recording is in progress during cleanup, stop it to save the recording
                if (_isRecording)
                {
                    await StopRecording();
                    ClearLevel();
                }
                _isInitialized = false;
            }

            if (mediaCapture != null)
            {
                mediaCapture.RecordLimitationExceeded -= mediaCapture_RecordLimitationExceeded;
                mediaCapture.Failed -= mediaCapture_Failed;
                mediaCapture.Dispose();
                mediaCapture = null;
            }
            if(AudioStream != null)
            {
                AudioStream.AmplitudeReading -= AudioStream_AmplitudeReading;
                AudioStream.Dispose();
                AudioStream = null;
            }
            return true;
        }
        /// <summary>
        /// convertAudio method which plays the video with the MediaElement from position 0
        /// </summary>
        private async void convertAudio_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecording == false)
            {
                try
                {
                    await CleanupRecording();
                    AudioStream = SpeechToTextStream.Create();
                    AudioStream.AmplitudeReading += AudioStream_AmplitudeReading;

                    if (await InitializeRecording() == true)
                    {
                        LogMessage("Recording initialized...");
                        if (await StartRecording(AudioStream) == true)
                        {
                            LogMessage("Start Recording...");
                            _isRecording = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Failed to record Audio: Exception: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    LogMessage("Stop Recording...");
                    await StopRecording();
                    _isRecording = false;
                    ClearLevel();
                    UpdateControls();
                    if (AudioStream != null)
                    {
                        string authUrl = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
                        string Key = subscriptionKey.Text;
                        if (!string.IsNullOrEmpty(Key))
                        {
                            string token = await GetToken(authUrl, Key);
                            if (!string.IsNullOrEmpty(token))
                            {
                                if (!subscriptionKey.Text.Equals(GetSavedSubscriptionKey()))
                                    SaveSubscriptionKey(subscriptionKey.Text);
                                string locale = language.SelectedItem.ToString();
                                string convertedText = string.Empty;
                                SpeechToTextResponse result = await StreamSpeechToText(token, AudioStream, locale);
                                if (result != null)
                                {
                                    if (result.Status() == "error")
                                        resultText.Text = "error";
                                    else
                                        resultText.Text = result.Result();
                                    LogMessage("Result: " + result.ToString());
                                }
                            }
                        }
                        else
                            LogMessage("Subscription Key missing");
                        UpdateControls();
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Failed to send Audio: Exception: " + ex.Message);
                }
            }
            UpdateControls();

        }
        bool _isInitialized = false;
        bool _isRecording = false;
        /// <summary>
        /// recordAudio method which plays the video with the MediaElement from position 0
        /// </summary>
        private async void recordAudio_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecording == false)
            {
                try
                {
                    AudioStream = SpeechToTextStream.Create();
                    AudioStream.AmplitudeReading += AudioStream_AmplitudeReading;

                    if(await InitializeRecording() == true)
                    {
                        LogMessage("Recording initialized...");
                        if (await StartRecording(AudioStream) == true)
                        {
                            LogMessage("Start Recording...");
                            _isRecording = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Failed to record Audio: Exception: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    LogMessage("Stop Recording...");
                    await StopRecording();
                    ClearLevel();
                    _isRecording = false;
                    UpdateControls();
                    if (AudioStream != null)
                    {
                        var filePicker = new Windows.Storage.Pickers.FileSavePicker();
                        filePicker.DefaultFileExtension = ".wav";
                        filePicker.SuggestedFileName = "record.wav";
                        filePicker.FileTypeChoices.Add("WAV files", new List<string>() { ".wav" });
                        filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
                        filePicker.SettingsIdentifier = "WavPicker";
                        filePicker.CommitButtonText = "Save buffer into a WAV File";

                        var wavFile = await filePicker.PickSaveFileAsync();
                        if (wavFile != null)
                        {
                            using (Stream stream = await wavFile.OpenStreamForWriteAsync())
                            {
                                if (stream != null)
                                {
                                    await AudioStream.AsStream().CopyToAsync(stream);
                                    LogMessage("Audio Stream stored in: " + wavFile.Path);
                                }
                            }
                            string fileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(wavFile);
                            mediaUri.Text = "file://" + wavFile.Path;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Failed to store Audio: Exception: " + ex.Message);
                }
            }
            UpdateControls();
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
        private async void convertWAVFile_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Converting file: " + mediaUri.Text + " into text");
            SpeechToTextResponse result = null;
            try
            {
                resultText.Text = string.Empty;
                if (!string.IsNullOrEmpty(mediaUri.Text))
                {
                    string authUrl = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
                    string Key = subscriptionKey.Text;
                    if (!string.IsNullOrEmpty(Key))
                    {
                        string token = await GetToken(authUrl, Key);
                        if (!string.IsNullOrEmpty(token))
                        {
                            if (!subscriptionKey.Text.Equals(GetSavedSubscriptionKey()))
                                SaveSubscriptionKey(subscriptionKey.Text);

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
                    }
                    else
                        LogMessage("Subscription Key missing");
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
