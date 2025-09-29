using StoreSystem.Cashier.CashWindows;
using StoreSystem.Classes;
using StoreSystem.ConnectToDB;
using StoreSystem.ConnectToDB.Model;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
using Org.BouncyCastle.Asn1.X500;
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

namespace StoreSystem.Finance.FinPages
{
    /// <summary>
    /// Логика взаимодействия для ShiftsCashierPage.xaml
    /// </summary>
    public partial class ShiftsCashierPage : Page
    {
       
        ObservableCollection<ShiftClassAnalyst> shiftlist = new ObservableCollection<ShiftClassAnalyst>(); //смены
        private APIClass db;
        List<Tovar> t;
        List<Sklad> s;
        List<Shift> sh;
        List<Account> ac;

        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private int timeForTimer = 90;
        public ShiftsCashierPage()
        {
            InitializeComponent();
            try
            {
                backButton.Visibility = Visibility.Hidden;
                StackpanelshiftDescription.Visibility = Visibility.Hidden;
                DataGridtable.Visibility = Visibility.Visible;
                CountRezultTbx.Visibility = Visibility.Visible;

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

                #region взаимодействуем с таблицами
                t = db.TovarList();
                s = db.SkladList();
                ac = db.AccountList();
                sh = db.ShiftList();
                #endregion
                shiftlist = new ObservableCollection<ShiftClassAnalyst>();

                //добавление товаров в список (из листа в обсерабл)
                string stat = "";
                var local = sh;
                Random rnd = new Random();
                int i = 0;
                if (local != null)
                    foreach (var item in local)
                    {
                        i++;
                        shiftlist.Add(new ShiftClassAnalyst
                        {
                            shift = item,
                            Number = i,
                            BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256)))

                        });
                    }


                DataGridtable.ItemsSource = shiftlist;


                CountRezultTbx.Text = "Результатов: " + shiftlist.Count;

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
                StackpanelshiftDescription.Visibility = Visibility.Visible;
                DataGridtable.Visibility = Visibility.Hidden;
                CountRezultTbx.Visibility = Visibility.Hidden;




                Shift shift = ((ShiftClassAnalyst)DataGridtable.SelectedItem).shift;

                Account account = ac.First(x => x.Account_id == shift.Cashier_id);

                if (account.Patronymic == null)
                {
                    AccountLabel.Content = "Кассир: " + account.Surname + " " + account.Name.Substring(0, 1) + ".";
                }
                else
                {
                    AccountLabel.Content = "Кассир: " + account.Surname + " " + account.Name.Substring(0, 1) + ". " + account.Patronymic.Substring(0, 1) + ".";
                }


                List<SellTovars> st = db.SellTovarsList().Where(x => x.Date_sell >= shift.Date_Start && x.Date_sell <= shift.Date_End && x.Kassir_id == account.Account_id).ToList();
                List<Sell> sel = db.SellList();
                List<Sell> needsel = new List<Sell>(); //список нужных sell

                int count = 0; //кол-во проданных товаров
                int summa = 0; //сумма проданных товаров

                foreach (var item in st)
                {
                    var loc = sel.Where(x => x.SellTovars_id == item.SellTovars_id).ToList();
                    foreach (var item2 in loc)
                    {
                        needsel.Add(item2);
                        summa += item2.Summary;
                    }
                }


                List<Refund> refundlist = db.RefundList().
                    Where(x => x.Cashier_id == account.Account_id &&
                    x.Date >= shift.Date_Start &&
                    x.Date <= shift.Date_End).ToList(); //возвраты, которые делал кассир за свою смену

                int countRef = 0; //кол-во возвращенных товаров
                int summaRef = 0; //сумма возвращенных товаров

                foreach (var item in refundlist)
                {
                    countRef += item.Count;
                    summaRef += item.Summary;
                }


                Date1Label.Content = "Дата начала смены: " + shift.Date_Start;
                Date2Label.Content = "Дата конца смены: " + shift.Date_End;
                NumberShift1Label.Content = "Номер смены: " + shift.Shift_id;


                RezultCountLabel.Content = "Количество проданных товаров (шт.): " + count;
                RezultSummaLabel.Content = "Сумма проданных товаров (руб.): " + summa;

                RefundSummaLabel.Content = "Сумма товаров, по которым был произведен возврат (руб.): " + summaRef;
                RefundCountLabel.Content = "Количество товаров, по которым был произведен возврат (шт.): " + countRef;

                RezultLabel.Content = "Остаток наличных средств в кассовом аппарате (руб.): " + shift.Summary;


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
                StackpanelshiftDescription.Visibility = Visibility.Hidden;
                DataGridtable.Visibility = Visibility.Visible;
                CountRezultTbx.Visibility = Visibility.Visible;

                DataGridtable.ItemsSource = null;
                SearchTextBox(null, null);
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
                var searchAcc = ac.Where(x => x.Surname.ToLower().Contains(txtSearch.Text.ToLower()) ||
                x.Name.ToLower().Contains(txtSearch.Text.ToLower()) ||
                x.Patronymic.ToLower().Contains(txtSearch.Text.ToLower())
                ).ToList(); //все пользователи, где встречаются введенные буквы


                List<Shift> search = new List<Shift>();
                foreach (var item in searchAcc)
                {
                    var forAdd = shiftlist.Where(x => x.shift.Cashier_id == item.Account_id).ToList();
                    foreach (var item2 in forAdd)
                    {
                        search.Add(item2.shift);
                    }
                }

                ObservableCollection<ShiftClassAnalyst> rezSearch = new ObservableCollection<ShiftClassAnalyst>();

                int i = 0;
                Random rnd = new Random();
                foreach (var item in search)
                {
                    i++;
                    rezSearch.Add(new ShiftClassAnalyst
                    {
                        shift = item,
                        Number = i,
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256)))
                    });
                }
                DataGridtable.ItemsSource = null;
                DataGridtable.ItemsSource = rezSearch;
                CountRezultTbx.Text = "Результатов: " + search.Count;

                if (txtSearch.Text == "" || txtSearch.Text == null || txtSearch.Text.Trim(' ').Length == 0)
                {
                    DataGridtable.ItemsSource = shiftlist;
                    CountRezultTbx.Text = "Результатов: " + shiftlist.Count;
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
    }

    public class ShiftClassAnalyst
    {
        public Shift shift { get; set; }
        public int Number { get; set; }
        public Brush BgColor { get; set; }
    }



}
