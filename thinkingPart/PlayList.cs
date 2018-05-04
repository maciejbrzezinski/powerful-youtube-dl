using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Threading;
using powerful_youtube_dl.Properties;
using powerful_youtube_dl.web;
using powerful_youtube_dl.window;

namespace powerful_youtube_dl.thinkingPart {

    public class PlayList {
        public static ObservableCollection<ListViewItemMy> _listOfPlayListsView { get; set; }
        public static List<PlayList> _listOfPlayLists = new List<PlayList>();
        public static PlayList singleVideos;
        public static bool isVisible;

        public List<Video> videoIDsToGetParams = new List<Video>();

        public List<Video> _listOfVideosInPlayList = new List<Video>();
        public ListViewItemMy position;
        public int checkedCount;

        public PlayList() {
            if (_listOfPlayListsView == null)
                _listOfPlayListsView = new ObservableCollection<ListViewItemMy>();
            if (singleVideos == null) {
                position = new ListViewItemMy {
                    Title = "Pojedyncze",
                    Check = false,
                    ParentPL = this
                };
                singleVideos = this;
            }
        }

        public PlayList(string link) {
            if (_listOfPlayListsView == null)
                _listOfPlayListsView = new ObservableCollection<ListViewItemMy>();

            string id;
            try {
                id = link.Substring(link.IndexOf("list=") + 5, 34); //24
            } catch (Exception) {
                id = link.Substring(link.IndexOf("list=") + 5, 24);
            }
            if (!checkIfPlayListExists(id)) {
                position = new ListViewItemMy {
                    Title = getTitle(id),
                    Id = id,
                    Check = false,
                    Link = link,
                    ParentPL = this
                };

                _listOfPlayLists.Add(this);

                Thread ths = new Thread(() => {
                    getPlayListVideos(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                    getParamsOfVideos();
                    Statistics.LoadedPlaylist(this);
                    if (Settings.Default.autoObservePlaylists && Settings.Default.autoDownloadObserve && !Video.isManualDownload) {
                        DownloadHandler.Load(this);
                        DownloadHandler.DownloadQueueAsync();
                    }
                });
                ths.Start();

                _listOfPlayListsView.Add(position);
                addPlayListToSettings(position.Link);

                DispatcherTimer checkingTimer = new DispatcherTimer();
                checkingTimer.Tick += checkPlayList_Tick;
                checkingTimer.Interval = new TimeSpan(0, 5, 0);
                checkingTimer.Start();
            } else
                MainWindow.Error("Ta playlista jest już dodana!");
        }

        private void checkPlayList_Tick(object sender, EventArgs e) {
            if (Settings.Default.autoObservePlaylists) {
                getPlaylistVideos_Timer(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                getParamsOfVideos();
            }
        }

        public PlayList(string id, string title) {
            if (_listOfPlayListsView == null)
                _listOfPlayListsView = new ObservableCollection<ListViewItemMy>();
            if (!checkIfPlayListExists(id)) {
                position = new ListViewItemMy {
                    Title = title,
                    Id = id,
                    Check = false,
                    Link = "https://www.youtube.com/playlist?list=" + id,
                    ParentPL = this
                };

                Thread ths = new Thread(() => {
                    getPlayListVideos(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                    getParamsOfVideos();
                    Statistics.LoadedPlaylist(this);
                    if (Settings.Default.autoObservePlaylists && Settings.Default.autoDownloadObserve && !Video.isManualDownload) {
                        DownloadHandler.Load(this);
                        DownloadHandler.DownloadQueueAsync();
                    }
                });
                ths.Start();

                _listOfPlayListsView.Add(position);
                _listOfPlayLists.Add(this);
                addPlayListToSettings(position.Link);

                DispatcherTimer checkingTimer = new DispatcherTimer();
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
                    position = new ListViewItemMy {
                        Title = "Pojedyncze",
                        Check = false,
                        ParentPL = this
                    };
                    singleVideos = this;
                }
                if (!isVisible) {
                    isVisible = true;
                    _listOfPlayListsView.Add(singleVideos.position);
                    _listOfPlayLists.Add(singleVideos);
                }
                ((MainWindow) Application.Current.MainWindow)?.addVideoToList(video.position, singleVideos.position.Id);
            }
        }

        public static void addPlayListToSettings(string link) {
            if (!Settings.Default.playlists.Contains(link) && Settings.Default.autoObservePlaylists && Settings.Default.savePlaylists) {
                Settings.Default.playlists.Add(link);
                Settings.Default.Save();
            }
        }

        public static void removePlaylistFromSettings(string link) {
            if (Settings.Default.playlists.Contains(link)) {
                Settings.Default.playlists.Remove(link);
                Settings.Default.Save();
            }
        }

        private bool checkIfPlayListExists(string id) {
            foreach (PlayList exists in _listOfPlayLists) {
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

        private void getPlaylistVideos_Timer(string json) {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = (Dictionary<string, object>) (result);

            Object[] val = (Object[]) obj2["items"];

            foreach (object item in val) {
                Dictionary<string, object> vid = (Dictionary<string, object>) item;
                Dictionary<string, object> temp = (Dictionary<string, object>) vid["snippet"];
                string title = temp["title"].ToString(); // resourceId -> videoId
                if (title != "Deleted video" && title != "Private video") {
                    Dictionary<string, object> vid2 = (Dictionary<string, object>) temp["resourceId"];
                    string id = vid2["videoId"].ToString();
                    if (!Video.isVideoLoaded(id)) {
                        Video toAdd = new Video(id, this);
                        if (toAdd.position != null) {
                            toAdd.position.Title = title;
                            toAdd.position.Check = true;
                            //((MainWindow) System.Windows.Application.Current.MainWindow).addVideoToList(toAdd.position, playListID);
                            if (Settings.Default.autoObservePlaylists && Settings.Default.autoDownloadObserve && !Video.isManualDownload) {
                                DownloadHandler.Load(toAdd);
                                DownloadHandler.DownloadQueueAsync();
                            }
                        }
                    }
                }
            }
            if (obj2.ContainsKey("nextPageToken")) {
                string nextPage = obj2["nextPageToken"].ToString();
                getPlaylistVideos_Timer(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&pageToken=" + nextPage + "&playlistId=" + position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
        }

        private void getPlayListVideos(string json) {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = (Dictionary<string, object>) (result);

            Object[] val = (Object[]) obj2["items"];

            foreach (object item in val) {
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
                getPlayListVideos(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&pageToken=" + nextPage + "&playlistId=" + position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
        }

        public void checkChanged(bool? isChecked) {
            int count = _listOfVideosInPlayList.Count;
            for (int i = 0; i < count; i++) {
                if (isChecked != null)
                    _listOfVideosInPlayList[i].position.Check = isChecked;
                else
                    _listOfVideosInPlayList[i].position.Check = _listOfVideosInPlayList[i].position.Status != "Pobrano";
            }
        }

        override
        public string ToString() {
            if (position == null)
                return singleVideos.position.Title;
            return position.Title;
        }

        public void getParamsOfVideos() {
            List<string> IDs = new List<string> { "" };
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
                Dictionary<string, object> obj2 = (Dictionary<string, object>) (result);

                Object[] val = (Object[]) obj2["items"];

                for (int i = 0; i < val.Length; i++) {
                    int currentIndex = -1;
                    Dictionary<string, object> vid = (Dictionary<string, object>) val[i];
                    string id = vid["id"].ToString();
                    for (int d = 0; d < _listOfVideosInPlayList.Count; d++) {
                        if (_listOfVideosInPlayList[d].position.Id == id) {
                            currentIndex = d;
                            break;
                        }
                    }
                    if (currentIndex == -1 || _listOfVideosInPlayList.Count == 0)
                        return;
                    Video current = _listOfVideosInPlayList[currentIndex];

                    Dictionary<string, object> temp2 = (Dictionary<string, object>) vid["contentDetails"];
                    string duration = decryptDuration(temp2["duration"].ToString());
                    current.position.Duration = duration;

                    if (current.position.Title == null) {
                        Dictionary<string, object> temp = (Dictionary<string, object>) vid["snippet"];
                        current.position.Title = temp["title"].ToString();
                    }

                    if (!checkIfVideoIsOnDisk(current))
                        current.position.Check = true;
                    else {
                        current.position.Status = "Pobrano";
                        if (Settings.Default.playlistAsFolder)
                            current.downloadPath = Settings.Default.textDestination + "\\" + current.playList + "\\" + current + ".mp3";
                        else
                            current.downloadPath = Settings.Default.textDestination + "\\" + current + ".mp3";
                    }
                    if (!current.isVideoLoadedInActivePlaylist) {
                        MainWindow.invokeShit(DispatcherPriority.Normal, async () => {
                            ((MainWindow) Application.Current.MainWindow)?.addVideoToList(current.position, position.Id);
                        });
                    }
                    videoIDsToGetParams.Remove(current);
                    Statistics.LoadedVideo(current);
                }
            }
            foreach (Video v in videoIDsToGetParams) {
                MainWindow.invokeShit(DispatcherPriority.Send, async () => {
                    ((MainWindow) Application.Current.MainWindow)?.deleteVideoFromAdd(v.position, position.Id);
                });
                _listOfVideosInPlayList.Remove(v);
            }
            videoIDsToGetParams = new List<Video>();
        }

        public static bool checkIfVideoIsOnDisk(Video video) {
            string path;
            if (Settings.Default.playlistAsFolder)
                path = Settings.Default.textDestination + "\\" + video.playList + "\\" + video + ".mp3";
            else
                path = Settings.Default.textDestination + "\\" + video + ".mp3";
            if (File.Exists(path))
                return true;
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
            string seconds = "00";
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
            } catch {
                Console.WriteLine(@"TUTAJ");
            }
            if (seconds.Length == 1)
                seconds = "0" + seconds;
            if (minutes.Length == 2)
                minutes = "0" + minutes;
            if (hours.Length == 2)
                hours = "0" + hours;
            return hours + minutes + seconds;
        }

        public void contextDeletePlaylist() {
            foreach (Video v in _listOfVideosInPlayList)
                Video._listOfVideos.Remove(v);

            _listOfPlayListsView.Remove(position);
            removePlaylistFromSettings(position.Link);
            _listOfPlayLists.Remove(this);
            _listOfVideosInPlayList.Clear();
        }
    }
}