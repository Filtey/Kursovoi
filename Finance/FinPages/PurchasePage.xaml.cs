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

namespace Kursovoi.Finance.FinPages
{
    public partial class PurchasePage : Page
    {
        ObservableCollection<DatagridSklad> products = new ObservableCollection<DatagridSklad>(); //товары склад
        public ObservableCollection<string> filter = new ObservableCollection<string>(); //фильтр группировка по виду товара (одежда, еда и т.д.)
        private DataContext db;
        public PurchasePage()
        {
            InitializeComponent();
            var converter = new BrushConverter();
            //  db = new DataContext();

            Loading();

        }

        public void Loading()
        {
            db = new DataContext();
            #region взаимодействуем с таблицами, между которым установлена нужная нам связь
            List<Tovar> t = db.Tovar.ToList();
            List<Sklad> s = db.Sklad.ToList();
            #endregion
            products = new ObservableCollection<DatagridSklad>();
            //добавление товаров в список (из листа в обсерабл)
            var local = db.Sklad.ToList();
            Random rnd = new Random();
            var local2 = new ObservableCollection<Sklad>(local.OrderBy(x => x.Count));  //сортируем изначально кол-ву на складе
            foreach (var item in local2)
            {
                products.Add(new DatagridSklad
                {
                    sklad = item,
                    Number = item.Tovar.Artikul,
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256))),
                    NameTovar = item.Tovar.Name.Substring(0, 1)
                });
            }

            DataGridtable.ItemsSource = products;
            CountRezultTbx.Text = "Результатов: " + products.Count;

            List<string> filter = new List<string>();
            filter = (List<string>)db.Tovar.Select(x => x.Type_tovar).Distinct().ToList();

            multicombobox.ItemsSource = filter;
        }


        private void CheckedFilter(object sender, RoutedEventArgs e)
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



        public void Filtering() //фильтрация
        {
            #region фильтрация
            Filtertbx.Text = null;
            if (filter.Count == 0)
            {
                DataGridtable.ItemsSource = products;
                Filtertbx.Text = "Здесь будут отображаться выбранные фильтры...";
                CountRezultTbx.Text = "Результатов: " + products.Count;
                #region поиск если фильтров нет
                List<DatagridSklad> search2 = products.Where(x => x.sklad.Tovar.Artikul.ToString().Contains(txtSearch.Text)).ToList();

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

            var productsss = products.Where(x => filter.Contains(x.sklad.Tovar.Type_tovar)).ToList();
            DataGridtable.ItemsSource = productsss;

            foreach (var item in filter)
            {
                Filtertbx.Text += item.ToString() + ", ";
            }

            Filtertbx.Text = Filtertbx.Text.Substring(0, Filtertbx.Text.Length - 2);
            CountRezultTbx.Text = "Результатов: " + productsss.Count;
            #endregion

            #region поиск
            List<DatagridSklad> search = productsss.Where(x => x.sklad.Tovar.Artikul.ToString().Contains(txtSearch.Text)).ToList();

            DataGridtable.ItemsSource = search;
            CountRezultTbx.Text = "Результатов: " + search.Count;
            if (txtSearch.Text == "" || txtSearch.Text == null)
            {
                DataGridtable.ItemsSource = productsss;
                CountRezultTbx.Text = "Результатов: " + productsss.Count;
            }
            #endregion

        }

        private void SearchTextBox(object sender, TextChangedEventArgs e)
        {

            #region поиск
            List<DatagridSklad> search = products.Where(x => x.sklad.Tovar.Artikul.ToString().Contains(txtSearch.Text)).ToList();

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

            var productsss = search.Where(x => filter.Contains(x.sklad.Tovar.Type_tovar)).ToList();
            DataGridtable.ItemsSource = productsss;

            foreach (var item in filter)
            {
                Filtertbx.Text += item.ToString() + ", ";
            }

            Filtertbx.Text = Filtertbx.Text.Substring(0, Filtertbx.Text.Length - 2);
            CountRezultTbx.Text = "Результатов: " + productsss.Count;
            #endregion
        }
    }   

}