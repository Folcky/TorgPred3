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

using System.IO;

namespace TorgPred3.Windows
{
    /// <summary>
    /// Interaction logic for Reports.xaml
    /// </summary>
    public partial class Reports : Window
    {
        public Reports(string usurname, string uname)
        {
            InitializeComponent();
            USurname = usurname;
            UName = uname;
            cDF.SelectedDate = helper.GetFirstDateOfWeek(DateTime.Now, DayOfWeek.Monday);
            cDT.SelectedDate = helper.GetLastDateOfWeek(DateTime.Now, DayOfWeek.Monday);
        }

        public string USurname { get; set; }
        public string UName { get; set; }
        private Code.ReportHelper helper = new Code.ReportHelper();
        private int period_offset = 0;

        private void bReports_Click(object sender, RoutedEventArgs e)
        {
            if (helper.GetWorkDir() != "" && Directory.Exists(helper.GetWorkDir()))
            {
                if (cDF.SelectedDate != null && cDT.SelectedDate != null && cDT.SelectedDate > cDF.SelectedDate)
                {
                    helper.CreateUploads1Report((DateTime)cDF.SelectedDate, (DateTime)cDT.SelectedDate, USurname, UName);
                    helper.CreateUploads2Report((DateTime)cDF.SelectedDate, (DateTime)cDT.SelectedDate, USurname, UName);
                    helper.CreateRefuse4Report((DateTime)cDF.SelectedDate, (DateTime)cDT.SelectedDate, USurname, UName);
                    helper.CreateDealer5Report((DateTime)cDF.SelectedDate, (DateTime)cDT.SelectedDate, USurname, UName);
                    helper.CreateSPStatus6Report((DateTime)cDF.SelectedDate, (DateTime)cDT.SelectedDate, USurname, UName);
                    helper.CreateUploads7Report((DateTime)cDF.SelectedDate, (DateTime)cDT.SelectedDate, USurname, UName);
                    MessageBox.Show("Отчеты сформированы.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                    MessageBox.Show("Выбраны неправильные даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                MessageBox.Show("Рабочая директория не задана или удалена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void bPrevPeriod_Click(object sender, RoutedEventArgs e)
        {
            period_offset--;
            int offset = period_offset;
            cDF.SelectedDate = helper.GetFirstDateOfWeek(DateTime.Now, DayOfWeek.Monday, offset);
            cDF.DisplayDate = (DateTime)cDF.SelectedDate;
            cDT.SelectedDate = helper.GetLastDateOfWeek(DateTime.Now, DayOfWeek.Monday, offset);
            cDT.DisplayDate = (DateTime)cDT.SelectedDate;
        }

        private void bNextPeriod_Click(object sender, RoutedEventArgs e)
        {
            period_offset++;
            int offset = period_offset;
            cDF.SelectedDate = helper.GetFirstDateOfWeek(DateTime.Now, DayOfWeek.Monday, offset);
            cDF.DisplayDate = (DateTime)cDF.SelectedDate;
            cDT.SelectedDate = helper.GetLastDateOfWeek(DateTime.Now, DayOfWeek.Monday, offset);
            cDT.DisplayDate = (DateTime)cDT.SelectedDate;
        }

        private void bCurPeriod_Click(object sender, RoutedEventArgs e)
        {
            period_offset=0;
            int offset = period_offset;
            cDF.SelectedDate = helper.GetFirstDateOfWeek(DateTime.Now, DayOfWeek.Monday, offset);
            cDF.DisplayDate = (DateTime)cDF.SelectedDate;
            cDT.SelectedDate = helper.GetLastDateOfWeek(DateTime.Now, DayOfWeek.Monday, offset);
            cDT.DisplayDate = (DateTime)cDT.SelectedDate;
        }

    }
}
