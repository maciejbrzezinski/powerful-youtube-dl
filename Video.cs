using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;


namespace powerful_youtube_dl
{
    public class Video
    {
        public static ObservableCollection<CheckBox> _listOfVideosCheckBox { get; set; }

        public static List<Video> _listOfVideos = new List<Video>();
        public string videoID, videoTitle, videoDuration;
        public string playList = "Pojedyncze";
        public bool toDownload = false;
        public ListViewItemMy position = null;

        public CheckBox check;

        public static List<Video> videoIDsToGetParams = new List<Video>();

        public Video()
        {
            _listOfVideosCheckBox = new ObservableCollection<CheckBox>();
        }

        public Video(string linkOrID)
        {
            string id;
            if (linkOrID.Length != 11)
                id = linkOrID.Substring(linkOrID.IndexOf("v=") + 2, 11);
            else
                id = linkOrID;
            videoID = id;
            addToGetParams(this);
            check = new CheckBox();
            check.Click += new RoutedEventHandler(checkChanged);
            check.Content = videoTitle;
            _listOfVideos.Add(this);
            _listOfVideosCheckBox.Add(check);
        }

      /*  public Video(string id, string title)
        {
            if (!isVideoLoaded(id))
            {
                videoID = id;
                videoTitle = title;
                videoDuration = getDuration(videoID);
                check = new CheckBox();
                check.Click += new RoutedEventHandler(checkChanged);
                check.Content = videoTitle;
                _listOfVideos.Add(this);
                _listOfVideosCheckBox.Add(check);
            }
        }*/

        private static void addToGetParams(Video v)
        {
            videoIDsToGetParams.Add(v);
        }

        public static void getParamsOfVideos()
        {
            string IDs = "";
            for (int i = 0; i < videoIDsToGetParams.Count; i++)
            {
                if (i == 0)
                    IDs = videoIDsToGetParams[i].videoID;
                else
                    IDs += @"%2C" + videoIDsToGetParams[i].videoID;
            }
            string json = HTTP.GET("https://www.googleapis.com/youtube/v3/videos?part=snippet%2CcontentDetails&id="+IDs+"&fields=items(contentDetails%2Fduration%2Cid%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o");

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = new Dictionary<string, object>();
            obj2 = (Dictionary<string, object>)(result);

            System.Object[] val = (System.Object[])obj2["items"];

            for (int i = 0; i < val.Length; i++)
            {
                Dictionary<string, object> vid = (Dictionary<string, object>)val[i];
                Dictionary<string, object> temp = (Dictionary<string, object>)vid["snippet"];
                Dictionary<string, object> temp2 = (Dictionary<string, object>)vid["contentDetails"];

                videoIDsToGetParams[i].videoTitle = temp["title"].ToString();
                videoIDsToGetParams[i].videoDuration = decryptDuration(temp2["duration"].ToString());
                videoIDsToGetParams[i].check.Content = videoIDsToGetParams[i].videoTitle;
            }
        }

        private static string decryptDuration(string jsonValue) // np. PT4M50S = 04:40
        {
            string tmp = jsonValue;
            tmp = tmp.Substring(2);
            string hours = "";
            string minutes = "";
            string seconds = "";
            try
            {
                 seconds = tmp.Substring(tmp.IndexOf("M") < 0 ? 0 : tmp.IndexOf("M")+1, tmp.IndexOf("S")-2);
                 minutes = tmp.Substring(tmp.IndexOf("H") > 0 ? 0 : tmp.IndexOf("H")+1, tmp.IndexOf("M")) + ":";
                 hours = tmp.Substring(0, tmp.IndexOf("H")) + ":";
            }
            catch { }
            if (seconds.Length == 1)
                seconds = "0" + seconds;
            if (minutes.Length == 2)
                minutes = "0" + minutes;
            if (hours.Length == 2)
                hours = "0" + hours;
            return hours + minutes + seconds;
        }

        private string queryCMD(string query)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = query;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
        }

        public void Download()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = MainWindow.ytDlPath;
            startInfo.Arguments = " -x -o \"" + MainWindow.downloadPath + "\\" + playList + "\\" + this.ToString() + ".mp3\" https://www.youtube.com/watch?v=" + videoID;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        override
        public string ToString()
        {
            string toReturn = videoTitle;
            toReturn = toReturn.Replace(@"\", @" ");
            toReturn = toReturn.Replace(@"/", @" ");
            return toReturn;
        }

        private void checkChanged(object sender, RoutedEventArgs e)
        {
            toDownload = (bool)((CheckBox)sender).IsChecked;
        }

        private bool isVideoLoaded(string id)
        {
            foreach (Video v in _listOfVideos)
            {
                if (v.videoID == id)
                    return true;
            }
            return false;
        }

    }
}
