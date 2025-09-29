using StoreSystem.Auth_Registr.UserControls;
using StoreSystem.ConnectToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace StoreSystem.Auth_Registr
{
    /// <summary>
    /// Логика взаимодействия для Register.xaml
    /// </summary>
    public partial class Register : Window
    {
      //  DataContext db = new DataContext();
        public Register()
        {
            InitializeComponent();
        }

    

        //для перемещения окна
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }




        //исчезает окно
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DoubleAnimation cancelAnim = new DoubleAnimation();
            cancelAnim.From = 1;
            cancelAnim.To = 0;
            cancelAnim.Duration = TimeSpan.FromSeconds(0.4);
            cancelAnim.Completed += cancelAnim_Completed;
            BeginAnimation(Window.OpacityProperty, cancelAnim);
        }
       
        
        //после исчезновения окна закрываем его и открываем окно после входа
        private void cancelAnim_Completed(object sender, EventArgs e)
        {
           Reg_Auth ra = new Reg_Auth();
           this.Close();
            ra.Show();
        }


        //закрываем прогу
        private void ExitProgram(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }



        /// <summary>
        /// метод для валидации ФИО
        /// </summary>
        private void SNFTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                MyTextBox mt = sender as MyTextBox;

                if (!char.IsLetter(char.Parse(e.Text)) || mt.textBox.Text.Length >= 20) //если не буква
                {
                    e.Handled = true;
                }
                mt.textBox.Text = mt.textBox.Text.Replace(" ", "");
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        /// <summary>
        /// метод для валидации телефона
        /// </summary>
        private void PhoneTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                string[] number = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

                if (e.Text == "+" && PhoneTextbox.textBox.Text.Length == 0) { } //если в начале ввели +
                else
                {
                    if (!number.Contains(e.Text))//если не цифра
                    {
                        e.Handled = true;
                    }
                    else if (PhoneTextbox.textBox.Text.Length >= 12) //длина номера макс 13 вместе со знаком +
                    {
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }


        /// <summary>
        /// выбрали гендер
        /// </summary>
        private void ChooseGender(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender as MyOption == MaleIcon)
                {
                    MaleIcon.ImgIconButton.Background = new SolidColorBrush(Colors.Black);
                    FemaleIcon.ImgIconButton.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                }

                else
                {
                    MaleIcon.ImgIconButton.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                    FemaleIcon.ImgIconButton.Background = new SolidColorBrush(Colors.Black);
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void RegistrationAccount(object sender, RoutedEventArgs e)
        {
            try
            {
                //проверяем почту
                var Email = EmailTextbox.textBox.Text;

                string pattern = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

                if (!Regex.IsMatch(Email, pattern)) //почта невалидна
                {
                    MessageBox.Show("Недействительный адрес эл.почты!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                else//почта валидна
                {
                    if (BirthdayTextbox.SelectedDate == null) //если не выбрана дата рождения
                    {
                        MessageBox.Show("Введите дату рождения!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (BirthdayTextbox.SelectedDate.Value >= DateTime.Now.AddYears(-18)) //если возраст меньше 18, то ошибка
                    {
                        MessageBox.Show("Приложением могут пользоваться лица, достигшие возраста 18 лет!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

        }
    }
}