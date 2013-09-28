using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;
using System.Globalization;

using System.Windows;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Data.Linq.Mapping;

namespace TorgPred3.Code
{
    public class BaseHelper : StarterHelper
    {
        public string USurname { get; set; }
        public string UName { get; set; }

        public ObservableCollection<Code.BaseRecord> LoadFromExcel(string filename)
        {
            Excel.Workbook workbook;
            Excel.Sheets worksheets;
            Excel._Worksheet worksheet;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            workbook = excel.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, false, false);
            worksheets = workbook.Worksheets;
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets.get_Item(1);
            //Код точки (без 0001)
            string doc_header = CheckForNull(worksheet.Cells[1, 1].Value);
            string doc_dw_header = CheckForNull(worksheet.Cells[1, 6].Value);
            string doc_rn_header = CheckForNull(worksheet.Cells[1, 7].Value);

            string doc_tpmts_header = CheckForNull(worksheet.Cells[1, 15].Value);
            string doc_tpbeemega_header = CheckForNull(worksheet.Cells[1, 16].Value);

            string doc_subway_header = CheckForNull(worksheet.Cells[1, 22].Value);
            string doc_contactperson_header = CheckForNull(worksheet.Cells[1, 27].Value);
            string doc_beelineprice_header = CheckForNull(worksheet.Cells[1, 32].Value);
            string doc_megaprice_header = CheckForNull(worksheet.Cells[1, 33].Value);
            ObservableCollection<Code.BaseRecord> base_recs = new ObservableCollection<Code.BaseRecord>();

