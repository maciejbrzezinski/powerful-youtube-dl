using System;
using System.Windows;
using System.Windows.Forms;

namespace powerful_youtube_dl
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string ytDlPath;
        public static string downloadPath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.DefaultExt = ".exe";
            dialog.Filter = "Exe Files (*.exe)|*.exe";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                ytDlPath = dialog.FileName;
                ytDLabel.Content = dialog.SafeFileName; ;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                localization.Content = dialog.SelectedPath;
                downloadPath = dialog.SelectedPath;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string url = link.Text;
            if(url.Contains(" "))
                Error("Podany link jest nieprawidłowy!");
            else if (url.Contains("channel") || url.Contains("user"))
                new User(url);
            else if (url.Contains("watch"))
                new Video(url);
            else if (url.Contains("playlist") || url.Contains("list"))
                new PlayList(url);
            else
                Error("Podany link jest nieprawidłowy!");
        }

        public static void Error(string err)
        {
            System.Windows.Forms.MessageBox.Show(err, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        

        private void link_GotFocus(object sender, RoutedEventArgs e)
        {
            if (link.Text == "Link do kanału, playlisty lub video")
                link.Text = "";
            link.SelectAll();
        }

        private void playlist_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int index = playlist.SelectedIndex;
            Video._listOfVideosCheckBox.Clear();
            foreach (System.Windows.Controls.CheckBox chec in PlayList._listOfPlayLists[index]._listOfVideosInPlayListCheckBox)
                Video._listOfVideosCheckBox.Add(chec);
            videos.ScrollIntoView(videos.Items[0]);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Download.Load();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Download.Delete(queue.SelectedIndex);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Download.DownloadQueue();
        }
    }
}
