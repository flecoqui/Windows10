using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.Web.Http;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Text.RegularExpressions;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GraphUWPSampleApp
{
    public class EventImage
    {
        public EventImage()
        {
            Name = string.Empty;
            StartDate = string.Empty;
            EndDate = string.Empty;
            Image = null;
            bOneDrive = false;
        }

        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public BitmapImage Image { get; set; }
        public bool bOneDrive { get; set; }

    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string serviceEndpoint = "https://graph.microsoft.com/v1.0/";
        Microsoft.Graph.GraphServiceClient graphClient = null;
        public ObservableCollection<EventImage> ImgList = new ObservableCollection<EventImage>();

        public MainPage()
        {
            this.InitializeComponent();
            UserInfo.Text = "";
        }
        bool IsConnected()
        {
            return graphClient != null ? true : false;
        }
        void UpdateControls()
        {
            if (!App.Current.Resources.ContainsKey("ida:ClientID"))
            {
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = false;
                EventsButton.IsEnabled = false;

            }
            else
            {
                if (IsConnected())
                {
                    ConnectButton.IsEnabled = false;
                    DisconnectButton.IsEnabled = true;
                    EventsButton.IsEnabled = true;


                }
                else
                {
                    ConnectButton.IsEnabled = true;
                    DisconnectButton.IsEnabled = false;
                    EventsButton.IsEnabled = false;

                }
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Developer code - if you haven't registered the app yet, we warn you. 
            if (!App.Current.Resources.ContainsKey("ida:ClientID"))
            {
                InfoText.Text = "No ClientId Message in App.xaml";
            }
            else
            {
                InfoText.Text = "Click on connect button to launch the authentication"; 
            }
            UpdateControls();
        }
        /// <summary>
        /// Signs in the current user.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SignInCurrentUserAsync()
        {
            graphClient = AuthenticationHelper.GetAuthenticatedClient();

            if (graphClient != null)
            {
                Microsoft.Graph.User user = await graphClient.Me.Request().GetAsync();
                if (user != null)
                {
                    string userId = user.Id;
                    UserInfo.Text = "Name:" + user.DisplayName + " and address: " + user.UserPrincipalName;
                    return true;
                }
            }
            return false;
        }


        // Gets the signed-in user's calendar events.

        public static  async Task<ObservableCollection<EventImage>> GetEventsAsync(string startDateTime, string endDateTime)
        {
            var events = new ObservableCollection<EventImage>();
            JObject jResult = null;

            try
            {
                HttpClient client = new HttpClient();
                var token = await AuthenticationHelper.GetTokenForUserAsync();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                // Endpoint for the current user's events
                Uri usersEndpoint = new Uri(serviceEndpoint + "me/calendarview?startdatetime=" + startDateTime + "&enddatetime=" + endDateTime);

                HttpResponseMessage response = await client.GetAsync(usersEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    jResult = JObject.Parse(responseContent);

                    foreach (JObject calendarEvent in jResult["value"])
                    {
                        EventImage ei = new EventImage();
                        ei.Name = (string)calendarEvent["subject"];
                        var startObj = calendarEvent["start"];
                        ei.StartDate = (string)startObj["dateTime"];
                        var endObj = calendarEvent["end"];
                        ei.EndDate = (string)endObj["dateTime"];
                        var bodyObj = calendarEvent["body"];
                        string contentType = (string)bodyObj["contentType"];
                        string content = (string)bodyObj["content"];

                        ei.bOneDrive = false;

                        string eventId = (string)calendarEvent["id"];
                        bool attach = (bool)calendarEvent["hasAttachments"];
                        if (attach == true)
                        {
                            var listAttach = await GetEventAttachmentsAsync(eventId, ei,contentType,content);
                            if (listAttach != null)
                            {
                                foreach(var e in listAttach)
                                {
                                    events.Add(e);
                                }
                            }
                        }
                        System.Diagnostics.Debug.WriteLine("Got event: " + eventId);
                    }

                }

                else
                {
                    System.Diagnostics.Debug.WriteLine("We could not get the current user's events. The request returned this status code: " + response.StatusCode);
                    return null;

                }
            }

            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("We could not get the current user's events: " + e.Message);
                return null;
            }

            return events;
        }
        public static async Task<Windows.Storage.Streams.IRandomAccessStream> GetStreamFromUrl(string url)
        {
            Windows.Storage.Streams.IRandomAccessStream returnStream = null;

            try
            {
                using (var cli = new Windows.Web.Http.HttpClient())
                {
                    var resp = await cli.GetAsync(new Uri(url));
                    var b = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                    if (resp != null && resp.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                    {
                        using (var stream = await resp.Content.ReadAsInputStreamAsync())
                        {
                            var memStream = new MemoryStream();
                            if (memStream != null)
                            {
                                await stream.AsStreamForRead().CopyToAsync(memStream);
                                memStream.Position = 0;
                                returnStream = memStream.AsRandomAccessStream();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error while downloading the attachment: " + e.Message);
            }

            return returnStream;
        }
        private static readonly Regex _regex = new Regex(@"[?|&]([\w\.]+)=([^?|^&]+)");

        public static IReadOnlyDictionary<string, string> ParseQueryString(Uri uri)
        {
            var match = _regex.Match(uri.PathAndQuery);
            var paramaters = new Dictionary<string, string>();
            while (match.Success)
            {
                paramaters.Add(match.Groups[1].Value, match.Groups[2].Value);
                match = match.NextMatch();
            }
            return paramaters;
        }

        public static async Task<string> GetReroutedUrl(string url)
        {
            string returnString = url;

            try
            {
                using (var cli = new Windows.Web.Http.HttpClient())
                {
                    var resp = await cli.GetAsync(new Uri(url));
                    if (resp != null && resp.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                    {
                        if ((resp.RequestMessage != null) && (resp.RequestMessage.RequestUri != null))
                        {

                            IReadOnlyDictionary<string, string> param = ParseQueryString(resp.RequestMessage.RequestUri);
                            if (param != null)
                            {
                                if (
                                    (param.ContainsKey("resid")) &&
                                    (param.ContainsKey("authkey")))
                                {
                                    string resid = param["resid"];
                                    string authkey = param["authkey"];

                                    authkey = authkey.Replace("!", "%21");
                                    char[] sep = { '!' };
                                    string[] array = resid.Split(sep);
                                    if(array.Length==2)
                                    {
                                        string residprefix = array[0];
                                        string residsuffix = "%21" + array[1];
                                        resid = resid.Replace("!", "%21");
                                        returnString = "https://onedrive.live.com/download.aspx?cid=" + residprefix + "&authKey=" + authkey + "&resid=" + resid;
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error while downloading the attachment: " + e.Message);
            }

            return returnString;
        }

        public static async Task<Windows.Storage.Streams.IRandomAccessStream> GetAttachmentFromOneDrive(string name, string contentType, string contentHtml)
        {
            Windows.Storage.Streams.IRandomAccessStream attachments = null;
            JObject jResult = null;

            // get attachment from html content
            try
            {
                if (string.Equals(contentType, "html", StringComparison.OrdinalIgnoreCase))
                {
                    int endpos = contentHtml.IndexOf(name + "</a>");
                    if (endpos > 0)
                    {
                        int startpos = contentHtml.LastIndexOf("<a", endpos);
                        if ((startpos > 0) && (endpos > startpos))
                        {
                            string s = contentHtml.Substring(startpos, endpos - startpos);
                            if (!string.IsNullOrEmpty(s))
                            {
                                char[] sep = { ' ' };
                                string[] array = s.Split(sep);
                                if (array != null)
                                {
                                    foreach (var h in array)
                                    {
                                        if (!string.IsNullOrEmpty(h))
                                        {
                                            if (h.StartsWith("href="))
                                            {
                                                string url = h.Substring(5);
                                                if (!string.IsNullOrEmpty(url))
                                                {
                                                    url = url.Replace('\"', ' ');
                                                    url = url.Trim();
                                                    if(url.StartsWith("https://1drv.ms",StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        url = await GetReroutedUrl(url);
                                                    }
                                                    attachments = await GetStreamFromUrl(url);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error while getting the attachment: " + e.Message);
            }

            // attachment not found in HTML content
            // check if the attachments are present in OneDrive "attachments" folder
            if (attachments == null)
            {
                try
                {
                    HttpClient client = new HttpClient();
                    var token = await AuthenticationHelper.GetTokenForUserAsync();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


                    Uri usersEndpoint = new Uri(serviceEndpoint + "me/drive/root:/attachments:/children");

                    HttpResponseMessage response = await client.GetAsync(usersEndpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        jResult = JObject.Parse(responseContent);

                        foreach (JObject attachment in jResult["value"])
                        {

                            string locname = (string)attachment["name"];
                            int size = (int)attachment["size"];
                            if (string.Equals(name, locname, StringComparison.OrdinalIgnoreCase))
                            {
                                string url = (string)attachment["@microsoft.graph.downloadUrl"];
                                attachments = await GetStreamFromUrl(url);
                                break;
                            }
                        }
                    }

                    else
                    {
                        System.Diagnostics.Debug.WriteLine("We could not get the current user's events. The request returned this status code: " + response.StatusCode);
                        return null;

                    }
                }

                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("We could not get the current user's events: " + e.Message);
                    return null;
                }
            }
            return attachments;
        }

        public static  async Task<List<EventImage>> GetEventAttachmentsAsync(string id, EventImage inputEventImage,string contentType, string contentHtml)
        {
            var attachments = new List<EventImage>();
            JObject jResult = null;
            try
            {
                HttpClient client = new HttpClient();
                var token = await AuthenticationHelper.GetTokenForUserAsync();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


                Uri usersEndpoint = new Uri(serviceEndpoint + "me/events/" + id + "/attachments" );

                HttpResponseMessage response = await client.GetAsync(usersEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    jResult = JObject.Parse(responseContent);

                    foreach (JObject attachment in jResult["value"])
                    {

                        string name = (string)attachment["name"];
                        string type = (string)attachment["contentType"];
                        int size = (int)attachment["size"];
                        bool isInline = (bool)attachment["isInline"];
                        string content = (string)attachment["contentBytes"];
                        if (string.Equals(type, "image/jpeg", StringComparison.OrdinalIgnoreCase))
                        {


                            if (content != null)
                            {
                                byte[] contentArray = Convert.FromBase64String(content);
                                MemoryStream ms = new MemoryStream();
                                if (ms != null)
                                {
                                    ms.Write(contentArray, 0, contentArray.Length);
                                    ms.Position = 0;
                                    var stream = ms.AsRandomAccessStream();

                                    if (stream != null)
                                    {
                                        EventImage ei = new EventImage();
                                        ei.Name = inputEventImage.Name;
                                        ei.StartDate = inputEventImage.StartDate;
                                        ei.EndDate = inputEventImage.EndDate;
                                        ei.bOneDrive = false;


                                        ei.Image = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                                        ei.Image.SetSource(stream);
                                        attachments.Add(ei);
                                    }

                                }
                            }
                            else
                            {
                                var stream = await GetAttachmentFromOneDrive(name,contentType,contentHtml);
                                if (stream != null)
                                {
                                    EventImage ei = new EventImage();
                                    ei.Name = inputEventImage.Name;
                                    ei.StartDate = inputEventImage.StartDate;
                                    ei.EndDate = inputEventImage.EndDate;
                                    ei.bOneDrive = true;

                                    ei.Image = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                                    ei.Image.SetSource(stream);
                                    attachments.Add(ei);
                                }
                            }
                        }
                    }
                }

                else
                {
                    System.Diagnostics.Debug.WriteLine("We could not get the current user's events. The request returned this status code: " + response.StatusCode);
                    return null;

                }
            }

            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("We could not get the current user's events: " + e.Message);
                return null;
            }

            return attachments;
        }
        // Gets the signed-in user's drive.
        public static async Task<string> GetCurrentUserDriveAsync()
        {
            string currentUserDriveId = null;
            JObject jResult = null;

            try
            {
                HttpClient client = new HttpClient();
                var token = await AuthenticationHelper.GetTokenForUserAsync();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                // Endpoint for the current user's drive
                Uri usersEndpoint = new Uri(serviceEndpoint + "me/drive");

                HttpResponseMessage response = await client.GetAsync(usersEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    jResult = JObject.Parse(responseContent);
                    currentUserDriveId = (string)jResult["id"];
                    Debug.WriteLine("Got user drive: " + currentUserDriveId);
                }

                else
                {
                    Debug.WriteLine("We could not get the current user drive. The request returned this status code: " + response.StatusCode);
                    return null;
                }

            }


            catch (Exception e)
            {
                Debug.WriteLine("We could not get the current user drive: " + e.Message);
                return null;

            }

            return currentUserDriveId;

        }

        private async void EventsButton_Click(object sender, RoutedEventArgs e)
        {
            graphClient = AuthenticationHelper.GetAuthenticatedClient();

            if (graphClient != null)
            {
                try
                {

                    DateTime startDate = DateTime.Now;
                    DateTime endDate = DateTime.Now.AddDays(1).AddTicks(-1);
                    string startDateTime = startDate.ToString("s");
                    string endDateTime = endDate.ToString("s");
                    ImgList.Clear();
                    InfoText.Text = "Connected " ;

                    ImgList = await GetEventsAsync(startDateTime, endDateTime);
                    ListEvent.ItemsSource = ImgList;
                    InfoText.Text = "Connected, " + ImgList.Count.ToString() + " Images found";

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while getting events: " + ex.Message);
                }
            }
            UpdateControls();
        }
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (await SignInCurrentUserAsync())
            {
                InfoText.Text = "Connected";
            }
            else
            {
                InfoText.Text = "Authentication Error";
            }
            UpdateControls();
        }
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            ImgList.Clear();
            ListEvent.ItemsSource = ImgList;
            AuthenticationHelper.SignOut();
            graphClient = null;
            UserInfo.Text = "";
            InfoText.Text = "Click on connect button to launch the authentication";
            UpdateControls();
        }
    }
}
