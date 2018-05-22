using powerful_youtube_dl.thinkingPart;
using System.ComponentModel;

namespace powerful_youtube_dl.window {

    public class ListViewItemMy : INotifyPropertyChanged {
        private bool? _check;

        private Video _parentV;
        private PlayList _parentPl;

        private string _title, _duration, _status, _id, _link, _path, _toolTip;
        private int _licznik;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ToolTip {
            get => _toolTip;
            set {
                _toolTip = value;
                NotifyPropertyChanged("ToolTip");
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

        public bool? Check {
            get => _check;
            set {
                _check = value;
                if (_parentPl == null && _parentV != null) {
                    if (CheckBool) {
                        if (_licznik == 0) {
                            _licznik++;
                            _parentV.PlayList.CheckedCount++;
                        }
                    } else {
                        if (_licznik == 1) {
                            _licznik--;
                            _parentV.PlayList.CheckedCount--;
                        }
                    }

                    if (_parentV.PlayList.CheckedCount == _parentV.PlayList.ListOfVideosInPlayList.Count)
                        _parentV.PlayList.Position.Check = true;
                    else if (_parentV.PlayList.CheckedCount == 0)
                        _parentV.PlayList.Position.Check = false;
                    else
                        _parentV.PlayList.Position.Check = null;
                }
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

        public string Duration {
            get => _duration;
            set {
                _duration = value;
                NotifyPropertyChanged("Duration");
            }
        }

        public Video ParentV {
            get => _parentV;
            set {
                _parentV = value;
                NotifyPropertyChanged("ParentV");
            }
        }

        public PlayList ParentPl {
            get => _parentPl;
            set {
                _parentPl = value;
                NotifyPropertyChanged("ParentPL");
            }
        }

        public string Status {
            get => _status;
            set {
                _status = value;
                if (_status == "Pobrano")
                    _parentV.IsDownloaded = true;
                else if (_parentV != null) {
                    _parentV.IsDownloaded = false;
                    Check = true;
                }
                NotifyPropertyChanged("Status");
            }
        }

        public string Title {
            get => _title;
            set {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }

        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}