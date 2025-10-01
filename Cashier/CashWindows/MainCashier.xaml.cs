using Google.Protobuf.WellKnownTypes;
using StoreSystem.Auth_Registr;
using StoreSystem.Auth_Registr.UserControls;
using StoreSystem.Cashier.Refund;
using StoreSystem.Classes;
using StoreSystem.ConnectToDB;
using StoreSystem.ConnectToDB.Model;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Xps.Packaging;
using Yandex.Checkout.V3;

namespace StoreSystem.Cashier.CashWindows
{
    /// <summary>
    /// Логика взаимодействия для MainCashier.xaml
    /// </summary>
    public partial class MainCashier : Window
    {
        public ObservableCollection<string> categories = new ObservableCollection<string>(); //список категорий
        private APIClass db;
        private List<CashierLbx> kassa = new List<CashierLbx>(); // для кассы
        private List<DatagridSklad> tovarsCategoryes = new List<DatagridSklad>(); // после выбора категории выводим товары этой категории
        private List<DatagridSklad> products = new List<DatagridSklad>(); // товары склада (для поиска)
        Account account;
        List<Tovar> t = new List<Tovar>();
        List<Sklad> s = new List<Sklad>();
        Shift shift;


        int countNal = 0;  //сколько продал налом/безналом
        int countBeznal = 0;
       
        int summaNal = 0; //на какую сумму продал налом/безналом
        int summaBeznal = 0;

        int countRefundNal = 0; //сколько было возвратов налом/безналом
        int countRefundBeznal = 0;
        
        int summaRefundNal = 0;  //сумма возвратов налом/безналом
        int summaRefundBeznal = 0;


        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private int timeForTimer = 1;


