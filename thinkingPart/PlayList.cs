﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Web.Script.Serialization;

namespace powerful_youtube_dl
{
    public class PlayList
    {
        public static ObservableCollection<CheckBox> _listOfPlayListsCheckBox { get; set; }
        public static List<PlayList> _listOfPlayLists = new List<PlayList>();
        public static PlayList singleVideos = null;

        public List<Video> _listOfVideosInPlayList = new List<Video>();
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
                getPlayListVideos(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                _listOfPlayListsCheckBox.Add(check);
                _listOfPlayLists.Add(this);
                ((MainWindow)System.Windows.Application.Current.MainWindow).playlist.SelectedItem = check;
            }
            else
                MainWindow.Error("Ta playlista jest już dodana!");
        }

        public PlayList(string id, string title)
        {
            if (!checkIfPlayListExists(id))
            {
                playListID = id;
                playListTitle = title;
                check = new CheckBox();
                check.Click += new RoutedEventHandler(checkChanged);
                check.Content = playListTitle;
                getPlayListVideos(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                _listOfPlayListsCheckBox.Add(check);
                _listOfPlayLists.Add(this);
                ((MainWindow)System.Windows.Application.Current.MainWindow).playlist.SelectedItem = check;
            }
            else
                MainWindow.Error("Playlista o nazwie "+title+" jest już dodana!");
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

        private string getTitle(string id)
        {
            string html = HTTP.GET("https://www.youtube.com/playlist?list=" + id);
            int start = html.IndexOf("<title>") + 7;
            int end = html.IndexOf(@" - YouTube</title>") - start;
            return html.Substring(start, end);
        }

        private static int licznik = 0;

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
