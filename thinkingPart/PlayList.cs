using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Web.Script.Serialization;
using powerful_youtube_dl.thinkingPart;
using System;
using System.Windows.Threading;

namespace powerful_youtube_dl
{
    public class PlayList
    {
        public static ObservableCollection<CheckBox> _listOfPlayListsCheckBox { get; set; }
        public static List<PlayList> _listOfPlayLists = new List<PlayList>();
        public static PlayList singleVideos = null;
        public System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();

        public List<Video> _listOfVideosInPlayList = new List<Video>();
        //ObservableCollection<Video> _listOfVideosInPlayList = new ObservableCollection<Video>();
        //public ObservableCollection<CheckBox> _listOfVideosInPlayListCheckBox = new ObservableCollection<CheckBox>();
        public string playListID, playListURL, playListTitle;
        public bool toDownload = false;

        public CheckBox check;

        public PlayList()
        {
            _listOfPlayListsCheckBox = new ObservableCollection<CheckBox>();
        }

        public PlayList(string link)
        {
            string id = "";
            try
            {
                id = link.Substring(link.IndexOf("list=") + 5, 34); //24
            }
            catch { id = link.Substring(link.IndexOf("list=") + 5, 24); }
            if (!checkIfPlayListExists(id))
            {
                playListID = id;
                playListURL = link;
                playListTitle = getTitle(id);
                check = new CheckBox();
                check.Click += new RoutedEventHandler(checkChanged);
                check.Content = playListTitle;
                check.ContextMenu = ((MainWindow)System.Windows.Application.Current.MainWindow).createPlaylistMenu(this);
                _listOfPlayLists.Add(this);
                getPlayListVideos(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                Video.getParamsOfVideos();
                Video.videoIDsToGetParams = new List<Video>();
                ((MainWindow)System.Windows.Application.Current.MainWindow).playlist.SelectedItem = check;
                _listOfPlayListsCheckBox.Add(check);
                addPlayListToSettings(playListURL);
                Statistics.LoadedPlaylist(this);

                System.Windows.Threading.DispatcherTimer checkingTimer = new System.Windows.Threading.DispatcherTimer();
                checkingTimer.Tick += checkPlayList_Tick;
                checkingTimer.Interval = new TimeSpan(0, 5, 0);
                checkingTimer.Start();
            }
            else
                MainWindow.Error("Ta playlista jest już dodana!");
        }

        private void checkPlayList_Tick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.autoObservePlaylists)
            {
                getPlaylistVideos_Timer(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                Video.getParamsOfVideos();
                Video.videoIDsToGetParams = new List<Video>();
                //((MainWindow)System.Windows.Application.Current.MainWindow).addVideos.Items.Refresh();
                //if (Properties.Settings.Default.autoDownloadObserve && Video.currentlyDownloading == 0)
                //{
                //    Video.isManualDownload = false;
                //    DownloadHandler.Load();
                //    DownloadHandler.DownloadQueue();
                //}
            }
        }

