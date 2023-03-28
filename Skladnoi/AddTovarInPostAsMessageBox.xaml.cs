using Kursovoi.Classes;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace Kursovoi.Skladnoi
{
    /// <summary>
    /// Логика взаимодействия для AddTovarInPostAsMessageBox.xaml
    /// </summary>
    public partial class AddTovarInPostAsMessageBox : Window
    {
        DataContext db;
        Tovar selTovar;
        DatagridPostavka dp;
        public AddTovarInPostAsMessageBox(DataContext _db)
        {
            InitializeComponent();
            db = _db;
            TovarsTypeCmbx.ItemsSource = db.Tovar.ToList();
         
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddTovarInPostClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selTovar == null)
                {
                    MessageBox.Show("Ошибка в выборе товара! Пожалуйста, выберите товар из списка.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if(CountTextbox.textBox.Text.Trim(' ').Length < 1)
                {
                    MessageBox.Show("Ошибка в количестве товара! Пожалуйста, введите корректное число.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                            
                Random rnd = new Random();
                TovarsListForPostavka.NumberI++;
                TovarsListForPostavka.tovarslist.Add(new DatagridPostavka
                {
                    tovar = selTovar,
                    sklad = db.Sklad.Where(x => x.Tovar_id == selTovar.Tovar_id).FirstOrDefault(),
                    Number = TovarsListForPostavka.NumberI,
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256))),
                    Count = int.Parse(CountTextbox.textBox.Text),
                    Pur_price = int.Parse(ZakPriceTextbox.textBox.Text)
                });
            }
            catch(System.OverflowException ef)
            {
               MessageBox.Show("Ошибка в количестве товара! Пожалуйста, введите корректное число.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void NumbersTextInput(object sender, TextCompositionEventArgs e)
        {
            string[] number = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            if (!number.Contains(e.Text))//если не цифра
            {
                e.Handled = true;
            }
        }

        private void SelectedTovarType(object sender, SelectionChangedEventArgs e)
        {
            List<Sklad> ss = db.Sklad.ToList();
            ComboBox cbx = (ComboBox)sender;
            selTovar = db.Tovar.Where(x => x== cbx.SelectedItem).FirstOrDefault();
          
            if (selTovar != null)
            {
                var c = selTovar.Sklad.FirstOrDefault();

                if (c != null)
                    ZakPriceTextbox.textBox.Text = c.Purchase_price.ToString();
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

    }
}
