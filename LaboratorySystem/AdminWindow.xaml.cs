using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.Entity;
using System.Net;

namespace LaboratorySystem
{
    /// <summary>
    /// Окно администратора системы, предоставляющее функционал управления пользователями и просмотра истории входов
    /// </summary>
    public partial class AdminWindow : Window
    {
        // Поля для хранения информации о текущем пользователе
        private int userId;
        private string userLogin;
        private string userName;
        private string userSurName;

        /// <summary>
        /// Конструктор окна администратора
        /// </summary>
        public AdminWindow(int userId, string login, string name, string surname)
        {
            InitializeComponent();

            // Установка изображения администратора
            img.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/images/admin.png"));

            // Инициализация данных пользователя
            this.userId = userId;
            this.userLogin = login;
            this.userName = name;
            this.userSurName = surname;

            // Отображение имени пользователя в интерфейсе
            tbUserName.Text = name + " " + surname;

            // Загрузка данных при открытии окна
            LoadUsers();
            LoadLoginHistory();
        }


        /// <summary>
        /// Загрузка списка пользователей в DataGrid
        /// </summary>
        private void LoadUsers()
        {
            try
            {
                using (var db = new Entities())
                {
                    dgUsers.ItemsSource = db.Users.Select(u => new
                    {
                        u.id,
                        u.login,
                        u.name,
                        Role = u.Role,
                        Lastenter = u.HistoryIn.OrderByDescending(h => h.lastenter).FirstOrDefault()
                    }).ToList();
                }
            }
            catch
            {
                MessageBox.Show($"Ошибка загрузки пользователей");
            }
        }

        /// <summary>
        /// Загрузка истории входов в систему с возможностью фильтрации по логину
        /// </summary>
        private void LoadLoginHistory(string filterLogin = null)
        {
            try
            {
                using (var db = new Entities())
                {
                    var query = db.HistoryIn
                        .Include(h => h.Users)
                        .Include(h => h.Reasons)
                        .OrderByDescending(h => h.lastenter)
                        .AsQueryable();

                    // Применение фильтра по логину, если он указан
                    if (!string.IsNullOrWhiteSpace(filterLogin))
                    {
                        query = query.Where(h => h.Users.login.Contains(filterLogin));
                    }

                    dgLoginHistory.ItemsSource = query.ToList();
                }
            }
            catch
            {
                MessageBox.Show($"Ошибка загрузки истории входов");
            }
        }


        //Обработчики событий для кнопок управления пользователями

        /// <summary>
        /// Обновление списка пользователей
        /// </summary>
        private void BtnRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        /// <summary>
        /// Открытие окна создания нового пользователя
        /// </summary>
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            CreatePersonalWindow createPersonalWindow = new CreatePersonalWindow();
            createPersonalWindow.Show();
        }


        //Обработчики событий для работы с фильтрацией истории входов

        /// <summary>
        /// Применение фильтра по логину к истории входов
        /// </summary>
        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadLoginHistory(txtFilterLogin.Text.Trim());
        }

        /// <summary>
        /// Сброс фильтра истории входов
        /// </summary>
        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            txtFilterLogin.Clear();
            LoadLoginHistory();
        }

        //Методы для работы с историей входов и выхода из системы

        /// <summary>
        /// Обработчик выхода из системы
        /// </summary>
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AddToHistory(userId);
            new MainWindow().Show();
            Close();
        }

        /// <summary>
        /// Добавление записи в историю входов при выходе из системы
        /// </summary>
        private void AddToHistory(int userId)
        {
            try
            {
                using (var db = new Entities())
                {
                    var history = new HistoryIn
                    {
                        id_user = userId,
                        lastenter = DateTime.Now,
                        ip = GetLocalIPAddress(),
                        id_Reason = 3 // Причина выхода (3 - выход из системы)
                    };

                    db.HistoryIn.Add(history);
                    db.SaveChanges();
                }
            }
            catch { }
        }

        /// <summary>
        /// Получение локального IP-адреса
        /// </summary>
        private string GetLocalIPAddress()
        {
            string ip;
            if (Dns.GetHostAddresses(Dns.GetHostName()).Length > 0)
            {
                ip = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
            }
            else ip = "10.11.93.111"; // Значение по умолчанию
            return ip;
        }

        private void dgLoginHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработчик изменения выбора в истории входов (не реализован)
        }

        private void dgLoginHistory_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // Дублирующийся обработчик (возможно, требуется удалить)
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработчик изменения выбора в списке пользователей (не реализован)
        }
    }
}