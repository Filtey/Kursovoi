using StoreSystem.Admin.Windows;
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

namespace StoreSystem.Admin.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainAdminPage.xaml
    /// </summary>
    public partial class MainAdminPage : Page
    {
      
        ObservableCollection<AdminClassUsers> members = new ObservableCollection<AdminClassUsers>();
        public ObservableCollection<string> filter = new ObservableCollection<string>();
        private APIClass db;
        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private int timeForTimer = 90;

        public MainAdminPage()
        {
            InitializeComponent();
            var converter = new BrushConverter();

            timer.Tick += Timer_Tick;
            Loading();
            timer.Start();

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
            catch (Exception ex)
            {
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
                List<Account> t = db.AccountList();

                #endregion
                members = new ObservableCollection<AdminClassUsers>();
                //добавление товаров в список (из листа в обсерабл)
                var local = db.AccountList();
                Random rnd = new Random();
                int i = 0;
                foreach (var item in local)
                {
                    i++;
                    members.Add(new AdminClassUsers
                    {
                        account = item,
                        Number = i,
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256))),
                        NameB = item.Name.Substring(0, 1),
                    });
                }

                DataGridtable.ItemsSource = members;
                CountRezultTbx.Text = "Результатов: " + members.Count;

                List<string> filter = new List<string>();
                filter = (List<string>)db.AccountList().Select(x => x.Post).Distinct().ToList();

                multicombobox.ItemsSource = filter;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;

            }
        }


        private void EditTovar(object sender, RoutedEventArgs e)
        {
            try
            {
                AdminClassUsers AccForEditing = (AdminClassUsers)(sender as FrameworkElement).DataContext;
                EditAccWindow edt = new EditAccWindow(AccForEditing.account); //создать окно редактирования
                edt.Closing += Adt_Closing;
                edt.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void Adt_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                DataGridtable.ItemsSource = null;
                Loading();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void RemoveAcc(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult rez = MessageBox.Show("Вы точно хотите удалить данного пользователя?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (rez == MessageBoxResult.Yes)
                {
                    AdminClassUsers AccForDeleting = (AdminClassUsers)(sender as FrameworkElement).DataContext;

                    db = new APIClass();
                    var del = db.AccountList().Where(x => x.Account_id == AccForDeleting.account.Account_id).First();

                    var ISshift = db.ShiftList().FirstOrDefault(x => x.Cashier_id == del.Account_id);

                    if (ISshift != null)
                    {
                        MessageBox.Show("Данный пользователь не может быть удален, пока он взаимодействует с информацией о продажах.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string rezultat = db.DeleteAccount(del.Account_id);

                    MessageBox.Show("Успешно!", "Уведомление", MessageBoxButton.OK);
                    Loading();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        /// <summary>
        /// для фильтра выбрали какой-то тип акк-та
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            catch (Exception ex)
            {
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
                List<AdminClassUsers> search = productsss.Where(x => x.account.Surname.ToLower().Contains(txtSearch.Text.ToLower()) ||
                x.account.Name.ToLower().Contains(txtSearch.Text.ToLower()) || x.account.Patronymic.ToLower().Contains(txtSearch.Text.ToLower()) ||
                x.account.Phone.ToLower().Contains(txtSearch.Text.ToLower()) || x.account.Email.ToLower().Contains(txtSearch.Text.ToLower())).ToList();

                DataGridtable.ItemsSource = search;
                CountRezultTbx.Text = "Результатов: " + search.Count;
                if (txtSearch.Text == "" || txtSearch.Text == null)
                {
                    DataGridtable.ItemsSource = productsss;
                    CountRezultTbx.Text = "Результатов: " + productsss.Count;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            #endregion

        }

        private void SearchTextBox(object sender, TextChangedEventArgs e)
        {
            try
            {

                #region поиск
                List<AdminClassUsers> search = members.Where(x => x.account.Surname.ToLower().Contains(txtSearch.Text.ToLower()) ||
              x.account.Name.ToLower().Contains(txtSearch.Text.ToLower()) || x.account.Patronymic.ToLower().Contains(txtSearch.Text.ToLower()) ||
              x.account.Phone.ToLower().Contains(txtSearch.Text.ToLower()) || x.account.Email.ToLower().Contains(txtSearch.Text.ToLower())).ToList();

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
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


    }
}

