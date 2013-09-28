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
    /// Interaction logic for Uploads.xaml
    /// </summary>
    public partial class Refusers : UserControl
    {
        public Refusers(IEnumerable<Code.BaseRecord> salepoints, string report_date)
        {
            InitializeComponent();
            this.SP_list = salepoints;
            this.Report_date = report_date;
            helper.FilldgRefusers(this.refusers, this.SP_list, this.Report_date);
            UpdateRefuseStatus();
            dgRefusers.ItemsSource = this.refusers;
            refusers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(refusers_CollectionChanged);
        }

        //Refusers Properties
        public IEnumerable<Code.BaseRecord> SP_list
        { get; set; }
        public string Report_date
        { get; set; }
        private ObservableCollection<Code.Refuse> _refusers = new ObservableCollection<Code.Refuse>();
        public ObservableCollection<Code.Refuse> refusers
        { get { return _refusers; } }
        private Code.RefuseHelper helper = new Code.RefuseHelper();

        protected void refusers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (Code.Refuse s in e.NewItems)
                {
                    s.SP_code_new = this.SP_list.ElementAt(0).SP_code_new;
                    s.SP_code_old = this.SP_list.ElementAt(0).SP_code_old;
                    s.Refuse_date = DateTime.ParseExact(this.Report_date, helper.date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Date;
                    s.owner_collection = refusers;
                }
        }
        
        private void dgRefusers_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.Row.DataContext.GetType() == typeof(Code.Refuse))
            {
                Code.Refuse item = e.Row.DataContext as Code.Refuse;
                (e.Row.DataContext as Code.Refuse).PropertyChanged -= new PropertyChangedEventHandler(refuserecord_PropertyChanged);
                (e.Row.DataContext as Code.Refuse).PropertyChanged += new PropertyChangedEventHandler(refuserecord_PropertyChanged);
            }
        }

        private void refuserecord_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ICC_id")
            {
                if (sender.GetType() == typeof(Code.Refuse))
                {
                    Code.Refuse item = sender as Code.Refuse;
                    //Валидируем запись, будем использовать статус в валидаторе
                    if (helper.ValidateRefuseRecord(ref item))
                    {
                        //если все ок создаем/обновляем запись в БД
                        helper.CreateOrUpdateRefuseRecord(item);
                    }
                    UpdateRefuseStatus();
                }
            }
        }

        private void dgRefusers_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //Если изменяем значение icc, нужно старый номер icc удалить из базы
            if (e.Column.Header as string == "Сим-карта" &&
                e.EditingElement.GetType() == typeof(TextBox) &&
                e.Row.DataContext.GetType() == typeof(Code.Refuse) &&
                (e.Row.DataContext as Code.Refuse).ICC_id != "")
            {
                Code.Refuse old_value = e.Row.DataContext as Code.Refuse;
                string new_icc_id = (e.EditingElement as TextBox).Text;
                if (new_icc_id != old_value.ICC_id)
                {
                    helper.DeleteRefuseRecord(old_value);
                }
            }
        }

        private void UpdateRefuseStatus()
        {
            lRefuseCount.Content = refusers.Where(p => p.ValidationStatus.Validated == true).Count().ToString();
        }

        private void dgtcICC_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !helper.SymbolIsNumber(e.Text);
        }
    }
}
