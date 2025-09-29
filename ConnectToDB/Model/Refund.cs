using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{
    [Table("Refund")]
    public class Refund
    {
        [Key]
        public int Refund_id { get; set; }
        public DateTime Date { get; set; }
        public int Tovar_id { get; set; }
        public int Summary { get; set; }
        public int Count { get; set; }
        public int Cashier_id { get; set; }
        public string PaymentId { get; set; }


        [ForeignKey("Tovar_id")]
        public Tovar Tovar { get; set; }  
        
        [ForeignKey("Cashier_id")]
        public Account Cashier { get; set; }
    }
}
