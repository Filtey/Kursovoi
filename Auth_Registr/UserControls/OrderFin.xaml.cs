﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kursovoi.Auth_Registr.UserControls
{
    /// <summary>
    /// Логика взаимодействия для OrderFin.xaml
    /// </summary>
    public partial class OrderFin : UserControl
    {
        public OrderFin()
        {
            InitializeComponent();
      //      fff = new Image { Source = new BitmapImage(new Uri(@"/Images/tshirt.png", UriKind.Relative)) };
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
        ("Title", typeof(string), typeof(OrderFin)); 
        
        
        public string Desc
        {
            get { return (string)GetValue(DescProperty); }
            set { SetValue(DescProperty, value); }
        }

        public static readonly DependencyProperty DescProperty = DependencyProperty.Register
        ("Desc", typeof(string), typeof(OrderFin)); 
        

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register
        ("Icon", typeof(string), typeof(OrderFin));
    }
}
