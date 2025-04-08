using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace LaboratorySystem
{
    /// <summary>
    /// Вспомогательный класс для представления услуги
    /// </summary>
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Окно для редактирования услуг в лабораторной системе
    /// </summary>
    public partial class EditServices : Window
    {
        /// <summary>
        /// Конструктор окна редактирования услуг
        /// </summary>
        public EditServices()
        {
            InitializeComponent();

            // Загрузка списка услуг из базы данных
            var Services = new Entities().Services.ToList();

            // Создание списка для отображения в DataGrid
            List<Service1> list1 = new List<Service1>();
            for (int i = 0; i < Services.Count(); i++)
            {
                // Создание объекта Service1 для каждой услуги
                list1.Add(new Service1(
                    Services[i].id,
                    Services[i].service,
                    Services[i].price,
                    Services[i].lead_time,
                    Services[i].deviation_from,
                    Services[i].deviation_to,
                    false));

                // Проверка, выбрана ли услуга в текущем заказе
                for (int j = 0; j < LabAssistantWindow.list.Count(); j++)
                {
                    if (LabAssistantWindow.list[j].id_service == Services[i].id)
                    {
                        list1[i].tr = true; // Помечаем как выбранную
                    }
                }
            }

            // Установка источника данных для DataGrid
            dbServe.ItemsSource = list1;
        }


        /// <summary>
        /// Обработчик изменения выбора в DataGrid
        /// </summary>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // В текущей реализации не выполняет действий
        }


        /// <summary>
        /// Обработчик установки флажка (добавление услуги в заказ)
        /// </summary>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var entities = new Entities();
            CheckBox chk = (CheckBox)(sender);
            var lo = chk.DataContext as Service1;

            // Получение ID текущего заказа
            int qwe = LabAssistantWindow.list[0].id_order;

            // Проверка, не добавлена ли услуга уже
            var jj = entities.ServicesForOrder.FirstOrDefault(x => x.id_order == qwe && x.id_service == lo.id);

            if (jj == null)
            {
                // Создание новой связи услуги с заказом
                ServicesForOrder sfo = new ServicesForOrder();
                sfo.id_order = LabAssistantWindow.list[0].id_order;
                sfo.id_service = lo.id;
                sfo.id_status = 1; // Статус "Активен"

                // Сохранение в базу данных
                entities.ServicesForOrder.Add(sfo);
                entities.SaveChanges();
            }
        }

        /// <summary>
        /// Обработчик снятия флажка (удаление услуги из заказа)
        /// </summary>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var entities = new Entities();
            CheckBox chk = (CheckBox)(sender);
            var lo = chk.DataContext as Service1;

            // Получение ID текущего заказа
            int qwe = LabAssistantWindow.list[0].id_order;

            // Поиск и удаление связи услуги с заказом
            var jj = entities.ServicesForOrder.FirstOrDefault(x => x.id_order == qwe && x.id_service == lo.id);
            entities.ServicesForOrder.Remove(jj);
            entities.SaveChanges();
        }

    }
}