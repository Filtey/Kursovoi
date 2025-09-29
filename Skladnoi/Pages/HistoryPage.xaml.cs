using StoreSystem.Classes;
using StoreSystem.ConnectToDB;
using StoreSystem.ConnectToDB.Model;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
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
using System.Windows.Threading;

namespace StoreSystem.Skladnoi.Pages
{
    /// <summary>
    /// Логика взаимодействия для HistoryPage.xaml
    /// </summary>
    public partial class HistoryPage : Page
    {
        ObservableCollection<HistorySkladnoiClass> history = new ObservableCollection<HistorySkladnoiClass>(); //товары склад
        ObservableCollection<ShipInHistoryClass> shipInHistory = new ObservableCollection<ShipInHistoryClass>(); //товары склад
        private APIClass db;
        List<Tovar> t;
        List<Sklad> s;
        List<Shipment> ss;
        List<History> his;

        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private int timeForTimer = 90;
        public HistoryPage()
        {
            InitializeComponent();
            try
            {
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

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (timeForTimer == 0) // обновляем окно
                {
                    timeForTimer = 90;
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
            try
            {
                db = new APIClass();
                // db = new DataContext();
                #region взаимодействуем с таблицами, между которым установлена нужная нам связь
                t = db.TovarList();
                s = db.SkladList();
                ss = db.ShipmentList();
                his = db.HistoryList();
                #endregion
                history = new ObservableCollection<HistorySkladnoiClass>();

                //добавление товаров в список (из листа в обсерабл)
                string stat = "";
                var local = his;
                Random rnd = new Random();
                int i = 0;
                foreach (var item in local)
                {


                    i++;
                    history.Add(new HistorySkladnoiClass
                    {
                        history = item,
                        Number = i,
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256)))
                    });
                }


                DataGridtable.ItemsSource = history;


                CountRezultTbx.Text = "Результатов: " + history.Count;

                #region для группировки (уже ненужно)
                //    ListCollectionView collection = new ListCollectionView(history);
                //   collection.GroupDescriptions.Add(new PropertyGroupDescription("history.Date"));
                #endregion
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
                backButton.Visibility = Visibility.Visible;
                DataGridshipment.Visibility = Visibility.Visible;
                DataGridtable.Visibility = Visibility.Hidden;


                #region (NO)  взаимодействуем с таблицами, между которым установлена нужная нам связь
                //List<History> t = db.History.ToList();
                //List<Shipment> s = db.Shipment.ToList();
                //List<Tovar> ss = db.Tovar.ToList();
                #endregion


                HistorySkladnoiClass perexod = (HistorySkladnoiClass)DataGridtable.SelectedItem;
                shipInHistory = new ObservableCollection<ShipInHistoryClass>();
                var local = ss.Where(x => x.History_id == perexod.history.History_id).ToList();
                Random rnd = new Random();
                int i = 0;
                foreach (var item in local)
                {
                    i++;
                    shipInHistory.Add(new ShipInHistoryClass
                    {
                        shipment = item,
                        Number = i,
                        tovar = t.First(x => x.Tovar_id == item.Tovar_id),
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256))),

                    });
                }


                DataGridshipment.ItemsSource = null;
                DataGridshipment.ItemsSource = shipInHistory;

                CountRezultTbx.Text = "Результатов: " + shipInHistory.Count;
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

                DataGridtable.ItemsSource = null;
                SearchTextBox(null, null);
                // DataGridtable.ItemsSource = history;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

        }

        private void SearchTextBox(object sender, TextChangedEventArgs e)
        {
            try
            {
                //выводит даты, где содержится данный товар


                #region поиск

                List<Shipment> ship = ss;
                List<Shipment> need = new List<Shipment>();

                foreach (var item in ship)
                {
                    var proverka = t.First(x => x.Tovar_id == item.Tovar_id);
                    if (proverka.Artikul.ToString().Contains(txtSearch.Text) || proverka.Name.ToLower().ToString().Contains(txtSearch.Text.ToLower()))
                    {
                        need.Add(item);
                    }
                }

                List<History> search = new List<History>();
                foreach (var item in need)
                {
                    search.Add(his.First(x => x.History_id == item.History_id));
                }
                search = search.Distinct().ToList();

                List<HistorySkladnoiClass> foundHistory = new List<HistorySkladnoiClass>();

                string stat = "";
                int i = 0;
                Random rnd = new Random();
                foreach (var item in search)
                {
                    i++;


                    foundHistory.Add(new HistorySkladnoiClass
                    {
                        history = item,
                        Number = i,
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256)))

                    });
                }
                foundHistory = foundHistory.Distinct().ToList();





                DataGridtable.ItemsSource = foundHistory;
                CountRezultTbx.Text = "Результатов: " + foundHistory.Count;
                if (txtSearch.Text == "" || txtSearch.Text == null || txtSearch.Text.Trim(' ').Length == 0)
                {
                    DataGridtable.ItemsSource = history;
                    CountRezultTbx.Text = "Результатов: " + history.Count;
                }
                #endregion
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        #region зелень
        //private void CheckedFilter(object sender, RoutedEventArgs e)
        //{
        //    var SelectedFilter = (sender as FrameworkElement).DataContext;

        //    var checkbox = sender as CheckBox;
        //    if (checkbox.IsChecked == true) //если выбрали, то добавляем
        //    {
        //        filter.Add(SelectedFilter.ToString());
        //    }
        //    else
        //    {
        //        filter.Remove(SelectedFilter.ToString());
        //    }
        //    Filtering();
        //}






        //private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //        DragMove();
        //}




        //private bool IsMiximized = false;
        //private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ClickCount == 2)
        //    {
        //        if (IsMiximized)
        //        {
        //            WindowState = WindowState.Normal;
        //            Width = 1080;
        //            Height = 720;
        //            IsMiximized = false;
        //        }
        //        else
        //        {
        //            WindowState = WindowState.Maximized;
        //            IsMiximized = true;
        //        }
        //    }
        //}
        #endregion

    }

}

