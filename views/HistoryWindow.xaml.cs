using System.Windows;
using QualityAppWPF.Services;
using System.Collections.Generic;
using QualityAppWPF.Models;

namespace QualityAppWPF.Views
{
    public partial class HistoryWindow : Window
    {
        private readonly DatabaseService _dbService;

        public HistoryWindow(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            LoadChecks();
        }

        private async void LoadChecks(string barcode = null)
        {
            try
            {
                List<QualityCheck> checks;

                if (string.IsNullOrEmpty(barcode))
                {
                    checks = await _dbService.GetQualityChecksAsync();
                }
                else
                {
                    checks = await _dbService.GetQualityChecksAsync(barcode);
                }

                dgChecks.ItemsSource = checks;

                // Статус
                Title = $"История проверок ({checks.Count} записей)";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadChecks(txtFilterBarcode.Text);
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            txtFilterBarcode.Text = "";
            LoadChecks();
        }
        public HistoryWindow(DatabaseService dbService, string barcodeFilter = null)
        {
            InitializeComponent();
            _dbService = dbService;

            // Если передан штрих-код, фильтруем по нему
            if (!string.IsNullOrEmpty(barcodeFilter))
            {
                txtFilterBarcode.Text = barcodeFilter;
                LoadChecks(barcodeFilter);
            }
            else
            {
                LoadChecks();
            }
        }
    }
}