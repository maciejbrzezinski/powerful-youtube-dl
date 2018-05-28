using powerful_youtube_dl.Properties;
using powerful_youtube_dl.window;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

namespace powerful_youtube_dl.thinkingPart {

    public class Video : BasicFunctionality {
        public static List<Video> ListOfVideos = new List<Video>();

        public static int CurrentlyDownloading, QueueToDownload;
        public static bool IsManualDownload = true;
        public PlayList PlayList;
        public VideoView Position;
        public bool IsDownloaded;

        private string _lastPercent = "";
        private string _lastMessage = "";

        public Video(string linkOrId, PlayList list) {
            string id;
            if (linkOrId.Length != 11) {
                Position = new VideoView();
                id = linkOrId.Substring(linkOrId.IndexOf("v=", StringComparison.Ordinal) + 2, 11);
                Position.Link = linkOrId;
            } else {
                Position = new VideoView();
                id = linkOrId;
                Position.Link = @"https://www.youtube.com/watch?v=" + id;
            }
            if (!IsVideoLoaded(id) && Position!=null) {
                Position.Id = id;
                PlayList = list;
                Position.Status = "---";
                Position.Check = false;
                Position.ParentVideo = this;
                Position.ParentPlaylist = list;
                list.AddToGetParams(this);
                list.ListOfVideosInPlayList.Add(this);
                ListOfVideos.Add(this);
                list.Position.CountVideos += 1;
            }
        }

        public Video(FileInfo fileInfo) {

        }

        public void Download() {
            QueueToDownload++;
            if (IsDownloaded) {
                QueueToDownload--;
                Position.Check = false;
                return;
            }
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo { FileName = Settings.Default.ytdlexe };
            if (Settings.Default.playlistAsFolder) {
                Position.Path = Settings.Default.textDestination + "\\" + PlayList + "\\" + ToString() + ".mp3";
                startInfo.Arguments = " -x -o \"" + Settings.Default.textDestination + "\\" + PlayList + "\\" + ToString() + ".mp3\" https://www.youtube.com/watch?v=" + Position.Id;
            } else {
                Position.Path = Settings.Default.textDestination + "\\" + ToString() + ".mp3";
                startInfo.Arguments = " -x -o \"" + Settings.Default.textDestination + "\\" + ToString() + ".mp3\" https://www.youtube.com/watch?v=" + Position.Id;
            }
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            process.EnableRaisingEvents = true;
            process.StartInfo = startInfo;
            Timer aTimer = new Timer();
            aTimer.Elapsed += (sender, e) => {
                if (!IsDownloaded && (IsManualDownload || (!IsManualDownload && Settings.Default.autoDownloadObserve))) {
                    if (CurrentlyDownloading < Settings.Default.maxDownloading && !IsDownloaded) {
                        QueueToDownload--;
                        CurrentlyDownloading++;
                        Statistics.BeginDownload(this);

                        process.Start();

                        string loger;
                        while ((loger = process.StandardOutput.ReadLine()) != null)
                            cmd_DataReceived(loger);

                        process.WaitForExit();

                        CurrentlyDownloading--;

                        if (_lastMessage != null && _lastMessage.Contains("Downloading video info webpage")) {
                            InvokeShit(DispatcherPriority.Send, async () => {
                                Position.Check = false;
                                ((MainWindow) Application.Current.MainWindow)?.DeleteVideoFromAdd(Position, PlayList.Position.Id);
                                PlayList.ListOfVideosInPlayList.Remove(this);
                                PlayList.Position.CountVideos -= 1;
                            });
                        } else {
                            if (File.Exists(Position.Path + ".part")) {
                                aTimer.Enabled = true;
                                return;
                            } else if (File.Exists(Position.Path)) {
                                Position.File = TagLib.File.Create(Position.Path, "taglib/mp3", TagLib.ReadStyle.None);
                                FinishDownload();
                                Statistics.CompleteDownload(this);
                            }
                        }
                        aTimer.Close();
                    }
                }
                if (!IsDownloaded)
                    aTimer.Enabled = true;
            };
            aTimer.Interval = 4000;
            aTimer.Enabled = true;
        }

        private void FinishDownload() {
            InvokeShit(DispatcherPriority.Normal, () => {
                Position.Status = "Pobrano";
                Position.Check = false;
                if (Settings.Default.messageAfterDownload)
                    BasicFunctionality.ShowNotifyIconMessage("Pobrano plik | " + PlayList, Position.Title, ToolTipIcon.Info, 100, Position.Path);
            });
            WriteID3Title(Position.File, Position.Id);
            Statistics.CompleteDownload(this);
        }

        override
        public string ToString() {
            if (Position.Title != null) {
                string toReturn = Position.Title;
                toReturn = toReturn.Replace(@"\", @" ");
                toReturn = toReturn.Replace(@"/", @" ");
                toReturn = toReturn.Replace(@"|", @" ");
                toReturn = toReturn.Replace(@":", @" ");
                toReturn = toReturn.Replace("\"", @" ");
                toReturn = toReturn.Replace("%", @" ");
                toReturn = toReturn.Replace("?", @" ");
                toReturn = toReturn.Replace("*", @" ");
                return toReturn;
            }

            if (Position.Id != null)
                return Position.Id;
            return "";
        }

        private static string GetPercent(string value) {
            if (value != null) {
                string start = "[download]   ";
                string end = " of";
                int st = value.IndexOf(start, StringComparison.Ordinal) + start.Length;
                int en = value.IndexOf(end, StringComparison.Ordinal);
                if (st > -1 && en > st) {
                    value = value.Substring(st, 5);
                    return value;
                }
            }
            return "---";
        }

        private void cmd_DataReceived(string value) {
            if (_lastMessage != null && _lastMessage != value) {
                _lastMessage = value;
                try {
                    InvokeShit(DispatcherPriority.Send, async () => {
                        string prc = GetPercent(value);
                        if (prc != "---" && _lastPercent != null && _lastPercent != prc) {
                            Console.WriteLine(value);
                            _lastPercent = prc;
                            Position.Status = prc;
                        }
                    });
                }
                catch { }
            }
        }

        public static bool IsVideoLoaded(string id) {
            for (int i = 0; i < ListOfVideos.Count; i++) {
                if (ListOfVideos[i] != null && ListOfVideos[i].Position != null && ListOfVideos[i].Position.Id == id)
                    return true;
            }
            return false;
        }

        public void ContextDeleteVideo() {
            Position.Check = false;
            ((MainWindow) Application.Current.MainWindow)?.DeleteVideoFromAdd(Position, PlayList.Position.Id);
            PlayList.ListOfVideosInPlayList.Remove(this);
            PlayList.Position.CountVideos -= 1;
        }

        public void ContextPlayVideo() {
            if (Position.Path != null)
                Process.Start(Position.Path);
        }
    }
}