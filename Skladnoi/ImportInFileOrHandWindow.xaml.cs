using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using StoreSystem.Classes;
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
using System.Globalization;

namespace StoreSystem.Skladnoi
{
    /// <summary>
    /// Логика взаимодействия для ImportInFileOrHandWindow.xaml
    /// </summary>
    public partial class ImportInFileOrHandWindow : Window
    {
        public ObservableCollection<DatagridSklad> Items;
        string PostavkaOrNewTovar;
        public ImportInFileOrHandWindow(string str)
        {
            InitializeComponent();
            PostavkaOrNewTovar = str;
        }


        //заносят данные из файла
        private void FileButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PostavkaOrNewTovar == "поставка")
                {
                    var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Excel|*.xlsx;*.xls" };
                    if (dlg.ShowDialog() == true)
                    {
                        ImportExportTovarToXLSX.ImportSkladDeltaFromExcel(new APIClass(), dlg.FileName);
                        // Обновим грид
                        Items = new ObservableCollection<DatagridSklad>(
                            new APIClass().SkladList().Join(
                                new APIClass().TovarList(),
                                s => s.Tovar_id, t => t.Tovar_id,
                                (s, t) => new DatagridSklad { sklad = s, tovar = t, NameTovar = t.Name, Number = 0, BgColor = Brushes.Transparent }
                            )
                        );
                        MessageBox.Show("Поставка успешно завершена. Данные были изменены.", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                }
                else if (PostavkaOrNewTovar == "новый товар")
                {
                    var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Excel|*.xlsx;*.xls" };
                    if (dlg.ShowDialog() == true)
                    {
                        var api = new APIClass();
                        ImportExportTovarToXLSX.ImportNewTovarsFromExcel(api, dlg.FileName);

                        // при желании — обновить грид
                        var sklads = api.SkladList() ?? new List<Sklad>();
                        var tovars = api.TovarList() ?? new List<Tovar>();
                        Items = new ObservableCollection<DatagridSklad>(
                            from s in sklads
                            join t in tovars on s.Tovar_id equals t.Tovar_id
                            select new DatagridSklad { sklad = s, tovar = t, NameTovar = t.Name, BgColor = Brushes.Transparent }
                        );
                        MessageBox.Show("Информация успешно занесена в систему.", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка! Проверьте корректность данных в файле.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        //данные заносятся вручную
        private void HandButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PostavkaOrNewTovar == "новый товар")
                {
                    AddTovar adt = new AddTovar();
                    adt.Closing += Adt_Closing;
                    adt.ShowDialog();
                }
                else if (PostavkaOrNewTovar == "поставка")
                {
                    PostavkaWindow adt = new PostavkaWindow();
                    adt.Closing += Adt_Closing;
                    adt.ShowDialog();
                }
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        //при закрытии окна добавления закрываем и это окно
        private void Adt_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
           Close();
        }
        //чтоб можно было перетаскивать
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        //крестик сверху справа
        private void ExitApp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
