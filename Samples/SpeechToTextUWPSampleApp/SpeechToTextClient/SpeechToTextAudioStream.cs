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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SpeechToTextClient
{
    /// <summary>
    /// class SpeechToTextAudioStream
    /// </summary>
    /// <info>
    /// Class used to:
    /// - record the audio samples in StorageFile
    /// - forward the audio samples towards the SpeechToText REST API
    /// This class automatically update the WAV Header based on the length 
    /// of data to store or transmit.
    /// Moreover, it removes the JUNK chunk from the WAV header.
    /// </info>
    public class SpeechToTextAudioStream : IRandomAccessStream
    {
        private uint nChannels;
        private uint nSamplesPerSec;
        private uint nAvgBytesPerSec;
        private uint nBlockAlign;
        private uint wBitsPerSample;
        private ulong audioStart = 0;
        private ulong audioEnd = 0;
        private Windows.Storage.Streams.IRandomAccessStream internalStream;
        private Windows.Storage.Streams.IInputStream inputStream;
        public static SpeechToTextAudioStream Create(
            uint Channels, 
            uint SamplesPerSec, 
            uint AvgBytesPerSec,
            uint BlockAlign,
            uint BitsPerSample,
            ulong AudioStart
            )
        {
            SpeechToTextAudioStream stts = null;
            try
            {
                stts = new SpeechToTextAudioStream(Channels, SamplesPerSec, AvgBytesPerSec, BlockAlign, BitsPerSample, AudioStart);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while creating SpeechToTextAudioStream: " + ex.Message);
            }
            return stts;
        }
        private SpeechToTextAudioStream(
            uint Channels,
            uint SamplesPerSec,
            uint AvgBytesPerSec,
            uint BlockAlign,
            uint BitsPerSample,
            ulong AudioStart)
        {
            nChannels = Channels;
            nSamplesPerSec = SamplesPerSec;
            nAvgBytesPerSec = AvgBytesPerSec;
            nBlockAlign = BlockAlign;
            wBitsPerSample = BitsPerSample;
            audioStart = AudioStart;


            internalStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            inputStream = internalStream.GetInputStreamAt(0);
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
            throw new NotImplementedException();
        }

        public IInputStream GetInputStreamAt(ulong position)
        {
            System.Diagnostics.Debug.WriteLine("GetInputStreamAt: " + position.ToString());
            if(internalStream.Size> position)
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
            get {
                System.Diagnostics.Debug.WriteLine("Position: " + internalStream.Position.ToString());
                return internalStream.Position;
            }
        }

        public void Seek(ulong position)
        {
            //System.Diagnostics.Debug.WriteLine("Seek: " + position.ToString() + " - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position);
            internalStream.Seek(position);
        }

        public ulong Size
        {
            get
            {
                System.Diagnostics.Debug.WriteLine("Size: " + internalStream.Size.ToString());
                return internalStream.Size;
            }
            set
            {
                internalStream.Size = value;
            }
        }

        public void Dispose()
        {
            internalStream.Dispose();
        }
        public Windows.Foundation.IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run<IBuffer, uint>((token, progress) =>
            {
                return Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("ReadAsync for: " + count.ToString() + " bytes - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position);
                    inputStream.ReadAsync(buffer, count, options).AsTask().Wait();
                    System.Diagnostics.Debug.WriteLine("ReadAsync return : " + buffer.Length.ToString() + " bytes - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position);
                    progress.Report((uint)buffer.Length);
                    return buffer;
                });
            });
        }



        public Windows.Foundation.IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run<uint, uint>((token, progress) =>
            {
                return Task.Run(() =>
                {
                    // System.Diagnostics.Debug.WriteLine("WriteAsync: " + buffer.Length.ToString() + " at position: " + internalStream.Position);
                    internalStream.WriteAsync(buffer).AsTask().Wait();
                    progress.Report((uint)buffer.Length);
                    return (uint)buffer.Length;
                });
            });
            
        }
        public Windows.Foundation.IAsyncOperation<bool> FlushAsync()
        {
            return internalStream.FlushAsync();
        }
        public ulong startIndex
        {
            get
            {
                return audioStart;
            }
            set
            {
                audioStart = value;
            }
        }
        public ulong endIndex
        {
            get
            {
                return audioEnd;
            }
            set
            {
                audioEnd = value;
            }
        }
        public TimeSpan startTime
        {
            get
            {
                TimeSpan ts = new TimeSpan ((long)((audioStart*10000000)/nAvgBytesPerSec));
                return ts;
            }
        }
        public TimeSpan endTime
        {
            get
            {
                TimeSpan ts = new TimeSpan((long)(((audioEnd)* 10000000) / nAvgBytesPerSec));
                return ts;
            }
        }
    }
}
