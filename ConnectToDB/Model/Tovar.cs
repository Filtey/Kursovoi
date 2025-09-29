using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreSystem.ConnectToDB.Model
{
    [Table("Tovar")]
    public class Tovar
    {
        [Key]
        public int Tovar_id { get; set; }
        public int Artikul { get; set; }
        public string Name { get; set; }
        public int id_Manufacturer { get; set; }
        public int? Manufacturer_warranty { get; set; }
        public DateTime Production_date { get; set; }
        public string Type_tovar { get; set; }
        public string? Comment { get; set; }
        public DateTime? Valid_until { get; set; }

        [ForeignKey("id_Manufacturer")]
        public Manufacturer Manufacturer { get; set; }
        public List<Shipment> Shipment { get; set; }
        public List<Sklad> Sklad { get; set; }
        public List<Sell> Sell { get; set; }
    }
}
