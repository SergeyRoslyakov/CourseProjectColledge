using Microsoft.Win32;
using QualityAppWPF.Models;
using QualityAppWPF.Services;
using QualityAppWPF.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QualityAppWPF.Views
{
    public partial class AdminPanelWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly User _adminUser;

        public AdminPanelWindow(DatabaseService dbService, User adminUser)
        {
            InitializeComponent();
            _dbService = dbService;
            _adminUser = adminUser;

            txtAdminInfo.Text = $"Вы вошли как: {adminUser.FullName} ({adminUser.Username})";

            // Загружаем все данные
            LoadAllData();
        }

        private async void LoadAllData()
        {
            try
            {
                // Загружаем пользователей
                var users = await _dbService.GetAllUsersAsync();
                dgUsers.ItemsSource = users;

                // Загружаем продукты
                var products = await _dbService.GetAllProductsAsync();
                dgProducts.ItemsSource = products;

                // Загружаем производителей
                var manufacturers = await _dbService.GetAllManufacturersAsync();
                dgManufacturers.ItemsSource = manufacturers;

                // Загружаем категории
                var categories = await _dbService.GetAllCategoriesAsync();
                dgCategories.ItemsSource = categories;

                // Загружаем параметры
                var parameters = await _dbService.GetAllQualityParametersAsync();
                dgParameters.ItemsSource = parameters;

                // Загружаем статистику
                await LoadStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadStatistics()
        {
            try
            {
                var stats = await _dbService.GetStatisticsAsync();
                var users = await _dbService.GetAllUsersAsync();
                var checks = await _dbService.GetQualityChecksAsync();

                // Обновляем статистику
                txtTotalProducts.Text = stats.TotalProducts.ToString();
                txtChecksToday.Text = stats.ChecksToday.ToString();
                txtAvgRating.Text = stats.AverageRating.ToString("0.0");
                txtActiveUsers.Text = users.Count(u => u.IsActive).ToString();

                if (checks.Count > 0)
                {
                    var lastCheck = checks.OrderByDescending(c => c.CheckDate).First();
                    txtLastActivity.Text = lastCheck.CheckDate.ToString("dd.MM.yyyy HH:mm");
                }
                else
                {
                    txtLastActivity.Text = "Нет данных";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== МЕТОДЫ ДЛЯ ПРОДУКТОВ ==========

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно добавления продукта
            var addProductWindow = new AddProductWindow(_dbService);
            addProductWindow.Owner = this;

            if (addProductWindow.ShowDialog() == true)
            {
                // Перезагружаем продукты
                LoadProducts();
            }
        }

        private async void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = dgProducts.SelectedItem as Product;

            if (selectedProduct == null)
            {
                MessageBox.Show("Выберите продукт для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить продукт '{selectedProduct.Name}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = await _dbService.DeleteProductAsync(selectedProduct.Id);

                    if (success)
                    {
                        MessageBox.Show("Продукт удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadProducts();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить продукт", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void LoadProducts()
        {
            try
            {
                var products = await _dbService.GetAllProductsAsync();
                dgProducts.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== МЕТОДЫ ДЛЯ ПОЛЬЗОВАТЕЛЕЙ ==========

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно добавления пользователя
            var addUserWindow = new AddUserWindow(_dbService);
            addUserWindow.Owner = this;

            if (addUserWindow.ShowDialog() == true)
            {
                // Перезагружаем пользователей
                LoadUsers();
            }
        }

        private async void LoadUsers()
        {
            try
            {
                var users = await _dbService.GetAllUsersAsync();
                dgUsers.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== МЕТОДЫ ДЛЯ ПРОИЗВОДИТЕЛЕЙ ==========

        private void BtnAddManufacturer_Click(object sender, RoutedEventArgs e)
        {
            // Простой диалог для добавления производителя
            var inputDialog = new InputDialogWindow("Добавить производителя",
                "Введите название производителя:");

            if (inputDialog.ShowDialog() == true)
            {
                AddManufacturer(inputDialog.Result);
            }
        }

        private async void AddManufacturer(string name)
        {
            try
            {
                var manufacturer = new Manufacturer
                {
                    Name = name,
                    ContactPerson = ""
                };

                await _dbService.AddManufacturerAsync(manufacturer);
                MessageBox.Show($"Производитель '{name}' добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Перезагружаем производителей
                await LoadManufacturers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadManufacturers()
        {
            try
            {
                var manufacturers = await _dbService.GetAllManufacturersAsync();
                dgManufacturers.ItemsSource = manufacturers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки производителей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== МЕТОДЫ ДЛЯ КАТЕГОРИЙ ==========

        private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new InputDialogWindow("Добавить категорию",
                "Введите название категории:");

            if (inputDialog.ShowDialog() == true)
            {
                AddCategory(inputDialog.Result);
            }
        }

        private async void AddCategory(string name)
        {
            try
            {
                var category = new Category { Name = name };
                await _dbService.AddCategoryAsync(category);

                MessageBox.Show($"Категория '{name}' добавлена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Перезагружаем категории
                await LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCategories()
        {
            try
            {
                var categories = await _dbService.GetAllCategoriesAsync();
                dgCategories.ItemsSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== МЕТОДЫ ДЛЯ ПАРАМЕТРОВ ==========

        private void BtnAddParameter_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new InputDialogWindow("Добавить параметр",
                "Введите название параметра:");

            if (inputDialog.ShowDialog() == true)
            {
                AddParameter(inputDialog.Result);
            }
        }

        private async void AddParameter(string name)
        {
            try
            {
                var parameter = new QualityParameter
                {
                    Name = name,
                    Description = "",
                    Unit = ""
                };

                await _dbService.AddQualityParameterAsync(parameter);

                MessageBox.Show($"Параметр '{name}' добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Перезагружаем параметры
                await LoadParameters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadParameters()
        {
            try
            {
                var parameters = await _dbService.GetAllQualityParametersAsync();
                dgParameters.ItemsSource = parameters;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки параметров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== ОБЩИЕ МЕТОДЫ ==========

        private void BtnRefreshStats_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
            MessageBox.Show("Статистика обновлена", "Обновление",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnRefreshAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAllData();
            MessageBox.Show("Все данные обновлены", "Обновление",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv|Текстовые файлы (*.txt)|*.txt",
                    FileName = $"quality_data_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    ExportDataToCsv(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportDataToCsv(string filePath)
        {
            try
            {
                var products = await _dbService.GetAllProductsAsync();
                var checks = await _dbService.GetQualityChecksAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Тип;Штрих-код;Название;Категория;Производитель;Дата");

                foreach (var product in products)
                {
                    csv.AppendLine($"Продукт;{product.Barcode};{product.Name};{product.Category};{product.Manufacturer};{product.CreatedAt:dd.MM.yyyy}");
                }

                foreach (var check in checks)
                {
                    csv.AppendLine($"Проверка;{check.ProductBarcode};{check.ProductName};;;{check.CheckDate:dd.MM.yyyy HH:mm};Оценка: {check.Rating};Инспектор: {check.Inspector}");
                }

                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

                MessageBox.Show($"Данные экспортированы в файл: {filePath}", "Экспорт завершен",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}