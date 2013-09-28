using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;

namespace TorgPred3.Code
{
    public class UploadValidationRule : ValidationRule
    {
        private ObservableCollection<Code.Upload> _source;
        public ObservableCollection<Code.Upload> Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            Code.Upload input_value = (value as BindingGroup).Items[0] as Code.Upload;
            IEnumerable<Code.Upload> list = input_value.owner_collection.Where(w=>w.ICC_id==input_value.ICC_id);
            if (list.Count() > 1)
            {
                input_value.ValidationStatus.Validated = false;
                input_value.ValidationStatus.Error_text = "Дублирование записи";
            }
            //Просто берем статус валидации и не даем дубликатить на форме
            if (input_value.ValidationStatus.Validated==false || list.Count() > 1)
            {
                return new ValidationResult(false, input_value.ValidationStatus.Error_text);
            }
            return result;
        }
    }

    public class RefuseValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            Code.Refuse input_value = (value as BindingGroup).Items[0] as Code.Refuse;
            IEnumerable<Code.Refuse> list = input_value.owner_collection.Where(w => w.ICC_id == input_value.ICC_id);
            if (list.Count() > 1)
            {
                input_value.ValidationStatus.Validated = false;
                input_value.ValidationStatus.Error_text = "Дублирование записи";
            }
            if (input_value.ValidationStatus.Validated == false || list.Count() > 1)
            {
                return new ValidationResult(false, input_value.ValidationStatus.Error_text);
            }
            return result;
        }
    }
    
    public class MoneyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            Code.MoneySum input_value = (value as BindingGroup).Items[0] as Code.MoneySum;
            IEnumerable<Code.MoneySum> list = input_value.owner_collection.Where(w => w.Money_date == input_value.Money_date && w.Money_action_id==input_value.Money_action_id);
            if (list.Count() > 1)
            {
                input_value.ValidationStatus.Validated = false;
                input_value.ValidationStatus.Error_text = "Дублирование записи";
            }
            if (input_value.ValidationStatus.Validated == false || list.Count() > 1)
            {
                return new ValidationResult(false, input_value.ValidationStatus.Error_text);
            }
            return result;
        }
    }
    public class MoneyCribValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return new ValidationResult(true, null);
        }
    }
}
