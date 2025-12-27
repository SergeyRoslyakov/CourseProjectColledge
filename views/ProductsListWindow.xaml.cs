using QualityAppWPF.Models;
using QualityAppWPF.Services;
using QualityAppWPF.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QualityAppWPF.Views
{
    public partial class ProductsListWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly User _currentUser;
        private ObservableCollection<Product> _allProducts;

        public ProductsListWindow(DatabaseService dbService, User currentUser)
        {
            InitializeComponent();
            _dbService = dbService;
            _currentUser = currentUser;

            LoadProducts();
        }

        private async void LoadProducts()
        {
            try
            {
                var products = await _dbService.GetAllProductsAsync();
                _allProducts = new ObservableCollection<Product>(products);
                lbProducts.ItemsSource = _allProducts;

                // Показываем сообщение о выборе продукта
                txtNoSelection.Visibility = Visibility.Visible;
                scrollProductInfo.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allProducts == null) return;

            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                lbProducts.ItemsSource = _allProducts;
            }
            else
            {
                var filtered = _allProducts.Where(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    p.Barcode.ToLower().Contains(searchText) ||
                    (p.Category != null && p.Category.ToLower().Contains(searchText)) ||
                    (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(searchText))
                ).ToList();

                lbProducts.ItemsSource = filtered;
            }
        }

        private void LbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProduct = lbProducts.SelectedItem as Product;
            if (selectedProduct != null)
            {
                ShowProductInfo(selectedProduct);
            }
        }

        private void ShowProductInfo(Product product)
        {
            // Показываем панель с информацией
            scrollProductInfo.Visibility = Visibility.Visible;
            txtNoSelection.Visibility = Visibility.Collapsed;

            // Заполняем базовую информацию
            txtProductName.Text = product.Name;
            txtBarcode.Text = product.Barcode;
            txtCategory.Text = product.Category ?? "Не указана";
            txtManufacturer.Text = product.Manufacturer ?? "Не указан";
            txtCreatedAt.Text = product.CreatedAt.ToString("dd.MM.yyyy");

            // Загружаем картинку
            LoadProductImage(product.Barcode);

            // Заполняем БЖУ
            FillNutritionInfo(product);
        }

        private void LoadProductImage(string barcode)
        {
            try
            {
                // Путь к картинке по штрих-коду
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string imagesFolder = Path.Combine(appPath, "Images", "Products");

                // Проверяем различные расширения
                string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };
                string imagePath = null;

                foreach (var ext in extensions)
                {
                    string testPath = Path.Combine(imagesFolder, $"{barcode}{ext}");
                    if (File.Exists(testPath))
                    {
                        imagePath = testPath;
                        break;
                    }
                }

                if (imagePath != null && File.Exists(imagePath))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgProduct.Source = bitmap;
                }
                else
                {
                    // Используем заглушку или скрываем
                    imgProduct.Source = null;
                    imgProduct.Opacity = 0.3;

                    // Можно показать текст вместо картинки
                    var visual = new System.Windows.Media.DrawingVisual();
                    using (var dc = visual.RenderOpen())
                    {
                        dc.DrawText(
                            new System.Windows.Media.FormattedText(
                                "Нет\nизображения",
                                System.Globalization.CultureInfo.CurrentCulture,
                                System.Windows.FlowDirection.LeftToRight,
                                new System.Windows.Media.Typeface("Arial"),
                                16,
                                System.Windows.Media.Brushes.Gray,
                                VisualTreeHelper.GetDpi(this).PixelsPerDip),
                            new System.Windows.Point(50, 120));
                    }

                    var rtb = new System.Windows.Media.Imaging.RenderTargetBitmap(
                        200, 200, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    rtb.Render(visual);
                    imgProduct.Source = rtb;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                imgProduct.Source = null;
            }
        }

        private void FillNutritionInfo(Product product)
        {
            if (product.Protein.HasValue)
            {
                txtProtein.Text = product.Protein.Value.ToString("0.0");
            }
            else
            {
                txtProtein.Text = "-";
                txtProtein.Foreground = System.Windows.Media.Brushes.Gray;
            }

            if (product.Fat.HasValue)
            {
                txtFat.Text = product.Fat.Value.ToString("0.0");
            }
            else
            {
                txtFat.Text = "-";
                txtFat.Foreground = System.Windows.Media.Brushes.Gray;
            }

            if (product.Carbs.HasValue)
            {
                txtCarbs.Text = product.Carbs.Value.ToString("0.0");
            }
            else
            {
                txtCarbs.Text = "-";
                txtCarbs.Foreground = System.Windows.Media.Brushes.Gray;
            }

            if (product.Calories.HasValue)
            {
                txtCalories.Text = $"Калории: {product.Calories.Value:F0} ккал/100г";
            }
            else if (product.Protein.HasValue && product.Fat.HasValue && product.Carbs.HasValue)
            {
                // Рассчитываем калории если указаны БЖУ
                double calories = product.Protein.Value * 4 + product.Fat.Value * 9 + product.Carbs.Value * 4;
                txtCalories.Text = $"Калории: {calories:F0} ккал/100г (расчет)";
            }
            else
            {
                txtCalories.Text = "Калории: не указаны";
                txtCalories.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void BtnShowChecks_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = lbProducts.SelectedItem as Product;
            if (selectedProduct != null)
            {
                // Открываем окно истории с фильтром по штрих-коду
                var historyWindow = new HistoryWindow(_dbService);
                historyWindow.Owner = this;

                // Можно добавить фильтрацию по продукту в HistoryWindow
                historyWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите продукт из списка", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}