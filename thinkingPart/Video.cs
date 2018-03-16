using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace powerful_youtube_dl
{
    public class Video
    {
        // public static ObservableCollection<CheckBox> _listOfVideosCheckBox { get; set; }

        public static List<Video> _listOfVideos = new List<Video>();

        public string videoID, videoTitle, videoDuration;
        public PlayList playList = null;
        public bool toDownload = false;
        public ListViewItemMy position = null;

        public CheckBox checkbox;


        public static List<Video> videoIDsToGetParams = new List<Video>();

        public Video()
        {
            // _listOfVideosCheckBox = new ObservableCollection<CheckBox>();
        }

        public Video(string linkOrID)
        {
            string id;
            if (linkOrID.Length != 11)
                id = linkOrID.Substring(linkOrID.IndexOf("v=") + 2, 11);
            else
                id = linkOrID;
            if (!isVideoLoaded(id))
            {
                videoID = id;
                addToGetParams(this);
                position = new ListViewItemMy { title = id, duration = videoDuration, status = "---", check = false };
                _listOfVideos.Add(this);
            }
        }

        private static void addToGetParams(Video v)
        {
            videoIDsToGetParams.Add(v);
        }

        public static void getParamsOfVideos()
        {
            //string IDs = "";
            List<string> IDs = new List<string>();
            IDs.Add("");
            int ktoryJuz = 0;

            for (int i = 0; i < videoIDsToGetParams.Count; i++)
            {
                int index = IDs.Count - 1;
                if (ktoryJuz == 0)
                    IDs[index] = videoIDsToGetParams[i].videoID;
                else
                    IDs[index] += @"%2C" + videoIDsToGetParams[i].videoID;
                ktoryJuz++;
                if (ktoryJuz == 50)
                {
                    IDs.Add("");
                    ktoryJuz = 0;
                }
            }
            for (int j = 0; j < IDs.Count; j++)
            {
                string json = HTTP.GET("https://www.googleapis.com/youtube/v3/videos?part=snippet%2CcontentDetails&id=" + IDs[j] + "&fields=items(contentDetails%2Fduration%2Cid%2Csnippet%2Ftitle)&key=AIzaSyAa33VM7zG0hnceZEEGdroB6DerP8fRJ6o");

                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                var result = jsSerializer.DeserializeObject(json);
                Dictionary<string, object> obj2 = new Dictionary<string, object>();
                obj2 = (Dictionary<string, object>)(result);

                System.Object[] val = (System.Object[])obj2["items"];

                for (int i = 0; i < val.Length; i++)
                {
                    int current = -1;
                    int current2 = -1;
                    Dictionary<string, object> vid = (Dictionary<string, object>)val[i];
                    string id = vid["id"].ToString();
                    for (int d = 0; d < _listOfVideos.Count; d++)
                    {
                        if (_listOfVideos[d].videoID == id)
                        {
                            current = d;
                            break;
                        }
                    }
                    for (int d = 0; d < videoIDsToGetParams.Count; d++)
                    {
                        if (videoIDsToGetParams[d].videoID == id)
                        {
                            current2 = d;
                            break;
                        }
                    }
                    Dictionary<string, object> temp = (Dictionary<string, object>)vid["snippet"];
                    Dictionary<string, object> temp2 = (Dictionary<string, object>)vid["contentDetails"];
                    videoIDsToGetParams[current2].videoTitle = temp["title"].ToString();
                    _listOfVideos[current].videoTitle = temp["title"].ToString();
                    _listOfVideos[current].videoDuration = decryptDuration(temp2["duration"].ToString());
                    if (_listOfVideos[current].position == null)
                    {
                        _listOfVideos[current].position = new ListViewItemMy
                        {
                            title = _listOfVideos[current].position.title
                            ,
                            duration = _listOfVideos[current].position.duration
                            ,
                            status = "---"
                        };
                    }
                    _listOfVideos[current].position.title = _listOfVideos[current].videoTitle;
                    _listOfVideos[current].position.duration = _listOfVideos[current].videoDuration;
                }
            }
            removeNotWorkingVideos();
        }

        private static void removeNotWorkingVideos()
        {
            foreach (PlayList p in PlayList._listOfPlayLists)
            {
                List<Video> newListOfVideos = new List<Video>();
                foreach (Video v in p._listOfVideosInPlayList)
                {
                    if (v.videoTitle != null)
                        newListOfVideos.Add(v);
                    else
                        ((MainWindow)System.Windows.Application.Current.MainWindow).deleteVideoFromAdd(v.position);
                }
                p._listOfVideosInPlayList = newListOfVideos;
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
                seconds = tmp.Substring(tmp.IndexOf("M") < 0 ? 0 : tmp.IndexOf("M") + 1, tmp.IndexOf("S") - 2);
                minutes = tmp.Substring(tmp.IndexOf("H") > 0 ? 0 : tmp.IndexOf("H") + 1, tmp.IndexOf("M")) + ":";
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
            startInfo.Arguments = " -x -o \"" + MainWindow.downloadPath + "\\" + playList.ToString() + "\\" + this.ToString() + ".mp3\" https://www.youtube.com/watch?v=" + videoID;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            process.OutputDataReceived += cmd_DataReceived;
            process.EnableRaisingEvents = true;

            process.StartInfo = startInfo;
            //process.Start();
            Thread ths = new Thread(() =>
            {
                bool ret = process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() =>
                  {
                      int ind = ((MainWindow)System.Windows.Application.Current.MainWindow).kolejka.Items.IndexOf(position);
                      if (ind > -1)
                      {
                          ((MainWindow)System.Windows.Application.Current.MainWindow).kolejka.Items.RemoveAt(ind);
                          ((MainWindow)System.Windows.Application.Current.MainWindow).kolejka.Items.Refresh();
                      }
                  }));
            });
            ths.Start();
        }

        private void cmd_DataReceived(object sender, DataReceivedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
      DispatcherPriority.Background,
       new Action(() =>
       {
           position.status = getPercent(e.Data);
           ((MainWindow)System.Windows.Application.Current.MainWindow).kolejka.Items.Refresh();
       }));
            Console.WriteLine(e.Data);
        }

        private static string getPercent(string value)
        {
            if (value != null)
            {
                string start = "[download]   ";
                string end = " of";
                int st = value.IndexOf(start) + start.Length;
                int en = value.IndexOf(end);
                if (st > -1 && en > st)
                {
                    value = value.Substring(st, 5);
                    return value;
                }
            }
            return "---";
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
