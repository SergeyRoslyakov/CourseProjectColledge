// Views/AddUserWindow.xaml.cs
using QualityAppWPF.Models;
using QualityAppWPF.Services;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace QualityAppWPF.Views
{
    public partial class AddUserWindow : Window
    {
        private readonly DatabaseService _dbService;

        public AddUserWindow(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            cmbRole.SelectedIndex = 0;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Введите логин пользователя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = new User
            {
                Username = txtUsername.Text,
                PasswordHash = HashPassword(txtPassword.Password),
                FullName = txtFullName.Text,
                Role = (cmbRole.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "User",
                IsActive = chkIsActive.IsChecked ?? true
            };

            try
            {
                bool success = await _dbService.AddUserAsync(user);

                if (success)
                {
                    MessageBox.Show("Пользователь успешно добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить пользователя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}