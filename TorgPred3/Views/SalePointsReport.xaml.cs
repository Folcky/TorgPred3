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
    /// Interaction logic for SalePointsReport.xaml
    /// </summary>
    public partial class SalePointsReport : UserControl
    {
        public SalePointsReport(IEnumerable<Code.BaseRecord> salepoints)
        {
            InitializeComponent();
            this.SP_list = salepoints;
            foreach (Code.BaseRecord salepoint in salepoints.OrderBy(o=>o.Dealer_name))
            {
                //lbSPList.Items.Add(new ListBoxItem() { Content = String.Format("{0} {1}({2})", salepoint.Dealer_name, salepoint.SP_code_new, salepoint.SP_code_old) });
                ShortSPInfo sp_item = new ShortSPInfo(salepoint) { DataContext = salepoint, HorizontalAlignment = HorizontalAlignment.Stretch };
                sp_item.SPSelected +=new System.ComponentModel.PropertyChangedEventHandler(sp_item_SPSelected);
                lbSPList.Items.Add(sp_item);
            }
        }

        public event PropertyChangedEventHandler SPSelected;

        public IEnumerable<Code.BaseRecord> SP_list
        { get; set; }

        private void sp_item_SPSelected(object sender, PropertyChangedEventArgs e)
        {
            if (SPSelected != null)
                SPSelected(sender, new PropertyChangedEventArgs("SPSelected"));
        }

        private void bSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SP_list != null && SP_list.Count() > 0)
            {
                Code.BaseRecord founded = (from sps in SP_list.OrderBy(o => o.Dealer_name)
                                           where
                                           sps.Dealer_name.Contains(tbSearch.Text)
                                           || sps.Area.Contains(tbSearch.Text)
                                           || sps.City.Contains(tbSearch.Text)
                                           || sps.Comment.Contains(tbSearch.Text)
                                           || sps.Contact_person.Contains(tbSearch.Text)
                                           || sps.Contact_phone.Contains(tbSearch.Text)
                                           || sps.DW.Contains(tbSearch.Text)
                                           || sps.House.Contains(tbSearch.Text)
                                           || sps.RN.Contains(tbSearch.Text)
                                           || sps.SP_code_new.Contains(tbSearch.Text)
                                           || sps.SP_code_next.Contains(tbSearch.Text)
                                           || sps.SP_code_old.Contains(tbSearch.Text)
                                           || sps.Street.Contains(tbSearch.Text)
                                           || sps.Subway_station.Contains(tbSearch.Text)
                                           select sps).OrderBy(o => o.Dealer_name).FirstOrDefault();
                ShortSPInfo result = lbSPList.Items.Cast<ShortSPInfo>().Where(w => w.DataContext == founded).FirstOrDefault();
                lbSPList.SelectedItem = result;
                //Не работает.
                //result.BringIntoView();
            }
        }
    }
}
