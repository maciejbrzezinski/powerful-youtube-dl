﻿using System.ComponentModel;

namespace powerful_youtube_dl {

    public class ListViewItemMy : INotifyPropertyChanged {
        private bool check;

        private Video parent;

        private string title, duration, status, id, link;

        public event PropertyChangedEventHandler PropertyChanged;

        public ListViewItemMy() {
        }

        public string Link {
            get => link;
            set {
                link = value;
                NotifyPropertyChanged("Link");
            }
        }

        public string Id {
            get => id;
            set {
                id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public bool Check {
            get => check;
            set {
                check = value;
                NotifyPropertyChanged("Check");
            }
        }

        public string Duration {
            get => duration;
            set {
                duration = value;
                NotifyPropertyChanged("Duration");
            }
        }

        public Video Parent {
            get => parent;
            set {
                parent = value;
                NotifyPropertyChanged("Parent");
            }
        }

        public string Status {
            get => status;
            set {
                status = value;
                NotifyPropertyChanged("Status");
            }
        }

        public string Title {
            get => title;
            set {
                title = value;
                NotifyPropertyChanged("Title");
            }
        }

        private void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}