using StoreSystem.Classes;
using StoreSystem.ConnectToDB.Model;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO.Packaging;
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

namespace StoreSystem.Cashier.CashWindows
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


                Date1Label.Content = "Смена: " + shift.Date_Start.AddHours(5) +"---" + shift.Date_End.AddHours(5);
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
                Print(numbercheck, null);
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

        public CheckForPrint(Account _account, List<CashierLbx> kassa,
            DateTime data_prodagi, int numbercheck, int summa, string str) //печать чека о ВОЗВРАТЕ
        {
            InitializeComponent();
            try
            {
                print.Visibility = Visibility.Visible;
                printEquipment.Visibility = Visibility.Collapsed;
                printCloseShift.Visibility = Visibility.Collapsed;
                DateOrder.Text = "Дата возврата: " + DateTime.Now + "\nДата покупки: " + data_prodagi.AddHours(5);
                NumberCheck.Text = numbercheck.ToString();

                if (_account.Patronymic != null)
                    CashierName.Text = "Кассир: " + _account.Surname + ". " + _account.Name.Substring(0, 1) + ". " + _account.Patronymic.Substring(0, 1);

                else
                    CashierName.Text = "Кассир: " + _account.Surname + ". " + _account.Name.Substring(0, 1);

                listviewOrder.ItemsSource = kassa;

                TotalPrice.Text = summa + " руб.";
                paymentIdentificator.Text = "Идентификатор платежа: " + StaticClassForUrlCardPayment.PaymentId;
                Print(numbercheck, "Возврат");
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }



        private void Print(int numbercheck, string str) //печать чека
        {
            try
            {
                StackPanel ForPrintStackPanel = null; //что именно печатать, какую стекпанель? Заносим это сюда
                //if (numbercheck > -5)
                //{
                //    PrintDialog printDialog = new PrintDialog();
                //    if (printDialog.ShowDialog() == true)
                //    {
                //        printDialog.PrintVisual(print, ("Чек № " + numbercheck));
                //        Close();
                //    }
                //    return;
                //}

                var fileDialog = new Microsoft.Win32.SaveFileDialog();
                fileDialog.Filter = "PDF|*.pdf";

                if (str == "Открытие") //открытие смены
                {
                    fileDialog.FileName = $"Открытие_Смены_{DateTime.Now:dd-MM-yyyy_HH-mm-ss_ffff}.pdf";
                    ForPrintStackPanel = printEquipment;
                }
                else if (str == "Закрытие")//закрытие смены
                {
                    fileDialog.FileName = $"Закрытие_Смены_{DateTime.Now:dd-MM-yyyy_HH-mm-ss_ffff}.pdf";
                    ForPrintStackPanel = printCloseShift;
                }

                else if(str == "Возврат") //печать чека ВОЗВРАТ
                {
                    fileDialog.FileName = $"ВОЗВРАТ_Чек № {numbercheck}__{DateTime.Now:dd-MM-yyyy_HH-mm-ss_ffff}.pdf";
                }
                else//печать чека
                {
                    fileDialog.FileName = $"Чек № {numbercheck}__{DateTime.Now:dd-MM-yyyy_HH-mm-ss_ffff}.pdf";
                }

                if (fileDialog.ShowDialog() != true)
                {
                    return;
                }

                string filePath = fileDialog.FileName;
                string desctiption = "Document";

                var svr = new System.Printing.LocalPrintServer();
                var queue = svr.GetPrintQueues().FirstOrDefault(_ => _.Name == "Microsoft Print to PDF");
                if (queue == null)
                {
                    return;
                }

                var ticket = queue.DefaultPrintTicket;

                System.IO.MemoryStream streamXPS = new System.IO.MemoryStream();
                using (Package pack = Package.Open(streamXPS, System.IO.FileMode.CreateNew))
                {
                    using (var doc = new System.Windows.Xps.Packaging.XpsDocument(pack, CompressionOption.SuperFast))
                    {
                        var writer = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(doc);
                        if(ForPrintStackPanel != null) writer.Write(ForPrintStackPanel, ticket); //PrintVisual
                        else writer.Write(print, ticket); //PrintVisual
                    }
                }
                streamXPS.Position = 0;


                Aspose.Plugins.AsposeVSOpenXML.XpsPrintHelper.Print(streamXPS, queue.Name, desctiption, false, filePath);

            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        private void PrintEquip() => Print(-20, "Открытие");
        private void PrintEndShift() => Print(-30, "Закрытие");



    }

    public class OrderList
    {

        public Tovar Tovar { get; set; }
        public int PriceOne { get; set; }
        public int Count { get; set; }
        public int Summary { get; set; }


    }





}
namespace Aspose.Plugins.AsposeVSOpenXML
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;

    public class XpsPrintHelper
    {
        public static void Print(Stream stream, string printerName, string jobName, bool isWait
            , string outputFileName)//ファイル名
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (printerName == null)
                throw new ArgumentNullException("printerName");

            IntPtr completionEvent = CreateEvent(IntPtr.Zero, true, false, null);
            if (completionEvent == IntPtr.Zero)
                throw new Win32Exception();

            try
            {
                IXpsPrintJob job;
                IXpsPrintJobStream jobStream;
                StartJob(printerName, jobName, completionEvent, out job, out jobStream, outputFileName);//ファイル名

                CopyJob(stream, job, jobStream);

                if (isWait)
                {
                    WaitForJob(completionEvent);
                    CheckJobStatus(job);
                }
            }
            finally
            {
                if (completionEvent != IntPtr.Zero)
                    CloseHandle(completionEvent);
            }
        }

        private static void StartJob(string printerName, string jobName, IntPtr completionEvent, out IXpsPrintJob job, out IXpsPrintJobStream jobStream
            , string outputFileName)//ファイル名
        {
            int result = StartXpsPrintJob
                (printerName, jobName, outputFileName //ファイル名
                , IntPtr.Zero, completionEvent
                , null, 0, out job, out jobStream, IntPtr.Zero);
            if (result != 0)
                throw new Win32Exception(result);
        }

        private static void CopyJob(Stream stream, IXpsPrintJob job, IXpsPrintJobStream jobStream)
        {
            try
            {
                byte[] buff = new byte[4096];
                while (true)
                {
                    uint read = (uint)stream.Read(buff, 0, buff.Length);
                    if (read == 0)
                        break;

                    uint written;
                    jobStream.Write(buff, read, out written);

                    if (read != written)
                        throw new Exception("Failed to copy data to the print job stream.");
                }

                // Indicate that the entire document has been copied. 
                jobStream.Close();
            }
            catch (Exception)
            {
                // Cancel the job if we had any trouble submitting it. 
                job.Cancel();
                throw;
            }
        }

        private static void WaitForJob(IntPtr completionEvent)
        {
            const int INFINITE = -1;
            switch (WaitForSingleObject(completionEvent, INFINITE))
            {
                case WAIT_RESULT.WAIT_OBJECT_0:
                    // Expected result, do nothing. 
                    break;
                case WAIT_RESULT.WAIT_FAILED:
                    throw new Win32Exception();
                default:
                    throw new Exception("Unexpected result when waiting for the print job.");
            }
        }

        private static void CheckJobStatus(IXpsPrintJob job)
        {
            XPS_JOB_STATUS jobStatus;
            job.GetJobStatus(out jobStatus);
            switch (jobStatus.completion)
            {
                case XPS_JOB_COMPLETION.XPS_JOB_COMPLETED:
                    // Expected result, do nothing. 
                    break;
                case XPS_JOB_COMPLETION.XPS_JOB_FAILED:
                    throw new Win32Exception(jobStatus.jobStatus);
                default:
                    throw new Exception("Unexpected print job status.");
            }
        }

        [DllImport("XpsPrint.dll", EntryPoint = "StartXpsPrintJob")]
        private static extern int StartXpsPrintJob(
            [MarshalAs(UnmanagedType.LPWStr)] String printerName,
            [MarshalAs(UnmanagedType.LPWStr)] String jobName,
            [MarshalAs(UnmanagedType.LPWStr)] String outputFileName, //こいつ
            IntPtr progressEvent,   // HANDLE 
            IntPtr completionEvent, // HANDLE 
            [MarshalAs(UnmanagedType.LPArray)] byte[] printablePagesOn,
            UInt32 printablePagesOnCount,
            out IXpsPrintJob xpsPrintJob,
            out IXpsPrintJobStream documentStream,
            IntPtr printTicketStream);  // This is actually "out IXpsPrintJobStream", but we don't use it and just want to pass null, hence IntPtr. 

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("Kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern WAIT_RESULT WaitForSingleObject(IntPtr handle, Int32 milliseconds);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }

    [Guid("0C733A30-2A1C-11CE-ADE5-00AA0044773D")]  // This is IID of ISequenatialSteam. 
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IXpsPrintJobStream
    {
        // ISequentualStream methods. 
        void Read([MarshalAs(UnmanagedType.LPArray)] byte[] pv, uint cb, out uint pcbRead);
        void Write([MarshalAs(UnmanagedType.LPArray)] byte[] pv, uint cb, out uint pcbWritten);
        // IXpsPrintJobStream methods. 
        void Close();
    }

    [Guid("5ab89b06-8194-425f-ab3b-d7a96e350161")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IXpsPrintJob
    {
        void Cancel();
        void GetJobStatus(out XPS_JOB_STATUS jobStatus);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct XPS_JOB_STATUS
    {
        public UInt32 jobId;
        public Int32 currentDocument;
        public Int32 currentPage;
        public Int32 currentPageTotal;
        public XPS_JOB_COMPLETION completion;
        public Int32 jobStatus; // UInt32 
    };

    enum XPS_JOB_COMPLETION
    {
        XPS_JOB_IN_PROGRESS = 0,
        XPS_JOB_COMPLETED = 1,
        XPS_JOB_CANCELLED = 2,
        XPS_JOB_FAILED = 3
    }

    enum WAIT_RESULT
    {
        WAIT_OBJECT_0 = 0,
        WAIT_ABANDONED = 0x80,
        WAIT_TIMEOUT = 0x102,
        WAIT_FAILED = -1 // 0xFFFFFFFF 
    }
}
