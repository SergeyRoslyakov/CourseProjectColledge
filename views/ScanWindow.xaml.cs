using System.Windows;
using System.Windows.Controls;
using QualityAppWPF.Models;
using QualityAppWPF.Services;

namespace QualityAppWPF.Views
{
    public partial class ScanWindow : Window
    {
        private readonly DatabaseService _dbService;
        private Product _currentProduct;

        public ScanWindow(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
        }

        // Обработчик изменения текста в поле штрих-кода
        private void TxtBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                CheckProduct(txtBarcode.Text);
            }
        }

        // Обработчик выбора тестового продукта
        private void CmbTestProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTestProducts.SelectedItem is ComboBoxItem selectedItem)
            {
                string itemText = selectedItem.Content.ToString();
                // Извлекаем штрих-код из текста (первые 13 цифр)
                string barcode = System.Text.RegularExpressions.Regex.Match(itemText, @"\d+").Value;

                if (!string.IsNullOrEmpty(barcode))
                {
                    txtBarcode.Text = barcode;
                    CheckProduct(barcode);
                }
            }
        }

        private async void CheckProduct(string barcode)
        {
            try
            {
                var product = await _dbService.GetProductByBarcodeAsync(barcode);

                if (product != null)
                {
                    _currentProduct = product;

                    txtProductName.Text = product.Name;
                    txtCategory.Text = product.Category;
                    txtManufacturer.Text = product.Manufacturer;
                    txtFoundBarcode.Text = product.Barcode;

                    borderResult.Visibility = Visibility.Visible;
                    borderNotFound.Visibility = Visibility.Collapsed;
                    btnProceed.IsEnabled = true;
                }
                else
                {
                    _currentProduct = null;

                    borderResult.Visibility = Visibility.Collapsed;
                    borderNotFound.Visibility = Visibility.Visible;
                    btnProceed.IsEnabled = false;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке продукта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddNewProduct_Click(object sender, RoutedEventArgs e)
        {
            string barcode = txtBarcode.Text;

            if (string.IsNullOrWhiteSpace(barcode))
            {
                MessageBox.Show("Введите штрих-код", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addProductWindow = new AddProductWindow(_dbService, barcode);
            addProductWindow.Owner = this;

            if (addProductWindow.ShowDialog() == true)
            {
                // После закрытия окна проверяем продукт снова
                if (!string.IsNullOrWhiteSpace(barcode))
                {
                    CheckProduct(barcode);
                }
            }
        }

        // Кнопка "Отмена"
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Кнопка "Перейти к проверке"
        private void BtnProceed_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProduct != null)
            {
                var checkWindow = new Views.CheckWindow(_dbService, _currentProduct);
                checkWindow.Owner = this;
                checkWindow.ShowDialog();
                this.Close();
            }
        }
    }
}