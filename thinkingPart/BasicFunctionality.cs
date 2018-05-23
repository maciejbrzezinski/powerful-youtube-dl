using powerful_youtube_dl.Properties;
using powerful_youtube_dl.window;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;

namespace powerful_youtube_dl.thinkingPart {

    public class BasicFunctionality {
        public static Icon notifyIcon { get; set; }

        public static void Error(string err) {
            System.Windows.Forms.MessageBox.Show(err, @"Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ContextOpenYT(string link) {
            Process.Start(link);
        }

        public static void ContextOpenPath(string path) {
            string argument = "/select, \"" + path + "\"";
            Process.Start("explorer.exe", argument);
        }

        public static void CopyURL(string link) {
            System.Windows.Clipboard.SetText(link);
        }

        public static bool CheckIfPlayListExists(string id) {
            foreach (PlayList exists in PlayList.ListOfPlayLists) {
                if (exists.Position.Id == id) {
                    return true;
                }
            }
            return false;
        }

        public static void CheckChanged(bool? isChecked, PlayList playList) {
            int count = playList.ListOfVideosInPlayList.Count;
            for (int i = 0; i < count; i++) {
                if (isChecked != null)
                    playList.ListOfVideosInPlayList[i].Position.Check = isChecked;
                else
                    playList.ListOfVideosInPlayList[i].Position.Check = !playList.ListOfVideosInPlayList[i].IsDownloaded;
            }
        }

        public static void OpenFolderOrBrowserVideo(VideoView sender) {
            if (sender != null) {
                if (sender.ParentVideo.IsDownloaded) {
                    string path = sender.Path;
                    if (!File.Exists(path))
                        path += ".part";
                    string argument = "/select, \"" + path + "\"";
                    Process.Start("explorer.exe", argument);
                } else
                    Process.Start(sender.ParentVideo.Position.Link);
            }
        }

        public static bool CheckIfVideoIsOnDisk(Video video) {
            string path;
            if (Settings.Default.playlistAsFolder)
                path = Settings.Default.textDestination + "\\" + video.PlayList + "\\" + video + ".mp3";
            else
                path = Settings.Default.textDestination + "\\" + video + ".mp3";
            if (File.Exists(path))
                return true;
            return false;
        }

        public static void InvokeShit(DispatcherPriority priority, Action action) {
            if (System.Windows.Application.Current != null) {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(priority, action);
            } else
                Environment.Exit(0);
        }

        public static void ShowNotifyIconMessage(string title, string message, ToolTipIcon icon, int miliseconds, string path) {
            NotifyIcon popupIcon = MainWindow.Notify;
            popupIcon.BalloonTipText = message;
            popupIcon.BalloonTipIcon = icon;
            popupIcon.BalloonTipTitle = title;
            popupIcon.Visible = true;
            popupIcon.Icon = notifyIcon;
            popupIcon.BalloonTipClicked += (sender, args) => {
                if (!File.Exists(path))
                    path += ".part";
                string argument = "/select, \"" + path + "\"";
                Process.Start("explorer.exe", argument);
            };
            popupIcon.BalloonTipClosed += (sender, args) => {
                popupIcon.Icon = null;
                popupIcon.Visible = false;
                popupIcon.Dispose();
            };
            popupIcon.ShowBalloonTip(miliseconds);
        }

        public static bool CheckIfYoutubeUrl(string url) {
            if (url.Contains("playlist") || url.Contains("list") || url.Contains("watch") || url.Contains("channel") || url.Contains("user")) {
                if (url.Contains("youtu")) {
                    string test;

                    int start = url.IndexOf("v=", StringComparison.Ordinal);
                    int finish = url.Substring(start + 2).IndexOf("&", StringComparison.Ordinal);
                    if (start > -1 && finish < 0)
                        test = url.Substring(start + 2);
                    else if (start > -1 && finish > 0)
                        test = url.Substring(start + 2, finish);
                    else
                        test = "";
                    if (test.Length == 11)
                        return true;

                    start = url.IndexOf("list=", StringComparison.Ordinal);
                    finish = url.Substring(start + 5).IndexOf("&", StringComparison.Ordinal);
                    if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else if (start > -1 && finish > 0)
                        test = url.Substring(start + 5, finish);
                    else
                        test = "";
                    if (test.Length == 34 || test.Length == 24)
                        return true;

                    start = url.IndexOf("nnel/", StringComparison.Ordinal) + 5;
                    finish = url.Substring(start + 5).IndexOf("?", StringComparison.Ordinal);
                    if (finish == -1)
                        finish = url.Substring(start + 5).IndexOf("/", StringComparison.Ordinal);
                    if (finish > 0 && start > -1)
                        test = url.Substring(start + 5, finish);
                    else if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else
                        test = "";
                    if (test != "")
                        return true;

                    start = url.IndexOf("user/", StringComparison.Ordinal) + 5;
                    finish = url.Substring(start + 5).IndexOf("/", StringComparison.Ordinal);
                    if (finish > 0 && start > -1)
                        test = url.Substring(start + 5, finish);
                    else if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else
                        test = "";
                    if (test != "")
                        return true;
                }
            }
            return false;
        }
    }
}