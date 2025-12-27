using System.Windows;
using QualityAppWPF.Models;
using QualityAppWPF.Services;

namespace QualityAppWPF.Views
{
    public partial class CheckWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly Product _product;

        public CheckWindow(DatabaseService dbService, Product product)
        {
            InitializeComponent();
            _dbService = dbService;
            _product = product;

            // Показываем информацию о продукте
            txtProductInfo.Text = $"{product.Name} ({product.Barcode})\n" +
                                 $"Категория: {product.Category}\n" +
                                 $"Производитель: {product.Manufacturer}";
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Получаем оценку
            int rating = 3;
            if (rbRating1.IsChecked == true) rating = 1;
            else if (rbRating2.IsChecked == true) rating = 2;
            else if (rbRating3.IsChecked == true) rating = 3;
            else if (rbRating4.IsChecked == true) rating = 4;
            else if (rbRating5.IsChecked == true) rating = 5;

            var check = new QualityCheck
            {
                ProductId = _product.Id,
                ProductBarcode = _product.Barcode,
                ProductName = _product.Name,
                Rating = rating,
                Comment = txtComment.Text,
                Inspector = txtInspector.Text,
                Location = txtLocation.Text
            };

            try
            {
                bool success = await _dbService.AddQualityCheckAsync(check);

                if (success)
                {
                    MessageBox.Show("Проверка сохранена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при сохранении", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnDetailedCheck_Click(object sender, RoutedEventArgs e)
        {
            // Получаем ID сохраненной проверки
            // Для демонстрации - создаем новую запись
            var detailedWindow = new DetailedCheckWindow(_dbService, 1); // В реальности передать ID
            detailedWindow.Owner = this;
            detailedWindow.ShowDialog();
        }
    }
}