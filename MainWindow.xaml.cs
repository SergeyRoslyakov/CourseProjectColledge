// MainWindow.xaml.cs - УПРОЩЕННАЯ ВЕРСИЯ
using System;
using System.Windows;
using System.Windows.Media;
using QualityAppWPF.Services;
using QualityAppWPF.Views;
using QualityAppWPF.Models;
using System.ComponentModel;
using System.Threading.Tasks;

namespace QualityAppWPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private string _dbStatusText = "Подключение к БД...";
        private string _dbStatusIcon = "🔌";
        private Brush _dbStatusColor = Brushes.Gray;

        public event PropertyChangedEventHandler PropertyChanged;

        public string DbStatusText
        {
            get => _dbStatusText;
            set { _dbStatusText = value; OnPropertyChanged(); }
        }

        public string DbStatusIcon
        {
            get => _dbStatusIcon;
            set { _dbStatusIcon = value; OnPropertyChanged(); }
        }

        public Brush DbStatusColor
        {
            get => _dbStatusColor;
            set { _dbStatusColor = value; OnPropertyChanged(); }
        }

        public User CurrentUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _dbService = new DatabaseService();

            // Сразу показываем окно
            this.Show();

            // Открываем окно авторизации
            ShowLoginWindow();
        }

        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Owner = this; 
            if (loginWindow.ShowDialog() == true && loginWindow.CurrentUser != null)
            {
                CurrentUser = loginWindow.CurrentUser;
                UpdateUserInfo();
                InitializeDatabase();
                LoadStatistics();
            }
            else
            {
                this.Close();
            }
        }

        private void UpdateUserInfo()
        {
            if (CurrentUser != null)
            {
                txtUserInfo.Text = $"Добро пожаловать, {CurrentUser.FullName}!";
                txtUserRole.Text = CurrentUser.Role == "Admin" ? "Администратор" : "Пользователь";
                Title = $"Quality Check App - {CurrentUser.FullName} ({CurrentUser.Role})";

                // Показываем/скрываем кнопку админа
                btnAdmin.Visibility = CurrentUser.Role == "Admin" ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async void InitializeDatabase()
        {
            try
            {
                DbStatusText = "Подключение к PostgreSQL...";
                DbStatusIcon = "🔌";
                DbStatusColor = Brushes.Orange;

                bool success = await _dbService.InitializeDatabaseAsync();

                if (success)
                {
                    DbStatusText = "✅ Подключено к PostgreSQL";
                    DbStatusIcon = "✅";
                    DbStatusColor = Brushes.LightGreen;
                }
                else
                {
                    DbStatusText = "❌ Ошибка подключения к БД";
                    DbStatusIcon = "❌";
                    DbStatusColor = Brushes.LightCoral;
                }
            }
            catch (Exception ex)
            {
                DbStatusText = $"❌ Ошибка: {ex.Message}";
                DbStatusIcon = "❌";
                DbStatusColor = Brushes.LightCoral;
            }
        }

        private async void LoadStatistics()
        {
            try
            {
                var stats = await _dbService.GetStatisticsAsync();
                txtTotalProducts.Text = stats.TotalProducts.ToString();
                txtChecksToday.Text = stats.ChecksToday.ToString();
                txtAvgRating.Text = stats.AverageRating.ToString("0.0");
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            var scanWindow = new ScanWindow(_dbService);
            scanWindow.Owner = this;
            scanWindow.ShowDialog();
            LoadStatistics();
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_dbService);
            historyWindow.Owner = this;
            historyWindow.ShowDialog();
        }

        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            var scanWindow = new ScanWindow(_dbService);
            scanWindow.Owner = this;
            scanWindow.ShowDialog();
            LoadStatistics();
        }

        private void BtnStats_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_dbService);
            historyWindow.Owner = this;
            historyWindow.ShowDialog();
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser?.Role == "Admin")
            {
                var adminPanel = new AdminPanelWindow(_dbService, CurrentUser);
                adminPanel.Owner = this;
                adminPanel.ShowDialog();

                LoadStatistics();
            }
            else
            {
                MessageBox.Show("Доступ запрещен. Требуются права администратора.", "Ошибка доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти из системы?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ShowLoginWindow();
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }
        private void BtnProductsList_Click(object sender, RoutedEventArgs e)
        {
            var productsWindow = new ProductsListWindow(_dbService, CurrentUser);
            productsWindow.Owner = this;
            productsWindow.ShowDialog();
        }
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}