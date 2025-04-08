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
using Mail_LIB;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace LaboratorySystem
{

    public partial class RegWindow : Window
    {
        Entities entities = new Entities();
        bool Check = false;
        public RegWindow()
        {
            InitializeComponent();
            TypePolis.ItemsSource = entities.TypesPolises.ToList();
            txtInsurance.ItemsSource = entities.Insurance.ToList();
        }
        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Visibility = Visibility.Visible;
        }

        private void CheckPol()
        {

            Validator validator = new Validator();
            Check = false;

            var check = validator.CheckLogin(txtLogin.Text);
            if (check.isValid == false)
            {
                MessageBox.Show(check.message);

                return;
            }
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;
            check = validator.CheckPassword(password);
            if (!check.isValid)
            {
                MessageBox.Show(check.message);
               
                return;
            }
            check = validator.CheckMail(txtEmail.Text);
            if (!check.isValid)
            {
                MessageBox.Show(check.message);
               
                return;
            }
            if (!Regex.IsMatch(txtPhoneNumber.Text, @"^7\d{10}$"))
            {
                MessageBox.Show("Телефон не подходит");
                
                return;
            }
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Имя не может быть пустым.");
               
                return;
            }
            if (string.IsNullOrWhiteSpace(txtSurName.Text))
            {
                MessageBox.Show("Фамилия не может быть пустой.");
               
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPolis.Text) || txtPolis.Text.Length != 16)
            {
                MessageBox.Show("Страховой полис должен содержать ровно 16 символов.");
                
                return;
            }
            if (txtInsurance.SelectedItem == null)
            {
                MessageBox.Show("Страховая компания не может быть пустой.");
               
                return;
            }
            if (TypePolis.SelectedItem == null || !Regex.IsMatch(txtPolis.Text, @"^\d{16}$"))
            {
                MessageBox.Show("Тип полиса не может быть пустой.");
                
                return;
            }
            if (txtBirthday.SelectedDate > DateTime.Now || txtBirthday.SelectedDate == null)
            {
                MessageBox.Show("Дата рождения не может быть пустой.");
               
                return;
            }
            if (string.IsNullOrEmpty(txtPasport.Text) || txtPasport.Text.Length != 10)
            {
                MessageBox.Show("Паспорт должен содержать ровно 10 символов.");
               
                return;
            }
            Check = true;
        }

        private void btnRegistr_Click(object sender, RoutedEventArgs e)
        {
            CheckPol();
            if (Check)
            {
                AddUsers();
            }
            else
            {
                MessageBox.Show("Ошибка при регистрации. Проверьте правильность ввода данных.");
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainwindow = new MainWindow();
            mainwindow.Show();
            this.Close();
        }

        private void AddUsers()
        {
            ///<summary>
            ///хэширование пароля
            ///</summary>
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
            Users users = new Users
            {
                id_role = 1,
                login = txtLogin.Text,
                password = pass,
                email = txtEmail.Text,
                phone_number = Convert.ToDouble(txtPhoneNumber.Text),
                name = txtName.Text,
                surname = txtSurName.Text,
                birthday = txtBirthday.SelectedDate,
                id_type = TypePolis.SelectedIndex + 1,
                number_polis = txtPolis.Text,
                pasport = Convert.ToDouble(txtPasport.Text),
                id_insurance = txtInsurance.SelectedIndex + 1
            };
            entities.Users.Add(users);
            entities.SaveChanges();
            MessageBox.Show("Пользователь зарегистрирован");
            }
            catch
            {
                MessageBox.Show("Данные заполнены неверно!");
            }
           
        }


        private void TypePolis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
