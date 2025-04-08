using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Linq;
using System.Net;

namespace LaboratorySystem
{
    public partial class UserWindow : Window
    {
        private int userId;


        public UserWindow(string name, string surname)
        {
            InitializeComponent();

            img.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/images/user.png"));

            tbUserName.Text = name + " " + surname;
        }

        public UserWindow(int userId)
        {
            this.userId = userId;
        }

        

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            AddToHistory(userId);
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
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
                        ip = GetLocalIPAddress(),
                        id_Reason = 3
                    };

                    db.HistoryIn.Add(history);
                    db.SaveChanges();
                }
            }
            catch { }
        }

        private string GetLocalIPAddress()
        {
            string ip;
            if (Dns.GetHostAddresses(Dns.GetHostName()).Length > 0)
            {
                ip = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
            }
            else ip = "10.11.93.111";
            return ip;
        }

        protected void ShowStatusMessage(string message)
        {
        }
    }
}
