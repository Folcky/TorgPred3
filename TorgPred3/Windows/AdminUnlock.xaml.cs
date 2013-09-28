using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TorgPred3.Windows
{
    /// <summary>
    /// Interaction logic for AdminUnlock.xaml
    /// </summary>
    public partial class AdminUnlock : Window
    {
        public AdminUnlock()
        {
            InitializeComponent();
        }

        private bool logined = false;
        public bool Logined { get { return logined; } set { logined = value; } }

        private void bLogin_Click(object sender, RoutedEventArgs e)
        {
            if (tbLogin.Text == Properties.Settings.Default.MainSet.Substring(1, Properties.Settings.Default.MainSet.Length - 2))
            {
                this.Logined = true;
                this.Close();
            }
        }
    }
}
