using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
            int maxDownloads = Properties.Settings.Default.maxDownloading;

            if (ytDlPath != "")
                textYTDL.Text = ytDlPath;

            if (downloadPath != "")
                textDestination.Text = downloadPath;

            if (maxDownloads > 0)
                textMaxDownloads.Text = maxDownloads.ToString();

            playlistAsFolder.IsChecked = Properties.Settings.Default.plAsFolder;
            autoLoadLink.IsChecked = Properties.Settings.Default.autoLoadLink;
            if ((bool)autoLoadLink.IsChecked)
                hierarchia.IsEnabled = true;
            else
                hierarchia.IsEnabled = false;

            startwithsystem.IsChecked = Properties.Settings.Default.startWithSystem;
            if ((bool)startwithsystem.IsChecked)
                startminimized.IsEnabled = true;
            else
                startminimized.IsEnabled = false;

            dlhistory.IsChecked = Properties.Settings.Default.dlHistory;
            dotray.IsChecked = Properties.Settings.Default.toTray;
            startminimized.IsChecked = Properties.Settings.Default.startMinimalized;
            closeToTray.IsChecked = Properties.Settings.Default.closeToTray;

            autoStartDownload.IsChecked = Properties.Settings.Default.autoDownload;
            if ((bool)autoStartDownload.IsChecked)
                hierarchyFields.IsEnabled = true;
            else
                hierarchyFields.IsEnabled = false;

            hierSingleVideo.Text = Properties.Settings.Default.hierSingleVideo.ToString();
            hierSinglePlaylist.Text = Properties.Settings.Default.hierSinglePlaylist.ToString();
            hierAllPlaylists.Text = Properties.Settings.Default.hierAllPlaylists.ToString();
            hierAllUsersVideos.Text = Properties.Settings.Default.hierAllUsersVideos.ToString();
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
                    Properties.Settings.Default.dlpath = dialog.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void setMaxDownloads(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox field = (System.Windows.Controls.TextBox)sender;
            short num = 0;
            if (Int16.TryParse(field.Text, out num))
            {
                Properties.Settings.Default.maxDownloading = Int16.Parse(field.Text);
                Properties.Settings.Default.Save();
            }
        }

        private void checkChanged(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox check = (System.Windows.Controls.CheckBox)sender;
            switch (check.Name)
            {
                case "playlistAsFolder":
                    Properties.Settings.Default.plAsFolder = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    break;
                case "autoLoadLink":
                    Properties.Settings.Default.autoLoadLink = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    if ((bool)check.IsChecked)
                        hierarchia.IsEnabled = true;
                    else
                    {
                        hierarchia.IsEnabled = false;
                    }
                    break;
                case "startwithsystem":
                    Properties.Settings.Default.startWithSystem = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
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
                case "dlhistory":
                    Properties.Settings.Default.dlHistory = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    break;
                case "dotray":
                    Properties.Settings.Default.toTray = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    break;
                case "startminimized":
                    Properties.Settings.Default.startMinimalized = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    break;
                case "closeToTray":
                    Properties.Settings.Default.closeToTray = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    break;
                case "autoStartDownload":
                    Properties.Settings.Default.autoDownload = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    if ((bool)check.IsChecked)
                        hierarchyFields.IsEnabled = true;
                    else
                        hierarchyFields.IsEnabled = false;
                    break;
            }
        }

        private void hierarchyChange(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            saveHierarchy(box);
        }

        private void saveHierarchy(System.Windows.Controls.TextBox box)
        {
            switch (box.Name)
            {
                case "hierSingleVideo":
                    Properties.Settings.Default.hierSingleVideo = getIntegerValue(hierSingleVideo.Text);
                    Properties.Settings.Default.Save();
                    break;
                case "hierSinglePlaylist":
                    Properties.Settings.Default.hierSinglePlaylist = getIntegerValue(hierSinglePlaylist.Text);
                    Properties.Settings.Default.Save();
                    break;
                case "hierAllPlaylists":
                    Properties.Settings.Default.hierAllPlaylists = getIntegerValue(hierAllPlaylists.Text);
                    Properties.Settings.Default.Save();
                    break;
                case "hierAllUsersVideos":
                    Properties.Settings.Default.hierAllUsersVideos = getIntegerValue(hierAllUsersVideos.Text);
                    Properties.Settings.Default.Save();
                    break;
            }
        }

        private int getIntegerValue(string value)
        {
            short a = 0;
            Int16.TryParse(value, out a);
            return a;
        }

        private void checkRestFields(int value, System.Windows.Controls.TextBox t1, System.Windows.Controls.TextBox t2, System.Windows.Controls.TextBox t3)
        {
            if (getIntegerValue(t1.Text) == value)
            {
                t1.Text = vTemp;
                saveHierarchy(t1);
            }
            else if (getIntegerValue(t2.Text) == value)
            {
                t2.Text = vTemp;
                saveHierarchy(t2);
            }
            else if (getIntegerValue(t3.Text) == value)
            {
                t3.Text = vTemp;
                saveHierarchy(t3);
            }
        }

        private void checkIfProperlyValue(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            int a = getIntegerValue(box.Text);
            if (a == 1 || a == 2 || a == 3 || a == 4)
            {
                switch (box.Name)
                {
                    case "hierSingleVideo":
                        Properties.Settings.Default.hierSingleVideo = a;
                        checkRestFields(a, hierSinglePlaylist, hierAllPlaylists, hierAllUsersVideos);
                        break;
                    case "hierSinglePlaylist":
                        Properties.Settings.Default.hierSinglePlaylist = a;
                        checkRestFields(a, hierSingleVideo, hierAllPlaylists, hierAllUsersVideos);
                        break;
                    case "hierAllPlaylists":
                        Properties.Settings.Default.hierAllPlaylists = a;
                        checkRestFields(a, hierSinglePlaylist, hierSingleVideo, hierAllUsersVideos);
                        break;
                    case "hierAllUsersVideos":
                        Properties.Settings.Default.hierAllUsersVideos = a;
                        checkRestFields(a, hierSinglePlaylist, hierAllPlaylists, hierSingleVideo);
                        break;
                }
            }
            else
                box.Text = vTemp;
        }
        private string vTemp = "";

        private void saveValueGotFocus(object sender, RoutedEventArgs e)
        {
            vTemp = ((System.Windows.Controls.TextBox)sender).Text;
        }
    }
}
