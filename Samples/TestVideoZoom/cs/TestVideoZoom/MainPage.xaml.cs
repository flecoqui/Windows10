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
        public MainPage()
        {
            this.InitializeComponent();
            // Set Minimum size for the view
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size
            {
                Height = 240,
                Width = 320
            });

        }

        bool bMin = true;
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
