using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreSystem.ConnectToDB.Model
{
    [Table("Shift")]
    public class Shift
    {
        [Key]
        public int Shift_id { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date_Start { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date_End { get; set; }
        public int Cashier_id { get; set; }
        public int Summary { get; set; }
     
        [ForeignKey("Cashier_id")]
        public Account Cashier { get; set; }
    }
}
