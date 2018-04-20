using System.Collections.Generic;
using System.Threading.Tasks;

namespace powerful_youtube_dl
{
    public class DownloadHandler
    {
        public static List<Video> toDownload = new List<Video>();
        public static bool isDownloading = false;

        public static void Load()
        {
            toDownload = new List<Video>();
            foreach (Video video in Video._listOfVideos)
            {
                if (video.position != null && (bool) video.position.Check && !toDownload.Contains(video) && video.videoTitle != null)
                    toDownload.Add(video);
            }
        }

        public static void Delete(int index)
        {
            try
            {
                toDownload.RemoveAt(index);
            } catch { }
        }

        public static async Task DownloadQueueAsync()
        {
            if (CheckFields() && !isDownloading)
            {
                isDownloading = true;
                foreach (Video v in toDownload)
                {
                    again:
                    if (Video.queueToDownload < 15)
                    {
                        v.Download();
                    } else
                    {
                        await Task.Delay(3000);
                        goto again;
                    }
                }
                isDownloading = false;
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
            if (Properties.Settings.Default.textDestination == "")
            {
                response += count + ". Nie wybrano lokalizacji zapisywania plików!\n\n";
                count++;
                allowDownload = false;
            }
            if (toDownload.Count == 0)
                allowDownload = false;

            if (response != "")
                MainWindow.Error(response);
            return allowDownload;
        }
    }
}