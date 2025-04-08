using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Threading;
using System.Text.Json;
using System.Web.Script.Serialization;

namespace LaboratorySystem
{
    public partial class ResearchAssistantWindow : Window
    {
       
        private DispatcherTimer sessionTimer;
        private DispatcherTimer timerAnalys;
        private TimeSpan sessionTime = TimeSpan.FromMinutes(150);
        private TimeSpan AnalysTime = TimeSpan.FromSeconds(30);
        private int userId;



        public ResearchAssistantWindow(int userId, string login, string name, string surname)
        {
            InitializeComponent();
            var entities = new Entities();
            img.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/images/laborantH.png"));
            this.userId = userId;
            dgServe.ItemsSource = entities.ServicesForOrder.Where(x => x.id_status != 3).ToList();
            tbUserName.Text = name + " " + surname;
            SessionTimer();
            cmbAnalyser.ItemsSource = entities.Analyser.ToList();
        }

        private void SessionTimer()
        {
            sessionTimer = new DispatcherTimer();
            sessionTimer.Interval = TimeSpan.FromSeconds(1);
            sessionTimer.Tick += SessionTimer_Tick;
            sessionTimer.Start();
        }

        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            sessionTime = sessionTime.Subtract(TimeSpan.FromSeconds(1));
            tbTimer.Text = sessionTime.ToString(@"hh\:mm\:ss");

            if (sessionTime <= TimeSpan.Zero)
            {
                sessionTimer.Stop();
                MessageBox.Show("Время сеанса истекло. Система будет заблокирована на 30 минут.");
                AddToHistoryEnd(userId);
                new MainWindow().Show();
                this.Close();
            }
            else if (sessionTime <= TimeSpan.FromMinutes(15))
            {
                tbTimer.Foreground = Brushes.Red;
            }
        }

       public class Services
        {
            public int serviceCode { get; set; }
            public string result { get; set; }
        }

        public class GetAnalizator
        {
            public string patient { get; set; }
            public List<Services> services { get; set; }
            public int progress { get; set; }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AddToHistory(userId);
            sessionTimer.Stop();
            new MainWindow().Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            sessionTimer?.Stop();
        }

        private void AddToHistoryEnd(int userId)
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
                        id_Reason = 1
                    };

                    db.HistoryIn.Add(history);
                    db.SaveChanges();
                }
            }
            catch { }
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


        //Услуги
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
           
        }



        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }
        string name;
        int servCode;
        DateTime start;

        private void Button_Click(object sender, RoutedEventArgs e)
        { try
            {
                Services services1 = new Services();
                List<Services> services = new List<Services>();

                if (dgServe.SelectedIndex != -1)
                {
                    services1.serviceCode = (dgServe.SelectedItem as ServicesForOrder).id_service;
                    servCode = (dgServe.SelectedItem as ServicesForOrder).id;
                    services.Add(services1);
                }
                else MessageBox.Show("Не выбран анализатор");

                //получаем из бд
                string patient = (dgServe.SelectedItem as ServicesForOrder).Order.id_user.ToString();
                if (cmbAnalyser.SelectedIndex != -1)
                {
                    name = cmbAnalyser.Text;


                    var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{name}");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = new JavaScriptSerializer().Serialize(new
                        {
                            patient,
                            services
                        });

                        streamWriter.Write(json);
                        start = DateTime.Now;
                    }


                
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    MessageBox.Show("Успешно");
                    timerAnalysator();
                }

                else
                    MessageBox.Show("Ошибка");
            }
            else MessageBox.Show("Выберите анализатор");
        }catch{
                MessageBox.Show("занято");
        }


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            GetAnalizator getAnalizators = new GetAnalizator();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{name}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {

                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        int statusID=3;
                        StreamReader reader = new StreamReader(stream);
                        string json = reader.ReadToEnd();
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        getAnalizators = serializer.Deserialize<GetAnalizator>(json);
                        MessageBox.Show(getAnalizators.patient + ": " + getAnalizators.services[0].result);
                        var answer = (MessageBox.Show("Результат одобрен?", "Одобрение результата", MessageBoxButton.YesNo));
                        if (answer == MessageBoxResult.Yes)
                        {
                            statusID = 3;
                        }
                        else
                        {
                            statusID = 2;
                        }
                        var entities = new Entities();
                        var serv = entities.ServicesForOrder.FirstOrDefault(x => x.id == servCode);
                        serv.id_status = statusID;
                        serv.result = getAnalizators.services[0].result;
                        entities.SaveChanges();
                        var analys = new InfoАnalyser()
                        {
                            data_receipt = start,
                            date_completion = DateTime.Now - start,
                            id_user = userId,
                            id_analyser = cmbAnalyser.SelectedIndex + 1,
                            id_servicesfororder = servCode
                        };
                        entities.InfoАnalyser.Add(analys);
                        entities.SaveChanges();
                        dgServe.ItemsSource = entities.ServicesForOrder.Where(x => x.id_status != 3).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public void timerAnalysator()
        {
            completed1 = 0;
            timerAnalys = new DispatcherTimer();
            timerAnalys.Interval = TimeSpan.FromSeconds(1);
            timerAnalys.Tick += AnalysTimer_Tick;
            timerAnalys.Start();
            MessageBox.Show("Ожидайте 30 секунд");
        }
        //таймер для анализатора
        int completed1 = 0;
        private void AnalysTimer_Tick(object sender, EventArgs e)
        {
            
               AnalysTime = AnalysTime.Subtract(TimeSpan.FromSeconds(1));
            
            progBar.Visibility = Visibility.Visible;
            

            if (AnalysTime <= TimeSpan.Zero)
            {
                timerAnalys.Stop();
                MessageBox.Show("30 секунд закончились");
                btnGet.IsEnabled = true;
                progBar.Visibility= Visibility.Hidden;
                AnalysTime = TimeSpan.FromSeconds(30);
            }
            else
            {
                btnGet.IsEnabled = false;
                progBar.Value = completed1;
                completed1 += 5;
            }

        }

        private void cmbAnalyser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var entities = new Entities();
            if (cmbAnalyser.SelectedIndex ==0 )
                dgServe.ItemsSource = entities.ServicesForOrder.Where(x => x.id_status != 3 && (x.Services.mark == 1 || x.Services.mark == 3)).ToList();
            else
                dgServe.ItemsSource = entities.ServicesForOrder.Where(x => x.id_status != 3 && (x.Services.mark == 2 || x.Services.mark == 3)).ToList();
        }
    }
}
