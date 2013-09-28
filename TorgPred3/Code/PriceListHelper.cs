using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace TorgPred3.Code
{
    public class PriceListHelper : StarterHelper
    {
        public ObservableCollection<Code.PriceListRecord> LoadFromExcel(string filename)
        {
            Excel.Workbook workbook;
            Excel.Sheets worksheets;
            Excel._Worksheet worksheet;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            workbook = excel.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, false, false);
            worksheets = workbook.Worksheets;
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets.get_Item(1);
            //Код точки (без 0001)
            string price_header = CheckForNull(worksheet.Cells[1, 1].Value);
            string price1_header = CheckForNull(worksheet.Cells[1, 2].Value);
            string price2_header = CheckForNull(worksheet.Cells[1, 3].Value);
            string price3_header = CheckForNull(worksheet.Cells[1, 4].Value);
            string price4_header = CheckForNull(worksheet.Cells[1, 5].Value);
            string price5_header = CheckForNull(worksheet.Cells[1, 6].Value);
            string price6_header = CheckForNull(worksheet.Cells[1, 7].Value);
            string price7_header = CheckForNull(worksheet.Cells[1, 8].Value);
            string price8_header = CheckForNull(worksheet.Cells[1, 9].Value);
            string price9_header = CheckForNull(worksheet.Cells[1, 10].Value);
            string price10_header = CheckForNull(worksheet.Cells[1, 11].Value);
            string suplier_header = CheckForNull(worksheet.Cells[1, 12].Value);
            string category_header = CheckForNull(worksheet.Cells[1, 13].Value);

            ObservableCollection<Code.PriceListRecord> pricelist_recs = new ObservableCollection<Code.PriceListRecord>();

            if (ValidatePriceListFile(price_header,
            price1_header,
            price2_header,
            price3_header,
            price4_header,
            price5_header,
            price6_header,
            price7_header,
            price8_header,
            price9_header,
            price10_header,
            suplier_header,
            category_header))
            {
                for (int doc_iterator = 2; doc_iterator < 2000; doc_iterator++)
                {
                    Code.PriceListRecord base_item = new Code.PriceListRecord();
                    base_item.TP_name = CheckForNull(worksheet.Cells[doc_iterator, 1].Value);
                    try
                    {
                        base_item.Price_n1 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 2].Value));
                        base_item.Price_n2 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 3].Value));
                        base_item.Price_n3 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 4].Value));
                        base_item.Price_n4 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 5].Value));
                        base_item.Price_n5 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 6].Value));
                        base_item.Price_n6 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 7].Value));
                        base_item.Price_n7 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 8].Value));
                        base_item.Price_n8 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 9].Value));
                        base_item.Price_n9 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 10].Value));
                        base_item.Price_n10 = Convert.ToDecimal(CheckForNull(worksheet.Cells[doc_iterator, 11].Value));
                    }
                    catch { }
                    base_item.Suplier_name = CheckForNull(worksheet.Cells[doc_iterator, 12].Value);
                    base_item.Product_category_name = CheckForNull(worksheet.Cells[doc_iterator, 13].Value);
                    if (base_item.TP_name.Trim() == "")
                    {
                        break;
                    }
                    pricelist_recs.Add(base_item);
                }
            }
            else
            {
                MessageBox.Show("Шапка номенклатуры не соответствует установленной форме.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            workbook.Close();
            workbook = null;
            //TODO Excel не закрывается
            excel.Quit();
            if (ValidatePriceListData(pricelist_recs))
                return pricelist_recs;
            else
                return null;
        }

        private string CheckForNull(object value)
        {
            if (value != null)
                return value.ToString().Trim();
            else
                return "";
        }

        private bool ValidatePriceListFile(string price_header,
            string price1_header,
            string price2_header,
            string price3_header,
            string price4_header,
            string price5_header,
            string price6_header,
            string price7_header,
            string price8_header,
            string price9_header,
            string price10_header,
            string suplier_header,
            string category_header)
        {
            bool result = false;
            if (price_header.Trim().StartsWith("Номенклатура", StringComparison.InvariantCultureIgnoreCase)
                && price1_header.Trim().StartsWith("Цена 1", StringComparison.InvariantCultureIgnoreCase)
                && price2_header.Trim().StartsWith("Цена 2", StringComparison.InvariantCultureIgnoreCase)
                && price3_header.Trim().StartsWith("Цена 3", StringComparison.InvariantCultureIgnoreCase)
                && price4_header.Trim().StartsWith("Цена 4", StringComparison.InvariantCultureIgnoreCase)
                && price5_header.Trim().StartsWith("Цена 5", StringComparison.InvariantCultureIgnoreCase)
                && price6_header.Trim().StartsWith("Цена 6", StringComparison.InvariantCultureIgnoreCase)
                && price7_header.Trim().StartsWith("Цена 7", StringComparison.InvariantCultureIgnoreCase)
                && price8_header.Trim().StartsWith("Цена 8", StringComparison.InvariantCultureIgnoreCase)
                && price9_header.Trim().StartsWith("Цена 9", StringComparison.InvariantCultureIgnoreCase)
                && price10_header.Trim().StartsWith("Цена 10", StringComparison.InvariantCultureIgnoreCase)
                && suplier_header.Trim().StartsWith("Оператор", StringComparison.InvariantCultureIgnoreCase)
                && category_header.Trim().StartsWith("Категория",StringComparison.InvariantCultureIgnoreCase))
                result = true;
            return result;
        }

        private bool ValidatePriceListData(ObservableCollection<Code.PriceListRecord> pricelist_data)
        {
            bool result = false;
            //Найти дубликаты Способ 1
            //var count = base_data.GroupBy(p => p.SP_code_old).SelectMany(g => g.Skip(1));
            //Найти дубликаты Способ 2
            int count1 = pricelist_data.GroupBy(p => p.TP_name).Where(w => w.Count() > 1).Select(s => s.Key).Count();
            if (pricelist_data != null && pricelist_data.Count() > 1 && count1 == 0)
            {
                result = true;
            }
            return result;
        }

        public bool UpdatePriceListData(ObservableCollection<Code.PriceListRecord> new_pricelist_data)
        {
            IEnumerable<DB.PriceList> to_delete = from loaded_data in db.PriceList.ToList()
                                                select loaded_data;

            db.PriceList.DeleteAllOnSubmit(to_delete);
            try
            {
                db.SubmitChanges();
                IEnumerable<DB.PriceList> to_add = from a in new_pricelist_data
                                                   select new DB.PriceList()
                                                   {
                                                       Tp_name = a.TP_name,
                                                       Price_n1 = a.Price_n1,
                                                       Price_n2 = a.Price_n2,
                                                       Price_n3 = a.Price_n3,
                                                       Price_n4 = a.Price_n4,
                                                       Price_n5 = a.Price_n5,
                                                       Price_n6 = a.Price_n6,
                                                       Price_n7 = a.Price_n7,
                                                       Price_n8 = a.Price_n8,
                                                       Price_n9 = a.Price_n9,
                                                       Price_n10 = a.Price_n10,
                                                       Suplier_name = a.Suplier_name,
                                                       Product_category_name = a.Product_category_name
                                                   };

                db.PriceList.InsertAllOnSubmit(to_add);
                db.SubmitChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