        int k = 0;
        public MainCashier(Account acc)
        {
            InitializeComponent();
            try
            {
                this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

                db = new APIClass();
                account = acc;
                IsEnabled = false;
                CashierShift.Date_Start = null;
                CashierShift.MoneyInCashMachine = 0;
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


        /// <summary>
        /// таймер тик
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (timeForTimer == 0) // обновляем окно
                {
                    IsEnabled = true;
                    timer.Stop();
                    OpenShift shift = new OpenShift();
                    shift.Closing += Shift_Closing;
                    shift.ShowDialog();
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


        /// <summary>
        /// после исчезновения окна закрываем его и открываем окно входа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelAnim_Completed(object sender, EventArgs e)
        {
            Reg_Auth ra = new Reg_Auth();
            Close();
            ra.Show();
        }




        /// <summary>
        /// закрыли окно открытия смены, создание смены
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shift_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (CashierShift.Date_Start == null) //если просто вышли из окна, то выходим с аккаунта
                {
                    DoubleAnimation cancelAnim = new DoubleAnimation();
                    cancelAnim.From = 1;
                    cancelAnim.To = 0;
                    cancelAnim.Duration = TimeSpan.FromSeconds(0.4);
                    cancelAnim.Completed += cancelAnim_Completed;
                    BeginAnimation(Window.OpacityProperty, cancelAnim);
                }
                else
                {
                    Loading();

                    shift = new Shift()
                    {
                        Cashier_id = account.Account_id,
                        Date_Start = (CashierShift.Date_Start.Value.ToUniversalTime()),
                        Date_End = (CashierShift.Date_Start.Value.ToUniversalTime().AddHours(1)),
                        Summary = CashierShift.MoneyInCashMachine
                    };

                    try
                    {
                      
                        string rez = db.AddShift(shift);
                        shift = db.ShiftList().Where(x => x.Cashier_id == account.Account_id).ToList().Last();
                        if (rez != "Успех") throw new Exception();

                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Ошибка связи с сервером!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    CheckForPrint checkForPrint = new CheckForPrint(account, DateTime.Now, db.ShiftList().Last().Shift_id);
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }




        /// <summary>
        /// загрузка данных окна
        /// </summary>
        public void Loading()
        {
            try
            {
                t = db.TovarList();
                s = db.SkladList();

                ListTovarsdg.Visibility = Visibility.Hidden;
                ListTovarsdg.Height = 0;

                scroller.Visibility = Visibility.Visible;
                scroller.Height = 892;

                backButton.Visibility = Visibility.Hidden;
                #region товары для поиска
                var local = s;
                Random rnd = new Random();
                products = new List<DatagridSklad>();
                foreach (var item in local)
                {

                    products.Add(new DatagridSklad
                    {
                        sklad = item,
                        tovar = t.Where(x => x.Tovar_id == item.Tovar_id).First(),
                        Number = t.Where(x => x.Tovar_id == item.Tovar_id).First().Artikul,
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256))),
                        NameTovar = t.Where(x => x.Tovar_id == item.Tovar_id).First().Name.Substring(0, 1)
                    });
                }

                #endregion


                OrderListLbx.ItemsSource = kassa;

                //получаем последнюю продажу в таблице селтовар, делаем инкрементацию
                int st = db.SellTovarsList().Last().SellTovars_id;
                NumberOrderLabel.Content = "Чек № " + (st + 1);
                CountLabel.Content = "Итоговая сумма: 0 руб.";
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

      

       

        /// <summary>
        /// переход по выбранной категории
        /// </summary>
        private void SelectedCategories(object sender, MouseButtonEventArgs e)
        {
            try
            {
                tovarsCategoryes.Clear();
                InfoCard sel = sender as InfoCard;

                #region список товаров на складе

                List<Tovar> tovarInSklad = new List<Tovar>(); //список товаров на складе


                foreach (var item in t)
                {
                    var fff = s.Where(x => x.Tovar_id == item.Tovar_id).FirstOrDefault();
                    if (fff != null) tovarInSklad.Add(item);
                }





                #endregion

                // var local = db.SkladList().Where(x => x.Tovar.Type_tovar == sel.Title).ToList(); 
                var local1 = tovarInSklad.Where(x => x.Type_tovar == sel.Title).ToList(); //товары на складе выбранной категории
                List<Sklad> local = new List<Sklad>();

                foreach (var item in local1)
                {
                    var fff = s.Where(x => x.Tovar_id == item.Tovar_id).FirstOrDefault();
                    if (fff != null) local.Add(fff);
                }



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


                List<Tovar> tovasInFIFO = new List<Tovar>(); //равносильно fifo.include(x => x.tovar)
                foreach (var itemss in fifo)
                {
                    var fff = t.Where(x => x.Tovar_id == itemss.Tovar_id).FirstOrDefault();
                    if (fff != null) tovasInFIFO.Add(fff);
                }

                foreach (var item in local)
                {
                    var iitem = t.Where(x => x.Tovar_id == item.Tovar_id).First(); //товар из цикла, который на складе 

                    var deleteTOVARS = tovasInFIFO.Where(x => x.Artikul == iitem.Artikul).ToList();

                    #region получаем все склады из списка delete
                    List<Sklad> delete = new List<Sklad>();
                    foreach (var itemss in deleteTOVARS)
                    {
                        var fff = s.Where(x => x.Tovar_id == itemss.Tovar_id).FirstOrDefault();
                        if (fff != null) delete.Add(fff);
                    }

                    #endregion



                    if (delete.Count == 0) continue;

                    Sklad tovarPoidet = delete[0];

                    var tovarPoidetTOVAR = t.Where(x => x.Tovar_id == tovarPoidet.Tovar_id).FirstOrDefault();



                    //находим товар, который был произведен раньше всех
                    foreach (var item2 in delete)
                    {
                        var tovarchik = t.Where(x => x.Tovar_id == item2.Tovar_id).First();
                        if (tovarPoidetTOVAR.Production_date > tovarchik.Production_date)
                        {
                            tovarPoidet = item2;
                        }
                    }
                    var dobavit = fifo.Where(x => x.Sklad_id == tovarPoidet.Sklad_id).First();
                    foreach (var item3 in delete)
                    {
                        var del = fifo.Where(x => x.Sklad_id == item3.Sklad_id).FirstOrDefault();
                        if (del != null)
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
                        tovar = t.Where(x => x.Tovar_id == item.Tovar_id).First(),
                        Number = t.Where(x => x.Tovar_id == item.Tovar_id).First().Artikul,
                        BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256))),
                        NameTovar = t.Where(x => x.Tovar_id == item.Tovar_id).First().Name.Substring(0, 1)
                    });
                }
                ListTovarsdg.Visibility = Visibility.Visible;
                ListTovarsdg.Height = 906;
                ListTovarsdg.ItemsSource = tovarsCategoryes;

                backButton.Visibility = Visibility.Visible;

                scroller.Visibility = Visibility.Hidden;
                scroller.Height = 0;
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
                List<DatagridSklad> search = products.Where(x => x.tovar.Artikul.ToString().Contains(txtSearch.Text) || x.tovar.Name.ToString().ToLower().Contains(txtSearch.Text.ToLower())).ToList();


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
                    var delete = fifo.Where(x => x.tovar.Artikul == item.tovar.Artikul).ToList();

                    if (delete.Count == 0) continue;

                    DatagridSklad tovarPoidet = delete[0];

                    //находим товар, который был произведен раньше всех
                    foreach (var item2 in delete)
                    {
                        if (tovarPoidet.tovar.Production_date > item2.tovar.Production_date)
                        {
                            tovarPoidet = item2;
                        }
                    }
                    var dobavit = fifo.Where(x => x.sklad.Sklad_id == tovarPoidet.sklad.Sklad_id).First();
                    foreach (var item3 in delete)
                    {
                        var del = fifo.Where(x => x.sklad.Sklad_id == item3.sklad.Sklad_id).FirstOrDefault();
                        if (del != null)
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
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }



        /// <summary>
        /// кнопка ПРОДАТЬ
        /// </summary>
    
        private void SellOrders(object sender, RoutedEventArgs e)
        {
            try
            {
                if (kassa.Count == 0)
                {
                    MessageBox.Show("Выберите хотя бы один товар!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                t = db.TovarList();
                s = db.SkladList();
                var st = db.SellTovarsList();
                #region проверка, что выбранного товара не больше, чем на складе есть
                bool b = true;
                foreach (var item in kassa)
                {
                    var skl = s.Where(x => x.Sklad_id == item.sklad.Sklad_id).First();
                    if ((skl.Count - item.Count) < 0)
                    {
                        var name = t.Where(x => x.Tovar_id == skl.Tovar_id).First().Name;
                        MessageBox.Show("Товара " + name + " в таком кол-ве нет на складе!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
                        b = false;
                    }
                }
                if (b == false)
                {
                    return;
                }
                #endregion



                //выбираем способ оплаты
                PaymentMethodWindow paymentMethod = new PaymentMethodWindow();
                paymentMethod.Closed += Sale_Method;
                paymentMethod.ShowDialog();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }



        /// <summary>
        /// добавляем в бд продажу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Sale_Method(object? sender, EventArgs? e)
        {
            try
            {
                int summa = 0;
                if (sender != null && e != null) //до совершения оплаты безналом
                {
                    if (StaticClassForUrlCardPayment.PaymentMethod == 1) //безнал
                    {
                        //вызываем окно оплаты безналом
                        summa = 0;
                        string description = "";
                        foreach (var item in kassa)
                        {
                            summa += item.Count * item.Price;
                            description += item.tovar.Artikul + " - " + item.sklad.Selling_priсe * item.Count + "; ";
                        }
                        CashlessPaymentWindow cashlessPayment = new CashlessPaymentWindow(summa, description);
                        cashlessPayment.Closed += IsPaymentSuccessful;
                        await Dispatcher.InvokeAsync(() => cashlessPayment.ShowDialog());
                        return;
                    }
                    //для налички
                    else if (StaticClassForUrlCardPayment.PaymentMethod == 2)
                    {
                        summa = 0;
                        string description = "";
                        foreach (var item in kassa)
                        {
                            summa += item.Count * item.Price;
                            description += item.tovar.Artikul + " - " + item.sklad.Selling_priсe * item.Count + "; ";
                        }

                        var forPayment = db.Beznal(summa, description).ToString();
                        StaticClassForUrlCardPayment.PaymentId = forPayment.Substring(48);


                        //подсчет налом кол-ва и суммы 
                        foreach (var item in kassa)
                        {
                            countNal += item.Count;
                            summaNal += item.Count * item.Price;
                        }

                        CashierShift.MoneyInCashMachine += summaNal;
                    }
                    else if (StaticClassForUrlCardPayment.PaymentMethod == -1) //способ оплаты не выбрали, нажали ОТМЕНА
                    {
                        return;
                    }

                    else if (StaticClassForUrlCardPayment.PaymentMethod == -2) //произошла ошибка при оплате, сбой юкассы
                    {
                        MessageBox.Show("Произошла непредвиденная ошибка! Пожалуйста, повторите попытку.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                #region Оформление продажи, основной код

                summa = 0;
                foreach (var itemC in kassa)
                {
                    summa += itemC.Count * itemC.Price;
                }



                t = db.TovarList();
                s = db.SkladList();
                var st = db.SellTovarsList();
                #region проверка, что выбранного товара не больше, чем на складе есть
                bool b = true;
                foreach (var item in kassa)
                {
                    var skl = s.Where(x => x.Sklad_id == item.sklad.Sklad_id).First();
                    if ((skl.Count - item.Count) < 0)
                    {
                        var name = t.Where(x => x.Tovar_id == skl.Tovar_id).First().Name;
                        MessageBox.Show("Товара " + name + " в таком кол-ве нет на складе!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    var skl = s.Where(x => x.Sklad_id == item.sklad.Sklad_id).First();
                    skl.Count -= item.Count;
                    db.UpdateSklad(skl);
                }

                SellTovars sellTovars = new SellTovars();
                sellTovars.Kassir_id = account.Account_id;
              //var c = DateTime.UtcNow.ToUniversalTime().AddHours(5);
                sellTovars.Date_sell = DateTime.Now.ToUniversalTime(); //.AddHours((DateTime.Now - DateTime.UtcNow).Hours + 1)
                sellTovars.PaymentId = StaticClassForUrlCardPayment.PaymentId;
                sellTovars.Summary = summa;
                db.AddSellTovars(sellTovars);

                st = db.SellTovarsList();
                List<Sell> sell = new List<Sell>();
                var f = st.Last().SellTovars_id;
                int summary = 0;
                foreach (var item in kassa)
                {
                    var ttovar = t.Where(x => x.Tovar_id == item.sklad.Tovar_id).First();
                    summary = item.Count * item.sklad.Selling_priсe;

                    db.AddSell(new Sell
                    {

                        Tovar_id = t.Where(x => x.Tovar_id == ttovar.Tovar_id).First().Tovar_id,
                        Count = item.Count,
                        SellTovars_id = st.Last().SellTovars_id,
                        Summary = summary
                    });
                }


                MessageBoxResult rezultatik = MessageBox.Show("Продано!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                if (rezultatik == MessageBoxResult.OK)
                {

                    #region Печать чека
                    CheckForPrint printCheck = new CheckForPrint(account, kassa, sellTovars.Date_sell.AddHours(5), f, summa);
                    //   printCheck.ShowDialog();
                    #endregion





                    //ОПУСТОШАЕМ КАССУ
                    OrderListLbx.ItemsSource = null;
                    kassa.Clear();


                    // db = new DataContext();

                    ListTovarsdg.Visibility = Visibility.Hidden;
                    ListTovarsdg.Height = 0;

                    scroller.Visibility = Visibility.Visible;
                    scroller.Height = 892;

                    backButton.Visibility = Visibility.Hidden;

                    #region товары для поиска
                    s = db.SkladList();
                    t = db.TovarList();
                    Random rnd = new Random();
                    products = new List<DatagridSklad>();
                    foreach (var item in s)
                    {
                        products.Add(new DatagridSklad
                        {
                            sklad = item,
                            tovar = t.Where(x => x.Tovar_id == item.Tovar_id).First(),
                            Number = t.Where(x => x.Tovar_id == item.Tovar_id).First().Artikul,
                            BgColor = new SolidColorBrush(Color.FromArgb((byte)rnd.Next(255, 256), (byte)rnd.Next(255, 256), (byte)rnd.Next(100, 156), (byte)rnd.Next(100, 256))),
                            NameTovar = t.Where(x => x.Tovar_id == item.Tovar_id).First().Name.Substring(0, 1)
                        });
                    }

                    #endregion

                    OrderListLbx.ItemsSource = kassa;
                    ListTovarsdg.ItemsSource = null;

                    //получаем последнюю продажу в таблице селтовар, делаем инкрементацию
                    int stInt = db.SellTovarsList().Last().SellTovars_id;
                    NumberOrderLabel.Content = "Чек № " + (stInt + 1);
                    CountLabel.Content = "Итоговая сумма: 0 руб.";
                    k = 0;


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




        /// <summary>
        /// успешно ли прошла оплата безналом
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsPaymentSuccessful(object? sender, EventArgs e)
        {
            try
            {
                if (StaticClassForUrlCardPayment.URL.Contains("https://yoomoney.ru/checkout/payments/v2/success")) //успешно
                {
                    StaticClassForUrlCardPayment.PaymentMethod = 2;

                    //подсчет безналом кол-ва и суммы 
                    foreach (var item in kassa)
                    {
                        countBeznal += item.Count;
                        summaBeznal += item.Count * item.Price;
                    }

                    Sale_Method(null, null);
                }

                else if (StaticClassForUrlCardPayment.URL.Contains("http://localhost:7114/thankyou")) //вышли из оплаты
                {
                    StaticClassForUrlCardPayment.PaymentMethod = -1;
                    return;
                }


                else //какая-то ошибка
                {
                    MessageBox.Show("Произошла ошибка! Пожалуйста, попробуйте ещё раз!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    StaticClassForUrlCardPayment.PaymentMethod = -2;
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

        private void Back(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

    
        
        /// <summary>
        /// кнопка ОТМЕНИТЬ
        /// </summary>
        private void CancelOrders(object sender, RoutedEventArgs e)
        {
            try
            {
                //ОПУСТОШАЕМ КАССУ
                OrderListLbx.ItemsSource = null;
                kassa.Clear();
                CountLabel.Content = "0 руб.";
                k = 0;
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        /// <summary>
        /// выбрали товар, добавляем его в список покупок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddInOrder(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DatagridSklad Tov = (DatagridSklad)ListTovarsdg.SelectedItem;
                if (Tov == null)
                {
                    MessageBox.Show("Ошибка! Попробуйте ещё раз!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
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
                    tovar = t.Where(x => x.Tovar_id == Tov.tovar.Tovar_id).First(),
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
                CountLabel.Content = "Итоговая сумма: " + sum.ToString() + " руб.";
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
            try
            {
                MessageBoxResult rez = MessageBox.Show("Вы точно хотите удалить данный товар?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (rez == MessageBoxResult.Yes)
                {
                    CashierLbx TovarForDelete = (CashierLbx)(sender as FrameworkElement).DataContext;


                    var del = kassa.Where(x => x.sklad == TovarForDelete.sklad).First();
                    kassa.Remove(del);
                    k = 0;
                    foreach (var item in kassa)
                    {
                        k++;
                        item.Number = k;
                    }

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
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

   
        /// <summary>
        /// печать закрытия кассы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                MessageBoxResult rez = MessageBox.Show("Вы точно хотите закрыть смену?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (rez == MessageBoxResult.Yes)
                {
                    #region закрываем смену


                    Shift? shiftInDB = db.ShiftList().FirstOrDefault(x => x.Cashier_id == shift.Cashier_id &&
                    x.Shift_id == shift.Shift_id);

                    if (shiftInDB == null)
                    {
                        MessageBox.Show("Ошибка! Повторите попытку.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    shiftInDB.Summary = CashierShift.MoneyInCashMachine;
                    shiftInDB.Date_End = DateTime.Now.ToUniversalTime();
                    db.UpdateShift(shiftInDB);

                    //логику подсчета всего, на печать

                    List<SellTovars> stEnd = db.SellTovarsList().Where(x => x.Kassir_id == account.Account_id &&   //все продажи кассира за смену
                   x.Date_sell >= shiftInDB.Date_Start && x.Date_sell <= shiftInDB.Date_End).ToList();

                    List<int> st_id = stEnd.Select(x => x.SellTovars_id).ToList();
                    List<Sell> sellEnd = db.SellList().Where(x => st_id.Contains(x.SellTovars_id)).ToList();  //состав чеков 


                    //закрытие смены
                    CheckForPrint shiftEND = new CheckForPrint(account, shiftInDB, st_id, sellEnd, summaNal, summaBeznal, countNal, countBeznal);


                    #region выйти из аккаунта в окно авторизации
                    DoubleAnimation cancelAnim = new DoubleAnimation();
                    cancelAnim.From = 1;
                    cancelAnim.To = 0;
                    cancelAnim.Duration = TimeSpan.FromSeconds(0.4);
                    cancelAnim.Completed += cancelAnim_Completed;
                    BeginAnimation(Window.OpacityProperty, cancelAnim);
                    #endregion

                    #endregion
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        //оформление возврата
        private void RefundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MainRefundWindow refund = new MainRefundWindow(account);
                refund.Closing += Refund_Closing;
                refund.ShowDialog();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void Refund_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Loading();
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