            if (ValidateBaseFile(doc_header, doc_dw_header,
                                            doc_rn_header,
                                            doc_tpmts_header,
                                            doc_tpbeemega_header,
                                            doc_subway_header,
                                            doc_contactperson_header,
                                            doc_beelineprice_header,
                                            doc_megaprice_header))
            {
                for (int doc_iterator = 2; doc_iterator < 2000; doc_iterator++)
                {
                    Code.BaseRecord base_item = new Code.BaseRecord();
                    base_item.ValidationStatus = new ValidationStatus();
                    base_item.Dealer_name = CheckForNull(worksheet.Cells[doc_iterator, 5].Value);
                    base_item.DW = CheckForNull(worksheet.Cells[doc_iterator, 6].Value);
                    base_item.RN = CheckForNull(worksheet.Cells[doc_iterator, 7].Value);
                    try
                    {
                        base_item.Zone = Convert.ToInt16(CheckForNull(worksheet.Cells[doc_iterator, 8].Value));
                    }
                    catch
                    {
                        base_item.ValidationStatus.Validated = false;
                        base_item.ValidationStatus.Error_text = "Ошибка загрузки зоны";
                    }
                    base_item.SP_code_new = CheckForNull(worksheet.Cells[doc_iterator, 1].Value);
                    base_item.SP_code_next = CheckForNull(worksheet.Cells[doc_iterator, 2].Value);
                    base_item.SP_code_old = CheckForNull(worksheet.Cells[doc_iterator, 9].Value);

                    base_item.ADR_awr = CheckForNull(worksheet.Cells[doc_iterator, 10].Value);
                    base_item.Suplier_SP_type = CheckForNull(worksheet.Cells[doc_iterator, 11].Value);
                    base_item.SP_profile_type = CheckForNull(worksheet.Cells[doc_iterator, 12].Value);
                    base_item.SP_desc = CheckForNull(worksheet.Cells[doc_iterator, 13].Value);

                    base_item.SP_status = CheckForNull(worksheet.Cells[doc_iterator, 14].Value);
                    base_item.Torgpred1 = CheckForNull(worksheet.Cells[doc_iterator, 15].Value);
                    base_item.Torgpred2 = CheckForNull(worksheet.Cells[doc_iterator, 16].Value);

                    try
                    {
                        base_item.Comm_rate = Convert.ToDecimal(TryConvertCommRate(CheckForNull(worksheet.Cells[doc_iterator, 17].Value)));
                    }
                    catch
                    {
                        base_item.ValidationStatus.Validated = false;
                        base_item.ValidationStatus.Error_text = "Ошибка загрузки ставки комиссии";
                    }
                    base_item.Infocart_reg = CheckForNull(worksheet.Cells[doc_iterator, 18].Value);

                    base_item.City = CheckForNull(worksheet.Cells[doc_iterator, 19].Value);
                    base_item.City_type = CheckForNull(worksheet.Cells[doc_iterator, 20].Value);
                    base_item.Area = CheckForNull(worksheet.Cells[doc_iterator, 21].Value);

                    base_item.Subway_station = CheckForNull(worksheet.Cells[doc_iterator, 22].Value);
                    base_item.Street = CheckForNull(worksheet.Cells[doc_iterator, 23].Value);
                    base_item.Street_type = CheckForNull(worksheet.Cells[doc_iterator, 24].Value);

                    base_item.House = CheckForNull(worksheet.Cells[doc_iterator, 25].Value);
                    base_item.House_build = CheckForNull(worksheet.Cells[doc_iterator, 26].Value);

                    base_item.Contact_person = CheckForNull(worksheet.Cells[doc_iterator, 27].Value);
                    base_item.Contact_phone = CheckForNull(worksheet.Cells[doc_iterator, 28].Value);
                    base_item.Comment = CheckForNull(worksheet.Cells[doc_iterator, 29].Value);
                    try
                    {
                        base_item.Visit_number = Convert.ToInt16(CheckForNull(worksheet.Cells[doc_iterator, 30].Value));
                    }
                    catch
                    {
                        base_item.ValidationStatus.Validated = false;
                        base_item.ValidationStatus.Error_text = "Ошибка загрузки кол-ва посещений";
                    }
                    try
                    {
                        base_item.Mts_price = Convert.ToInt16(CheckForNull(worksheet.Cells[doc_iterator, 31].Value));
                        if (base_item.Mts_price < 1 || base_item.Beeline_price > 10)
                        {
                            base_item.ValidationStatus.Validated = false;
                            base_item.ValidationStatus.Error_text = "Ошибка загрузки цены для МТС";
                        }
                    }
                    catch
                    {
                        base_item.ValidationStatus.Validated = false;
                        base_item.ValidationStatus.Error_text = "Ошибка загрузки цены для МТС";
                    }
                    try
                    {
                        base_item.Beeline_price = Convert.ToInt16(CheckForNull(worksheet.Cells[doc_iterator, 32].Value));
                        if (base_item.Beeline_price < 1 || base_item.Beeline_price > 10)
                        {
                            base_item.ValidationStatus.Validated = false;
                            base_item.ValidationStatus.Error_text = "Ошибка загрузки цены для Билайн";
                        }
                    }
                    catch
                    {
                        base_item.ValidationStatus.Validated = false;
                        base_item.ValidationStatus.Error_text = "Ошибка загрузки цены для Билайн";
                    }
                    try
                    {
                        base_item.Megafon_price = Convert.ToInt16(CheckForNull(worksheet.Cells[doc_iterator, 33].Value));
                        if (base_item.Megafon_price < 1 || base_item.Beeline_price > 10)
                        {
                            base_item.ValidationStatus.Validated = false;
                            base_item.ValidationStatus.Error_text = "Ошибка загрузки цены для Мегафон";
                        }
                    }
                    catch
                    {
                        base_item.ValidationStatus.Validated = false;
                        base_item.ValidationStatus.Error_text = "Ошибка загрузки цены для Мегафон";
                    }
                    if (base_item.Dealer_name.Trim() == "")
                    {
                        break;
                    }
                    ValidateBaseRecord(base_item);
                    base_recs.Add(base_item);
                }
            }
            else
                MessageBox.Show("Шапка файла не прошла проверку. Данные загружены не будут.","Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
            workbook.Close();
            workbook = null;
            //TODO Excel не закрывается
            excel.Quit();
            if (ValidateBaseData(base_recs))
                return base_recs;
            else
                return null;
        }

        private string TryConvertCommRate(string value)
        {
            if (value.Trim() == "")
                value = "0";
            try
            {
                if (value.Contains("%"))
                {
                    Regex reg = new Regex("[^0-9,]*", RegexOptions.None);
                    value = reg.Replace(value, "");
                    value = (Convert.ToDecimal(value) / 100).ToString();
                }
            }
            catch { }
            return value;
        }

        private string CheckForNull(object value)
        {
            if (value != null)
                return value.ToString();
            else
                return "";
        }

