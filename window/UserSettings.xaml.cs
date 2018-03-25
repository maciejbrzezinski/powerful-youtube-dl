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

            startwithsystem.IsChecked = Properties.Settings.Default.startWithSystem;
            if ((bool)startwithsystem.IsChecked)
                startminimized.IsEnabled = true;
            else
                startminimized.IsEnabled = false;

            dlhistory.IsChecked = Properties.Settings.Default.dlHistory;
            dotray.IsChecked = Properties.Settings.Default.toTray;
            startminimized.IsChecked = Properties.Settings.Default.startMinimalized;
        }

        public static void getSettingsValues()
        {
            
        }

        private void Button_DownloadPath(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("KLIK");
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
                    MainWindow.downloadPath = dialog.SelectedPath;
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
            switch (check.Name) {
                case "playlistAsFolder":
                    Properties.Settings.Default.plAsFolder = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    break;
                case "autoLoadLink":
                    Properties.Settings.Default.autoLoadLink = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    break;
                case "startwithsystem":
                    Properties.Settings.Default.startWithSystem = (bool)check.IsChecked;
                    Properties.Settings.Default.Save();
                    if ((bool)check.IsChecked)
                        startminimized.IsEnabled = true;
                    else
                        startminimized.IsEnabled = false;
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
            }
        }
    }
}
