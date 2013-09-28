using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Globalization;

namespace TorgPred3.Code
{
    public class ReportHelper : StarterHelper
    {
        private Excel.Application _excel = null;

        private bool ExcelStarted()
        {
            bool result = false;
            if(_excel==null)
            {
                try
                {
                    _excel = new Microsoft.Office.Interop.Excel.Application();
                    result = true;
                }
                catch{}
            }
            else
                result = true;
            return result;
        }

        public string GetDefaultXlsExt()
        {
            string result = "xls";
            try
            {
                if (ExcelStarted())
                {
                    string versionName = this._excel.Version;
                    int length = versionName.IndexOf('.');
                    versionName = versionName.Substring(0, length);
                    int versionNumber = int.Parse(versionName, CultureInfo.GetCultureInfo("en-US"));

                    if (versionNumber >= 12)
                    {
                        // Excel 2007 or above.
                        result = "xlsx";
                    }
                    else
                    {
                        // Excel 2003 or below.
                        result = "xls";
                    }
                }
            }
            catch { }

            return result;
        }

        //Агрегатор отгрузок
        public void CreateUploads1Report(DateTime df, DateTime dt, string usurname, string uname)
        {
            try
            {
                string filename = String.Format(@"{0}\1 Реализация {1} {2} {3}-{4}.{5}", GetWorkDir(), usurname, uname, df.ToString("dd.MM.yyyy"), dt.ToString("dd.MM.yyyy"), GetDefaultXlsExt());

                ObservableCollection<Code.StockStatus> data = GetUploadAggData(df, dt);

                if (ExcelStarted() && data.Count() > 0)
                {
                    Excel.Workbook workbook;
                    Excel.Sheets worksheets;
                    Excel._Worksheet worksheet;
                    workbook = _excel.Workbooks.Add();
                    worksheets = workbook.Worksheets;
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets.get_Item(1);

                    //Шапка
                    worksheet.Cells[1, 1] = "Торговый представитель";
                    worksheet.Cells[1, 2] = "Тарифный план";
                    worksheet.Cells[1, 3] = "Кол-во";
                    worksheet.Cells[1, 4] = "Цена";
                    worksheet.Cells[1, 5] = "Итог";
                    //Данные
                    worksheet.Cells[2, 1] = String.Format("{0} {1}", usurname, uname);
                    int i = 0;
                    for (i = 0; i < data.Count(); i++)
                    {
                        worksheet.Cells[i + 2, 2] = data.ElementAt(i).TP_name;
                        worksheet.Cells[i + 2, 3] = data.ElementAt(i).Count;
                        worksheet.Cells[i + 2, 4] = data.ElementAt(i).Nominal;
                        //worksheet.Cells[i + 2, 5] = data.ElementAt(i).Nominal;
                    }
                    //Итог
                    worksheet.Cells[i + 2, 1] = String.Format("{0} {1} (ИТОГ)", usurname, uname);
                    worksheet.Cells[i + 2, 3] = data.Sum(s => s.Count);
                    worksheet.Cells[i + 2, 5] = data.Sum(s => s.Nominal);
                    //Красота
                    worksheet.Range[String.Format("A{0}:E{1}", 1, 1)].Cells.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                    worksheet.Range[String.Format("A{0}:A{1}", 2, i + 1)].Merge();
                    worksheet.Range[String.Format("A{0}:E{1}", 1, i + 2)].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    worksheet.Range[String.Format("A{0}:E{1}", i + 2, i + 2)].Font.Size = 12;
                    worksheet.Range[String.Format("A{0}:E{1}", i + 2, i + 2)].Font.Bold = true;
                    worksheet.Range[String.Format("A{0}:E{1}", 1, i + 2)].Columns.AutoFit();

                    workbook.SaveAs(filename,
                                    Type.Missing, Type.Missing,
                                    Type.Missing, Type.Missing,
                                    Type.Missing,
                                    Excel.XlSaveAsAccessMode.xlNoChange,
                                    Type.Missing, Type.Missing,
                                    Type.Missing, Type.Missing,
                                    Type.Missing);
                    workbook.Close();
                    workbook = null;
                }
            }
            catch { }
        }

