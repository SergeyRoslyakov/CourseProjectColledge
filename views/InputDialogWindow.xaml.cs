using System.Windows;

namespace QualityAppWPF.Views
{
    public partial class InputDialogWindow : Window
    {
        public string Result { get; private set; }

        public InputDialogWindow(string title, string prompt)
        {
            InitializeComponent();

            Title = title;
            txtTitle.Text = prompt;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("Введите значение", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Result = txtInput.Text.Trim();
            DialogResult = true;
            Close();
        }
    }
}