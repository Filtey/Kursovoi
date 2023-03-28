using Kursovoi.Classes;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Kursovoi.Classes
{
    public class DatagridPostavka
    {
        public Tovar tovar { get; set; }
        public Sklad sklad { get; set; }
        public int Number { get; set; }
        public int Count { get; set; }
        public int Pur_price { get; set; }
        public Brush BgColor { get; set; }
    }
}
