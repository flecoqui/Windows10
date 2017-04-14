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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace AudioVideoPlayer.DataModel
{
    /// <summary>
    /// Audio Video item data model.
    /// </summary>
    public class MediaItem
    {
        public MediaItem(String uniqueId, 
                              String comment,
                              String title,
                              String imagePath,
                              String description,
                              String content,
                              String posterContent,
                              long start,
                              long duration,
                              String httpHeaders,
                              String playReadyUrl,
                              String playReadyCustomData,
                              bool backgroundAudio)

        {
            this.UniqueId = uniqueId;
            this.Comment = comment;
            this.Title = title;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.PosterContent = posterContent;
            this.Start = start;
            this.Duration = duration;
            this.HttpHeaders = httpHeaders;
            this.PlayReadyUrl = playReadyUrl;
            this.PlayReadyCustomData = playReadyCustomData;
            this.BackgroundAudio = backgroundAudio;
        }
        public string UniqueId { get; private set; }
        public string Comment { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public string PosterContent { get; private set; }
        public long Start { get; private set; }
        public long Duration { get; private set; }
        public string HttpHeaders { get; private set; }
        public string PlayReadyUrl { get; private set; }
        public string PlayReadyCustomData { get; private set; }
        public bool BackgroundAudio { get; private set; }
        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Audio Video group data model.
    /// </summary>
    public class MediaDataGroup
    {
        public MediaDataGroup(String uniqueId,
                               String title,
                               String category,
                               String imagePath,
                               String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Category = category;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<MediaItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<MediaItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    class MediaDataSource
    {
        public static string MediaDataPath { get; private set; }

        private static MediaDataSource _MediaDataSource = new MediaDataSource();

        private ObservableCollection<MediaDataGroup> _groups = new ObservableCollection<MediaDataGroup>();
        public ObservableCollection<MediaDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<MediaDataGroup>> GetGroupsAsync(string path)
        {
            if(await _MediaDataSource.GetMediaDataAsync(path)== true)
                return _MediaDataSource.Groups;
            return null;
        }

        public static async Task<MediaDataGroup> GetGroupAsync(string path, string uniqueId)
        {
            if (await _MediaDataSource.GetMediaDataAsync(path) == true)
            {
                // Simple linear search is acceptable for small data sets
                var matches = _MediaDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
                if (matches.Count() == 1) return matches.First();
            }
            return null;
        }

        public static async Task<MediaItem> GetItemAsync(string path,  string uniqueId)
        {
            if (await _MediaDataSource.GetMediaDataAsync(path) == true)
            {
                // Simple linear search is acceptable for small data sets
                var matches = _MediaDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
                if (matches.Count() == 1) return matches.First();
            }
            return null;
        }
        public static void Clear()
        {
            if (_MediaDataSource._groups.Count != 0)
            {
                _MediaDataSource._groups.Clear();
            }

        }
        private async Task<bool> GetMediaDataAsync(string path)
        {
            if (this._groups.Count != 0)
                return false;
            string jsonText = string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                // load the default data
                //If retrieving json from web failed then use embedded json data file.
                if (string.IsNullOrEmpty(jsonText))
                {
                    Uri dataUri = new Uri("ms-appx:///DataModel/MediaData.json");

                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                    jsonText = await FileIO.ReadTextAsync(file);
                    MediaDataPath = "ms-appx:///DataModel/MediaData.json";
                }
            }
            else
            {
                if (path.StartsWith("ms-appx://"))
                {
                    Uri dataUri = new Uri(path);

                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                    jsonText = await FileIO.ReadTextAsync(file);
                    MediaDataPath = path;
                }
                else if (path.StartsWith("http://")|| path.StartsWith("https://"))
                {
                    try
                    {
                        //Download the json file from the server to configure what content will be dislayed.
                        //You can also modify the local MediaData.json file and delete this code block to test
                        //the local json file
                        string MediaDataFile = path;
                        Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
                        filter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;
                        Windows.Web.Http.HttpClient http = new Windows.Web.Http.HttpClient(filter);
                        Uri httpUri = new Uri(MediaDataFile);
                        jsonText = await http.GetStringAsync(httpUri);
                        MediaDataPath = MediaDataFile;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Exception while opening the playlist: " + path + " Exception: " + e.Message);
                    }
                }
                else
                {
                    try
                    {
                        //Download the json file from the server to configure what content will be dislayed.
                        //You can also modify the local MediaData.json file and delete this code block to test
                        //the local json file
                        string MediaDataFile = path;                        
                        StorageFile file;
                        file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
                        if (file != null)
                        {
                            jsonText = await FileIO.ReadTextAsync(file);
                            MediaDataPath = MediaDataFile;
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Exception while opening the playlist: " + path + " Exception: " + e.Message);
                    }
                }
            }

            if (string.IsNullOrEmpty(jsonText))
                return false;

            try
            {

                JsonObject jsonObject = JsonObject.Parse(jsonText);
                JsonArray jsonArray = jsonObject["Groups"].GetArray();

                foreach (JsonValue groupValue in jsonArray)
                {
                    JsonObject groupObject = groupValue.GetObject();
                    MediaDataGroup group = new MediaDataGroup(groupObject["UniqueId"].GetString(),
                                                                groupObject["Title"].GetString(),
                                                                groupObject["Category"].GetString(),
                                                                groupObject["ImagePath"].GetString(),
                                                                groupObject["Description"].GetString());

                    foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                    {
                        JsonObject itemObject = itemValue.GetObject();
                        long timeValue = 0;
                        group.Items.Add(new MediaItem(itemObject["UniqueId"].GetString(),
                                                           itemObject["Comment"].GetString(),
                                                           itemObject["Title"].GetString(),
                                                           itemObject["ImagePath"].GetString(),
                                                           itemObject["Description"].GetString(),
                                                           itemObject["Content"].GetString(),
                                                           itemObject["PosterContent"].GetString(),
                                                           (long.TryParse(itemObject["Start"].GetString(), out timeValue) ? timeValue : 0),
                                                           (long.TryParse(itemObject["Duration"].GetString(), out timeValue) ? timeValue : 0),
                                                           (itemObject.ContainsKey("HttpHeaders") ? itemObject["HttpHeaders"].GetString() : ""),
                                                           itemObject["PlayReadyUrl"].GetString(),
                                                           itemObject["PlayReadyCustomData"].GetString(),
                                                           itemObject["BackgroundAudio"].GetBoolean()));
                    }
                    this.Groups.Add(group);
                    return true;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Exception while opening the playlist: " + path + " Exception: " + e.Message);
            }
            return false;
        }

    }
}
