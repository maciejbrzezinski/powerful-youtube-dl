using powerful_youtube_dl.thinkingPart;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace powerful_youtube_dl {

    public class PlayList {
        public static ObservableCollection<ListViewItemMy> _listOfPlayListsView { get; set; }
        public static List<PlayList> _listOfPlayLists = new List<PlayList>();
        public static PlayList singleVideos = null;
        public static bool isVisible = false;

        public List<Video> videoIDsToGetParams = new List<Video>();

        public System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
        public List<Video> _listOfVideosInPlayList = new List<Video>();
        public string playListID, playListURL, playListTitle;
        public bool toDownload = false;
        public ListViewItemMy position = null;

        // public CheckBox check;

        public PlayList() {
            if (_listOfPlayListsView == null)
                _listOfPlayListsView = new ObservableCollection<ListViewItemMy>();
            if (singleVideos == null) {
                playListTitle = "Pojedyncze";
                position = new ListViewItemMy();
                position.Title = playListTitle;
                position.Check = false;
                singleVideos = this;
            }
        }

        public PlayList(string link) {
            if (_listOfPlayListsView == null)
                _listOfPlayListsView = new ObservableCollection<ListViewItemMy>();

            string id = "";
            try {
                id = link.Substring(link.IndexOf("list=") + 5, 34); //24
            } catch (Exception) {
                id = link.Substring(link.IndexOf("list=") + 5, 24);
            }
            if (!checkIfPlayListExists(id)) {
                position = new ListViewItemMy();
                playListID = id;
                playListURL = link;
                playListTitle = getTitle(id);
                position.Title = playListTitle;
                position.Check = false;
                _listOfPlayLists.Add(this);

                Thread ths = new Thread(() => {
                    getPlayListVideos(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                    getParamsOfVideos();
                    Statistics.LoadedPlaylist(this);
                    if (Properties.Settings.Default.autoObservePlaylists && Properties.Settings.Default.autoDownloadObserve && !Video.isManualDownload) {
                        DownloadHandler.Load(this);
                        DownloadHandler.DownloadQueueAsync();
                    }
                });
                ths.Start();

                _listOfPlayListsView.Add(position);
                addPlayListToSettings(playListURL);

                System.Windows.Threading.DispatcherTimer checkingTimer = new System.Windows.Threading.DispatcherTimer();
                checkingTimer.Tick += checkPlayList_Tick;
                checkingTimer.Interval = new TimeSpan(0, 5, 0);
                checkingTimer.Start();
            } else
                MainWindow.Error("Ta playlista jest już dodana!");
        }

        private void checkPlayList_Tick(object sender, EventArgs e) {
            if (Properties.Settings.Default.autoObservePlaylists) {
                getPlaylistVideos_Timer(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                getParamsOfVideos();
            }
        }

        public PlayList(string id, string title) {
            if (_listOfPlayListsView == null)
                _listOfPlayListsView = new ObservableCollection<ListViewItemMy>();
            if (!checkIfPlayListExists(id)) {
                position = new ListViewItemMy();
                playListID = id;
                playListTitle = title;
                playListURL = "https://www.youtube.com/playlist?list=" + playListID;
                position.Title = playListTitle;
                position.Check = false;

                Thread ths = new Thread(() => {
                    getPlayListVideos(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                    getParamsOfVideos();
                    Statistics.LoadedPlaylist(this);
                    if (Properties.Settings.Default.autoObservePlaylists && Properties.Settings.Default.autoDownloadObserve && !Video.isManualDownload) {
                        DownloadHandler.Load(this);
                        DownloadHandler.DownloadQueueAsync();
                    }
                });
                ths.Start();

                _listOfPlayListsView.Add(position);
                _listOfPlayLists.Add(this);
                addPlayListToSettings(playListURL);

                System.Windows.Threading.DispatcherTimer checkingTimer = new System.Windows.Threading.DispatcherTimer();
                checkingTimer.Tick += checkPlayList_Tick;
                checkingTimer.Interval = new TimeSpan(0, 5, 0);
                checkingTimer.Start();
            } else
                MainWindow.Error("Playlista o nazwie " + title + " jest już dodana!");
        }

        public PlayList(Video video) {
            if (_listOfPlayListsView == null)
                _listOfPlayListsView = new ObservableCollection<ListViewItemMy>();

            if (video.position != null) {
                if (singleVideos == null) {
                    position = new ListViewItemMy();
                    playListTitle = "Pojedyncze";
                    position.Title = playListTitle;
                    position.Check = false;
                    singleVideos = this;
                }
                if (!isVisible) {
                    isVisible = true;
                    _listOfPlayListsView.Add(singleVideos.position);
                    _listOfPlayLists.Add(singleVideos);
                }
                ((MainWindow) System.Windows.Application.Current.MainWindow).addVideoToList(video.position, singleVideos.playListID);
            }
        }

        public static void addPlayListToSettings(string link) {
            if (!Properties.Settings.Default.playlists.Contains(link) && Properties.Settings.Default.autoObservePlaylists && Properties.Settings.Default.savePlaylists) {
                Properties.Settings.Default.playlists.Add(link);
                Properties.Settings.Default.Save();
            }
        }

        public static void removePlaylistFromSettings(string link) {
            if (Properties.Settings.Default.playlists.Contains(link)) {
                Properties.Settings.Default.playlists.Remove(link);
                Properties.Settings.Default.Save();
            }
        }

        private bool checkIfPlayListExists(string id) {
            foreach (PlayList exists in _listOfPlayLists) {
                if (exists.playListID == id) {
                    return true;
                }
            }
            return false;
        }

        private bool checkIfVideoExists(string id) {
            foreach (Video exists in _listOfVideosInPlayList) {
                if (exists.position.Id == id) {
                    return true;
                }
            }
            return false;
        }

        private string getTitle(string id) {
            string html = new HTTP().GET("https://www.youtube.com/playlist?list=" + id);
            int start = html.IndexOf("<title>") + 7;
            int end = html.IndexOf(@" - YouTube</title>") - start;
            return html.Substring(start, end);
        }

        private static int licznik = 0;

        private void getPlaylistVideos_Timer(string json) {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = new Dictionary<string, object>();
            obj2 = (Dictionary<string, object>) (result);

            System.Object[] val = (System.Object[]) obj2["items"];

            foreach (object item in val) {
                licznik++;
                Dictionary<string, object> vid = (Dictionary<string, object>) item;
                Dictionary<string, object> temp = (Dictionary<string, object>) vid["snippet"];
                string title = temp["title"].ToString(); // resourceId -> videoId
                Dictionary<string, object> vid2 = (Dictionary<string, object>) temp["resourceId"];
                string id = vid2["videoId"].ToString();
                if (!checkIfVideoExists(id)) {
                    Video toAdd = new Video(id, this);
                    if (toAdd.position != null) {
                        toAdd.position.Title = title;
                        ((MainWindow) System.Windows.Application.Current.MainWindow).addVideoToList(toAdd.position, playListID);
                        if (Properties.Settings.Default.autoObservePlaylists && Properties.Settings.Default.autoDownloadObserve && !Video.isManualDownload) {
                            DownloadHandler.Load(toAdd);
                            DownloadHandler.DownloadQueueAsync();
                        }
                    }
                }
            }
            if (obj2.ContainsKey("nextPageToken")) {
                string nextPage = obj2["nextPageToken"].ToString();
                getPlaylistVideos_Timer(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&pageToken=" + nextPage + "&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
        }

        private void getPlayListVideos(string json) {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = new Dictionary<string, object>();
            obj2 = (Dictionary<string, object>) (result);

            System.Object[] val = (System.Object[]) obj2["items"];

            foreach (object item in val) {
                licznik++;
                Dictionary<string, object> vid = (Dictionary<string, object>) item;
                Dictionary<string, object> temp = (Dictionary<string, object>) vid["snippet"];
                string title = temp["title"].ToString(); // resourceId -> videoId
                if (title != "Deleted video" && title != "Private video") {
                    Dictionary<string, object> vid2 = (Dictionary<string, object>) temp["resourceId"];
                    string id = vid2["videoId"].ToString();
                    Video toAdd = new Video(id, this);
                    if (toAdd.position != null) {
                        toAdd.position.Title = title;
                    }
                }
            }
            if (obj2.ContainsKey("nextPageToken")) {
                string nextPage = obj2["nextPageToken"].ToString();
                getPlayListVideos(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&pageToken=" + nextPage + "&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
        }

        private void checkChanged(object sender, RoutedEventArgs e) {
            toDownload = (bool) ((CheckBox) sender).IsChecked;
            foreach (Video vid in _listOfVideosInPlayList)
                vid.position.Check = toDownload;
        }

        override
        public string ToString() {
            return playListTitle;
        }

        public void getParamsOfVideos() {
            List<string> IDs = new List<string>();
            IDs.Add("");
            int ktoryJuz = 0;

            for (int i = 0; i < videoIDsToGetParams.Count; i++) {
                int index = IDs.Count - 1;
                if (ktoryJuz == 0)
                    IDs[index] = videoIDsToGetParams[i].position.Id;
                else
                    IDs[index] += @"%2C" + videoIDsToGetParams[i].position.Id;
                ktoryJuz++;
                if (ktoryJuz == 50) {
                    IDs.Add("");
                    ktoryJuz = 0;
                }
            }
            for (int j = 0; j < IDs.Count; j++) {
                string json = new HTTP().GET("https://www.googleapis.com/youtube/v3/videos?part=snippet%2CcontentDetails&id=" + IDs[j] + "&fields=items(contentDetails%2Fduration%2Cid%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o");

                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                var result = jsSerializer.DeserializeObject(json);
                Dictionary<string, object> obj2 = new Dictionary<string, object>();
                obj2 = (Dictionary<string, object>) (result);

                System.Object[] val = (System.Object[]) obj2["items"];

                for (int i = 0; i < val.Length; i++) {
                    int current = -1;

                    Dictionary<string, object> vid = (Dictionary<string, object>) val[i];
                    string id = vid["id"].ToString();
                    for (int d = 0; d < _listOfVideosInPlayList.Count; d++) {
                        if (_listOfVideosInPlayList[d].position.Id == id) {
                            current = d;
                            break;
                        }
                    }

                    Dictionary<string, object> temp2 = (Dictionary<string, object>) vid["contentDetails"];
                    string duration = decryptDuration(temp2["duration"].ToString());
                    _listOfVideosInPlayList[current].position.Duration = duration;

                    if (_listOfVideosInPlayList[current].position.Title == null) {
                        Dictionary<string, object> temp = (Dictionary<string, object>) vid["snippet"];
                        _listOfVideosInPlayList[current].position.Title = temp["title"].ToString();
                    }

                    if (!checkIfVideoIsOnDisk(_listOfVideosInPlayList[current]))
                        _listOfVideosInPlayList[current].position.Check = true;
                    else {
                        _listOfVideosInPlayList[current].position.Status = "Pobrano";
                        if (Properties.Settings.Default.playlistAsFolder)
                            _listOfVideosInPlayList[current].downloadPath = Properties.Settings.Default.textDestination + "\\" + _listOfVideosInPlayList[current].playList.ToString() + "\\" + _listOfVideosInPlayList[current].ToString() + ".mp3";
                        else
                            _listOfVideosInPlayList[current].downloadPath = Properties.Settings.Default.textDestination + "\\" + _listOfVideosInPlayList[current].ToString() + ".mp3";
                    }
                    if (!_listOfVideosInPlayList[current].isVideoLoadedInActivePlaylist) {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                          DispatcherPriority.Normal,
                          new Action(() => {
                              ((MainWindow) System.Windows.Application.Current.MainWindow).addVideoToList(_listOfVideosInPlayList[current].position, playListID);
                          }));
                    }
                    Statistics.LoadedVideo(_listOfVideosInPlayList[current]);
                }
            }
            videoIDsToGetParams = new List<Video>();
        }

        public static bool checkIfVideoIsOnDisk(Video video) {
            string path = "";
            if (Properties.Settings.Default.playlistAsFolder)
                path = Properties.Settings.Default.textDestination + "\\" + video.playList.ToString() + "\\" + video.ToString() + ".mp3";
            else
                path = Properties.Settings.Default.textDestination + "\\" + video.ToString() + ".mp3";
            if (File.Exists(path))
                return true;
            else
                return false;
        }

        public void addToGetParams(Video v) {
            videoIDsToGetParams.Add(v);
        }

        public static string decryptDuration(string jsonValue) // np. PT4M50S = 04:40
       {
            string tmp = jsonValue;
            tmp = tmp.Substring(2);
            string hours = "";
            string minutes = "00:";
            string seconds = "";
            try {
                int start = tmp.IndexOf("M") < 0 ? 0 : tmp.IndexOf("M") + 1;
                int end = start > 0 ? tmp.IndexOf("S") - start : tmp.IndexOf("S");
                if (start > -1 && end > 0)
                    seconds = tmp.Substring(start, end);

                start = tmp.IndexOf("H") < 0 ? 0 : tmp.IndexOf("H") + 1;
                end = start > 0 ? tmp.IndexOf("M") - start : tmp.IndexOf("M");
                if (start > -1 && end > 0)
                    minutes = tmp.Substring(start, end) + ":";

                start = tmp.IndexOf("H");
                if (start > 0)
                    hours = tmp.Substring(0, start) + ":";
            } catch (ArgumentOutOfRangeException exc) {
                Console.WriteLine("TUTAJ");
            }
            if (seconds.Length == 1)
                seconds = "0" + seconds;
            if (minutes.Length == 2)
                minutes = "0" + minutes;
            if (hours.Length == 2)
                hours = "0" + hours;
            return hours + minutes + seconds;
        }
    }
}