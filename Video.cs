using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace powerful_youtube_dl
{
    public class Video
    {
        public static ObservableCollection<CheckBox> _listOfVideosCheckBox { get; set; }

        public static List<Video> _listOfVideos = new List<Video>();
        public string videoID, videoTitle;
        public bool toDownload = false;

        public CheckBox check;

        public Video()
        {
            _listOfVideosCheckBox = new ObservableCollection<CheckBox>();
        }

        public Video(string link)
        {
            string id;
            id = link.Substring(link.IndexOf("v=") + 2, 11);
            videoID = id;
            videoTitle = getTitle(id);
            check = new CheckBox();
            check.Click += new RoutedEventHandler(checkChanged);
            check.Content = videoTitle;
            _listOfVideos.Add(this);
            _listOfVideosCheckBox.Add(check);
        }

        public Video(string id, string title)
        {
            videoID = id;
            videoTitle = title;
            check = new CheckBox();
            check.Click += new RoutedEventHandler(checkChanged);
            check.Content = videoTitle;
            _listOfVideos.Add(this);
            _listOfVideosCheckBox.Add(check);
        }

        private string getTitle(string id)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c D:\\youtube-dl.exe --get-title \"https://www.youtube.com/watch?v=" + id + " /T";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(); var output = process.StandardOutput.ReadToEnd();
            return output;
        }

        override
        public string ToString()
        {
            return videoTitle;
        }

        private void checkChanged(object sender, RoutedEventArgs e)
        {
            toDownload = (bool)((CheckBox)sender).IsChecked;
        }
    }
}
