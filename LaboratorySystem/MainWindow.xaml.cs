using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Mail_LIB;

namespace LaboratorySystem
{
    /// <summary>
    /// Главное окно авторизации лабораторной системы
    /// </summary>
    public partial class MainWindow : Window
    {
        // Поля для работы с CAPTCHA и блокировкой
        private string captchaText;
        private int failedAttempts = 0;
        private DispatcherTimer blockTimer;
        private DateTime? blockEndTime;
        private BlockWindow blockWindow;
        private Validator validator = new Validator();

        /// <summary>
        /// Конструктор главного окна
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            BlockTimer(); // Инициализация таймера блокировки
        }

        // Блок работы с отображением пароля

        /// <summary>
        /// Показ пароля в открытом виде
        /// </summary>
        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Скрытие пароля
        /// </summary>
        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обработчик кнопки входа
        /// </summary>
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Проверка блокировки системы
            if (blockEndTime.HasValue && DateTime.Now < blockEndTime.Value)
            {
                MessageBox.Show($"Система заблокирована. Попробуйте через {(blockEndTime.Value - DateTime.Now).Seconds} сек");
                return;
            }

            string login = txtLogin.Text.Trim();
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;

            // Валидация логина
            var loginVal = validator.CheckLogin(login);
            if (!loginVal.isValid)
            {
                FailedAttempt(loginVal.message);
                return;
            }

            // Проверка заполнения пароля
            if (string.IsNullOrEmpty(password))
            {
                FailedAttempt("Введите пароль");
                return;
            }

            // Проверка CAPTCHA
            if (captchaPanel.Visibility == Visibility.Visible &&
                (string.IsNullOrEmpty(txtCaptcha.Text) || txtCaptcha.Text != captchaText))
            {
                FailedAttempt("Неверная CAPTCHA");
                return;
            }

