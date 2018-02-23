using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using MahApps.Metro.Controls;
using Microsoft.Win32;
//using MahApps.Metro.Controls;

namespace powerful_youtube_dl
{
    public partial class MainWindow
    {
        public static string ytDlPath = "";
        public static string downloadPath = "";

        public MainWindow()
        {
            InitializeComponent();

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Powerful YouTube Dl", true);
            ytDlPath = key.GetValue("ytdlexe", "").ToString();
            if (ytDlPath == "")
                ytDLabel.Content = "Wybierz plik youtube-dl.exe";
            else
                ytDLabel.Content = Path.GetFileName(ytDlPath);

            downloadPath = key.GetValue("dlpath", "").ToString();
            if (downloadPath == "")
                localization.Content = "Wybierz lokalizację pobierania";
            else
                localization.Content = downloadPath;
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
                ytDLabel.Content = dialog.SafeFileName;
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Powerful YouTube Dl", true);
                key.SetValue("ytdlexe", ytDlPath);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                localization.Content = dialog.SelectedPath;
                downloadPath = dialog.SelectedPath;
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Powerful YouTube Dl", true);
                key.SetValue("dlpath", downloadPath);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            loadURL();
        }

        private void loadURL()
        {
            string url = link.Text;
            if (url.Contains(" "))
                Error("Podany link jest nieprawidłowy!");
            else if (url.Contains("channel") || url.Contains("user"))
            {
                int wynik = Dialog.Prompt("Co dokładnie ma zostać pobrane:", "Powerful YouTube DL", "Wszystkie playlisty użytkownika", "Wszystkie materiały dodane przez użytkownika");
                if (wynik == 0 || wynik == 1)
                    new User(url, wynik);    ///////////////////////// ZROBIĆ POBIERANIE WSZYSTKICH DODANYCH PLIKÓW 
                else if (wynik == 3)
                    System.Windows.MessageBox.Show("Wystąpił błąd!", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (url.Contains("watch"))
            {
                int wynik = -1;
                if (url.Contains("list"))
                {
                    wynik = Dialog.Prompt("Co dokładnie ma zostać pobrane:", "Powerful YouTube DL", "Tylko piosenka, bez playlisty", "Cała playlista na której umieszczona jest piosenka");
                    if (wynik == 0)
                        new PlayList(new Video(url));
                    else if (wynik == 1)
                        new PlayList(url);
                    else if (wynik == 3)
                        System.Windows.MessageBox.Show("Wystąpił błąd!", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                    new Video(url);
            }
            else if (url.Contains("playlist") || url.Contains("list"))
            {
                new PlayList(url);
            }
            else
                Error("Podany link jest nieprawidłowy!");
            Video.getParamsOfVideos();
            Video.videoIDsToGetParams = new List<Video>();
        }

        public static void Error(string err)
        {
            System.Windows.Forms.MessageBox.Show(err, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool checkIfYoutubeURL()
        {
            string url = System.Windows.Clipboard.GetText();
            if (url.Contains("playlist") || url.Contains("list") || url.Contains("watch") || url.Contains("channel") || url.Contains("user"))
            {
                if (url.Contains("youtu"))
                    return true;
            }
            return false;
        }

        private void playlist_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int index = playlist.SelectedIndex;
            if (index != -1)
            {
                deleteAllVideosFromList();
                foreach (Video vid in PlayList._listOfPlayLists[index]._listOfVideosInPlayList)
                    addVideoToList(vid.position);
                addVideos.Items.Refresh();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Download.Load();
            tabs.SelectedIndex = 1;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Download.Delete(kolejka.SelectedIndex);
            kolejka.Items.Refresh();

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (buttonDownload.Content.ToString() != "Stop")
            {
                buttonDownload.Content = "Stop";
                Download.DownloadQueue();
            }
            else
            {
                buttonDownload.Content = "Pobierz";
            }
        }

        public void addVideoToQueue(ListViewItemMy video)
        {
            kolejka.Items.Add(video);
            kolejka.Items.Refresh();

        }
        public void addVideoToList(ListViewItemMy videom)
        {
            addVideos.Items.Add(videom);
            addVideos.Items.Refresh();

        }

        public void deleteVideoFromQueue(int index)
        {
            kolejka.Items.RemoveAt(index);
            kolejka.Items.Refresh();
        }

        public void deleteVideoFromQueue(ListViewItemMy pos)
        {
            int ind = kolejka.Items.IndexOf(pos);
            if (ind > -1)
            {
                kolejka.Items.RemoveAt(ind);
                kolejka.Items.Refresh();
            }
        }

        public void deleteVideoFromAdd(ListViewItemMy pos)
        {
            int ind = addVideos.Items.IndexOf(pos);
            if (ind > -1)
            {
                addVideos.Items.RemoveAt(ind);
                addVideos.Items.Refresh();
            }
        }

        public void deleteAllVideosFromList()
        {
            addVideos.Items.Clear();
            addVideos.Items.Refresh();
        }

        public void changeCheckVideos(bool isTrue)
        {
            System.Windows.Controls.ItemCollection a = addVideos.Items;
            foreach (Video vid in addVideos.Items)
                vid.position.check = isTrue;
            kolejka.Items.Refresh();
        }

        private void link_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            linkHandler();
        }
        private void link_GotFocus(object sender, RoutedEventArgs e)
        {
            linkHandler();
        }
        private void linkHandler()
        {
            if (link.Text == "Link do kanału, playlisty lub video" || link.Text == "")
            {
                if (checkIfYoutubeURL())
                    link.Text = System.Windows.Clipboard.GetText();
                else
                    link.Text = "";
            }
            link.SelectAll();
        }
    }
}
