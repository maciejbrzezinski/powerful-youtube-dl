using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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
        public static System.Windows.Forms.NotifyIcon ni = null;
        public static List<string> dontLoad = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            startTray();
            Statistics stats = new Statistics();

            if (Properties.Settings.Default.firstRun)
            {
                window.UserSettings ss = new window.UserSettings();
                ss.ShowDialog();
                Properties.Settings.Default.firstRun = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.playlists != null && Properties.Settings.Default.autoObservePlaylists)
            {
                foreach (Object o in Properties.Settings.Default.playlists)
                {
                    try
                    {
                        Video.isManualDownload = false;
                        string link = o.ToString();
                        new PlayList(link);
                    }
                    catch { }
                }
            }
            else
                Properties.Settings.Default.playlists = new System.Collections.Specialized.StringCollection();

            if (Properties.Settings.Default.startMinimized && Properties.Settings.Default.startWithSystem)
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
            if (WindowState == WindowState.Minimized && Properties.Settings.Default.doTray)
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

        public static void showNotifyIconMessage(string title, string message, ToolTipIcon icon, int miliseconds)
        {
            ni.BalloonTipText = message;
            ni.BalloonTipIcon = icon;
            ni.BalloonTipTitle = title;
            ni.ShowBalloonTip(miliseconds);
        }

        private ContextMenu createMenu()
        {
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Wyjdź", (s, e) => System.Windows.Application.Current.Shutdown());
            return menu;
        }

        public System.Windows.Controls.ContextMenu createPlaylistMenu(PlayList toDelete)
        {
            System.Windows.Controls.ContextMenu menu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem();
            item.Header = "Usuń";
            item.Click += (s, e) => deletePlaylist(toDelete);
            menu.Items.Add(item);
            return menu;
        }

        private void deletePlaylist(PlayList toDelete)
        {
            int index = PlayList._listOfPlayListsCheckBox.IndexOf(toDelete.check);
            foreach (Video v in toDelete._listOfVideosInPlayList)
                Video._listOfVideos.Remove(v);
            PlayList._listOfPlayListsCheckBox.Remove(toDelete.check);
            PlayList.removePlaylistFromSettings(toDelete.playListURL);
            PlayList._listOfPlayLists.Remove(toDelete);
            toDelete._listOfVideosInPlayList.Clear();

            if (PlayList._listOfPlayListsCheckBox.Count > 0)
            {
                if (PlayList._listOfPlayListsCheckBox.Count > index)
                    playlist.SelectedIndex = index;
                else
                {
                    while (true)
                    {
                        index--;
                        if (index == -1)
                        {
                            playlist.SelectedItem = null;
                            break;
                        }
                        if (PlayList._listOfPlayListsCheckBox.Count > index)
                        {
                            playlist.SelectedIndex = index;
                            break;
                        }
                    }
                }
            }
            else
            {
                deleteAllVideosFromList();
                addVideos.Items.Refresh();
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
                    new PlayList(new Video(url));
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

        private bool checkIfYoutubeURL(string url)
        {
            if (url.Contains("playlist") || url.Contains("list") || url.Contains("watch") || url.Contains("channel") || url.Contains("user"))
            {
                if (url.Contains("youtu"))
                {
                    string test = "";

                    int start = url.IndexOf("v=");
                    int finish = url.Substring(start + 2).IndexOf("&");
                    if (start > -1 && finish < 0)
                        test = url.Substring(start + 2);
                    else if (start > -1 && finish > 0)
                        test = url.Substring(start + 2, finish);
                    else
                        test = "";
                    if (test.Length == 11)
                        return true;



                    start = url.IndexOf("list=");
                    finish = url.Substring(start + 5).IndexOf("&");
                    if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else if (start > -1 && finish > 0)
                        test = url.Substring(start + 5, finish);
                    else
                        test = "";
                    if (test.Length == 34 || test.Length == 24)
                        return true;



                    start = url.IndexOf("nnel/") + 5;
                    finish = url.Substring(start + 5).IndexOf("?");
                    if (finish == -1)
                        finish = url.Substring(start + 5).IndexOf("/");
                    if (finish > 0 && start > -1)
                        test = url.Substring(start + 5, finish);
                    if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else
                        test = "";
                    if (test != "")
                        return true;



                    start = url.IndexOf("user/") + 5;
                    finish = url.Substring(start + 5).IndexOf("/");
                    if (finish > 0 && start > -1)
                        test = url.Substring(start + 5, finish);
                    if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else
                        test = "";
                    if (test != "")
                        return true;
                }
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
            Video.isManualDownload = true;
            DownloadHandler.Load();
            DownloadHandler.DownloadQueue();
        }

        public void addVideoToList(ListViewItemMy videom)
        {
            addVideos.Items.Add(videom);
            addVideos.Items.Refresh();
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

        private static bool isWriting = false;
        private void search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            isWriting = true;
            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(1000);
                isWriting = false;
                if (!isWriting)
                {
                    if (addVideos != null)
                    {
                        CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(addVideos.Items);
                        if (view != null)
                        {
                            view.Filter = UserFilter;
                            CollectionViewSource.GetDefaultView(addVideos.Items).Refresh();
                        }
                    }
                }
            });
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(search.Text))
                return true;
            else
                return ((item as ListViewItemMy).title.IndexOf(search.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void searchAfterSubmit(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            if (addVideos != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(addVideos.Items);
                if (view != null)
                {
                    view.Filter = UserFilter;
                    CollectionViewSource.GetDefaultView(addVideos.Items).Refresh();
                }
            }
        }

        private void search_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            if (box.Text == "Przeszukaj listę")
            {
                box.SelectAll();
            }
        }

        private string tmpURL = "";
        private void link_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            string url = box.Text;
            if (checkIfYoutubeURL(url))
            {
                tmpURL = url;
                if (Properties.Settings.Default.autoLoadLink)
                {
                    if (!dontLoad.Contains(tmpURL))
                    {
                        dontLoad.Add(tmpURL);
                        loadURL();
                        if (Properties.Settings.Default.autoStartDownload)
                        {
                            DownloadHandler.Load();
                            if (DownloadHandler.toDownload.Count > 0)
                                DownloadHandler.DownloadQueue();
                        }
                    }
                }
            }
            else
                box.Text = tmpURL;
        }

        private void link_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            string url = box.Text;
            if (checkIfYoutubeURL(url))
            {
                tmpURL = url;
                if (Properties.Settings.Default.autoLoadLink)
                {
                    if (!dontLoad.Contains(tmpURL))
                    {
                        dontLoad.Add(tmpURL);
                        loadURL();
                        if (Properties.Settings.Default.autoStartDownload)
                        {
                            DownloadHandler.Load();
                            if (DownloadHandler.toDownload.Count > 0)
                                DownloadHandler.DownloadQueue();
                        }
                    }
                }
            }
        }

        private void link_GotFocus(object sender, RoutedEventArgs e)
        {
            Video.isManualDownload = true;
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            tmpURL = box.Text;
            if (!checkIfYoutubeURL(tmpURL))
            {
                if (checkIfYoutubeURL(System.Windows.Clipboard.GetText()))
                {
                    tmpURL = System.Windows.Clipboard.GetText();
                    box.Text = tmpURL;
                    if (Properties.Settings.Default.autoLoadLink)
                    {
                        if (!dontLoad.Contains(tmpURL))
                        {
                            dontLoad.Add(tmpURL);
                            loadURL();
                            if (Properties.Settings.Default.autoStartDownload)
                            {
                                DownloadHandler.Load();
                                if (DownloadHandler.toDownload.Count > 0)
                                    DownloadHandler.DownloadQueue();
                            }
                        }
                    }
                    else
                        box.SelectAll();
                }
            }
        }

        private void openSetting(object sender, RoutedEventArgs e)
        {
            tabs.SelectedIndex = 0;
            window.UserSettings ss = new window.UserSettings();
            ss.ShowDialog();
        }

        private void link_lSubmit(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            if (checkIfYoutubeURL(box.Text))
            {
                tmpURL = box.Text;
                if (Properties.Settings.Default.autoLoadLink)
                {
                    if (!dontLoad.Contains(tmpURL))
                    {
                        dontLoad.Add(tmpURL);
                        loadURL();
                        if (Properties.Settings.Default.autoStartDownload)
                        {
                            DownloadHandler.Load();
                            if (DownloadHandler.toDownload.Count > 0)
                                DownloadHandler.DownloadQueue();
                        }
                    }
                }

            }
        }

        private void openFolderOrBrowser(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListView listView = (System.Windows.Controls.ListView)sender;
            ListViewItemMy itemMy = (ListViewItemMy)listView.SelectedItem;
            if (itemMy != null)
            {
                if (itemMy.status == "Pobrano")
                    Process.Start(itemMy.parent.downloadPath.Substring(0, itemMy.parent.downloadPath.IndexOf(itemMy.parent.ToString())));
                else
                    Process.Start(itemMy.parent.videoURL);
            }
        }
    }
}
