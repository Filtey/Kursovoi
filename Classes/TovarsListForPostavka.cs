using Kursovoi.ConnectToDB.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Kursovoi.Classes
{
    public static class TovarsListForPostavka
    {
        public static ObservableCollection<DatagridPostavka> tovarslist { get; set; } = new ObservableCollection<DatagridPostavka>();
        public static int NumberI { get; set; }
    }
}
