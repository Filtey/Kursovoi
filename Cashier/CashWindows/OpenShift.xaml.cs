using StoreSystem.Classes;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
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

namespace StoreSystem.Cashier.CashWindows
{
    /// <summary>
    /// Логика взаимодействия для OpenShift.xaml
    /// </summary>
    public partial class OpenShift : Window
    {
        APIClass api;
        public OpenShift()
        {
            InitializeComponent();
            api = new APIClass();
        }

        private void StartCkick(object sender, RoutedEventArgs e)
        {
            try
            {
                CashierShift.Date_Start = DateTime.Now;
                try
                {
                    int val = int.Parse(MoneyInCashMachineTextbox.textBox.Text.ToString());
                    if (val < 1000)
                    {
                        MessageBox.Show("Слишком маленькое значение!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    CashierShift.MoneyInCashMachine = val;
                    Close();
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка! Перезапустите программу!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
    
        
        /// <summary>
        /// метод для валидации (только цифры)
        /// </summary>
        private void NumbersTextInput(object sender, TextCompositionEventArgs e)
        {
            string[] number = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            if (!number.Contains(e.Text))//если не цифра
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// для перемещения окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

    }
}
