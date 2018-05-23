using powerful_youtube_dl.thinkingPart;
using System.ComponentModel;

namespace powerful_youtube_dl.window {

    public class CommonThingsView : INotifyPropertyChanged {
        protected bool? _check;

        private PlayList _parentPl;

        private string _title, _status, _id, _link, _path;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title {
            get => _title;
            set {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }

        public virtual string Status {
            get => _status;
            set {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }

        public string Path {
            get => _path;
            set {
                _path = value;
                NotifyPropertyChanged("Path");
            }
        }

        public string Link {
            get => _link;
            set {
                _link = value;
                NotifyPropertyChanged("Link");
            }
        }

        public string Id {
            get => _id;
            set {
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public PlayList ParentPlaylist {
            get => _parentPl;
            set {
                _parentPl = value;
                NotifyPropertyChanged("ParentPlaylist");
            }
        }

        public virtual bool? Check {
            get => _check;
            set {
                _check = value;
                NotifyPropertyChanged("Check");
            }
        }

        public bool CheckBool {
            get {
                if (_check != null)
                    return (bool) _check;
                return false;
            }
        }

        public void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}