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
using TranslatorText;

namespace TranslatorTextUWPSampleApp
{
    /// <summary>
    /// Main page for the application.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        TranslatorTextClient client;
        Dictionary<string, string> languages;
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

            InputText.TextChanged += InputText_TextChanged;
            /*
            InputLanguages.Items.Clear();
            foreach(var l in LanguageArray)
                InputLanguages.Items.Add(l);
            InputLanguages.SelectedItem = "en-US";

            OutputLanguages.Items.Clear();
            foreach (var l in LanguageArray)
                OutputLanguages.Items.Add(l);
            OutputLanguages.SelectedItem = "en-US";
            */

            // Get Subscription ID from the local settings
            ReadSettingsAndState();
            
            // Update control and play first video
            UpdateControls();

            
            // Register Suspend/Resume
            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
            
            // Display OS, Device information
            LogMessage(TranslatorText.SystemInformation.GetString());
            
            // Create Cognitive Service TranslatorText Client
            client = new TranslatorTextClient();


            // Cognitive Service TranslatorText GetToken 
            if (!string.IsNullOrEmpty(subscriptionKey.Text))
            {
                LogMessage("Getting Token for subscription key: " + subscriptionKey.Text.ToString());
                string s = await client.GetToken(subscriptionKey.Text);
                if (!string.IsNullOrEmpty(s))
                    LogMessage("Getting Token successful Token: " + s.ToString());
                else
                    LogMessage("Getting Token failed for subscription Key: " + subscriptionKey.Text);
            }

        }

        private void InputText_TextChanged(object sender, TextChangedEventArgs e)
        {
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
        void Current_Resuming(object sender, object e)
        {
            LogMessage("Resuming");
            ReadSettingsAndState();

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
            deferal.Complete();
        }

        #region Settings
        const string keySubscription = "subscriptionKey";
        const string keyLevel = "levelKey";
        const string keyDuration = "durationKey";
        const string keyIsRecordingContinuously = "isRecordingContinuouslyKey";
        /// <summary>
        /// Function to save all the persistent attributes
        /// </summary>
        public bool SaveSettingsAndState()
        {
            SaveSettingsValue(keySubscription,subscriptionKey.Text);
            return true;
        }
        /// <summary>
        /// Function to read all the persistent attributes
        /// </summary>
        public bool ReadSettingsAndState()
        {
            string s = ReadSettingsValue(keySubscription) as string;
            if (!string.IsNullOrEmpty(s))
                subscriptionKey.Text = s;
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



        #region ui
        /// <summary>
        /// UpdateControls Method which update the controls on the page  
        /// </summary>
        async void UpdateControls()
        {

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                 () =>
                 {
                     {
                         if ((languages != null) && (languages.Count > 0) && (!string.IsNullOrEmpty(InputText.Text)))
                         {
                             TranslatingButton.IsEnabled = true;
                             DetectLanguageButton.IsEnabled = true;
                         }
                         else
                         {
                             TranslatingButton.IsEnabled = false;
                             DetectLanguageButton.IsEnabled = false;
                         }
                     }
                 });
        }
        bool bInProgress = false;
        /// <summary>
        /// sendContinuousAudioBuffer method which :
        /// - record audio sample permanently in the buffer
        /// - send the buffer to TranslatorText REST API once the recording is done
        /// </summary>
        private async void Translating_Click(object sender, RoutedEventArgs e)
        {
            if (bInProgress == true)
                return;
            bInProgress = true;
            try
            {
                if (InputText.Text.Length > 10000)
                {
                    LogMessage("More than 10000 characters to translate, translation cancelled...");
                }
                else
                {

                    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 1);

                    if (client != null)
                    {
                        if ((!client.HasToken()) && (!string.IsNullOrEmpty(subscriptionKey.Text)))
                        {
                            LogMessage("Getting Token for subscription key: " + subscriptionKey.Text.ToString());
                            string token = await client.GetToken(subscriptionKey.Text);
                            if (!string.IsNullOrEmpty(token))
                            {
                                LogMessage("Getting Token successful Token: " + token.ToString());
                                // Save subscription key
                                SaveSettingsAndState();
                            }
                        }
                        if (client.HasToken())
                        {
                            LogMessage("Start Translating...");
                            resultText.Text = await client.Translate(InputText.Text, InputLanguages.SelectedValue.ToString(), OutputLanguages.SelectedValue.ToString());
                            if (!string.IsNullOrEmpty(resultText.Text))
                            {
                                LogMessage("Translating succesful...");
                            }
                            else
                                LogMessage("Translating failed");
                        }
                        else
                            LogMessage("Authentication failed check your subscription Key: " + subscriptionKey.Text.ToString());
                    }
                    UpdateControls();
                }
            }
            finally
            {
                bInProgress = false;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            }

        }

        /// <summary>
        /// DetectLanguage method
        /// </summary>
        private async void DetectLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (bInProgress == true)
                return;
            bInProgress = true;
            try
            {

                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 1);

                if (client != null)
                {
                    if ((!client.HasToken()) && (!string.IsNullOrEmpty(subscriptionKey.Text)))
                    {
                        LogMessage("Getting Token for subscription key: " + subscriptionKey.Text.ToString());
                        string token = await client.GetToken(subscriptionKey.Text);
                        if (!string.IsNullOrEmpty(token))
                        {
                            LogMessage("Getting Token successful Token: " + token.ToString());
                            // Save subscription key
                            SaveSettingsAndState();
                        }
                    }
                    if (client.HasToken())
                    {
                            LogMessage("Detect Language from the input text...");
                            string lang = await client.DetectLanguage(InputText.Text);
                            if (!string.IsNullOrEmpty(lang))
                            {
                                LogMessage("Language detected: " + lang);
                                InputLanguages.SelectedValue = lang;
                            }
                            else
                                LogMessage("Language detection failed");
                    }
                    else
                        LogMessage("Authentication failed check your subscription Key: " + subscriptionKey.Text.ToString());
                }
                UpdateControls();
            }
            finally
            {
                bInProgress = false;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            }

        }

        /// <summary>
        /// GetLanguages from the Cognitive Services backend:
        /// </summary>
        private async void GetLanguages_Click(object sender, RoutedEventArgs e)
        {
            if (bInProgress == true)
                return;
            bInProgress = true;
            try
            {

                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 1);

                if (client != null)
                {
                    if ((!client.HasToken()) && (!string.IsNullOrEmpty(subscriptionKey.Text)))
                    {
                        LogMessage("Getting Token for subscription key: " + subscriptionKey.Text.ToString());
                        string token = await client.GetToken(subscriptionKey.Text);
                        if (!string.IsNullOrEmpty(token))
                        {
                            LogMessage("Getting Token successful Token: " + token.ToString());
                            // Save subscription key
                            SaveSettingsAndState();
                        }
                    }
                    if (client.HasToken())
                    {


                            LogMessage("Start Getting Languages...");
                            languages = await client.GetLanguages();
                            if ((languages!=null)&&(languages.Count>0))
                            {
                                LogMessage("Getting Languages sucessful");

                                OutputLanguages.ItemsSource = languages.OrderBy(d => d.Key);
                                //OutputLanguages.SelectedValue = languages.Keys.FirstOrDefault();
                                OutputLanguages.SelectedIndex = 0;

                                InputLanguages.ItemsSource = languages.OrderBy(d => d.Key);
                                //InputLanguages.SelectedValue = languages.Keys.FirstOrDefault();
                                InputLanguages.SelectedIndex = 0;
                            }
                            else
                                LogMessage("Getting Languages failed");
                    }
                    else
                        LogMessage("Authentication failed check your subscription Key: " + subscriptionKey.Text.ToString());
                }
                UpdateControls();
            }
            finally
            {
                bInProgress = false;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            }

        }

        #endregion ui
    }
}
