using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{

    [Table("History")]
    public class History
    {
        [Key]
        public int History_id { get; set; }
        public DateTime Date { get; set; }

        public List<Shipment> Shipment { get; set; }
    }
}
