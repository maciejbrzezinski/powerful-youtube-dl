using Microsoft.Win32;
using powerful_youtube_dl.thinkingPart;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace powerful_youtube_dl.window
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class UserSettings
    {
        public UserSettings()
        {
            InitializeComponent();
            getSettings();
        }

        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private void getSettings()
        {
            string ytDlPath = Properties.Settings.Default.ytdlexe;
            string downloadPath = Properties.Settings.Default.dlpath;
            string logDestinate = Properties.Settings.Default.logsDestination;
            int maxDownloads = Properties.Settings.Default.maxDownloading;

            if (ytDlPath != "")
                textYTDL.Text = ytDlPath;

            if (downloadPath != "")
                textDestination.Text = downloadPath;

            if (maxDownloads > -1)
                maxDownloading.Text = maxDownloads.ToString();

            if(logDestinate != "")
                logsDestination.Text = Properties.Settings.Default.logsDestination;

            playlistAsFolder.IsChecked = Properties.Settings.Default.playlistAsFolder;
            autoLoadLink.IsChecked = Properties.Settings.Default.autoLoadLink;

            startWithSystem.IsChecked = Properties.Settings.Default.startWithSystem;

            doTray.IsChecked = Properties.Settings.Default.doTray;
            startMinimized.IsChecked = Properties.Settings.Default.startMinimized;
            closeToTray.IsChecked = Properties.Settings.Default.closeToTray;

            autoStartDownload.IsChecked = Properties.Settings.Default.autoStartDownload;

            autoObservePlaylists.IsChecked = Properties.Settings.Default.autoObservePlaylists;
            observePlaylistGrid.IsEnabled = (bool)autoObservePlaylists.IsChecked;

            savePlaylists.IsChecked = Properties.Settings.Default.savePlaylists;
            autoDownloadObserve.IsChecked = Properties.Settings.Default.autoDownloadObserve;
            messageAfterDownload.IsChecked = Properties.Settings.Default.messageAfterDownload;

            createLogs.IsChecked = (bool)Properties.Settings.Default.createLogs;
            logsGrid.IsEnabled = (bool)createLogs.IsChecked;
        }

        private void selectYoutubeDLPath(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.DefaultExt = ".exe";
            dialog.Filter = "Exe Files (*.exe)|*.exe";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                Properties.Settings.Default.ytdlexe = dialog.FileName;
                Properties.Settings.Default.Save();
                textYTDL.Text = dialog.FileName;
            }
        }

        private void selectDestinationFolder(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (dialog.SelectedPath != "")
                {
                    textDestination.Text = dialog.SelectedPath;
                    saveSetting(textDestination.Name, dialog.SelectedPath);
                }
            }
        }

        private void selectLogDestination(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (dialog.SelectedPath != "")
                {
                    logsDestination.Text = dialog.SelectedPath;
                    saveSetting(logsDestination.Name, dialog.SelectedPath);
                    Statistics.logPath = dialog.SelectedPath;
                }
            }
        }

        private void setMaxDownloads(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox field = (System.Windows.Controls.TextBox)sender;
            int num = 0;
            if (Int32.TryParse(field.Text, out num))
            {
                vTemp = field.Text;
                saveSetting(field.Name, num);
            }
        }

        private void checkChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.CheckBox check = (System.Windows.Controls.CheckBox)sender;
                saveSetting(check.Name, (bool)check.IsChecked);
                switch (check.Name)
                {
                    case "autoLoadLink":
                        hierarchia.IsEnabled = (bool)check.IsChecked;
                        break;
                    case "startWithSystem":
                        if ((bool)check.IsChecked)
                            rkApp.SetValue("PowerfulYTDownloader", System.Windows.Forms.Application.ExecutablePath);
                        else
                        {
                            try
                            {
                                rkApp.DeleteValue("PowerfulYTDownloader");
                            }
                            catch { }
                        }
                        break;
                    case "autoObservePlaylists":
                        observePlaylistGrid.IsEnabled = (bool)check.IsChecked;
                        break;
                    case "createLogs":
                        logsGrid.IsEnabled = (bool)check.IsChecked;
                        break;
                }
            }
            catch { }
        }

        private void saveSetting(string setting, object status)
        {
            Properties.Settings.Default[setting] = status;
            Properties.Settings.Default.Save();
        }

        private int getIntegerValue(string value)
        {
            int a = -1;
            Int32.TryParse(value, out a);
            if (value == "0")
                return 0;
            else if (a == 0)
                return -1;
            else
                return a;
        }

        private void checkRestFields(int value, System.Windows.Controls.TextBox t1, System.Windows.Controls.TextBox t2, System.Windows.Controls.TextBox t3)
        {
            if (getIntegerValue(t1.Text) == value)
            {
                t1.Text = vTemp;
                saveSetting(t1.Name, value);
            }
            else if (getIntegerValue(t2.Text) == value)
            {
                t2.Text = vTemp;
                saveSetting(t2.Name, value);
            }
            else if (getIntegerValue(t3.Text) == value)
            {
                t3.Text = vTemp;
                saveSetting(t3.Name, value);
            }
        }


        private string vTemp = "";

        private void saveValueGotFocus(object sender, RoutedEventArgs e)
        {
            vTemp = ((System.Windows.Controls.TextBox)sender).Text;
        }

        private void maxDownloadLostFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            int a = getIntegerValue(box.Text);
            if (a > -1)
            {
                saveSetting(box.Name, a);
            }
            else
                box.Text = vTemp;
        }
    }
}
