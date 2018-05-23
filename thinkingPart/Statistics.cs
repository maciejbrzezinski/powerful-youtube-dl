using powerful_youtube_dl.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Threading;

namespace powerful_youtube_dl.thinkingPart {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Statistics {
        private static string _logPath = string.Empty;
        private static string _message = "";

        public Statistics() {
            _logPath = Settings.Default.logsDestination;
            DispatcherTimer checkingTimer = new DispatcherTimer();
            checkingTimer.Tick += SaveLog;
            checkingTimer.Interval = new TimeSpan(0, 0, 5);
            checkingTimer.Start();
        }

        private void SaveLog(object sender, EventArgs e) {
            DispatcherTimer timer = (DispatcherTimer) sender;
            if (Properties.Settings.Default.createLogs) {
                timer.Interval = new TimeSpan(0, 0, 5);
                _logPath = Settings.Default.logsDestination;
                if (_logPath != "") {
                    using (TextWriter w = File.AppendText(_logPath + "\\" + "Logs.txt"))
                        w.WriteLine(_message);
                }
            } else {
                timer.Interval = new TimeSpan(0, 0, 30);
            }
        }

        private static void GenerateLog(string operationName, string logMessage) {
            if (Settings.Default.createLogs) {
                _message += DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString();
                _message += " : " + operationName;
                _message += "\r\n";
                _message += "\r\n   " + logMessage;
                _message += "\r\n-------------------------------";
                _message += "\r\n";
            }
        }

        public static void CompleteDownload(Video video) {
            Settings.Default.sumDownloadedVideos++;
            string mess = "Pomyślnie pobrano: " + video.Position.Title + "\r\n                         (" + video.Position.Duration + ")\r\n                         " + video.Position.Link + "\r\n                         " + video.Position.Path;
            GenerateLog("Pobrano plik", mess);
        }

        public static void LoadedVideo(Video video) {
            string mess = "Pomyślnie załadowano: " + video.Position.Title + " \r\n                         (" + video.Position.Duration + ")\r\n                         " + video.Position.Link;
            GenerateLog("Załadowano film", mess);
        }

        public static void LoadedPlaylist(PlayList playList) {
            string mess = "Pomyślnie załadowano playlistę: " + playList.Position.Title + " (" + playList.ListOfVideosInPlayList.Count + " filmów)\r\n                         " + playList.Position.Link;
            GenerateLog("Załadowano playlistę", mess);
        }

        public static void NotWorkingVideo(Video video) {
            string mess = "Nie udało się pobrać informacji o filmie z ID: " + video.Position.Id;
            GenerateLog("Niedziałający film", mess);
        }

        public static void BeginDownload(Video video) {
            string mess = "Rozpoczęto pobieranie: " + video.Position.Title + "\r\n                         (" + video.Position.Duration + ")\r\n                         " + video.Position.Link + "\r\n                         " + video.Position.Path;
            GenerateLog("Pobieranie pliku", mess);
        }

        public static void DeletePlaylist(PlayList playList) {
            string mess = "Playlista: " + playList.Position.Title + "\r\n                         (" + playList.ListOfVideosInPlayList.Count + " filmów) została usunięta\r\n                         " + playList.Position.Link;
            GenerateLog("Usunięto playlistę", mess);
        }
    }
}