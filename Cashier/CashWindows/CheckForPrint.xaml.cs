using Kursovoi.Classes;
using Kursovoi.ConnectToDB.Model;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Principal;
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
using Yandex.Checkout.V3;

namespace Kursovoi.Cashier.CashWindows
{
    /// <summary>
    /// Логика взаимодействия для CheckForPrint.xaml
    /// </summary>
    public partial class CheckForPrint : Window
    {
        List<int> sellTovars_id;
        List<Sell> sellEnd;  //состав чеков 
        Account account;


        int countNal = 0;  //сколько продал налом/безналом
        int countBeznal = 0;

        int summaNal = 0; //на какую сумму продал налом/безналом
        int summaBeznal = 0;

        int countRefundNal = 0; //сколько было возвратов налом/безналом
        int countRefundBeznal = 0;

        int summaRefundNal = 0;  //сумма возвратов налом/безналом
        int summaRefundBeznal = 0;
        Shift shift;
        public CheckForPrint(Account _account, Shift _shift, List<int> _sellTovars_id, List<Sell> _sellEnd,
            int _summaNal, int _summaBeznal,
            int _countNal, int _countBeznal) //для закрытия смены, отчетность
        {
            InitializeComponent();
            try
            {
                print.Visibility = Visibility.Collapsed;
                printEquipment.Visibility = Visibility.Collapsed;
                printCloseShift.Visibility = Visibility.Visible;

                sellTovars_id = _sellTovars_id;
                shift = _shift;
                sellEnd = _sellEnd;
                account = _account;

                countNal = _countNal;
                countBeznal = _countBeznal;

                summaNal = _summaNal;
                summaBeznal = _summaBeznal;

                countRefundNal = CashierShift.countNal;
                countRefundBeznal = CashierShift.countBeznal;

                summaRefundNal = CashierShift.summaNal;
                summaRefundBeznal = CashierShift.summaBeznal;


                //сделать стекпанель с выводом этого всего
                if (account.Patronymic == null)
                {
                    Account1Label.Content = "Кассир: " + account.Surname + " " + account.Name.Substring(0, 1) + ".";
                }
                else
                {
                    Account1Label.Content = "Кассир: " + account.Surname + " " + account.Name.Substring(0, 1) + ". " + account.Patronymic.Substring(0, 1) + ".";
                }


                Date1Label.Content = "Дата: " + shift.Date_End;
                NumberShift1Label.Content = "Номер смены: " + shift.Shift_id;



                CountNalLabel.Content = "Количество товаров, проданных наличным расчетом (шт.): " + countNal;
                CountBeznalLabel.Content = "Количество товаров, проданных безналичным расчетом (шт.): " + countBeznal;

                SummaNalLabel.Content = "Сумма товаров, проданных наличным расчетом (руб.): " + summaNal;
                SummaBeznalLabel.Content = "Сумма товаров, проданных безналичным расчетом (руб.): " + summaBeznal;

                RezultCountLabel.Content = "Итоговое количество проданных товаров (шт.): " + (countNal + countBeznal);
                RezultSummaLabel.Content = "Итоговая сумма проданных товаров (руб.): " + (summaNal + summaBeznal);

                //7 8
                RefundCountNalLabel.Content = "Количество товаров, по которым был произведен возврат наличным расчетом (шт.): " + countRefundNal;
                RefundCountBeznalLabel.Content = "Количество товаров, по которым был произведен возврат безналичным расчетом (шт.): " + countRefundBeznal;




                //9 10
                RefundSummaNalLabel.Content = "Сумма товаров, по которым был произведен возврат наличным расчетом (руб.): " + summaRefundNal;
                RefundSummaBeznalLabel.Content = "Сумма товаров, по которым был произведен возврат безналичным расчетом (руб.): " + summaRefundBeznal;

                //11 12
                RefundSummaLabel.Content = "Итоговая сумма товаров, по которым был произведен возврат (руб.): " + (summaRefundBeznal + summaRefundNal);
                RezultLabel.Content = "Остаток наличных средств в кассовом аппарате (руб.): " + (CashierShift.MoneyInCashMachine - CashierShift.summaNal);


                //13
                RefundCountLabel.Content = "Итоговое количество товаров, по которым был произведен возврат (шт.): " + (countRefundNal + countRefundBeznal);



                PrintEndShift();


            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


        }

        public CheckForPrint(Account _account, List<CashierLbx> kassa, DateTime data_prodagi, int numbercheck, int summa) //печать чека
        {
            InitializeComponent();
            try
            {
                print.Visibility = Visibility.Visible;
                printEquipment.Visibility = Visibility.Collapsed;
                printCloseShift.Visibility = Visibility.Collapsed;
                DateOrder.Text = "Дата покупки: " + data_prodagi;
                NumberCheck.Text = numbercheck.ToString();

                if (_account.Patronymic != null)
                    CashierName.Text = "Кассир: " + _account.Surname + ". " + _account.Name.Substring(0, 1) + ". " + _account.Patronymic.Substring(0, 1);

                else
                    CashierName.Text = "Кассир: " + _account.Surname + ". " + _account.Name.Substring(0, 1);

                listviewOrder.ItemsSource = kassa;

                TotalPrice.Text = summa + " руб.";
                paymentIdentificator.Text = "Идентификатор платежа: " + StaticClassForUrlCardPayment.PaymentId;
                Print(numbercheck);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }




        public CheckForPrint(Account acc, DateTime date, int numbershift) //проверка оборудования
        {
            InitializeComponent();
            try
            {
                print.Visibility = Visibility.Collapsed;
                printCloseShift.Visibility = Visibility.Collapsed;
                printEquipment.Visibility = Visibility.Visible;

                if (acc.Patronymic == null)
                {
                    AccountLabel.Content = "Кассир: " + acc.Surname + " " + acc.Name.Substring(0, 1) + ".";
                }
                else
                {
                    AccountLabel.Content = "Кассир: " + acc.Surname + " " + acc.Name.Substring(0, 1) + ". " + acc.Patronymic.Substring(0, 1) + ".";
                }


                DateLabel.Content = "Дата: " + date;
                NumberShiftLabel.Content = "Номер смены: " + numbershift;



                PrintEquip();

            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            // List<
            //  listviewOrder.ItemsSource = list;
        }

        private void Print(int numbercheck) //печать чека
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(print, ("Чек № " + numbercheck));
                    Close();
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        
        
        private void PrintEquip()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(printEquipment, "Проверка");
                    Close();
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        
        
        private void PrintEndShift()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(printCloseShift, "Смена_закрыта");
                    Close();
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
    }

    public class OrderList
    {

        public Tovar Tovar { get; set; }
        public int PriceOne { get; set; }
        public int Count { get; set; }
        public int Summary { get; set; }


    }





}
