using Kursovoi.Auth_Registr.UserControls;
using Kursovoi.Classes;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;
using Kursovoi.ConnectToDB.Model.ApiCRUDs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kursovoi.Admin.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddUserAdminPage.xaml
    /// </summary>
    public partial class AddUserAdminWindow : Window
    {
        APIClass db ;
        public string post = "";
        public int postIndex = -1;
        public string gender = "";
        public AddUserAdminWindow()
        {
            InitializeComponent();
            db = new APIClass();
        }





        //для перемещения окна
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    this.DragMove();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }



        //исчезает окно
        private void CancelButtonClick(object sender, RoutedEventArgs e)
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
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
            try
            {
                MyTextBox mt = sender as MyTextBox;

                if (!char.IsLetter(char.Parse(e.Text)) || mt.textBox.Text.Length >= 20) //если не буква
                {
                    e.Handled = true;
                }
                mt.textBox.Text = mt.textBox.Text.Replace(" ", "");
            }
            catch (Exception ex)
            {
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
            catch (Exception ex)
            {
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
                    gender = "м";
                }

                else
                {
                    MaleIcon.ImgIconButton.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                    FemaleIcon.ImgIconButton.Background = new SolidColorBrush(Colors.Black);
                    gender = "ж";
                }
            }
            catch (Exception ex)
            {
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

                    //try catch если неверные данные
                    //ЛОГИН ПАРОЛЬ
                    try
                    {

                        //добавление в таблицу АККАУНТ
                        Account newAcc = new Account();
                        newAcc.Surname = FamiliaTextbox.textBox.Text;
                        newAcc.Name = NameTextbox.textBox.Text;
                        newAcc.Patronymic = PatronymicTextbox.textBox.Text;
                        newAcc.Birthday = (DateTime)BirthdayTextbox.SelectedDate;
                        newAcc.Post = post;
                        newAcc.Phone = PhoneTextbox.textBox.Text;
                        newAcc.Email = EmailTextbox.textBox.Text;
                        newAcc.Gender = gender;

                        db.AddAccount(newAcc);




                        //добавление в таблицу АВТОРИЗАЦИЯ

                        string login = "";
                        string password = "";
                        var loglist = db.AutorizationList().OrderBy(x => x.Autorization_id);
                        var log = loglist.LastOrDefault();
                        if (log == null)
                        {
                            login = "login1";
                        }
                        else
                        {
                            int ch = int.Parse(log.Login.Substring(5, 1)) + 1;
                            login = "login" + ch.ToString();
                        }

                        Autorization logpas = new Autorization();
                        logpas.Login = login;
                        logpas.Password = Hashing.hashPassword(PasswordTextbox.Password);
                        logpas.Account_type = postIndex;
                        logpas.Account_id = newAcc.Account_id;

                        db.AddAutorization(logpas);

                        MessageBox.Show("Нажмите ОК для отображения данных для входа в аккаунт сотрудника", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);

                        MessageBox.Show($"ИНФОРМАЦИЯ ДЛЯ СОТРУДНИКА:\nЛогин:{login}\nПароль:{PasswordTextbox.Password}", "ИНФОРМАЦИЯ ДЛЯ СОТРУДНИКА", MessageBoxButton.OK, MessageBoxImage.Warning);
                        MessageBox.Show("Сотрудник успешно добавлен в базу!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Проверьте правильность введенных данных!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

        }

        private void SelectedAccType(object sender, SelectionChangedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
    }
}
