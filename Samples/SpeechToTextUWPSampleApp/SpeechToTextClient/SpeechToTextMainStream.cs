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
using System.Collections.Concurrent;

namespace SpeechToTextClient
{
    /// <summary>
    /// class Chunk
    /// </summary>
    /// <info>
    /// Class used to read the Chunk in the WAV file header 
    /// (for instance:"fmt ", "data", "JUNK" chunks).
    /// </info>
    class Chunk
    {
        public byte[] tag;
        public uint length;
        public byte[] data;

        public Chunk()
        {
            tag = null;
            length = 0;
            data = null;
        }
        public Chunk(byte[] Tag, uint Length, byte[] Data)
        {
            if (Tag != null)
            {
                this.tag = new byte[Tag.Length];
                for (int i = 0; i < Tag.Length; i++)
                    this.tag[i] = Tag[i];
            }
            else
                this.tag = Tag;

            this.length = Length;

            if (Data != null)
            {
                this.data = new byte[Data.Length];
                for (int j = 0; j < Data.Length; j++)
                    this.data[j] = Data[j];
            }

        }
    }
    /// <summary>
    /// class WAVAttributes
    /// </summary>
    /// <info>
    /// Static Class with Guid used to parse the WAV header
    /// </info>
    static class WAVAttributes
    {
        /// <summary>
        /// Audio block alignment
        /// </summary>
        public static readonly Guid MF_MT_AUDIO_BLOCK_ALIGNMENT = new Guid("322de230-9eeb-43bd-ab7a-ff412251541d");
        /// <summary>
        /// Audio number of channels
        /// </summary>
        public static readonly Guid MF_MT_AUDIO_NUM_CHANNELS = new Guid("37e48bf5-645e-4c5b-89de-ada9e29b696a");
        /// <summary>
        /// Media type Major Type
        /// </summary>
        public static readonly Guid MF_MT_MAJOR_TYPE = new Guid("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
        /// <summary>
        /// Audio samples per second
        /// </summary>
        public static readonly Guid MF_MT_AUDIO_SAMPLES_PER_SECOND = new Guid("5faeeae7-0290-4c31-9e8a-c534f68d9dba");
        /// <summary>
        /// Specifies for a media type whether each sample is independent of the other samples in the stream. 
        /// </summary>
        public static readonly Guid MF_MT_ALL_SAMPLES_INDEPENDENT = new Guid("c9173739-5e56-461c-b713-46fb995cb95f");
        /// <summary>
        /// Audio bits per sample
        /// </summary>
        public static readonly Guid MF_MT_AUDIO_BITS_PER_SAMPLE = new Guid("f2deb57f-40fa-4764-aa33-ed4f2d1ff669");
        /// <summary>
        /// Media Type subtype
        /// </summary>
        public static readonly Guid MF_MT_SUBTYPE = new Guid("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
        /// <summary>
        /// Audio average bytes per second
        /// </summary>
        public static readonly Guid MF_MT_AUDIO_AVG_BYTES_PER_SECOND = new Guid("1aab75c8-cfef-451c-ab95-ac034b8e1731");
    }
    public delegate void StreamAudioLevelEventHandler(SpeechToTextMainStream sender, double reading);

    public delegate void StreamBufferReadyEventHandler(SpeechToTextMainStream sender);
    /// <summary>
    /// class SpeechToTextMainStream
    /// </summary>
    /// <info>
    /// Class used to:
    /// - record the audio samples in StorageFile
    /// - forward the audio samples towards the SpeechToText REST API
    /// - extract audio buffer based on the audio level and measurement duration. 
    ///   Those audio buffers are stored in a queue      
    /// This class automatically update the WAV Header based on the length 
    /// of data to store or transmit.
    /// Moreover, it removes the JUNK chunk from the WAV header.
    /// </info>
    public class SpeechToTextMainStream : IRandomAccessStream
    {
        private Chunk fmt = null;
        private Chunk data = null;
        private uint nChannels;
        private uint nSamplesPerSec;
        private uint nAvgBytesPerSec;
        private uint nBlockAlign;
        private uint wBitsPerSample;
        private ulong wavHeaderLength = 0;
        private ulong ReadDataIndex = 0;
        private ulong WriteDataIndex = 0;
        private ulong seekOffset = 0;
        private Object maxSizeLock = new Object();
        private ulong maxSize = 0;
        private uint thresholdDurationInBytes = 0;
        private uint tresholdDuration = 0;
        private uint thresholdLevel = 0;
        private ulong thresholdStart = 0;
        private ulong thresholdEnd = 0;
        private SpeechToTextAudioStream audioStream;
        private Windows.Storage.Streams.IRandomAccessStream internalStream;
        private Windows.Storage.Streams.IInputStream inputStream;
        private ConcurrentQueue<SpeechToTextAudioStream> audioQueue;
        public static SpeechToTextMainStream Create(ulong maxSizeInBytes, uint thresholdDuration, uint thresholdLevel)
        {
            SpeechToTextMainStream stts = null;
            try
            {
                stts = new SpeechToTextMainStream(maxSizeInBytes, thresholdDuration, thresholdLevel);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while creating SpeechToTextMainStream: " + ex.Message);
            }
            return stts;
        }
        private SpeechToTextMainStream(ulong MaxSizeInBytes = 0, uint ThresholdDuration = 0,uint ThresholdLevel = 0)
        {
            tresholdDuration = ThresholdDuration;
            thresholdDurationInBytes = 0;
            thresholdLevel = ThresholdLevel;
            maxSize = MaxSizeInBytes;

            thresholdStart = 0;
            thresholdEnd = 0;
            audioStream = null;
            internalStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            inputStream = internalStream.GetInputStreamAt(0);
            audioQueue = new ConcurrentQueue<SpeechToTextAudioStream>();
        }
        public ulong GetLength()
        {
            uint delta = 0;
            uint minwavHeaderLength = 4 + fmt.length + 8 + 8 + 8;
            if (wavHeaderLength > minwavHeaderLength)
                delta = (uint)wavHeaderLength - minwavHeaderLength;

            if (internalStream.Size  >= wavHeaderLength)
                return internalStream.Size - delta;
            return internalStream.Size;
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
            if (position >= seekOffset)
                position = position - seekOffset ;
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
        public ulong MaxSize
        {
            get
            {
                return maxSize;
            }
            set
            {
                maxSize = value;
            }
        }
        public void Dispose()
        {
            internalStream.Dispose();
        }
        public byte[] CreateWAVHeaderBuffer(uint Len)
        {
            uint headerLen = 4 + fmt.length + 8 + 8 + 8;
            byte[] updatedBuffer = new byte[headerLen];
            if (updatedBuffer != null)
            {
                System.Text.UTF8Encoding.UTF8.GetBytes("RIFF").CopyTo(0, updatedBuffer.AsBuffer(), 0, 4);
                BitConverter.GetBytes(4 + fmt.length + 8 + Len + 8).CopyTo(0, updatedBuffer.AsBuffer(), 4, 4);
                System.Text.UTF8Encoding.UTF8.GetBytes("WAVE").CopyTo(0, updatedBuffer.AsBuffer(), 8, 4);
                System.Text.UTF8Encoding.UTF8.GetBytes("fmt ").CopyTo(0, updatedBuffer.AsBuffer(), 12, 4);
                BitConverter.GetBytes(fmt.length).CopyTo(0, updatedBuffer.AsBuffer(), 16, 4);
                fmt.data.CopyTo(0, updatedBuffer.AsBuffer(), 20, (int)fmt.length);
                System.Text.UTF8Encoding.UTF8.GetBytes("data").CopyTo(0, updatedBuffer.AsBuffer(), 20 + fmt.length, 4);
                BitConverter.GetBytes(Len).CopyTo(0, updatedBuffer.AsBuffer(), 24 + fmt.length, 4);
            }
            return updatedBuffer;
        }

        public Windows.Foundation.IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run<IBuffer, uint>((token, progress) =>
            {
                return Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("ReadAsync for: " + count.ToString() + " bytes - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position);
                    // If first Read call
                    if ((ReadDataIndex == 0)&&(internalStream.Size>count))
                    {
                        // First dummy read of the header
                        inputStream = internalStream.GetInputStreamAt(wavHeaderLength);
                        uint currentDataLength = (uint)(internalStream.Size - wavHeaderLength);
                        if (currentDataLength > 0)
                        {
                            data.length = currentDataLength;
                            var WAVHeaderBuffer = CreateWAVHeaderBuffer(data.length);
                            if (WAVHeaderBuffer != null)
                            {
                                int headerLen = WAVHeaderBuffer.Length;
                                if (count >= headerLen)
                                {
                                    byte[] updatedBuffer = new byte[count];
                                    WAVHeaderBuffer.CopyTo(updatedBuffer.AsBuffer());
                                    if (count > headerLen)
                                    {
                                        //fill buffer
                                        inputStream.ReadAsync(updatedBuffer.AsBuffer((int)headerLen, (int)(count - headerLen)), (uint)(count - headerLen), options).AsTask().Wait();
                                    }

                                    buffer = updatedBuffer.AsBuffer();
                                    ReadDataIndex += buffer.Length;
                                    System.Diagnostics.Debug.WriteLine("ReadAsync return : " + buffer.Length.ToString() + " bytes - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position);
                                    progress.Report((uint)buffer.Length);
                                    return updatedBuffer.AsBuffer();
                                }
                            }
                        }
                    }
                    else
                    {
                        inputStream.ReadAsync(buffer, count, options).AsTask().Wait();
                        ReadDataIndex += buffer.Length;
                        System.Diagnostics.Debug.WriteLine("ReadAsync return : " + buffer.Length.ToString() + " bytes - Stream Size: " + internalStream.Size + " Stream position: " + internalStream.Position);
                        progress.Report((uint)buffer.Length);
                        return buffer;
                    }
                    return null;
                });
            });
        }



        public Windows.Foundation.IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run<uint, uint>((token, progress) =>
            {
                return Task.Run(() =>
                {

                    // If it's the first WriteAsync in the stream
                    // the buffer should contains the WAV Header
                    if((internalStream.Size==0) && (wavHeaderLength == 0) )
                    {
                        WriteDataIndex = 0;
                        // Check header
                        byte[] array = buffer.ToArray();
                        wavHeaderLength = ParseAndGetWAVHeaderLength(array);
                        internalStream.WriteAsync(buffer).AsTask().Wait();
                        WriteDataIndex += buffer.Length;
                        progress.Report((uint)(buffer.Length));
                        return (uint)(buffer.Length);
                    }
                    else
                    {
                        if(internalStream.Position != internalStream.Size)
                            System.Diagnostics.Debug.WriteLine("Warning WriteAsync: " + internalStream.Position.ToString() + "/" + internalStream.Size.ToString());

                        ulong index = internalStream.Size;
                        uint byteToWrite = buffer.Length;
                        
                       // System.Diagnostics.Debug.WriteLine("WriteAsync: " + buffer.Length.ToString() + " at position: " + internalStream.Position);
                        internalStream.WriteAsync(buffer.ToArray(0, (int)byteToWrite).AsBuffer()).AsTask().Wait();
                        WriteDataIndex += buffer.Length;
                        var byteArray = buffer.ToArray();
                        if (byteArray.Length >= 2)
                        {
                            var amplitude = Decode(byteArray).Select(Math.Abs).Average(x => x);
                            if (AudioLevel != null)
                                this.AudioLevel(this, amplitude);

                            // Currently the level is too low
                            if (thresholdDurationInBytes > 0)
                            {
                                if (audioStream == null)
                                {
                                    if (internalStream.Size > thresholdDurationInBytes)
                                    {
                                        var readStream = internalStream.GetInputStreamAt(internalStream.Size - thresholdDurationInBytes);
                                        byte[] readBuffer = new byte[thresholdDurationInBytes];
                                        readStream.ReadAsync(readBuffer.AsBuffer(), (uint)thresholdDurationInBytes, InputStreamOptions.None).AsTask().Wait();
                                        var level = Decode(readBuffer).Select(Math.Abs).Average(x => x);
                                        if (level > thresholdLevel)
                                        {
                                            System.Diagnostics.Debug.WriteLine("Audio Level sufficient to start recording");
                                            thresholdStart = WriteDataIndex - thresholdDurationInBytes;
                                            audioStream = SpeechToTextAudioStream.Create(nChannels, nSamplesPerSec, nAvgBytesPerSec, nBlockAlign, wBitsPerSample, thresholdStart);
                                            var headerBuffer = CreateWAVHeaderBuffer(0);
                                            if ((audioStream != null) && (headerBuffer != null))
                                            {
                                                audioStream.WriteAsync(headerBuffer.AsBuffer()).AsTask().Wait();
                                                audioStream.WriteAsync(readBuffer.AsBuffer()).AsTask().Wait();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    audioStream.WriteAsync(buffer.ToArray(0, (int)byteToWrite).AsBuffer()).AsTask().Wait();
                                    var readStream = internalStream.GetInputStreamAt(internalStream.Size - thresholdDurationInBytes);
                                    byte[] readBuffer = new byte[thresholdDurationInBytes];
                                    readStream.ReadAsync(readBuffer.AsBuffer(), (uint)thresholdDurationInBytes, InputStreamOptions.None).AsTask().Wait();
                                    var level = Decode(readBuffer).Select(Math.Abs).Average(x => x);
                                    if (level < thresholdLevel)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Audio Level lower enough to stop recording");
                                        thresholdEnd = WriteDataIndex;
                                        audioStream.Seek(0);
                                        var headerBuffer = CreateWAVHeaderBuffer((uint)(thresholdEnd - thresholdStart));
                                        if (headerBuffer != null)
                                            audioStream.WriteAsync(headerBuffer.AsBuffer()).AsTask().Wait();
                                        if (audioQueue != null)
                                        {
                                            audioStream.endIndex = thresholdEnd;
                                            audioQueue.Enqueue(audioStream);
                                        }
                                        if (BufferReady != null)
                                        {
                                            this.BufferReady(this);
                                            if (audioStream != null)
                                            {
                                                audioStream = null;
                                            }
                                            thresholdStart = 0;
                                            thresholdEnd = 0;
                                        }
                                    }
                                }
                            }

                        }
                        if(maxSize>0)
                        {
                            // check maxSize
                            if((internalStream.Size>maxSize)&&(audioStream==null))
                            {
                                lock(maxSizeLock)
                                {
                                    byte[] headerBuffer = null;
                                    if (wavHeaderLength > 0)
                                    {
                                        // WAV header present
                                        headerBuffer = new byte[wavHeaderLength];
                                        inputStream = internalStream.GetInputStreamAt(0);
                                        inputStream.ReadAsync(headerBuffer.AsBuffer(), (uint)wavHeaderLength, InputStreamOptions.None).AsTask().Wait();
                                    }
                                    seekOffset += (internalStream.Size - wavHeaderLength);
                                    internalStream.Dispose();
                                    inputStream.Dispose();
                                    internalStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                                    if (headerBuffer != null)
                                        internalStream.WriteAsync(headerBuffer.AsBuffer()).AsTask().Wait();
                                    inputStream = internalStream.GetInputStreamAt(0);
                                }
                            }
                        }
                        if (internalStream.Position == internalStream.Size)
                            WriteDataIndex += buffer.Length;
                        progress.Report((uint)buffer.Length);
                        return (uint)buffer.Length;
                    }
                });
            });
            
        }
        public Windows.Foundation.IAsyncOperation<bool> FlushAsync()
        {
            return internalStream.FlushAsync();
        }
        public SpeechToTextAudioStream GetAudioStream()
        {
            SpeechToTextAudioStream audioStream = null;
            if (audioQueue != null)
            {
                if (audioQueue.TryDequeue(out audioStream))
                    return audioStream;
            }
            return audioStream;
        }


        public event StreamAudioLevelEventHandler AudioLevel;

        public event StreamBufferReadyEventHandler BufferReady;

        #region private
        private IEnumerable<Int16> Decode(byte[] byteArray)
        {
            for (var i = 0; i < byteArray.Length - 1; i += 2)
            {
                yield return (BitConverter.ToInt16(byteArray, i));
            }
        }
        private uint ParseAndGetWAVHeaderLength(byte[] buffer)
        {

            int length = 0;
            uint source = 0;
            if (IsTag(buffer, "RIFF") == true)
            {
                source += 4;
                if (GetInt(buffer.AsBuffer().ToArray(source, 4), out length) == true)
                {
                    source += 4;
                    if (IsTag(buffer.AsBuffer().ToArray(source, 4), "WAVE") == true)
                    {
                        source += 4;
                        Chunk c = new Chunk();
                        while ((source + 8 <= buffer.Length) && (ReadChunkHeader(buffer.AsBuffer().ToArray(source, 8), c) == true))
                        {
                            source += 8;
                            if (IsTag(c.tag, "fmt ") == true)
                            {
                                fmt = new Chunk(c.tag, c.length, c.data);

                                if ((source + c.length < buffer.Length) && (ReadChunkData(buffer.AsBuffer().ToArray(source, (int)c.length), fmt) == true))
                                {
                                    nChannels = BitConverter.ToUInt16(fmt.data, 2);
                                    nSamplesPerSec = BitConverter.ToUInt32(fmt.data, 4);
                                    nAvgBytesPerSec = BitConverter.ToUInt32(fmt.data, 8);
                                    nBlockAlign = BitConverter.ToUInt16(fmt.data, 12);
                                    wBitsPerSample = BitConverter.ToUInt16(fmt.data, 14);
                                    thresholdDurationInBytes = (nAvgBytesPerSec * tresholdDuration) / 1000;
                                    source += c.length;
                                }
                                else
                                    return 0;
                            }
                            else if (IsTag(c.tag, "data") == true)
                            {
                                // Almost done
                                if (fmt.length > 0)
                                {
                                    //source += 8;
                                    data = new Chunk(c.tag, c.length, c.data);
                                    break;
                                }
                            }
                            else
                            {
                                source += c.length;
                            }
                        }
                    }
                }
            }
            return source;
        }
        static bool ReadChunkHeader(byte[] buffer, Chunk c)
        {
            if ((buffer != null) && (buffer.Length >= 8) && (c != null))
            {
                c.tag = new byte[4];
                buffer.CopyTo(0, c.tag.AsBuffer(), 0, 4);
                {
                    byte[] array = new byte[4];
                    buffer.CopyTo(4, array.AsBuffer(), 0, 4);
                    c.length = BitConverter.ToUInt32(array, 0);
                    if (c.length >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        static bool ReadChunkData(byte[] buffer, Chunk c)
        {
            if ((buffer != null) && (buffer.Length >= 8) && (c != null))
            {
                c.data = new byte[c.length];
                buffer.CopyTo(0, c.data.AsBuffer(), 0, (int)c.length);
                return true;
            }
            return false;
        }
        static bool ReadTag(System.IO.FileStream ifs, string tag)
        {
            if ((ifs != null) && (tag != null))
            {
                byte[] a = System.Text.UTF8Encoding.UTF8.GetBytes(tag);
                if (a != null)
                {
                    byte[] buffer = new byte[a.Length];
                    if (ifs.Read(buffer, 0, a.Length) == a.Length)
                    {
                        for (int i = 0; i < a.Length; i++)
                        {
                            if (a[i] != buffer[i])
                                return false;
                        }
                        return true;
                    };
                }
            }
            return false;
        }
        static bool IsTag(byte[] buffer, string tag)
        {
            if ((buffer != null) && (tag != null))
            {
                byte[] a = System.Text.UTF8Encoding.UTF8.GetBytes(tag);
                if (a != null)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        if (a[i] != buffer[i])
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }
        static bool GetInt(byte[] buffer, out int i)
        {
            i = 0;
            if ((buffer != null) && (buffer.Length >= 4))
            {
                i = BitConverter.ToInt32(buffer, 0);
                return true;
            }
            return false;
        }
        #endregion private


    }
}
