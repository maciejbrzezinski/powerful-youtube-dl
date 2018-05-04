using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using powerful_youtube_dl.Properties;
using powerful_youtube_dl.thinkingPart;
using Application = System.Windows.Application;
using CheckBox = System.Windows.Controls.CheckBox;
using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Forms.ContextMenu;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListView = System.Windows.Controls.ListView;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using Timer = System.Timers.Timer;

namespace powerful_youtube_dl.window {

    public partial class MainWindow {
        public static NotifyIcon ni;
        public static List<string> dontLoad = new List<string>();
        public static int selectedPlaylistIndex = -1;
        public ObservableCollection<ListViewItemMy> videosInActivePlayList { get; set; }
        public PlayList pl { get; set; }

        public MainWindow() {
            videosInActivePlayList = new ObservableCollection<ListViewItemMy>();

            InitializeComponent();
            DataContext = this;
            startTray();
            new Statistics();

            if (Settings.Default.firstRun) {
                UserSettings ss = new UserSettings();
                ss.ShowDialog();
                Settings.Default.firstRun = false;
                Settings.Default.Save();
            }

            timer.Elapsed += (sender1, e1) => {
                invokeShit(DispatcherPriority.Normal, async () => {
                    if (addVideos != null) {
                        CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(videosInActivePlayList);
                        if (view != null) {
                            view.Filter = UserFilter;
                            CollectionViewSource.GetDefaultView(videosInActivePlayList).Refresh();
                        }
                    }
                });
            };
            timer.AutoReset = false;

            if (Settings.Default.playlists != null && Settings.Default.autoObservePlaylists) {
                foreach (string o in Settings.Default.playlists) {
                    try {
                        Video.isManualDownload = false;
                        string plaURL = o;
                        pl = new PlayList(plaURL);
                        pl.position.Menu = createPlaylistMenu(pl);
                    } catch { }
                }
            } else
                Settings.Default.playlists = new StringCollection();

            if (Settings.Default.startMinimized && Settings.Default.startWithSystem)
                Hide();
        }

        private void closing(object sender, CancelEventArgs e) {
            if (Settings.Default.closeToTray) {
                Hide();
                e.Cancel = true;
            }
        }

        private void tryToTray(object sender, EventArgs e) {
            if (WindowState == WindowState.Minimized && Settings.Default.doTray)
                Hide();
        }

        private void trayDoubleClick(object sender, EventArgs args) {
            Show();
            WindowState = WindowState.Normal;
        }

