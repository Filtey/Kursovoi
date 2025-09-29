using Kursovoi.Classes;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
using Kursovoi.ConnectToDB.Model.ApiCRUDs;
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
        APIClass db;
        Tovar selTovar;
        DatagridPostavka dp;
        List<Tovar> t;
        List<Sklad> skladlist;
        public AddTovarInPostAsMessageBox()
        {
            InitializeComponent();
            try
            {
                db = new APIClass();
                skladlist = db.SkladList();
                t = db.TovarList();
                t.Insert(0, new Tovar { Name = "новый...", Artikul = 0 });
                TovarsTypeCmbx.ItemsSource = t;
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
                    sklad = skladlist.Where(x => x.Tovar_id == selTovar.Tovar_id).FirstOrDefault(),
                    Number = TovarsListForPostavka.NumberI,
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256))),
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

            try
            {
                List<Sklad> ss = skladlist;
                ComboBox cbx = (ComboBox)sender;

                if (cbx.SelectedItem == null) return;



                //если выбран "новый" товар, то перенаправляем на создание нового товара
                if (((Tovar)cbx.SelectedItem).Name == "новый...")
                {
                    AddTovar newtovar = new AddTovar();
                    newtovar.Closing += Newtovar_Closing;
                    newtovar.ShowDialog();
                    return;
                }


                selTovar = db.TovarList().Where(x => x.Tovar_id == ((Tovar)cbx.SelectedItem).Tovar_id).FirstOrDefault();

                if (selTovar != null)
                {

                    var c = skladlist.FirstOrDefault(x => x.Tovar_id == selTovar.Tovar_id);

                    if (c != null)
                        ZakPriceTextbox.textBox.Text = c.Purchase_price.ToString();
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
        /// после закрытия окна создания нового товара, обновляем список товаров
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Newtovar_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                skladlist = db.SkladList();
                t = db.TovarList();
                t.Insert(0, new Tovar { Name = "новый...", Artikul = 0 });
                TovarsTypeCmbx.ItemsSource = t;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
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

    }
}
