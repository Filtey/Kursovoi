﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB.Model
{
    [Table("SellTovars")]
    public class SellTovars
    {
        [Key]
        public int SellTovars_id { get; set; }
        public int Kassir_id { get; set; }
        public DateTime Date_sell { get; set; }
       
        [ForeignKey("Kassir_id")]
        public Account Account { get; set; }
        public List<Sell> sell { get; set; }


    }
}
