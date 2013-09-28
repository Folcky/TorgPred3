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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;

namespace TorgPred3.Views
{
    /// <summary>
    /// Interaction logic for MoneyGetter.xaml
    /// </summary>
    public partial class Visit : UserControl
    {
        public Visit(IEnumerable<Code.BaseRecord> salepoints, string report_date)
        {
            InitializeComponent();
            this.SP_list = salepoints;
            this.Report_date = report_date;
            dgVisit.ItemsSource = visits;
            FilldgVisits();
            FillSPStatuses();
        }

        //Money Properties
        public IEnumerable<Code.BaseRecord> SP_list
        { get; set; }
        public string Report_date
        { get; set; }
        private ObservableCollection<Code.Visit> _visits = new ObservableCollection<Code.Visit>();
        public ObservableCollection<Code.Visit> visits
        { get { return _visits; } }
        private Code.VisitHelper helper = new Code.VisitHelper();

        private void FilldgVisits()
        {
            helper.FillVisits(visits, this.SP_list.ElementAt(0).SP_code_old, this.Report_date);
        }

        private void FillSPStatuses()
        {
            ObservableCollection<ComboBoxItem> statuses = new ObservableCollection<ComboBoxItem>(helper.GetSPStatuses(this.SP_list.ElementAt(0).SP_code_old));
            cbSPStatus.ItemsSource = statuses;
            ComboBoxItem selected_status = statuses.Where(w => w.IsSelected == true).FirstOrDefault();
            cbSPStatus.SelectedItem = selected_status;
        }

        private void bVisit_Click(object sender, RoutedEventArgs e)
        {
            helper.TryVisitSP(visits, this.SP_list.ElementAt(0).SP_code_old, this.Report_date);
        }
    }
}
