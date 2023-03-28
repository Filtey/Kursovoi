using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{
    [Table("Reporting")]
    public class Reporting
    {
        [Key]
        public int Reporting_id { get; set; }
        public DateTime Date { get; set; }
        public int Total { get; set; }

        public List<Report> Report { get; set; }      
      
    }
}
