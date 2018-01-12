﻿using System;
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
            if (CheckFields())
            {
                foreach (Video v in toDownload)
                    v.Download();
            }
        }

        public static bool CheckFields()
        {
            string response = "";
            bool allowDownload = true;
            int count = 1;
            if (!MainWindow.ytDlPath.Contains("youtube-dl.exe"))
            {
                response += count + ". Nie wybrano youtube-dl.exe!\n\n";
                count++;
                allowDownload = false;
            }
            if (MainWindow.downloadPath == "")
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
