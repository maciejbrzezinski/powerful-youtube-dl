using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace powerful_youtube_dl
{
    public class Download
    {
        public static ObservableCollection<string> _listOfVideosToDownload { get; set; }

        private static List<Video> toDownload = new List<Video>();

        public Download() { _listOfVideosToDownload = new ObservableCollection<string>(); }

        public static void Load()
        {
            foreach (Video video in Video._listOfVideos)
            {
                if ((bool)video.check.IsChecked && !toDownload.Contains(video))
                {
                    _listOfVideosToDownload.Add(video.ToString());
                    toDownload.Add(video);
                }
            }
        }

        public static void Delete(int index)
        {
            _listOfVideosToDownload.RemoveAt(index);
            toDownload.RemoveAt(index);
        }

        public static void DownloadQueue()
        {
            foreach (Video v in toDownload)
                v.Download();
        }
    }
}
