using StoreSystem.ConnectToDB;
using StoreSystem.ConnectToDB.Model;
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
using System.Security.Cryptography;
using StoreSystem.Classes;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
using Npgsql;

namespace StoreSystem.Auth_Registr
{
    /// <summary>
    /// Логика взаимодействия для Reg_Auth.xaml
    /// </summary>
    public partial class Reg_Auth : Window
    {
        int Acctype; //1-складной, 2-финансовый работник, 3-кассир, 4-админ
        Autorization? auth = null;
        APIClass api;

        public Reg_Auth()
        {
            InitializeComponent();
            try
            {
                UsTbx.GotFocus += RemoveText;
                UsTbx.LostFocus += AddText;
                PasswTbx.GotFocus += RemoveTextPassword;
                PasswTbx.LostFocus += AddTextPassword;
                api = new APIClass();
                // UsTbx.Text = "login2";
                //PasswTbx.Password = "password2";
                //LoginClick(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        //для placeholder username
        public void RemoveText(object sender, EventArgs e)
        {
            if (UsTbx.Text == "ИМЯ ПОЛЬЗОВАТЕЛЯ")
            {
                UsTbx.Text = "";
            }
        }

        //для placeholder username
        public void AddText(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsTbx.Text))
                UsTbx.Text = "ИМЯ ПОЛЬЗОВАТЕЛЯ";
        }






        //для placeholder password
        public void RemoveTextPassword(object sender, EventArgs e)
        {
            if (PasswTbx.Password == "PASSWORD")
            {
                PasswTbx.Password = "";
            }
        }

        //для placeholder password 
        public void AddTextPassword(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswTbx.Password))
                PasswTbx.Password = "PASSWORD";
        }




        //для того, чтобы окно можно было перемещать
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }


        //Login
        private void LoginClick(object sender, RoutedEventArgs e)
        {
            //авторизация
            string log = UsTbx.Text;
             string pass = Hashing.hashPassword(PasswTbx.Password);

            //  string log = "login4";
            //  string pass = Hashing.hashPassword("password4");
         
            string otvet = "Неверные учётные данные!";
           
            try
            {
                api = new APIClass();
             
                // db = new DataContext();
              
                List<Account> acc = api.AccountList();
                //  db = new DataContext();
                var bb = api.AutorizationList();
                auth = api.AutorizationList().FirstOrDefault(x => x.Login == log && x.Password == pass);


                if (auth == null)
                {
                    MessageBox.Show(otvet, "Авторизация", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    Acctype = int.Parse(auth.Account_type.ToString());

                }
                //анимация для исчезновения
                DoubleAnimation logAnim = new DoubleAnimation();
                logAnim.From = 1;
                logAnim.To = 0;
                logAnim.Duration = TimeSpan.FromSeconds(0.9);
                logAnim.Completed += logAnim_Completed;
                BeginAnimation(Window.OpacityProperty, logAnim);
                
            }

            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

        }


        //после исчезновения окна закрываем его и открываем окно после входа
        private void logAnim_Completed(object sender, EventArgs e)
        {
            try
            {
                if (Acctype == 1) //Skladnoi (работник склада)
                {
                    var skladnoiAcc = api.AccountList().Where(x => x.Account_id == auth.Account_id).First();
                    Skladnoi.MainSkladnoi sk = new Skladnoi.MainSkladnoi(skladnoiAcc);
                    this.Close();
                    sk.Show();

                }

                else if (Acctype == 2) //финансовый работник
                {
                    var finAcc = api.AccountList().Where(x => x.Account_id == auth.Account_id).First();
                    Finance.FinWindows.FinMain fin = new Finance.FinWindows.FinMain(finAcc);
                    this.Close();
                    fin.Show();

                }

                else if (Acctype == 3) //Кассир
                {

                    var kassirAcc = api.AccountList().Where(x => x.Account_id == auth.Account_id).First();
                    Cashier.CashWindows.MainCashier mainCashier = new Cashier.CashWindows.MainCashier(kassirAcc);
                    this.Close();
                    mainCashier.Show();
                }

                else if (Acctype == 4) //администратор
                {
                    var adminAcc = api.AccountList().Where(x => x.Account_id == auth.Account_id).First();
                    Admin.Windows.MainAdmin adm = new Admin.Windows.MainAdmin(adminAcc);
                    this.Close();
                    adm.Show();
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }



        //Registration
        private void RegClick(object sender, RoutedEventArgs e)
        {
            //анимация для исчезновения
            DoubleAnimation opac = new DoubleAnimation();
            opac.From = 1;
            opac.To = 0;
            opac.Duration = TimeSpan.FromSeconds(0.4);
            opac.Completed += opac_Completed;
            BeginAnimation(Window.OpacityProperty, opac);         
        }

        //после исчезновения окна закрываем его и открываем окно регистрации
        private void opac_Completed(object sender, EventArgs e)
        {
            Register reg = new Register();
            this.Close();
            reg.Show();
        }






        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

      
        private void HideAndViewPassword_PreviewKeyUp(object sender, MouseButtonEventArgs e)
        {
            if (HideAndViewPassword.Kind == MahApps.Metro.IconPacks.PackIconMaterialKind.Eye)
            {
                PasswTbx.Password = pwdTextBox.Text;
                HideAndViewPassword.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.EyeOff;
                pwdTextBox.Visibility = Visibility.Collapsed;
                PasswTbx.Visibility = Visibility.Visible;
            }
            else
            {
                pwdTextBox.Text = PasswTbx.Password;
                HideAndViewPassword.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Eye;
                pwdTextBox.Visibility = Visibility.Visible;
                PasswTbx.Visibility = Visibility.Collapsed;
            }
        }

        private void HideAndViewPassword_Click(object sender, RoutedEventArgs e) => HideAndViewPassword_PreviewKeyUp(null, null);
    }
}