        private bool ValidateBaseFile(string doc_header, string doc_dw_header,
                                      string doc_rn_header,
                                            string doc_tpmts_header,
                                            string doc_tpbeemega_header,
                                            string doc_subway_header,
                                            string doc_contactperson_header,
                                            string doc_beelineprice_header,
                                            string doc_megaprice_header)
        {
            bool result = false;
            if (doc_header.Trim().StartsWith("Код точки (без )")
                && doc_dw_header.Trim().StartsWith("D / W")
                && doc_rn_header.Trim().StartsWith("R / N")
                && doc_tpmts_header.Trim().StartsWith("ТП МТС")
                && doc_tpbeemega_header.Trim().StartsWith("ТП БИ+МЕГА")
                && doc_subway_header.Trim().StartsWith("Метро")
                && doc_contactperson_header.Trim().StartsWith("Контактное лицо")
                && doc_beelineprice_header.Trim().StartsWith("Цена Билайн")
                && doc_megaprice_header.Trim().StartsWith("Цена Мегафон"))
                result = true;
            return result;
        }

        public static int GetLengthLimit(object obj, string field)
        {
            int dblenint = 0;   // default value = we can't determine the length

            Type type = obj.GetType();
            PropertyInfo prop = type.GetProperty(field);
            // Find the Linq 'Column' attribute
            // e.g. [Column(Storage="_FileName", DbType="NChar(256) NOT NULL", CanBeNull=false)]
            object[] info = prop.GetCustomAttributes(typeof(ColumnAttribute), true);
            // Assume there is just one
            if (info.Length == 1)
            {
                ColumnAttribute ca = (ColumnAttribute)info[0];
                string dbtype = ca.DbType;

                if (dbtype.StartsWith("NChar") || dbtype.StartsWith("NVarChar"))
                {
                    int index1 = dbtype.IndexOf("(");
                    int index2 = dbtype.IndexOf(")");
                    string dblen = dbtype.Substring(index1 + 1, index2 - index1 - 1);
                    int.TryParse(dblen, out dblenint);
                }
            }
            return dblenint;
        }

        private void ValidateBaseRecord(Code.BaseRecord base_item)
        {
            DB.Dealers db_item = CodeBaseRecord2DbDealer(base_item);
            if (db_item != null)
            {
                foreach (PropertyInfo pinfo in db_item.GetType().GetProperties())
                {
                    if (pinfo.PropertyType == typeof(String))
                    {
                        int length = GetLengthLimit(db_item, pinfo.Name);
                        int fact_length = (pinfo.GetValue(db_item, null) as String).Length;
                        if (fact_length > length)
                        {
                            base_item.ValidationStatus.Validated = false;
                            base_item.ValidationStatus.Error_text = String.Format("{0}" + Environment.NewLine + "Слишком длинное значение({3} символов, а должно быть {2}): {1}", base_item.ValidationStatus.Error_text, pinfo.GetValue(db_item, null) as String, length, fact_length);
                        }
                    }
                }
            }
        }

