using System.Windows;

namespace powerful_youtube_dl
{
    public partial class Dialog : Window
    {
        public Dialog(string question, string title, string radio11, string radio22) // 0-user, 1-video z playlistą, 2-anulowano
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PromptDialog_Loaded);
            txtQuestion.Text = question;
            Title = title;
            radio1.Content = radio11;
            radio2.Content = radio22;
        }

        void PromptDialog_Loaded(object sender, RoutedEventArgs e)
        {
            radio1.Focus();
        }

        public static int Prompt(string question, string title, string radio11, string radio22)
        {
            Dialog inst = new Dialog(question, title, radio11, radio22);
            inst.ShowDialog();
            if (inst.DialogResult == true)
                return inst.Response;
            return 2;
        }

        public int Response
        {
            get
            {
                if ((bool)radio1.IsChecked)
                    return 0;
                else if ((bool)radio2.IsChecked)
                    return 1;
                else
                    return 2;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}