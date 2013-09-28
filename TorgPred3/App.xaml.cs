using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TorgPred3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //Windows.MainWindow mainwindow = new Windows.MainWindow();
            //mainwindow.Show();
            Windows.Starter start = new Windows.Starter();
            start.Show();
        }
    }
}
