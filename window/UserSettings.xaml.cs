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
            getSettings();
        }

        private readonly RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private void getSettings() {
            string ytDlPath = Settings.Default.ytdlexe;
            string downloadPath = Settings.Default.textDestination;
            string logDestinate = Settings.Default.logsDestination;
            int maxDownloads = Settings.Default.maxDownloading;

            if (ytDlPath != "")
                textYTDL.Text = ytDlPath;

            if (downloadPath != "")
                textDestination.Text = downloadPath;

            if (maxDownloads > -1)
                maxDownloading.Text = maxDownloads.ToString();

            if (logDestinate != "")
                logsDestination.Text = Settings.Default.logsDestination;

            playlistAsFolder.IsChecked = Settings.Default.playlistAsFolder;
            autoLoadLink.IsChecked = Settings.Default.autoLoadLink;

            startWithSystem.IsChecked = Settings.Default.startWithSystem;

            doTray.IsChecked = Settings.Default.doTray;
            startMinimized.IsChecked = Settings.Default.startMinimized;
            closeToTray.IsChecked = Settings.Default.closeToTray;

            autoStartDownload.IsChecked = Settings.Default.autoStartDownload;

            autoObservePlaylists.IsChecked = Settings.Default.autoObservePlaylists;
            observePlaylistGrid.IsEnabled = (bool) autoObservePlaylists.IsChecked;

            savePlaylists.IsChecked = Settings.Default.savePlaylists;
            autoDownloadObserve.IsChecked = Settings.Default.autoDownloadObserve;
            messageAfterDownload.IsChecked = Settings.Default.messageAfterDownload;

            createLogs.IsChecked = Settings.Default.createLogs;
            logsGrid.IsEnabled = (bool) createLogs.IsChecked;
        }

        private void selectYoutubeDLPath(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog {
                DefaultExt = ".exe",
                Filter = "Exe Files (*.exe)|*.exe"
            };

            bool? result = dialog.ShowDialog();

            if (result == true) {
                Settings.Default.ytdlexe = dialog.FileName;
                Settings.Default.Save();
                textYTDL.Text = dialog.FileName;
            }
        }

        private void selectDestinationFolder(object sender, RoutedEventArgs e) {
            using (var dialog = new FolderBrowserDialog()) {
                dialog.ShowDialog();
                if (dialog.SelectedPath != "") {
                    textDestination.Text = dialog.SelectedPath;
                    saveSetting(textDestination.Name, textDestination.Text);
                }
            }
        }

        private void selectLogDestination(object sender, RoutedEventArgs e) {
            using (var dialog = new FolderBrowserDialog()) {
                dialog.ShowDialog();
                if (dialog.SelectedPath != "") {
                    logsDestination.Text = dialog.SelectedPath;
                    saveSetting(logsDestination.Name, dialog.SelectedPath);
                }
            }
        }

        private void setMaxDownloads(object sender, TextChangedEventArgs e) {
            TextBox field = (TextBox) sender;
            if (Int32.TryParse(field.Text, out var num)) {
                vTemp = field.Text;
                saveSetting(field.Name, num);
            }
        }

        private void checkChanged(object sender, RoutedEventArgs e) {
            try {
                CheckBox check = (CheckBox) sender;
                saveSetting(check.Name, check.IsChecked != null && (bool) check.IsChecked);
                switch (check.Name) {
                    case "autoLoadLink":
                        if (check.IsChecked != null)
                            hierarchia.IsEnabled = (bool) check.IsChecked;
                        break;

                    case "startWithSystem":
                        if (check.IsChecked != null && (bool) check.IsChecked)
                            rkApp.SetValue("PowerfulYTDownloader", Application.ExecutablePath);
                        else {
                            try {
                                rkApp.DeleteValue("PowerfulYTDownloader");
                            } catch { }
                        }
                        break;

                    case "autoObservePlaylists":
                        if (check.IsChecked != null)
                            observePlaylistGrid.IsEnabled = (bool) check.IsChecked;
                        break;

                    case "createLogs":
                        if (check.IsChecked != null)
                            logsGrid.IsEnabled = (bool) check.IsChecked;
                        break;
                }
            } catch { }
        }

        private void saveSetting(string setting, object status) {
            Settings.Default[setting] = status;
            Settings.Default.Save();
        }

        private int getIntegerValue(string value) {
            Int32.TryParse(value, out var a);
            if (value == "0")
                return 0;
            if (a == 0)
                return -1;
            return a;
        }

        private string vTemp = "";

        private void saveValueGotFocus(object sender, RoutedEventArgs e) {
            vTemp = ((TextBox) sender).Text;
        }

        private void maxDownloadLostFocus(object sender, RoutedEventArgs e) {
            TextBox box = (TextBox) sender;
            int a = getIntegerValue(box.Text);
            if (a > -1) {
                saveSetting(box.Name, a);
            } else
                box.Text = vTemp;
        }
    }
}