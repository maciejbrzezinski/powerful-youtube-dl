using powerful_youtube_dl.web;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows;

namespace powerful_youtube_dl.thinkingPart {

    internal class User {
        public string UserId, UserUrl;

        public User(string link, int status) //(status) 0 - playlisty 1 - dodane
        {
            string id;

            if (link.Contains("/channel/")) {
                int start = link.IndexOf("nnel/", StringComparison.Ordinal) + 5;
                int finish = link.Substring(start).IndexOf("?", StringComparison.Ordinal);
                if (finish == -1)
                    finish = link.Substring(start).IndexOf("/", StringComparison.Ordinal);
                UserId = finish != -1 ? link.Substring(start, finish) : link.Substring(start);

                id = "id=" + UserId;
            } else {
                int start = link.IndexOf("user/", StringComparison.Ordinal) + 5;
                int finish = link.Substring(start).IndexOf("/", StringComparison.Ordinal);
                string title = finish > 0 ? link.Substring(start, finish) : link.Substring(start);
                id = "forUsername=" + title;
                UserId = GetUserId(title);
            }

            UserUrl = link;
            if (status == 0) {
                string zapytanie = "https://www.googleapis.com/youtube/v3/playlists?part=snippet&channelId=" + UserId + "&maxResults=50&fields=items(id%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o";
                string responseYouTubeApi = new Http().Get(zapytanie);
                GetUserPlayLists(responseYouTubeApi);
            } else if (status == 1) {
                string responseYouTubeApi = new Http().Get("https://www.googleapis.com/youtube/v3/channels?part=snippet%2CcontentDetails&" + id + "&maxResults=50&fields=items(contentDetails%2FrelatedPlaylists%2Fuploads%2Cid%2Csnippet%2Ftitle)%2CnextPageToken%2CpageInfo&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o");
                GetUserUploadedVideos(responseYouTubeApi);
            }
        }

        private void GetUserPlayLists(string json) {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = (Dictionary<string, object>) (result);

            Object[] val = (Object[]) obj2["items"];

            foreach (object item in val) {
                Dictionary<string, object> vid = (Dictionary<string, object>) item;
                Dictionary<string, object> temp = (Dictionary<string, object>) vid["snippet"];
                string playlistid = vid["id"].ToString();
                string playlistTitle = temp["title"].ToString();
                new PlayList(playlistid, playlistTitle);
            }
            try {
                string nextPage = obj2["nextPageToken"].ToString();
                GetUserPlayLists(new Http().Get("https://www.googleapis.com/youtube/v3/playlists?part=snippet&" + UserId + "&maxResults=50&pageToken=" + nextPage + "&fields=items(id%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
            catch { }
        }

        private void GetUserUploadedVideos(string json) {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = (Dictionary<string, object>) (result);

            Object[] val = (Object[]) obj2["items"];

            string id = "";
            string title = "";
            try {
                Dictionary<string, object> idJ = (Dictionary<string, object>) val.GetValue(0);
                id = idJ["id"].ToString();
                Object v = idJ["snippet"];
                Dictionary<string, object> vJ = (Dictionary<string, object>) v;
                title = vJ["title"].ToString();
            }
            catch { }
            id = id.Substring(2);
            id = "UU" + id;
            new PlayList(id, title);
        }

        private string GetUserId(string title) {
            string json = new Http().Get("https://www.googleapis.com/youtube/v3/channels?part=snippet&forUsername=" + title + "&fields=items%2Fid&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o");
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = (Dictionary<string, object>) (result);

            Object[] val = (Object[]) obj2["items"];
            string response = null;
            try {
                Dictionary<string, object> id = (Dictionary<string, object>) val.GetValue(0);
                response = id["id"].ToString();
            }
            catch { MessageBox.Show("Wystąpił jakiś błąd, lub link do kanału jest niepoprawny", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error); }
            return response;
        }
    }
}