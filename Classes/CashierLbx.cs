﻿using Kursovoi.ConnectToDB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.Classes
{
    public class CashierLbx
    {
        public int Number { get; set; }
        public Sklad sklad { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
    }
}