        public void CreateUploads2Report(DateTime df, DateTime dt, string usurname, string uname)
        {
            try
            {
                string filename = String.Format(@"{0}\2 Реализация {1} {2} {3}-{4}.{5}", GetWorkDir(), usurname, uname, df.ToString("dd.MM.yyyy"), dt.ToString("dd.MM.yyyy"), GetDefaultXlsExt());

                ObservableCollection<Code.Upload> data = GetUploadData(df, dt);

                if (ExcelStarted() && data.Count() > 0)
                {
                    Excel.Workbook workbook;
                    Excel.Sheets worksheets;
                    Excel._Worksheet worksheet;
                    workbook = _excel.Workbooks.Add();
                    worksheets = workbook.Worksheets;
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets.get_Item(1);

                    //Шапка
                    worksheet.Cells[1, 1] = "№_сим_карты";
                    worksheet.Cells[1, 2] = "Код_точки (действующий код МТС последние 5 цифр)";
                    //Данные
                    int i = 2;
                    foreach (Code.Upload item in data)
                    {
                        worksheet.Cells[i, 1].NumberFormat = "@";
                        worksheet.Cells[i, 1] = item.ICC_id;
                        worksheet.Cells[i, 2].NumberFormat = "@";
                        worksheet.Cells[i, 2] = GetActualSPCode(item.SP_code_old, true);
                        i++;
                    }
                    //Красота
                    worksheet.Range[String.Format("A{0}:B{1}", 1, 1)].Cells.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                    worksheet.Range[String.Format("A{0}:B{1}", 1, i - 1)].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    worksheet.Range[String.Format("A{0}:B{1}", 1, i)].Columns.AutoFit();
                    workbook.SaveAs(filename,
                                    Type.Missing, Type.Missing,
                                    Type.Missing, Type.Missing,
                                    Type.Missing,
                                    Excel.XlSaveAsAccessMode.xlNoChange,
                                    Type.Missing, Type.Missing,
                                    Type.Missing, Type.Missing,
                                    Type.Missing);
                    workbook.Close();
                    workbook = null;
                }
            }
            catch { }
        }

        public void CreateRefuse4Report(DateTime df, DateTime dt, string usurname, string uname)
        {
            try{
                string filename = String.Format(@"{0}\4 Возврат {1} {2} {3}-{4}.{5}", GetWorkDir(), usurname, uname, df.ToString("dd.MM.yyyy"), dt.ToString("dd.MM.yyyy"), GetDefaultXlsExt());

            ObservableCollection<Code.Refuse> data = GetRefuseData(df, dt);

            if (ExcelStarted() && data.Count() > 0)
            {
                Excel.Workbook workbook;
                Excel.Sheets worksheets;
                Excel._Worksheet worksheet;
                workbook = _excel.Workbooks.Add();
                worksheets = workbook.Worksheets;
                worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets.get_Item(1);

                //Шапка
                worksheet.Cells[1, 1] = "№_сим_карты";
                worksheet.Cells[1, 2] = "Код_точки (действующий код МТС последние 5 цифр)";
                //Данные
                int i = 2;
                foreach (Code.Refuse item in data)
                {
                    worksheet.Cells[i, 1] = item.ICC_id;
                    worksheet.Cells[i, 2] = GetActualSPCode(item.SP_code_old, true);
                    i++;
                }
                //Красота
                worksheet.Range[String.Format("A{0}:B{1}", 1, 1)].Cells.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                worksheet.Range[String.Format("A{0}:B{1}", 1, i - 1)].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Range[String.Format("A{0}:B{1}", 1, i)].Columns.AutoFit();
                workbook.SaveAs(filename,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing,
                                Excel.XlSaveAsAccessMode.xlNoChange,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing);
                workbook.Close();
                workbook = null;
            }
            }
            catch { }
        }

