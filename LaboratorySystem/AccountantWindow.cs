using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace LaboratorySystem
{
    public partial class AccountantWindow : Window
    {
        private int userId;
        private string userLogin;
        private string userName;
        private string userSurName;

        public AccountantWindow(int userId, string login, string name, string surname)
        {
            InitializeComponent();
            img.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/images/buhgalter.jpeg"));
            this.userId = userId;
            this.userLogin = login;
            this.userName = name;
            this.userSurName = surname;
            tbUserName.Text = name + " " + surname;
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("В данный момент функция в разработке");
        }

        private void BtnCreateInvoice_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("В данный момент функция в разработке");
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AddToHistory(userId);
            new MainWindow().Show();
            Close();
        }

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
                        //ip = GetLocalIPAddress(),
                        id_Reason = 3
                    };

                    db.HistoryIn.Add(history);
                    db.SaveChanges();
                }
            }
            catch { }
        }

        //private string GetLocalIPAddress()
        //{
        //    string ip;
        //    if (Dns.GetHostAddresses(Dns.GetHostName()).Length > 0)
        //    {
        //        ip = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
        //    }
        //    else ip = "10.11.93.111";
        //    return ip;
        //}
    }
}
