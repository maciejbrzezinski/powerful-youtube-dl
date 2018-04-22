using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows;

namespace powerful_youtube_dl.thinkingPart {

    internal class User {
        public string userID, userURL;

        public User(string link, int status) //(status) 0 - playlisty 1 - dodane
        {
            string id = "";

            if (link.Contains("/channel/")) {
                int start = link.IndexOf("nnel/") + 5;
                int finish = link.Substring(start).IndexOf("?");
                if (finish == -1)
                    finish = link.Substring(start).IndexOf("/");
                if (finish != -1)
                    userID = link.Substring(start, finish);
                else
                    userID = link.Substring(start);

                id = "id=" + userID;
            } else {
                string title;
                int start = link.IndexOf("user/") + 5;
                int finish = link.Substring(start).IndexOf("/");
                if (finish > 0)
                    title = link.Substring(start, finish);
                else
                    title = link.Substring(start);
                id = "forUsername=" + title;
                userID = getUserID(title);
            }

            userURL = link;
            if (status == 0) {
                string zapytanie = "https://www.googleapis.com/youtube/v3/playlists?part=snippet&channelId=" + userID + "&maxResults=50&fields=items(id%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o";
                string ResponseYouTubeAPI = new HTTP().GET(zapytanie);
                getUserPlayLists(ResponseYouTubeAPI);
            } else if (status == 1) {
                string ResponseYouTubeAPI = new HTTP().GET("https://www.googleapis.com/youtube/v3/channels?part=snippet%2CcontentDetails&" + id + "&maxResults=50&fields=items(contentDetails%2FrelatedPlaylists%2Fuploads%2Cid%2Csnippet%2Ftitle)%2CnextPageToken%2CpageInfo&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o");
                getUserUploadedVideos(ResponseYouTubeAPI);
            }
        }

        private void getUserPlayLists(string json) {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = new Dictionary<string, object>();
            obj2 = (Dictionary<string, object>) (result);

            System.Object[] val = (System.Object[]) obj2["items"];

            foreach (object item in val) {
                Dictionary<string, object> vid = (Dictionary<string, object>) item;
                Dictionary<string, object> temp = (Dictionary<string, object>) vid["snippet"];
                string playlistid = vid["id"].ToString();
                string playlistTitle = temp["title"].ToString();
                PlayList toAdd = new PlayList(playlistid, playlistTitle);
            }
            try {
                string nextPage = obj2["nextPageToken"].ToString();
                getUserPlayLists(new HTTP().GET("https://www.googleapis.com/youtube/v3/playlists?part=snippet&" + userID + "&maxResults=50&pageToken=" + nextPage + "&fields=items(id%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            } catch (Exception e) {
                Console.WriteLine("TUTAJ");
            }
        }

        private void getUserUploadedVideos(string json) {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = (Dictionary<string, object>) (result);

            System.Object[] val = (System.Object[]) obj2["items"];

            string id = "";
            string title = "";
            try {
                Dictionary<string, object> idJ = (Dictionary<string, object>) val.GetValue(0);
                id = idJ["id"].ToString();
                System.Object v = (System.Object) idJ["snippet"];
                Dictionary<string, object> vJ = (Dictionary<string, object>) v;
                title = vJ["title"].ToString();
            } catch (Exception e) {
                Console.WriteLine("TUTAJ");
            }
            id = id.Substring(2);
            id = "UU" + id;
            PlayList user = new PlayList(id, title);
        }

        private string getUserID(string title) {
            string json = new HTTP().GET("https://www.googleapis.com/youtube/v3/channels?part=snippet&forUsername=" + title + "&fields=items%2Fid&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o");
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = new Dictionary<string, object>();
            obj2 = (Dictionary<string, object>) (result);

            System.Object[] val = (System.Object[]) obj2["items"];
            string response = null;
            try {
                Dictionary<string, object> id = (Dictionary<string, object>) val.GetValue(0);
                response = id["id"].ToString();
            } catch { MessageBox.Show("Wystąpił jakiś błąd, lub link do kanału jest niepoprawny", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error); }
            return response;
        }
    }
}