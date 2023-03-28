using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
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

namespace Kursovoi.Finance.FinPages
{
    /// <summary>
    /// Логика взаимодействия для MainFinPage.xaml
    /// </summary>
    public partial class MainFinPage : Page
    {
        DataContext db;
        public MainFinPage()
        {
            InitializeComponent();
            db = new DataContext();
          

            //продажи за месяц
            List<Sell> sells = db.Sell.Where(x => x.SellTovars.Date_sell <= DateTime.Now && x.SellTovars.Date_sell >= DateTime.Now.AddDays(-30)).ToList();

            #region взаимодействуем таблицы для связей
            List<Account> a = db.Account.ToList();
            List<Tovar> t = db.Tovar.ToList();
            List<SellTovars> st = db.SellTovars.ToList();
            List<Sell> sel = db.Sell.ToList();
            #endregion

            #region для диаграммы
            LiveCharts.ChartValues<int> values = new LiveCharts.ChartValues<int>();
           
            List<DateTime> dt = new List<DateTime>();
            int k = 0; //счетчик покупок за день
            int dohod = 0;
            int pribil = 0;
            for (int i = 29; i >= 0; i--)//счетчик от 1 до 30 дней
            {
                List<Sell> sellLocale = sells.Where(x => x.SellTovars.Date_sell.Date == DateTime.Now.AddDays(-i).Date  ).ToList();
               
                k = 0;
                foreach (var item2 in sellLocale)
                {                  
                    dohod += item2.Count * db.Sklad.Where(x => x.Tovar_id == item2.Tovar_id).First().Selling_priсe;
                    pribil += item2.Count * (db.Sklad.Where(x => x.Tovar_id == item2.Tovar_id).First().Selling_priсe - db.Sklad.Where(x => x.Tovar_id == item2.Tovar_id).First().Purchase_price);
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
           var sellssort = sells.OrderBy(x => x.SellTovars.Date_sell);
            List<Sell> ordersortList = new List<Sell>();
            foreach (var item in sellssort)
            {
                ordersortList.Add(item);
            }

            //прятаем последние покупки
            switch(ordersortList.Count)
            {
                case 0:
                    order1.Visibility = Visibility.Hidden; 
                    order2.Visibility = Visibility.Hidden; 
                    order3.Visibility = Visibility.Hidden; 
                    order4.Visibility = Visibility.Hidden; 
                    break;

                case 1:
                    order1.Title = ordersortList[0].Tovar.Name;
                    order1.Desc = ordersortList[0].SellTovars.Account.Surname + " " + ordersortList[0].SellTovars.Account.Name.Substring(0,1) +
                        ". " + ordersortList[0].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[0].SellTovars.Date_sell.ToString("d");
                    order1.Icon = IconImg(ordersortList[0]);

                    order2.Visibility = Visibility.Hidden;
                    order3.Visibility = Visibility.Hidden;
                    order4.Visibility = Visibility.Hidden;
                    break; 

                case 2:
                    order1.Title = ordersortList[0].Tovar.Name;
                    order1.Desc = ordersortList[0].SellTovars.Account.Surname + " " + ordersortList[0].SellTovars.Account.Name.Substring(0, 1) +
                        ". " + ordersortList[0].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[0].SellTovars.Date_sell.ToString("d");
                    order1.Icon = IconImg(ordersortList[0]);

                    order2.Title = ordersortList[1].Tovar.Name;
                    order2.Desc = ordersortList[1].SellTovars.Account.Surname + " " + ordersortList[1].SellTovars.Account.Name.Substring(0, 1) +
                        ". " + ordersortList[1].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[1].SellTovars.Date_sell.ToString("d");
                    order2.Icon = IconImg(ordersortList[1]);

                    order3.Visibility = Visibility.Hidden;
                    order4.Visibility = Visibility.Hidden;
                    break; 

                case 3:
                    order1.Title = ordersortList[0].Tovar.Name;
                    order1.Desc = ordersortList[0].SellTovars.Account.Surname + " " + ordersortList[0].SellTovars.Account.Name.Substring(0, 1) +
                        ". " + ordersortList[0].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[0].SellTovars.Date_sell.ToString("d");
                    order1.Icon = IconImg(ordersortList[0]);

                    order2.Title = ordersortList[1].Tovar.Name;
                    order2.Desc = ordersortList[1].SellTovars.Account.Surname + " " + ordersortList[1].SellTovars.Account.Name.Substring(0, 1) +
                        ". " + ordersortList[1].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[1].SellTovars.Date_sell.ToString("d");
                    order2.Icon = IconImg(ordersortList[1]);

                    order3.Title = ordersortList[2].Tovar.Name;
                    order3.Desc = ordersortList[2].SellTovars.Account.Surname + " " + ordersortList[2].SellTovars.Account.Name.Substring(0, 1) +
                        ". " + ordersortList[2].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[2].SellTovars.Date_sell.ToString("d");
                    order3.Icon = IconImg(ordersortList[2]);

                    order4.Visibility = Visibility.Hidden;
                    break; 
                
                default:
                    order1.Title = ordersortList[0].Tovar.Name;
                    order1.Desc = ordersortList[0].SellTovars.Account.Surname + " " + ordersortList[0].SellTovars.Account.Name.Substring(0, 1) +
                        ". " + ordersortList[0].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[0].SellTovars.Date_sell.ToString("d");
                    order1.Icon = IconImg(ordersortList[0]);

                    order2.Title = ordersortList[1].Tovar.Name;
                    order2.Desc = ordersortList[1].SellTovars.Account.Surname + " " + ordersortList[1].SellTovars.Account.Name.Substring(0, 1) +
                        ". " + ordersortList[1].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[1].SellTovars.Date_sell.ToString("d");
                    order2.Icon = IconImg(ordersortList[1]);

                    order3.Title = ordersortList[2].Tovar.Name;
                    order3.Desc = ordersortList[2].SellTovars.Account.Surname + " " + ordersortList[2].SellTovars.Account.Name.Substring(0, 1) +
                        ". " + ordersortList[2].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[2].SellTovars.Date_sell.ToString("d");
                    order3.Icon = IconImg(ordersortList[2]);

                    order4.Title = ordersortList[3].Tovar.Name;
                    order4.Desc = ordersortList[3].SellTovars.Account.Surname + " " + ordersortList[3].SellTovars.Account.Name.Substring(0, 1) +
                        ". " + ordersortList[3].SellTovars.Account.Patronymic.Substring(0, 1) + ". - " + ordersortList[3].SellTovars.Date_sell.ToString("d");
                    order4.Icon = IconImg(ordersortList[3]);
                    break;
            }

        }




        public string IconImg(Sell _sell)
        {
            switch(_sell.Tovar.Type_tovar)
            {
                case "Одежда":                  return @"/Images/tshirt.png"; break;
                case "Обувь":                   return @"/Images/sneaker.png"; break;
                case "Спортивный стиль":        return @"/Images/sportstyle.png"; break;
                case "Все для детей":           return @"/Images/children.png"; break;
                case "Аксессуары":              return @"/Images/accessory.png"; break;
                case "Тренажеры и фитнес":      return @"/Images/fitness.png"; break;
                case "Бег":                     return @"/Images/run.png"; break;
                case "Командные виды спорта":   return @"/Images/teamsport.png"; break;
                case "Единоборства":            return @"/Images/martialarts.png"; break;
                case "Ледовые коньки и хоккей": return @"/Images/ice skates.png"; break;
                case "Беговые лыжи":            return @"/Images/ski.png"; break;
                case "Сноубординг":             return @"/Images/snowboarding.png"; break;
                case "Горные лыжи":             return @"/Images/alpine skiing.png"; break;
                case "Туризм и активный отдых": return @"/Images/tourism.png"; break;
                case "Бассейн и отдых":         return @"/Images/pool.png"; break;
                case "Летний отдых":            return @"/Images/summerrest.png"; break;
                case "Подарочные карты":        return @"/Images/giftcard.png"; break;
            }
            return null;
        }



        
    } 
}