            AttemptLogin(login);
        }

        /// <summary>
        /// Попытка авторизации пользователя
        /// </summary>
        private void AttemptLogin(string login)
        {
            try
            {
                using (var db = new Entities())
                {
                    var userl = db.Users.FirstOrDefault(x => x.login == login);
                    bool isCorrect = VerifyPassword(userl?.password);

                    var userChecked = db.Users.FirstOrDefault(u => u.login == login && isCorrect == true);

                    if (userChecked != null)
                    {
                        RegAttempt(userChecked.id, true);
                        SuccessfulLogin(userChecked);
                    }
                    else
                    {
                        HandleFailedLogin(login);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Что-то пошло не так");
            }
        }

        /// <summary>
        /// Проверка совпадения паролей
        /// </summary>
        private bool VerifyPassword(string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword)) return false;

            if (hashedPassword.Length > 20)
            {
                // Проверка хешированного пароля
                byte[] buffer4;
                byte[] src = Convert.FromBase64String(hashedPassword);
                byte[] dst = new byte[0x10];
                Buffer.BlockCopy(src, 1, dst, 0, 0x10);
                byte[] buffer3 = new byte[0x20];
                Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);

                using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(txtPassword.Password, dst, 0x3e8))
                {
                    buffer4 = bytes.GetBytes(0x20);
                }
                return Convert.ToBase64String(buffer4) == Convert.ToBase64String(buffer3);
            }

            return hashedPassword == txtPassword.Password || hashedPassword == txtPasswordVisible.Text;
        }

        /// <summary>
        /// Обработка неудачной попытки входа
        /// </summary>
        private void HandleFailedLogin(string login)
        {
            using (var db = new Entities())
            {
                var userLog = db.Users.FirstOrDefault(u => u.login == login);
                if (userLog != null) RegAttempt(userLog.id, false);
            }
            FailedAttempt("Неверный логин или пароль");
        }

        /// <summary>
        /// Неудачная попытка входа
        /// </summary>
        private void FailedAttempt(string message)
        {
            failedAttempts++;
            MessageBox.Show(message);

            // Активация CAPTCHA после 1 ошибки
            if (failedAttempts == 1)
            {
                captchaPanel.Visibility = Visibility.Visible;
                GenerateCaptcha();
            }
            // Блокировка после 2 ошибок
            else if (failedAttempts >= 2)
            {
                StartBlocking(TimeSpan.FromSeconds(10));
            }
        }

        /// <summary>
        /// Успешная авторизация
        /// </summary>
        private void SuccessfulLogin(Users user)
        {
            failedAttempts = 0;
            captchaPanel.Visibility = Visibility.Collapsed;
            OpenWindow(user.id, user.login, user.Role.name, user.name, user.surname);
        }

        // Блок работы с историей входов

        /// <summary>
        /// Запись попытки входа в базу данных
        /// </summary>
        private void RegAttempt(int userId, bool isSuccess)
        {
            try
            {
                using (var db = new Entities())
                {
                    // Проверка блокировки системы
                    var reasOut = db.HistoryIn.Where(u => u.id_user == userId).ToList();
                    TimeSpan time = new TimeSpan(0, 30, 0);
                    if (reasOut.Last().id_Reason == 1 && reasOut.Last().lastenter + time > DateTime.Now)
                    {
                        MessageBox.Show("Идет кварцевание помещения");
                        Application.Current.Shutdown();
                    }

                    // Запись в историю
                    var history = new HistoryIn
                    {
                        id_user = userId,
                        ip = GetLocalIPAddress(),
                        lastenter = DateTime.Now,
                        id_Reason = isSuccess ? 4 : 2
                    };

                    db.HistoryIn.Add(history);
                    db.SaveChanges();
                }
            }
            catch { }
        }


        /// <summary>
        /// Генерация новой CAPTCHA
        /// </summary>
        private void GenerateCaptcha()
        {
            captchaCanvas.Children.Clear();
            var random = new Random();

            // Генерация случайного текста
            captchaText = new string(Enumerable.Repeat("abcdefghkjlmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789", 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Добавление текста
            for (int i = 0; i < captchaText.Length; i++)
            {
                var textBlock = new TextBlock
                {
                    Text = captchaText[i].ToString(),
                    FontSize = random.Next(20, 26),
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(
                        (byte)random.Next(50, 200),
                        (byte)random.Next(50, 200),
                        (byte)random.Next(50, 200)))
                };

                Canvas.SetLeft(textBlock, i * 30 + random.Next(-8, 8));
                Canvas.SetTop(textBlock, random.Next(5, 15));
                captchaCanvas.Children.Add(textBlock);
            }

            // Добавление линий
            for (int i = 0; i < 5; i++)
            {
                captchaCanvas.Children.Add(new Line
                {
                    X1 = random.Next(0, 100),
                    Y1 = random.Next(0, 40),
                    X2 = random.Next(100, 200),
                    Y2 = random.Next(0, 40),
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5,
                    Opacity = 0.7
                });
            }

            // Добавление шума
            for (int i = 0; i < 30; i++)
            {
                captchaCanvas.Children.Add(new Ellipse
                {
                    Width = random.Next(1, 3),
                    Height = random.Next(1, 3),
                    Fill = Brushes.Gray,
                    Opacity = 0.5
                });
                Canvas.SetLeft(captchaCanvas.Children[captchaCanvas.Children.Count - 1], random.Next(0, 200));
                Canvas.SetTop(captchaCanvas.Children[captchaCanvas.Children.Count - 1], random.Next(0, 40));
            }
        }

        // Блок управления блокировкой

        /// <summary>
        /// Инициализация таймера блокировки
        /// </summary>
        private void BlockTimer()
        {
            blockTimer = new DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(1);
            blockTimer.Tick += BlockTimer_Tick;
        }

        /// <summary>
        /// Обработчик тика таймера блокировки
        /// </summary>
        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            if (blockEndTime.HasValue && DateTime.Now >= blockEndTime.Value)
            {
                EndBlocking();
            }
        }

        /// <summary>
        /// Запуск блокировки системы
        /// </summary>
        private void StartBlocking(TimeSpan duration)
        {
            blockEndTime = DateTime.Now.Add(duration);
            blockTimer.Start();
            btnLogin.IsEnabled = false;
            blockWindow = new BlockWindow(duration);
            blockWindow.Show();
        }

        /// <summary>
        /// Снятие блокировки
        /// </summary>
        private void EndBlocking()
        {
            blockTimer.Stop();
            blockEndTime = null;
            btnLogin.IsEnabled = true;
            blockWindow?.Close();
        }

        // Вспомогательные методы

        /// <summary>
        /// Получение локального IP-адреса
        /// </summary>
        private string GetLocalIPAddress()
        {
            try
            {
                return Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
            }
            catch
            {
                return "10.11.93.111";
            }
        }

        private void OpenWindow(int userId, string login, string roleName, string name, string surname)
        {
            Window userWindow;

            switch (roleName)
            {
                case "Лаборант":
                    userWindow = new LabAssistantWindow(userId, name, surname);
                    break;
                case "Лаборант-исследователь":
                    userWindow = new ResearchAssistantWindow(userId, login, name, surname);
                    break;
                case "Бухгалтер":
                    userWindow = new AccountantWindow(userId, login, name, surname);
                    break;
                case "Администратор":
                    userWindow = new AdminWindow(userId, login, name, surname);
                    break;
                case "Пациент":
                    userWindow = new UserWindow(name, surname);
                    break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя");
                    return;
            }
            userWindow.Show();
            this.Close();
        }

        //Кнопка РЕГИСТРАЦИЯ
        private void btnRegistr_Click(object sender, RoutedEventArgs e)
        {
            RegWindow regWindow = new RegWindow();
            regWindow.Show();
            this.Close();
        }


        /// <summary>
        /// Обновление CAPTCHA
        /// </summary>
        private void btnRefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }
    }
}