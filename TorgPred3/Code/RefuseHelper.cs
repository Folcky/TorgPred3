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
    public class RefuseHelper : StarterHelper
    {
        public void FilldgRefusers(ObservableCollection<Code.Refuse> refusers, IEnumerable<Code.BaseRecord> SP_list, string Report_date)
        {
            IEnumerable<DB.Refusers> refused = db.Refusers.Where(p => p.Sale_point_code_old == SP_list.ElementAt(0).SP_code_old && p.Refuse_date.Date == DateTime.ParseExact(Report_date, date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Date);
            foreach (DB.Refusers item in refused)
            {
                refusers.Add(new Code.Refuse()
                {
                    ICC_id = item.Barcode,
                    SP_code_new = item.Sale_point_code_new,
                    SP_code_old = item.Sale_point_code_old,
                    Refuse_date = item.Refuse_date
                });
            }
        }

        public bool ValidateRefuseRecord(ref Code.Refuse item)
        {
            string icc_id = item.ICC_id;
            DB.Refusers refuserecord = db.Refusers.Where(u => u.Barcode == icc_id).OrderByDescending(o => o.Refuse_date).FirstOrDefault();
            if (refuserecord != null &&
                    (refuserecord.Refuse_date.Date != item.Refuse_date.Date ||
                    refuserecord.Sale_point_code_old != item.SP_code_old))
            {
                item.ValidationStatus = new ValidationStatus() { Validated = false, Error_text = "Возврат уже был" };
                return false;
            }
            if (icc_id.Length>20)
            {
                item.ValidationStatus = new ValidationStatus() { Validated = false, Error_text = "Максимальная длина номера 20 символов" };
                return false;
            }

            item.ValidationStatus = new ValidationStatus() { Validated = true, Error_text = "" };
            return true;
        }

        public void CreateOrUpdateRefuseRecord(Code.Refuse item)
        {
            DB.Refusers refuse2update = db.Refusers.Where(p => p.Barcode == item.ICC_id &&
                p.Refuse_date == item.Refuse_date &&
                p.Sale_point_code_old == item.SP_code_old).FirstOrDefault();
            if (refuse2update == null)
            {
                refuse2update = new DB.Refusers()
                {
                    Barcode = item.ICC_id,
                    Sale_point_code_new = item.SP_code_new,
                    Sale_point_code_old = item.SP_code_old,
                    Refuse_date = item.Refuse_date,
                    Operation_date = DateTime.Now
                };
                db.Refusers.InsertOnSubmit(refuse2update);
            }
            else
            {
                refuse2update.Barcode = item.ICC_id;
                refuse2update.Refuse_date = item.Refuse_date;
                refuse2update.Operation_date = DateTime.Now;
            }
            try { db.SubmitChanges(); }
            catch { }
        }

        public void DeleteRefuseRecord(Code.Refuse old_value)
        {
            DB.Refusers old_upload = db.Refusers.Where(p => p.Barcode == old_value.ICC_id &&
                p.Refuse_date == old_value.Refuse_date &&
                p.Sale_point_code_old == old_value.SP_code_old).FirstOrDefault();
            if (old_upload != null)
            {
                db.Refusers.DeleteOnSubmit(old_upload);
                try { db.SubmitChanges(); }
                catch { }
            }
        }
    }
}
