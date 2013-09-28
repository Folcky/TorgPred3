using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;
using System.Windows;

namespace TorgPred3.Code
{
    public class StockHelper : StarterHelper
    {
        public ObservableCollection<Code.StockInRecord> LoadFromExcel(string filename)
        {
            Excel.Workbook workbook;
            Excel.Sheets worksheets;
            Excel._Worksheet worksheet;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            workbook = excel.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, false, false);
            worksheets = workbook.Worksheets;
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets.get_Item(1);
            //Расходная накладная № 34789 от 10.01.13
            string doc_header = CheckForNull(worksheet.Cells[2, 2].Value, typeof(string));
            string stockinner_header = CheckForNull(worksheet.Cells[5, 2].Value, typeof(string));
            string stockoutter_header = CheckForNull(worksheet.Cells[6, 2].Value, typeof(string));
            string punkt_header = CheckForNull(worksheet.Cells[8, 2].Value, typeof(string));
            string icc_header = CheckForNull(worksheet.Cells[8, 3].Value, typeof(string));
            string msisdn_header = CheckForNull(worksheet.Cells[8, 4].Value, typeof(string));
            string comment_header = CheckForNull(worksheet.Cells[8, 5].Value, typeof(string));
            string tp_header = CheckForNull(worksheet.Cells[8, 6].Value, typeof(string));
            string nominal_header = CheckForNull(worksheet.Cells[8, 7].Value, typeof(string));
            ObservableCollection<Code.StockInRecord> doc_recs = new ObservableCollection<Code.StockInRecord>();

