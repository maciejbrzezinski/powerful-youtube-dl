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
        private string m_exePath = string.Empty;

        public Statistics()
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public void Log(string operationName, string logMessage, TextWriter txtWriter)
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

        private void generateLog(string operationName, string message)
        {
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "Logs.txt"))
                {
                    Log(operationName, message, w);
                }
            }
            catch { }
        }

        public void completeDownload(Video video)
        {
            Properties.Settings.Default.sumDownloadedVideos++;
            string message = "Pomyślnie pobrano: " + video.videoTitle + " (" + video.videoDuration + ")\r\n   " + video.downloadPath;
            generateLog("Pobrano plik", message);
        }

        public void loadedVideo(Video video)
        {
            string message = "Pomyślnie załadowano: " + video.videoTitle + " (" + video.videoDuration + ")";
            generateLog("Załadowano film", message);
        }

        public void loadedPlaylist(PlayList playList)
        {
            string message = "Pomyślnie załadowano playlistę: " + playList.playListTitle + " (" + playList._listOfVideosInPlayList.Count + " filmów)";
            generateLog("Załadowano playlistę", message);
        }

        public void notWorkingVideo(Video video)
        {
            string message = "Nie udało się pobrać informacji o filmie z ID: " + video.videoID;
            generateLog("Niedziałający film", message);
        }

        public void beginDownload(Video video)
        {
            string message = "Rozpoczęto pobieranie: " + video.videoTitle + " (" + video.videoDuration + ")\r\n   " + video.downloadPath;
            generateLog("Pobieranie pliku", message);
        }
    }
}
