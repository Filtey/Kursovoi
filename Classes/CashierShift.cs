using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.Classes
{
    public static class CashierShift
    {
        public static DateTime? Date_Start { get; set; } = null; // дата начала смены
        public static int MoneyInCashMachine { get; set; } //денег в кассе изначально
        public static int countBeznal { get; set; } //для возврата количество безналом возвращенных
        public static int summaBeznal { get; set; } //для возврата сумма безналом возвращенных

        public static int countNal { get; set; } //для возврата количество налом возвращенных
        public static int summaNal { get; set; } //для возврата сумма налом возвращенных
    }
}
