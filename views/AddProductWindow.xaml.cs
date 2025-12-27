using System;
using System.Windows;
using Microsoft.Win32;
using QualityAppWPF.Models;
using QualityAppWPF.Services;

namespace QualityAppWPF.Views
{
    public partial class AddProductWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly string _barcode;
        private string _selectedImagePath = "";

        public AddProductWindow(DatabaseService dbService, string barcode)
        {
            InitializeComponent();
            _dbService = dbService;
            _barcode = barcode;

            txtBarcode.Text = barcode;
            txtBarcode.IsEnabled = false;
            cmbCategory.SelectedIndex = 0;

            // Устанавливаем фокус на поле названия
            txtName.Focus();
        }

        public AddProductWindow(DatabaseService dbService) : this(dbService, "")
        {
            txtBarcode.IsEnabled = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Введите штрих-код продукта", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBarcode.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название продукта", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            // Парсим значения БЖУ
            double? protein = ParseNullableDouble(txtProtein.Text);
            double? fat = ParseNullableDouble(txtFat.Text);
            double? carbs = ParseNullableDouble(txtCarbs.Text);
            double? calories = ParseNullableDouble(txtCalories.Text);

            // Если указаны БЖУ, но не калории - рассчитываем автоматически
            if (calories == null && protein.HasValue && fat.HasValue && carbs.HasValue)
            {
                calories = protein * 4 + fat * 9 + carbs * 4;
                txtCalories.Text = calories.Value.ToString("0");
            }

            var product = new Product
            {
                Barcode = txtBarcode.Text,
                Name = txtName.Text,
                Category = (cmbCategory.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Другое",
                Manufacturer = txtManufacturer.Text,
                Protein = protein,
                Fat = fat,
                Carbs = carbs,
                Calories = calories
            };

            try
            {
                bool success = await _dbService.AddProductAsync(product);

                if (success)
                {
                    // Сохраняем изображение если выбрано
                    if (!string.IsNullOrEmpty(_selectedImagePath))
                    {
                        SaveProductImage(txtBarcode.Text, _selectedImagePath);
                    }

                    MessageBox.Show("Продукт успешно добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить продукт. Возможно, продукт с таким штрих-кодом уже существует.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double? ParseNullableDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (double.TryParse(text, out double result))
                return result;

            return null;
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Все файлы (*.*)|*.*",
                Title = "Выберите изображение продукта",
                CheckFileExists = true,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                txtImageName.Text = System.IO.Path.GetFileName(_selectedImagePath);

                // Показываем предпросмотр если это картинка
                if (System.IO.Path.GetExtension(_selectedImagePath).ToLower() == ".jpg" ||
                    System.IO.Path.GetExtension(_selectedImagePath).ToLower() == ".jpeg")
                {
                    txtImageName.Text += " ✓";
                }
            }
        }

        private void SaveProductImage(string barcode, string sourceImagePath)
        {
            try
            {
                string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                string imagesFolder = System.IO.Path.Combine(appFolder, "Images", "Products");

                if (!System.IO.Directory.Exists(imagesFolder))
                {
                    System.IO.Directory.CreateDirectory(imagesFolder);
                }
                string extension = System.IO.Path.GetExtension(sourceImagePath).ToLower();
                if (extension != ".jpg" && extension != ".jpeg")
                {
                    // Конвертируем в JPG если нужно
                    extension = ".jpg";
                }
                string destinationPath = System.IO.Path.Combine(imagesFolder, $"{barcode}{extension}");
                System.IO.File.Copy(sourceImagePath, destinationPath, true);
                Console.WriteLine($"Изображение сохранено: {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения изображения: {ex.Message}");
                MessageBox.Show($"Не удалось сохранить изображение: {ex.Message}\nПродукт сохранен без изображения.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}