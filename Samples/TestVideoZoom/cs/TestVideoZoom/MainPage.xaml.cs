using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Playback;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestVideoZoom
{
    public class MediaItem
    {
        public String title { get; set; }
        public String mediaUrl { get; set; }
        public String imageUrl { get; set; }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MediaItem mediaItem = null;
        public MainPage()
        {
            mediaItem = new MediaItem { title = "Channel 1", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" };
            //mediaItem = new MediaItem { title = "Channel 1", mediaUrl = "video://channel2.mp4", imageUrl = "" };
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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
            }

        }
        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            sender.PlaybackSession.Position = TimeSpan.Zero;
            sender.Play();
        }
        MediaItem GetItem()
        {
            return mediaItem;
        }
        private async void MediaPlayerElement_Loaded(object sender, RoutedEventArgs e)
        {
            MediaPlayerElement mediaPlayerElement = sender as MediaPlayerElement;
            if (mediaPlayerElement != null)
            {
                System.Diagnostics.Debug.WriteLine("MPE: " + mediaPlayerElement.Name + " loaded");
                MediaItem item = GetItem();
                if (item != null)
                {
                    var player = new MediaPlayer();
                    if (player != null)
                    {
                        string url = item.mediaUrl;
                        player.MediaEnded += Player_MediaEnded;
                        player.AutoPlay = true;
                        player.IsMuted = false;
                        if (!url.StartsWith("video://"))
                        {
                            var uri = new Uri(url);
                            if (uri != null)
                            {
                                player.Source = Windows.Media.Core.MediaSource.CreateFromUri(uri);
                            }
                        }
                        else
                        {
                            var path = url.Replace("video://", "");
                            if (path != null)
                            {
                                try
                                {
                                    Windows.Storage.StorageFolder folder = Windows.Storage.KnownFolders.VideosLibrary;
                                    Windows.Storage.StorageFile file = null;
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
                                    {
                                        file = await folder.GetFileAsync(filename);
                                        if (file != null)
                                            player.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("Exception while opening file: " + ex.Message);
                                }
                            }

                        }
                        mediaPlayerElement.SetMediaPlayer(player);
                    }
                }
            }
        }
        bool bMin = true;
        private void mediaElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MediaPlayerElement loc = sender as MediaPlayerElement;
            if (loc != null)
            {
                if (bMin == true)
                {
                    Windows.UI.Xaml.Media.Animation.Storyboard lmax = loc.Resources["MyStoryboardMax"] as Windows.UI.Xaml.Media.Animation.Storyboard;
                    if (lmax != null)
                    {
                        foreach( var a in lmax.Children)
                        {
                            Windows.UI.Xaml.Media.Animation.DoubleAnimation da = a as Windows.UI.Xaml.Media.Animation.DoubleAnimation;
                            if(da!=null)
                            {
                                double w = Window.Current.Bounds.Width;
                                da.To = w / 320;
                            }
                        }
                        lmax.Begin();
                    }
                }
                else
                {
                    Windows.UI.Xaml.Media.Animation.Storyboard lmin = loc.Resources["MyStoryboardMin"] as Windows.UI.Xaml.Media.Animation.Storyboard;
                    lmin.Begin();
                }
            }
            bMin = !bMin;
        }

    }
}
