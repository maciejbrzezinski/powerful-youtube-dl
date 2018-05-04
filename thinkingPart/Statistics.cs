using powerful_youtube_dl.Properties;
using System;
using System.IO;
using System.Windows.Threading;

namespace powerful_youtube_dl.thinkingPart {

    public class Statistics {
        private static string logPath = string.Empty;
        private static string message = "";

        public Statistics() {
            logPath = Settings.Default.logsDestination;
            DispatcherTimer checkingTimer = new DispatcherTimer();
            checkingTimer.Tick += saveLog;
            checkingTimer.Interval = new TimeSpan(0, 0, 5);
            checkingTimer.Start();
        }

        private void saveLog(object sender, EventArgs e) {
            logPath = Settings.Default.logsDestination;
            if (logPath != "") {
                using (TextWriter w = File.AppendText(logPath + "\\" + "Logs.txt"))
                    w.WriteLine(message);
            }
        }

        private static void GenerateLog(string operationName, string logMessage) {
            if (Settings.Default.createLogs) {
                message += DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString();
                message += " : " + operationName;
                message += "\r\n";
                message += "\r\n   " + logMessage;
                message += "\r\n-------------------------------";
                message += "\r\n";
            }
        }

        public static void CompleteDownload(Video video) {
            Settings.Default.sumDownloadedVideos++;
            string mess = "Pomyślnie pobrano: " + video.position.Title + "\r\n                         (" + video.position.Duration + ")\r\n                         " + video.position.Link + "\r\n                         " + video.downloadPath;
            GenerateLog("Pobrano plik", mess);
        }

        public static void LoadedVideo(Video video) {
            string mess = "Pomyślnie załadowano: " + video.position.Title + " \r\n                         (" + video.position.Duration + ")\r\n                         " + video.position.Link;
            GenerateLog("Załadowano film", mess);
        }

        public static void LoadedPlaylist(PlayList playList) {
            string mess = "Pomyślnie załadowano playlistę: " + playList.position.Title + " (" + playList._listOfVideosInPlayList.Count + " filmów)\r\n                         " + playList.position.Link;
            GenerateLog("Załadowano playlistę", mess);
        }

        public static void NotWorkingVideo(Video video) {
            string mess = "Nie udało się pobrać informacji o filmie z ID: " + video.position.Id;
            GenerateLog("Niedziałający film", mess);
        }

        public static void BeginDownload(Video video) {
            string mess = "Rozpoczęto pobieranie: " + video.position.Title + "\r\n                         (" + video.position.Duration + ")\r\n                         " + video.position.Link + "\r\n                         " + video.downloadPath;
            GenerateLog("Pobieranie pliku", mess);
        }

        public static void DeletePlaylist(PlayList playList) {
            string mess = "Playlista: " + playList.position.Title + "\r\n                         (" + playList._listOfVideosInPlayList.Count + " filmów) została usunięta\r\n                         " + playList.position.Link;
            GenerateLog("Usunięto playlistę", mess);
        }
    }
}