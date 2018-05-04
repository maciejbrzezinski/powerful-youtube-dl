using System.Windows;

namespace powerful_youtube_dl.window {

    public partial class Dialog {
        private static int _toOut = 2;

        public Dialog() {
        }

        public Dialog(string question, string title, string radio11, string radio22) // 0-pierwsze radio, 1-drugie radio, 2-anulowano, 3-niepoprawnie wypełniono
        {
            InitializeComponent();
            Loaded += PromptDialog_Loaded;
            _toOut = 2;
            TxtQuestion.Text = question;
            Title = title;
            Radio1.Content = radio11;
            Radio1.IsChecked = true;
            Radio2.Content = radio22;
        }

        public int Response {
            get {
                if (Radio1.IsChecked != null && (bool) Radio1.IsChecked)
                    return 0;
                if (Radio2.IsChecked != null && (bool) Radio2.IsChecked)
                    return 1;
                return 3;
            }
        }

        public static int Prompt(string question, string title, string radio11, string radio22) {
            Dialog inst = new Dialog(question, title, radio11, radio22);
            inst.ShowDialog();
            if (inst.DialogResult == true)
                _toOut = inst.Response;
            return _toOut;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            _toOut = 2;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (Radio1.IsChecked != null && (bool) Radio1.IsChecked)
                _toOut = 0;
            else if (Radio2.IsChecked != null && (bool) Radio2.IsChecked)
                _toOut = 1;
            else
                _toOut = 3;
            DialogResult = true;
            Close();
        }

        private void PromptDialog_Loaded(object sender, RoutedEventArgs e) {
            Radio1.Focus();
        }
    }
}