        public void CreateDealer5Report(DateTime df, DateTime dt, string usurname, string uname)
        {
            try{
                string filename = String.Format(@"{0}\5 База {1} {2} {3}-{4}.{5}", GetWorkDir(), usurname, uname, df.ToString("dd.MM.yyyy"), dt.ToString("dd.MM.yyyy"), GetDefaultXlsExt());

            //ObservableCollection<Code.BaseRecord> data = GetDealerData(df, dt, usurname, uname, BaseRecStatus.New);
            ObservableCollection<Code.BaseRecord> data = GetDealerData(df, dt, usurname, uname);
            IEnumerable<Code.MoneySum> debets = GetDebetSum(dt);
            if (ExcelStarted() && data.Count() > 0)
            {
                Excel.Workbook workbook;
                Excel.Sheets worksheets;
                Excel._Worksheet worksheet;
                workbook = _excel.Workbooks.Add();
                worksheets = workbook.Worksheets;
                worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets.get_Item(1);

                //Шапка
                worksheet.Cells[1, 1] = "Код точки (без )";
                worksheet.Cells[1, 2] = "Новый код";
                worksheet.Cells[1, 3] = "Наименование точки для внесения в 1С";
                worksheet.Cells[1, 4] = @"\";
                worksheet.Cells[1, 5] = "Наименование дилера";
                worksheet.Cells[1, 6] = "D / W";
                worksheet.Cells[1, 7] = "R / N";
                worksheet.Cells[1, 8] = "код ТП";
                worksheet.Cells[1, 9] = "старый код (временный)";
                worksheet.Cells[1, 10] = "ADR или AWR комиссионера";
                worksheet.Cells[1, 11] = "Тип точки МТС";
                worksheet.Cells[1, 12] = "Тип точки (П-профиль, Н-непрофиль)";
                worksheet.Cells[1, 13] = "описание точки (салон, стойка, ремонт, продукты, киоск, табак...)";
                worksheet.Cells[1, 14] = "статус (открыта или закрыта)";
                worksheet.Cells[1, 15] = "ТП МТС";
                worksheet.Cells[1, 16] = "ТП БИ+МЕГА";
                worksheet.Cells[1, 17] = "Ставка по комисии (если платим)";
                worksheet.Cells[1, 18] = "Кто оформляет инфокарты";
                worksheet.Cells[1, 19] = "Населенный пункт";
                worksheet.Cells[1, 20] = "тип населенного пункта";
                worksheet.Cells[1, 21] = "Округ/направление";
                worksheet.Cells[1, 22] = "Метро";
                worksheet.Cells[1, 23] = "(улица)";
                worksheet.Cells[1, 24] = "Тип улицы";
                worksheet.Cells[1, 25] = "Номер дома";
                worksheet.Cells[1, 26] = "Номер строения";
                worksheet.Cells[1, 27] = "Контактное лицо";
                worksheet.Cells[1, 28] = "Контактный телефон";
                worksheet.Cells[1, 29] = "Описание, комментарий";
                worksheet.Cells[1, 30] = "Сколько раз точка посещается в месяц";
                worksheet.Cells[1, 31] = "Цена мтс";
                worksheet.Cells[1, 32] = "Цена Билайн";
                worksheet.Cells[1, 33] = "Цена Мегафон";
                worksheet.Cells[1, 34] = "Дата последнего посещения ТТ ТП";
                worksheet.Cells[1, 35] = "ДЗ ИТОГ";
                //worksheet.Cells[1, 34] = "Дата последнего посещения ТТ ТП МТС";
                //worksheet.Cells[1, 35] = "Дата последнего посещения ТТ ТП Би+Мега";
                //worksheet.Cells[1, 36] = "ДЗ ТТ ТП МТС";
                //worksheet.Cells[1, 37] = "ДЗ ТТ ТП Би+Мега";
                //worksheet.Cells[1, 38] = "ДЗ ИТОГ";
                //Данные
                int i = 2;
                foreach (Code.BaseRecord item in data)
                {
                    MoneySum d_and_dz = debets.Where(w => w.SP_code_old == item.SP_code_old).FirstOrDefault();
                    worksheet.Cells[i, 1] = item.SP_code_new;
                    worksheet.Cells[i, 2] = item.SP_code_next;
                    worksheet.Cells[i, 3] = String.Format(@"{0} {1}{2}{3}\{4}", item.Dealer_name, item.DW, item.RN, "", GetActualSPCode(item.SP_code_old, true));
                    worksheet.Cells[i, 4] = @"\";
                    worksheet.Cells[i, 5] = item.Dealer_name;
                    worksheet.Cells[i, 6] = item.DW;
                    worksheet.Cells[i, 7] = item.RN;
                    worksheet.Cells[i, 8] = item.Zone;
                    worksheet.Cells[i, 9] = item.SP_code_old;
                    worksheet.Cells[i, 10] = item.ADR_awr;
                    worksheet.Cells[i, 11] = item.Suplier_SP_type;
                    worksheet.Cells[i, 12] = item.SP_profile_type;
                    worksheet.Cells[i, 13] = item.SP_desc;
                    worksheet.Cells[i, 14] = item.SP_status;
                    worksheet.Cells[i, 15] = item.Torgpred1;
                    worksheet.Cells[i, 16] = item.Torgpred2;
                    worksheet.Cells[i, 17] = item.Comm_rate;
                    worksheet.Cells[i, 18] = item.Infocart_reg;
                    worksheet.Cells[i, 19] = item.City;
                    worksheet.Cells[i, 20] = item.City_type;
                    worksheet.Cells[i, 21] = item.Area;
                    worksheet.Cells[i, 22] = item.Subway_station;
                    worksheet.Cells[i, 23] = item.Street;
                    worksheet.Cells[i, 24] = item.Street_type;
                    worksheet.Cells[i, 25] = item.House;
                    worksheet.Cells[i, 26] = item.House_build;
                    worksheet.Cells[i, 27] = item.Contact_person;
                    worksheet.Cells[i, 28] = item.Contact_phone;
                    worksheet.Cells[i, 29] = item.Comment;
                    worksheet.Cells[i, 30] = item.Visit_number;
                    worksheet.Cells[i, 31] = item.Mts_price;
                    worksheet.Cells[i, 32] = item.Beeline_price;
                    worksheet.Cells[i, 33] = item.Megafon_price;
                    if (d_and_dz != null)
                    {
                        worksheet.Cells[i, 34] = d_and_dz.Money_date.ToString("dd.MM.yyyy");
                        worksheet.Cells[i, 35] = d_and_dz.Money_sum;
                    }
                    //worksheet.Cells[i, 34] = "";
                    //worksheet.Cells[i, 35] = "";
                    //worksheet.Cells[i, 36] = "";
                    //worksheet.Cells[i, 37] = "";
                    //worksheet.Cells[i, 38] = debets.Where(w => w.SP_code_old == item.SP_code_old).Select(s=>s.Money_sum).FirstOrDefault();
                    i++;
                }
                //Красота
                worksheet.Range[String.Format("A{0}:AI{1}", 1, 1)].Cells.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                worksheet.Range[String.Format("A{0}:AI{1}", 1, i - 1)].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Range[String.Format("A{0}:AI{1}", 1, i)].Columns.AutoFit();

                //Сохранение
                workbook.SaveAs(filename,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing,
                                Excel.XlSaveAsAccessMode.xlNoChange,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing);
                workbook.Close();
                workbook = null;
            }
            }
            catch { }
        }

