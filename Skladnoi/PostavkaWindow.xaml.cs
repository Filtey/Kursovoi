using StoreSystem.Classes;
using StoreSystem.ConnectToDB;
using StoreSystem.ConnectToDB.Model;
using StoreSystem.ConnectToDB.Model.ApiCRUDs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace StoreSystem.Skladnoi
{
    /// <summary>
    /// Логика взаимодействия для PostavkaWindow.xaml
    /// </summary>
    public partial class PostavkaWindow : Window
    {
        APIClass db;
      
        public PostavkaWindow()
        {
            InitializeComponent();
            DataGridtable.ItemsSource = TovarsListForPostavka.tovarslist;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddPostClick(object sender, RoutedEventArgs e)
        {
            db = new APIClass();
            List<Sklad> s = db.SkladList();
            if (TovarsListForPostavka.tovarslist.Count == 0)
            {
                MessageBox.Show("В списке поставленных товаров ничего нет!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            History hry = new History { Date = DateTime.Now };
            db.AddHistory(hry);
            foreach (var item in TovarsListForPostavka.tovarslist)
            { 
                
                Sklad skladUpd = s.Where(x => x.Tovar_id == item.tovar.Tovar_id).First();
                skladUpd.Count += item.Count;

                Shipment sh = new Shipment();
                sh.Tovar_id = skladUpd.Tovar_id;
                sh.Unit = skladUpd.unit;
                sh.Count = item.Count;
                sh.Purchase_price = skladUpd.Purchase_price;
                sh.History_id =db.HistoryList().Last().History_id;

                db.UpdateSklad(skladUpd);
                db.AddShipment(sh);
            }


          
           
          //  db.SaveChanges();
            MessageBox.Show("Успешно!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            TovarsListForPostavka.tovarslist.Clear();
            TovarsListForPostavka.NumberI = 0;

            //из tovarslist в бд
        }

        /// <summary>
        /// добавить товар в список поставки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTovarInSP(object sender, RoutedEventArgs e)
        {
            AddTovarInPostAsMessageBox add = new AddTovarInPostAsMessageBox();
            add.Closing += Adt_Closing;
            add.ShowDialog();

        }


        private void Adt_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            DataGridtable.ItemsSource = null;
            DataGridtable.ItemsSource = TovarsListForPostavka.tovarslist;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
      
        private void RemoveTovar(object sender, RoutedEventArgs e)
        {
            MessageBoxResult rez = MessageBox.Show("Вы точно хотите удалить данный товар?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (rez == MessageBoxResult.Yes)
            {
                DatagridPostavka Delete = (DatagridPostavka)(sender as FrameworkElement).DataContext;
                //полученный выбранный элемент датагрид удаляем
                DataGridtable.ItemsSource = null;
                TovarsListForPostavka.tovarslist.Remove(Delete);
                TovarsListForPostavka.NumberI--;

                int i = 0;
                foreach (var item in TovarsListForPostavka.tovarslist)
                {
                    i++;
                    item.Number = i;
                }
                TovarsListForPostavka.NumberI = i;

                DataGridtable.ItemsSource = TovarsListForPostavka.tovarslist;
                MessageBox.Show("Успешно!", "Уведомление", MessageBoxButton.OK);               
            }
        }

    }
}
