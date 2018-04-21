using System.Windows;

namespace powerful_youtube_dl
{
    public partial class Dialog : Window
    {
        private static int toOut = 2;

        public Dialog(string question, string title, string radio11, string radio22) // 0-pierwsze radio, 1-drugie radio, 2-anulowano, 3-niepoprawnie wypełniono
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PromptDialog_Loaded);
            toOut = 2;
            txtQuestion.Text = question;
            Title = title;
            radio1.Content = radio11;
            radio1.IsChecked = true;
            radio2.Content = radio22;
        }

        public int Response
        {
            get
            {
                if ((bool) radio1.IsChecked)
                    return 0;
                else if ((bool) radio2.IsChecked)
                    return 1;
                else
                    return 3;
            }
        }

        public static int Prompt(string question, string title, string radio11, string radio22)
        {
            Dialog inst = new Dialog(question, title, radio11, radio22);
            inst.ShowDialog();
            if (inst.DialogResult == true)
                toOut = inst.Response;
            return toOut;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            toOut = 2;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if ((bool) radio1.IsChecked)
                toOut = 0;
            else if ((bool) radio2.IsChecked)
                toOut = 1;
            else
                toOut = 3;
            DialogResult = true;
            Close();
        }

        private void PromptDialog_Loaded(object sender, RoutedEventArgs e)
        {
            radio1.Focus();
        }
    }
}