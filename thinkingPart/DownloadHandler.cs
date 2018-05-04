using System.Collections.Generic;
using System.Threading.Tasks;
using powerful_youtube_dl.Properties;

namespace powerful_youtube_dl.thinkingPart {

    public class DownloadHandler {
        public static List<Video> toDownload = new List<Video>();
        public static bool isDownloading;

        public static void Load(PlayList list) {
            foreach (Video video in list._listOfVideosInPlayList) {
                if (video.position != null && video.position.CheckBool && !toDownload.Contains(video) && video.position.Title != null)
                    toDownload.Add(video);
            }
        }

        public static void Load() {
            foreach (Video video in Video._listOfVideos) {
                if (video.position != null && video.position.CheckBool && !toDownload.Contains(video) && video.position.Title != null)
                    toDownload.Add(video);
            }
        }

        public static void Load(Video video) {
            if (video.position != null && video.position.CheckBool && !toDownload.Contains(video) && video.position.Title != null)
                toDownload.Add(video);
        }

        public static async Task DownloadQueueAsync() {
            if (CheckFields() && !isDownloading) {
                isDownloading = true;
                while (true) {
                    if (toDownload.Count > 0) {
                        again:
                        if (Video.queueToDownload < 15) {
                            toDownload[0].Download();
                            toDownload.RemoveAt(0);
                        } else {
                            await Task.Delay(3000);
                            goto again;
                        }
                    } else
                        break;
                }
                isDownloading = false;
            }
        }

        public static bool CheckFields() {
            string response = "";
            bool allowDownload = true;
            int count = 1;
            if (!Settings.Default.ytdlexe.Contains("youtube-dl.exe")) {
                response += count + ". Nie wybrano youtube-dl.exe!\n\n";
                count++;
                allowDownload = false;
            }
            if (Settings.Default.textDestination == "") {
                response += count + ". Nie wybrano lokalizacji zapisywania plików!\n\n";
                allowDownload = false;
            }
            if (toDownload.Count == 0)
                allowDownload = false;

            if (response != "")
                window.MainWindow.Error(response);
            return allowDownload;
        }
    }
}