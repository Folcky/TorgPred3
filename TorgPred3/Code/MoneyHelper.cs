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
    public class MoneyHelper : StarterHelper
    {
        public void FilldgMoney(ObservableCollection<Code.MoneySum> monies, IEnumerable<Code.BaseRecord> SP_list, string Report_date)
        {
            IEnumerable<DB.Money> moneied = db.Money.Where(p => p.Sale_point_code_old == SP_list.ElementAt(0).SP_code_old).OrderByDescending(o=>o.Money_date);
            foreach (DB.Money item in moneied)
            {
                monies.Add(new Code.MoneySum()
                {
                    Money_action_id = item.Money_action_id,
                    Money_sum = item.Money_sum,
                    SP_code_new = item.Sale_point_code_new,
                    SP_code_old = item.Sale_point_code_old,
                    Money_date = item.Money_date,
                    owner_collection = monies
                });
            }
        }

        public bool ValidateMoneyRecord(ref Code.MoneySum item, string Report_date)
        {
            string sp =item.SP_code_old;
            DateTime dt = item.Money_date;
            int acid = item.Money_action_id;
            if (Report_date != item.Money_date.ToString("dd.MM.yyyy"))
            {
                item.ValidationStatus = new ValidationStatus() { Validated = false, Error_text = "Нельзя редактировать прошлые даты" };
                return false;
            }
            IEnumerable<Code.MoneySum> list = item.owner_collection.Where(w => w.SP_code_old == sp && w.Money_date == dt && w.Money_action_id == acid);
            if (list.Count() > 1)
            {
                item.ValidationStatus.Validated = false;
                item.ValidationStatus.Error_text = "Дублирование записи";
                return false;
            }
            if (item.Money_action_id != 1)
            {
                item.ValidationStatus = new ValidationStatus() { Validated = false, Error_text = "У Вас недостаточно прав на редактирование" };
                return false;
            }
            item.ValidationStatus = new ValidationStatus() { Validated = true, Error_text = "" };
            return true;
        }

        public void CreateOrUpdateMoneyRecord(Code.MoneySum item)
        {
            DB.Money money2update = db.Money.Where(p => p.Sale_point_code_old == item.SP_code_old &&
                p.Money_date == item.Money_date &&
                p.Money_action_id == item.Money_action_id).FirstOrDefault();
            if (money2update == null)
            {
                money2update = CodeMoney2DbMoney(item, money2update);
                db.Money.InsertOnSubmit(money2update);
            }
            else
                money2update = CodeMoney2DbMoney(item, money2update);
            try { db.SubmitChanges(); }
            catch { }
        }

        public void DeleteMoneyRecord(Code.MoneySum old_value)
        {
            DB.Money old_money = db.Money.Where(p => 
                p.Money_action_id == old_value.Money_action_id &&
                p.Money_date == old_value.Money_date &&
                p.Sale_point_code_old == old_value.SP_code_old).FirstOrDefault();
            if (old_money != null)
            {
                db.Money.DeleteOnSubmit(old_money);
                try { db.SubmitChanges(); }
                catch { }
            }
        }
    }
}
