using powerful_youtube_dl.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace powerful_youtube_dl.thinkingPart {

    public class DownloadHandler {
        public static List<Video> ToDownload = new List<Video>();
        public static bool IsDownloading;

        public static void Load(PlayList list) {
            foreach (Video video in list.ListOfVideosInPlayList) {
                if (video.Position != null && video.Position.CheckBool && !ToDownload.Contains(video) && video.Position.Title != null)
                    ToDownload.Add(video);
            }
        }

        public static void Load() {
            foreach (Video video in Video.ListOfVideos) {
                if (video.Position != null && video.Position.CheckBool && !ToDownload.Contains(video) && video.Position.Title != null)
                    ToDownload.Add(video);
            }
        }

        public static void Load(Video video) {
            if (video.Position != null && video.Position.CheckBool && !ToDownload.Contains(video) && video.Position.Title != null)
                ToDownload.Add(video);
        }

        public static async Task DownloadQueueAsync() {
            if (CheckFields() && !IsDownloading) {
                IsDownloading = true;
                while (true) {
                    if (ToDownload.Count > 0) {
                        again:
                        if (Video.QueueToDownload < 15) {
                            ToDownload[0].Download();
                            ToDownload.RemoveAt(0);
                        } else {
                            await Task.Delay(1000);
                            goto again;
                        }
                    } else
                        break;
                }
                IsDownloading = false;
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
            if (ToDownload.Count == 0)
                allowDownload = false;

            if (response != "")
                window.MainWindow.Error(response);
            return allowDownload;
        }
    }
}