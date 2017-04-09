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

namespace TestVideoMultiPIP
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
        public MainPage()
        {
            mediaList.Add(new MediaItem { title = "Channel 1", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 2", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 3", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 4", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 5", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 6", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            //mediaList.Add(new MediaItem { title = "Channel 1", mediaUrl = "video://channel1.mp4", imageUrl = "" });
            //mediaList.Add(new MediaItem { title = "Channel 2", mediaUrl = "video://channel2.mp4", imageUrl = "" });
            //mediaList.Add(new MediaItem { title = "Channel 3", mediaUrl = "video://channel3.mp4", imageUrl = "" });
            //mediaList.Add(new MediaItem { title = "Channel 4", mediaUrl = "video://channel4.mp4", imageUrl = "" });
            //mediaList.Add(new MediaItem { title = "Channel 5", mediaUrl = "video://channel5.mp4", imageUrl = "" });
            //mediaList.Add(new MediaItem { title = "Channel 6", mediaUrl = "video://channel6.mp4", imageUrl = "" });

            this.InitializeComponent();
        }
        ObservableCollection<MediaItem> mediaList = new ObservableCollection<MediaItem>();

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

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (gridView.Items.Count > 0)
                gridView.SelectedIndex = gridView.Items.Count - 1;
            foreach (var item in mediaList)
            {
                string url = item.mediaUrl;
                var mediaPlayerElement = FindControl<MediaPlayerElement>(gridView, item.title);
                if (mediaPlayerElement != null)
                {
                }
            }
        }

        void MuteAllMediaPlayerElements(DependencyObject parent)
        {
            var c = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < c; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var sv = child as MediaPlayerElement;
                if (sv != null)
                    sv.MediaPlayer.IsMuted = true;
                else
                    MuteAllMediaPlayerElements(child);
            }
        }
  

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            sender.PlaybackSession.Position = TimeSpan.Zero;
            sender.Play();
        }

        private List<FrameworkElement> AllChildren(DependencyObject parent)
        {
            var _List = new List<FrameworkElement>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is FrameworkElement)
                {
                    _List.Add(_Child as FrameworkElement);
                }
                _List.AddRange(AllChildren(_Child));
            }
            return _List;
        }


        private T FindControl<T>(DependencyObject parentContainer, string controlName)
        {
            var childControls = AllChildren(parentContainer);
            if((childControls != null)&&(childControls.Count>0))
            {
                try
                {

                    var control = childControls.OfType<FrameworkElement>().Where(x => x.Name.Equals(controlName)).Cast<T>().First();
                    return control;
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while getting control: " + e.Message);
                }
            }
            return default(T);
        }
        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            GridView g = sender as GridView;
            if (g != null)
            {
                var item = g.SelectedItem as MediaItem;
                if (item != null)
                {
                    MuteAllMediaPlayerElements(g);
                    var mediaPlayerElement = FindControl<MediaPlayerElement>(g, item.title);
                    if (mediaPlayerElement != null)
                        mediaPlayerElement.MediaPlayer.IsMuted = false;
                }
            }
        }
    
        MediaItem GetItem(string name)
        {
            foreach(var item in mediaList)
            {
                if (string.Equals(item.title, name))
                    return item;
            }
            return null;
        }
        private async void MediaPlayerElement_Loaded(object sender, RoutedEventArgs e)
        {
            MediaPlayerElement mediaPlayerElement = sender as MediaPlayerElement;
            if (mediaPlayerElement != null)
            {
                System.Diagnostics.Debug.WriteLine("MPE: " + mediaPlayerElement.Name + " loaded");
                MediaItem item = GetItem(mediaPlayerElement.Name);
                if (item != null)
                {
                    var player = new MediaPlayer();
                    if (player != null)
                    {
                        string url = item.mediaUrl;
                        player.MediaEnded += Player_MediaEnded;
                        player.AutoPlay = true;
                        player.IsMuted = true;
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
    }
}
