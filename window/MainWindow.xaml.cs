using powerful_youtube_dl.thinkingPart;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;

namespace powerful_youtube_dl {

    public partial class MainWindow {
        public static NotifyIcon ni = null;
        public static List<string> dontLoad = new List<string>();
        public static int selectedPlaylistIndex = -1;
        public ObservableCollection<ListViewItemMy> videosInActivePlayList { get; set; }
        public PlayList pl { get; set; }

        public MainWindow() {
            videosInActivePlayList = new ObservableCollection<ListViewItemMy>();

            InitializeComponent();
            DataContext = this;
            startTray();
            Statistics statistics = new Statistics();

            if (Properties.Settings.Default.firstRun) {
                window.UserSettings ss = new window.UserSettings();
                ss.ShowDialog();
                Properties.Settings.Default.firstRun = false;
                Properties.Settings.Default.Save();
            }

            timer.Elapsed += new ElapsedEventHandler((object sender1, ElapsedEventArgs e1) => {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            DispatcherPriority.Send,
                            new Action(async () => {
                                if (addVideos != null) {
                                    CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(videosInActivePlayList);
                                    if (view != null) {
                                        view.Filter = UserFilter;
                                        CollectionViewSource.GetDefaultView(videosInActivePlayList).Refresh();
                                    }
                                }
                            }));
            });
            timer.AutoReset = false;

            if (Properties.Settings.Default.playlists != null && Properties.Settings.Default.autoObservePlaylists) {
                foreach (Object o in Properties.Settings.Default.playlists) {
                    try {
                        Video.isManualDownload = false;
                        string plaURL = o.ToString();
                        pl = new PlayList(plaURL);
                    } catch (Exception e) {
                        Console.WriteLine("TUTAJ");
                    }
                }
            } else
                Properties.Settings.Default.playlists = new System.Collections.Specialized.StringCollection();

            if (Properties.Settings.Default.startMinimized && Properties.Settings.Default.startWithSystem)
                Hide();
        }

