using powerful_youtube_dl.Properties;
using powerful_youtube_dl.thinkingPart;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using Timer = System.Timers.Timer;

namespace powerful_youtube_dl.window {

    public partial class MainWindow {
        public static NotifyIcon Notify;
        public static List<string> DontLoad = new List<string>();
        public ObservableCollection<VideoView> VideosInActivePlayList { get; set; }
        public static ObservableCollection<PlaylistView> ListOfPlayListsView { get; set; }

        public MainWindow() {
            VideosInActivePlayList = new ObservableCollection<VideoView>();

            SetLanguageDictionary();

            InitializeComponent();
            DataContext = this;
            StartTray();
            new Statistics();

            if (Settings.Default.firstRun) {
                UserSettings ss = new UserSettings();
                ss.ShowDialog();
                UserSettings.SaveSetting("firstRun", false);
            }

            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;

            Timer.Elapsed += (sender1, e1) => {
                BasicFunctionality.InvokeShit(DispatcherPriority.Normal, async () => {
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
                        new PlayList(o, null);
                    }
                    catch { }
                }
            } else
                Settings.Default.playlists = new StringCollection();

            if (Settings.Default.startMinimized && Settings.Default.startWithSystem)
                Hide();
        }

        private void SetLanguageDictionary() {
            Resources.MergedDictionaries.Clear();
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString()) {
                case "en-US":
                    dict.Source = new Uri("pack://application:,,,/powerful-youtube-dl;component/Languages/EN.xaml", UriKind.Absolute);
                    break;
                default:
                    dict.Source = new Uri("pack://application:,,,/powerful-youtube-dl;component/Languages/PL.xaml", UriKind.Absolute);
                    break;
            }
            Resources.MergedDictionaries.Add(dict);
        }

        private void StartTray() {
            if (Notify == null) {
                Notify = new NotifyIcon();
                Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/powerful-youtube-dl;component/something/logo.ico"))?.Stream;
                if (iconStream != null) {
                    Notify.Icon = new Icon(iconStream);
                    BasicFunctionality.notifyIcon = Notify.Icon;
                }
                Notify.Visible = true;
                Notify.ContextMenu = CreateMenu();
                Notify.DoubleClick += TrayDoubleClick;
            }
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

        private static ContextMenu CreateMenu() {
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Wyjdź", (s, e) => Application.Current.Shutdown());
            return menu;
        }

        private void ContextDeletePlaylist(object sender, RoutedEventArgs e) {
            int oldIndex = Playlist.SelectedIndex;
            ((PlaylistView) ((MenuItem) sender).DataContext).ParentPlaylist.ContextDeletePlaylist();
            if (Playlist.Items.Count == 0)
                DeleteAllVideosFromList();
            else if (Playlist.Items.Count > 0 && oldIndex != 0)
                Playlist.SelectedIndex = oldIndex - 1;
            else
                Playlist.SelectedIndex = 0;
        }

        private void ContextOpenYT(object sender, RoutedEventArgs e) {
            string link = ((CommonThingsView) ((MenuItem) sender).DataContext).Link;
            BasicFunctionality.ContextOpenYT(link);
        }

        private void ContextOpenPath(object sender, RoutedEventArgs e) {
            string path = ((CommonThingsView) ((MenuItem) sender).DataContext).Path;
            BasicFunctionality.ContextOpenPath(path);
        }

        private void ContextCopyLink(object sender, RoutedEventArgs e) {
            string link = ((CommonThingsView) ((MenuItem) sender).DataContext).Link;
            BasicFunctionality.CopyURL(link);
        }

        private void ContextVideoPlay(object sender, RoutedEventArgs e) {
            ((VideoView) ((MenuItem) sender).DataContext).ParentVideo.ContextPlayVideo();
        }

        private void ContextDeleteVideo(object sender, RoutedEventArgs e) {
            ((VideoView) ((MenuItem) sender).DataContext).ParentVideo.ContextDeleteVideo();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            LoadUrl();
        }

        private void LoadUrl() {
            PlayList listId = new PlayList();
            if (PlayList.SingleVideos != null)
                listId = PlayList.SingleVideos;
            string url = Link.Text;
            if (url.Contains(" "))
                BasicFunctionality.Error("Podany link jest nieprawidłowy!");
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
                        new PlayList(url, null);
                    else if (wynik == 3)
                        MessageBox.Show("Wystąpił błąd!", "Powerful YouTube DL", MessageBoxButton.OK, MessageBoxImage.Error);
                } else
                    new PlayList(new Video(url, listId));
            } else if (url.Contains("playlist") || url.Contains("list")) {
                new PlayList(url, null);
            } else
                BasicFunctionality.Error("Podany link jest nieprawidłowy!");

