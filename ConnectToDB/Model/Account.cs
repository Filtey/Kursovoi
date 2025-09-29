using Kursovoi.Classes;
using Kursovoi.ConnectToDB.Model.ApiCRUDs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{
    [Table("Account")]
    public class Account
    {
        [Key]
        public int Account_id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }        
        public string? Patronymic { get; set; }
        public string Gender { get; set; }
        public DateTime Birthday { get; set; }
        public string Post { get; set; }        
        public string Phone { get; set; }       
        public string? Email { get; set; }
        public List<Autorization> Autorization { get; set; } = new();

    }
}


