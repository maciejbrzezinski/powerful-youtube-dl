using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using powerful_youtube_dl.thinkingPart;
//using MahApps.Metro.Controls;

namespace powerful_youtube_dl
{
    public partial class MainWindow
    {
        /*     public static string ytDlPath = Properties.Settings.Default.ytdlexe;
             public static string downloadPath = Properties.Settings.Default.dlpath;
             public static int maxDownloads = Properties.Settings.Default.maxDownloading;*/
        System.Windows.Forms.NotifyIcon ni = null;

        public MainWindow()
        {
            InitializeComponent();
            startTray();

            if (Properties.Settings.Default.firstRun)
            {
                window.UserSettings ss = new window.UserSettings();
                ss.ShowDialog();
                Properties.Settings.Default.firstRun = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.startMinimalized && Properties.Settings.Default.startWithSystem)
                Hide();
        }

        private void closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.closeToTray)
            {
                Hide();
                e.Cancel = true;
            }
        }

        private void tryToTray(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && Properties.Settings.Default.toTray)
                this.Hide();
        }

        private void trayDoubleClick(object sender, EventArgs args)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void startTray()
        {
            if (ni == null)
            {
                ni = new System.Windows.Forms.NotifyIcon();
                ni.Icon = new System.Drawing.Icon(@"C:\Users\miejs\Documents\GitHub\powerful-youtube-dl\something\logo_ytdl_d4Q_icon.ico");
                ni.Visible = true;
                ni.ContextMenu = createMenu();
                ni.DoubleClick += trayDoubleClick;
            }
        }

        private ContextMenu createMenu()
        {
            ContextMenu menu = new System.Windows.Forms.ContextMenu();
            menu.MenuItems.Add("Wyjdź", (s, e) => System.Windows.Application.Current.Shutdown());
            return menu;
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

            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                int ind = kolejka.Items.IndexOf(pos);
                if (ind > -1)
                {
                    kolejka.Items.RemoveAt(ind);
                    kolejka.Items.Refresh();
                }
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() =>
                  {
                      int ind = this.kolejka.Items.IndexOf(pos);
                      if (ind > -1)
                      {
                          this.kolejka.Items.RemoveAt(ind);
                          this.kolejka.Items.Refresh();
                      }
                  }));
            }
        }

        public void setDownloadState(ListViewItemMy pos, String value)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() =>
                  {
                      pos.status = value;
                      this.kolejka.Items.Refresh();
                  }));
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
            if (!textChanged)
            {
                textChanged = true;
                linkHandler();
                textChanged = false;
            }
        }

        private void link_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!textChanged)
            {
                textChanged = true;
                linkHandler();
                textChanged = false;
            }
        }

        private void linkHandler()
        {
            if (PlayList._listOfPlayListsCheckBox != null && tmpURL != link.Text)
            {
                tmpURL = link.Text;
                if (checkIfYoutubeURL())
                {
                    link.Text = System.Windows.Clipboard.GetText();
                    if (Properties.Settings.Default.autoLoadLink)
                        loadURL();
                }
                else
                    link.Text = "";
            }
            link.SelectAll();
        }

        private void openSetting(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            tabs.SelectedIndex = index;
            window.UserSettings ss = new window.UserSettings();
            ss.ShowDialog();
        }
        private int index = 0;
        private void saveTab(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            index = tabs.SelectedIndex;
        }

        private string tmpURL = "";
        private static bool textChanged = false;
        private void link_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!textChanged)
            {
                textChanged = true;
                linkHandler();
                textChanged = false;
            }
        }
    }
}
