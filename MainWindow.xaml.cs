using System;
using System.Collections.Generic;
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
using Kursovoi.Auth_Registr;
namespace Kursovoi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Reg_Auth reg_Auth = new Reg_Auth();
            reg_Auth.Show();
            this.Close();

        }
        public MainWindow(bool f)
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Reg_Auth reg_Auth = new Reg_Auth();
            this.Close();
            reg_Auth.Show();
        }
    }
}