        public PlayList(string id, string title)
        {
            if (!checkIfPlayListExists(id))
            {
                playListID = id;
                playListTitle = title;
                playListURL = "https://www.youtube.com/playlist?list=" + playListID;
                check = new CheckBox();
                check.Click += new RoutedEventHandler(checkChanged);
                check.Content = playListTitle;
                getPlayListVideos(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                _listOfPlayListsCheckBox.Add(check);
                _listOfPlayLists.Add(this);
                ((MainWindow)System.Windows.Application.Current.MainWindow).playlist.SelectedItem = check;
                addPlayListToSettings(playListURL);
                Statistics.LoadedPlaylist(this);

                System.Windows.Threading.DispatcherTimer checkingTimer = new System.Windows.Threading.DispatcherTimer();
                checkingTimer.Tick += checkPlayList_Tick;
                checkingTimer.Interval = new TimeSpan(0, 5, 0);
                checkingTimer.Start();
            }
            else
                MainWindow.Error("Playlista o nazwie " + title + " jest już dodana!");
        }

        public PlayList(Video video)
        {
            if (video.position != null)
            {
                if (singleVideos == null)
                {
                    playListTitle = "Pojedyncze";
                    check = new CheckBox();
                    check.Click += new RoutedEventHandler(checkChanged);
                    check.Content = playListTitle;
                    _listOfPlayListsCheckBox.Add(check);
                    _listOfPlayLists.Add(this);
                    singleVideos = this;
                }
                video.playList = singleVideos;
                singleVideos._listOfVideosInPlayList.Add(video);
                ((MainWindow)System.Windows.Application.Current.MainWindow).addVideoToList(video.position);
                ((MainWindow)System.Windows.Application.Current.MainWindow).addVideos.Items.Refresh();
                ((MainWindow)System.Windows.Application.Current.MainWindow).playlist.SelectedItem = singleVideos.check;
            }
        }

        public static void addPlayListToSettings(string link)
        {
            if (!Properties.Settings.Default.playlists.Contains(link) && Properties.Settings.Default.autoObservePlaylists && Properties.Settings.Default.savePlaylists)
            {
                Properties.Settings.Default.playlists.Add(link);
                Properties.Settings.Default.Save();
            }
        }

        public static void removePlaylistFromSettings(string link)
        {
            if (Properties.Settings.Default.playlists.Contains(link))
            {
                Properties.Settings.Default.playlists.Remove(link);
                Properties.Settings.Default.Save();
            }
        }

        private bool checkIfPlayListExists(string id)
        {
            foreach (PlayList exists in _listOfPlayLists)
            {
                if (exists.playListID == id)
                {
                    return true;
                }
            }
            return false;
        }

        private bool checkIfVideoExists(string id)
        {
            foreach (Video exists in _listOfVideosInPlayList)
            {
                if (exists.videoID == id)
                {
                    return true;
                }
            }
            return false;
        }

        private string getTitle(string id)
        {
            string html = HTTP.GET("https://www.youtube.com/playlist?list=" + id);
            int start = html.IndexOf("<title>") + 7;
            int end = html.IndexOf(@" - YouTube</title>") - start;
            return html.Substring(start, end);
        }

        private static int licznik = 0;

        private void getPlaylistVideos_Timer(string json)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = new Dictionary<string, object>();
            obj2 = (Dictionary<string, object>)(result);

            System.Object[] val = (System.Object[])obj2["items"];

            foreach (object item in val)
            {
                licznik++;
                Dictionary<string, object> vid = (Dictionary<string, object>)item;
                Dictionary<string, object> temp = (Dictionary<string, object>)vid["snippet"];
                string title = temp["title"].ToString(); // resourceId -> videoId
                Dictionary<string, object> vid2 = (Dictionary<string, object>)temp["resourceId"];
                string id = vid2["videoId"].ToString();
                if (!checkIfVideoExists(id))
                {
                    Video toAdd = new Video(id);
                    toAdd.playList = this;
                    ((MainWindow)System.Windows.Application.Current.MainWindow).addVideoToList(toAdd.position);
                    Video._listOfVideos.Add(toAdd);
                    _listOfVideosInPlayList.Add(toAdd);
                }
            }

            try
            {
                string nextPage = obj2["nextPageToken"].ToString();
                getPlayListVideos(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&pageToken=" + nextPage + "&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
            catch { }
        }

        private void getPlayListVideos(string json)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = new Dictionary<string, object>();
            obj2 = (Dictionary<string, object>)(result);

            System.Object[] val = (System.Object[])obj2["items"];

            foreach (object item in val)
            {
                licznik++;
                Dictionary<string, object> vid = (Dictionary<string, object>)item;
                Dictionary<string, object> temp = (Dictionary<string, object>)vid["snippet"];
                string title = temp["title"].ToString(); // resourceId -> videoId
                Dictionary<string, object> vid2 = (Dictionary<string, object>)temp["resourceId"];
                string id = vid2["videoId"].ToString();
                Video toAdd = new Video(id);
                toAdd.playList = this;
                _listOfVideosInPlayList.Add(toAdd);
            }

            try
            {
                string nextPage = obj2["nextPageToken"].ToString();
                getPlayListVideos(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&pageToken=" + nextPage + "&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
            catch { }
        }

        private void checkChanged(object sender, RoutedEventArgs e)
        {
            toDownload = (bool)((CheckBox)sender).IsChecked;
            foreach (Video vid in _listOfVideosInPlayList)
                vid.position.check = toDownload;
            ((MainWindow)System.Windows.Application.Current.MainWindow).addVideos.Items.Refresh();
        }

        override
        public string ToString()
        {
            return playListTitle;
        }

    }
}