        private void startTray() {
            if (ni == null) {
                ni = new NotifyIcon();
                Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/powerful-youtube-dl;component/something/logo.ico"))?.Stream;
                if (iconStream != null)
                    ni.Icon = new Icon(iconStream);
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
            menu.MenuItems.Add("Wyjdź", (s, e) => Application.Current.Shutdown());
            return menu;
        }

        public System.Windows.Controls.ContextMenu createPlaylistMenu(PlayList toDelete) {
            System.Windows.Controls.ContextMenu menu = new System.Windows.Controls.ContextMenu();
            MenuItem item = new MenuItem { Header = "Usuń" };
            item.Click += (s, e) => deletePlaylist(toDelete);
            menu.Items.Add(item);
            return menu;
        }

        private void deletePlaylist(PlayList toDelete) {
            int index = PlayList._listOfPlayListsView.IndexOf(toDelete.position);
            foreach (Video v in toDelete._listOfVideosInPlayList)
                Video._listOfVideos.Remove(v);
            PlayList._listOfPlayListsView.Remove(toDelete.position);
            PlayList.removePlaylistFromSettings(toDelete.position.Link);
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
                    MessageBox.Show("Wystąpił błąd!", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error);
            } else if (url.Contains("watch")) {
                if (url.Contains("list")) {
                    int wynik = Dialog.Prompt("Co dokładnie ma zostać pobrane:", "Powerful YouTube DL", "Tylko piosenka, bez playlisty", "Cała playlista na której umieszczona jest piosenka");
                    if (wynik == 0)
                        new PlayList(new Video(url, listID));
                    else if (wynik == 1)
                        new PlayList(url);
                    else if (wynik == 3)
                        MessageBox.Show("Wystąpił błąd!", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error);
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
            System.Windows.Forms.MessageBox.Show(err, @"Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool checkIfYoutubeURL(string url) {
            if (url.Contains("playlist") || url.Contains("list") || url.Contains("watch") || url.Contains("channel") || url.Contains("user")) {
                if (url.Contains("youtu")) {
                    string test;

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
                    else if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else
                        test = "";
                    if (test != "")
                        return true;

                    start = url.IndexOf("user/") + 5;
                    finish = url.Substring(start + 5).IndexOf("/");
                    if (finish > 0 && start > -1)
                        test = url.Substring(start + 5, finish);
                    else if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else
                        test = "";
                    if (test != "")
                        return true;
                }
            }
            return false;
        }

        private void playlist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            selectedPlaylistIndex = playlist.SelectedIndex;
            int index = playlist.SelectedIndex;
            if (index != -1) {
                deleteAllVideosFromList();

                Thread ths = new Thread(async () => {
                    int x = PlayList._listOfPlayLists[index]._listOfVideosInPlayList.Count;
                    for (int i = 0; i < x; i++) {
                        if (index == selectedPlaylistIndex) {
                            int j = i;
                            invokeShit(DispatcherPriority.Send, async () => {
                                if (j < PlayList._listOfPlayLists[index]._listOfVideosInPlayList.Count) {
                                    PlayList._listOfPlayLists[index]._listOfVideosInPlayList[j].isVideoLoadedInActivePlaylist = true;
                                    addVideoToList(PlayList._listOfPlayLists[index]._listOfVideosInPlayList[j].position, PlayList._listOfPlayLists[index].position.Id);
                                }
                            });
                            await Task.Delay(2);
                        } else
                            break;
                    }
                });
                ths.Start();
            }
        }

        public static void invokeShit(DispatcherPriority priority, Action action) {
            if (Application.Current != null) {
                Application.Current.Dispatcher.BeginInvoke(priority, action);
            } else
                Environment.Exit(0);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            Video.isManualDownload = true;
            DownloadHandler.Load();
            DownloadHandler.DownloadQueueAsync();
        }

        public void addVideoToList(ListViewItemMy videom, string playlistID) {
            int index = playlist.SelectedIndex;
            if (index != -1 && PlayList._listOfPlayLists[index].position.Id == playlistID) {
                videosInActivePlayList.Add(videom);
            }
        }

        public void deleteVideoFromAdd(ListViewItemMy pos, string playlistID) {
            int index = playlist.SelectedIndex;
            if (index != -1 && PlayList._listOfPlayLists[index].position.Id == playlistID) {
                videosInActivePlayList.Remove(pos);
            }
        }

        public void deleteAllVideosFromList() {
            videosInActivePlayList.Clear();
        }

        private static readonly Timer timer = new Timer();

        private void search_TextChanged(object sender, TextChangedEventArgs e) {
            if (videosInActivePlayList.Count > 0) {
                timer.Interval = 300;
                if (!timer.Enabled)
                    timer.Enabled = true;
            }
        }

        private bool UserFilter(object item) {
            if (String.IsNullOrEmpty(search.Text))
                return true;
            if (item != null)
                return ((item as ListViewItemMy)?.Title.IndexOf(search.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            return true;
        }

        private void searchAfterSubmit(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter)
                return;

            if (addVideos != null) {
                CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(videosInActivePlayList);
                if (view != null) {
                    view.Filter = UserFilter;
                    CollectionViewSource.GetDefaultView(videosInActivePlayList).Refresh();
                }
            }
        }

        private void search_GotMouseCapture(object sender, MouseEventArgs e) {
            TextBox box = (TextBox) sender;
            if (box.Text == "Przeszukaj listę") {
                box.SelectAll();
            }
        }

        private string tmpURL = "";

        private void link_LostFocus(object sender, RoutedEventArgs e) {
            TextBox box = (TextBox) sender;
            string url = box.Text;
            if (checkIfYoutubeURL(url)) {
                tmpURL = url;
                if (Settings.Default.autoLoadLink) {
                    if (!dontLoad.Contains(tmpURL)) {
                        dontLoad.Add(tmpURL);
                        loadURL();
                        if (Settings.Default.autoStartDownload) {
                            DownloadHandler.Load();
                            if (DownloadHandler.toDownload.Count > 0)
                                DownloadHandler.DownloadQueueAsync();
                        }
                    }
                }
            } else
                box.Text = tmpURL;
        }

        private void link_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox box = (TextBox) sender;
            string url = box.Text;
            if (checkIfYoutubeURL(url)) {
                tmpURL = url;
                if (Settings.Default.autoLoadLink) {
                    if (!dontLoad.Contains(tmpURL)) {
                        dontLoad.Add(tmpURL);
                        loadURL();
                        if (Settings.Default.autoStartDownload) {
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
            TextBox box = (TextBox) sender;
            tmpURL = box.Text;
            if (!checkIfYoutubeURL(tmpURL)) {
                if (checkIfYoutubeURL(Clipboard.GetText())) {
                    tmpURL = Clipboard.GetText();
                    box.Text = tmpURL;
                    if (Settings.Default.autoLoadLink) {
                        if (!dontLoad.Contains(tmpURL)) {
                            dontLoad.Add(tmpURL);
                            loadURL();
                            if (Settings.Default.autoStartDownload) {
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
            UserSettings ss = new UserSettings();
            ss.ShowDialog();
        }

        private void link_lSubmit(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter)
                return;
            TextBox box = (TextBox) sender;
            if (checkIfYoutubeURL(box.Text)) {
                tmpURL = box.Text;
                if (Settings.Default.autoLoadLink) {
                    if (!dontLoad.Contains(tmpURL)) {
                        dontLoad.Add(tmpURL);
                        loadURL();
                        if (Settings.Default.autoStartDownload) {
                            DownloadHandler.Load();
                            if (DownloadHandler.toDownload.Count > 0)
                                DownloadHandler.DownloadQueueAsync();
                        }
                    }
                }
            }
        }

        private void openFolderOrBrowser(object sender, MouseButtonEventArgs e) {
            ListView listView = (ListView) sender;
            ListViewItemMy itemMy = (ListViewItemMy) listView.SelectedItem;
            if (itemMy != null) {
                if (itemMy.Status == "Pobrano") {
                    string argument = "/select, \"" + itemMy.ParentV.downloadPath + "\"";
                    Process.Start("explorer.exe", argument);
                } else
                    Process.Start(itemMy.ParentV.position.Link);
            }
        }

        private void checkChanged(object sender, RoutedEventArgs e) {
            var isChecked = ((CheckBox) sender).IsChecked;
            bool toDownload = isChecked != null && (bool) isChecked;
            ((ListViewItemMy) ((CheckBox) sender).DataContext).ParentPL.checkChanged(toDownload);
        }
    }
}