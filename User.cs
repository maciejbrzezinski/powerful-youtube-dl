using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace powerful_youtube_dl
{
    class User
    {
        public string userID, userURL;


        public User(string link)
        {
            string id = link.Substring(link.IndexOf("nnel/") + 5);
            userID = id;
            userURL = link;
            getUserPlayLists(HTTP.GET("https://www.googleapis.com/youtube/v3/playlists?part=snippet&channelId=" + userID + "&maxResults=50&fields=items(id%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
        }

        private void getUserPlayLists(string json)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = new Dictionary<string, object>();
            obj2 = (Dictionary<string, object>)(result);

            System.Object[] val = (System.Object[])obj2["items"];
            
            foreach (object item in val)
            {
                Dictionary<string, object> vid = (Dictionary<string, object>)item;
                Dictionary<string, object> temp = (Dictionary<string, object>)vid["snippet"];
                string playlistid = vid["id"].ToString(); 
                string playlistTitle = temp["title"].ToString();
                PlayList toAdd = new PlayList(playlistid, playlistTitle);
            }
            try
            {
                string nextPage = obj2["nextPageToken"].ToString();
                getUserPlayLists(HTTP.GET(" https://www.googleapis.com/youtube/v3/playlists?part=snippet&channelId=" + userID + "&maxResults=50&pageToken=" + nextPage + "&fields=items(id%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o"));
            }
            catch { }
        }
    }
}
