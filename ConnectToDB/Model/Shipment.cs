using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{
    [Table("Shipment")]
    public class Shipment
    {
        [Key]
        public int Shipment_id { get; set; }
        public int History_id { get; set; }
        public int Tovar_id { get; set; }
        public string Unit { get; set; }
        public int Count { get; set; }
        public int Purchase_price { get; set; }


        [ForeignKey("History_id")]
        public History History { get; set; }

        [ForeignKey("Tovar_id")]
        public Tovar Tovar { get; set; }
    }
}
