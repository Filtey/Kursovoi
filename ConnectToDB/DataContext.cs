using Kursovoi.ConnectToDB.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovoi.ConnectToDB
{
    public class DataContext : DbContext
    {
        public virtual DbSet<Autorization> Autorization { get; set; }
        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<AutoPurchase> AutoPurchase { get; set; }
        public virtual DbSet<History> History { get; set; }
        public virtual DbSet<Manufacturer> Manufacturer { get; set; }
        public virtual DbSet<Report> Report { get; set; }
        public virtual DbSet<Reporting> Reporting { get; set; }
        public virtual DbSet<Shipment> Shipment { get; set; }
        public virtual DbSet<Sklad> Sklad { get; set; }
        public virtual DbSet<Tovar> Tovar { get; set; }
        public virtual DbSet<Sell> Sell { get; set; }
        public virtual DbSet<SellTovars> SellTovars { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseMySql(@"Server=sql.freedb.tech, 3306; Database=freedb_ProbBD; UId=freedb_freedb_ProbBD; PWD=?2?mzCUg%fq6ME?;",
            //    new MySqlServerVersion(new Version(8, 0, 11)));
            optionsBuilder.UseMySql(@"Server=h1.host.filess.io, 3306; Database=KursovoiDB_orangewood; UId=KursovoiDB_orangewood; PWD=5b27562bcaa04129d996b3bb63aafdeb7fe1b6f7;",
              new MySqlServerVersion(new Version(5, 7, 38)));
        }
       
    }
}

