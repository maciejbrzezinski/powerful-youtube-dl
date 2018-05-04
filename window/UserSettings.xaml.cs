using Microsoft.Win32;
using powerful_youtube_dl.Properties;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using CheckBox = System.Windows.Controls.CheckBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace powerful_youtube_dl.window {

    public partial class UserSettings {

        public UserSettings() {
            InitializeComponent();
            GetSettings();
        }

        private readonly RegistryKey _rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private void GetSettings() {
            string ytDlPath = Settings.Default.ytdlexe;
            string downloadPath = Settings.Default.textDestination;
            string logDestinate = Settings.Default.logsDestination;
            int maxDownloads = Settings.Default.maxDownloading;

            if (ytDlPath != "")
                TextYtdl.Text = ytDlPath;

            if (downloadPath != "")
                TextDestination.Text = downloadPath;

            if (maxDownloads > -1)
                MaxDownloading.Text = maxDownloads.ToString();

            if (logDestinate != "")
                LogsDestination.Text = Settings.Default.logsDestination;

            PlaylistAsFolder.IsChecked = Settings.Default.playlistAsFolder;
            AutoLoadLink.IsChecked = Settings.Default.autoLoadLink;

            StartWithSystem.IsChecked = Settings.Default.startWithSystem;

            DoTray.IsChecked = Settings.Default.doTray;
            StartMinimized.IsChecked = Settings.Default.startMinimized;
            CloseToTray.IsChecked = Settings.Default.closeToTray;

            AutoStartDownload.IsChecked = Settings.Default.autoStartDownload;

            AutoObservePlaylists.IsChecked = Settings.Default.autoObservePlaylists;
            ObservePlaylistGrid.IsEnabled = (bool) AutoObservePlaylists.IsChecked;

            SavePlaylists.IsChecked = Settings.Default.savePlaylists;
            AutoDownloadObserve.IsChecked = Settings.Default.autoDownloadObserve;
            MessageAfterDownload.IsChecked = Settings.Default.messageAfterDownload;

            CreateLogs.IsChecked = Settings.Default.createLogs;
            LogsGrid.IsEnabled = (bool) CreateLogs.IsChecked;
        }

        private void SelectYoutubeDlPath(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog {
                DefaultExt = ".exe",
                Filter = "Exe Files (*.exe)|*.exe"
            };

            bool? result = dialog.ShowDialog();

            if (result == true) {
                SaveSetting("ytdlexe", dialog.FileName);
                TextYtdl.Text = dialog.FileName;
            }
        }

        private void SelectDestinationFolder(object sender, RoutedEventArgs e) {
            using (var dialog = new FolderBrowserDialog()) {
                dialog.ShowDialog();
                if (dialog.SelectedPath != "") {
                    TextDestination.Text = dialog.SelectedPath;
                    SaveSetting(TextDestination.Name, TextDestination.Text);
                }
            }
        }

        private void SelectLogDestination(object sender, RoutedEventArgs e) {
            using (var dialog = new FolderBrowserDialog()) {
                dialog.ShowDialog();
                if (dialog.SelectedPath != "") {
                    LogsDestination.Text = dialog.SelectedPath;
                    SaveSetting(LogsDestination.Name, dialog.SelectedPath);
                }
            }
        }

        private void SetMaxDownloads(object sender, TextChangedEventArgs e) {
            TextBox field = (TextBox) sender;
            if (Int32.TryParse(field.Text, out var num)) {
                _vTemp = field.Text;
                SaveSetting(field.Name, num);
            }
        }

        private void CheckChanged(object sender, RoutedEventArgs e) {
            try {
                CheckBox check = (CheckBox) sender;
                SaveSetting(check.Name, check.IsChecked != null && (bool) check.IsChecked);
                switch (check.Name) {
                    case "autoLoadLink":
                        if (check.IsChecked != null)
                            Hierarchia.IsEnabled = (bool) check.IsChecked;
                        break;

                    case "startWithSystem":
                        if (check.IsChecked != null && (bool) check.IsChecked)
                            _rkApp.SetValue("PowerfulYTDownloader", Application.ExecutablePath);
                        else {
                            try {
                                _rkApp.DeleteValue("PowerfulYTDownloader");
                            } catch { }
                        }
                        break;

                    case "autoObservePlaylists":
                        if (check.IsChecked != null)
                            ObservePlaylistGrid.IsEnabled = (bool) check.IsChecked;
                        break;

                    case "createLogs":
                        if (check.IsChecked != null)
                            LogsGrid.IsEnabled = (bool) check.IsChecked;
                        break;
                }
            } catch { }
        }

        public static void SaveSetting(string setting, object status) {
            Settings.Default[setting] = status;
            Settings.Default.Save();
        }

        private int GetIntegerValue(string value) {
            Int32.TryParse(value, out var a);
            if (value == "0")
                return 0;
            if (a == 0)
                return -1;
            return a;
        }

        private string _vTemp = "";

        private void SaveValueGotFocus(object sender, RoutedEventArgs e) {
            _vTemp = ((TextBox) sender).Text;
        }

        private void MaxDownloadLostFocus(object sender, RoutedEventArgs e) {
            TextBox box = (TextBox) sender;
            int a = GetIntegerValue(box.Text);
            if (a > -1) {
                SaveSetting(box.Name, a);
            } else
                box.Text = _vTemp;
        }
    }
}