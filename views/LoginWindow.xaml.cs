using QualityAppWPF.Models;
using QualityAppWPF.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QualityAppWPF.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseService _dbService;
        public User CurrentUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            Loaded += LoginWindow_Loaded;
        }

        private async void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Инициализируем БД
                bool initialized = await _dbService.InitializeDatabaseAsync();

                if (initialized)
                {
                    // Создаем тестовых пользователей, если их нет в БД
                    await CreateDefaultUsersAsync();
                }
                else
                {
                    MessageBox.Show("Ошибка подключения к базе данных", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CreateDefaultUsersAsync()
        {
            try
            {
                var users = await _dbService.GetAllUsersAsync();

                // Проверяем, есть ли пользователь 'user'
                if (!users.Any(u => u.Username == "user"))
                {
                    await _dbService.AddUserAsync(new User
                    {
                        Username = "user",
                        PasswordHash = HashPassword("user123"),
                        FullName = "Оператор проверки",
                        Role = "User",
                        IsActive = true
                    });
                }

                if (!users.Any(u => u.Username == "admin"))
                {
                    await _dbService.AddUserAsync(new User
                    {
                        Username = "admin",
                        PasswordHash = HashPassword("admin123"),
                        FullName = "Администратор системы",
                        Role = "Admin",
                        IsActive = true
                    });
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Ошибка создания пользователей: {ex.Message}");
            }
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

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите имя пользователя и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Хэшируем пароль для сравнения с БД
                string passwordHash = HashPassword(password);

                // Пробуем аутентифицировать пользователя
                var user = await _dbService.AuthenticateUserAsync(username, passwordHash);

                if (user != null && user.IsActive)
                {
                    CurrentUser = user;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль", "Ошибка авторизации",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}