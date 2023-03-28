using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{
    [Table("Sklad")]
    public class Sklad
    {
        [Key]
        public int Sklad_id { get; set; }
        public int Tovar_id { get; set; }
        public int Purchase_price { get; set; }
        public int Selling_priсe { get; set; }
        public int Count { get; set; }
        public string? unit { get; set; }        
        public string? Comment { get; set; }


        [ForeignKey("Tovar_id")]
        public Tovar Tovar { get; set; }
    }
}
