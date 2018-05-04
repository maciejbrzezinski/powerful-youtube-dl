using System.ComponentModel;
using powerful_youtube_dl.thinkingPart;

namespace powerful_youtube_dl.window {

    public class ListViewItemMy : INotifyPropertyChanged {
        private bool? check;

        private Video parentV;
        private PlayList parentPL;

        private string title, duration, status, id, link;
        private int licznik;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public bool? Check {
            get => check;
            set {
                check = value;
                if (parentPL == null && parentV != null) {
                    if (CheckBool) {
                        if (licznik == 0) {
                            licznik++;
                            parentV.playList.checkedCount++;
                        }
                    } else {
                        if (licznik == 1) {
                            licznik--;
                            parentV.playList.checkedCount--;
                        }
                    }

                    if (parentV.playList.checkedCount == parentV.playList._listOfVideosInPlayList.Count)
                        parentV.playList.position.Check = true;
                    else if (parentV.playList.checkedCount == 0)
                        parentV.playList.position.Check = false;
                    else
                        parentV.playList.position.Check = null;
                }
                NotifyPropertyChanged("Check");
            }
        }

        public bool CheckBool {
            get {
                if (check != null)
                    return (bool) check;
                return false;
            }
        }

        public string Duration {
            get => duration;
            set {
                duration = value;
                NotifyPropertyChanged("Duration");
            }
        }

        public Video ParentV {
            get => parentV;
            set {
                parentV = value;
                NotifyPropertyChanged("ParentV");
            }
        }

        public PlayList ParentPL {
            get => parentPL;
            set {
                parentPL = value;
                NotifyPropertyChanged("ParentPL");
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}