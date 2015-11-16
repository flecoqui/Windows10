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
using System.IO;
using System.IO.IsolatedStorage;

namespace AdaptiveMediaCache
{
     class DiskCache
    {
        public DiskCache()
        {
            root = string.Empty;
            applicationData = null;
        }
        /// <summary>
        /// Used for synchronization
        /// </summary>
        /// 
        private readonly AsyncReaderWriterLock internalAudioDiskLock = new AsyncReaderWriterLock();
        private readonly AsyncReaderWriterLock internalVideoDiskLock = new AsyncReaderWriterLock();
        private readonly AsyncReaderWriterLock internalManifestDiskLock = new AsyncReaderWriterLock();

        public async Task<bool> Initialize(string Root)
        {
            root = Root;
            applicationData = Windows.Storage.ApplicationData.Current;
            return await CreateDirectory(Root);
        }
        private Windows.Storage.ApplicationData applicationData;
        private string root = "";
        private const string manifestFileName = "manifest.xml";
        private const string audioIndexFileName = "AudioIndex";
        private const string videoIndexFileName = "VideoIndex";
        private const string audioContentFileName = "Audio";
        private const string videoContentFileName = "Video";
        public const ulong indexSize = 20;

        /// <summary> Get object from isolated storage file.</summary>
        /// <param name="fullpath"> file name to retreive</param>
        /// <param name="type"> type of object to read</param>
        /// <returns> a <c>object</c> instance, or null if the operation failed.</returns>
        private async Task<object> GetObjectByType(string filepath, Type type)
        {
            object retVal = null;
            try
            {
                using (var releaser = await internalManifestDiskLock.ReaderLockAsync())
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Reader Enter for Uri: " + filepath.ToString());

                    byte[] bytes = await Restore(filepath);
                    if (bytes != null)
                    {
                        try
                        {
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                if (ms != null)
                                {
                                    System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(type);
                                    retVal = ser.ReadObject(ms);
                                }
                            }
                        }
                        catch(Exception e)
                        {

                            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Reader Exception for Uri: " + filepath.ToString() + " Exception: " + e.Message);
                        }
                    }
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Reader Exit for Uri: " + filepath.ToString());

                }
            }
            catch(Exception)
            {

            }
            return retVal;
        }

        /// <summary>
        /// Return a list of all urls in local storage
        /// </summary>
        public async Task<List<ManifestCache>> RestoreAllAssets(string pattern)
        {
            List<ManifestCache> downloads = new List<ManifestCache>();
            List<string> dirs = await GetDirectoryNames(root);
            if (dirs != null)
            {
                for (int i = 0; i < dirs.Count; i++)
                {
                    string path = Path.Combine(root, dirs[i]);
                    if (!string.IsNullOrEmpty(path))
                    {
                        string file = Path.Combine(path, manifestFileName);
                        if (!string.IsNullOrEmpty(file))
                        {
                            ManifestCache de = await GetObjectByType(file, typeof(ManifestCache)) as ManifestCache;
                            if (de != null)
                            {
                                // Sanity check are the manifest file and chunk files consistent
                                if ((de.AudioSavedChunks == (ulong)(await GetFileSize(Path.Combine(path, audioIndexFileName)) / indexSize)) &&
                                    (de.VideoSavedChunks == (ulong)(await GetFileSize(Path.Combine(path, videoIndexFileName)) / indexSize)))
                                {
                                    downloads.Add(de);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache - RestoreAllAssets - Manifest and Chunk file not consistent for path: " + path.ToString());
                                }
                            }
                        }
                    }
                }
            }
            return downloads;
        }
        /// <summary>
        /// Return a ManifestCache based on its Uri
        /// </summary>
        public async Task<ManifestCache> RestoreAsset(Uri uri)
        {
            List<string> dirs = await GetDirectoryNames(root);
            if (dirs != null)
            {
                for (int i = 0; i < dirs.Count; i++)
                {
                    string path = Path.Combine(root, dirs[i]);
                    if (!string.IsNullOrEmpty(path))
                    {
                        string file = Path.Combine(path, manifestFileName);
                        if (!string.IsNullOrEmpty(file))
                        {

                            ManifestCache de = await GetObjectByType(file, typeof(ManifestCache)) as ManifestCache;
                            if (de != null)
                            {
                                if (de.ManifestUri == uri)
                                {
                                    return de;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// SaveManifest
        /// Save manifest on disk 
        /// </summary>
        public async Task<bool> SaveManifest(ManifestCache cache)
        {
            bool bResult = false;
            using (var releaser = await internalManifestDiskLock.WriterLockAsync())
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer Enter for Uri: " + cache.ManifestUri.ToString());
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        if (ms != null)
                        {
                            System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(typeof(ManifestCache));
                            ser.WriteObject(ms, cache);
                            bResult = await Save(Path.Combine(Path.Combine(root, cache.StoragePath), manifestFileName), ms.ToArray());
                        }
                    }
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer exception for Uri: " + cache.ManifestUri.ToString() + " Exception: " + e.Message);

                }
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer Exit for Uri: " + cache.ManifestUri.ToString());
            }
            return bResult;
        }
        /// <summary>
        /// SaveAudioChunks
        /// Save audio chunks on disk 
        /// </summary>
        public async Task<bool> SaveAudioChunks(ManifestCache cache)
        {
            bool bResult = false;
            // Saving Audio and Video chunks 
            string AudioIndexFile = Path.Combine(Path.Combine(root, cache.StoragePath), audioIndexFileName);
            string AudioContentFile = Path.Combine(Path.Combine(root, cache.StoragePath), audioContentFileName);
            if ((!string.IsNullOrEmpty(AudioIndexFile)) &&
                    (!string.IsNullOrEmpty(AudioContentFile)))
            {
                using (var releaser = await internalAudioDiskLock.WriterLockAsync())
                {
                    ulong AudioOffset = await GetFileSize(AudioContentFile);
                    ulong InitialAudioOffset = AudioOffset;
                    // delete the initial files
                    /*
                    await DeleteFile(AudioIndexFile);
                    await DeleteFile(AudioContentFile);
                    cache.AudioSavedChunks = 0;
                    */
                    for (int Index = (int)cache.AudioSavedChunks; Index < (int)cache.AudioDownloadedChunks; Index++)
                    {
                        var cc = cache.AudioChunkList[Index];
                        if ((cc != null) && (cc.GetLength() > 0))
                        {
                            IndexCache ic = new IndexCache(cc.Time, AudioOffset, cc.GetLength());
                            if (ic != null)
                            {
                                ulong res = await Append(AudioContentFile, cc.chunkBuffer);
                                if (res == cc.GetLength())
                                {
                                    AudioOffset += res;
                                    ulong result = await Append(AudioIndexFile, ic.GetByteData());
                                    if (result == indexSize)
                                    {
                                        cache.AudioSavedChunks++;
                                        cache.AudioSavedBytes += res;
                                        // Free buffer
                                        cc.chunkBuffer = null;
                                    }
                                }
                            }
                        }
                        else
                            break;
                    }
                    if (InitialAudioOffset < AudioOffset)
                        bResult = true;
                     if(cache.AudioSavedChunks == cache.AudioDownloadedChunks)
                        bResult = true;

                }
            }

            return bResult;
        }

        /// <summary>
        /// SaveVideoChunks
        /// Save video chunks on disk 
        /// </summary>
        public async Task<bool> SaveVideoChunks(ManifestCache cache)
        {
            bool bResult = false;
            string VideoIndexFile = Path.Combine(Path.Combine(root, cache.StoragePath), videoIndexFileName);
            string VideoContentFile = Path.Combine(Path.Combine(root, cache.StoragePath), videoContentFileName);
            if ((!string.IsNullOrEmpty(VideoIndexFile)) &&
                    (!string.IsNullOrEmpty(VideoContentFile)))
            {
                using (var releaser = await internalVideoDiskLock.WriterLockAsync())
                {
                    ulong VideoOffset = await GetFileSize(VideoContentFile);
                    ulong InitialVideoOffset = VideoOffset;

                    // delete the initial files
                    /*
                    await DeleteFile(VideoIndexFile);
                    await DeleteFile(VideoContentFile);
                    cache.VideoSavedChunks = 0;
                    */
                    for (int Index = (int)cache.VideoSavedChunks; Index < (int)cache.VideoDownloadedChunks; Index++)
                    //foreach (var cc in cache.VideoChunkList)
                    {
                        var cc = cache.VideoChunkList[Index];
                        if ((cc != null) && (cc.GetLength() > 0))
                        {
                            IndexCache ic = new IndexCache(cc.Time, VideoOffset, cc.GetLength());
                            if (ic != null)
                            {
                                ulong res = await Append(VideoContentFile, cc.chunkBuffer);
                                if (res == cc.GetLength())
                                {
                                    VideoOffset += res;
                                    ulong result = await Append(VideoIndexFile, ic.GetByteData());
                                    if (result == indexSize)
                                    {
                                        cache.VideoSavedChunks++;
                                        cache.VideoSavedBytes += res;
                                        // free buffer
                                        cc.chunkBuffer = null;
                                    }
                                }
                            }
                        }
                        else
                            break;
                    }
                if (InitialVideoOffset < VideoOffset)
                    bResult = true;
                    if (cache.VideoSavedChunks == cache.VideoDownloadedChunks)
                        bResult = true;

                }
            }

            return bResult;
        }
        /// <summary>
        /// RemoveAudioChunks
        /// Remove audio chunks from disk 
        /// </summary>
        public async Task<bool> RemoveAudioChunks(ManifestCache cache)
        {
            bool bResult = false;
            string pathContent = Path.Combine(Path.Combine(root, cache.StoragePath), audioContentFileName);
            string pathIndex = Path.Combine(Path.Combine(root, cache.StoragePath), audioIndexFileName);
            using (var releaser = await internalAudioDiskLock.WriterLockAsync())
            {
                if (pathContent != null)
                {
                    bool res = await FileExists(pathContent);
                    if (res)
                        bResult = await DeleteFile(pathContent);
                }
                if (pathIndex != null)
                {
                    bool res = await FileExists(pathIndex);
                    if (res)
                        bResult = await DeleteFile(pathIndex);
                }
            }
            return bResult;
        }
        /// <summary>
        /// RemoveVideoChunks
        /// Remove video chunks from disk 
        /// </summary>
        public async Task<bool> RemoveVideoChunks(ManifestCache cache)
        {
            bool bResult = false;
            string pathContent = Path.Combine(Path.Combine(root, cache.StoragePath), videoContentFileName);
            string pathIndex = Path.Combine(Path.Combine(root, cache.StoragePath), videoIndexFileName);
            using (var releaser = await internalVideoDiskLock.WriterLockAsync())
            {
                if (pathContent != null)
                {
                    bool res = await FileExists(pathContent);
                    if (res)
                        bResult = await DeleteFile(pathContent);
                }
                if (pathIndex != null)
                {
                    bool res = await FileExists(pathIndex);
                    if (res)
                        bResult = await DeleteFile(pathIndex);
                }
            }
            return bResult;
        }
        /// <summary>
        /// RemoveManifest
        /// Remove manifest from disk 
        /// </summary>
        public async Task<bool> RemoveManifest(ManifestCache cache)
        {
            bool bResult = false;
            string path = Path.Combine(Path.Combine(root, cache.StoragePath), manifestFileName);
            if (path != null)
            {
                bool res = await FileExists(path);
                if (res)
                    bResult = await DeleteFile(path);
            }
            return bResult;
        }
        /// <summary>
        /// GetCacheSize
        /// Return the current size of the cache on disk: adding the size of each asset 
        /// </summary>
        public async Task<ulong> GetCacheSize()
        {
            ulong Val = 0;
            List<string> dirs = await GetDirectoryNames(root);
            if (dirs != null)
            {
                for (int i = 0; i < dirs.Count; i++)
                {
                    string path = Path.Combine(root, dirs[i]);
                    if (!string.IsNullOrEmpty(path))
                    {
                        string file = Path.Combine(path, manifestFileName);
                        if (!string.IsNullOrEmpty(file))
                        {
                            ManifestCache de = await GetObjectByType(file, typeof(ManifestCache)) as ManifestCache;
                            if (de != null)
                            {
                                Val += await GetAssetSize(de);
                            }
                        }
                    }
                }
            }
            return Val;

        }
        /// <summary>
        /// GetAssetSize
        /// Return the current asset size on disk: audio chunks, video chunks and manifest 
        /// </summary>
        public async Task<ulong> GetAssetSize(ManifestCache cache)
        {
            ulong val = 0;
            string path = string.Empty;
            path = Path.Combine(Path.Combine(root, cache.StoragePath), manifestFileName);
            if (!string.IsNullOrEmpty(path))
                val += await GetFileSize(path);

            using (var releaser = await internalVideoDiskLock.ReaderLockAsync())
            {
                path = Path.Combine(Path.Combine(root, cache.StoragePath), videoIndexFileName);
                if (!string.IsNullOrEmpty(path))
                    val += await GetFileSize(path);
                path = Path.Combine(Path.Combine(root, cache.StoragePath), videoContentFileName);
                if (!string.IsNullOrEmpty(path))
                    val += await GetFileSize(path);
            }

            using (var releaser = await internalAudioDiskLock.ReaderLockAsync())
            {
                path = Path.Combine(Path.Combine(root, cache.StoragePath), audioIndexFileName);
                if (!string.IsNullOrEmpty(path))
                    val += await GetFileSize(path);
                path = Path.Combine(Path.Combine(root, cache.StoragePath), audioContentFileName);
                if (!string.IsNullOrEmpty(path))
                    val += await GetFileSize(path);
            }
            return val;
        }
        /// <summary>
        /// RemoveAsset
        /// Remove the asset on disk: audio chunks, video chunks and manifest 
        /// </summary>
        public async Task<bool> RemoveAsset(ManifestCache cache)
        {
            bool bResult = false;
            await RemoveAudioChunks(cache);
            await RemoveVideoChunks(cache);
            await RemoveManifest(cache);
            bResult = await RemoveDirectory(Path.Combine(root, cache.StoragePath));
            return bResult;
        }
        /// <summary>
        /// SaveAsset
        /// Save the asset on disk: audio chunks, video chunks and manifest 
        /// </summary>
        public async Task<bool> SaveAsset(ManifestCache cache)
        {
            bool bResult = true;

            if (!(await SaveAudioChunks(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while saving audio chunks for url: " + cache.ManifestUri.ToString());
            }
            if (!(await SaveVideoChunks(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while saving video chunks for url: " + cache.ManifestUri.ToString());
            }
            if (!(await SaveManifest(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while saving manifest chunks for url: " + cache.ManifestUri.ToString());
            }
            return bResult;
        }
        /// <summary>
        /// GetChunkBuffer
        /// Return the chunk buffer from the disk  
        /// </summary>
        private async Task<byte[]> GetChunkBuffer(bool isVideo, string path, ulong time)
        {
            byte[] buffer = null;
            string dir = Path.Combine(root, path);
            if (!string.IsNullOrEmpty(dir))
            {
                string indexFile = Path.Combine(dir, (isVideo == true ? videoIndexFileName : audioIndexFileName));
                string contentFile = Path.Combine(dir, (isVideo == true ? videoContentFileName : audioContentFileName));
                if ((!string.IsNullOrEmpty(contentFile))&&
                    (!string.IsNullOrEmpty(indexFile)))
                {

                    using (var releaser = (isVideo == true ? await internalVideoDiskLock.ReaderLockAsync(): await internalAudioDiskLock.ReaderLockAsync()))
                    {
                        ulong offset = 0;
                        ulong size = 20;
                        ulong fileSize = await GetFileSize(indexFile);
                        while (offset < fileSize)
                        {
                            byte[] b = await Restore(indexFile, offset, size);
                            IndexCache ic = new IndexCache(b);
                            if (ic != null)
                            {
                                if (ic.Time == time)
                                {
                                    buffer = await Restore(contentFile, ic.Offset, ic.Size);
                                    break;
                                }
                            }
                            offset += size;
                        }
                    }
                }
            }
            return buffer;
        }
        /// <summary>
        /// Return Audio chunk from disk with Uri and Time
        /// </summary>
        public  byte[] GetAudioChunkBuffer(string path, ulong time)
        {
            var t =  GetChunkBuffer(false, path, time);
            t.Wait();
            if(t.IsCompleted)
                return t.Result;
            return null;
        }
        /// <summary>
        /// Return Audio chunk from disk with Uri and Time
        /// </summary>
        public  byte[] GetVideoChunkBuffer(string path, ulong time)
        {
            var t = GetChunkBuffer(true, path, time);
            t.Wait();
            if(t.IsCompleted)
                return t.Result;
            return null;
        }

        //private Windows.Storage.ApplicationData storage;
        /// <summary>
        /// returns available size in bytes
        /// </summary>
        /// <returns></returns>
        public async Task<ulong> GetAvailableSize()
        {
            var retrivedProperties = await Windows.Storage.ApplicationData.Current.LocalFolder.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace" });
            return (UInt64)retrivedProperties["System.FreeSpace"];
        }

        /// <summary>
        /// returns a list of directory names for the 
        /// specified directory.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public async Task<List<string>> GetDirectoryNames(string directoryPath)
        {
            List<string> listDirectory = new List<string>(); 
            try
            {
                var folder = await applicationData.LocalFolder.GetFolderAsync(directoryPath);
                if(folder != null)
                {
                    IReadOnlyList<Windows.Storage.StorageFolder> list = await folder.GetFoldersAsync();
                    if (list != null)
                        foreach (var fold in list)
                            listDirectory.Add(fold.Name);
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("GetDirectoryNames: " + e.Message);
            }
            return listDirectory;
        }


        /// <summary>
        /// returns a list of filenames for the 
        /// specified directory.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public async Task<List<string>> GetFileNames(string directoryPath, string pattern)
        {
            List<string> listFile = new List<string>();
            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add(pattern);

            Windows.Storage.Search.QueryOptions queryOptions = new Windows.Storage.Search.QueryOptions(Windows.Storage.Search.CommonFileQuery.OrderBySearchRank, fileTypeFilter);
            queryOptions.UserSearchFilter = directoryPath;
            var fileQuery = applicationData.LocalFolder.CreateFileQueryWithOptions(queryOptions);

            var list = await fileQuery.GetFilesAsync();
            if (list != null)
                foreach (var file in list)
                    listFile.Add(file.Path);
            return listFile;
        }

        /// <summary>
        /// deletes the specified file
        /// </summary>
        /// <param name="fullPath"></param>        
        public async Task<bool> DeleteFile(string fullPath)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile FileExists for path: " + fullPath);
            bool bRes = await FileExists(fullPath);
            if (bRes != true)
                return true;
            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile Delete for path: " + fullPath);
                var file = await applicationData.LocalFolder.GetFileAsync(fullPath);
                if (file != null)
                {
                    try
                    {
                        await file.DeleteAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile for path: " + fullPath + " exception 1: " + ex.Message);

                    }
                }
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile Delete done for path: " + fullPath);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile for path: " +fullPath+" exception 2: " + e.Message);
            }
            return true;
        }
        /// <summary>
        /// IsDirectoryExist
        /// </summary>
        public async Task<bool> IsDirectoryExist(string directoryPath)
        {
            bool bResult = false;
            try
            {
                var directory = await applicationData.LocalFolder.GetFolderAsync(directoryPath);
                if (directory != null)
                {
                    bResult = true;
                }
            }
            catch (Exception)
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// creates the specified directory
        /// </summary>
        /// <param name="fullPath"></param>  
        public async Task<bool> CreateDirectory(string directoryPath)
        {
            bool bResult = false;
            try
            {
                bool result = await IsDirectoryExist(directoryPath);
                if (true != result)
                {
                    var folder = await applicationData.LocalFolder.CreateFolderAsync(directoryPath);
                    if(folder != null)
                        bResult = true;
                }
                else
                    bResult = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("CreateDirectory - Exception: " + e.Message);
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// Gets the size of the file specified in bytes
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<ulong> GetFileSize(string filePath)
        {
            ulong len = 0;
            bool bRes = await FileExists(filePath);
            if (bRes != true)
                return len;

            var file = await applicationData.LocalFolder.GetFileAsync(filePath);
            if (file != null)
            {
                var pro = await file.GetBasicPropertiesAsync();
                if(pro != null)
                    len = pro.Size;
                
            }
            return len;
        }
        /// <summary>
        /// Gets the size of the storage in bytes
        /// </summary>
        /// <returns></returns>
        public async Task<ulong> GetStorageSize()
        {
            ulong len = 0;
            var pro = await applicationData.LocalFolder.GetBasicPropertiesAsync();
            if (pro != null)
                len = pro.Size;
            //applicationData.LocalFolder
            return len;
            //return storage.Quota;
        }
        /// <summary>
        /// reads the data from the specified file and 
        /// writes them to a byte[] buffer
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public Task<byte[]> Restore(string fullPath)
        {
            return Restore(fullPath, 0, 0);
        }

        /// <summary>
        /// reads the data from the specified file with the offset as the starting point the size
        /// and writes them to a byte[] buffer
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public async Task<byte[]> Restore(string fullPath, ulong offset, ulong size)
        {
            byte[] dataArray = null;

            try
            {
                Windows.Storage.StorageFile file = await applicationData.LocalFolder.GetFileAsync(fullPath);
                if(file!=null)
                {
                    if (size <= 0)
                    {
                        var prop = await file.GetBasicPropertiesAsync();
                        if(prop!= null)
                            size = prop.Size;
                    }
                    var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    if (stream != null)
                    {
                        using (var inputStream = stream.GetInputStreamAt(offset))
                        {
                            using (Windows.Storage.Streams.DataReader dataReader = new Windows.Storage.Streams.DataReader(inputStream))
                            {
                                uint numBytesLoaded = await dataReader.LoadAsync((uint)size);
                                dataArray = new byte[numBytesLoaded];
                                if (dataArray != null)
                                {
                                    dataReader.ReadBytes(dataArray);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception )
            {
            }
            return dataArray;
        }

        /// <summary>
        /// Append data to isolated storage file and return the size of the current file
        /// </summary>
        /// <param name="fullPath">String representation of the path to isolated storage file</param>
        /// <param name="data">MemoryStream containing data to append isolated storage file with</param>
        public async Task<ulong> Append(string fullPath, byte[] data)
        {
            ulong retVal = 0;
            bool bRes = await FileExists(fullPath);
            try
            {
                Windows.Storage.StorageFile file;
                if (bRes != true)
                    file = await applicationData.LocalFolder.CreateFileAsync(fullPath);
                else
                    file = await applicationData.LocalFolder.GetFileAsync(fullPath);
                if (file != null)
                {
                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        if (stream != null)
                        {
                            stream.Seek(0, SeekOrigin.End);
                            await stream.WriteAsync(data, 0, data.Length);
                            retVal =  (ulong) data.Length;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return retVal;
        }
        private Windows.Storage.Streams.IBuffer GetBufferFromString(String str)
        {
            using (Windows.Storage.Streams.InMemoryRandomAccessStream memoryStream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
            {
                using (Windows.Storage.Streams.DataWriter dataWriter = new Windows.Storage.Streams.DataWriter(memoryStream))
                {
                    dataWriter.WriteString(str);
                    return dataWriter.DetachBuffer();
                }
            }
        }
        private Windows.Storage.Streams.IBuffer GetBufferFromBytes(byte[] str)
        {
            using (Windows.Storage.Streams.InMemoryRandomAccessStream memoryStream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
            {
                using (Windows.Storage.Streams.DataWriter dataWriter = new Windows.Storage.Streams.DataWriter(memoryStream))
                {
                    dataWriter.WriteBytes(str);
                    return dataWriter.DetachBuffer();
                }
            }
        }
        /// <summary>
        /// Saves the provided data to the file
        /// specified
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> Save(string fullPath, byte[] data)
        { 
            bool retVal = false;
           // System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save DeleteFile for Path: " + fullPath);
           // await DeleteFile(fullPath);
            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save Saving for Path: " + fullPath);
                Windows.Storage.StorageFile file = await applicationData.LocalFolder.CreateFileAsync(fullPath, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                if (file != null)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save OpenStreamForWriteAsync for Path: " + fullPath);
                    using (var s = await file.OpenStreamForWriteAsync())
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save WriteAsync for Path: " + fullPath);
                        await s.WriteAsync(data, 0, data.Length);
                    }
                    /*
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save Saving GetBufferFromBytes for Path: " + fullPath);
                    Windows.Storage.Streams.IBuffer buffer = GetBufferFromBytes(data);
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save Saving WriteBufferAsync for Path: " + fullPath);
                    await Windows.Storage.FileIO.WriteBufferAsync(file, buffer);
                    */
                    retVal = true;
                }
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save Saving done for Path: " + fullPath);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save Saving exception for Path: " + fullPath + " Exception: " + e.Message);
            }

            return retVal;
        }

        /// <summary>
        /// Truncate file from isolated storage file to specific size
        /// </summary>
        /// <param name="fullPath">String representation of the path to isolated storage file</param>
        /// <param name="size">bytes of file from starting postion to be kept</param>
        public async Task<bool> Truncate(string fullPath, long size)
        {
            bool bResult = false;
            if (size <= 0) return bResult;

            bool bRes = await FileExists(fullPath);
            if (bRes != true)
                return bResult;

            try
            {
                Windows.Storage.StorageFile file = await applicationData.LocalFolder.GetFileAsync(fullPath);
                if (file != null)
                {
                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        if (stream != null)
                        {
                            stream.SetLength(size);
                            bResult = true;
                        }
                    }
                }
            }
            catch(Exception )
            {

            }
            return bResult;
        }

        /// <summary>
        /// returns true, if the specified file exists otherwise false
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>

        public async Task<bool> FileExists(string fullPath)
        {
            bool bResult = false;
            try
            {
                var directory = await applicationData.LocalFolder.GetFileAsync(fullPath);
                if (directory != null)
                {
                    bResult = true;
                }
            }
            catch (Exception)
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// removes the specified directory
        /// </summary>
        /// <param name="path"></param>
        public async Task<bool> RemoveDirectory(string fullPath)
        {
            bool bRes = await DirectoryExists(fullPath);
            if (bRes == true)
            {
                Windows.Storage.StorageFolder folder = await applicationData.LocalFolder.GetFolderAsync(fullPath);
                if (folder != null)
                {
                    await folder.DeleteAsync();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// returns true if the specified directory exists otherwise false
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public async Task<bool> DirectoryExists(string fullPath)
        {
            bool bResult = false;
            try
            {
                var directory = await applicationData.LocalFolder.GetFolderAsync(fullPath);
                if (directory != null)
                {
                    bResult = true;
                }
            }
            catch (Exception )
            {
                bResult = false;
            }
            return bResult;

        }
    }
}
