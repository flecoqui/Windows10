using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Custom;
using Windows.Storage.Streams;

namespace TestDVDApp.CDReader
{
    /// <summary>
    /// class CDTrackStream
    /// </summary>
    /// <info>
    /// Class used to:
    /// - play audio CD track
    /// - store audio CD track in a file
    /// This class automatically update the WAV Header based on the length 
    /// of data to read.
    /// </info>
    public class CDTrackStream : IRandomAccessStream
    {
        // CD Reader Device ID
        string DeviceID;
        // Start sector for the selected track
        int StartSector;
        // End sector for the selected track
        int EndSector;
        // Static properties associated with CDReader
        static CustomDevice CDReaderDevice;
        static IOControlCode readRaw = new IOControlCode(FILE_DEVICE_CD_ROM, 0x000F, IOControlAccessMode.Read, IOControlBufferingMethod.DirectOutput);

        // CD Reader constant
        const uint CD_RAW_SECTOR_SIZE = 2352;
        const uint CD_SECTOR_SIZE = 2048;
        const ushort FILE_DEVICE_CD_ROM = 0x00000002;

        // CD reader internal Stream and read/write index (bytes)
        Windows.Storage.Streams.InMemoryRandomAccessStream internalStream;
        private ulong ReadDataIndex = 0;
        private ulong WriteDataIndex = 0;

