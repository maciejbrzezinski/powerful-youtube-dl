using powerful_youtube_dl.Properties;
using powerful_youtube_dl.thinkingPart;
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
        public static NotifyIcon Notify;
        public static List<string> DontLoad = new List<string>();
        public ObservableCollection<ListViewItemMy> VideosInActivePlayList { get; set; }
        public static ObservableCollection<ListViewItemMy> ListOfPlayListsView { get; set; }

        public MainWindow() {
            VideosInActivePlayList = new ObservableCollection<ListViewItemMy>();

            InitializeComponent();
            DataContext = this;
            StartTray();
            new Statistics();

            if (Settings.Default.firstRun) {
                UserSettings ss = new UserSettings();
                ss.ShowDialog();
                UserSettings.SaveSetting("firstRun", false);
            }

            Timer.Elapsed += (sender1, e1) => {
                InvokeShit(DispatcherPriority.Normal, async () => {
                    if (AddVideos != null) {
                        CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(VideosInActivePlayList);
                        if (view != null) {
                            view.Filter = UserFilter;
                            CollectionViewSource.GetDefaultView(VideosInActivePlayList).Refresh();
                        }
                    }
                });
            };
            Timer.AutoReset = false;

            if (Settings.Default.playlists != null && Settings.Default.autoObservePlaylists) {
                foreach (string o in Settings.Default.playlists) {
                    try {
                        Video.IsManualDownload = false;
                        new PlayList(o);
                    } catch { }
                }
            } else
                Settings.Default.playlists = new StringCollection();

            if (Settings.Default.startMinimized && Settings.Default.startWithSystem)
                Hide();
        }

        private void ClosingWindow(object sender, CancelEventArgs e) {
            if (Settings.Default.closeToTray) {
                Hide();
                e.Cancel = true;
            }
        }

        private void TryToTray(object sender, EventArgs e) {
            if (WindowState == WindowState.Minimized && Settings.Default.doTray)
                Hide();
        }

        private void TrayDoubleClick(object sender, EventArgs args) {
            Show();
            WindowState = WindowState.Normal;
        }

        private void StartTray() {
            if (Notify == null) {
                Notify = new NotifyIcon();
                Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/powerful-youtube-dl;component/something/logo.ico"))?.Stream;
                if (iconStream != null)
                    Notify.Icon = new Icon(iconStream);
                Notify.Visible = true;
                Notify.ContextMenu = CreateMenu();
                Notify.DoubleClick += TrayDoubleClick;
            }
        }

        public static void ShowNotifyIconMessage(string title, string message, ToolTipIcon icon, int miliseconds, string path) {
            NotifyIcon popupIcon = new NotifyIcon();
            popupIcon.BalloonTipText = message;
            popupIcon.BalloonTipIcon = icon;
            popupIcon.BalloonTipTitle = title;
            popupIcon.Visible = true;
            popupIcon.Icon = Notify.Icon;
            popupIcon.BalloonTipClicked += (sender, args) => {
                if (!File.Exists(path))
                    path += ".part";
                string argument = "/select, \"" + path + "\"";
                Process.Start("explorer.exe", argument);
            };
            popupIcon.BalloonTipClosed += (sender, args) => {
                popupIcon.Icon = null;
                popupIcon.Visible = false;
                popupIcon.Dispose();
            };
            popupIcon.ShowBalloonTip(miliseconds);
        }

        private static ContextMenu CreateMenu() {
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Wyjdź", (s, e) => Application.Current.Shutdown());
            return menu;
        }

        private void ContextDeletePlaylist(object sender, RoutedEventArgs e) {
            int oldIndex = Playlist.SelectedIndex;
            ((ListViewItemMy) ((MenuItem) sender).DataContext).ParentPl.ContextDeletePlaylist();
            if (Playlist.Items.Count == 0)
                DeleteAllVideosFromList();
            else if (Playlist.Items.Count > 0 && oldIndex != 0)
                Playlist.SelectedIndex = oldIndex - 1;
            else
                Playlist.SelectedIndex = 0;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            LoadUrl();
        }

        private void LoadUrl() {
            PlayList listId = new PlayList();
            string url = Link.Text;
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
                        new PlayList(new Video(url, listId));
                    else if (wynik == 1)
                        new PlayList(url);
                    else if (wynik == 3)
                        MessageBox.Show("Wystąpił błąd!", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error);
                } else
                    new PlayList(new Video(url, listId));
            } else if (url.Contains("playlist") || url.Contains("list")) {
                new PlayList(url);
            } else
                Error("Podany link jest nieprawidłowy!");

            if (listId.VideoIDsToGetParams.Count > 0)
                listId.GetParamsOfVideos();
        }

        public static void Error(string err) {
            System.Windows.Forms.MessageBox.Show(err, @"Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool CheckIfYoutubeUrl(string url) {
            if (url.Contains("playlist") || url.Contains("list") || url.Contains("watch") || url.Contains("channel") || url.Contains("user")) {
                if (url.Contains("youtu")) {
                    string test;

                    int start = url.IndexOf("v=", StringComparison.Ordinal);
                    int finish = url.Substring(start + 2).IndexOf("&", StringComparison.Ordinal);
                    if (start > -1 && finish < 0)
                        test = url.Substring(start + 2);
                    else if (start > -1 && finish > 0)
                        test = url.Substring(start + 2, finish);
                    else
                        test = "";
                    if (test.Length == 11)
                        return true;

                    start = url.IndexOf("list=", StringComparison.Ordinal);
                    finish = url.Substring(start + 5).IndexOf("&", StringComparison.Ordinal);
                    if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else if (start > -1 && finish > 0)
                        test = url.Substring(start + 5, finish);
                    else
                        test = "";
                    if (test.Length == 34 || test.Length == 24)
                        return true;

                    start = url.IndexOf("nnel/", StringComparison.Ordinal) + 5;
                    finish = url.Substring(start + 5).IndexOf("?", StringComparison.Ordinal);
                    if (finish == -1)
                        finish = url.Substring(start + 5).IndexOf("/", StringComparison.Ordinal);
                    if (finish > 0 && start > -1)
                        test = url.Substring(start + 5, finish);
                    else if (start > -1 && finish < 0)
                        test = url.Substring(start + 5);
                    else
                        test = "";
                    if (test != "")
                        return true;

                    start = url.IndexOf("user/", StringComparison.Ordinal) + 5;
                    finish = url.Substring(start + 5).IndexOf("/", StringComparison.Ordinal);
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
            ListViewItemMy item = (ListViewItemMy) Playlist.SelectedItem;
            int index = Playlist.SelectedIndex;
            if (index != -1 && index < PlayList.ListOfPlayLists.Count) {
                DeleteAllVideosFromList();
                Thread ths = new Thread(async () => {
                    int x = item.ParentPl.ListOfVideosInPlayList.Count;
                    for (int i = 0; i < x; i++) {
                        int j = i;
                        if (index != -1) {
                            InvokeShit(DispatcherPriority.Send, async () => {
                                if (index < PlayList.ListOfPlayLists.Count &&
                                    j < item.ParentPl.ListOfVideosInPlayList.Count) {
                                    AddVideoToList(item.ParentPl.ListOfVideosInPlayList[j].Position,
                                        item.ParentPl.Position.Id);
                                } else
                                    index = -1;
                            });
                            await Task.Delay(1);
                        }
                    }
                });
                ths.Start();
            }
        }

        public static void InvokeShit(DispatcherPriority priority, Action action) {
            if (Application.Current != null) {
                Application.Current.Dispatcher.BeginInvoke(priority, action);
            } else
                Environment.Exit(0);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            Video.IsManualDownload = true;
            DownloadHandler.Load();
            DownloadHandler.DownloadQueueAsync();
        }

        public void AddVideoToList(ListViewItemMy videom, string playlistId) {
            int index = Playlist.SelectedIndex;
            if (index != -1 && PlayList.ListOfPlayLists[index].Position.Id == playlistId) {
                VideosInActivePlayList.Add(videom);
            }
        }

        public void DeleteVideoFromAdd(ListViewItemMy pos, string playlistId) {
            int index = Playlist.SelectedIndex;
            if (index != -1 && PlayList.ListOfPlayLists[index].Position.Id == playlistId) {
                VideosInActivePlayList.Remove(pos);
            }
        }

        public void DeleteAllVideosFromList() {
            VideosInActivePlayList.Clear();
        }

        private static readonly Timer Timer = new Timer();

        private void search_TextChanged(object sender, TextChangedEventArgs e) {
            if (VideosInActivePlayList.Count > 0) {
                Timer.Interval = 300;
                if (!Timer.Enabled)
                    Timer.Enabled = true;
            }
        }

        private bool UserFilter(object item) {
            if (String.IsNullOrEmpty(Search.Text))
                return true;
            if (item != null)
                return ((item as ListViewItemMy)?.Title.IndexOf(Search.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            return true;
        }

        private void SearchAfterSubmit(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter)
                return;

            if (AddVideos != null) {
                CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(VideosInActivePlayList);
                if (view != null) {
                    view.Filter = UserFilter;
                    CollectionViewSource.GetDefaultView(VideosInActivePlayList).Refresh();
                }
            }
        }

        private void search_GotMouseCapture(object sender, MouseEventArgs e) {
            TextBox box = (TextBox) sender;
            if (box.Text == "Przeszukaj listę") {
                box.SelectAll();
            }
        }

        private string _tmpUrl = "";

        private void link_LostFocus(object sender, RoutedEventArgs e) {
            TextBox box = (TextBox) sender;
            string url = box.Text;
            if (CheckIfYoutubeUrl(url)) {
                _tmpUrl = url;
                if (Settings.Default.autoLoadLink) {
                    if (!DontLoad.Contains(_tmpUrl)) {
                        DontLoad.Add(_tmpUrl);
                        LoadUrl();
                        if (Settings.Default.autoStartDownload) {
                            DownloadHandler.Load();
                            if (DownloadHandler.ToDownload.Count > 0)
                                DownloadHandler.DownloadQueueAsync();
                        }
                    }
                }
            } else
                box.Text = _tmpUrl;
        }

        private void link_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox box = (TextBox) sender;
            string url = box.Text;
            if (CheckIfYoutubeUrl(url)) {
                _tmpUrl = url;
                if (Settings.Default.autoLoadLink) {
                    if (!DontLoad.Contains(_tmpUrl)) {
                        DontLoad.Add(_tmpUrl);
                        LoadUrl();
                        if (Settings.Default.autoStartDownload) {
                            DownloadHandler.Load();
                            if (DownloadHandler.ToDownload.Count > 0)
                                DownloadHandler.DownloadQueueAsync();
                        }
                    }
                }
            }
        }

        private void link_GotFocus(object sender, RoutedEventArgs e) {
            Video.IsManualDownload = true;
            TextBox box = (TextBox) sender;
            _tmpUrl = box.Text;
            if (!CheckIfYoutubeUrl(_tmpUrl)) {
                if (CheckIfYoutubeUrl(Clipboard.GetText())) {
                    _tmpUrl = Clipboard.GetText();
                    box.Text = _tmpUrl;
                    if (Settings.Default.autoLoadLink) {
                        if (!DontLoad.Contains(_tmpUrl)) {
                            DontLoad.Add(_tmpUrl);
                            LoadUrl();
                            if (Settings.Default.autoStartDownload) {
                                DownloadHandler.Load();
                                if (DownloadHandler.ToDownload.Count > 0)
                                    DownloadHandler.DownloadQueueAsync();
                            }
                        }
                    } else
                        box.SelectAll();
                }
            }
        }

        private void OpenSetting(object sender, RoutedEventArgs e) {
            Tabs.SelectedIndex = 0;
            UserSettings ss = new UserSettings();
            ss.ShowDialog();
        }

        private void link_lSubmit(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter)
                return;
            TextBox box = (TextBox) sender;
            if (CheckIfYoutubeUrl(box.Text)) {
                _tmpUrl = box.Text;
                if (Settings.Default.autoLoadLink) {
                    if (!DontLoad.Contains(_tmpUrl)) {
                        DontLoad.Add(_tmpUrl);
                        LoadUrl();
                        if (Settings.Default.autoStartDownload) {
                            DownloadHandler.Load();
                            if (DownloadHandler.ToDownload.Count > 0)
                                DownloadHandler.DownloadQueueAsync();
                        }
                    }
                }
            }
        }

        private void OpenFolderOrBrowser(object sender, MouseButtonEventArgs e) {
            ListView listView = (ListView) sender;
            ListViewItemMy itemMy = (ListViewItemMy) listView.SelectedItem;
            if (itemMy != null) {
                if (itemMy.ParentV.IsDownloaded) {
                    string path = itemMy.Path;
                    if (!File.Exists(path))
                        path += ".part";
                    string argument = "/select, \"" + path + "\"";
                    Process.Start("explorer.exe", argument);
                } else
                    Process.Start(itemMy.ParentV.Position.Link);
            }
        }

        private void CheckChanged(object sender, RoutedEventArgs e) {
            ((ListViewItemMy) ((CheckBox) sender).DataContext).ParentPl.CheckChanged(((CheckBox) sender).IsChecked);
        }
    }
}