        public void CreateSPStatus6Report(DateTime df, DateTime dt, string usurname, string uname)
        {
            try{
                string filename = String.Format(@"{0}\6 Изменение статусов ТТ {1} {2} {3}-{4}.{5}", GetWorkDir(), usurname, uname, df.ToString("dd.MM.yyyy"), dt.ToString("dd.MM.yyyy"), GetDefaultXlsExt());

            ObservableCollection<Code.BaseRecord> data = GetDealerStatusData(df, dt);

            if (ExcelStarted() && data.Count() > 0)
            {
                Excel.Workbook workbook;
                Excel.Sheets worksheets;
                Excel._Worksheet worksheet;
                workbook = _excel.Workbooks.Add();
                worksheets = workbook.Worksheets;
                worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets.get_Item(1);

                //Шапка
                worksheet.Cells[1, 1] = "Код ТТ";
                worksheet.Cells[1, 2] = "Было";
                worksheet.Cells[1, 3] = "Стало";
                //Данные
                int i = 2;
                foreach (Code.BaseRecord item in data)
                {
                    worksheet.Cells[i, 1].NumberFormat = "@";
                    worksheet.Cells[i, 1] = "0001"+GetActualSPCode(item.SP_code_old, true);
                    worksheet.Cells[i, 3] = item.SP_status;
                    i++;
                }
                //Красота
                worksheet.Range[String.Format("A{0}:C{1}", 1, 1)].Cells.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                worksheet.Range[String.Format("A{0}:C{1}", 1, i - 1)].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Range[String.Format("A{0}:C{1}", 1, i)].Columns.AutoFit();
                workbook.SaveAs(filename,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing,
                                Excel.XlSaveAsAccessMode.xlNoChange,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing);
                workbook.Close();
                workbook = null;
            }
            }
            catch { }
        }

