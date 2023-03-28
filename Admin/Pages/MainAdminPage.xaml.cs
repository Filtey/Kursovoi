using Kursovoi.Admin.Windows;
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

namespace Kursovoi.Admin.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainAdminPage.xaml
    /// </summary>
    public partial class MainAdminPage : Page
    {
      
        ObservableCollection<AdminClassUsers> members = new ObservableCollection<AdminClassUsers>();
        public ObservableCollection<string> filter = new ObservableCollection<string>();
        private DataContext db;

        public MainAdminPage()
        {
            InitializeComponent();
            var converter = new BrushConverter();


            Loading();

        }

        public void Loading()
        {
            db = new DataContext();
            #region взаимодействуем с таблицами, между которым установлена нужная нам связь
            List<Account> t = db.Account.ToList();

            #endregion
            members = new ObservableCollection<AdminClassUsers>();
            //добавление товаров в список (из листа в обсерабл)
            var local = db.Account.ToList();
            Random rnd = new Random();
            int i = 0;
            foreach (var item in local)
            {
                i++;
                members.Add(new AdminClassUsers
                {
                    account = item,
                    Number = i,
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256))),
                    NameB = item.Name.Substring(0, 1),
                });
            }

            DataGridtable.ItemsSource = members;
            CountRezultTbx.Text = "Результатов: " + members.Count;

            List<string> filter = new List<string>();
            filter = (List<string>)db.Account.Select(x => x.Post).Distinct().ToList();

            multicombobox.ItemsSource = filter;
        }


        private void EditTovar(object sender, RoutedEventArgs e)
        {
            AdminClassUsers AccForEditing = (AdminClassUsers)(sender as FrameworkElement).DataContext;
            EditAccWindow edt = new EditAccWindow(AccForEditing.account); //создать окно редактирования
            edt.Closing += Adt_Closing;
            edt.ShowDialog();
        }

        private void Adt_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            DataGridtable.ItemsSource = null;
            Loading();
        }

        private void RemoveAcc(object sender, RoutedEventArgs e)
        {
            MessageBoxResult rez = MessageBox.Show("Вы точно хотите удалить данного пользователя?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (rez == MessageBoxResult.Yes)
            {
                AdminClassUsers AccForDeleting = (AdminClassUsers)(sender as FrameworkElement).DataContext;

                db = new DataContext();
                var del = db.Account.Where(x => x.Account_id == AccForDeleting.account.Account_id).First();

                db.Account.Remove(del);

                db.SaveChanges();
                MessageBox.Show("Успешно!", "Уведомление", MessageBoxButton.OK);
                Loading();

            }
        }


        /// <summary>
        /// для фильтра выбрали какой-то тип акк-та
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                DataGridtable.ItemsSource = members;
                Filtertbx.Text = "Здесь будут отображаться выбранные фильтры...";
                CountRezultTbx.Text = "Результатов: " + members.Count;
                #region поиск если фильтров нет
                List<AdminClassUsers> search2 = members.Where(x => x.account.Surname.Contains(txtSearch.Text)).ToList();

                DataGridtable.ItemsSource = search2;
                CountRezultTbx.Text = "Результатов: " + search2.Count;
                if (txtSearch.Text == "" || txtSearch.Text == null)
                {
                    DataGridtable.ItemsSource = members;
                    CountRezultTbx.Text = "Результатов: " + members.Count;
                }
                return;
                #endregion
            }

            var productsss = members.Where(x => filter.Contains(x.account.Post)).ToList();
            DataGridtable.ItemsSource = productsss;

            foreach (var item in filter)
            {
                Filtertbx.Text += item.ToString() + ", ";
            }

            Filtertbx.Text = Filtertbx.Text.Substring(0, Filtertbx.Text.Length - 2);
            CountRezultTbx.Text = "Результатов: " + productsss.Count;
            #endregion

            #region поиск
            List<AdminClassUsers> search = productsss.Where(x => x.account.Surname.Contains(txtSearch.Text)).ToList();

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
            List<AdminClassUsers> search = members.Where(x => x.account.Surname.Contains(txtSearch.Text)).ToList();

            DataGridtable.ItemsSource = search;
            CountRezultTbx.Text = "Результатов: " + search.Count;
            if (txtSearch.Text == "" || txtSearch.Text == null)
            {
                DataGridtable.ItemsSource = members;
                CountRezultTbx.Text = "Результатов: " + members.Count;
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

            var productsss = search.Where(x => filter.Contains(x.account.Post)).ToList();
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

