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
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;

namespace TorgPred3.Views
{
    /// <summary>
    /// Interaction logic for Stock.xaml
    /// </summary>
    public partial class Invoice : UserControl
    {
        public Invoice()
        {
            InitializeComponent();
            RefreshDocs();
        }

        private Code.StockHelper helper = new Code.StockHelper();

        private void RefreshDocs()
        {
            dgDocs.ItemsSource = helper.GetLoadedDocs(dgDocs.ItemsSource as ObservableCollection<Code.Document>);
        }

        private void bAdd_Click(object sender, RoutedEventArgs e)
        {
            string current_dir = helper.GetWorkDir();
            Microsoft.Win32.OpenFileDialog open_doc = new Microsoft.Win32.OpenFileDialog();
            open_doc.DefaultExt = ".xlsx";
            open_doc.Filter = "Excel documents|*.xlsx;*.xls";
            open_doc.InitialDirectory = current_dir;
            bool doc_selected = (bool)open_doc.ShowDialog();
            if (doc_selected)
            {
                ObservableCollection<Code.StockInRecord> doc_content = helper.LoadFromExcel(open_doc.FileName);

                if (doc_content.Count() > 0)
                {
                    helper.LoadDoc2Stock(doc_content);
                }
                RefreshDocs();
            }
        }
        
        private void dgDocs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as DataGrid).CurrentItem != null)
                dgDocItems.ItemsSource = helper.GetLoadedDocItems(((sender as DataGrid).CurrentItem as Code.Document).Document_num);
        }
    }
}
