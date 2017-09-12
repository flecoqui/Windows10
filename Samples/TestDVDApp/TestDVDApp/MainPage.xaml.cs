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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestDVDApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DeviceWatcher dw;
        List<DeviceInformation> ListDeviceInformation;
        public MainPage()
        {
            this.InitializeComponent();
            if (ListDevices.Items != null)
                ListDevices.Items.Clear();
            ListDeviceInformation = new List<DeviceInformation>();
            ListDevices.SelectionChanged += ListDevices_SelectionChanged;
            ButtonEjectMedia.IsEnabled = false;
            ButtonStartDiscover.Visibility = Visibility.Visible;
            ButtonStopDiscover.Visibility = Visibility.Collapsed;
            ListDevices.IsEnabled = false;
            CheckListDevices();
        }
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
                ButtonEjectMedia.IsEnabled = true;
            else 
                ButtonEjectMedia.IsEnabled = false;
        }

        private void Dw_EnumerationCompleted(DeviceWatcher sender, object args)
        {
        }

        private async void Dw_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
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

        private async void Dw_Added(DeviceWatcher sender, DeviceInformation args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
             () =>
             {
                 if(ListDeviceInformation!=null)
                     ListDeviceInformation.Add(args);
                 ListDevices.Items.Add(args.Id);
                 CheckListDevices();
             });
        }
        private void ButtonStartDiscover_Click(object sender, RoutedEventArgs e)
        {
            if(ListDevices.Items!=null)
                ListDevices.Items.Clear();
            CheckListDevices();
            string selector = CustomDevice.GetDeviceSelector(new Guid("53f56308-b6bf-11d0-94f2-00a0c91efb8b"));
            //string selector = CustomDevice.GetDeviceSelector(new Guid("53f56308-b6bf-11d0-94f2-00a0c91efb8b"));
            IEnumerable<string> additionalProperties = new string[] { "System.Devices.DeviceInstanceId" };
            dw = DeviceInformation.CreateWatcher(selector, additionalProperties);
            dw.Added += Dw_Added;
            dw.Removed += Dw_Removed;
            dw.EnumerationCompleted += Dw_EnumerationCompleted;
            dw.Start();
            ButtonStartDiscover.Visibility = Visibility.Collapsed;
            ButtonStopDiscover.Visibility = Visibility.Visible;
            ListDevices.IsEnabled = true;

        }
        private void ButtonStopDiscover_Click(object sender, RoutedEventArgs e)
        {
            if (dw != null)
            {
                dw.Stop();
                dw.Added -= Dw_Added;
                dw.Removed -= Dw_Removed;
                dw.EnumerationCompleted -= Dw_EnumerationCompleted;
                dw = null;
            }
            ButtonStartDiscover.Visibility = Visibility.Visible;
            ButtonStopDiscover.Visibility = Visibility.Collapsed;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var devices = await DeviceInformation.FindAllAsync();
            foreach (var device in devices)
            {
                try
                {
                    //Debug.WriteLine("Device Name: " + device.Name);
                    if (string.Equals(device.Name, "MT1887"))

                    {
                        Debug.WriteLine("Device Name: " + device.Name);

                        foreach ( var p in device.Properties)
                        {
                            Debug.WriteLine("Property: " + p.Key  + " value: " + (p.Value!=null?p.Value.ToString():"null"));
                        }
                        var customDevice = await Windows.Devices.Custom.CustomDevice.FromIdAsync(device.Id, DeviceAccessMode.ReadWrite,
                            DeviceSharingMode.Exclusive);

                        var inputBuffer = new byte[1];
                        var outputBuffer = new byte[1];
                  //      IOControlCode ^ Fx2Driver::EjectMedia = ref new IOControlCode(IOCTL_STORAGE_BASE, 0x0202, IOControlAccessMode::Read, IOControlBufferingMethod::Buffered);

                        var r = await customDevice.SendIOControlAsync(
                                new IOControlCode(0x0000002d, 0x0202,IOControlAccessMode.Read, IOControlBufferingMethod.Buffered),
                            null, null);

//                        var r = await customDevice.SendIOControlAsync(
//                            new IOControlCode(UInt16.MinValue, UInt16.MaxValue, IOControlAccessMode.Read,
//                                IOControlBufferingMethod.Neither),
//                            inputBuffer.AsBuffer(), outputBuffer.AsBuffer());
                    }

                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine(ex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

        }

        private async void ButtonEjectMedia_Click(object sender, RoutedEventArgs e)
        {
            string id  = ListDevices.SelectedItem as string;
            if (!string.IsNullOrEmpty(id) && (id != "None"))
            {
                DeviceInformation device = null;
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
                            Debug.WriteLine("Device Name: " + device.Name);

                            foreach (var p in device.Properties)
                            {
                                Debug.WriteLine("Property: " + p.Key + " value: " + (p.Value != null ? p.Value.ToString() : "null"));
                            }
                            var customDevice = await Windows.Devices.Custom.CustomDevice.FromIdAsync(device.Id, DeviceAccessMode.ReadWrite,
                                DeviceSharingMode.Exclusive);

                            //var inputBuffer = new byte[1];
                            //var outputBuffer = new byte[1];
                            //      IOControlCode ^ Fx2Driver::EjectMedia = ref new IOControlCode(IOCTL_STORAGE_BASE, 0x0202, IOControlAccessMode::Read, IOControlBufferingMethod::Buffered);

                            var r = await customDevice.SendIOControlAsync(
                                    new IOControlCode(0x0000002d, 0x0202, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered),
                                null, null);

                            //                        var r = await customDevice.SendIOControlAsync(
                            //                            new IOControlCode(UInt16.MinValue, UInt16.MaxValue, IOControlAccessMode.Read,
                            //                                IOControlBufferingMethod.Neither),
                            //                            inputBuffer.AsBuffer(), outputBuffer.AsBuffer());
                        }

                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

        }













    }
}
