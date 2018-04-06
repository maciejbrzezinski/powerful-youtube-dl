﻿using powerful_youtube_dl.thinkingPart;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace powerful_youtube_dl
{
    public class Video
    {
        public static List<Video> _listOfVideos = new List<Video>();

        public string videoID, videoTitle, videoDuration, videoURL;
        public PlayList playList = null;
        public ListViewItemMy position = null;
        public CheckBox checkbox;
        public static bool acceptDownload = false;
        public string downloadPath;
        public static bool isManualDownload = true;

        public static int currentlyDownloading = 0;

        public static List<Video> videoIDsToGetParams = new List<Video>();

        //public Video() {}

        public Video(string linkOrID)
        {
            string id;
            if (linkOrID.Length != 11)
            {
                id = linkOrID.Substring(linkOrID.IndexOf("v=") + 2, 11);
                videoURL = linkOrID;
            }
            else
            {
                id = linkOrID;
                videoURL = @"https://www.youtube.com/watch?v=" + id;
            }
            if (!isVideoLoaded(id))
            {

                videoID = id;
                addToGetParams(this);
                position = new ListViewItemMy { title = id, duration = videoDuration, status = "---", check = false, parent = this };
                _listOfVideos.Add(this);
            }
        }

        public static bool checkIfVideoIsOnDisk(Video video)
        {
            string path = "";
            if (Properties.Settings.Default.playlistAsFolder)
                path = Properties.Settings.Default.dlpath + "\\" + video.playList.ToString() + "\\" + video.ToString() + ".mp3";
            else
                path = Properties.Settings.Default.dlpath + "\\" + video.ToString() + ".mp3";
            if (File.Exists(path))
                return true;
            else
                return false;
        }

        private static void addToGetParams(Video v)
        {
            videoIDsToGetParams.Add(v);
        }

        public static void getParamsOfVideos()
        {
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
                    if (!checkIfVideoIsOnDisk(_listOfVideos[current]))
                        _listOfVideos[current].position.check = true;
                    else
                    {
                        _listOfVideos[current].position.status = "Pobrano";
                        if (Properties.Settings.Default.playlistAsFolder)
                            _listOfVideos[current].downloadPath = Properties.Settings.Default.dlpath + "\\" + _listOfVideos[current].playList.ToString() + "\\" + _listOfVideos[current].ToString() + ".mp3";
                        else
                            _listOfVideos[current].downloadPath = Properties.Settings.Default.dlpath + "\\" + _listOfVideos[current].ToString() + ".mp3";
                    }
                    Statistics.LoadedVideo(_listOfVideos[current]);
                }
            }
            removeNotWorkingVideos();
            if (Properties.Settings.Default.autoObservePlaylists && Properties.Settings.Default.autoDownloadObserve && !isManualDownload)
            {
                DownloadHandler.Load();
                DownloadHandler.DownloadQueue();
            }
        }

        private static void removeNotWorkingVideos()
        {
            foreach (PlayList p in PlayList._listOfPlayLists)
            {
                List<Video> newListOfVideos = new List<Video>();
                foreach (Video v in p._listOfVideosInPlayList)
                {
                    if (v.videoTitle != null && v.videoTitle != "")
                        newListOfVideos.Add(v);
                    else
                    {
                        Statistics.NotWorkingVideo(v);
                        ((MainWindow)System.Windows.Application.Current.MainWindow).deleteVideoFromAdd(v.position);
                    }
                }
                p._listOfVideosInPlayList = newListOfVideos;
            }
        }

        private static string decryptDuration(string jsonValue) // np. PT4M50S = 04:40
        {
            string tmp = jsonValue;
            tmp = tmp.Substring(2);
            string hours = "";
            string minutes = "00:";
            string seconds = "";
            try
            {
                int start = tmp.IndexOf("M") < 0 ? 0 : tmp.IndexOf("M") + 1;
                seconds = tmp.Substring(start, start > 0 ? tmp.IndexOf("S") - start : tmp.IndexOf("S"));
                start = tmp.IndexOf("H") < 0 ? 0 : tmp.IndexOf("H") + 1;
                minutes = tmp.Substring(start, start > 0 ? tmp.IndexOf("M") - start : tmp.IndexOf("M")) + ":";
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
            startInfo.FileName = Properties.Settings.Default.ytdlexe;
            if (Properties.Settings.Default.playlistAsFolder)
            {
                downloadPath = Properties.Settings.Default.dlpath + "\\" + playList.ToString() + "\\" + this.ToString() + ".mp3";
                startInfo.Arguments = " -x -o \"" + Properties.Settings.Default.dlpath + "\\" + playList.ToString() + "\\" + this.ToString() + ".mp3\" https://www.youtube.com/watch?v=" + videoID;
            }
            else
            {
                downloadPath = Properties.Settings.Default.dlpath + "\\" + this.ToString() + ".mp3";
                startInfo.Arguments = " -x -o \"" + Properties.Settings.Default.dlpath + "\\" + this.ToString() + ".mp3\" https://www.youtube.com/watch?v=" + videoID;
            }
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            process.OutputDataReceived += cmd_DataReceived;
            process.EnableRaisingEvents = true;
            process.StartInfo = startInfo;

            Thread ths = new Thread(() =>
            {
                while (true)
                {
                    if (isManualDownload || (!isManualDownload && Properties.Settings.Default.autoDownloadObserve))
                    {
                        if (currentlyDownloading < Properties.Settings.Default.maxDownloading)
                        {
                            currentlyDownloading++;
                            Statistics.BeginDownload(this);
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                              DispatcherPriority.Background,
                              new Action(() =>
                              {
                                  position.status = "Pobieranie ";
                                  ((MainWindow)System.Windows.Application.Current.MainWindow).addVideos.Items.Refresh();
                              }));
                            bool ret = process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            process.WaitForExit();

                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                              DispatcherPriority.Background,
                              new Action(() =>
                              {
                                  position.status = "Pobrano";
                                  position.check = false;
                                  if (Properties.Settings.Default.messageAfterDownload)
                                      MainWindow.showNotifyIconMessage("Pobrano plik", position.title + " został pobrany", System.Windows.Forms.ToolTipIcon.Info, 100);
                                  ((MainWindow)System.Windows.Application.Current.MainWindow).addVideos.Items.Refresh();
                              }));
                            Statistics.CompleteDownload(this);
                            currentlyDownloading--;
                            break;
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
            });
            ths.Start();
        }

        private void cmd_DataReceived(object sender, DataReceivedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                position.status = getPercent(e.Data);
                ((MainWindow)System.Windows.Application.Current.MainWindow).addVideos.Items.Refresh();
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
            toReturn = toReturn.Replace(@"|", @" ");
            return toReturn;
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
