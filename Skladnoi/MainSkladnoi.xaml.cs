using Kursovoi.Auth_Registr;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
using Kursovoi.Skladnoi.Pages;
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

namespace Kursovoi.Skladnoi
{
    /// <summary>
    /// Логика взаимодействия для MainSkladnoi.xaml
    /// </summary>
    public partial class MainSkladnoi : Window
    {
        Account account;
        //private DataContext db;

        public MainSkladnoi(Account _account)
        {
            InitializeComponent();
            // db = new DataContext();
            try
            {
                account = _account;
                TbFamName.Text = account.Surname.Substring(0, 1) + account.Name.Substring(0, 1);
                FIOTb.Text = account.Surname + " " + account.Name;
                frameContent.Navigate(new Main());
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        //исчезает окно
        private void Logout(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation cancelAnim = new DoubleAnimation();
                cancelAnim.From = 1;
                cancelAnim.To = 0;
                cancelAnim.Duration = TimeSpan.FromSeconds(0.4);
                cancelAnim.Completed += cancelAnim_Completed;
                BeginAnimation(Window.OpacityProperty, cancelAnim);
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        //после исчезновения окна закрываем его и открываем окно входа
        private void cancelAnim_Completed(object sender, EventArgs e)
        {
            try
            {
                Reg_Auth ra = new Reg_Auth();
                Close();
                ra.Show();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void MainPerexod(object sender, RoutedEventArgs e)
        {
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("pack://application:,,,/ModernButtonTheme.xaml", UriKind.RelativeOrAbsolute);

            SkladButton.Style = myResourceDictionary["menuButtonActive"] as Style;
            HistoryButton.Style = myResourceDictionary["menuButton"] as Style;
            //PostavkiButton.Style = myResourceDictionary["menuButton"] as Style;

            frameContent.Navigate(new Main());
        }

        private void HistoryPerexod(object sender, RoutedEventArgs e)
        {
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("pack://application:,,,/ModernButtonTheme.xaml", UriKind.RelativeOrAbsolute);

            HistoryButton.Style = myResourceDictionary["menuButtonActive"] as Style;
            SkladButton.Style = myResourceDictionary["menuButton"] as Style;
            //PostavkiButton.Style = myResourceDictionary["menuButton"] as Style;

            frameContent.Navigate(new HistoryPage());
        }

        //private void PostavkiPerexod(object sender, RoutedEventArgs e)
        //{
        //    var myResourceDictionary = new ResourceDictionary();
        //    myResourceDictionary.Source = new Uri("pack://application:,,,/ModernButtonTheme.xaml", UriKind.RelativeOrAbsolute);

        //    PostavkiButton.Style = myResourceDictionary["menuButtonActive"] as Style;
        //    HistoryButton.Style = myResourceDictionary["menuButton"] as Style;
        //    SkladButton.Style = myResourceDictionary["menuButton"] as Style;

        //    frameContent.Navigate(new PostavkiPage());
        //}
    }
}
