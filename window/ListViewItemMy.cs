using System.ComponentModel;

namespace powerful_youtube_dl
{
    public class ListViewItemMy : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool check;
        private string title, duration, status;
        private Video parent;

        public string Title
        {
            get => title;
            set
            {
                title = value;
                NotifyPropertyChanged("Title");
            }
        }

        public string Duration
        {
            get => duration;
            set
            {
                duration = value;
                NotifyPropertyChanged("Duration");
            }
        }

        public string Status
        {
            get => status;
            set
            {
                status = value;
                NotifyPropertyChanged("Status");
            }
        }

        public bool Check
        {
            get => check;
            set
            {
                check = value;
                NotifyPropertyChanged("Check");
            }
        }

        public Video Parent
        {
            get => parent;
            set
            {
                parent = value;
                NotifyPropertyChanged("Parent");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}