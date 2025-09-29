using Kursovoi.Classes;
using Kursovoi.ConnectToDB.Model.ApiCRUDs;
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
using System.Windows.Threading;

namespace Kursovoi.Cashier.CashWindows
{
    /// <summary>
    /// Логика взаимодействия для CashlessPaymentWindow.xaml
    /// </summary>
    public partial class CashlessPaymentWindow : Window
    {
        APIClass api;
        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private int timeForTimer = 2;

        public CashlessPaymentWindow(decimal summa, string description)
        {
            InitializeComponent();
            try
            {
                api = new APIClass();
                timer.Tick += Timer_Tick;
                webView.Source = api.Beznal(summa, description);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        /// <summary>
        /// таймер тик
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (timeForTimer == 0) // обновляем окно
                {
                    IsEnabled = true;
                    timer.Stop();
                    Close();
                }
                else
                    timeForTimer--;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                var url = webView.Source.ToString();
                StaticClassForUrlCardPayment.URL = url;

                if (url.Contains("https://yoomoney.ru/transfer/process/success")) //оплата успешно завершена
                {
                    StaticClassForUrlCardPayment.PaymentId = url.Substring(57);
                    timer.Start();
                }
                if (url.Contains("http://localhost:7114/thankyou")) //вышли из оплаты
                {
                    timer.Start();

                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
    }
}
