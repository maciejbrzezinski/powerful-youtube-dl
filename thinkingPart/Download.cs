using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace powerful_youtube_dl
{
    public class Download
    {
        private static List<Video> toDownload = new List<Video>();

        public static void Load()
        {
            foreach (Video video in Video._listOfVideos)
            {
                if ((bool)video.position.check && !toDownload.Contains(video))
                {
                    ((MainWindow)System.Windows.Application.Current.MainWindow).addVideoToQueue(video.position);
                    toDownload.Add(video);
                }
            }
        }

        public static void Delete(int index)
        {
            try
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).deleteVideoFromQueue(index);
                toDownload.RemoveAt(index);
            }
            catch { }
        }

        public static void DownloadQueue()
        {
            if (CheckFields())
            {
                foreach (Video v in toDownload)
                {
                    v.Download();
                    Thread.Sleep(100);
                }
                toDownload = new List<Video>();
            }
        }

        public static bool CheckFields()
        {
            string response = "";
            bool allowDownload = true;
            int count = 1;
            if (!Properties.Settings.Default.ytdlexe.Contains("youtube-dl.exe"))
            {
                response += count + ". Nie wybrano youtube-dl.exe!\n\n";
                count++;
                allowDownload = false;
            }
            if (Properties.Settings.Default.dlpath == "")
            {
                response += count + ". Nie wybrano lokalizacji zapisywania plików!\n\n";
                count++;
                allowDownload = false;
            }
            if (toDownload.Count == 0)
            {
                response += count + ". Nie dodano żadnego elementu do pobrania!";
                count++;
                allowDownload = false;
            }
            if (response != "")
                MainWindow.Error(response);
            return allowDownload;
        }
    }
}
