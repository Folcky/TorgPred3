using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace TorgPred3.Views
{
    /// <summary>
    /// Interaction logic for SalePointWork.xaml
    /// </summary>
    public partial class SalePointWork : UserControl
    {
        public SalePointWork(IEnumerable<Code.BaseRecord> salepoints)
        {
            InitializeComponent();
            this.SP_list = salepoints;
            Code.BaseRecord salepoint= salepoints.FirstOrDefault();
            lSalePointDesc.Content = String.Format("{0} {1}({2})   -   {3}   -   {4}", salepoint.Dealer_name, helper.GetActualSPCode(salepoint.SP_code_new, true), salepoint.SP_code_old, salepoint.SP_desc, salepoint.Comment);
        }

        //SalePointWork Properties
        public IEnumerable<Code.BaseRecord> SP_list
        { get; set; }
        private ObservableCollection<UserControl> _views = new ObservableCollection<UserControl>();
        public ObservableCollection<UserControl> Views { get { return _views; } }
        private Code.StarterHelper helper = new Code.StarterHelper();

        private UserControl GetViewManager(Type object_type, IEnumerable<Code.BaseRecord> tag, String report_date)
        {
            UserControl view = (from v in this.Views
                                where v.GetType() == object_type
                                && v.GetType().GetProperty("SP_list").GetValue(v, null) == tag
                                && v.GetType().GetProperty("Report_date").GetValue(v, null).ToString() == report_date
                                select v).FirstOrDefault();
            if (view == null)
            {
                view = Activator.CreateInstance(object_type, tag, report_date) as UserControl;
                this.Views.Add(view);
                return view;
            }
            else
                return view;
        }

        private void tbUploads_Checked(object sender, RoutedEventArgs e)
        {
            PageTransition1.ShowPage(GetViewManager(typeof(Views.Uploads), this.SP_list, DateTime.Now.ToString("dd.MM.yyyy")));
            tbMoneyGetter.IsChecked = false;
            tbRefusers.IsChecked = false;
            tbVisits.IsChecked = false;
        }

        private void tbMoneyGetter_Checked(object sender, RoutedEventArgs e)
        {
            PageTransition1.ShowPage(GetViewManager(typeof(Views.MoneyGetter), this.SP_list, DateTime.Now.ToString("dd.MM.yyyy")));
            tbUploads.IsChecked = false;
            tbRefusers.IsChecked = false;
            tbVisits.IsChecked = false;
        }

        private void tbRefusers_Checked(object sender, RoutedEventArgs e)
        {
            PageTransition1.ShowPage(GetViewManager(typeof(Views.Refusers), this.SP_list, DateTime.Now.ToString("dd.MM.yyyy")));
            tbUploads.IsChecked = false;
            tbMoneyGetter.IsChecked = false;
            tbVisits.IsChecked = false;
        }

        private void tbVisits_Checked(object sender, RoutedEventArgs e)
        {
            PageTransition1.ShowPage(GetViewManager(typeof(Views.Visit), this.SP_list, DateTime.Now.ToString("dd.MM.yyyy")));
            tbUploads.IsChecked = false;
            tbMoneyGetter.IsChecked = false;
            tbRefusers.IsChecked = false;
        }

        private void tbToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)tbUploads.IsChecked && !(bool)tbMoneyGetter.IsChecked && !(bool)tbRefusers.IsChecked && !(bool)tbVisits.IsChecked)
                PageTransition1.UnloadPage(PageTransition1.CurrentPage);
        }

    }
}
