using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreSystem.ConnectToDB.Model
{
    [Table("Sell")]
    public class Sell
    {
        [Key]
        public int Sell_id { get; set; }   
        public int Tovar_id { get; set; }
        public int SellTovars_id { get; set; }
        public int Count { get; set; }
        public int Summary { get; set; }


        [ForeignKey("Tovar_id")] 
        public Tovar Tovar { get; set; }


        [ForeignKey("SellTovars_id")]
        public SellTovars SellTovars { get; set; }

    }
}
