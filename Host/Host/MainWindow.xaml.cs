﻿using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
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

namespace Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        ServiceHost host;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            host = new ServiceHost(typeof(FourRowService));
            host.Description.Behaviors.Add(new ServiceMetadataBehavior { 
            HttpGetEnabled = true
            });
            host.Open();
            status.Content = "Service is Running";
        }
    }
}
