using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Custom;
using Windows.Devices.Enumeration;
using Windows.Foundation;

namespace TestDVDApp.CDReader
{
    class CDReaderManager
    {
        DeviceWatcher deviceWatcher;
        List<DeviceInformation> ListDeviceInformation;
        // CD Reader constant
        const uint CD_RAW_SECTOR_SIZE = 2352;
        const uint CD_SECTOR_SIZE = 2048;

        // Summary:
        //     Raised when CD Reader is detected.
        public event TypedEventHandler<CDReaderManager, CDReaderDevice> CDReaderDeviceAdded;
        // Summary:
        //     Raised when CD Reader is removed.
        public event TypedEventHandler<CDReaderManager, CDReaderDevice> CDReaderDeviceRemoved;
        protected virtual void OnDeviceAdded(CDReaderManager m, CDReaderDevice d)
        {
            if (CDReaderDeviceAdded != null)
                CDReaderDeviceAdded(m, d);
        }
        protected virtual void OnDeviceRemoved(CDReaderManager m, CDReaderDevice d)
        {
            if (CDReaderDeviceRemoved != null)
                CDReaderDeviceRemoved(m, d);
        }
        public CDReaderManager()
        {
            deviceWatcher = null;
            ListDeviceInformation = new List<DeviceInformation>();
            return;
        }
        public bool StartDiscovery()
        {
            bool result = false;
            string selector = CustomDevice.GetDeviceSelector(new Guid("53f56308-b6bf-11d0-94f2-00a0c91efb8b"));
            IEnumerable<string> additionalProperties = new string[] { "System.Devices.DeviceInstanceId" };
            if (deviceWatcher!=null)
            {
                StopDiscovery();
            }
            ListDeviceInformation.Clear();
            deviceWatcher = DeviceInformation.CreateWatcher(selector, additionalProperties);
            if (deviceWatcher != null)
            {
                deviceWatcher.Added += deviceWatcher_Added;
                deviceWatcher.Removed += deviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted += deviceWatcher_EnumerationCompleted;
                deviceWatcher.Start();
            }
            return result;
        }
        public bool StopDiscovery()
        {
            bool result = false;
            if (deviceWatcher != null)
            {
                deviceWatcher.Stop();
                deviceWatcher.Added -= deviceWatcher_Added;
                deviceWatcher.Removed -= deviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= deviceWatcher_EnumerationCompleted;
                deviceWatcher = null;
                result = true;
            }
            return result;
        }
        public async System.Threading.Tasks.Task<bool> EjectMedia(string Id)
        {
            bool result = false;
            var customDevice = await Windows.Devices.Custom.CustomDevice.FromIdAsync(Id, 
                DeviceAccessMode.ReadWrite,
                DeviceSharingMode.Exclusive);
            if (customDevice != null)
            {
                try
                {
                    uint r = await customDevice.SendIOControlAsync(
                           ejectMedia,
                        null, null);
                    result = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while ejecting Media: " + ex.Message);

                }
            }
            return result;
        }
        public async System.Threading.Tasks.Task<CDMetadata> ReadMediaMetadata(string Id)
        {
            CDMetadata result = null;
            var customDevice = await Windows.Devices.Custom.CustomDevice.FromIdAsync(Id,
                DeviceAccessMode.ReadWrite,
                DeviceSharingMode.Exclusive);
            if (customDevice != null)
            {
                try
                {
                    int[] SectorArray = await GetCDSectorArray(customDevice);
                    if ((SectorArray != null) && (SectorArray.Length > 1))
                    {
                        result = new CDMetadata();
                        for (int i = 0; i < (SectorArray.Length - 1); i++)
                        {
                            CDTrackMetadata t = new CDTrackMetadata() { Number = i + 1, Title = string.Empty, ISrc = string.Empty, FirstSector = SectorArray[i], LastSector = SectorArray[i + 1], Duration = TimeSpan.FromSeconds((SectorArray[i + 1] - SectorArray[i]) * CD_RAW_SECTOR_SIZE / (44100 * 4)) };
                            if (i < result.Tracks.Count)
                                result.Tracks[i] = t;
                            else
                                result.Tracks.Add(t);
                        }
                        byte[] TextArray = await GetCDTextArray(customDevice);
                        if (TextArray != null)
                        {
                            var r = FillCDMetadata(result, TextArray);
                            if (r != null)
                                result = r;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while reading Media Metadata: " + ex.Message);
                    result = null;

                }
            }
            return result;
        }
        static int MSF_TO_LBA2(byte min, byte sec, byte frame)
        {
            return (int)(frame + 75 * (sec - 2 + 60 * min));
        }
        private async System.Threading.Tasks.Task<int[]> GetCDSectorArray(Windows.Devices.Custom.CustomDevice device)
        {
            int[] Array = null;
            if (device != null)
            {
                var outputBuffer = new byte[MAXIMUM_NUMBER_TRACKS * 8 + 4];

                try
                {
                    uint r = await device.SendIOControlAsync(
                           readTable,
                        null, outputBuffer.AsBuffer());

                    if (r > 0)
                    {
                        int i_tracks = outputBuffer[3] - outputBuffer[2] + 1;

                        Array = new int[i_tracks + 1];
                        if (Array != null)
                        {
                            for (int i = 0; (i <= i_tracks) && (4 + i * 8 + 4 + 3 < r); i++)
                            {
                                int sectors = MSF_TO_LBA2(
                                    outputBuffer[4 + i * 8 + 4 + 1],
                                    outputBuffer[4 + i * 8 + 4 + 2],
                                    outputBuffer[4 + i * 8 + 4 + 3]);
                                Array[i] = sectors;
                                System.Diagnostics.Debug.WriteLine("track number: " + i.ToString() + " sectors: " + sectors.ToString());
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while reading CD Sector Array : " + ex.Message);
                }
            }
            return Array;
        }
        async System.Threading.Tasks.Task<byte[]> GetCDTextArray(Windows.Devices.Custom.CustomDevice device)
        {
            byte[] Array = null;
            if (device != null)
            {
                var inputBuffer = new byte[4];
                inputBuffer[0] = 0x05;
                var outputBuffer = new byte[4];

                try
                {
                    uint r = await device.SendIOControlAsync(
                           readTableEx,
                        inputBuffer.AsBuffer(), outputBuffer.AsBuffer());

                    if (r > 0)
                    {
                        int i_text = 2 + (outputBuffer[0] << 8) + outputBuffer[1];
                        if (i_text > 4)
                        {

                            Array = new byte[i_text];

                            r = await device.SendIOControlAsync(
                                       readTableEx,
                                    inputBuffer.AsBuffer(), Array.AsBuffer());
                        }

                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while reading CD Text Array : " + ex.Message);
                }
            }
            return Array;
        }
        CDMetadata FillCDMetadata(CDMetadata currentCD, byte[] TextArray)
        {
            try
            {
                int i_track_last = 0;
                string[] ArrayTrackInfo = new string[99];
                int Count = (TextArray.Length - 4) / 18;
                //Clear CD and Track metadata info:
                currentCD.ISrc = string.Empty;
                currentCD.Message = string.Empty;
                currentCD.Genre = string.Empty;
                currentCD.AlbumTitle = string.Empty;
                currentCD.Artist = string.Empty;
                currentCD.DiscID = string.Empty;


                for (int i = 0; i < currentCD.Tracks.Count; i++)
                {
                    currentCD.Tracks[i].ISrc = string.Empty;
                    currentCD.Tracks[i].Title = string.Empty;
                }
                for (int i = 0; i < Count; i++)
                {


                    int i_pack_type = TextArray[4 + 18 * i];
                    if (i_pack_type < 0x80 || i_pack_type > 0x8f)
                        continue;

                    int i_track_number = (TextArray[4 + 18 * i + 1] >> 0) & 0x7f;
                    int i_extension_flag = (TextArray[4 + 18 * i + 1] >> 7) & 0x01;
                    if (i_extension_flag != 0)
                        continue;

                    int i_track = i_track_number;

                    int indexTrack = 4 + 18 * i + 4;
                    int indexTrackMax = indexTrack + 12;
                    while (i_track <= 127 && indexTrack < indexTrackMax)
                    {
                        byte[] resArray = new byte[13];
                        int k = 0;
                        int l = 0;
                        for (k = 0; k < 12; k++, l++)
                        {
                            resArray[l] = TextArray[4 + 18 * i + 4 + k];
                            if ((resArray[l] == 0x00) || (k == 11))
                            {
                                string str;
                                str = System.Text.Encoding.UTF8.GetString(resArray, 0, (resArray[l] == 0x00) ? l : l + 1);
                                if (!string.IsNullOrEmpty(str))
                                {
                                    switch (i_pack_type - 0x80)
                                    {
                                        // Title
                                        case 0x00:
                                            if (i_track == 0)
                                                currentCD.AlbumTitle += str;
                                            else
                                            {
                                                if (i_track == currentCD.Tracks.Count + 1)
                                                {
                                                    CDTrackMetadata t = new CDTrackMetadata() { Number = i_track, Title = string.Empty, ISrc = string.Empty, FirstSector = 0, LastSector = 0, Duration = TimeSpan.FromSeconds(0) };
                                                    if ((i_track - 1) < currentCD.Tracks.Count)
                                                        currentCD.Tracks[i_track - 1] = t;
                                                    else
                                                        currentCD.Tracks.Add(t);
                                                }
                                                if (i_track <= currentCD.Tracks.Count)
                                                    currentCD.Tracks[i_track - 1].Title += str;
                                            }
                                            break;
                                        // DiscID
                                        case 0x06:
                                            if (i_track == 0)
                                                currentCD.DiscID += str;
                                            break;
                                        // Artist
                                        case 0x01:
                                            if (i_track == 0)
                                                currentCD.Artist += str;
                                            break;
                                        // Message
                                        case 0x05:
                                            if (i_track == 0)
                                                currentCD.Message += str;
                                            break;
                                        // Genre
                                        case 0x07:
                                            if (i_track == 0)
                                                currentCD.Genre += str;
                                            break;
                                        // ISRC
                                        case 0x0E:
                                            if (i_track == 0)
                                                currentCD.ISrc += str;
                                            else
                                            {
                                                if (i_track == currentCD.Tracks.Count + 1)
                                                {
                                                    CDTrackMetadata t = new CDTrackMetadata() { Number = i_track, Title = string.Empty, ISrc = string.Empty, FirstSector = 0, LastSector = 0, Duration = TimeSpan.FromSeconds(0) };
                                                    if ((i_track - 1) < currentCD.Tracks.Count)
                                                        currentCD.Tracks[i_track - 1] = t;
                                                    else
                                                        currentCD.Tracks.Add(t);
                                                }
                                                if (i_track <= currentCD.Tracks.Count)
                                                    currentCD.Tracks[i_track - 1].ISrc += str;
                                            }
                                            break;
                                        default:
                                            break;

                                    }
                                }
                                // System.Diagnostics.Debug.WriteLine("Track: " + i_track.ToString() + " Type: " + (i_pack_type - 0x80).ToString() + " Text: " + str);
                                i_track++;
                                l = -1;
                            }
                        }
                        indexTrack += k;
                        i_track_last = (i_track_last > i_track ? i_track_last : i_track);

                        i_track++;
                        indexTrack += 1 + 12;
                    }
                }
                System.Diagnostics.Debug.WriteLine("Title: " + currentCD.AlbumTitle + " Artist: " + currentCD.Artist + " DiscID: " + currentCD.DiscID + " ISrc: " + currentCD.ISrc);
                for (int l = 0; l < currentCD.Tracks.Count; l++)
                {
                    System.Diagnostics.Debug.WriteLine("Track : " + currentCD.Tracks[l].Number.ToString() + " Title: " + currentCD.Tracks[l].Title + "Duration: " + currentCD.Tracks[l].Duration.ToString() + " ISRC: " + currentCD.Tracks[l].ISrc);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while parsing CD Text Array : " + ex.Message);
                currentCD = null;
            }
            return currentCD;
        }
        private void deviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
        }

        private void deviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if ((ListDeviceInformation != null) && (ListDeviceInformation.Count > 0))
            {
                foreach (var d in ListDeviceInformation)
                {
                    if (d.Id == args.Id)
                    {
                        ListDeviceInformation.Remove(d);
                        OnDeviceRemoved(this, new CDReaderDevice(d.Name, d.Id));
                        break;
                    }
                }
            }
        }

        private void deviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            if (ListDeviceInformation != null)
            {
                ListDeviceInformation.Add(args);
                OnDeviceAdded(this, new CDReaderDevice(args.Name, args.Id));
            }
        }
        // CD attributes
        const ushort FILE_DEVICE_CD_ROM = 0x00000002;
        const ushort FILE_DEVICE_MASS_STORAGE = 0x0000002d;
        const int MAXIMUM_NUMBER_TRACKS = 100;
        IOControlCode ejectMedia = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0202, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode loadMedia = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0203, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode reserveMedia = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0204, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode releaseMedia = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0205, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode readTable = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0000, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode readTableEx = new IOControlCode(FILE_DEVICE_CD_ROM, 0x0015, IOControlAccessMode.Read, IOControlBufferingMethod.Buffered);
        IOControlCode readRaw = new IOControlCode(FILE_DEVICE_CD_ROM, 0x000F, IOControlAccessMode.Read, IOControlBufferingMethod.DirectOutput);

    }
}
