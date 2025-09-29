using StoreSystem.ConnectToDB.Model;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreSystem.Classes
{
    public class AdminClassUsers
    {
        public Account account { get; set; }
        public int Number { get; set; }
        public string NameB { get; set; }
        public Brush BgColor { get; set; }
    }
}
