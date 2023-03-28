using Kursovoi.Auth_Registr.UserControls;
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
using System.Windows.Shapes;

namespace Kursovoi.Cashier.CashWindows
{
    /// <summary>
    /// Логика взаимодействия для MainCashier.xaml
    /// </summary>
    public partial class MainCashier : Window
    {
        public ObservableCollection<string> categories = new ObservableCollection<string>(); //список категорий
        private DataContext db = new DataContext();
        private List<CashierLbx> kassa = new List<CashierLbx>(); // для кассы
        private List<DatagridSklad> tovarsCategoryes = new List<DatagridSklad>(); // после выбора категории выводим товары этой категории
        private List<DatagridSklad> products = new List<DatagridSklad>(); // товары склада (для поиска)
        Account account;

        int k = 0;
        public MainCashier(Account acc)
        {
            InitializeComponent();
            account = acc;
            #region взаимодействуем с таблицами, между которым установлена нужная нам связь
            List<Tovar> t = db.Tovar.ToList();
            List<Sklad> s = db.Sklad.ToList();
            #endregion

            ListTovarsdg.Visibility = Visibility.Hidden;
            ListTovarsdg.Height = 0;

            scroller.Visibility = Visibility.Visible;
            scroller.Height = 892;

            backButton.Visibility = Visibility.Hidden;
            #region товары для поиска
            var local = db.Sklad.ToList();
            Random rnd = new Random();
            foreach (var item in local)
            {
                products.Add(new DatagridSklad
                {
                    sklad = item,
                    Number = item.Tovar.Artikul,
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256))),
                    NameTovar = item.Tovar.Name.Substring(0, 1)
                });
            }

            #endregion


            OrderListLbx.ItemsSource = kassa;

            //получаем последнюю продажу в таблице селтовар, делаем инкрементацию
            List<SellTovars> st = db.SellTovars.ToList();
            NumberOrderLabel.Content = "Чек № " + (st.Last().SellTovars_id + 1);
            CountLabel.Content = "Итоговая сумма: 0 руб.";
        }

        /// <summary>
        /// переход по выбранной категории
        /// </summary>
        private void SelectedCategories(object sender, MouseButtonEventArgs e)
        {
            tovarsCategoryes.Clear();
            InfoCard sel = sender as InfoCard;

            #region взаимодействуем с таблицами, между которым установлена нужная нам связь
            List<Tovar> t = db.Tovar.ToList();
            List<Sklad> s = db.Sklad.ToList();
            #endregion

            var local = db.Sklad.Where(x => x.Tovar.Type_tovar == sel.Title).ToList(); //товары на складе выбранной категории
            Random rnd = new Random();

            #region если кол-во товара=0, то удаляем его из этого списка
            local = local.Where(x => x.Count != 0).ToList();
            #endregion

            #region ФИФО

            List<Sklad> fifo = new List<Sklad>();
            foreach (var item in local)
            {
                fifo.Add(item);
            }

            //товары с одинаковыми артикалами =>1 товар, который был произведен раньше всех

            //логика: находим все товары с одинаковыми артикулами, среди них ищем товар, который был произведен раньше всех, сохраняем его, удаляем все товары с одинаковыми
            //артикулами, и обратно добавляем ранее сохраненный товар

            foreach (var item in local)
            {
                var delete = fifo.Where(x => x.Tovar.Artikul == item.Tovar.Artikul).ToList();

                if (delete.Count == 0) continue;

                Sklad tovarPoidet = delete[0];

                //находим товар, который был произведен раньше всех
                foreach (var item2 in delete)
                {
                    if (tovarPoidet.Tovar.Production_date > item2.Tovar.Production_date)
                    {
                        tovarPoidet = item2;
                    }
                }
                var dobavit = fifo.Where(x => x.Sklad_id == tovarPoidet.Sklad_id).First();
                foreach (var item3 in delete)
                {
                    var del = fifo.Where(x => x.Sklad_id == item3.Sklad_id).First();
                    fifo.Remove(del);
                }
                fifo.Add(dobavit);
            }

            local = fifo;

            #endregion



            foreach (var item in local)
            {
                tovarsCategoryes.Add(new DatagridSklad
                {
                    sklad = item,
                    Number = item.Tovar.Artikul,
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256))),
                    NameTovar = item.Tovar.Name.Substring(0, 1)
                });
            }
            ListTovarsdg.Visibility = Visibility.Visible;
            ListTovarsdg.Height = 906;
            ListTovarsdg.ItemsSource = tovarsCategoryes;

            backButton.Visibility = Visibility.Visible;

            scroller.Visibility = Visibility.Hidden;
            scroller.Height = 0;
        }





        private void SearchTextBox(object sender, TextChangedEventArgs e)
        {
            List<DatagridSklad> search = products.Where(x => x.sklad.Tovar.Artikul.ToString().Contains(txtSearch.Text)).ToList();


            #region если кол-во товара=0, то удаляем его из этого списка
            search = search.Where(x => x.sklad.Count != 0).ToList();

            #endregion


            #region ФИФО

            List<DatagridSklad> fifo = new List<DatagridSklad>();
            foreach (var item in search)
            {
                fifo.Add(item);
            }

            //товары с одинаковыми артикалами =>1 товар, который был произведен раньше всех

            //логика: находим все товары с одинаковыми артикулами, среди них ищем товар, который был произведен раньше всех, сохраняем его, удаляем все товары с одинаковыми
            //артикулами, и обратно добавляем ранее сохраненный товар

            foreach (var item in search)
            {
                var delete = fifo.Where(x => x.sklad.Tovar.Artikul == item.sklad.Tovar.Artikul).ToList();

                if (delete.Count == 0) continue;

                DatagridSklad tovarPoidet = delete[0];

                //находим товар, который был произведен раньше всех
                foreach (var item2 in delete)
                {
                    if (tovarPoidet.sklad.Tovar.Production_date > item2.sklad.Tovar.Production_date)
                    {
                        tovarPoidet = item2;
                    }
                }
                var dobavit = fifo.Where(x => x.sklad.Sklad_id == tovarPoidet.sklad.Sklad_id).First();
                foreach (var item3 in delete)
                {
                    var del = fifo.Where(x => x.sklad.Sklad_id == item3.sklad.Sklad_id).First();
                    fifo.Remove(del);
                }
                fifo.Add(dobavit);
            }

            search = fifo;

            #endregion

            ListTovarsdg.ItemsSource = search;
            if (txtSearch.Text == "" || txtSearch.Text == null)
            {
                Back(null, null);
                return;
            }

            ListTovarsdg.Visibility = Visibility.Visible;
            ListTovarsdg.Height = 906;

            backButton.Visibility = Visibility.Visible;

            scroller.Visibility = Visibility.Hidden;
            scroller.Height = 0;

        }



        /// <summary>
        /// кнопка ПРОДАТЬ
        /// </summary>
        private void SellOrders(object sender, RoutedEventArgs e)
        {

            db = new DataContext();

            #region взаимодействуем с таблицами, между которым установлена нужная нам связь
            List<Tovar> t = db.Tovar.ToList();
            List<Sklad> s = db.Sklad.ToList();
            List<Account> ac = db.Account.ToList();
            #endregion



            #region проверка, что выбранного товара не больше, чем на складе есть
            bool b = true;
            foreach (var item in kassa)
            {
                var skl = db.Sklad.Where(x => x.Sklad_id == item.sklad.Sklad_id).First();
                if ((skl.Count - item.Count) < 0)
                {
                    MessageBox.Show("Товара " + item.sklad.Tovar.Name + " в таком кол-ве нет на складе!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
                    b = false;
                }
            }
            if (b == false)
            {
                return;
            }
            #endregion


            foreach (var item in kassa)
            {
                var skl = db.Sklad.Where(x => x.Sklad_id == item.sklad.Sklad_id).First();
                skl.Count -= item.Count;
            }


            SellTovars sellTovars = new SellTovars();
            sellTovars.Account = db.Account.Where(x => x.Account_id == account.Account_id).First();
            sellTovars.Date_sell = DateTime.Now;
            db.SellTovars.Add(sellTovars);

            //db.SaveChanges();
            //db = new DataContext();

            //#region взаимодействуем с таблицами, между которым установлена нужная нам связь
            //List<Tovar> t23 = db.Tovar.ToList();
            //List<Sklad> s23 = db.Sklad.ToList();
            //List<Account> ac23 = db.Account.ToList();
            //#endregion


            List<Sell> sell = new List<Sell>();


            foreach (var item in kassa)
            {
                db.Sell.Add(new Sell
                {
                    Tovar = db.Tovar.Where(x => x.Tovar_id == item.sklad.Tovar.Tovar_id).First(),
                    Count = item.Count,
                    SellTovars = sellTovars
                });
            }
            db.SaveChanges();

            
            //Date_sell = DateTime.Now,
            //        Account = db.Account.Where(x => x.Account_id == account.Account_id).First()
            MessageBox.Show("Продано!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
         
            //ОПУСТОШАЕМ КАССУ
            OrderListLbx.ItemsSource = null;
            kassa.Clear();


            db = new DataContext();

            #region взаимодействуем с таблицами, между которым установлена нужная нам связь
            List<Tovar> t2 = db.Tovar.ToList();
            List<Sklad> s2 = db.Sklad.ToList();
            List<Account> ac2 = db.Account.ToList();
            #endregion


            ListTovarsdg.Visibility = Visibility.Hidden;
            ListTovarsdg.Height = 0;

            scroller.Visibility = Visibility.Visible;
            scroller.Height = 892;

            backButton.Visibility = Visibility.Hidden;

            #region товары для поиска
            var local = db.Sklad.ToList();
            Random rnd = new Random();
            foreach (var item in local)
            {
                products.Add(new DatagridSklad
                {
                    sklad = item,
                    Number = item.Tovar.Artikul,
                    BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 256), (byte)rnd.Next(100, 256))),
                    NameTovar = item.Tovar.Name.Substring(0, 1)
                });
            }

            #endregion

            OrderListLbx.ItemsSource = kassa;
            ListTovarsdg.ItemsSource = null;

            //получаем последнюю продажу в таблице селтовар, делаем инкрементацию
            List<SellTovars> st = db.SellTovars.ToList();
            NumberOrderLabel.Content = "Чек № " + (st.Last().SellTovars_id + 1);
            CountLabel.Content = "Итоговая сумма: 0 руб.";

        }

        private void Back(object sender, RoutedEventArgs e)
        {
            //выходим из определенной категории, выводим все категории, скрываем датагрид, появляется скроллер
            ListTovarsdg.ItemsSource = null;
            tovarsCategoryes.Clear();
            backButton.Visibility = Visibility.Hidden;

            ListTovarsdg.Visibility = Visibility.Hidden;
            ListTovarsdg.Height = 0;

            scroller.Visibility = Visibility.Visible;
            scroller.Height = 892;

            txtSearch.Text = null;

        }

        /// <summary>
        /// кнопка ОТМЕНИТЬ
        /// </summary>
        private void CancelOrders(object sender, RoutedEventArgs e)
        {
            //ОПУСТОШАЕМ КАССУ
            OrderListLbx.ItemsSource = null;
            kassa.Clear();
            CountLabel.Content = "0 руб.";
        }


        //выбрали товар, добавляем его в список покупок
        private void AddInOrder(object sender, MouseButtonEventArgs e)
        {
            DatagridSklad Tov = (DatagridSklad)ListTovarsdg.SelectedItem;

            //если мы 2 раза выбираем товар, который уже есть в списке покупок, то мы просто count++ этому товару, а не добавляем его 2 раза в список покупок
            if (kassa.Count != 0)
            {
                var v = kassa.Where(x => x.sklad == Tov.sklad && x.Price == Tov.sklad.Selling_priсe).FirstOrDefault();
                if (v != null)
                {
                    v.Count++;
                    OrderListLbx.ItemsSource = null;
                    OrderListLbx.ItemsSource = kassa;
                    int sum2 = 0;
                    foreach (var itemC in kassa)
                    {
                        sum2 += itemC.Count * itemC.Price;
                    }
                    CountLabel.Content = "Итоговая сумма: " + sum2.ToString() + " руб.";
                    return;
                }
            }

            k++;
            CashierLbx addTov = new CashierLbx
            {
                Number = k,
                sklad = Tov.sklad,
                Price = Tov.sklad.Selling_priсe,
                Count = 1
            };

            kassa.Add(addTov);
            OrderListLbx.ItemsSource = null;
            OrderListLbx.ItemsSource = kassa;

            int sum = 0;
            foreach (var itemC in kassa)
            {
                sum += itemC.Count * itemC.Price;
            }
            CountLabel.Content = "Итоговая сумма: " +sum.ToString() +  " руб.";
        }


        #region изменение кол-ва товара для покупки
        string countNew = string.Empty;
        private void CountChanged(object sender, DataGridRowEditEndingEventArgs e)
        {
            CashierLbx obj = e.Row.Item as CashierLbx;

            var loc = kassa.Where(x => x.sklad == obj.sklad).First();
            loc.Count = int.Parse(countNew);
            int sum = 0;
            foreach (var itemC in kassa)
            {
                sum += itemC.Count * itemC.Price;
            }
            CountLabel.Content = "Итоговая сумма: " + sum.ToString() + " руб.";
        }

        private void OrderListLbx_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;

                    int rowIndex = e.Row.GetIndex();
                    var el = e.EditingElement as TextBox;
                    countNew = el.Text;
                }
            }
        }
        #endregion

        private void RemoveTovar(object sender, RoutedEventArgs e)
        {
            MessageBoxResult rez = MessageBox.Show("Вы точно хотите удалить данный товар?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (rez == MessageBoxResult.Yes)
            {
                CashierLbx TovarForDelete = (CashierLbx)(sender as FrameworkElement).DataContext;
 

                var del = kassa.Where(x => x.sklad == TovarForDelete.sklad).First();
                kassa.Remove(del);

                OrderListLbx.ItemsSource = null;
                OrderListLbx.ItemsSource = kassa;
                int sum = 0;
                foreach (var itemC in kassa)
                {
                    sum += itemC.Count * itemC.Price;
                }
                CountLabel.Content = "Итоговая сумма: " + sum.ToString() + " руб.";
            }
        }

        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
