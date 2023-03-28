using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{
    [Table("AutoPurchase")]
    public class AutoPurchase
    {
        [Key]
        public int AutoPurchase_id { get; set; }
        public int Account_id { get; set; }
        public int Min_quantity { get; set; }
        public int Tovar_id { get; set; }
        public int Count { get; set; }


        [ForeignKey("Account_id")]
        public Account Account { get; set; }


        [ForeignKey("Tovar_id")]
        public Tovar Tovar { get; set; }
    }
}
