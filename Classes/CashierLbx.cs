using StoreSystem.ConnectToDB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreSystem.Classes
{
    public class CashierLbx
    {
        public int Number { get; set; }
        public Sklad sklad { get; set; }
        public Tovar tovar { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
    }
}
