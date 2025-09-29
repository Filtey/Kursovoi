using Kursovoi.Auth_Registr.UserControls;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
using Kursovoi.ConnectToDB.Model.ApiCRUDs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kursovoi.Skladnoi
{
    /// <summary>
    /// Логика взаимодействия для AddNewManufWindow.xaml
    /// </summary>
    public partial class AddNewManufWindow : Window
    {
        APIClass db;
        public AddNewManufWindow()
        {
            InitializeComponent();
            try
            {
                db = new APIClass();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void AddTovarClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //проверяем почту
                var Email = EmailTextbox.textBox.Text;

                string pattern = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

                if (!Regex.IsMatch(Email, pattern)) //почта невалидна
                {
                    MessageBox.Show("Недействительный адрес эл.почты!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Manufacturer man = new Manufacturer
                {
                    Name_Company = NameCompanyTextbox.textBox.Text,
                    FIO_director = NameTextbox.textBox.Text,
                    Address = AddressTextbox.textBox.Text,
                    Email = EmailTextbox.textBox.Text
                };

                db.AddManufacturer(man);
                // db.SaveChanges();
                MessageBoxResult r = MessageBox.Show("Успешно!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);

                if (r == MessageBoxResult.OK)
                    Close();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }


        /// <summary>
        /// метод для валидации артикула (только цифры)
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
        /// метод для валидации только буквы
        /// </summary>
        private void SNFTextInput(object sender, TextCompositionEventArgs e)
        {
            MyTextBox mt = sender as MyTextBox;

            if (!char.IsLetter(char.Parse(e.Text))) //если не буква
            {
                e.Handled = true;
            }           
        }

        //для перемещения окна
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
