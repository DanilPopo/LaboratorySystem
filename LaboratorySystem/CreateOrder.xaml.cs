using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LaboratorySystem
{
    /// <summary>
    /// Окно для создания новых заказов в лабораторной системе
    /// </summary>
    public partial class CreateOrder : Window
    {
        // Коллекция доступных услуг
        public ObservableCollection<Service> Services { get; set; }

        // Контекст базы данных
        public static Entities entities = new Entities();

        // Список пациентов (пользователей с ролью 1)
        List<Users> listUser = entities.Users.Where(x => x.id_role == 1).ToList();

        /// <summary>
        /// Конструктор окна создания заказа
        /// </summary>
        public CreateOrder()
        {
            InitializeComponent();

            // Настройка элементов управления
            cmbPatient.ItemsSource = listUser; // Заполнение списка пациентов
            var Services = new Entities().Services.ToList(); // Получение списка услуг

            // Преобразование услуг для отображения в DataGrid
            List<Service1> list1 = new List<Service1>();
            for (int i = 0; i < Services.Count(); i++)
            {
                list1.Add(new Service1(
                    Services[i].id,
                    Services[i].service,
                    Services[i].price,
                    Services[i].lead_time,
                    Services[i].deviation_from,
                    Services[i].deviation_to,
                    false));
            }
            dbServe.ItemsSource = list1; // Установка источника данных для DataGrid
        }

        // Обработчик изменения выбора в таблице услуг
        private void dbServe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // В текущей реализации не выполняет действий
        }

        /// <summary>
        /// Обработчик создания нового заказа
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entities = new Entities();

                // Получение ID выбранного пациента
                var idUser = listUser.FirstOrDefault(x => x.login == cmbPatient.Text).id;

                // Создание нового заказа
                Order order = new Order()
                {
                    id_user = idUser,
                    id_status = 1,
                    date_create = DateTime.Now
                };

                // Сохранение заказа в БД
                entities.Order.Add(order);
                entities.SaveChanges();
                MessageBox.Show("Заказ создан");
                dbServe.IsEnabled = true;
            }
            catch
            {
                MessageBox.Show("Что-то пошло не так");
            }
        }

        /// <summary>
        /// Обработчик добавления услуги в заказ (при установке флажка)
        /// </summary>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var entities = new Entities();
            CheckBox chkecking = (CheckBox)(sender);
            var uslu = chkecking.DataContext as Service1;

            // Получение ID последнего созданного заказа
            int poslz = entities.Order.ToList().Last().id;

            // Проверка, не добавлена ли услуга уже
            var nedyzh = entities.ServicesForOrder.FirstOrDefault(x => x.id_order == poslz && x.id_service == uslu.id);

            if (nedyzh == null)
            {
                // Добавление новой услуги в заказ
                ServicesForOrder sfo = new ServicesForOrder();
                sfo.id_order = poslz;
                sfo.id_service = uslu.id;
                sfo.id_status = 1; // Статус "Активен"

                entities.ServicesForOrder.Add(sfo);
                entities.SaveChanges();
            }
            else
            {
                MessageBox.Show("Данная услуга уже есть в заказе");
            }
        }

        /// <summary>
        /// Обработчик удаления услуги из заказа (при снятии флажка)
        /// </summary>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkecking = (CheckBox)(sender);
            var uslu = chkecking.DataContext as Service1;

            // Получение ID последнего заказа
            int poslz = entities.Order.ToList().Last().id;

            // Удаление услуги из заказа
            var nedyzh = entities.ServicesForOrder.FirstOrDefault(x => x.id_order == poslz && x.id_service == uslu.id);
            entities.ServicesForOrder.Remove(nedyzh);
            entities.SaveChanges();
        }

        // Неиспользуемый обработчик (заглушка)
        private void TextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Обработчик открытия окна регистрации нового пациента
        /// </summary>
        private void CreatePacient_Click(object sender, RoutedEventArgs e)
        {
            RegWindow createPacient = new RegWindow();
            createPacient.Show();
        }
    }
}