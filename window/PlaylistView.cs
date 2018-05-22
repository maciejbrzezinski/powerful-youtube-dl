namespace powerful_youtube_dl.window {
    public class PlaylistView : CommonThingsView {
        private int _countVideos;

        public int CountVideos {
            get => _countVideos;
            set {
                _countVideos = value;
                NotifyPropertyChanged("CountVideos");
            }
        }
    }
}
