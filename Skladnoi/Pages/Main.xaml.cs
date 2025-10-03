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
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        ObservableCollection<DatagridSklad> products = new ObservableCollection<DatagridSklad>(); //товары склад
        public ObservableCollection<string> filter = new ObservableCollection<string>(); //фильтр группировка по виду товара (одежда, еда и т.д.)
        private APIClass db;
        List<Tovar> t;
        List<Sklad> s;

        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private int timeForTimer = 90;
        public Main()
        {
            InitializeComponent();
            var converter = new BrushConverter();
            //  db = new DataContext();



            #region зелень айтемы
            ////создаем датагрид итемы инфо
            //products.Add(new Member { Number = "1", Character = "J", BgColor = (Brush)converter.ConvertFromString("#1098ad"), Name = "John Doe", Position = "Лох", Email = "Pav@gmail.com", Phone = "79834523345" });
            //products.Add(new Member { Number = "2", Character = "H", BgColor = (Brush)converter.ConvertFromString("#1ee8e5"), Name = "Иван Иванов", Position = "Лох", Email = "Jon@gmail.com", Phone = "79806754432" });
            //products.Add(new Member { Number = "3", Character = "G", BgColor = (Brush)converter.ConvertFromString("#ff8f00"), Name = "Петр Петров", Position = "Лох", Email = "Faq@gmail.com", Phone = "79291123433" });
            //products.Add(new Member { Number = "4", Character = "F", BgColor = (Brush)converter.ConvertFromString("#ff5252"), Name = "Сергей Сергеев", Position = "Лох", Email = "oru@gmail.com", Phone = "79820987890" });
            //products.Add(new Member { Number = "5", Character = "D", BgColor = (Brush)converter.ConvertFromString("#0ca678"), Name = "Андрей Андреев", Position = "Лох", Email = "rty@gmail.com", Phone = "79294431866" });
            //products.Add(new Member { Number = "6", Character = "S", BgColor = (Brush)converter.ConvertFromString("#ff6d00"), Name = "Илья Илеев", Position = "Лох", Email = "dfg@gmail.com", Phone = "79059978401" });
            //products.Add(new Member { Number = "7", Character = "A", BgColor = (Brush)converter.ConvertFromString("#1e88e5"), Name = "Павел Палеев", Position = "Лох", Email = "sdf@gmail.com", Phone = "79292245678" });
            //products.Add(new Member { Number = "8", Character = "Q", BgColor = (Brush)converter.ConvertFromString("#0ca678"), Name = "Артем Артемов", Position = "Админ", Email = "artem@gmail.com", Phone = "79827423345" });
            //products.Add(new Member { Number = "9", Character = "W", BgColor = (Brush)converter.ConvertFromString("#ff5252"), Name = "Кирилл Кириллов", Position = "Админ", Email = "kirill@gmail.com", Phone = "79292246555" });
            #endregion
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
                t = db.TovarList();
                s = db.SkladList();
                #endregion
                products = new ObservableCollection<DatagridSklad>();
                //добавление товаров в список (из листа в обсерабл)
                var local = s;
                Random rnd = new Random();
                if (local == null) return;
                foreach (var item in local)
                {
                    var tovar = t.First(x => x.Tovar_id == item.Tovar_id);
                    products.Add(new DatagridSklad
                    {
                        sklad = item,
                        Number = tovar.Artikul,
                        tovar = tovar,
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256))),
                        NameTovar = tovar.Name.Substring(0, 1)
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






        private void AddTovarPerexod(object sender, RoutedEventArgs e)
        {
            try
            {
                //из экселя или вручную
                ImportInFileOrHandWindow adt = new ImportInFileOrHandWindow("новый товар");
                adt.Closing += Adt_Closing;
                adt.ShowDialog();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        /// <summary>
        /// после добавления нового товара обновляем датагрид
        /// </summary>
        private void Adt_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                DataGridtable.ItemsSource = null;
                TovarsListForPostavka.tovarslist.Clear();
                TovarsListForPostavka.NumberI = 0;
                Loading();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void EditTovar(object sender, RoutedEventArgs e)
        {
            try
            {
                DatagridSklad TovarForEditing = (DatagridSklad)(sender as FrameworkElement).DataContext;
                EditingTovar edt = new EditingTovar(TovarForEditing.sklad);
                edt.Closing += Adt_Closing;
                edt.ShowDialog();
                //полученный выбранный элемент датагрид редактируем
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void RemoveTovar(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult rez = MessageBox.Show("Вы точно хотите удалить данный товар?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (rez == MessageBoxResult.Yes)
                {
                    DatagridSklad TovarForDelete = (DatagridSklad)(sender as FrameworkElement).DataContext;
                    //полученный выбранный элемент датагрид удаляем
                    //db = new DataContext();
                    var del = s.Where(x => x.Sklad_id == TovarForDelete.sklad.Sklad_id).First();
                    var delT = t.Where(x => x.Tovar_id == TovarForDelete.sklad.Tovar_id).First();

                    db.DeleteSklad(del.Sklad_id);
                    db.DeleteTovar(delT.Tovar_id);

                    //  db.SaveChanges();
                    MessageBox.Show("Успешно!", "Уведомление", MessageBoxButton.OK);
                    Loading();
                }
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
                List<DatagridSklad> search = productsss.Where(x => x.tovar.Artikul.ToString().Contains(txtSearch.Text) || x.tovar.Name.ToLower().ToString().Contains(txtSearch.Text.ToLower())).ToList();

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

        private void AddPostavkaPerexod(object sender, RoutedEventArgs e)
        {
            try
            {
                //из экселя или вручную
                ImportInFileOrHandWindow adt = new ImportInFileOrHandWindow("поставка");
                adt.Closing += Adt_Closing;
                adt.ShowDialog();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        //экспорт информации о товарах на складе в файл 
        private void ExportDataButton(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Excel|*.xlsx", FileName = "Склад.xlsx" };
                if (dlg.ShowDialog() == true)
                {
                    ImportExportTovarToXLSX.ExportToExcel(new APIClass(), dlg.FileName);
                    MessageBox.Show("Экспорт успешно завершён.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


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

    }

    //public class Member
    //{
    //    public string Character { get; set; }
    //    public string Number { get; set; }
    //    public string Name { get; set; }
    //    public string Position { get; set; }
    //    public string Email { get; set; }
    //    public string Phone { get; set; }
    //    public Brush BgColor { get; set; }
    //}

}