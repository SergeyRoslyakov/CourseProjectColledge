using System;
using System.Windows;
using System.Linq;
using QualityAppWPF.Models;
using QualityAppWPF.Services;

namespace QualityAppWPF.Views
{
    public partial class AddParameterWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly int _qualityCheckId;

        public AddParameterWindow(DatabaseService dbService, int qualityCheckId)
        {
            InitializeComponent();
            _dbService = dbService;
            _qualityCheckId = qualityCheckId;

            LoadParameters();
        }

        private async void LoadParameters()
        {
            try
            {
                var parameters = await _dbService.GetAllQualityParametersAsync();
                cmbParameters.ItemsSource = parameters;

                if (parameters.Any())
                {
                    cmbParameters.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки параметров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbParameters.SelectedItem == null)
            {
                MessageBox.Show("Выберите параметр", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtValue.Text, out decimal value))
            {
                MessageBox.Show("Введите корректное числовое значение", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var parameter = cmbParameters.SelectedItem as QualityParameter;
            var detailedCheck = new DetailedCheck
            {
                QualityCheckId = _qualityCheckId,
                ParameterId = parameter.Id,
                Value = value,
                IsPassed = chkIsPassed.IsChecked ?? true,
                Notes = txtNotes.Text
            };

            try
            {
                bool success = await _dbService.AddDetailedCheckAsync(detailedCheck);

                if (success)
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить параметр", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}