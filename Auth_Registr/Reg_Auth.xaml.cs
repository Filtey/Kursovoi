using Kursovoi.ConnectToDB;
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
using System.Security.Cryptography;
using Kursovoi.Classes;

namespace Kursovoi.Auth_Registr
{
    /// <summary>
    /// Логика взаимодействия для Reg_Auth.xaml
    /// </summary>
    public partial class Reg_Auth : Window
    {
        private DataContext db;
        int Acctype; //1-складной, 2-финансовый работник, 3-кассир, 4-админ
        Autorization auth = null;
        public Reg_Auth()
        {
            InitializeComponent();

            UsTbx.GotFocus += RemoveText;
            UsTbx.LostFocus += AddText;
            PasswTbx.GotFocus += RemoveTextPassword;
            PasswTbx.LostFocus += AddTextPassword;
            //    Autorization auth = database.Autorization.FirstOrDefault(x => x.Login == "abc" && x.Password == "12aaabc");

            // if(auth != null)   auth.Password = "1aaabc";
            //     database.SaveChanges();
        
            
            
            
            //  db = new DataContext();
            //  Autorization logpas = db.Autorization.Where(x => x.Account_id == 13).First();

            //#region шифрование
            //byte[] key, iv;
            //Rijndael myRijndael = Rijndael.Create();
            //key = myRijndael.Key;
            //iv = myRijndael.IV;
            //byte[] encrypted = Hashing.EncryptStringToBytes("password5", myRijndael.Key, myRijndael.IV);
            //string zapic = System.Text.Encoding.Default.GetString(encrypted);
            //zapic += "RRRR" + System.Text.Encoding.Default.GetString(key); //32
            //zapic += "FFFF" + System.Text.Encoding.Default.GetString(iv);  //16

            //byte[] zapic2 = System.Text.Encoding.Default.GetBytes(zapic);
            //#endregion

            //zapic = Encoding.UTF8.GetString(zapic2);
            ////\0\\^�0�{��X��&��RRRR\u0015��?ol�.Yз���\nS�1�s��_���u-�\u000fk�FFFF�\r7�\u0003\u001b&!��L\u0014�:��
            //logpas.Password = zapic;

            //db.Autorization.Update(logpas);
            //db.SaveChanges();




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
                db = new DataContext();

                List<Account> acc = db.Account.ToList();
                //  db = new DataContext();
                auth = db.Autorization.FirstOrDefault(x => x.Login == log && x.Password == pass); 


                if (auth == null)
                {
                    MessageBox.Show(otvet, "Авторизация", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    Acctype = int.Parse(auth.Account_type.ToString());
                }
            }

            catch (System.InvalidOperationException ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            //анимация для исчезновения
            DoubleAnimation logAnim = new DoubleAnimation();
            logAnim.From = 1;
            logAnim.To = 0;
            logAnim.Duration = TimeSpan.FromSeconds(0.9);
            logAnim.Completed += logAnim_Completed;
            BeginAnimation(Window.OpacityProperty, logAnim);
        }



        //после исчезновения окна закрываем его и открываем окно после входа
        private void logAnim_Completed(object sender, EventArgs e)
        {
            if (Acctype == 1) //Skladnoi (работник склада)
            {
                var skladnoiAcc = auth.Account;
                Skladnoi.MainSkladnoi sk = new Skladnoi.MainSkladnoi(skladnoiAcc);
                this.Close();
                sk.Show();

            }

            else if (Acctype == 2) //финансовый работник
            {
                var finAcc = auth.Account;
                Finance.FinWindows.FinMain fin = new Finance.FinWindows.FinMain(finAcc);
                this.Close();
                fin.Show();

            }

            else if (Acctype == 3) //Кассир
            {
                var kassirAcc = auth.Account;
                Cashier.CashWindows.MainCashier mainCashier = new Cashier.CashWindows.MainCashier(kassirAcc);
                this.Close();
                mainCashier.Show();
            }

            else if (Acctype == 4) //администратор
            {               
                var adminAcc = auth.Account;
                Admin.Windows.MainAdmin adm = new Admin.Windows.MainAdmin(adminAcc);
                this.Close();
                adm.Show();
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
    }
}
