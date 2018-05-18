using powerful_youtube_dl.Properties;
using powerful_youtube_dl.web;
using powerful_youtube_dl.window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Threading;

namespace powerful_youtube_dl.thinkingPart {

    public class PlayList {
        public static List<PlayList> ListOfPlayLists = new List<PlayList>();
        public static PlayList SingleVideos;
        public static bool IsVisible;

        public List<Video> VideoIDsToGetParams = new List<Video>();

        public List<Video> ListOfVideosInPlayList = new List<Video>();
        public ListViewItemMy Position;
        public int CheckedCount;

        public PlayList() {
            if (MainWindow.ListOfPlayListsView == null)
                MainWindow.ListOfPlayListsView = new ObservableCollection<ListViewItemMy>();
            if (SingleVideos == null) {
                Position = new ListViewItemMy {
                    Title = "Pojedyncze",
                    Check = false,
                    ParentPl = this
                };
                SingleVideos = this;
            }
        }

        public PlayList(string link) {
            if (MainWindow.ListOfPlayListsView == null)
                MainWindow.ListOfPlayListsView = new ObservableCollection<ListViewItemMy>();

            string id;
            try {
                id = link.Substring(link.IndexOf("list=", StringComparison.Ordinal) + 5, 34); //24
            } catch (Exception) {
                id = link.Substring(link.IndexOf("list=", StringComparison.Ordinal) + 5, 24);
            }

            if (!CheckIfPlayListExists(id)) {
                Position = new ListViewItemMy {
                    Title = GetTitle(id),
                    Id = id,
                    Check = false,
                    Link = link,
                    ParentPl = this
                };
                string path = "";
                if (Settings.Default.playlistAsFolder) {
                    path = Settings.Default.textDestination + "\\" + Position.Title;
                } else {
                    path = Settings.Default.textDestination;
                }
                Position.Path = path;

                ListOfPlayLists.Add(this);

                Thread ths = new Thread(() => {
                    GetPlayListVideos(new Http().Get("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + Position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                    GetParamsOfVideos();
                    Statistics.LoadedPlaylist(this);
                    if (Settings.Default.autoObservePlaylists && Settings.Default.autoDownloadObserve && !Video.IsManualDownload) {
                        DownloadHandler.Load(this);
                        DownloadHandler.DownloadQueueAsync();
                    }
                });
                ths.Start();

                MainWindow.ListOfPlayListsView.Add(Position);
                AddPlayListToSettings(Position.Link);

                DispatcherTimer checkingTimer = new DispatcherTimer();
                checkingTimer.Tick += checkPlayList_Tick;
                checkingTimer.Interval = new TimeSpan(0, 5, 0);
                checkingTimer.Start();
            } else
                MainWindow.Error("Ta playlista jest już dodana!");
        }

        private void checkPlayList_Tick(object sender, EventArgs e) {
            if (Settings.Default.autoObservePlaylists) {
                getPlaylistVideos_Timer(new Http().Get("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + Position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                GetParamsOfVideos();
            }
        }

        public PlayList(string id, string title) {
            if (MainWindow.ListOfPlayListsView == null)
                MainWindow.ListOfPlayListsView = new ObservableCollection<ListViewItemMy>();
            if (!CheckIfPlayListExists(id)) {
                Position = new ListViewItemMy {
                    Title = title,
                    Id = id,
                    Check = false,
                    Link = "https://www.youtube.com/playlist?list=" + id,
                    ParentPl = this
                };
                string path = "";
                if (Settings.Default.playlistAsFolder) {
                    path = Settings.Default.textDestination + "\\" + Position.Title;
                } else {
                    path = Settings.Default.textDestination;
                }
                Position.Path = path;

                Thread ths = new Thread(() => {
                    GetPlayListVideos(new Http().Get("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + Position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                    GetParamsOfVideos();
                    Statistics.LoadedPlaylist(this);
                    if (Settings.Default.autoObservePlaylists && Settings.Default.autoDownloadObserve && !Video.IsManualDownload) {
                        DownloadHandler.Load(this);
                        DownloadHandler.DownloadQueueAsync();
                    }
                });
                ths.Start();

                MainWindow.ListOfPlayListsView.Add(Position);
                ListOfPlayLists.Add(this);
                AddPlayListToSettings(Position.Link);

                DispatcherTimer checkingTimer = new DispatcherTimer();
                checkingTimer.Tick += checkPlayList_Tick;
                checkingTimer.Interval = new TimeSpan(0, 5, 0);
                checkingTimer.Start();
            } else
                MainWindow.Error("Playlista o nazwie " + title + " jest już dodana!");
        }

        public PlayList(Video video) {
            if (MainWindow.ListOfPlayListsView == null)
                MainWindow.ListOfPlayListsView = new ObservableCollection<ListViewItemMy>();

            if (video.Position != null) {
                if (SingleVideos == null) {
                    Position = new ListViewItemMy {
                        Title = "Pojedyncze",
                        Check = false,
                        ParentPl = this,
                    };
                    string path = "";
                    if (Settings.Default.playlistAsFolder) {
                        path = Settings.Default.textDestination + "\\" + Position.Title;
                    } else {
                        path = Settings.Default.textDestination;
                    }
                    Position.Path = path;
                    SingleVideos = this;
                }
                if (!IsVisible) {
                    IsVisible = true;
                    MainWindow.ListOfPlayListsView.Add(SingleVideos.Position);
                    ListOfPlayLists.Add(SingleVideos);
                }
                ((MainWindow) Application.Current.MainWindow)?.AddVideoToList(video.Position, SingleVideos.Position.Id);
            }
        }

        public static void AddPlayListToSettings(string link) {
            if (!Settings.Default.playlists.Contains(link) && Settings.Default.autoObservePlaylists && Settings.Default.savePlaylists) {
                Settings.Default.playlists.Add(link);
                Settings.Default.Save();
            }
        }

        public static void RemovePlaylistFromSettings(string link) {
            if (Settings.Default.playlists.Contains(link)) {
                Settings.Default.playlists.Remove(link);
                Settings.Default.Save();
            }
        }

        private bool CheckIfPlayListExists(string id) {
            foreach (PlayList exists in ListOfPlayLists) {
                if (exists.Position.Id == id) {
                    return true;
                }
            }
            return false;
        }

        private string GetTitle(string id) {
            string html = new Http().Get("https://www.youtube.com/playlist?list=" + id);
            int start = html.IndexOf("<title>", StringComparison.Ordinal) + 7;
            int end = html.IndexOf(@" - YouTube</title>", StringComparison.Ordinal) - start;
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
                    if (!Video.IsVideoLoaded(id)) {
                        Video toAdd = new Video(id, this);
                        if (toAdd.Position != null) {
                            toAdd.Position.Title = title;
                            toAdd.Position.Check = true;
                            if (Settings.Default.autoObservePlaylists && Settings.Default.autoDownloadObserve && !Video.IsManualDownload) {
                                DownloadHandler.Load(toAdd);
                                DownloadHandler.DownloadQueueAsync();
                            }
                        }
                    }
                }
            }
            if (obj2.ContainsKey("nextPageToken")) {
                string nextPage = obj2["nextPageToken"].ToString();
                getPlaylistVideos_Timer(new Http().Get("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&pageToken=" + nextPage + "&playlistId=" + Position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
        }

        private void GetPlayListVideos(string json) {
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
                    if (toAdd.Position != null) {
                        toAdd.Position.Title = title;
                    }
                }
            }
            if (obj2.ContainsKey("nextPageToken")) {
                string nextPage = obj2["nextPageToken"].ToString();
                GetPlayListVideos(new Http().Get("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&pageToken=" + nextPage + "&playlistId=" + Position.Id + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
        }

        public void CheckChanged(bool? isChecked) {
            int count = ListOfVideosInPlayList.Count;
            for (int i = 0; i < count; i++) {
                if (isChecked != null)
                    ListOfVideosInPlayList[i].Position.Check = isChecked;
                else
                    ListOfVideosInPlayList[i].Position.Check = !ListOfVideosInPlayList[i].IsDownloaded;
            }
        }

        override
        public string ToString() {
            if (Position == null)
                return SingleVideos.Position.Title;
            return Position.Title;
        }

        public void GetParamsOfVideos() {
            List<string> ds = new List<string> { "" };
            int ktoryJuz = 0;

            for (int i = 0; i < VideoIDsToGetParams.Count; i++) {
                int index = ds.Count - 1;

                if (ktoryJuz == 0)
                    ds[index] = VideoIDsToGetParams[i].Position.Id;
                else
                    ds[index] += @"%2C" + VideoIDsToGetParams[i].Position.Id;
                ktoryJuz++;
                if (ktoryJuz == 50) {
                    ds.Add("");
                    ktoryJuz = 0;
                }
            }
            for (int j = 0; j < ds.Count; j++) {
                string json = new Http().Get("https://www.googleapis.com/youtube/v3/videos?part=snippet%2CcontentDetails&id=" + ds[j] + "&fields=items(contentDetails%2Fduration%2Cid%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o");

                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                var result = jsSerializer.DeserializeObject(json);
                Dictionary<string, object> obj2 = (Dictionary<string, object>) (result);

                Object[] val = (Object[]) obj2["items"];

                for (int i = 0; i < val.Length; i++) {
                    int currentIndex = -1;
                    Dictionary<string, object> vid = (Dictionary<string, object>) val[i];
                    string id = vid["id"].ToString();
                    for (int d = 0; d < ListOfVideosInPlayList.Count; d++) {
                        if (ListOfVideosInPlayList[d].Position.Id == id) {
                            currentIndex = d;
                            break;
                        }
                    }
                    if (currentIndex == -1 || ListOfVideosInPlayList.Count == 0)
                        return;
                    Video current = ListOfVideosInPlayList[currentIndex];

                    Dictionary<string, object> temp2 = (Dictionary<string, object>) vid["contentDetails"];
                    string duration = DecryptDuration(temp2["duration"].ToString());
                    current.Position.Duration = duration;

                    if (current.Position.Title == null) {
                        Dictionary<string, object> temp = (Dictionary<string, object>) vid["snippet"];
                        current.Position.Title = temp["title"].ToString();
                    }

                    if (!CheckIfVideoIsOnDisk(current))
                        current.Position.Check = true;
                    else {
                        current.Position.Status = "Pobrano";
                        if (Settings.Default.playlistAsFolder)
                            current.Position.Path = Settings.Default.textDestination + "\\" + current.PlayList + "\\" + current + ".mp3";
                        else
                            current.Position.Path = Settings.Default.textDestination + "\\" + current + ".mp3";
                    }
                    VideoIDsToGetParams.Remove(current);
                    Statistics.LoadedVideo(current);
                }
            }
            foreach (Video v in VideoIDsToGetParams) {
                MainWindow.InvokeShit(DispatcherPriority.Send, async () => {
                    ((MainWindow) Application.Current.MainWindow)?.DeleteVideoFromAdd(v.Position, Position.Id);
                });
                ListOfVideosInPlayList.Remove(v);
            }
            VideoIDsToGetParams = new List<Video>();
        }

        public static bool CheckIfVideoIsOnDisk(Video video) {
            string path;
            if (Settings.Default.playlistAsFolder)
                path = Settings.Default.textDestination + "\\" + video.PlayList + "\\" + video + ".mp3";
            else
                path = Settings.Default.textDestination + "\\" + video + ".mp3";
            if (File.Exists(path))
                return true;
            return false;
        }

        public void AddToGetParams(Video v) {
            VideoIDsToGetParams.Add(v);
        }

        public static string DecryptDuration(string jsonValue) // np. PT4M50S = 04:40
       {
            string tmp = jsonValue;
            tmp = tmp.Substring(2);
            string hours = "";
            string minutes = "00:";
            string seconds = "00";
            try {
                int start = tmp.IndexOf("M", StringComparison.Ordinal) < 0 ? 0 : tmp.IndexOf("M", StringComparison.Ordinal) + 1;
                int end = start > 0 ? tmp.IndexOf("S", StringComparison.Ordinal) - start : tmp.IndexOf("S", StringComparison.Ordinal);
                if (start > -1 && end > 0)
                    seconds = tmp.Substring(start, end);

                start = tmp.IndexOf("H", StringComparison.Ordinal) < 0 ? 0 : tmp.IndexOf("H", StringComparison.Ordinal) + 1;
                end = start > 0 ? tmp.IndexOf("M", StringComparison.Ordinal) - start : tmp.IndexOf("M", StringComparison.Ordinal);
                if (start > -1 && end > 0)
                    minutes = tmp.Substring(start, end) + ":";

                start = tmp.IndexOf("H", StringComparison.Ordinal);
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

        public void ContextDeletePlaylist() {
            foreach (Video v in ListOfVideosInPlayList)
                Video.ListOfVideos.Remove(v);

            MainWindow.ListOfPlayListsView.Remove(Position);
            RemovePlaylistFromSettings(Position.Link);
            ListOfPlayLists.Remove(this);
            ListOfVideosInPlayList.Clear();
        }

        public void ContextOpenPath() {
            string argument = "/select, \"" + Position.Path + "\"";
            Process.Start("explorer.exe", argument);
        }

        public void ContextOpenYT() {
            Process.Start(Position.Link);
        }
    }
}