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
            this.InitializeComponent();
            mediaList.Add(new MediaItem { title = "Channel 1", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 2", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 3", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 4", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 5", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            mediaList.Add(new MediaItem { title = "Channel 6", mediaUrl = "http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/Manifest(format=m3u8-aapl-v3)", imageUrl = "" });
            // Set Minimum size for the view
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size
            {
                Height = 240,
                Width = 320
            });

        }
        ObservableCollection<MediaItem> mediaList = new ObservableCollection<MediaItem>();

        bool bMin = true;
        private void Player_Tapped(object sender, TappedRoutedEventArgs e)
        {
            double w = Window.Current.Bounds.Width;
            double h = Window.Current.Bounds.Height;
            MediaElement loc = sender as MediaElement;
            if (loc != null)
            {
                if (bMin == true)
                {
                    Windows.UI.Xaml.Media.Animation.Storyboard lmax = loc.Resources["MyStoryboardMax"] as Windows.UI.Xaml.Media.Animation.Storyboard;
                //    loc.Margin = new Thickness(-100,0,0,0);
                    lmax.Begin();
                }
                else
                {
                  //  loc.Margin = new Thickness(0, 0, 0, 0);
                    Windows.UI.Xaml.Media.Animation.Storyboard lmin = loc.Resources["MyStoryboardMin"] as Windows.UI.Xaml.Media.Animation.Storyboard;
                    lmin.Begin();
                }
            }
            bMin = !bMin;
        }

        MediaElement GetFirstDescendantMediaElement(DependencyObject parent)
        {
            var c = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < c; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var sv = child as MediaElement;
                if (sv != null)
                {
                    return sv;
                }
                sv = GetFirstDescendantMediaElement(child);
                if (sv != null)
                    return sv;
            }

            return null;
        }
        void MuteAllMediaElements(DependencyObject parent)
        {
            var c = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < c; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var sv = child as MediaElement;
                if (sv != null)
                    sv.IsMuted = true;
                else
                    MuteAllMediaElements(child);
            }
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
            var control = childControls.OfType<FrameworkElement>().Where(x => x.Name.Equals(controlName)).Cast<T>().First();
            return control;
        }
        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridView g = sender as GridView;
            if(g!=null)
            {
                var item = g.SelectedItem as MediaItem;
                if(item!=null)
                {
                    MuteAllMediaElements(g);
                    var mediaElement = FindControl<MediaElement>(g, item.title); 
                    if (mediaElement != null)
                        mediaElement.IsMuted = false;
                }
            }
        }


        private void mediaElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MediaElement loc = sender as MediaElement;
            if (loc != null)
            {
                if (bMin == true)
                {
                    Windows.UI.Xaml.Media.Animation.Storyboard lmax = loc.Resources["MyStoryboardMax"] as Windows.UI.Xaml.Media.Animation.Storyboard;
                    if (lmax != null)
                    {
                        foreach (var a in lmax.Children)
                        {
                            Windows.UI.Xaml.Media.Animation.DoubleAnimation da = a as Windows.UI.Xaml.Media.Animation.DoubleAnimation;
                            if (da != null)
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
