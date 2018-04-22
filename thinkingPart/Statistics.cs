using System;
using System.IO;
using System.Security.AccessControl;

namespace powerful_youtube_dl.thinkingPart {

    public class Statistics {
        private static string logPath = string.Empty;
        private static string message = "";

        public Statistics() {
            logPath = Properties.Settings.Default.logsDestination;
            System.Windows.Threading.DispatcherTimer checkingTimer = new System.Windows.Threading.DispatcherTimer();
            checkingTimer.Tick += saveLog;
            checkingTimer.Interval = new TimeSpan(0, 0, 5);
            checkingTimer.Start();
        }

        private void saveLog(object sender, EventArgs e) {
            logPath = Properties.Settings.Default.logsDestination;
            if (logPath != "") {
                using (TextWriter w = File.AppendText(logPath + "\\" + "Logs.txt"))
                    w.WriteLine(message);
            }
        }

        private static void GenerateLog(string operationName, string logMessage) {
            if (Properties.Settings.Default.createLogs) {
                message += DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString();
                message += " : " + operationName;
                message += "\r\n";
                message += "\r\n   " + logMessage;
                message += "\r\n-------------------------------";
                message += "\r\n";
            }
        }

        public static void CompleteDownload(Video video) {
            Properties.Settings.Default.sumDownloadedVideos++;
            string message = "Pomyślnie pobrano: " + video.position.Title + "\r\n                         (" + video.position.Duration + ")\r\n                         " + video.position.Link + "\r\n                         " + video.downloadPath;
            GenerateLog("Pobrano plik", message);
        }

        public static void LoadedVideo(Video video) {
            string message = "Pomyślnie załadowano: " + video.position.Title + " \r\n                         (" + video.position.Duration + ")\r\n                         " + video.position.Link;
            GenerateLog("Załadowano film", message);
        }

        public static void LoadedPlaylist(PlayList playList) {
            string message = "Pomyślnie załadowano playlistę: " + playList.playListTitle + " (" + playList._listOfVideosInPlayList.Count + " filmów)\r\n                         " + playList.playListURL;
            GenerateLog("Załadowano playlistę", message);
        }

        public static void NotWorkingVideo(Video video) {
            string message = "Nie udało się pobrać informacji o filmie z ID: " + video.position.Id;
            GenerateLog("Niedziałający film", message);
        }

        public static void BeginDownload(Video video) {
            string message = "Rozpoczęto pobieranie: " + video.position.Title + "\r\n                         (" + video.position.Duration + ")\r\n                         " + video.position.Link + "\r\n                         " + video.downloadPath;
            GenerateLog("Pobieranie pliku", message);
        }

        public static void DeletePlaylist(PlayList playList) {
            string message = "Playlista: " + playList.playListTitle + "\r\n                         (" + playList._listOfVideosInPlayList.Count + " filmów) została usunięta\r\n                         " + playList.playListURL;
            GenerateLog("Usunięto playlistę", message);
        }
    }
}