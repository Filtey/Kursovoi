using Kursovoi.Classes;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
using Kursovoi.ConnectToDB.Model.ApiCRUDs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace Kursovoi.Finance.FinPages
{
    /// <summary>
    /// Логика взаимодействия для AllOrders.xaml
    /// </summary>
    public partial class AllOrders : Page
    {
        APIClass db;
        ObservableCollection<FinAllOrder> listorder;
        ObservableCollection<FinAllOrder> listorderInDate;
        DateTime? beforeD;
        DateTime? afterD;
      

        List<Sell> sel;
        List<Account> a;
        List<Tovar> t;
        List<SellTovars> st;


        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private int timeForTimer = 90;
        public AllOrders()
        {
            InitializeComponent();
            //DataGridtable.ItemsSource 
            try
            {
                db = new APIClass();

                #region взаимодействуем с таблицами, между которым установлена нужная нам связь
                sel = db.SellList();
                a = db.AccountList();
                t = db.TovarList();
                st = db.SellTovarsList();
                #endregion
                Loading();
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        //обновление окна каждые 1.5 минуты
        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (timeForTimer == 0) // обновляем окно
                {
                    timeForTimer = 90;
                    #region взаимодействуем с таблицами, между которым установлена нужная нам связь
                    sel = db.SellList();
                    a = db.AccountList();
                    t = db.TovarList();
                    st = db.SellTovarsList();
                    #endregion
                    Loading();
                    return;
                }

                timeForTimer--;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        public void Loading()
        {
            // db = new DataContext();
            try
            {
                listorder = new ObservableCollection<FinAllOrder>();

                //добавление товаров в список (из листа в обсерабл)

                var local = st;
                var local2 = new ObservableCollection<SellTovars>(local.OrderByDescending(x => x.Date_sell));  //сортируем изначально по датам
                Random rnd = new Random();
                int i = 0;
                foreach (var item in local2)
                {
                    i++;
                    listorder.Add(new FinAllOrder
                    {
                        sellTovars = item,
                        Number = i,
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256)))

                    });
                }

                // listorder = new ObservableCollection<FinAllOrder>(listorder.OrderByDescending(x => x.sellTovars.Date_sell));
                DataGridtable.ItemsSource = listorder;

                CountRezultTbx.Text = "Результатов: " + listorder.Count;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void SelectedHistoryDG(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FinAllOrder perexod = (FinAllOrder)DataGridtable.SelectedItem;
                if (perexod == null) return; //проверка на то, что случайно нажали на датагрид, в котором нет элементов

                #region отображение 
                backButton.Visibility = Visibility.Visible;
                DataGridshipment.Visibility = Visibility.Visible;
                DataGridtable.Visibility = Visibility.Hidden;
                #endregion

                listorderInDate = new ObservableCollection<FinAllOrder>();
                var local = sel.Where(x => x.SellTovars_id == perexod.sellTovars.SellTovars_id).ToList();
                Random rnd = new Random();
                int i = 0;
                foreach (var item in local)
                {
                    i++;
                    var forFIO = st.First(x => x.SellTovars_id == item.SellTovars_id);
                    var acc = a.First(x => x.Account_id == forFIO.Kassir_id);
                    string fio;

                    if (acc.Patronymic == null) fio = acc.Surname + " " + acc.Name.Substring(0, 1) + ". ";
                    else fio = acc.Surname + " " + acc.Name.Substring(0, 1) + ". " + acc.Patronymic.Substring(0, 1) + ". ";

                    listorderInDate.Add(new FinAllOrder
                    {
                        sell = item,
                        Number = i,
                        tovar = t.First(x => x.Tovar_id == item.Tovar_id),
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256))),
                        FIO = fio
                    });
                }


                DataGridshipment.ItemsSource = null;
                DataGridshipment.ItemsSource = listorderInDate;

                CountRezultTbx.Text = "Результатов: " + listorderInDate.Count;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            try
            {
                backButton.Visibility = Visibility.Hidden;
                DataGridshipment.Visibility = Visibility.Hidden;
                DataGridtable.Visibility = Visibility.Visible;

                SearchDateClick(null, null);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void BeforeDateClick(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                beforeD = (DateTime?)BeforeDatePicker.SelectedDate;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void AfterDateClick(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                afterD = (DateTime?)AfterDatePicker.SelectedDate;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void SearchDateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Loading();
                if (beforeD != null && afterD != null) //если обе даты выбраны
                {
                    listorder = new ObservableCollection<FinAllOrder>(listorder.Where(x => x.sellTovars.Date_sell >= beforeD && x.sellTovars.Date_sell <= afterD));
                    DataGridtable.ItemsSource = null;
                    DataGridtable.ItemsSource = listorder;
                    CountRezultTbx.Text = "Результатов: " + listorder.Count;

                }
                else if (beforeD != null) //выбрана С какого числа
                {
                    listorder = new ObservableCollection<FinAllOrder>(listorder.Where(x => x.sellTovars.Date_sell >= beforeD));
                    DataGridtable.ItemsSource = null;
                    DataGridtable.ItemsSource = listorder;
                    CountRezultTbx.Text = "Результатов: " + listorder.Count;
                }
                else if (afterD != null) //выбрана ПО какое число
                {
                    listorder = new ObservableCollection<FinAllOrder>(listorder.Where(x => x.sellTovars.Date_sell <= afterD));
                    DataGridtable.ItemsSource = null;
                    DataGridtable.ItemsSource = listorder;
                    CountRezultTbx.Text = "Результатов: " + listorder.Count;
                }
                else if (sender == null && e == null) //если нажали на кнопку назад, а даты выбраны не были
                {
                    Loading();
                }
                else //ничего не выбрано
                {
                    MessageBox.Show("Выберите хотя бы одну из дат!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void ClearDateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                BeforeDatePicker.SelectedDate = null;
                AfterDatePicker.SelectedDate = null;
                Loading();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        //private void SearchTextBox(object sender, TextChangedEventArgs e)
        //{
        //    //выводит даты, где содержится данный товар


        //    #region поиск

        //    List<Shipment> ship = db.Shipment.ToList();
        //    List<Shipment> need = new List<Shipment>();

        //    foreach (var item in ship)
        //    {
        //        if (item.Tovar.Artikul.ToString().Contains(txtSearch.Text))
        //        {
        //            need.Add(item);
        //        }
        //    }

        //    List<History> search = new List<History>();
        //    foreach (var item in need)
        //    {
        //        search.Add(item.History);
        //    }
        //    search = search.Distinct().ToList();

        //    List<HistorySkladnoiClass> foundHistory = new List<HistorySkladnoiClass>();

        //    string stat = "";
        //    int i = 0;
        //    Random rnd = new Random();
        //    foreach (var item in search)
        //    {
        //        i++;


        //        foundHistory.Add(new HistorySkladnoiClass
        //        {
        //            history = item,
        //            Number = i,
        //            BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256)))
        //        });
        //    }
        //    foundHistory = foundHistory.Distinct().ToList();





        //    DataGridtable.ItemsSource = foundHistory;
        //    CountRezultTbx.Text = "Результатов: " + foundHistory.Count;
        //    if (txtSearch.Text == "" || txtSearch.Text == null || txtSearch.Text.Trim(' ').Length == 0)
        //    {
        //        DataGridtable.ItemsSource = history;
        //        CountRezultTbx.Text = "Результатов: " + history.Count;
        //    }
        //    #endregion
        // }
    }
}
