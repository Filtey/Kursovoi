using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreSystem.Classes
{
    public static class StaticClassForUrlCardPayment
    {
        /// <summary>
        /// 1-безналичный, 2-наличный, -1 - отменили продажу, способ оплаты не выбран, -2 - ошибка в процессе оплаты
        /// </summary>
        public static int PaymentMethod {  get; set; } 
        public static string PaymentId {  get; set; }

        /// <summary>
        /// ссылка после завершения оплаты
        /// </summary>
        public static string URL { get; set; } = "";
    }
}
