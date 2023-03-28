﻿using Kursovoi.Auth_Registr.UserControls;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
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
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Kursovoi.Classes;

namespace Kursovoi.Admin.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditAccPage.xaml
    /// </summary>
    public partial class EditAccWindow : Window
    {
        DataContext db = new DataContext();
        Account acc = new Account();
        public string post = "";
        public int postIndex = -1;
        public string gender = "";
        public EditAccWindow(Account _acc)
        {
            InitializeComponent();
            db = new DataContext();
            acc = db.Account.Where(x => x.Account_id == _acc.Account_id).FirstOrDefault();
            var a = db.Autorization.ToList();

            FamiliaTextbox.textBox.Text = acc.Surname; 
            NameTextbox.textBox.Text = acc.Name;
            PatronymicTextbox.textBox.Text = acc.Patronymic;
            #region гендер
            if (acc.Gender == "м")
            {
                MaleIcon.ImgIconButton.Background = new SolidColorBrush(Colors.Black);
                FemaleIcon.ImgIconButton.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                gender = "м";
            }
            else
            {
                MaleIcon.ImgIconButton.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                FemaleIcon.ImgIconButton.Background = new SolidColorBrush(Colors.Black);
                gender = "ж";

            }
            #endregion

            BirthdayTextbox.SelectedDate = (DateTime) acc.Birthday;
            EmailTextbox.textBox.Text = acc.Email;
            PhoneTextbox.textBox.Text = acc.Phone;

            #region должность
            post = acc.Post;
            AccTypeCmbx.SelectedIndex = acc.Autorization.First().Account_type -1;
            #endregion
            
        }





        //для перемещения окна
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
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
            Close();
        }


        //закрываем прогу
        private void ExitProgram(object sender, MouseButtonEventArgs e)
        {
            Close();
        }



        /// <summary>
        /// метод для валидации ФИО
        /// </summary>
        private void SNFTextInput(object sender, TextCompositionEventArgs e)
        {
            MyTextBox mt = sender as MyTextBox;

            if (!char.IsLetter(char.Parse(e.Text)) || mt.textBox.Text.Length >= 20) //если не буква
            {
                e.Handled = true;
            }
            mt.textBox.Text = mt.textBox.Text.Replace(" ", "");
        }


        /// <summary>
        /// метод для валидации телефона
        /// </summary>
        private void PhoneTextInput(object sender, TextCompositionEventArgs e)
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


        /// <summary>
        /// выбрали гендер
        /// </summary>
        private void ChooseGender(object sender, MouseButtonEventArgs e)
        {
            if (sender as MyOption == MaleIcon)
            {
                MaleIcon.ImgIconButton.Background = new SolidColorBrush(Colors.Black);
                FemaleIcon.ImgIconButton.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                gender = "м";
            }

            else
            {
                MaleIcon.ImgIconButton.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                FemaleIcon.ImgIconButton.Background = new SolidColorBrush(Colors.Black);
                gender = "ж";
            }
        }

        private void RegistrationAccount(object sender, RoutedEventArgs e)
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

                //если поля пустые

                #region валидация всех данных
                if (FamiliaTextbox.textBox.Text.Trim(' ').Length <= 1 || //если длина с пробелами равна нулю и меньше
                   NameTextbox.textBox.Text.Trim(' ').Length <= 1 ||
                   PatronymicTextbox.textBox.Text.Trim(' ').Length <= 1 ||
                   gender.Length < 1 ||
                   BirthdayTextbox.SelectedDate == null ||
                   EmailTextbox.textBox.Text.Trim(' ').Length < 1 ||
                   PhoneTextbox.textBox.Text.Trim(' ').Length < 1 ||
                   post.Trim(' ').Length < 1 ||
                   PasswordTextbox.Password.Length < 8
                   )

                {
                    MessageBox.Show("Проверьте правильность введенных данных!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                #endregion

                #region расшифровываем пароль на сервере
                string passwordInBase = db.Autorization.Where(x => x.Account.Account_id == acc.Account_id).FirstOrDefault().Password;
                #endregion

                if (Hashing.hashPassword(PasswordTextbox.Password) == passwordInBase)
                {
                    MessageBox.Show("Новый пароль совпадает со старым!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }



                //try catch если неверные данные
                //ЛОГИН ПАРОЛЬ
                try
                {
                  

                    //обновление в таблице АККАУНТ

                    acc.Surname = FamiliaTextbox.textBox.Text;
                    acc.Name = NameTextbox.textBox.Text;
                    acc.Patronymic = PatronymicTextbox.textBox.Text;
                    acc.Birthday = (DateTime)BirthdayTextbox.SelectedDate;
                    acc.Post = post;
                    acc.Phone = PhoneTextbox.textBox.Text;
                    acc.Email = EmailTextbox.textBox.Text;
                    acc.Gender = gender;


                    //обновление в таблице АВТОРИЗАЦИЯ

                    string login = db.Autorization.Where(x => x.Account_id == acc.Account_id).First().Login;

                    Autorization logpas = db.Autorization.Where(x => x.Account_id == acc.Account_id).First();
                    logpas.Login = login;
                    logpas.Password = Hashing.hashPassword(PasswordTextbox.Password);
                    logpas.Account_type = postIndex;
                    logpas.Account = acc;

                    db.Account.Update(acc);
                    db.Autorization.Update(logpas);
                    db.SaveChanges();



                    MessageBox.Show("Нажмите ОК для отображения данных для входа в аккаунт сотрудника", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);

                    MessageBox.Show($"ИНФОРМАЦИЯ ДЛЯ СОТРУДНИКА:\nЛогин:{login}\nПароль:{PasswordTextbox.Password}", "ИНФОРМАЦИЯ ДЛЯ СОТРУДНИКА", MessageBoxButton.OK, MessageBoxImage.Warning);
                    MessageBox.Show("Сотрудник успешно отредактирован!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Проверьте правильность введенных данных!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }

        }

        private void SelectedAccType(object sender, SelectionChangedEventArgs e)
        {
            var post1 = AccTypeCmbx.SelectedIndex;
            postIndex = post1 + 1;
            switch (post1)
            {
                case 0: post = "складной работник"; break;
                case 1: post = "финансовый работник"; break;
                case 2: post = "кассовый работник"; break;
                case 3: post = "администратор"; break;
            }
        }
    }
}
