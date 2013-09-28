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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ComponentModel;

namespace TorgPred3.Views
{
    /// <summary>
    /// Interaction logic for ShortSPInfo.xaml
    /// </summary>
    public partial class ShortSPInfo : UserControl
    {
        public ShortSPInfo(Code.BaseRecord base_rec)
        {
            InitializeComponent();
            lSPFullName.Content = String.Format("Дилер {0}, точка {1}({2})", base_rec.Dealer_name, base_rec.SP_code_new, base_rec.SP_code_old);
            lSPFullAddress.Content = String.Format("{1} {0}, {2} {3}, {4}/{5}", base_rec.City,
                                                                    base_rec.City_type,
                                                                    base_rec.Street,
                                                                    base_rec.Street_type,
                                                                    base_rec.House,
                                                                    base_rec.House_build);
            lPhone.Content = String.Format("тел.: {0}, {1}", base_rec.Contact_phone, base_rec.Contact_person);
        }

        public event PropertyChangedEventHandler SPSelected;

        private void ShortSPInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SPSelected != null)
                SPSelected(this, new PropertyChangedEventArgs("SPSelected"));
        }
    }
}