        public async static System.Threading.Tasks.Task<CDTrackStream> Create(string deviceID, int startSector, int endSector)
        {
            CDTrackStream cdts = null;
            try
            {
                cdts = new CDTrackStream(deviceID, startSector, endSector);
                if (cdts != null)
                {
                    bool result = await cdts.SetCDDeviceAsync();
                    if (result == true)
                    {
                        result = await cdts.WriteWAVHeader();
                    }
                    if (result != true)
                        cdts = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while creating CDTrackStream: " + ex.Message);
            }
            return cdts;
        }
        private CDTrackStream(string deviceID, int startSector, int endSector)
        {
            DeviceID = deviceID;
            StartSector = startSector;
            EndSector = endSector;

            WriteDataIndex = 0;
            ReadDataIndex = 0;
            internalStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
        }
        private async System.Threading.Tasks.Task<bool> WriteWAVHeader()
        {
            if (internalStream != null)
            {
                byte[] buffer = this.CreateWAVHeaderBuffer((uint)(CD_RAW_SECTOR_SIZE * (EndSector - StartSector)));
                if (buffer != null)
                {
                    uint l = await internalStream.WriteAsync(buffer.AsBuffer());
                    if (l == buffer.Length)
                    {
                        WriteDataIndex = l;
                        return true;
                    }
                }
            }
            return false;
        }
        private async System.Threading.Tasks.Task<bool> SetCDDeviceAsync()
        {
            try
            {
                if (CDReaderDevice == null)
                    CDReaderDevice = await CustomDevice.FromIdAsync(DeviceID, DeviceAccessMode.Read, DeviceSharingMode.Exclusive);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while getting CDREaderDevice: " + ex.Message);
            }
            if (CDReaderDevice != null)
                return true;
            return false;
        }
        // Return the WAV stream length
        public ulong GetLength()
        {
            return (ulong)((CD_RAW_SECTOR_SIZE * (EndSector - StartSector)) + GetWAVHeaderBufferLen());
        }
        private uint GetWAVHeaderBufferLen()
        {
            return 4 + 16 + 8 + 8 + 8;
        }
        private byte[] CreateWAVHeaderBuffer(uint Len)
        {
            uint headerLen = 4 + 16 + 8 + 8 + 8;
            byte[] updatedBuffer = new byte[headerLen];
            if (updatedBuffer != null)
            {
                System.Text.UTF8Encoding.UTF8.GetBytes("RIFF").CopyTo(0, updatedBuffer.AsBuffer(), 0, 4);
                BitConverter.GetBytes(4 + 16 + 8 + Len + 8).CopyTo(0, updatedBuffer.AsBuffer(), 4, 4);
                System.Text.UTF8Encoding.UTF8.GetBytes("WAVE").CopyTo(0, updatedBuffer.AsBuffer(), 8, 4);
                System.Text.UTF8Encoding.UTF8.GetBytes("fmt ").CopyTo(0, updatedBuffer.AsBuffer(), 12, 4);
                BitConverter.GetBytes((uint)16).CopyTo(0, updatedBuffer.AsBuffer(), 16, 4);
                BitConverter.GetBytes(1).CopyTo(0, updatedBuffer.AsBuffer(), 20, 2);
                BitConverter.GetBytes((ushort)2).CopyTo(0, updatedBuffer.AsBuffer(), 22, 2);
                BitConverter.GetBytes((uint)44100).CopyTo(0, updatedBuffer.AsBuffer(), 24, 4);
                BitConverter.GetBytes((uint)176400).CopyTo(0, updatedBuffer.AsBuffer(), 28, 4);
                BitConverter.GetBytes((UInt16)4).CopyTo(0, updatedBuffer.AsBuffer(), 32, 2);
                BitConverter.GetBytes((UInt16)16).CopyTo(0, updatedBuffer.AsBuffer(), 34, 2);

                System.Text.UTF8Encoding.UTF8.GetBytes("data").CopyTo(0, updatedBuffer.AsBuffer(), 20 + 16, 4);
                BitConverter.GetBytes(Len).CopyTo(0, updatedBuffer.AsBuffer(), 24 + 16, 4);
            }
            return updatedBuffer;
        }

        public bool CanRead
        {
            get { return true; }
        }

        public bool CanWrite
        {
            get { return true; }
        }

        public IRandomAccessStream CloneStream()
        {
            // return this.internalStream.CloneStream();

            var t = CDTrackStream.Create(DeviceID, StartSector, EndSector);
            t.Wait();
            CDTrackStream cdts = t.Result;
            if (cdts != null)
            {
                if (this.Size == cdts.Size)
                    return cdts;
            }
            return null;
        }

        public IInputStream GetInputStreamAt(ulong position)
        {
            System.Diagnostics.Debug.WriteLine("GetInputStreamAt: " + position.ToString());
            if (internalStream.Size > position)
                return internalStream.GetInputStreamAt(position);
            return null;
        }

        public IOutputStream GetOutputStreamAt(ulong position)
        {
            System.Diagnostics.Debug.WriteLine("GetOutputStreamAt: " + position.ToString());
            if (internalStream.Size > position)
                return internalStream.GetOutputStreamAt(position);
            return null;
        }

        public ulong Position
        {
            get
            {
                System.Diagnostics.Debug.WriteLine("Position: " + internalStream.Position.ToString());
                return internalStream.Position;
            }
        }

        public void Seek(ulong position)
        {
            if (position >= internalStream.Size)
            {
                // Fill the buffer till the new position 
                var t = FillInternalStream(position);
                t.Wait();
            }
            System.Diagnostics.Debug.WriteLine("Seek: " + position.ToString() + " - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position);
            ReadDataIndex = position;
        }

        public ulong Size
        {
            get
            {
                System.Diagnostics.Debug.WriteLine("Size: " + GetLength().ToString());
                return GetLength();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public ulong MaxSize
        {
            get
            {
                return GetLength();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public void Dispose()
        {
            internalStream.Dispose();
            internalStream = null;
        }
        private async System.Threading.Tasks.Task<bool> FillInternalStream(ulong index)
        {
            bool result = false;
          //  ulong startIndexInCD = ((ulong)StartSector * CD_SECTOR_SIZE) + internalStream.Size - GetWAVHeaderBufferLen();
          //  ulong endIndexInCD = ((ulong)StartSector * CD_SECTOR_SIZE) + index - GetWAVHeaderBufferLen();
//            ulong startReadSector = startIndexInCD / CD_SECTOR_SIZE;
//            ulong endReadSector = (endIndexInCD / CD_SECTOR_SIZE) + 1;
            ulong startReadSector = (ulong)StartSector +  ((internalStream.Size - GetWAVHeaderBufferLen()) / CD_RAW_SECTOR_SIZE);
            ulong endReadSector = (ulong)StartSector + ((index - GetWAVHeaderBufferLen()) / CD_RAW_SECTOR_SIZE) + 1;
            if (endReadSector > (ulong)EndSector)
                endReadSector = (ulong)EndSector;
            if ((StartSector < EndSector) &&
                (startReadSector < endReadSector))
            {
                int numberSector = 20;
                var inputBuffer = new byte[8 + 4 + 4];
                var outputBuffer = new byte[CD_RAW_SECTOR_SIZE * numberSector];
                ulong k = startReadSector;
                while (k < endReadSector)
                {
                    numberSector = (int)((((ulong)(k + (ulong)numberSector)) < endReadSector) ? 20 :endReadSector-k);
                    ulong firstSector = k * CD_SECTOR_SIZE;
                    byte[] array = BitConverter.GetBytes(firstSector);
                    for (int i = 0; i < array.Length; i++)
                        inputBuffer[i] = array[i];
                    byte[] intarray = BitConverter.GetBytes(numberSector);
                    for (int i = 0; i < intarray.Length; i++)
                        inputBuffer[8 + i] = intarray[i];
                    intarray = BitConverter.GetBytes((int)2);
                    for (int i = 0; i < intarray.Length; i++)
                        inputBuffer[12 + i] = intarray[i];
                    uint r = 0; ;
                    r = await CDReaderDevice.SendIOControlAsync(
                           readRaw, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());
                    if (r > 0)
                    {
                        uint len = await this.WriteAsync(outputBuffer.AsBuffer(0, (int)r));
                        if (len == r)
                            result = true;
                    }
                    k += (ulong)numberSector;
                }
            }
            return result;
        }
        public Windows.Foundation.IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run<IBuffer, uint>((token, progress) =>
            {
                return System.Threading.Tasks.Task.Run(async () =>
                {
                    uint len = 0;
                    bool result = false;
                    if (internalStream != null)
                    {
                        System.Diagnostics.Debug.WriteLine("ReadAsync request - count: " + count.ToString() + " bytes  at " + ReadDataIndex.ToString() + " - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position + " Track size: " + GetLength().ToString());
                        if (ReadDataIndex + count > internalStream.Size)
                        {
                            // Fill the buffer
                            result = await FillInternalStream(ReadDataIndex + count);
                        }
                        if (internalStream != null)
                        {
                            var inputStream = internalStream.GetInputStreamAt(ReadDataIndex);
                            if (inputStream != null)
                            {

                                inputStream.ReadAsync(buffer, count, options).AsTask().Wait();
                                len = buffer.Length;
                                ReadDataIndex += len;
                                System.Diagnostics.Debug.WriteLine("ReadAsync return : " + buffer.Length.ToString() + " bytes  at " + (ReadDataIndex - len).ToString() + " - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position + " Track size: " + GetLength().ToString());
                            }
                        }
                    }
                    progress.Report(len);
                    return buffer;
                });
            });
        }



        public Windows.Foundation.IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run<uint, uint>((token, progress) =>
            {
                return System.Threading.Tasks.Task.Run(() =>
                {
                    uint len = 0;
                    if (internalStream != null)
                    {

                        var outputStream = internalStream.GetOutputStreamAt(WriteDataIndex);
                        if (outputStream != null)
                        {
                            outputStream.WriteAsync(buffer).AsTask().Wait();
                            WriteDataIndex += buffer.Length;
                            len = buffer.Length;
                            System.Diagnostics.Debug.WriteLine("WriteAsync return : " + buffer.Length.ToString() + " bytes  at " + (WriteDataIndex - len).ToString() + " - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position + " Track size: " + GetLength().ToString());
                        }
                    }
                    progress.Report(len);
                    return len;
                });
            });

        }
        public Windows.Foundation.IAsyncOperation<bool> FlushAsync()
        {
            return internalStream.FlushAsync();
        }
    }
}
