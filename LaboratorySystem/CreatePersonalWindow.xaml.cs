using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Mail_LIB;
using System.Security.Cryptography;

namespace LaboratorySystem
{
    /// <summary>
    /// Окно для создания нового сотрудника в лабораторной системе
    /// </summary>
    public partial class CreatePersonalWindow : Window
    {
        // Контекст базы данных
        Entities entities = new Entities();

        // Флаг успешного создания сотрудника
        bool Check = false;

        /// <summary>
        /// Конструктор окна создания сотрудника
        /// </summary>
        public CreatePersonalWindow()
        {
            InitializeComponent();

            // Заполнение выпадающего списка ролей
            role.ItemsSource = entities.Role.ToList();
        }

        // Блок проверки введенных данных 

        /// <summary>
        /// Проверка корректности введенных данных
        /// </summary>
        private void CheckPol()
        {
            Validator validator = new Validator();

            // Проверка логина
            var check = validator.CheckLogin(txtLogin.Text);
            if (!check.isValid)
            {
                MessageBox.Show(check.message);
                return;
            }

            // Проверка пароля (учитывает видимый/скрытый режим)
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;
            check = validator.CheckPassword(password);
            if (!check.isValid)
            {
                MessageBox.Show(check.message);
                return;
            }

            // Проверка email
            check = validator.CheckMail(txtEmail.Text);
            if (!check.isValid)
            {
                MessageBox.Show(check.message);
                return;
            }

            // Проверка телефона (формат 7XXXXXXXXXX)
            if (!Regex.IsMatch(txtPhoneNumber.Text, @"^7\d{10}$"))
            {
                MessageBox.Show("Телефон не подходит");
                return;
            }

            // Проверка имени
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Имя не может быть пустым.");
                return;
            }

            // Проверка фамилии
            if (string.IsNullOrWhiteSpace(txtSurName.Text))
            {
                MessageBox.Show("Фамилия не может быть пустой.");
                return;
            }

            // Проверка даты рождения
            if (txtBirthday.SelectedDate > DateTime.Now || txtBirthday.SelectedDate == null)
            {
                MessageBox.Show("Дата рождения не может быть пустой.");
                return;
            }

            // Проверка паспорта (ровно 10 символов)
            if (string.IsNullOrEmpty(txtPasport.Text) || txtPasport.Text.Length != 10)
            {
                MessageBox.Show("Паспорт должен содержать ровно 10 символов.");
                return;
            }
        }

        // Блок работы с отображением пароля //

        /// <summary>
        /// Обработчик скрытия пароля
        /// </summary>
        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обработчик показа пароля
        /// </summary>
        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Visibility = Visibility.Visible;
        }

        // Блок создания сотрудника //

        /// <summary>
        /// Обработчик нажатия кнопки создания сотрудника
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CheckPol(); // Проверка данных
            AddUsers(); // Добавление пользователя

            if (Check == true)
                MessageBox.Show("Сотрудник добавлен");

            Close(); // Закрытие окна
        }

        /// <summary>
        /// Метод добавления нового пользователя в систему
        /// </summary>
        private void AddUsers()
        {
            var pass = txtPassword.Password;
            byte[] salt;
            byte[] buffer2;

            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(pass, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }

            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            pass = Convert.ToBase64String(dst);

            try
            {
                // Создание объекта пользователя
                Users users = new Users
                {
                    id_role = 1, // Роль по умолчанию (1)
                    login = txtLogin.Text,
                    password = txtPassword.Password,
                    email = txtEmail.Text,
                    phone_number = Convert.ToDouble(txtPhoneNumber.Text),
                    name = txtName.Text,
                    surname = txtSurName.Text,
                    birthday = txtBirthday.SelectedDate,
                    pasport = Convert.ToDouble(txtPasport.Text),
                };

                // Сохранение в базу данных
                entities.Users.Add(users);
                entities.SaveChanges();
                Check = true; // Установка флага успешного создания
            }
            catch
            {
                MessageBox.Show("Данные заполнены неверно!");
            }
        }
    }
}