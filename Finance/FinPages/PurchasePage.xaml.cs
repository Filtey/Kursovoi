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
using System.Windows.Threading;

namespace Kursovoi.Finance.FinPages
{
    public partial class PurchasePage : Page
    {
        ObservableCollection<DatagridSklad> products = new ObservableCollection<DatagridSklad>(); //товары склад
        public ObservableCollection<string> filter = new ObservableCollection<string>(); //фильтр группировка по виду товара (одежда, еда и т.д.)
        private APIClass db;
        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private int timeForTimer = 90;
        public PurchasePage()
        {
            InitializeComponent();
            var converter = new BrushConverter();
            //  db = new DataContext();
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
                #region взаимодействуем с таблицами, между которым установлена нужная нам связь
                List<Tovar> t = db.TovarList();
                List<Sklad> s = db.SkladList();
                #endregion

                products = new ObservableCollection<DatagridSklad>();
                //добавление товаров в список (из листа в обсерабл)
                var local = s;
                Random rnd = new Random();
                var local2 = new ObservableCollection<Sklad>(local.OrderBy(x => x.Count));  //сортируем изначально кол-ву на складе
                foreach (var item in local2)
                {
                    var tov = t.Where(x => x.Tovar_id == item.Tovar_id).First();
                    products.Add(new DatagridSklad
                    {
                        sklad = item,
                        tovar = tov,
                        Number = tov.Artikul,
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256))),
                        NameTovar = tov.Name.Substring(0, 1)
                    });
                }

                DataGridtable.ItemsSource = products;
                CountRezultTbx.Text = "Результатов: " + products.Count;

                List<string> filter = new List<string>();
                filter = (List<string>)t.Select(x => x.Type_tovar).Distinct().ToList();

                multicombobox.ItemsSource = filter;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        private void CheckedFilter(object sender, RoutedEventArgs e)
        {
            try
            {
                var SelectedFilter = (sender as FrameworkElement).DataContext;

                var checkbox = sender as CheckBox;
                if (checkbox.IsChecked == true) //если выбрали, то добавляем
                {
                    filter.Add(SelectedFilter.ToString());
                }
                else
                {
                    filter.Remove(SelectedFilter.ToString());
                }
                Filtering();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }



        public void Filtering() //фильтрация
        {
            try
            {
                #region фильтрация
                Filtertbx.Text = null;
                if (filter.Count == 0)
                {
                    DataGridtable.ItemsSource = products;
                    Filtertbx.Text = "Здесь будут отображаться выбранные фильтры...";
                    CountRezultTbx.Text = "Результатов: " + products.Count;
                    #region поиск если фильтров нет
                    List<DatagridSklad> search2 = products.Where(x => x.tovar.Artikul.ToString().Contains(txtSearch.Text)).ToList();

                    DataGridtable.ItemsSource = search2;
                    CountRezultTbx.Text = "Результатов: " + search2.Count;
                    if (txtSearch.Text == "" || txtSearch.Text == null)
                    {
                        DataGridtable.ItemsSource = products;
                        CountRezultTbx.Text = "Результатов: " + products.Count;
                    }
                    return;
                    #endregion
                }

                var productsss = products.Where(x => filter.Contains(x.tovar.Type_tovar)).ToList();
                DataGridtable.ItemsSource = productsss;

                foreach (var item in filter)
                {
                    Filtertbx.Text += item.ToString() + ", ";
                }

                Filtertbx.Text = Filtertbx.Text.Substring(0, Filtertbx.Text.Length - 2);
                CountRezultTbx.Text = "Результатов: " + productsss.Count;
                #endregion

                #region поиск
                List<DatagridSklad> search = productsss.Where(x => x.tovar.Artikul.ToString().Contains(txtSearch.Text)).ToList();

                DataGridtable.ItemsSource = search;
                CountRezultTbx.Text = "Результатов: " + search.Count;
                if (txtSearch.Text == "" || txtSearch.Text == null)
                {
                    DataGridtable.ItemsSource = productsss;
                    CountRezultTbx.Text = "Результатов: " + productsss.Count;
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

        private void SearchTextBox(object sender, TextChangedEventArgs e)
        {
            try
            {
                #region поиск
                List<DatagridSklad> search = products.Where(x => x.tovar.Artikul.ToString().Contains(txtSearch.Text) || x.tovar.Name.ToLower().ToString().Contains(txtSearch.Text.ToLower())).ToList();

                DataGridtable.ItemsSource = search;
                CountRezultTbx.Text = "Результатов: " + search.Count;
                if (txtSearch.Text == "" || txtSearch.Text == null)
                {
                    DataGridtable.ItemsSource = products;
                    CountRezultTbx.Text = "Результатов: " + products.Count;
                }
                #endregion

                #region фильтрация
                Filtertbx.Text = null;
                if (filter.Count == 0)
                {
                    DataGridtable.ItemsSource = search;
                    Filtertbx.Text = "Здесь будут отображаться выбранные фильтры...";
                    CountRezultTbx.Text = "Результатов: " + search.Count;
                    return;
                }

                var productsss = search.Where(x => filter.Contains(x.tovar.Type_tovar)).ToList();
                DataGridtable.ItemsSource = productsss;

                foreach (var item in filter)
                {
                    Filtertbx.Text += item.ToString() + ", ";
                }

                Filtertbx.Text = Filtertbx.Text.Substring(0, Filtertbx.Text.Length - 2);
                CountRezultTbx.Text = "Результатов: " + productsss.Count;
                #endregion
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