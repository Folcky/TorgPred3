using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace TorgPred3.Code
{
    public class Visit
    {
        public DateTime Visit_date { get; set; }

        private int _uploads_flag = 0;
        public int Uploads_flag { get { return _uploads_flag; } set { _uploads_flag = value; } }

        private int _refusers_flag = 0;
        public int Refusers_flag { get { return _refusers_flag; } set { _refusers_flag = value; } }

        private decimal _price_sum = 0;
        public decimal Price_sum { get { return _price_sum; } set { _price_sum = value; } }

        private decimal _money_sum = 0;
        public decimal Money_sum { get { return _money_sum; } set { _money_sum = value; } }
    }

    public class Document
    {
        public string Document_num{get; set;}
        public DateTime Doc_date { get; set; }
        public DateTime Operation_date { get; set; }
    }

    public class ValidationStatus
    {
        private bool _validated=true;
        private string _error_text = "";
        public bool Validated { get { return _validated; } set { _validated = value; } }
        public string Error_text { get { return _error_text; } set { _error_text = value; } }
    }
    //Используется для проверки истории операций по симке
    public class Operation
    {
        public int operation_id { get; set; }
        public DateTime Operation_date { get; set; }
        public DB.Stock stock { get; set; }
        public DB.Uploads upload { get; set; }
        public DB.Refusers refuse { get; set; }
    }

    public class MoneySum : INotifyPropertyChanged
    {
        private ValidationStatus _validationstatus = new ValidationStatus();
        public ValidationStatus ValidationStatus { get { return _validationstatus; } set { _validationstatus = value; } }
        public ObservableCollection<MoneySum> owner_collection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime Money_date { get; set; }

        private decimal _money_sum = 0;
        public decimal Money_sum { get { return _money_sum; } set { _money_sum = value; NotifyPropertyChanged("Money_sum"); } }
        private int _money_action_id = 1;
        public int Money_action_id 
        { 
            get{return _money_action_id; } 
            set 
            { 
                _money_action_id = value;
                switch (_money_action_id)
                {
                    case 1:
                        _money_action_name = "Прием";
                        NotifyPropertyChanged("Money_action_name");
                        break;
                    case 2:
                        _money_action_name = "Списание";
                        NotifyPropertyChanged("Money_action_name");
                        break;
                    default:
                        _money_action_name = "Ошибка";
                        NotifyPropertyChanged("Money_action_name");
                        break;
                }
            } 
        }
        private string _money_action_name = "";
        public string Money_action_name { get { return _money_action_name; } }
        public string SP_code_new { get; set; }
        public string SP_code_old { get; set; }

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }

    public class Refuse : INotifyPropertyChanged
    {
        private ValidationStatus _validationstatus = new ValidationStatus();
        public ValidationStatus ValidationStatus { get { return _validationstatus; } set { _validationstatus = value; } }
        public ObservableCollection<Refuse> owner_collection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _icc_id = "";

        public DateTime Refuse_date { get; set; }
        public string ICC_id { get { return _icc_id; } set { _icc_id = value; NotifyPropertyChanged("ICC_id"); } }
        public string SP_code_new { get; set; }
        public string SP_code_old { get; set; }

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }

    public class StockStatus
    {
        private string _suplier_name = "";
        private string _tp_name = "";
        private decimal _nominal = 0;
        private int _count = 0;
        public string Suplier_name { get { return _suplier_name; } set { _suplier_name = value; } }
        public string TP_name { get { return _tp_name; } set { _tp_name = value; } }
        public decimal Nominal { get { return _nominal; } set { _nominal = value; } }
        public int Count { get { return _count; } set { _count = value; } }
    }

    public class Upload : INotifyPropertyChanged
    {
        private ValidationStatus _validationstatus = new ValidationStatus();
        public ValidationStatus ValidationStatus { get { return _validationstatus; } set { _validationstatus = value; } }
        public ObservableCollection<Upload> owner_collection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _icc_id="";
        private decimal _price;
        private string _doc_num = "";
        private string _tp_name="";

        public string Document_num { get { return _doc_num; } set { _doc_num = value; NotifyPropertyChanged("Document_num"); } }
        public DateTime Upload_date { get; set; }
        public string ICC_id { get { return _icc_id; } set { _icc_id = value; NotifyPropertyChanged("ICC_id"); } }
        public decimal Price { get { return _price; } set { _price = value; NotifyPropertyChanged("Price"); } }
        public string SP_code_new { get; set; }
        public string SP_code_old { get; set; }
        public string TP_name { get { return _tp_name; } set { _tp_name = value; NotifyPropertyChanged("TP_name"); } }

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }

    public class DocumentItem
    {
        public string ICC_id { get; set; }
        public DateTime Upload_date { get; set; }
    }

    public class PriceListRecord
    {
        private string _tp_name = "";
        public string TP_name { get { return _tp_name; } set { _tp_name = value ?? String.Empty; } }

        private decimal _price_n1 = 0;
        public decimal Price_n1 { get { return _price_n1; } set { _price_n1 = value; } }
        private decimal _price_n2 = 0;
        public decimal Price_n2 { get { return _price_n2; } set { _price_n2 = value; } }
        private decimal _price_n3 = 0;
        public decimal Price_n3 { get { return _price_n3; } set { _price_n3 = value; } }
        private decimal _price_n4 = 0;
        public decimal Price_n4 { get { return _price_n4; } set { _price_n4 = value; } }
        private decimal _price_n5 = 0;
        public decimal Price_n5 { get { return _price_n5; } set { _price_n5 = value; } }
        private decimal _price_n6 = 0;
        public decimal Price_n6 { get { return _price_n6; } set { _price_n6 = value; } }
        private decimal _price_n7 = 0;
        public decimal Price_n7 { get { return _price_n7; } set { _price_n7 = value; } }
        private decimal _price_n8 = 0;
        public decimal Price_n8 { get { return _price_n8; } set { _price_n8 = value; } }
        private decimal _price_n9 = 0;
        public decimal Price_n9 { get { return _price_n9; } set { _price_n9 = value; } }
        private decimal _price_n10 = 0;
        public decimal Price_n10 { get { return _price_n10; } set { _price_n10 = value; } }

        private string _suplier = "";
        public string Suplier_name { get { return _suplier; } set { _suplier = value; } }

        private string _product_category_name = "";
        public string Product_category_name { get { return _product_category_name; } set { _product_category_name = value; } }
    }

    public enum BaseRecStatus
    {
        NotChanged = 0, Updated = 1, New = 2
    }

    public class BaseRecord
    {
        private ValidationStatus _validationstatus = new ValidationStatus();
        public ValidationStatus ValidationStatus { get { return _validationstatus; } set { _validationstatus = value; } }

        private BaseRecStatus _baseRecStatus = BaseRecStatus.NotChanged;
        public BaseRecStatus Base_RecStatus { get { return _baseRecStatus; } set { _baseRecStatus = value; } }

        private string _dealer_name = "";
        public string Dealer_name { get { return _dealer_name; } set { _dealer_name = value ?? String.Empty; } }

        private string _dw = "";
        public string DW { get { return _dw; } set { _dw = value != null ? value : ""; } }

        private string _rn = "";
        public string RN { get { return _rn; } set { _rn = value != null ? value : ""; } }

        private string _sp_code_new = "";
        public string SP_code_new { get { return _sp_code_new; } set { _sp_code_new = value != null ? value : ""; } }

        private string _sp_code_next = "";
        public string SP_code_next { get { return _sp_code_next; } set { _sp_code_next = value != null ? value : ""; } }

        private string _sp_code_old = "";
        public string SP_code_old { get { return _sp_code_old; } set { _sp_code_old = value != null ? value : ""; } }

        private string _sp_status = "";
        public string SP_status { get { return _sp_status; } set { _sp_status = value != null ? value : ""; } }

        private string _area = "";
        public string Area { get { return _area; } set { _area = value != null ? value : ""; } }

        private string _city = "";
        public string City { get { return _city; } set { _city = value != null ? value : ""; } }

        private string _city_type = "";
        public string City_type { get { return _city_type; } set { _city_type = value != null ? value : ""; } }

        private string _subway_station = "";
        public string Subway_station { get { return _subway_station; } set { _subway_station = value != null ? value : ""; } }

        private string _street = "";
        public string Street { get { return _street; } set { _street = value != null ? value : ""; } }
        
        private string _street_type = "";
        public string Street_type { get { return _street_type; } set { _street_type = value != null ? value : ""; } }

        private string _house = "";
        public string House { get { return _house; } set { _house = value != null ? value : ""; } }
        
        private string _house_build = "";
        public string House_build { get { return _house_build; } set { _house_build = value != null ? value : ""; } }

        private string _torgpred1 = "";
        public string Torgpred1 { get { return _torgpred1; } set { _torgpred1 = value != null ? value : ""; } }
        private string _torgpred2 = "";
        public string Torgpred2 { get { return _torgpred2; } set { _torgpred2 = value != null ? value : ""; } }

        private string _contact_person = "";
        public string Contact_person { get { return _contact_person; } set { _contact_person = value != null ? value : ""; } }

        private string _contact_phone = "";
        public string Contact_phone { get { return _contact_phone; } set { _contact_phone = value != null ? value : ""; } }

        private string _comment = "";
        public string Comment { get { return _comment; } set { _comment = value != null ? value : ""; } }

        private int _visit_number = 0;
        public int Visit_number { get { return _visit_number; } set { _visit_number = value; } }
        
        private int _zone = 0;
        public int Zone { get { return _zone; } set { _zone = value; } }

        private int _mts_price = 1;
        public int Mts_price { get { return _mts_price; } set { _mts_price = value; } }
        private int _beeline_price = 1;
        public int Beeline_price { get { return _beeline_price; } set { _beeline_price = value; } }
        private int _megafon_price = 1;
        public int Megafon_price { get { return _megafon_price; } set { _megafon_price = value; } }

        private string _adr_awr = "";
        public string ADR_awr { get { return _adr_awr; } set { _adr_awr = value != null ? value : ""; } }
        private string _suplier_sp_type = "";
        public string Suplier_SP_type { get { return _suplier_sp_type; } set { _suplier_sp_type = value != null ? value : ""; } }
        private string _sp_profile_type = "";
        public string SP_profile_type { get { return _sp_profile_type; } set { _sp_profile_type = value != null ? value : ""; } }
        private string _sp_desc = "";
        public string SP_desc { get { return _sp_desc; } set { _sp_desc = value != null ? value : ""; } }
        private decimal _comm_rate = 0;
        public decimal Comm_rate { get { return _comm_rate; } set { _comm_rate = value; } }
        private string _infocart_reg = "";
        public string Infocart_reg { get { return _infocart_reg; } set { _infocart_reg = value != null ? value : ""; } }
    }

    public class StockInRecord
    {
        private string[] date_formats = { "dd.MM.yyyy HH:mm", "dd.MM.yyyy H:mm", "dd.MM.yyyy H:mm:ss", "dd.MM.yyyy", "dd.MM.yy" };
        private string _icc_id = "";
        private string _msisdn = "";
        private string _comment = "";
        private string _tp_name = "";
        private decimal _nominal = 0;
        private string _doc_num = "";
        private DateTime _doc_date = DateTime.Now;

        public string Icc_id { get { return _icc_id; } set { _icc_id = value != null ? value : ""; } }
        public string Msisdn { get { return _msisdn; } set { _msisdn = value != null ? value : ""; } }
        public string Comment { 
            get { return _comment; } 
            set {
                if (value != null)
                {
                    _comment = value;
                    //Regex regex = new Regex(@"^(?<tp_name>.*)\((?<nominal>\d*)\).*$"); //regex that matches disallowed text
                    //if (regex.IsMatch(_comment))
                    //{
                    //    try
                    //    {
                    //        _tp_name = regex.Split(_comment)[regex.GroupNumberFromName("tp_name")].Trim();
                    //        _nominal = Convert.ToDecimal(regex.Split(_comment)[regex.GroupNumberFromName("nominal")].Trim());
                    //    }
                    //    catch { }
                    //}
                }
            } }
        public string TP_name { get { return _tp_name; } set { _tp_name = value; } }
        public decimal Nominal { get { return _nominal; } set { _nominal = value; } }
        public DateTime Doc_date { get { return _doc_date; } }
        public string Doc_num { get { return _doc_num; }
            set
            {
                if (value != null)
                {
                    Regex regex = new Regex(@"^(?<doc_name>.*)\s№\s(?<doc_num>\d*)\D*(?<doc_date>\d{2}\.\d{2}\.(\d{2}$|\d{4}$))$");
                    if (regex.IsMatch(value))
                    {
                        try
                        {
                            _doc_num = regex.Split(value)[regex.GroupNumberFromName("doc_num")].Trim();
                            _doc_date = DateTime.ParseExact(regex.Split(value)[regex.GroupNumberFromName("doc_date")].Trim(), date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                        }
                        catch { }
                    }
                    else
                    _doc_num = value;
                }
            }
        }
    }
}
