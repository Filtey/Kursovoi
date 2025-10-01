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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StoreSystem.Skladnoi
{
    /// <summary>
    /// Логика взаимодействия для ImportInFileOrHandWindow.xaml
    /// </summary>
    public partial class ImportInFileOrHandWindow : Window
    {
        public ImportInFileOrHandWindow()
        {
            InitializeComponent();
        }

        private void FileButtonClick(object sender, RoutedEventArgs e)
        {
            //filedialog
        }

        private void HandButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddTovar adt = new AddTovar();
                adt.Closing += Adt_Closing;
                adt.ShowDialog();
            }
            catch (Exception ee)
            {
                var c = ee.Message;
                MessageBox.Show("Проверьте своё подключение к Интернету!", "Нет соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void Adt_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
           Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
