using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
using Kursovoi.ConnectToDB.Model.ApiCRUDs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Kursovoi.Finance.FinPages
{
    /// <summary>
    /// Логика взаимодействия для MainFinPage.xaml
    /// </summary>
    public partial class MainFinPage : Page
    {
        APIClass db;
        private DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private int timeForTimer = 90;

        public MainFinPage()
        {
            InitializeComponent();
            try
            {
                db = new APIClass();
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
                #region взаимодействуем таблицы для связей
                List<Account> a = db.AccountList();
                List<Tovar> t = db.TovarList();
                List<SellTovars> st = db.SellTovarsList();
                List<Sell> sel = db.SellList();
                List<Sklad> s = db.SkladList();
                #endregion

                Dictionary<SellTovars, List<Sell>> listSt = new Dictionary<SellTovars, List<Sell>>();
                //продажи за месяц


                foreach (var item in st)
                {
                    listSt.Add(item, sel.Where(x => x.SellTovars_id == item.SellTovars_id).ToList());
                }


                List<Sell> sells = new List<Sell>();

                var b = listSt.Where(x => x.Key.Date_sell <= DateTime.Now && x.Key.Date_sell >= DateTime.Now.AddDays(-30)).ToList();// .Select(y => y.Value);

                foreach (var item in b)
                {
                    foreach (var item2 in item.Value)
                    {
                        sells.Add(item2);

                    }
                }








                #region для диаграммы
                LiveCharts.ChartValues<int> values = new LiveCharts.ChartValues<int>();

                List<DateTime> dt = new List<DateTime>();
                int k = 0; //счетчик покупок за день
                int dohod = 0;
                int pribil = 0;
                for (int i = 29; i >= 0; i--)//счетчик от 1 до 30 дней
                {
                    List<Sell> sellLocale = new List<Sell>();
                    var b2 = listSt.Where(x => x.Key.Date_sell.Date == DateTime.Now.AddDays(-i).Date).ToList();// .Select(y => y.Value);

                    foreach (var item in b2)
                    {
                        foreach (var item2 in item.Value)
                        {
                            sellLocale.Add(item2);

                        }
                    }
                    k = 0;
                    foreach (var item2 in sellLocale)
                    {
                        dohod += item2.Summary;
                        pribil += (item2.Summary - (s.Where(x => x.Tovar_id == item2.Tovar_id).First().Purchase_price * item2.Count));
                        k += item2.Count;
                    }
                    values.Add(k);
                }
                Valueslvc.Values = values;
                #endregion

                #region под диаграммой
                MinSell.TextMiddle = values.Min().ToString();
                MinSell.TextBottom = "День: " + (values.IndexOf(values.Min()) + 1).ToString();

                MaxSell.TextMiddle = values.Max().ToString();
                MaxSell.TextBottom = "День: " + (values.IndexOf(values.Max()) + 1).ToString();

                SredSell.TextMiddle = Math.Round(values.Average(), 0).ToString();
                #endregion

                #region 3 блока сверху
                Pokupki.Number = values.Sum().ToString();
                Dohodi.Number = dohod.ToString() + " ₽";
                Pribil.Number = pribil.ToString() + " ₽";
                #endregion



                //x: Name = "order1" visibility , тянуть из базы nameT, accountName, datetimeSell, 
                listSt.OrderByDescending(x => x.Key.Date_sell);
                var sellssort = listSt.OrderByDescending(x => x.Key.Date_sell);

                List<Sell> ordersortList = new List<Sell>();
                foreach (var item in sellssort)
                {
                    foreach (var item2 in item.Value)
                    {
                        ordersortList.Add(item2);

                    }
                }
                // ordersortList[0].SellTovars.Account.Surname 



                var rezultt1 = st.First(x => x.SellTovars_id == ordersortList[0].SellTovars_id);
                var acc1 = a.First(x => x.Account_id == rezultt1.Kassir_id);

                var rezultt2 = st.First(x => x.SellTovars_id == ordersortList[1].SellTovars_id);
                var acc2 = a.First(x => x.Account_id == rezultt2.Kassir_id);

                var rezultt3 = st.First(x => x.SellTovars_id == ordersortList[2].SellTovars_id);
                var acc3 = a.First(x => x.Account_id == rezultt3.Kassir_id);

                var rezultt4 = st.First(x => x.SellTovars_id == ordersortList[3].SellTovars_id);
                var acc4 = a.First(x => x.Account_id == rezultt4.Kassir_id);

                //прятаем последние покупки
                switch (ordersortList.Count)
                {
                    case 0:
                        order1.Visibility = Visibility.Hidden;
                        order2.Visibility = Visibility.Hidden;
                        order3.Visibility = Visibility.Hidden;
                        order4.Visibility = Visibility.Hidden;
                        break;

                    case 1:
                        order1.Title = t.First(x => x.Tovar_id == ordersortList[0].Tovar_id).Name;
                        order1.Desc = acc1.Surname + " " + acc1.Name.Substring(0, 1) +
                            ". " + acc1.Patronymic.Substring(0, 1) + ". - " + rezultt1.Date_sell.ToString("d");
                        order1.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[0].Tovar_id));

                        order2.Visibility = Visibility.Hidden;
                        order3.Visibility = Visibility.Hidden;
                        order4.Visibility = Visibility.Hidden;
                        break;

                    case 2:
                        order1.Title = t.First(x => x.Tovar_id == ordersortList[0].Tovar_id).Name;
                        order1.Desc = acc1.Surname + " " + acc1.Name.Substring(0, 1) + ". " + acc1.Patronymic.Substring(0, 1) + ". - " + rezultt1.Date_sell.ToString("d");
                        order1.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[0].Tovar_id));

                        order2.Title = t.First(x => x.Tovar_id == ordersortList[1].Tovar_id).Name;
                        order2.Desc = acc2.Surname + " " + acc2.Name.Substring(0, 1) + ". " + acc2.Patronymic.Substring(0, 1) + ". - " + rezultt2.Date_sell.ToString("d");
                        order2.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[1].Tovar_id));

                        order3.Visibility = Visibility.Hidden;
                        order4.Visibility = Visibility.Hidden;
                        break;

                    case 3:
                        order1.Title = t.First(x => x.Tovar_id == ordersortList[0].Tovar_id).Name;
                        order1.Desc = acc1.Surname + " " + acc1.Name.Substring(0, 1) + ". " + acc1.Patronymic.Substring(0, 1) + ". - " + rezultt1.Date_sell.ToString("d");
                        order1.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[0].Tovar_id));

                        order2.Title = t.First(x => x.Tovar_id == ordersortList[1].Tovar_id).Name;
                        order2.Desc = acc2.Surname + " " + acc2.Name.Substring(0, 1) + ". " + acc2.Patronymic.Substring(0, 1) + ". - " + rezultt2.Date_sell.ToString("d");
                        order2.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[1].Tovar_id));

                        order3.Title = t.First(x => x.Tovar_id == ordersortList[2].Tovar_id).Name;
                        order3.Desc = acc3.Surname + " " + acc3.Name.Substring(0, 1) + ". " + acc3.Patronymic.Substring(0, 1) + ". - " + rezultt3.Date_sell.ToString("d");
                        order3.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[2].Tovar_id));

                        order4.Visibility = Visibility.Hidden;
                        break;

                    default:
                        order1.Title = t.First(x => x.Tovar_id == ordersortList[0].Tovar_id).Name;
                        order1.Desc = acc1.Surname + " " + acc1.Name.Substring(0, 1) + ". " + acc1.Patronymic.Substring(0, 1) + ". - " + rezultt1.Date_sell.ToString("d");
                        order1.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[0].Tovar_id));

                        order2.Title = t.First(x => x.Tovar_id == ordersortList[1].Tovar_id).Name;
                        order2.Desc = acc2.Surname + " " + acc2.Name.Substring(0, 1) + ". " + acc2.Patronymic.Substring(0, 1) + ". - " + rezultt2.Date_sell.ToString("d");
                        order2.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[1].Tovar_id));

                        order3.Title = t.First(x => x.Tovar_id == ordersortList[2].Tovar_id).Name;
                        order3.Desc = acc3.Surname + " " + acc3.Name.Substring(0, 1) + ". " + acc3.Patronymic.Substring(0, 1) + ". - " + rezultt3.Date_sell.ToString("d");
                        order3.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[2].Tovar_id));

                        order4.Title = t.First(x => x.Tovar_id == ordersortList[3].Tovar_id).Name;
                        order4.Desc = acc4.Surname + " " + acc4.Name.Substring(0, 1) + ". " + acc4.Patronymic.Substring(0, 1) + ". - " + rezultt4.Date_sell.ToString("d");
                        order4.Icon = IconImg(t.First(x => x.Tovar_id == ordersortList[3].Tovar_id));

                        break;
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        public string IconImg(Tovar _tovar)
        {
            try
            {
                switch (_tovar.Type_tovar)
                {
                    case "Одежда": return @"/Images/tshirt.png";
                    case "Обувь": return @"/Images/sneaker.png";
                    case "Спортивный стиль": return @"/Images/sportstyle.png";
                    case "Все для детей": return @"/Images/children.png";
                    case "Аксессуары": return @"/Images/accessory.png";
                    case "Тренажеры и фитнес": return @"/Images/fitness.png";
                    case "Бег": return @"/Images/run.png";
                    case "Командные виды спорта": return @"/Images/teamsport.png";
                    case "Единоборства": return @"/Images/martialarts.png";
                    case "Ледовые коньки и хоккей": return @"/Images/ice skates.png";
                    case "Беговые лыжи": return @"/Images/ski.png";
                    case "Сноубординг": return @"/Images/snowboarding.png";
                    case "Горные лыжи": return @"/Images/alpine skiing.png";
                    case "Туризм и активный отдых": return @"/Images/tourism.png";
                    case "Бассейн и отдых": return @"/Images/pool.png";
                    case "Летний отдых": return @"/Images/summerrest.png";
                    case "Подарочные карты": return @"/Images/giftcard.png";
                }
                return "";
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проблема с картинками (не найдены)!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return "";
            }
        } 
    } 
}