        private bool ValidateBaseData(ObservableCollection<Code.BaseRecord> base_data)
        {
            bool result = true;
            //Найти дубликаты Способ 1
            //var count = base_data.GroupBy(p => p.SP_code_old).SelectMany(g => g.Skip(1));
            //Найти дубликаты Способ 2
            if (base_data== null || base_data.Count() == 0)
            {
                MessageBox.Show("Нет загруженных записей", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            int double_count = base_data.GroupBy(p => p.SP_code_old).Where(w => w.Count() > 1).Select(s => s.Key).Count();
            if (double_count > 0)
            {
                MessageBox.Show("Записи по коду точки дублируются.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                result = false;
            }
            int notvalidated_count = base_data.Where(w => w.ValidationStatus.Validated == false).Count();
            if (notvalidated_count > 0)
            {
                if (MessageBox.Show("Ошибка загрузки записей из базы." + Environment.NewLine + "Вывести ошибки?", "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    string all_errors = "";
                    foreach (BaseRecord item in base_data.Where(w => w.ValidationStatus.Validated == false))
                    {
                        all_errors = all_errors + Environment.NewLine + item.ValidationStatus.Error_text;
                    }
                    all_errors = all_errors.Substring(0, Math.Min(all_errors.Length, 700)) + "...";
                    MessageBox.Show(all_errors, "Список ошибочных данных", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                result = false;
            }
            //Проверить длину полей


            return result;
        }

        public bool UpdateBaseData(ObservableCollection<Code.BaseRecord> new_base_data)
        {

            //04.09.2013 Николай попросил полногстью заменять точки новой базой
            //ObservableCollection<Code.BaseRecord> old_base_data = GetBaseDBData(USurname, UName);

            //IEnumerable<DB.Dealers> to_delete = from loaded_data in db.Dealers.ToList()
            //                                    from new_data in new_base_data
            //                                    where loaded_data.Sale_point_code_old == new_data.SP_code_old
            //                                    select loaded_data;
            IEnumerable<DB.Dealers> to_delete = db.Dealers;

            db.Dealers.DeleteAllOnSubmit(to_delete);
            try
            {
                db.SubmitChanges();
                IEnumerable<DB.Dealers> to_add = from a in new_base_data
                                                 select CodeBaseRecord2DbDealer(a);
                db.Dealers.InsertAllOnSubmit(to_add);
                db.SubmitChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ObservableCollection<Code.BaseRecord> GetBaseDBData(string surname, string name)
        {
            try
            {
                db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, db.Dealers);
            }
            catch { }
            return new ObservableCollection<Code.BaseRecord>(from b in db.Dealers
                                                             where b.TorgPred1.Replace(" ", "").ToLower() == String.Format("{0}{1}", surname, name).Replace(" ", "").ToLower()
                                                             || b.TorgPred2.Replace(" ", "").ToLower() == String.Format("{0}{1}", surname, name).Replace(" ", "").ToLower()
                                                             select DbDealer2CodeBaseRecord(b));
        }

        public List<string> GetSPStatuses()
        {
            List<string> result;
            try 
            {
                result = db.Sp_statuses.Select(s => s.Status_name).ToList<string>();

            }
            catch 
            {
                result = new List<string>();
            }
            return result;

        }

        public bool CreateUpdateSalePoint(Code.BaseRecord newSalePoint)
        {
            DB.Dealers dbNewSalePoint = db.Dealers.Where(w => w.Sale_point_code_old.Trim().ToLower() == newSalePoint.SP_code_old.Trim().ToLower()).FirstOrDefault();
            if (dbNewSalePoint == null)
            {
                dbNewSalePoint = CodeBaseRecord2DbDealer(newSalePoint);
                db.Dealers.InsertOnSubmit(dbNewSalePoint);
            }
            else
            {
                if (dbNewSalePoint.Record_status == (int)BaseRecStatus.New)
                { 
                    dbNewSalePoint.Add_comment = newSalePoint.Comment;
                    dbNewSalePoint.Area = newSalePoint.Area;
                    dbNewSalePoint.Beeline_price_id = newSalePoint.Beeline_price;
                    dbNewSalePoint.City = newSalePoint.City;
                    dbNewSalePoint.City_type = newSalePoint.City_type;
                    dbNewSalePoint.Contact_person = newSalePoint.Contact_person;
                    dbNewSalePoint.Contact_phone = newSalePoint.Contact_phone;
                    dbNewSalePoint.Dealer_name = newSalePoint.Dealer_name;
                    dbNewSalePoint.DW = newSalePoint.DW;
                    dbNewSalePoint.House = newSalePoint.House;
                    dbNewSalePoint.House_build = newSalePoint.House_build;
                    dbNewSalePoint.Megafon_price_id = newSalePoint.Megafon_price;
                    dbNewSalePoint.Mts_price_id = newSalePoint.Mts_price;
                    dbNewSalePoint.Record_status = (int)newSalePoint.Base_RecStatus;
                    dbNewSalePoint.RN = newSalePoint.RN;
                    dbNewSalePoint.Sale_point_code_new = newSalePoint.SP_code_new;
                    dbNewSalePoint.Sale_point_code_next = newSalePoint.SP_code_next;
                    dbNewSalePoint.Sale_point_code_old = newSalePoint.SP_code_old;
                    dbNewSalePoint.Sale_point_status = newSalePoint.SP_status;
                    dbNewSalePoint.Street = newSalePoint.Street;
                    dbNewSalePoint.Street_type = newSalePoint.Street_type;
                    dbNewSalePoint.Subway_station_name = newSalePoint.Subway_station;
                    dbNewSalePoint.TorgPred1 = newSalePoint.Torgpred1;
                    dbNewSalePoint.TorgPred2 = newSalePoint.Torgpred2;
                    dbNewSalePoint.Visit_number = newSalePoint.Visit_number;
                    dbNewSalePoint.Zone_id = newSalePoint.Zone;
                }
                else
                {
                    dbNewSalePoint.Add_comment = newSalePoint.Comment;
                    dbNewSalePoint.Contact_person = newSalePoint.Contact_person;
                    dbNewSalePoint.Contact_phone = newSalePoint.Contact_phone;
                    dbNewSalePoint.Record_status = (int)BaseRecStatus.Updated;
                    dbNewSalePoint.Sale_point_status = newSalePoint.SP_status;
                }
            }
            try { db.SubmitChanges(); return true; }
            catch { return false; }
        }
    }
}
