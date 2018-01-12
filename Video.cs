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
        public string playList = "Pojedyncze";
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
            videoTitle = getTitle();
            check = new CheckBox();
            check.Click += new RoutedEventHandler(checkChanged);
            check.Content = videoTitle;
            _listOfVideos.Add(this);
            _listOfVideosCheckBox.Add(check);
            PlayList play = new PlayList(this);
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

        private string getTitle()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c D:\\youtube-dl.exe --get-title \"https://www.youtube.com/watch?v=" + videoID + " /T";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            if (output.Contains("\n"))
                output = output.Substring(0, output.Length - 1);
            return output;
        }

        public void Download()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = MainWindow.ytDlPath;
            startInfo.Arguments = " -x -o \""+MainWindow.downloadPath+"\\"+playList+"\\"+this.ToString()+".mp4\" https://www.youtube.com/watch?v="+videoID;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
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