        private void closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Properties.Settings.Default.closeToTray) {
                Hide();
                e.Cancel = true;
            }
        }

        private void tryToTray(object sender, EventArgs e) {
            if (WindowState == WindowState.Minimized && Properties.Settings.Default.doTray)
                this.Hide();
        }

        private void trayDoubleClick(object sender, EventArgs args) {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void startTray() {
            if (ni == null) {
                ni = new NotifyIcon();
                Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/powerful-youtube-dl;component/something/logo.ico")).Stream;
                ni.Icon = new System.Drawing.Icon(iconStream);
                ni.Visible = true;
                ni.ContextMenu = createMenu();
                ni.DoubleClick += trayDoubleClick;
            }
        }

        public static void showNotifyIconMessage(string title, string message, ToolTipIcon icon, int miliseconds) {
            ni.BalloonTipText = message;
            ni.BalloonTipIcon = icon;
            ni.BalloonTipTitle = title;
            ni.ShowBalloonTip(miliseconds);
        }

        private ContextMenu createMenu() {
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Wyjdź", (s, e) => System.Windows.Application.Current.Shutdown());
            return menu;
        }

        public System.Windows.Controls.ContextMenu createPlaylistMenu(PlayList toDelete) {
            System.Windows.Controls.ContextMenu menu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem();
            item.Header = "Usuń";
            item.Click += (s, e) => deletePlaylist(toDelete);
            menu.Items.Add(item);
            return menu;
        }

        private void deletePlaylist(PlayList toDelete) {
            int index = PlayList._listOfPlayListsView.IndexOf(toDelete.position);
            foreach (Video v in toDelete._listOfVideosInPlayList)
                Video._listOfVideos.Remove(v);
            PlayList._listOfPlayListsView.Remove(toDelete.position);
            PlayList.removePlaylistFromSettings(toDelete.playListURL);
            PlayList._listOfPlayLists.Remove(toDelete);
            toDelete._listOfVideosInPlayList.Clear();

            if (PlayList._listOfPlayListsView.Count > 0) {
                if (PlayList._listOfPlayListsView.Count > index)
                    playlist.SelectedIndex = index;
                else {
                    while (true) {
                        index--;
                        if (index == -1) {
                            playlist.SelectedItem = null;
                            break;
                        }
                        if (PlayList._listOfPlayListsView.Count > index) {
                            playlist.SelectedIndex = index;
                            break;
                        }
                    }
                }
            } else {
                deleteAllVideosFromList();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            loadURL();
        }

        private void loadURL() {
            PlayList listID = new PlayList();
            string url = link.Text;
            if (url.Contains(" "))
                Error("Podany link jest nieprawidłowy!");
            else if (url.Contains("channel") || url.Contains("user")) {
                int wynik = Dialog.Prompt("Co dokładnie ma zostać pobrane:", "Powerful YouTube DL", "Wszystkie playlisty użytkownika", "Wszystkie materiały dodane przez użytkownika");
                if (wynik == 0 || wynik == 1)
                    new User(url, wynik);    ///////////////////////// ZROBIĆ POBIERANIE WSZYSTKICH DODANYCH PLIKÓW
                else if (wynik == 3)
                    System.Windows.MessageBox.Show("Wystąpił błąd!", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error);
            } else if (url.Contains("watch")) {
                int wynik = -1;
                if (url.Contains("list")) {
                    wynik = Dialog.Prompt("Co dokładnie ma zostać pobrane:", "Powerful YouTube DL", "Tylko piosenka, bez playlisty", "Cała playlista na której umieszczona jest piosenka");
                    if (wynik == 0)
                        new PlayList(new Video(url, listID));
                    else if (wynik == 1)
                        new PlayList(url);
                    else if (wynik == 3)
                        System.Windows.MessageBox.Show("Wystąpił błąd!", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error);
                } else
                    new PlayList(new Video(url, listID));
            } else if (url.Contains("playlist") || url.Contains("list")) {
                new PlayList(url);
            } else
                Error("Podany link jest nieprawidłowy!");

            if (listID.videoIDsToGetParams.Count > 0)
                listID.getParamsOfVideos();
        }

        public static void Error(string err) {
            System.Windows.Forms.MessageBox.Show(err, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool checkIfYoutubeURL(string url) {
            if (url.Contains("playlist") || url.Contains("list") || url.Contains("watch") || url.Contains("channel") || url.Contains("user")) {
                if (url.Contains("youtu")) {
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

        private void playlist_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            selectedPlaylistIndex = playlist.SelectedIndex;
            int index = playlist.SelectedIndex;
            if (index != -1) {
                deleteAllVideosFromList();

                Thread ths = new Thread(async () => {
                    int x = PlayList._listOfPlayLists[index]._listOfVideosInPlayList.Count;
                    for (int i = 0; i < x; i++) {
                        if (index == selectedPlaylistIndex) {
                            int j = i;
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Send,
                             new Action(async () => {
                                 PlayList._listOfPlayLists[index]._listOfVideosInPlayList[j].isVideoLoadedInActivePlaylist = true;
                                 addVideoToList(PlayList._listOfPlayLists[index]._listOfVideosInPlayList[j].position, PlayList._listOfPlayLists[index].playListID);
                             }));
                            await Task.Delay(2);
                        } else
                            break;
                    }
                });
                ths.Start();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            Video.isManualDownload = true;
            DownloadHandler.Load();
            DownloadHandler.DownloadQueueAsync();
        }

        public void addVideoToList(ListViewItemMy videom, string playlistID) {
            int index = playlist.SelectedIndex;
            if (index != -1 && PlayList._listOfPlayLists[index].playListID == playlistID) {
                videosInActivePlayList.Add(videom);
            }
        }

        public void deleteVideoFromAdd(ListViewItemMy pos, string playlistID) {
            int index = playlist.SelectedIndex;
            if (index != -1 && PlayList._listOfPlayLists[index].playListID == playlistID) {
                videosInActivePlayList.Remove(pos);
            }
        }

        public void deleteAllVideosFromList() {
            videosInActivePlayList.Clear();
        }

        private static bool isWriting = false;
        private static System.Timers.Timer timer = new System.Timers.Timer();

        private void search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            if (videosInActivePlayList.Count > 0) {
                timer.Interval = 300;
                if (!timer.Enabled)
                    timer.Enabled = true;
            }
        }

        private bool UserFilter(object item) {
            if (String.IsNullOrEmpty(search.Text))
                return true;
            else {
                if (item != null)
                    return ((item as ListViewItemMy).Title.IndexOf(search.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                else
                    return true;
            }
        }

        private void searchAfterSubmit(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;

            if (addVideos != null) {
                CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(videosInActivePlayList);
                if (view != null) {
                    view.Filter = UserFilter;
                    CollectionViewSource.GetDefaultView(videosInActivePlayList).Refresh();
                }
            }
        }

        private void search_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e) {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox) sender;
            if (box.Text == "Przeszukaj listę") {
                box.SelectAll();
            }
        }

        private string tmpURL = "";

        private void link_LostFocus(object sender, RoutedEventArgs e) {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox) sender;
            string url = box.Text;
            if (checkIfYoutubeURL(url)) {
                tmpURL = url;
                if (Properties.Settings.Default.autoLoadLink) {
                    if (!dontLoad.Contains(tmpURL)) {
                        dontLoad.Add(tmpURL);
                        loadURL();
                        if (Properties.Settings.Default.autoStartDownload) {
                            DownloadHandler.Load();
                            if (DownloadHandler.toDownload.Count > 0)
                                DownloadHandler.DownloadQueueAsync();
                        }
                    }
                }
            } else
                box.Text = tmpURL;
        }

        private void link_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox) sender;
            string url = box.Text;
            if (checkIfYoutubeURL(url)) {
                tmpURL = url;
                if (Properties.Settings.Default.autoLoadLink) {
                    if (!dontLoad.Contains(tmpURL)) {
                        dontLoad.Add(tmpURL);
                        loadURL();
                        if (Properties.Settings.Default.autoStartDownload) {
                            DownloadHandler.Load();
                            if (DownloadHandler.toDownload.Count > 0)
                                DownloadHandler.DownloadQueueAsync();
                        }
                    }
                }
            }
        }

        private void link_GotFocus(object sender, RoutedEventArgs e) {
            Video.isManualDownload = true;
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox) sender;
            tmpURL = box.Text;
            if (!checkIfYoutubeURL(tmpURL)) {
                if (checkIfYoutubeURL(System.Windows.Clipboard.GetText())) {
                    tmpURL = System.Windows.Clipboard.GetText();
                    box.Text = tmpURL;
                    if (Properties.Settings.Default.autoLoadLink) {
                        if (!dontLoad.Contains(tmpURL)) {
                            dontLoad.Add(tmpURL);
                            loadURL();
                            if (Properties.Settings.Default.autoStartDownload) {
                                DownloadHandler.Load();
                                if (DownloadHandler.toDownload.Count > 0)
                                    DownloadHandler.DownloadQueueAsync();
                            }
                        }
                    } else
                        box.SelectAll();
                }
            }
        }

        private void openSetting(object sender, RoutedEventArgs e) {
            tabs.SelectedIndex = 0;
            window.UserSettings ss = new window.UserSettings();
            ss.ShowDialog();
        }

        private void link_lSubmit(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox) sender;
            if (checkIfYoutubeURL(box.Text)) {
                tmpURL = box.Text;
                if (Properties.Settings.Default.autoLoadLink) {
                    if (!dontLoad.Contains(tmpURL)) {
                        dontLoad.Add(tmpURL);
                        loadURL();
                        if (Properties.Settings.Default.autoStartDownload) {
                            DownloadHandler.Load();
                            if (DownloadHandler.toDownload.Count > 0)
                                DownloadHandler.DownloadQueueAsync();
                        }
                    }
                }
            }
        }

        private void openFolderOrBrowser(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            System.Windows.Controls.ListView listView = (System.Windows.Controls.ListView) sender;
            ListViewItemMy itemMy = (ListViewItemMy) listView.SelectedItem;
            if (itemMy != null) {
                if (itemMy.Status == "Pobrano") {
                    string argument = "/select, \"" + itemMy.Parent.downloadPath + "\"";
                    Process.Start("explorer.exe", argument);
                } else
                    Process.Start(itemMy.Parent.position.Link);
            }
        }
    }
}