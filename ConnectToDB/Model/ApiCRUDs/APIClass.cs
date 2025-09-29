using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Xml.Linq;
using System.Net.Http.Json;
using Yandex.Checkout.V3;

namespace Kursovoi.ConnectToDB.Model.ApiCRUDs
{
    public class APIClass
    {
        HttpClient client = new HttpClient();
        public APIClass()
        {
           // client.BaseAddress = new Uri("http://probaapi-001-site1.itempurl.com");
            client.BaseAddress = new Uri("https://localhost:7114");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }



        //оплата безналом
        public Uri Beznal(decimal summa, string description)
        {
            try
            {
                var response = client.PostAsJsonAsync("api/Beznal", new Tovar() { Name = summa.ToString(), Comment = description }).Result;

                if (response.IsSuccessStatusCode)
                {
                    var url = response.Content.ReadAsAsync<string>().Result;
                    return new Uri(url);
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        } 
        
        
        //возврат (БЕЗНАЛОМ НЕ РАБОТАЕТ, ВЫДАЕМ НАЛИЧНЫМИ)
        public string ListOfReceipts(Tovar forRefund)
        {
           try
           { 
           // HttpResponseMessage response = client.GetAsync("api/ListOfReceipts").Result;
            var response = client.PostAsJsonAsync("api/ListOfReceipts", forRefund).Result;

            if (response.IsSuccessStatusCode)
            {
                var rez = response.Content.ReadAsStringAsync().Result;

                return rez;
            }
            else
            {
                MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
           }
           catch (Exception ex)
           {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
           }
        }



        #region Shift
        public List<Shift> ShiftList()
        {
            try 
            { 
                 HttpResponseMessage response = client.GetAsync("api/ShiftList").Result;
                 if (response.IsSuccessStatusCode)
                 {
                     var ShiftList = response.Content.ReadAsAsync<IEnumerable<Shift>>().Result.ToList();
                     return ShiftList;
                 }
                 else
                 {
                     MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                     return null;
                 }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
        public string DeleteShift(int id)
        {
            try
            {
                var url = "api/ShiftDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
}


        public string AddShift(Shift ShiftForAdd)
        {
            try
            {
                //   var b = client.PostAsJsonAsync("api/TovarAdd", TovarForAdd);

                var response = client.PostAsJsonAsync("api/ShiftAdd", ShiftForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateShift(Shift ShiftUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/ShiftUpdate", ShiftUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region Refund
        public List<Refund> RefundList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/RefundList").Result;
                if (response.IsSuccessStatusCode)
                {
                    var RefundList = response.Content.ReadAsAsync<IEnumerable<Refund>>().Result.ToList();
                    return RefundList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteRefund(int id)
        {
            try
            {
                var url = "api/RefundDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddRefund(Refund RefundForAdd)
        {
            try
            {
                //   var b = client.PostAsJsonAsync("api/TovarAdd", TovarForAdd);

                var response = client.PostAsJsonAsync("api/RefundAdd", RefundForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateRefund(Refund RefundUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/RefundUpdate", RefundUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region Tovar
        public List<Tovar> TovarList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/TovarList").Result;
                if (response.IsSuccessStatusCode)
                {
                    var TovarList = response.Content.ReadAsAsync<IEnumerable<Tovar>>().Result.ToList();
                    return TovarList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteTovar(int id)
        {
            try
            {
                var url = "api/TovarDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddTovar(Tovar TovarForAdd)
        {
            try
            {
                //   var b = client.PostAsJsonAsync("api/TovarAdd", TovarForAdd);

                var response = client.PostAsJsonAsync("api/TovarAdd", TovarForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateTovar(Tovar TovarUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/TovarUpdate", TovarUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region Account
        public List<Account> AccountList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/AccountList").Result;
                if (response.IsSuccessStatusCode)
                {
                    var AccountList = response.Content.ReadAsAsync<IEnumerable<Account>>().Result.ToList();
                    return AccountList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteAccount(int id)
        {
            try
            {
                var url = "api/AccountDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddAccount(Account AccountForAdd)
        {
            try
            {
                var response = client.PostAsJsonAsync("api/AccountAdd", AccountForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateAccount(Account AccountUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/AccountUpdate", AccountUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region Autorization
        public List<Autorization> AutorizationList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/AutorizationList").Result;
                if (response.IsSuccessStatusCode)
                {
                    var AutorizationList = response.Content.ReadAsAsync<IEnumerable<Autorization>>().Result.ToList();
                    return AutorizationList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteAutorization(int id)
        {
            try
            {
                var url = "api/AutorizationDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddAutorization(Autorization AutorizationForAdd)
        {
            try
            {
                var response = client.PostAsJsonAsync("api/AutorizationAdd", AutorizationForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateAutorization(Autorization AutorizationUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/AutorizationUpdate", AutorizationUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region History
        public List<History> HistoryList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/HistoryList").Result;
                if (response.IsSuccessStatusCode)
                {
                    var HistoryList = response.Content.ReadAsAsync<IEnumerable<History>>().Result.ToList();
                    return HistoryList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteHistory(int id)
        {
            try
            {
                var url = "api/HistoryDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddHistory(History HistoryForAdd)
        {
            try
            {
                var response = client.PostAsJsonAsync("api/HistoryAdd", HistoryForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Соединение с интернетом потеряно. Пожалуйста, восстановите соединение для продолжения работы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return ("Информация для разработчиков: Сообщение - " + ex.Message);
            }
        }
        public string UpdateHistory(History HistoryUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/HistoryUpdate", HistoryUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region Manufacturer
        public List<Manufacturer> ManufacturerList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/ManufacturerList").Result;
                if (response.IsSuccessStatusCode)
                {
                    var ManufacturerList = response.Content.ReadAsAsync<IEnumerable<Manufacturer>>().Result.ToList();
                    return ManufacturerList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteManufacturer(int id)
        {
            try
            {
                var url = "api/ManufacturerDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddManufacturer(Manufacturer ManufacturerForAdd)
        {
            try
            {
                var response = client.PostAsJsonAsync("api/ManufacturerAdd", ManufacturerForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateManufacturer(Manufacturer ManufacturerUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/ManufacturerUpdate", ManufacturerUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region Sell
        public List<Sell> SellList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/SellList").Result;
                if (response.IsSuccessStatusCode)
                {
                    var SellList = response.Content.ReadAsAsync<IEnumerable<Sell>>().Result.ToList();
                    return SellList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteSell(int id)
        {
            try
            {
                var url = "api/SellDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddSell(Sell SellForAdd)
        {
            try
            {
                var response = client.PostAsJsonAsync("api/SellAdd", SellForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateSell(Sell SellUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/SellUpdate", SellUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region SellTovars
        public List<SellTovars> SellTovarsList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/SellTovarsList").Result;
                if (response.IsSuccessStatusCode)
                {
                    var SellTovarsList = response.Content.ReadAsAsync<IEnumerable<SellTovars>>().Result.ToList();
                    return SellTovarsList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteSellTovars(int id)
        {
            try
            {
                var url = "api/SellTovarsDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddSellTovars(SellTovars SellTovarsForAdd)
        {
            try
            {
                var response = client.PostAsJsonAsync("api/SellTovarsAdd", SellTovarsForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateSellTovars(SellTovars SellTovarsUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/SellTovarsUpdate", SellTovarsUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region Shipment
        public List<Shipment> ShipmentList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/ShipmentList").Result;
                if (response.IsSuccessStatusCode)
                {
                    var ShipmentList = response.Content.ReadAsAsync<IEnumerable<Shipment>>().Result.ToList();
                    return ShipmentList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteShipment(int id)
        {
            try
            {
                var url = "api/ShipmentDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddShipment(Shipment ShipmentForAdd)
        {
            try
            {
                var response = client.PostAsJsonAsync("api/ShipmentAdd", ShipmentForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateShipment(Shipment ShipmentUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/ShipmentUpdate", ShipmentUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion

        #region Sklad
        public List<Sklad> SkladList()
        {
            try
            {
                HttpResponseMessage response = client.GetAsync("api/SkladList").Result; //http://sdfdfgdfgdfg.com/api/SkladList
                if (response.IsSuccessStatusCode)
                {
                    var SkladList = response.Content.ReadAsAsync<IEnumerable<Sklad>>().Result.ToList();
                    return SkladList;
                }
                else
                {
                    MessageBox.Show("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string DeleteSklad(int id)
        {
            try
            {
                var url = "api/SkladDelete/" + id;

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string AddSklad(Sklad SkladForAdd)
        {
            try
            {
                var response = client.PostAsJsonAsync("api/SkladAdd", SkladForAdd).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        public string UpdateSklad(Sklad SkladUpdated)
        {
            try
            {
                HttpResponseMessage response = client.PutAsJsonAsync("api/SkladUpdate", SkladUpdated).Result;
                if (response.IsSuccessStatusCode)
                {
                    return "Успех";
                }
                else
                {
                    return ("Код ошибки:" + response.StatusCode + " : Сообщение - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            }
        }
        #endregion
    }
}
