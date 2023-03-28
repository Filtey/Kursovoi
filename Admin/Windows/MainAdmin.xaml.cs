using Kursovoi.Admin.Pages;
using Kursovoi.Auth_Registr;
using Kursovoi.ConnectToDB.Model;
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

namespace Kursovoi.Admin.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainAdmin.xaml
    /// </summary>
    public partial class MainAdmin : Window
    {
        Account account;
        public MainAdmin(Account _account)
        {
            InitializeComponent();
            account = _account;
            TbFamName.Text = account.Surname.Substring(0, 1) + account.Name.Substring(0, 1);
            FIOTb.Text = account.Surname + " " + account.Name;
            frameContent.Navigate(new MainAdminPage());
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        //исчезает окно
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

        private void MainPerexod(object sender, RoutedEventArgs e)
        {
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("pack://application:,,,/ModernButtonTheme.xaml", UriKind.RelativeOrAbsolute);

            MainButton.Style = myResourceDictionary["menuButtonActive"] as Style;
            AddUserButton.Style = myResourceDictionary["menuButton"] as Style;
      
            frameContent.Navigate(new MainAdminPage());
        }

        private void AddPerexod(object sender, RoutedEventArgs e)
        {
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("pack://application:,,,/ModernButtonTheme.xaml", UriKind.RelativeOrAbsolute);

            AddUserButton.Style = myResourceDictionary["menuButtonActive"] as Style;
            MainButton.Style = myResourceDictionary["menuButton"] as Style;

            AddUserAdminWindow addw = new AddUserAdminWindow();
            addw.Closing += Addw_Closing;
            addw.ShowDialog();
        }

        //после закрытия окна открываем главную страницу и обновляем там все данные 
        private void Addw_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            MainPerexod(null, null);
        }
    }
}
