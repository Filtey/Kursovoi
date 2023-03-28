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
using Kursovoi.Auth_Registr.UserControls;
using Kursovoi.ConnectToDB;
using Kursovoi.ConnectToDB.Model;

namespace Kursovoi.Skladnoi
{
    /// <summary>
    /// Логика взаимодействия для AddTovar.xaml
    /// </summary>
    public partial class AddTovar : Window
    {
        private DataContext db;
        public string tovarType = ""; //для combobox
        public string Manufact = ""; //для combobox изготовителя


        /// <summary>
        /// добавление
        /// </summary>
        public AddTovar()
        {
            InitializeComponent();
            db = new DataContext();
            //заполнить типами товаров
            List<string> filter = new List<string>();
            filter.Add("Одежда");
            filter.Add("Обувь");
            filter.Add("Спортивный стиль");
            filter.Add("Все для детей");
            filter.Add("Аксессуары");
            filter.Add("Тренажеры и фитнес");
            filter.Add("Бег");
            filter.Add("Командные виды спорта");
            filter.Add("Единоборства");
            filter.Add("Ледовые коньки и хоккей");
            filter.Add("Беговые лыжи");
            filter.Add("Сноубординг");
            filter.Add("Горные лыжи");
            filter.Add("Туризм и активный отдых");
            filter.Add("Бассейн и отдых");
            filter.Add("Летний отдых");
            filter.Add("Подарочные карты");

            // filter = db.Tovar.Select(x => x.Type_tovar).Distinct().ToList();
            TovarTypeCmbx.ItemsSource = filter;


            //комбобокс изготовителей
            List<string> manufactr = new List<string>();
            manufactr = db.Manufacturer.Select(x => x.Name_Company).Distinct().ToList();
            manufactr.Add("новый...");
            ManufacturerTypeCmbx.ItemsSource = manufactr;
        }

        private async void AddTovarClick(object sender, RoutedEventArgs e)
        {
            Tovar addtovar = new Tovar();
            DateTime? valid_until = ValidUntilCalendar.SelectedDate;

            #region валидация всех данных
            if (ArtikulTextbox.textBox.Text.Trim(' ').Length <=4 || //если длина с пробелами равна нулю и меньше
               NameTextbox.textBox.Text.Trim(' ').Length < 2 ||
               WarrantyTextbox.textBox.Text.Trim(' ').Length <= 0 ||
               tovarType.Length <=1 ||
               ProductionDateCalendar.SelectedDate == null ||
               ZakupPriceTextbox.textBox.Text.Trim(' ').Length < 1 ||
               SellPriceTextbox.textBox.Text.Trim(' ').Length < 1 ||
               UnitTextbox.textBox.Text.Trim(' ').Length < 1 ||
               CountTextbox.textBox.Text.Trim(' ').Length < 1)
            {
                MessageBox.Show("Проверьте правильность введенных данных!","Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion

            #region валидация дата производства
            if (ProductionDateCalendar.SelectedDate > DateTime.Now)
            {
                MessageBox.Show("Дата производства не может быть позже текущего дня!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion

            #region валидация комбобокса (что выбран тип товара)
            if (tovarType == "")
            {
                MessageBox.Show("Не выбран тип товара!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion

            #region валидация комбобокса изготовителя (что выбран изготовитель)
            if (Manufact == "")
            {
                MessageBox.Show("Не выбран изготовитель!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion


            #region добавление товара в таблицу товаров и на склад

            //добавление в таблицу ТОВАР
            Tovar newTovar = new Tovar();
            newTovar.Artikul = int.Parse(ArtikulTextbox.textBox.Text);
            newTovar.Name = NameTextbox.textBox.Text;
            newTovar.Manufacturer_warranty = int.Parse(WarrantyTextbox.textBox.Text);
            newTovar.Manufacturer = db.Manufacturer.Where(x => x.Name_Company == Manufact).FirstOrDefault();
            newTovar.Production_date = (DateTime)ProductionDateCalendar.SelectedDate;
            newTovar.Type_tovar = tovarType;
            newTovar.Valid_until = valid_until;

            db.Tovar.Add(newTovar);
            db.SaveChanges();




            //добавление в таблицу СКЛАД
            Sklad newItemInSklad = new Sklad();
            newItemInSklad.Tovar = newTovar;
            newItemInSklad.Tovar_id = newTovar.Tovar_id;
            newItemInSklad.Purchase_price = int.Parse(ZakupPriceTextbox.textBox.Text);
            newItemInSklad.Selling_priсe = int.Parse(SellPriceTextbox.textBox.Text);
            newItemInSklad.Count = int.Parse(CountTextbox.textBox.Text);
            newItemInSklad.unit = UnitTextbox.textBox.Text;
            newItemInSklad.Comment = CommentTextbox.textBox.Text;


            db.Sklad.Add(newItemInSklad);
            db.SaveChanges();

            #endregion

           MessageBoxResult r =  MessageBox.Show("Успешно!", "Уведомление", MessageBoxButton.OK);
            if (r == MessageBoxResult.OK)
            {
                Close();
            }
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
            Close();
        }




        /// <summary>
        /// метод для валидации артикула (только цифры)
        /// </summary>
        private void NumbersTextInput(object sender, TextCompositionEventArgs e)
        {
            string[] number = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            if (!number.Contains(e.Text))//если не цифра
            {
                e.Handled = true;
            }
        }

        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

     
        /// <summary>
        /// выбрали тип товара
        /// </summary>
        private void SelectedTovarType(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            tovarType = comboBox.SelectedItem.ToString();
           // tovarType = selectedItem.Content.ToString();
        }

        private void SelectedManufacturerType(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;
            if(cbx.SelectedItem.ToString() == "новый...")
            {
                AddNewManufWindow newManufWindow = new AddNewManufWindow();
                newManufWindow.Closing += Adt_Closing;
                newManufWindow.ShowDialog();
            }    
            else
            Manufact = cbx.SelectedItem.ToString();
        }

        private void Adt_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            db = new DataContext();
            List<string> manufactr = new List<string>();
            manufactr = db.Manufacturer.Select(x => x.Name_Company).Distinct().ToList();
            manufactr.Add("новый...");
            ManufacturerTypeCmbx.ItemsSource = manufactr;
        }
    }
 
}
