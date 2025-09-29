using Kursovoi.ConnectToDB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.Classes
{
    public class RefundDatagridClass
    {
        public int Number { get; set; }
        public DateTime Time { get; set; }
        public string PaymentId { get; set; }
        public int Summary { get; set; }
        public SellTovars selltovar { get; set; }
    }
}