            if (listId.VideoIDsToGetParams.Count > 0)
                listId.GetParamsOfVideos();
        }

        private void playlist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            PlaylistView item = (PlaylistView) Playlist.SelectedItem;
            int index = Playlist.SelectedIndex;
            if (index != -1 && index < PlayList.ListOfPlayLists.Count) {
                DeleteAllVideosFromList();
                Thread ths = new Thread(async () => {
                    int x = item.ParentPlaylist.ListOfVideosInPlayList.Count;
                    for (int i = 0; i < x; i++) {
                        int j = i;
                        if (index != -1) {
                            BasicFunctionality.InvokeShit(DispatcherPriority.Send, async () => {
                                if (index < PlayList.ListOfPlayLists.Count &&
                                    j < item.ParentPlaylist.ListOfVideosInPlayList.Count) {
                                    AddVideoToList(item.ParentPlaylist.ListOfVideosInPlayList[j].Position,
                                        item.ParentPlaylist.Position.Id);
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

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            Video.IsManualDownload = true;
            DownloadHandler.Load();
            DownloadHandler.DownloadQueueAsync();
        }

        public void AddVideoToList(VideoView videom, string playlistId) {
            int index = Playlist.SelectedIndex;
            if (index != -1 && PlayList.ListOfPlayLists[index].Position.Id == playlistId) {
                VideosInActivePlayList.Add(videom);
            }
        }

        public void DeleteVideoFromAdd(VideoView pos, string playlistId) {
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
                return ((item as VideoView)?.Title.IndexOf(Search.Text, StringComparison.OrdinalIgnoreCase) >= 0);
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

        private void AutoLoad() {
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

        private void OpenSetting(object sender, RoutedEventArgs e) {
            Tabs.SelectedIndex = 0;
            UserSettings ss = new UserSettings();
            ss.ShowDialog();
        }

        private void CheckChanged(object sender, RoutedEventArgs e) {
            bool? isCheckedPlaylist = ((CheckBox) sender).IsChecked;
            PlayList playList = ((PlaylistView) ((CheckBox) sender).DataContext).ParentPlaylist;
            BasicFunctionality.CheckChanged(isCheckedPlaylist, playList);
        }

        private void OpenFolderOrBrowser(object sender, MouseButtonEventArgs e) {
            VideoView data = ((VideoView) ((System.Windows.Controls.ListView) sender).SelectedItem);
            BasicFunctionality.OpenFolderOrBrowserVideo(data);
        }

        private string _tmpUrl = "";

        private void link_LostFocus(object sender, RoutedEventArgs e) {
            if (!BasicFunctionality.CheckIfYoutubeUrl(Link.Text))
                Link.Text = "Link do kanału, playlisty lub video";
        }

        private void link_TextChanged(object sender, TextChangedEventArgs e) {
            if (BasicFunctionality.CheckIfYoutubeUrl(Link.Text)) {
                _tmpUrl = Link.Text;
                if (Settings.Default.autoLoadLink)
                    AutoLoad();
            }
        }

        private void Link_GotMouseCapture(object sender, MouseEventArgs e) {
            if (Link.Text == "")
                Link.Text = _tmpUrl;
            if (BasicFunctionality.CheckIfYoutubeUrl(Clipboard.GetText())) {
                _tmpUrl = Clipboard.GetText();
                Link.Text = _tmpUrl;
                if (Settings.Default.autoLoadLink)
                    AutoLoad();
            }
            if (Link.CaretIndex == 0) {
                Link.Focus();
                Link.SelectAll();
            }
        }

        private void Link_GotFocus(object sender, RoutedEventArgs e) {
            _tmpUrl = Link.Text;
            Link.Text = "";
        }

        private void Link_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Link.Focus();
            Link.SelectAll();
        }

        private void link_lSubmit(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter)
                return;
            if (BasicFunctionality.CheckIfYoutubeUrl(Link.Text))
                LoadUrl();
            else
                BasicFunctionality.Error("Podany link jest nieprawidłowy");
        }
    }
}