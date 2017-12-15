﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Web.Script.Serialization;

namespace powerful_youtube_dl
{
    class PlayList
    {
        public static ObservableCollection<CheckBox> _listOfPlayListsCheckBox { get; set; }
        public static List<PlayList> _listOfPlayLists = new List<PlayList>();

        public List<Video> _listOfVideosInPlayList = new List<Video>();
        public ObservableCollection<CheckBox> _listOfVideosInPlayListCheckBox = new ObservableCollection<CheckBox>();
        public string playListID, playListURL, playListTitle;
        public bool toDownload = false;

        public CheckBox check;

        public PlayList()
        {
            _listOfPlayListsCheckBox = new ObservableCollection<CheckBox>();
        }

        public PlayList(string link)
        {
            string id = link.Substring(link.IndexOf("list=") + 5, 34); //34
            playListID = id;
            bool isPlayListExisting = false;
            foreach(PlayList exists in _listOfPlayLists)
            {
                if (exists.playListID == playListID)
                {
                    isPlayListExisting = true;
                    break;
                }
            }
            if (!isPlayListExisting)
            {
                playListURL = link;
                playListTitle = getTitle(id);
                check = new CheckBox();
                check.Click += new RoutedEventHandler(checkChanged);
                check.Content = playListTitle;
                getPlayListVideos(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
                _listOfPlayListsCheckBox.Add(check);
                _listOfPlayLists.Add(this);
            }
            else
                MainWindow.Error("Ta playlista jest już dodana!");
        }

        public PlayList(string id, string title)
        {
            playListID = id;
            playListTitle = title;
            check = new CheckBox();
            check.Click += new RoutedEventHandler(checkChanged);
            check.Content = playListTitle;
            getPlayListVideos(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + playListID + "&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            _listOfPlayListsCheckBox.Add(check);
            _listOfPlayLists.Add(this);
        }

        private string getTitle(string id)
        {
            string html = HTTP.GET("https://www.youtube.com/playlist?list=" + id);
            int start = html.IndexOf("<title>") + 7;
            int end = html.IndexOf(@" - YouTube</title>") - start;
            return html.Substring(start, end);
        }

        private void getPlayListVideos(string json)
        {
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                var result = jsSerializer.DeserializeObject(json);
                Dictionary<string, object> obj2 = new Dictionary<string, object>();
                obj2 = (Dictionary<string, object>)(result);

                System.Object[] val = (System.Object[])obj2["items"];

                //List<object> val = (List<object>)obj2["items"];
                foreach (object item in val)
                {
                    Dictionary<string, object> vid = (Dictionary<string, object>)item;
                    Dictionary<string, object> temp = (Dictionary<string, object>)vid["snippet"];
                    string title = temp["title"].ToString(); // resourceId -> videoId
                    Dictionary<string, object> vid2 = (Dictionary<string, object>)temp["resourceId"];
                    //  System.Object[] temp2 = (System.Object[])vid2["resourceId"];
                    string id = vid2["videoId"].ToString();
                    Video toAdd = new Video(id, title);
                    _listOfVideosInPlayListCheckBox.Add(toAdd.check);
                    _listOfVideosInPlayList.Add(toAdd);
                }
            try
            {
                string nextPage = obj2["nextPageToken"].ToString();
                getPlayListVideos(HTTP.GET("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&pageToken="+nextPage+"&playlistId="+playListID+"&fields=items(snippet(resourceId%2FvideoId%2Ctitle))%2CnextPageToken&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
            catch { }
        }

        private void checkChanged(object sender, RoutedEventArgs e)
        {
            toDownload = (bool)((CheckBox)sender).IsChecked;
            foreach (Video vid in _listOfVideosInPlayList)
                vid.check.IsChecked = toDownload;
        }

        override
        public string ToString()
        {
            return playListTitle;
        }

    }
}