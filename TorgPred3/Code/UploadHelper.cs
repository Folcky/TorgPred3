using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;

namespace TorgPred3.Code
{
    public class UploadHelper : StarterHelper
    {
        public void FillCurrentUploads(ObservableCollection<Code.Upload> uploads, string SP_code_old, string Report_date)
        {
            IEnumerable<DB.Uploads> uploaded = db.Uploads.Where(p => p.Sale_point_code_old == SP_code_old && p.Upload_date.Date == DateTime.ParseExact(Report_date, date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Date);
            foreach (DB.Uploads item in uploaded)
            {
                uploads.Add(new Code.Upload()
                {
                    Document_num = item.Document_num,
                    ICC_id = item.Barcode,
                    Price = item.Price,
                    SP_code_new = item.Sale_point_code_new,
                    SP_code_old = item.Sale_point_code_old,
                    Upload_date = item.Upload_date,
                    TP_name = item.Tp_name,
                    owner_collection = uploads
                });
            }
        }

        public bool ValidateUploadRecord(ref Code.Upload item)
        {
            string icc_id = item.ICC_id;

            Code.Operation last_operation = ((from s in db.Stock
                                     where s.Barcode == icc_id
                                              select new Code.Operation { operation_id = 1, Operation_date = s.Document_date }).Concat(
                                    from u in db.Uploads
                                    where u.Barcode == icc_id
                                    select new Code.Operation { operation_id = 2, Operation_date = u.Upload_date }).Concat(
                                    from r in db.Refusers
                                    where r.Barcode == icc_id
                                    select new Code.Operation { operation_id = 3, Operation_date = r.Refuse_date }
                                    )).OrderByDescending(o=>o.Operation_date).FirstOrDefault();

            //Если даже накладной не было, ошибка
            if (last_operation == null)
            {
                item.ValidationStatus = new ValidationStatus() { Validated=false, Error_text = "Накладная не найдена" };
                item.Document_num = "";
                item.TP_name = "";
                return false;
            }

            //Если был возврат, ошибка
            if (last_operation.operation_id == 3)
            {
                item.ValidationStatus = new ValidationStatus() { Validated = false, Error_text = "Последняя операция возврат." };
                return false;
            }
            //Если была другая отгрузка, ошибка
            if (last_operation.operation_id == 2)
            {
                DB.Uploads uploadrecord = db.Uploads.Where(u => u.Barcode == icc_id).OrderByDescending(o => o.Upload_date).FirstOrDefault();
                if (uploadrecord != null &&
                    (uploadrecord.Upload_date.Date != item.Upload_date.Date ||
                    uploadrecord.Document_num != item.Document_num ||
                    uploadrecord.Sale_point_code_old != item.SP_code_old))
                {
                    item.ValidationStatus = new ValidationStatus() { Validated = false, Error_text = String.Format("Отгрузка уже была. {0}({1}) от {2}", uploadrecord.Sale_point_code_new, uploadrecord.Sale_point_code_old, uploadrecord.Upload_date.ToString("dd.MM.yyyy")) };
                    return false;
                }    
            }
            
            //Если была все таки накладная, обновляем
            if (last_operation.operation_id == 1)
            {
                DB.Stock stockrecord = db.Stock.Where(p => p.Barcode == icc_id).OrderByDescending(o => o.Document_date).FirstOrDefault();
                //Если накладная была обновляем ТП и Номинал
                if (stockrecord != null)
                {
                    //Проверяем наличие по номенклатуре
                    DB.PriceList pricelistrecord = db.PriceList.Where(p => p.Tp_name == stockrecord.Tp_name).FirstOrDefault();
                    if (pricelistrecord == null)
                    {
                        item.ValidationStatus = new ValidationStatus() { Validated = false, Error_text = String.Format("Не найдено в номенклатуре") };
                        return false;
                    }
                    else
                    {
                        int price_index = GetPriceIndex(item.SP_code_old, pricelistrecord.Suplier_name);
                        decimal price = Convert.ToDecimal(pricelistrecord.GetType().GetProperty(String.Format("Price_n{0}", price_index)).GetValue(pricelistrecord, null));
                        item.Document_num = stockrecord.Document_num;
                        item.TP_name = stockrecord.Tp_name;
                        if (item.Price != price)
                        {
                            item.Price = price;
                        }
                    }
                }
            }
            if (icc_id.Length > 20)
            {
                item.ValidationStatus = new ValidationStatus() { Validated = false, Error_text = "Максимальная длина номера 20 символов" };
                return false;
            }
            //Если до сюда добрались, значит все нормально
            item.ValidationStatus = new ValidationStatus() { Validated = true, Error_text = "" };
            return true;
        }

        public int GetPriceIndex(string sp_code_old, string suplier_name)
        {
            int result = 1;
            DB.Dealers sp = db.Dealers.Where(w => w.Sale_point_code_old == sp_code_old).FirstOrDefault();
            if (sp != null)
            {
                switch (suplier_name.Trim().ToUpper())
                {
                    case "МТС":
                        result = sp.Mts_price_id;
                        break;
                    case "БИЛАЙН":
                        result = sp.Beeline_price_id;
                        break;
                    case "МЕГАФОН":
                        result = sp.Megafon_price_id;
                        break;
                }
            }
            return result;
        }

        public void CreateOrUpdateUploadRecord(Code.Upload item)
        {
            DB.Uploads upload2update = db.Uploads.Where(p => p.Barcode == item.ICC_id && p.Document_num == item.Document_num && p.Upload_date == item.Upload_date && p.Sale_point_code_old == item.SP_code_old).FirstOrDefault();
            if (upload2update == null)
            {
                upload2update = new DB.Uploads()
                {
                    Barcode = item.ICC_id,
                    Document_num = item.Document_num,
                    Sale_point_code_new = item.SP_code_new,
                    Sale_point_code_old = item.SP_code_old,
                    Upload_date = item.Upload_date,
                    Tp_name = item.TP_name,
                    Price = item.Price,
                    Operation_date = DateTime.Now
                };
                db.Uploads.InsertOnSubmit(upload2update);
            }
            else
            {
                upload2update.Barcode = item.ICC_id;
                upload2update.Upload_date = item.Upload_date;
                upload2update.Price = item.Price;
                upload2update.Tp_name = item.TP_name;
                upload2update.Document_num = item.Document_num;
                upload2update.Operation_date = DateTime.Now;
            }
            try { db.SubmitChanges(); }
            catch { }
        }
        public void DeleteUploadRecord(Code.Upload old_value)
        {
            DB.Uploads old_upload = db.Uploads.Where(p => p.Barcode == old_value.ICC_id &&
                p.Document_num == old_value.Document_num &&
                p.Upload_date == old_value.Upload_date &&
                p.Sale_point_code_old == old_value.SP_code_old).FirstOrDefault();
            if (old_upload != null)
            {
                db.Uploads.DeleteOnSubmit(old_upload);
                try { db.SubmitChanges(); }
                catch { }
            }
        }
    }
}
