using Google.Protobuf.WellKnownTypes;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Net.Http.Json;

namespace StoreSystem.Auth_Registr
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
          //  webView.Source = new Uri("https://yandex.ru");
            APIClass api = new();
            webView.Source = api.Beznal((decimal)2, "тестовая оплата");
           
        }
     
        private void Button_Click(object sender, RoutedEventArgs e)
        {
        
        }

        public record CreatePaymentLinkResponse(string OrderId, string PaymentUrl);
    }

    
}
