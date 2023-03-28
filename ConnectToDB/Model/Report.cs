using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{
    [Table("Report")]
    public class Report
    {
        [Key]
        public int Report_id { get; set; }
        public int Tovar_id { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }
        public int Reporting_id { get; set; }

        [ForeignKey("Tovar_id")]
        public Tovar Tovar { get; set; }

        [ForeignKey("Reporting_id")]
        public Reporting Reporting { get; set; }
    }
}