        public void CreateUploads7Report(DateTime df, DateTime dt, string usurname, string uname)
        {
            try{
                string filename = String.Format(@"{0}\7 Отчет в МТС {1} {2} {3}-{4}.{5}", GetWorkDir(), usurname, uname, df.ToString("dd.MM.yyyy"), dt.ToString("dd.MM.yyyy"), GetDefaultXlsExt());

            ObservableCollection<Code.Upload> data = GetUploadData(df, dt);

            if (ExcelStarted() && data.Count() > 0)
            {
                Excel.Workbook workbook;
                Excel.Sheets worksheets;
                Excel._Worksheet worksheet;
                workbook = _excel.Workbooks.Add();
                worksheets = workbook.Worksheets;
                worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets.get_Item(1);

                //Шапка
                worksheet.Cells[1, 1] = "№_сим_карты";
                worksheet.Cells[1, 2] = "Код_точки (действующий код МТС)";
                worksheet.Cells[1, 3] = "Дата_отгрузки";
                //Данные
                int i = 2;
                foreach (Code.Upload item in data)
                {
                    worksheet.Cells[i, 1].NumberFormat = "@";
                    worksheet.Cells[i, 1] = item.ICC_id;
                    worksheet.Cells[i, 2].NumberFormat = "@";
                    worksheet.Cells[i, 2] = "0001"+GetActualSPCode(item.SP_code_old, true);
                    worksheet.Cells[i, 3] = item.Upload_date.Date.ToString("dd.MM.yyyy");
                    i++;
                }
                //Красота
                worksheet.Range[String.Format("A{0}:C{1}", 1, 1)].Cells.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                worksheet.Range[String.Format("A{0}:C{1}", 1, i - 1)].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Range[String.Format("A{0}:C{1}", 1, i)].Columns.AutoFit();
                workbook.SaveAs(filename,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing,
                                Excel.XlSaveAsAccessMode.xlNoChange,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing);
                workbook.Close();
                workbook = null;
            }
            }
            catch { }
        }

        public ObservableCollection<Code.Upload> GetUploadData(DateTime df, DateTime dt)
        {
            ObservableCollection<Code.Upload> result = new ObservableCollection<Upload>(from u in db.Uploads
                                                                                        where u.Upload_date.Date >= df.Date
                                                                                        && u.Upload_date.Date <= dt.Date
                                                                                        select DbUpload2CodeUpload(u));
            return result;
        }

        public ObservableCollection<Code.Refuse> GetRefuseData(DateTime df, DateTime dt)
        {
            ObservableCollection<Code.Refuse> result = new ObservableCollection<Refuse>(from r in db.Refusers
                                                                                        where r.Refuse_date.Date >= df.Date
                                                                                        && r.Refuse_date.Date <= dt.Date
                                                                                        select DbRefuse2CodeRefuse(r));
            return result;
        }

        public ObservableCollection<Code.StockStatus> GetUploadAggData(DateTime df, DateTime dt)
        {
            ObservableCollection<Code.StockStatus> result = new ObservableCollection<StockStatus>(from u in db.Uploads
                                                                                                  where u.Upload_date.Date >= df.Date
                                                                                                  && u.Upload_date.Date <= dt.Date
                                                                                                  group u by new { u.Tp_name, u.Price } into r
                                                                                                  select new Code.StockStatus()
                                                                                                  {
                                                                                                      TP_name = r.Key.Tp_name,
                                                                                                      Nominal = r.Sum(s => s.Price),
                                                                                                      Count = r.Count()
                                                                                                  });
            return result;
        }

        public ObservableCollection<Code.BaseRecord> GetDealerStatusData(DateTime df, DateTime dt)
        {
            ObservableCollection<Code.BaseRecord> result = new ObservableCollection<BaseRecord>(from d in db.Dealers
                                                                                                where d.Record_status != 0
                                                                                                select DbDealer2CodeBaseRecord(d));
            return result;
        }

        public ObservableCollection<Code.BaseRecord> GetDealerData(DateTime df, DateTime dt, string usurname, string uname, BaseRecStatus status)
        {
            return new ObservableCollection<Code.BaseRecord>(from b in db.Dealers
                                                             where (b.TorgPred1.Replace(" ", "").ToLower() == String.Format("{0}{1}", usurname, uname).Replace(" ", "").ToLower()
                                                             || b.TorgPred2.Replace(" ", "").ToLower() == String.Format("{0}{1}", usurname, uname).Replace(" ", "").ToLower())
                                                             && b.Record_status == (int)status
                                                             select DbDealer2CodeBaseRecord(b));
        }

        public ObservableCollection<Code.BaseRecord> GetDealerData(DateTime df, DateTime dt, string usurname, string uname)
        {
            IEnumerable<Code.MoneySum> debets = GetDebetSum(dt);
            return new ObservableCollection<Code.BaseRecord>(from b in db.Dealers
                                                             where (b.TorgPred1.Replace(" ", "").ToLower() == String.Format("{0}{1}", usurname, uname).Replace(" ", "").ToLower()
                                                             || b.TorgPred2.Replace(" ", "").ToLower() == String.Format("{0}{1}", usurname, uname).Replace(" ", "").ToLower())
                                                             select DbDealer2CodeBaseRecord(b));
        }
    }
}
