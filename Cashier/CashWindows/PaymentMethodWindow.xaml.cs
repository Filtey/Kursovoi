using Kursovoi.Classes;
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

namespace Kursovoi.Cashier.CashWindows
{
    /// <summary>
    /// Логика взаимодействия для PaymentMethodWindow.xaml
    /// </summary>
    public partial class PaymentMethodWindow : Window
    {
        public PaymentMethodWindow()
        {
            InitializeComponent();
        }

        private void BeznalMethod(object sender, MouseButtonEventArgs e)
        {
            StaticClassForUrlCardPayment.PaymentMethod = 1;
            Close();
        }

        private void NalMethod(object sender, MouseButtonEventArgs e)
        {
            StaticClassForUrlCardPayment.PaymentMethod = 2;
            Close();
        }

        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            StaticClassForUrlCardPayment.PaymentMethod = -1;
            Close();
        }
    }
}
