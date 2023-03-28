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

namespace Kursovoi.Skladnoi.Pages
{
    /// <summary>
    /// Логика взаимодействия для HistoryPage.xaml
    /// </summary>
    public partial class HistoryPage : Page
    {
        ObservableCollection<HistorySkladnoiClass> history = new ObservableCollection<HistorySkladnoiClass>(); //товары склад
        ObservableCollection<ShipInHistoryClass> shipInHistory = new ObservableCollection<ShipInHistoryClass>(); //товары склад
        private DataContext db = new DataContext();
        public HistoryPage()
        {
            InitializeComponent();

            Loading();

        }

        public void Loading()
        {
           // db = new DataContext();
            #region взаимодействуем с таблицами, между которым установлена нужная нам связь
            List<Tovar> t = db.Tovar.ToList();
            List<Sklad> s = db.Sklad.ToList();
            List<Shipment> ss = db.Shipment.ToList();
            #endregion
            history = new ObservableCollection<HistorySkladnoiClass>();

            //добавление товаров в список (из листа в обсерабл)
            string stat = "";
            var local = db.History.ToList();
            Random rnd = new Random();
            int i = 0;
            foreach (var item in local)
            {
               

                i++;
                history.Add(new HistorySkladnoiClass
                {
                    history = item,
                    Number = i,
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256)))
                 
                });
            }


            DataGridtable.ItemsSource = history; 


            CountRezultTbx.Text = "Результатов: " + history.Count;

            #region для группировки (уже ненужно)
            //    ListCollectionView collection = new ListCollectionView(history);
            //   collection.GroupDescriptions.Add(new PropertyGroupDescription("history.Date"));
            #endregion 
        }

        private void SelectedHistoryDG(object sender, MouseButtonEventArgs e)
        {
            backButton.Visibility = Visibility.Visible;
            DataGridshipment.Visibility = Visibility.Visible;
            DataGridtable.Visibility = Visibility.Hidden;


            #region взаимодействуем с таблицами, между которым установлена нужная нам связь
            List<History> t = db.History.ToList();
            List<Shipment> s = db.Shipment.ToList();
            List<Tovar> ss = db.Tovar.ToList();
            #endregion


            HistorySkladnoiClass perexod = (HistorySkladnoiClass)DataGridtable.SelectedItem;
            shipInHistory = new ObservableCollection<ShipInHistoryClass>();
            var local = db.Shipment.Where(x => x.History_id == perexod.history.History_id).ToList();
            Random rnd = new Random();
            int i = 0;
            foreach (var item in local)
            {
                i++;
                shipInHistory.Add(new ShipInHistoryClass
                {
                    shipment = item,
                    Number = i,
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256))),
                });
            }


            DataGridshipment.ItemsSource = null;
            DataGridshipment.ItemsSource = shipInHistory;

            CountRezultTbx.Text = "Результатов: " + shipInHistory.Count;

        }

        private void Back(object sender, RoutedEventArgs e)
        {
            backButton.Visibility = Visibility.Hidden;
            DataGridshipment.Visibility = Visibility.Hidden;
            DataGridtable.Visibility = Visibility.Visible;
            
            DataGridtable.ItemsSource = null;
            DataGridtable.ItemsSource = history;

            CountRezultTbx.Text = "Результатов: " + history.Count;
        }

        private void SearchTextBox(object sender, TextChangedEventArgs e)
        {
            //выводит даты, где содержится данный товар


            #region поиск

            List<Shipment> ship = db.Shipment.ToList();
            List<Shipment> need = new List<Shipment>();
            
            foreach (var item in ship)
            {
                if (item.Tovar.Artikul.ToString().Contains(txtSearch.Text))
                {
                    need.Add(item);
                }
            }

            List<History> search = new List<History>();    
            foreach (var item in need)
            {
                search.Add(item.History);
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
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256)))
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

