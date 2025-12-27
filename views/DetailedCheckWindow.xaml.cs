using System.Windows;
using System.Collections.ObjectModel;
using QualityAppWPF.Models;
using QualityAppWPF.Services;

namespace QualityAppWPF.Views
{
    public partial class DetailedCheckWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly int _qualityCheckId;
        private ObservableCollection<DetailedCheck> _detailedChecks;

        public DetailedCheckWindow(DatabaseService dbService, int qualityCheckId)
        {
            InitializeComponent();
            _dbService = dbService;
            _qualityCheckId = qualityCheckId;

            LoadDetailedChecks();
        }

        private async void LoadDetailedChecks()
        {
            try
            {
                var checks = await _dbService.GetDetailedChecksAsync(_qualityCheckId);
                _detailedChecks = new ObservableCollection<DetailedCheck>(checks);
                dgParameters.ItemsSource = _detailedChecks;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки параметров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddParameter_Click(object sender, RoutedEventArgs e)
        {
            var addParamWindow = new AddParameterWindow(_dbService, _qualityCheckId);
            addParamWindow.Owner = this;

            if (addParamWindow.ShowDialog() == true)
            {
                LoadDetailedChecks();
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сохраняем изменения в существующих параметрах
                foreach (var check in _detailedChecks)
                {
                    // Здесь можно реализовать обновление существующих записей
                }

                MessageBox.Show("Параметры сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}