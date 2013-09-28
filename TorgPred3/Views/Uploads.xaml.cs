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
    public partial class Uploads : UserControl
    {
        public Uploads(IEnumerable<Code.BaseRecord> salepoints, string report_date)
        {
            InitializeComponent();
            this.SP_list = salepoints;
            this.Report_date = report_date;
            FilldgUploads();
            dgUploads.ItemsSource = uploads;
            UpdateUploadStatus();
            uploads.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(uploads_CollectionChanged);
        }

        //Uploads Properties
        public IEnumerable<Code.BaseRecord> SP_list
        { get; set; }
        public string Report_date
        { get; set; }
        private ObservableCollection<Code.Upload> _uploads = new ObservableCollection<Code.Upload>();
        public ObservableCollection<Code.Upload> uploads
        { get { return _uploads; } }
        private Code.UploadHelper helper = new Code.UploadHelper();
        private string[] date_formats = { "dd.MM.yyyy HH:mm", "dd.MM.yyyy H:mm", "dd.MM.yyyy H:mm:ss", "dd.MM.yyyy", "dd.MM.yy" };

        private void FilldgUploads()
        {
            helper.FillCurrentUploads(uploads, this.SP_list.ElementAt(0).SP_code_old, this.Report_date);
        }

        protected void uploads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (Code.Upload s in e.NewItems)
                {
                    s.SP_code_new = this.SP_list.ElementAt(0).SP_code_new;
                    s.SP_code_old = this.SP_list.ElementAt(0).SP_code_old;
                    s.Upload_date = DateTime.ParseExact(this.Report_date, date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                    s.owner_collection = this.uploads;
                }
            else
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (Code.Upload item in e.OldItems)
                    {
                        helper.DeleteUploadRecord(item);
                    }
                    UpdateUploadStatus();
                }
        }

        private void uploadrecord_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Пользователь может редактировать только icc и номинал
            if (e.PropertyName == "ICC_id" || e.PropertyName == "Nominal")
            {
                if (sender.GetType() == typeof(Code.Upload))
                {
                    Code.Upload item = sender as Code.Upload;
                    //Валидируем запись, будем использовать статус в валидаторе
                    if (helper.ValidateUploadRecord(ref item))
                    {
                        //если все ок создаем/обновляем запись в БД
                        helper.CreateOrUpdateUploadRecord(item);
                    }
                    UpdateUploadStatus();
                }
            }
        }

        private void dgUploads_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //Заканчиваем редактировать запись
            //Прикручиваем к каждой записи отслеживание изменений свойств
            if (e.Row.DataContext!=null && e.Row.DataContext.GetType() == typeof(Code.Upload))
            {
                Code.Upload item = e.Row.DataContext as Code.Upload;
                item.PropertyChanged -= new PropertyChangedEventHandler(uploadrecord_PropertyChanged);
                item.PropertyChanged += new PropertyChangedEventHandler(uploadrecord_PropertyChanged);
            }
        }

        private void dgUploads_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //Если изменяем значение icc, нужно старый номер icc удалить из базы
            //Но только если с новым значением еще нет записи
            if (e.Column.Header as string == "Сим-карта" &&
                e.EditingElement.GetType() == typeof(TextBox) &&
                e.Row.DataContext.GetType() == typeof(Code.Upload) &&
                (e.Row.DataContext as Code.Upload).ICC_id != "")
            {
                Code.Upload old_value = e.Row.DataContext as Code.Upload;
                string new_icc_id = (e.EditingElement as TextBox).Text;
                if (old_value!=null && new_icc_id != old_value.ICC_id)
                {
                    helper.DeleteUploadRecord(old_value);
                }
            }
        }

        private void UpdateUploadStatus()
        {
            lUploadCount.Content = uploads.Where(p => p.ValidationStatus.Validated == true).Count().ToString();
            lUploadSum.Content = uploads.Where(p => p.ValidationStatus.Validated == true).Sum(p => p.Price).ToString();
        }

        private void dgtcICC_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !helper.SymbolIsNumber(e.Text);
        }
    }
}
