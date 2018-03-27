using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace powerful_youtube_dl.thinkingPart
{
    public class Statistics
    {
        public static string logPath = string.Empty;

        public Statistics()
        {
            logPath = Properties.Settings.Default.logsDestination;
        }

        public static void Log(string operationName, string logMessage, TextWriter txtWriter)
        {
            try
            {

                txtWriter.Write("\r\n" + "{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                txtWriter.WriteLine(" : " + operationName);
                txtWriter.WriteLine("  ");
                txtWriter.WriteLine("   {0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch { }
        }

        public static void GenerateLog(string operationName, string message)
        {
            if (Properties.Settings.Default.createLogs)
            {
                try
                {
                    using (StreamWriter w = File.AppendText(logPath + "\\" + "Logs.txt"))
                    {
                        Log(operationName, message, w);
                    }
                }
                catch { }
            }
        }

        public static void CompleteDownload(Video video)
        {
            Properties.Settings.Default.sumDownloadedVideos++;
            string message = "Pomyślnie pobrano: " + video.videoTitle + "\r\n                         (" + video.videoDuration + ")\r\n                         "+video.videoURL+ "\r\n                         " + video.downloadPath;
            GenerateLog("Pobrano plik", message);
        }

        public static void LoadedVideo(Video video)
        {
            string message = "Pomyślnie załadowano: " + video.videoTitle + " \r\n                         (" + video.videoDuration + ")\r\n                         "+video.videoURL;
            GenerateLog("Załadowano film", message);
        }

        public static void LoadedPlaylist(PlayList playList)
        {
            string message = "Pomyślnie załadowano playlistę: " + playList.playListTitle + " (" + playList._listOfVideosInPlayList.Count + " filmów)\r\n                         " + playList.playListURL;
            GenerateLog("Załadowano playlistę", message);
        }

        public static void NotWorkingVideo(Video video)
        {
            string message = "Nie udało się pobrać informacji o filmie z ID: " + video.videoID;
            GenerateLog("Niedziałający film", message);
        }

        public static void BeginDownload(Video video)
        {
            string message = "Rozpoczęto pobieranie: " + video.videoTitle + "\r\n(" + video.videoDuration + ")\r\n                         "+video.videoURL+ "\r\n                         " + video.downloadPath;
            GenerateLog("Pobieranie pliku", message);
        }

        public static void DeletePlaylist(PlayList playList)
        {
            string message = "Playlista: " + playList.playListTitle + "\r\n                         (" + playList._listOfVideosInPlayList.Count + " filmów) została usunięta\r\n                         " + playList.playListURL;
            GenerateLog("Usunięto playlistę", message);
        }
    }
}
