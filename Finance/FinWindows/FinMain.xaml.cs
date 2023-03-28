using Kursovoi.Auth_Registr;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
using Kursovoi.Finance.FinPages;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kursovoi.Finance.FinWindows
{
    /// <summary>
    /// Логика взаимодействия для FinMain.xaml
    /// </summary>
    public partial class FinMain : Window
    {
        Account account;
        DataContext db;
        public FinMain(Account _acc)
        {
            InitializeComponent();
            db = new DataContext();

            account = _acc;
            TbFamName.Text = account.Surname.Substring(0, 1) + account.Name.Substring(0, 1);
            FIOTb.Text = account.Surname + " " + account.Name;
            frameContent.Navigate(new MainFinPage());
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void ShopPerexod(object sender, RoutedEventArgs e)
        {
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("pack://application:,,,/ModernButtonTheme.xaml", UriKind.RelativeOrAbsolute);

            ShopButton.Style = myResourceDictionary["menuButtonActive"] as Style;
            AutoZakupButton.Style = myResourceDictionary["menuButton"] as Style;
            MainButton.Style = myResourceDictionary["menuButton"] as Style;

            frameContent.Navigate(new AllOrders());
        } 
        private void AutoZakupPerexod(object sender, RoutedEventArgs e)
        {
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("pack://application:,,,/ModernButtonTheme.xaml", UriKind.RelativeOrAbsolute);

            AutoZakupButton.Style = myResourceDictionary["menuButtonActive"] as Style;
            MainButton.Style = myResourceDictionary["menuButton"] as Style;
            ShopButton.Style = myResourceDictionary["menuButton"] as Style;

              frameContent.Navigate(new PurchasePage());
        } 
        private void MainPerexod(object sender, RoutedEventArgs e)
        {
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("pack://application:,,,/ModernButtonTheme.xaml", UriKind.RelativeOrAbsolute);

            MainButton.Style = myResourceDictionary["menuButtonActive"] as Style;
            AutoZakupButton.Style = myResourceDictionary["menuButton"] as Style;
            ShopButton.Style = myResourceDictionary["menuButton"] as Style;

            frameContent.Navigate(new MainFinPage());
        }
      


        private void Logout(object sender, RoutedEventArgs e)
        {
            DoubleAnimation cancelAnim = new DoubleAnimation();
            cancelAnim.From = 1;
            cancelAnim.To = 0;
            cancelAnim.Duration = TimeSpan.FromSeconds(0.4);
            cancelAnim.Completed += cancelAnim_Completed;
            BeginAnimation(Window.OpacityProperty, cancelAnim);
        }

        //после исчезновения окна закрываем его и открываем окно входа
        private void cancelAnim_Completed(object sender, EventArgs e)
        {
            Reg_Auth ra = new Reg_Auth();
            Close();
            ra.Show();
        }

      
    }
}
