using powerful_youtube_dl.thinkingPart;

namespace powerful_youtube_dl.window {

    public class VideoView : CommonThingsView {
        private Video _parentV;
        private string _status, _duration;
        private int _licznik;

        public override bool? Check {
            get => _check;
            set {
                _check = value;
                if (ParentPlaylist != null && _parentV != null) {
                    if (CheckBool) {
                        if (_licznik == 0) {
                            _licznik++;
                            ParentPlaylist.CheckedCount++;
                        }
                    } else {
                        if (_licznik == 1) {
                            _licznik--;
                            ParentPlaylist.CheckedCount--;
                        }
                    }

                    if (ParentPlaylist.CheckedCount == ParentPlaylist.ListOfVideosInPlayList.Count)
                        ParentPlaylist.Position.Check = true;
                    else if (ParentPlaylist.CheckedCount == 0)
                        ParentPlaylist.Position.Check = false;
                    else
                        ParentPlaylist.Position.Check = null;
                }
                NotifyPropertyChanged("Check");
            }
        }

        public string Duration {
            get => _duration;
            set {
                _duration = value;
                NotifyPropertyChanged("Duration");
            }
        }

        public override string Status {
            get => _status;
            set {
                _status = value;
                if (_status == "Pobrano") {
                    _parentV.IsDownloaded = true;
                    Check = false;
                } else if (_parentV != null) {
                    _parentV.IsDownloaded = false;
                    Check = true;
                }
                NotifyPropertyChanged("Status");
            }
        }

        public Video ParentVideo {
            get => _parentV;
            set {
                _parentV = value;
                NotifyPropertyChanged("ParentVideo");
            }
        }
    }
}