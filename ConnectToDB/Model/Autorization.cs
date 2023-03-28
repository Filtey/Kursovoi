using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{
    [Table("Autorization")]
    public class Autorization
    {
        [Key]
        public int Autorization_id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int Account_type { get; set; }
        public int Account_id { get; set; }

        [ForeignKey("Account_id")]
        public Account Account { get; set; }

    }
}
