using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreSystem.ConnectToDB.Model
{
    [Table("Manufacturer")]
    public class Manufacturer
    {
        [Key]
        public int Manufacturer_id { get; set; }
        public string Name_Company { get; set; }
        public string FIO_director { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

        public List<Tovar> Tovar { get; set; }
    }
}
