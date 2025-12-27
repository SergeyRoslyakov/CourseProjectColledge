using System.Windows;

namespace QualityAppWPF.Views
{
    public partial class SimpleInputWindow : Window
    {
        public string Value1 { get; private set; }
        public string Value2 { get; private set; }
        public string Value3 { get; private set; }

        public bool ThirdFieldVisible
        {
            get => lblField3.Visibility == Visibility.Visible;
            set
            {
                lblField3.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                txtField3.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // Конструктор для 2 полей
        public SimpleInputWindow(string title, string field1Label, string field2Label)
            : this(title, field1Label, field2Label, "", "", "", "")
        {
        }

        // Конструктор для 2 полей со значениями
        public SimpleInputWindow(string title, string field1Label, string field2Label,
                               string field1Value, string field2Value)
            : this(title, field1Label, field2Label, "", field1Value, field2Value, "")
        {
        }

        // Полный конструктор для 3 полей
        public SimpleInputWindow(string title, string field1Label, string field2Label, string field3Label,
                               string field1Value, string field2Value, string field3Value)
        {
            InitializeComponent();

            Title = title;
            txtTitle.Text = title;
            lblField1.Text = field1Label;
            lblField2.Text = field2Label;
            lblField3.Text = field3Label;

            txtField1.Text = field1Value;
            txtField2.Text = field2Value;
            txtField3.Text = field3Value;

            // Показываем третье поле только если задана подпись
            ThirdFieldVisible = !string.IsNullOrEmpty(field3Label);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtField1.Text))
            {
                MessageBox.Show("Заполните обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Value1 = txtField1.Text.Trim();
            Value2 = txtField2.Text.Trim();
            Value3 = txtField3.Text.Trim();

            DialogResult = true;
            Close();
        }
    }
}