using powerful_youtube_dl.thinkingPart;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows.Threading;

namespace powerful_youtube_dl {

    public class Video {
        public static List<Video> _listOfVideos = new List<Video>();

        public static int currentlyDownloading = 0;
        public static bool isManualDownload = true;
        public static int queueToDownload = 0;
        public string downloadPath;
        public PlayList playList = null;
        public ListViewItemMy position = null;

        //public string videoID, videoTitle, videoDuration, videoURL;
        private string lastMessage = "";

        private string lastPercent = "";

        public Video(string linkOrID, PlayList list) {
            position = new ListViewItemMy();
            string id;
            if (linkOrID.Length != 11) {
                id = linkOrID.Substring(linkOrID.IndexOf("v=") + 2, 11);
                position.Link = linkOrID;
            } else {
                id = linkOrID;
                position.Link = @"https://www.youtube.com/watch?v=" + id;
            }
            if (!isVideoLoaded(id)) {
                position.Id = id;
                playList = list;
                position.Status = "---";
                position.Check = false;
                position.Parent = this;
                list.addToGetParams(this);
                list._listOfVideosInPlayList.Add(this);
                _listOfVideos.Add(this);
            }
        }

        public void Download() {
            queueToDownload++;
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Properties.Settings.Default.ytdlexe;
            if (Properties.Settings.Default.playlistAsFolder) {
                downloadPath = Properties.Settings.Default.textDestination + "\\" + playList.ToString() + "\\" + this.ToString() + ".mp3";
                startInfo.Arguments = " -x -o \"" + Properties.Settings.Default.textDestination + "\\" + playList.ToString() + "\\" + this.ToString() + ".mp3\" https://www.youtube.com/watch?v=" + position.Id;
            } else {
                downloadPath = Properties.Settings.Default.textDestination + "\\" + this.ToString() + ".mp3";
                startInfo.Arguments = " -x -o \"" + Properties.Settings.Default.textDestination + "\\" + this.ToString() + ".mp3\" https://www.youtube.com/watch?v=" + position.Id;
            }
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            process.EnableRaisingEvents = true;
            process.StartInfo = startInfo;
            Timer aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) => {
                if (isManualDownload || (!isManualDownload && Properties.Settings.Default.autoDownloadObserve)) {
                    if (currentlyDownloading < Properties.Settings.Default.maxDownloading) {
                        queueToDownload--;
                        currentlyDownloading++;
                        Statistics.BeginDownload(this);
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                          DispatcherPriority.Normal,
                          new Action(() => {
                              position.Status = "Pobieranie ";
                          }));
                        bool ret = process.Start();

                        string loger;
                        while ((loger = process.StandardOutput.ReadLine()) != null) {
                            cmd_DataReceived(loger);
                        }

                        process.WaitForExit();

                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                          DispatcherPriority.Normal,
                          new Action(() => {
                              position.Status = "Pobrano";
                              position.Check = false;
                              if (Properties.Settings.Default.messageAfterDownload)
                                  MainWindow.showNotifyIconMessage("Pobrano plik", position.Title + " został pobrany", System.Windows.Forms.ToolTipIcon.Info, 100);
                          }));

                        Statistics.CompleteDownload(this);
                        currentlyDownloading--;
                        try {
                            aTimer.AutoReset = false;
                            aTimer.Enabled = false;
                            aTimer.Close();
                        } catch (Exception exc) {
                            Console.WriteLine("TUTAJ");
                        }
                    }
                }
            });
            aTimer.Interval = 1000;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        override
        public string ToString() {
            string toReturn = position.Title;
            toReturn = toReturn.Replace(@"\", @" ");
            toReturn = toReturn.Replace(@"/", @" ");
            toReturn = toReturn.Replace(@"|", @" ");
            toReturn = toReturn.Replace(@":", @" ");
            toReturn = toReturn.Replace("\"", @" ");
            return toReturn;
        }

        private static string getPercent(string value) {
            if (value != null) {
                string start = "[download]   ";
                string end = " of";
                int st = value.IndexOf(start) + start.Length;
                int en = value.IndexOf(end);
                if (st > -1 && en > st) {
                    value = value.Substring(st, 5);
                    return value;
                }
            }
            return "---";
        }

        private void cmd_DataReceived(string value) {
            if (lastMessage != null && lastMessage != value) {
                lastMessage = value;
                try {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                        string prc = getPercent(value);
                        if (prc != "---" && lastPercent != null && lastPercent != prc) {
                            Console.WriteLine(value);
                            lastPercent = prc;
                            position.Status = prc;
                        }
                    }));
                } catch (Exception e) {
                    Console.WriteLine("KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK" + e.Message);
                }
            }
        }

        private bool isVideoLoaded(string id) {
            for (int i = 0; i < _listOfVideos.Count; i++) {
                if (_listOfVideos[i].position.Id == id)
                    return true;
            }
            return false;
        }
    }
}