using DocumentFormat.OpenXml.Office2010.ExcelAc;
using StoreSystem.Cashier.CashWindows;
using StoreSystem.Classes;
using StoreSystem.ConnectToDB.Model;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
using StoreSystem.Skladnoi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace StoreSystem.Cashier.Refund
{

    public class ForOutputTovarsInPaycheck
    {
        public int Number { get; set; }
        public Tovar tovar { get; set; }
        public int Count { get; set; }
        public int Summary { get; set; }
        public int sellTovars_id { get; set; }
    }


    /// <summary>
    /// Логика взаимодействия для MainRefundWindow.xaml
    /// </summary>
    public partial class MainRefundWindow : Window
    {
        ObservableCollection<RefundDatagridClass> refundDatagrids; //для вывода
        APIClass api;
        List<Sklad> s;
        List<Tovar> t;
        List<SellTovars> st;
        List<Sell> sel;
        List<Sell> selForRemove = new List<Sell>(); //при выборе неск товаров в чеке, сюда они добавляются
        List<ForOutputTovarsInPaycheck> forOutput;
        int SUMMARYforRefundTovarInCheck = 0;
        string PaymentIDforRefundTovarInCheck = "";
        Account account;
        public MainRefundWindow(Account _account)
        {
            InitializeComponent();
            api = new APIClass();
            account = _account;

            //       public int Number { get; set; }
            //public DateTime Time { get; set; }
            //public string PaymentId { get; set; }
            //public int Summary { get; set; }


            try
            {

                #region заполнить список чеков из базы
                //refundDatagrids
                t = api.TovarList();
                st = api.SellTovarsList();
                sel = api.SellList();
                s = api.SkladList();

                refundDatagrids = new ObservableCollection<RefundDatagridClass>();

                //добавление товаров в список (из листа в обсерабл)
                var local = st;
                int k = 0;
                Random rnd = new Random();
                foreach (var item in local)
                {
                    k++;
                    refundDatagrids.Add(new RefundDatagridClass
                    {
                        Number = k,
                        PaymentId = item.PaymentId,
                        Time = item.Date_sell,
                        Summary = item.Summary,
                        selltovar = item
                    });
                }
                foreach (var item in refundDatagrids) item.Time = item.Time.AddHours(5);
                DataGridtable.ItemsSource = refundDatagrids;
                CountRezultTbx.Text = "Результатов: " + refundDatagrids.Count.ToString();
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
        /// полный возврат чека
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FullRefund(object sender, RoutedEventArgs e)
        {
            try
            {
                List<CashierLbx> forCheckCashierLbx = new(); //список товаров для возврата(для печати на чеке)
                int i = 1; //для чека #
                RefundDatagridClass forRefund = (RefundDatagridClass)(sender as FrameworkElement).DataContext;
                //name = summa
                //comment = paymentid
                summaInRefundTextblock.Text = "Сумма к возврату: " + forRefund.Summary.ToString() + " руб.";

                string IsBank = api.ListOfReceipts(new Tovar() { Name = forRefund.Summary.ToString(), Comment = forRefund.PaymentId });
                var listl = sel.Where(x => x.SellTovars_id == forRefund.selltovar.SellTovars_id).ToList();

                StaticClassForUrlCardPayment.PaymentId = PaymentIDforRefundTovarInCheck;
                if (IsBank == "Оплата была совершена наличным расчетом") //если провал, то делаем манибэк налом
                {
                    CashierShift.summaNal += forRefund.Summary;



                    List<ConnectToDB.Model.Refund> refundlist = new List<ConnectToDB.Model.Refund>();

                    foreach (var item in listl)
                    {
                        CashierShift.countNal += item.Count;
                        forCheckCashierLbx.Add(new()
                        {
                            Count = item.Count,
                            Number = i,
                            Price = item.Summary / item.Count,
                            tovar = t.FirstOrDefault(x => x.Tovar_id == item.Tovar_id)
                        });
                        i++;
                        //для таблицы Refund
                        refundlist.Add(new ConnectToDB.Model.Refund
                        {
                            PaymentId = forRefund.selltovar.PaymentId,
                            Cashier_id = forRefund.selltovar.Kassir_id,
                            Count = item.Count,
                            Date = DateTime.Now.ToUniversalTime(),
                            Summary = item.Summary,
                            Tovar = item.Tovar,
                            Tovar_id = item.Tovar_id
                        });

                        api.DeleteSell(item.Sell_id);
                    }

                    foreach (var item in refundlist)
                    {
                        api.AddRefund(item);
                    }


                    api.DeleteSellTovars(forRefund.selltovar.SellTovars_id);

                    MessageBox.Show("Необходимо произвести возврат средств наличными средствами!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);



                }
                else // успешно деньги на карту отправились
                {
                    CashierShift.summaBeznal += forRefund.Summary;


                    List<ConnectToDB.Model.Refund> refundlist = new List<ConnectToDB.Model.Refund>();

                    foreach (var item in listl)
                    {
                        CashierShift.countBeznal += item.Count;
                        
                        //для чека о возврате
                        forCheckCashierLbx.Add(new()
                        {
                            Count = item.Count,
                            Number = i,
                            Price = item.Summary / item.Count,
                            tovar = t.FirstOrDefault(x => x.Tovar_id == item.Tovar_id)
                        });
                        i++;

                        //для таблицы Refund
                        refundlist.Add(new ConnectToDB.Model.Refund
                        {
                            PaymentId = forRefund.selltovar.PaymentId,
                            Cashier_id = forRefund.selltovar.Kassir_id,
                            Count = item.Count,
                            Date = DateTime.Now.ToUniversalTime(),
                            Summary = item.Summary,
                            Tovar_id = item.Tovar_id
                        });
                        api.DeleteSell(item.Sell_id);
                    }


                    foreach (var item in refundlist)
                    {
                        api.AddRefund(item);
                    }

                    api.DeleteSellTovars(forRefund.selltovar.SellTovars_id);


                    MessageBox.Show("Возврат средств успешно совершен!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);

                }


                s = api.SkladList();
                foreach (var item in listl)
                {
                    Sklad forUpdate = s.First(x => x.Tovar_id == item.Tovar_id);
                    forUpdate.Count += item.Count;
                    api.UpdateSklad(forUpdate);
                }

                #region Печать чека о возврате
                StaticClassForUrlCardPayment.PaymentId = forRefund.selltovar.PaymentId;
                CheckForPrint printCheck = new CheckForPrint(account, forCheckCashierLbx, forRefund.selltovar.Date_sell, forRefund.selltovar.SellTovars_id, forRefund.selltovar.Summary, "Возврат");
       // public CheckForPrint(Account _account, List<CashierLbx> kassa, DateTime data_prodagi, int numbercheck, int summa, string str) //печать чека о ВОЗВРАТЕ

                #endregion

                Close();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

        }


        /// <summary>
        /// выбрали чек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedCheck(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //выбрали чек
                RefundDatagridClass? selectedCheck = (RefundDatagridClass?)DataGridtable.SelectedItem;

                if (selectedCheck == null)
                {
                    MessageBox.Show("Ошибка! Попробуйте ещё раз!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }



                backButton.Visibility = Visibility.Visible;
                DataGridCheck.Visibility = Visibility.Visible;
                DataGridtable.Visibility = Visibility.Hidden;

                List<Sell> listForTwoDatagrid = sel.Where(x => x.SellTovars_id == selectedCheck.selltovar.SellTovars_id).ToList();
                List<ForOutputTovarsInPaycheck> forOutput = new List<ForOutputTovarsInPaycheck>();
                int k = 0;
                foreach (var item in listForTwoDatagrid)
                {
                    for (int i = 0; i < item.Count; i++)
                    {
                        k++;
                        forOutput.Add(new ForOutputTovarsInPaycheck()
                        {
                            Number = k,
                            tovar = t.First(x => x.Tovar_id == item.Tovar_id),
                            Count = 1,
                            Summary = item.Summary / item.Count,
                            sellTovars_id = item.SellTovars_id
                        });

                    }

                }

                DataGridCheck.ItemsSource = forOutput;
                CountRezultTbx.Text = "Результатов: " + forOutput.Count.ToString();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        /// <summary>
        /// поиск
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox(object sender, TextChangedEventArgs e)
        {
            try
            {
                //поиск по артикулу или наименованию
                List<ForOutputTovarsInPaycheck> forOutput = new List<ForOutputTovarsInPaycheck>();
                int k = 0;
                foreach (var item in sel)
                {
                    k++;
                    forOutput.Add(new ForOutputTovarsInPaycheck()
                    {
                        Number = k,
                        tovar = t.First(x => x.Tovar_id == item.Tovar_id),
                        Count = item.Count,
                        Summary = item.Summary,
                        sellTovars_id = item.SellTovars_id
                    });
                }

                //получили содержимое чеков
                List<ForOutputTovarsInPaycheck> search = forOutput.Where(x => x.tovar.Name.ToLower().Contains(txtSearch.Text.ToLower()) || x.tovar.Artikul.ToString().Contains(txtSearch.Text)).ToList();

                //ищем сами основные чеки для вывода в первом датагриде

                List<RefundDatagridClass> list = new List<RefundDatagridClass>();

                foreach (var item in search)
                {
                    var a = refundDatagrids.Where(x => x.selltovar.SellTovars_id == item.sellTovars_id).FirstOrDefault();
                    if (a != null)
                        if (!list.Contains(a))
                            list.Add(a);
                }

                 
                DataGridtable.ItemsSource = null;
                DataGridtable.ItemsSource = list;
                CountRezultTbx.Text = "Результатов: " + list.Count;
                if (txtSearch.Text == "" || txtSearch.Text == null)
                {
                    DataGridtable.ItemsSource = refundDatagrids;
                    CountRezultTbx.Text = "Результатов: " + refundDatagrids.Count;
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
        /// кнопка назад
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back(object sender, RoutedEventArgs e)
        {
            try
            {
                backButton.Visibility = Visibility.Hidden;
                DataGridCheck.Visibility = Visibility.Hidden;
                DataGridtable.Visibility = Visibility.Visible;
                SUMMARYforRefundTovarInCheck = 0;
                RefundTovarsButton.Visibility = Visibility.Hidden;
                PaymentIDforRefundTovarInCheck = "";
                summaInRefundTextblock.Text = "Сумма к возврату: 0 руб.";
                DataGridtable.ItemsSource = null;
                selForRemove.Clear();
                SearchTextBox(null, null);

            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // CountRezultTbx.Text = "Результатов: " + history.Count;
        }


        /// <summary>
        /// открыли чек и выбрали товар
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedTovarForMoneyback(object sender, RoutedEventArgs e)
        {
            try
            {
                ForOutputTovarsInPaycheck? selectedTovar = (ForOutputTovarsInPaycheck?)DataGridCheck.SelectedItem;

                if (selectedTovar == null)
                {
                    MessageBox.Show("Ошибка! Попробуйте ещё раз!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CheckBox checkbox = sender as CheckBox;
                if (checkbox.IsChecked == true) //выбрали
                {
                    RefundTovarsButton.Visibility = Visibility.Visible;
                    SUMMARYforRefundTovarInCheck += selectedTovar.Summary;
                    PaymentIDforRefundTovarInCheck = refundDatagrids.First(x => x.selltovar.SellTovars_id == selectedTovar.sellTovars_id).PaymentId;
                    var AddOrDelete = sel.First(x => x.SellTovars_id == selectedTovar.sellTovars_id && x.Tovar_id == selectedTovar.tovar.Tovar_id);
                    
                    Sell addordel = new Sell()
                    {
                        Count = 1,
                        Summary = AddOrDelete.Summary / AddOrDelete.Count,
                        SellTovars_id = AddOrDelete.SellTovars_id,
                        Sell_id = AddOrDelete.Sell_id,
                        Tovar_id = AddOrDelete.Tovar_id
                    };

                    selForRemove.Add(addordel);
                    summaInRefundTextblock.Text = "Сумма к возврату: " + SUMMARYforRefundTovarInCheck + " руб.";


                }
                else //убрали
                {
                    SUMMARYforRefundTovarInCheck -= selectedTovar.Summary;
                    summaInRefundTextblock.Text = "Сумма к возврату: " + SUMMARYforRefundTovarInCheck + " руб.";

                    var AddOrDelete = sel.First(x => x.SellTovars_id == selectedTovar.sellTovars_id && x.Tovar_id == selectedTovar.tovar.Tovar_id);


                    Sell addordel = new Sell()
                    {
                        Count = 1,
                        Summary = AddOrDelete.Summary / AddOrDelete.Count,
                        SellTovars_id = AddOrDelete.SellTovars_id,
                        Sell_id = AddOrDelete.Sell_id,
                        Tovar_id = AddOrDelete.Tovar_id
                    };
                    selForRemove.Remove(addordel);
                }

                if (SUMMARYforRefundTovarInCheck == 0)//если товаров для возврата нет, удаляем PaymentId
                {
                    PaymentIDforRefundTovarInCheck = "";
                    RefundTovarsButton.Visibility = Visibility.Hidden;
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
        /// закрыть окно
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

     
        /// <summary>
        /// частичный возврат товаров из чека
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PartRefundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                List<CashierLbx> forCheckCashierLbx = new(); //список товаров для возврата(для печати на чеке)
                int i = 1; //для чека #
                string IsBank = api.ListOfReceipts(new Tovar() { Name = SUMMARYforRefundTovarInCheck.ToString(), Comment = PaymentIDforRefundTovarInCheck });

                List<ConnectToDB.Model.Refund> refundlist = new List<ConnectToDB.Model.Refund>();
                var forRefund = st.First(x => x.PaymentId == PaymentIDforRefundTovarInCheck);



                if (IsBank == "Оплата была совершена наличным расчетом") //если провал, то делаем манибэк налом
                {
                    CashierShift.summaNal += SUMMARYforRefundTovarInCheck;
                    CashierShift.countNal += selForRemove.Count;

                    MessageBoxResult r = MessageBox.Show("Необходимо произвести возврат средств наличными средствами!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (r == MessageBoxResult.OK)
                    {
                        List<Sell> forRemoveSell = new List<Sell>();
                        List<int> choosedSell = new List<int>();
                        foreach (var item in selForRemove)
                        {
                            if (!choosedSell.Contains(item.Sell_id))
                            {
                                //если все товары манибэк, то обновляем данные
                                forRemoveSell = selForRemove.Where(x => x.SellTovars_id == item.SellTovars_id && x.Tovar_id == item.Tovar_id).ToList();

                                var updOrDeleteSell = sel.First(x => x.Sell_id == item.Sell_id);
                              
                                if (forRemoveSell.Count == updOrDeleteSell.Count) //если например в чеке 2 одинановых товара и хотят их оба вернуть
                                {
                                    //CashierShift.countNal += item.Count;
                                    //CashierShift.summaNal += item.Summary;
                                    int priceForOneTovar = updOrDeleteSell.Summary / updOrDeleteSell.Count;

                                    #region для подсчета итоговой суммы возврата по этому товару
                                    var sum = 0;
                                    foreach (var item2 in forRemoveSell)
                                        sum += item2.Summary;
                                    #endregion

                                    forCheckCashierLbx.Add(new()
                                    {
                                        Count = updOrDeleteSell.Count,
                                        Number = i,
                                        Price = sum/updOrDeleteSell.Count,
                                        tovar = t.FirstOrDefault(x => x.Tovar_id == updOrDeleteSell.Tovar_id)
                                    });
                                    i++;

                                    //для таблицы Refund
                                    refundlist.Add(new ConnectToDB.Model.Refund
                                    {
                                        PaymentId = forRefund.PaymentId,
                                        Cashier_id = forRefund.Kassir_id,
                                        Count = forRemoveSell.Count,
                                        Date = DateTime.Now.ToUniversalTime(),
                                        Summary = priceForOneTovar * forRemoveSell.Count,
                                        Tovar_id = item.Tovar_id
                                    });
                                    api.DeleteSell(item.Sell_id);
                                }

                                else //если только 1 товар условно из двух манибэк, то обновляем данные
                                {
                                    int priceForOneTovar = updOrDeleteSell.Summary / updOrDeleteSell.Count;

                                    //   CashierShift.countNal += forRemoveSell.Count;
                                    //   CashierShift.summaNal += (priceForOneTovar * forRemoveSell.Count);

                                    updOrDeleteSell.Summary -= priceForOneTovar * forRemoveSell.Count;
                                    updOrDeleteSell.Count -= forRemoveSell.Count;

                                    #region для подсчета итоговой суммы возврата по этому товару
                                    var sum = 0;
                                    foreach (var item2 in forRemoveSell)
                                        sum += item2.Summary;
                                    #endregion

                                    forCheckCashierLbx.Add(new()
                                    {
                                        Count = forRemoveSell.Count,
                                        Number = i,
                                        Price = sum / forRemoveSell.Count,
                                        tovar = t.FirstOrDefault(x => x.Tovar_id == updOrDeleteSell.Tovar_id)
                                    });
                                    i++;
                                    //для таблицы Refund
                                    refundlist.Add(new ConnectToDB.Model.Refund
                                    {
                                        PaymentId = forRefund.PaymentId,
                                        Cashier_id = forRefund.Kassir_id,
                                        Count = forRemoveSell.Count,
                                        Date = DateTime.Now.ToUniversalTime(),
                                        Summary = priceForOneTovar * forRemoveSell.Count,
                                        Tovar_id = updOrDeleteSell.Tovar_id
                                    });
                                   

                                    api.UpdateSell(updOrDeleteSell);
                                }
                                choosedSell.Add(item.Sell_id);
                            }
                        }

                        //если товаров в чеке нет, то удаляем чек
                        var selltovar = st.First(x => x.PaymentId == PaymentIDforRefundTovarInCheck);
                        sel = api.SellList();

                        if (sel.FirstOrDefault(x => x.SellTovars_id == selltovar.SellTovars_id) == null)
                            api.DeleteSellTovars(selltovar.SellTovars_id);


                        else  //иначе меняем сумму чека
                        {

                            int sumforedit = 0;
                            foreach (var item in selForRemove)
                            {
                                sumforedit += item.Summary;
                            }

                            selltovar.Summary -= sumforedit;
                            api.UpdateSellTovars(selltovar);
                        }

                        //заливаем все возвращенные товары в таблицу Refund
                        foreach (var item in refundlist)
                        {
                            api.AddRefund(item);
                        }

                    
                        //добавляем товары обратно на склад
                        List<int> IsUpdated = new List<int>();

                        foreach (var item in selForRemove)
                        {

                            //зачем if:
                            //список выглядит так: товар1 - 1шт. товар1 - 1шт.
                            //чтоб он обновил нормально, делаем проверку, чтобы неск раз один и тот же товар на складе не обновлять
                            if (IsUpdated.Contains(item.Tovar_id)) continue;

                            IsUpdated.Add(item.Tovar_id);
                            //forUpdateSellList - все одинаковые товары для возврата на склад
                            List<Sell> forUpdateSellList = selForRemove.Where(x => x.Tovar_id == item.Tovar_id).ToList();

                            Sklad forUpdate = s.First(x => x.Tovar_id == item.Tovar_id);

                            forUpdate.Count += forUpdateSellList.Count;
                            api.UpdateSklad(forUpdate);
                        }

                        Close();

                    }

                }

                else // НЕ РАБОТАЕТ
                {
                    CashierShift.summaBeznal += SUMMARYforRefundTovarInCheck;
                    CashierShift.countBeznal += selForRemove.Count;

                    MessageBoxResult r = MessageBox.Show("Возврат средств успешно совершен!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (r == MessageBoxResult.OK)
                    {

                        List<Sell> forRemoveSell = new List<Sell>();
                        List<int> choosedSell = new List<int>();
                        foreach (var item in selForRemove)
                        {
                            if (!choosedSell.Contains(item.Sell_id))
                            {
                                //если все товары манибэк, то обновляем данные
                                forRemoveSell = selForRemove.Where(x => x.SellTovars_id == item.SellTovars_id && x.Tovar_id == item.Tovar_id).ToList();

                                var updOrDeleteSell = sel.First(x => x.Sell_id == item.Sell_id);
                                if (forRemoveSell.Count == updOrDeleteSell.Count)
                                {
                                    forCheckCashierLbx.Add(new()
                                    {
                                        Count = item.Count,
                                        Number = i,
                                        Price = item.Summary / item.Count,
                                        tovar = t.FirstOrDefault(x => x.Tovar_id == item.Tovar_id)
                                    });
                                    i++;
                                  //  var forRefund = st.First(x => x.PaymentId == PaymentIDforRefundTovarInCheck);

                                    //для таблицы Refund
                                    refundlist.Add(new ConnectToDB.Model.Refund
                                    {
                                        PaymentId = forRefund.PaymentId,
                                        Cashier_id = forRefund.Kassir_id,
                                        Count = updOrDeleteSell.Count,
                                        Date = DateTime.Now.ToUniversalTime(),
                                        Summary = updOrDeleteSell.Summary,
                                        Tovar_id = item.Tovar_id
                                    });
                                    api.DeleteSell(item.Sell_id);
                                }

                                else //если только 1 товар условно из двух манибэк, то обновляем данные
                                {
                                    int priceForOneTovar = updOrDeleteSell.Summary / updOrDeleteSell.Count;


                                    updOrDeleteSell.Summary -= priceForOneTovar * forRemoveSell.Count;
                                    updOrDeleteSell.Count -= forRemoveSell.Count;

                                 //   var forRefund = st.First(x => x.PaymentId == PaymentIDforRefundTovarInCheck);

                                    //для таблицы Refund
                                    refundlist.Add(new ConnectToDB.Model.Refund
                                    {
                                        PaymentId = forRefund.PaymentId,
                                        Cashier_id = forRefund.Kassir_id,
                                        Count = updOrDeleteSell.Count,
                                        Date = DateTime.Now.ToUniversalTime(),
                                        Summary = updOrDeleteSell.Summary,
                                        Tovar_id = updOrDeleteSell.Tovar_id
                                    });
                                    api.UpdateSell(updOrDeleteSell);
                                }
                                choosedSell.Add(item.Sell_id);
                            }
                        }

                        //если товаров в чеке нет, то удаляем чек
                        var selltovar = st.First(x => x.PaymentId == PaymentIDforRefundTovarInCheck);
                        sel = api.SellList();

                        if (sel.FirstOrDefault(x => x.SellTovars_id == selltovar.SellTovars_id) == null)
                            api.DeleteSellTovars(selltovar.SellTovars_id);


                        else  //иначе меняем сумму чека
                        {

                            int sumforedit = 0;
                            foreach (var item in selForRemove)
                            {
                                sumforedit += item.Summary;
                            }

                            selltovar.Summary -= sumforedit;
                            api.UpdateSellTovars(selltovar);
                        }


                        SUMMARYforRefundTovarInCheck = 0;
                        PaymentIDforRefundTovarInCheck = "";


                        //заливаем все возвращенные товары в таблицу Refund
                        foreach (var item in refundlist)
                        {
                            api.AddRefund(item);
                        }


                        //добавляем товары обратно на склад
                        List<int> IsUpdated = new List<int>();

                        foreach (var item in selForRemove)
                        {

                            //зачем if:
                            //список выглядит так: товар1 - 1шт. товар1 - 1шт.
                            //чтоб он обновил нормально, делаем проверку, чтобы неск раз один и тот же товар на складе не обновлять
                            if (IsUpdated.Contains(item.Tovar_id)) continue;

                            IsUpdated.Add(item.Tovar_id);
                            //forUpdateSellList - все одинаковые товары для возврата на склад
                            List<Sell> forUpdateSellList = selForRemove.Where(x => x.Tovar_id == item.Tovar_id).ToList();

                            Sklad forUpdate = s.First(x => x.Tovar_id == item.Tovar_id);

                            forUpdate.Count += forUpdateSellList.Count;
                            api.UpdateSklad(forUpdate);
                        }


                        Close();

                    }

                }

                #region Печать чека о возврате
                StaticClassForUrlCardPayment.PaymentId = PaymentIDforRefundTovarInCheck;
                CheckForPrint printCheck = new CheckForPrint(account, forCheckCashierLbx, forRefund.Date_sell, forRefund.SellTovars_id, refundlist.Select(x => x.Summary).Sum(), "Возврат");
                // public CheckForPrint(Account _account, List<CashierLbx> kassa, DateTime data_prodagi, int numbercheck, int summa, string str) //печать чека о ВОЗВРАТЕ
                #endregion
                SUMMARYforRefundTovarInCheck = 0;
                PaymentIDforRefundTovarInCheck = "";



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