            if (ValidateStockInFile(doc_header,
                                    stockinner_header,
                                    stockoutter_header,
                                    punkt_header,
                                    icc_header,
                                    msisdn_header,
                                    comment_header,
                                    tp_header,
                                    nominal_header))
            {
                for (int doc_iterator = 9; doc_iterator < 2000; doc_iterator++)
                {
                    Code.StockInRecord doc_item = new Code.StockInRecord();
                    try
                    {
                        doc_item.Icc_id = CheckForNull(worksheet.Cells[doc_iterator, 3].Value, typeof(string));
                        doc_item.Msisdn = CheckForNull(worksheet.Cells[doc_iterator, 4].Value, typeof(string));
                        doc_item.Comment = CheckForNull(worksheet.Cells[doc_iterator, 5].Value, typeof(string));
                        doc_item.TP_name = CheckForNull(worksheet.Cells[doc_iterator, 6].Value, typeof(string));
                        doc_item.Nominal = CheckForNull(worksheet.Cells[doc_iterator, 7].Value, typeof(decimal));
                        doc_item.Doc_num = doc_header;
                        if (doc_item.Icc_id.Trim() == "")
                        {
                            break;
                        }
                        doc_recs.Add(doc_item);
                    }
                    catch
                    {
                        doc_recs.Clear();
                        MessageBox.Show(String.Format("Ошибка загрузки накладной. Посмотрите строку для {0} {1}", icc_header, CheckForNull(worksheet.Cells[doc_iterator, 3].Value, typeof(string))), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Шапка накладной не соответствует установленной форме.","Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            workbook.Close();
            workbook = null;
            //TODO Excel не закрывается
            excel.Quit();
            if (!ValidateStockInData(doc_recs)) doc_recs.Clear();
            return doc_recs;
        }

        private object CheckForNull(object value, Type value_type)
        {
            if (value != null)
                return Convert.ChangeType(value, value_type);
            else
                if (value_type == typeof(decimal) || value_type == typeof(double) || value_type == typeof(int))
                    return 0;
                else
                    return "";
        }

        private bool ValidateStockInData(ObservableCollection<Code.StockInRecord> recs)
        {
            bool result = true;
            int tp_notfounded = (from l in recs
                                 join p in db.PriceList on l.TP_name.ToLower() equals p.Tp_name.ToLower() into gp
                                 from prices in gp.DefaultIfEmpty()
                                 where prices == null
                                 select l).Count();
            if (tp_notfounded > 0)
            {
                MessageBox.Show("Тарифные планы по номенклатуре не найдены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                result = false; 
            }

            int maxlen_founded = (from l in recs
                                 where l.Icc_id.Length > 20
                                 select l).Count();
            if (maxlen_founded > 0)
            {
                MessageBox.Show("Найдены записи с номерами длиной более 20 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                result = false;
            }

            return result;
        }

        private bool ValidateStockInFile(string header,
            string stockinner,
            string stockoutter,
            string punkt,
            string icc,
            string msisdn,
            string comment,
            string tp,
            string nominal)
        {
            bool result = false;
            if (header.Trim().StartsWith("Расходная накладная")
                && stockinner.Trim().StartsWith("Поставщик")
                && stockoutter.Trim().StartsWith("Получатель")
                && punkt.Trim().StartsWith("№ п/п")
                && icc.Trim().StartsWith("№ СИМ-карты")
                && msisdn.Trim().StartsWith("Вызывной номер")
                && comment.Trim().StartsWith("Примечание")
                && tp.Trim().StartsWith("Тарифный план")
                && nominal.Trim().StartsWith("Номинал"))
                result = true;
            return result;
        }

        public ObservableCollection<Code.Document> GetLoadedDocs(ObservableCollection<Code.Document> refresh_docs)
        {
            ObservableCollection<Code.Document> doc_recs = new ObservableCollection<Code.Document>(
                from l in db.Stock
                group l by l.Document_num into r
                select new Code.Document()
                {
                    Document_num = r.Key,
                    Doc_date = (DateTime)r.Max(d=>d.Document_date),
                    Operation_date = (DateTime)r.Max(d => d.Operation_date)
                }
                );

            return doc_recs;
        }

        public ObservableCollection<DocumentItem> GetLoadedDocItems(string Doc_num)
        {
            //Отгрузка всегда привязана к накладной
            ObservableCollection<DocumentItem> doc_items = new ObservableCollection<DocumentItem>(
                from l in db.Stock
                where l.Document_num == Doc_num
                join u in db.Uploads on new { l.Barcode, l.Document_num } equals new { u.Barcode, u.Document_num } into gj
                from uploads in gj.DefaultIfEmpty()
                select new DocumentItem()
                {
                    ICC_id=l.Barcode,
                    Upload_date = uploads == null ? new DateTime(1900, 1, 1) : uploads.Upload_date
                });

            return doc_items;
        }

        public ObservableCollection<Code.StockStatus> GetStockStatus()
        {
            ObservableCollection<Code.StockStatus> stocksatus = new ObservableCollection<Code.StockStatus>(
                from l in db.Stock
                join u in db.Uploads on new { l.Barcode, l.Document_num } equals new { u.Barcode, u.Document_num } into gj
                from uploads in gj.DefaultIfEmpty()
                join p in db.PriceList on l.Tp_name equals p.Tp_name into gp
                from prices in gp.DefaultIfEmpty()
                where uploads == null
                group l by new { prices.Suplier_name, l.Tp_name, l.Nominal } into r
                select new Code.StockStatus()
                {
                    Suplier_name = r.Key.Suplier_name,
                    TP_name = r.Key.Tp_name,
                    Nominal = r.Key.Nominal,
                    Count = r.Count()
                }
                );

            return stocksatus;
        }

        public void LoadDoc2Stock(ObservableCollection<Code.StockInRecord> doc_content)
        {
            foreach (Code.StockInRecord doc_item in doc_content)
            {
                DB.Stock stock_record = db.Stock.Where(w => w.Barcode == doc_item.Icc_id && w.Document_num == doc_item.Doc_num).FirstOrDefault();
                if (stock_record == null)
                {
                    stock_record = new DB.Stock();
                    stock_record.Barcode = doc_item.Icc_id;
                    stock_record.Nominal = doc_item.Nominal;
                    stock_record.Tp_name = doc_item.TP_name;
                    stock_record.Operation_date = DateTime.Now;
                    stock_record.Document_num = doc_item.Doc_num;
                    stock_record.Document_date = doc_item.Doc_date;
                    db.Stock.InsertOnSubmit(stock_record);
                }
            }
            try
            {
                db.SubmitChanges();
            }
            catch { }
        }
    }
}
