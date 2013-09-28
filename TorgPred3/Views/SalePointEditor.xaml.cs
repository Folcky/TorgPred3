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

using TorgPred3.Code;
using System.Text.RegularExpressions;

namespace TorgPred3.Views
{
    /// <summary>
    /// Interaction logic for Uploads.xaml
    /// </summary>
    public partial class SalePointEditor : UserControl
    {
        public SalePointEditor(ObservableCollection<Code.BaseRecord> salepoints, string usurname, string uname)
        {
            InitializeComponent();
            this.SP_list = salepoints;
            helper.USurname = usurname;
            helper.UName = uname;
            USurname = usurname;
            UName = uname;
            //1.Инициализация списков
            dw_list.Add("D");
            dw_list.Add("W");
            dw_list = dw_list.Concat(salepoints.Select(s => s.DW.Trim())).Distinct().ToList<string>();
            cbDW.ItemsSource = dw_list;
            dw_list.Add("R");
            dw_list.Add("N");
            rn_list = rn_list.Concat(salepoints.Select(s => s.RN.Trim())).Distinct().ToList<string>();
            cbRN.ItemsSource = rn_list;
            cityType_list = salepoints.Select(s => s.City_type.Trim()).Distinct().ToList<string>();
            cbCityType.ItemsSource = cityType_list;
            subway_list = salepoints.Select(s => s.Subway_station.Trim()).Distinct().ToList<string>();
            cbSubway.ItemsSource = subway_list;
            streetType_list = salepoints.Select(s => s.Street_type.Trim()).Distinct().ToList<string>();
            cbStreetType.ItemsSource = streetType_list;
            SPStatuses_list = helper.GetSPStatuses();
            cbSPStatus.ItemsSource = SPStatuses_list;
            //2.Публикация
            GetIPSPSData(ipsps_position);
            EnableButtons(ipsps_position);
        }

        //Refusers Properties
        public ObservableCollection<Code.BaseRecord> SP_list
        { get; set; }
        private Code.BaseHelper helper = new Code.BaseHelper();
        private int ipsps_position = 0;
        private List<string> dw_list = new List<string>();
        private List<string> rn_list = new List<string>();
        private List<string> cityType_list;
        private List<string> subway_list;
        private List<string> streetType_list;
        private List<string> SPStatuses_list;
        public string USurname { get; set; }
        public string UName { get; set; }

        private void bNewTT_Click(object sender, RoutedEventArgs e)
        {
            if (((string)bNewTT.Content) == "Новая точка")
            {
                bPrevious.IsEnabled = false;
                bNext.IsEnabled = false;
                bFirst.IsEnabled = false;
                bLast.IsEnabled = false;
                //bSave.IsEnabled = true;
                ipsps_position = -1;
                GetIPSPSData(ipsps_position);
                bNewTT.Content = "Отменить";
                tbSP_code.Text = String.Format("{0}{1}{2}", USurname.Substring(0, 1), UName.Substring(0, 1), GetSPNewIndex());
                EnableEditing(true);
            }
            else
                if (((string)bNewTT.Content) == "Отменить")
                {
                    ipsps_position = SP_list.Count() - 1;
                    GetIPSPSData(ipsps_position);
                    bPrevious.IsEnabled = true;
                    bNext.IsEnabled = false;
                    EnableButtons(ipsps_position);
                    bNewTT.Content = "Новая точка";
                }
        }

        private int GetSPNewIndex()
        {
            ObservableCollection<string> init_code = new ObservableCollection<string>();
            init_code.Add(String.Format("{0}{1}{2}", USurname.Substring(0, 1), UName.Substring(0, 1), 100));
            try
            {
                var reg = new Regex("[^0-9,]*", RegexOptions.None);
                int index = Convert.ToInt16((from i in SP_list.Select(s => s.SP_code_old).Union(init_code)
                        where i.Trim().StartsWith(String.Format("{0}{1}", USurname.Substring(0, 1), UName.Substring(0, 1)))
                        select reg.Replace(i,"")).Max())+1;
                return index;
            }
            catch { return 101; };
        }

        private void GetIPSPSData(int ipsps_position)
        {
            try
            {
                if (ipsps_position >= 0)
                {
                    tbDealername.Text = SP_list.ElementAt(ipsps_position).Dealer_name;
                    tbSP_code.Text = SP_list.ElementAt(ipsps_position).SP_code_old;
                    cbDW.SelectedItem = SP_list.ElementAt(ipsps_position).DW.Trim();
                    cbRN.SelectedItem = SP_list.ElementAt(ipsps_position).RN.Trim();
                    cbSPStatus.SelectedItem = SPStatuses_list.Where(w => w.Trim().ToLower() == SP_list.ElementAt(ipsps_position).SP_status.Trim().ToLower()).FirstOrDefault();
                    

                    tbCity.Text = SP_list.ElementAt(ipsps_position).City;
                    cbCityType.SelectedItem = SP_list.ElementAt(ipsps_position).City_type;
                    tbArea.Text = SP_list.ElementAt(ipsps_position).Area;
                    cbSubway.SelectedItem = SP_list.ElementAt(ipsps_position).Subway_station;

                    tbStreet.Text = SP_list.ElementAt(ipsps_position).Street;
                    cbStreetType.SelectedItem = SP_list.ElementAt(ipsps_position).Street_type;
                    tbHouse.Text = SP_list.ElementAt(ipsps_position).House;
                    tbBuild.Text = SP_list.ElementAt(ipsps_position).House_build;

                    tbContactPerson.Text = SP_list.ElementAt(ipsps_position).Contact_person;
                    tbContactPhone.Text = SP_list.ElementAt(ipsps_position).Contact_phone;
                    tbVisit.Text = SP_list.ElementAt(ipsps_position).Visit_number.ToString();

                    tbComment.Text = SP_list.ElementAt(ipsps_position).Comment;

                    if (SP_list.ElementAt(ipsps_position).Base_RecStatus != BaseRecStatus.New)
                    {
                        EnableEditing(false);
                    }
                    else
                    {
                        EnableEditing(true);
                    }
                }
                if (ipsps_position == -1)
                {
                    tbDealername.Text = "";
                    //tbSP_code.Text = "";
                    cbDW.SelectedItem = null;
                    cbRN.SelectedItem = null;
                    cbSPStatus.SelectedItem = null;


                    tbCity.Text = "";
                    cbCityType.SelectedItem = null;
                    tbArea.Text = "";
                    cbSubway.SelectedItem = null;

                    tbStreet.Text = "";
                    cbStreetType.SelectedItem = null;
                    tbHouse.Text = "";
                    tbBuild.Text = "";

                    tbContactPerson.Text = "";
                    tbContactPhone.Text = "";
                    tbVisit.Text = "";

                    tbComment.Text = "";
                }
            }
            catch { }
        }

        private void EnableEditing(bool enable)
        {
            if (!enable)
            {
                //Редактировать нельзя
                tbDealername.IsReadOnly = true;
                tbSP_code.IsEnabled = true;
                cbDW.IsEnabled = false;
                cbRN.IsEnabled = false;
                cbSPStatus.IsEnabled = true;
                cbSPStatus.IsReadOnly = true;

                tbCity.IsReadOnly = true;
                cbCityType.IsEnabled = false;
                tbArea.IsReadOnly = true;
                cbSubway.IsEnabled = false;

                tbStreet.IsReadOnly = true;
                cbStreetType.IsEnabled = false;
                tbHouse.IsReadOnly = true;
                tbBuild.IsReadOnly = true;

                tbContactPerson.IsReadOnly = false;
                tbContactPhone.IsReadOnly = false;
                tbVisit.IsReadOnly = false;

                tbComment.IsReadOnly = false;
            }
            else
            {
                //Редактировать можно
                tbDealername.IsReadOnly = false;
                tbSP_code.IsReadOnly = true;
                cbDW.IsEnabled = true;
                cbRN.IsEnabled = true;
                cbSPStatus.IsEnabled = true;
                cbSPStatus.IsReadOnly = true;

                tbCity.IsReadOnly = false;
                cbCityType.IsEnabled = true;
                cbCityType.IsReadOnly = false;
                tbArea.IsReadOnly = false;
                cbSubway.IsEnabled = true;
                cbSubway.IsReadOnly = false;

                tbStreet.IsReadOnly = false;
                cbStreetType.IsEnabled = true;
                cbStreetType.IsReadOnly = false;
                tbHouse.IsReadOnly = false;
                tbBuild.IsReadOnly = false;

                tbContactPerson.IsReadOnly = false;
                tbContactPhone.IsReadOnly = false;
                tbVisit.IsReadOnly = false;

                tbComment.IsReadOnly = false;
            }
        }

        private void EnableButtons(int ipsps_position)
        {
            try
            {
                if (ipsps_position == SP_list.Count() - 1)
                {
                    bLast.IsEnabled = false;
                    bNext.IsEnabled = false;
                }
                else
                    if (ipsps_position >= 0 && ipsps_position < SP_list.Count() - 1)
                    {
                        bLast.IsEnabled = true;
                        bNext.IsEnabled = true;
                    }

                if (ipsps_position == 0)
                {
                    bFirst.IsEnabled = false;
                    bPrevious.IsEnabled = false;
                    if (SP_list.Count() == 0)
                    {
                        bLast.IsEnabled = false;
                        bNext.IsEnabled = false;
                    }
                }
                else
                    if (ipsps_position > 0 && ipsps_position <= SP_list.Count() - 1)
                    {
                        bFirst.IsEnabled = true;
                        bPrevious.IsEnabled = true;
                    }
                lSPCount.Content = String.Format("{0} из {1}", ipsps_position + 1, SP_list.Count());
            }
            catch { }
        }

        private void bFirst_Click(object sender, RoutedEventArgs e)
        {
            ipsps_position = 0;
            GetIPSPSData(ipsps_position);
            EnableButtons(ipsps_position);
        }

        private void bNext_Click(object sender, RoutedEventArgs e)
        {
            ipsps_position = ipsps_position + 1;
            if (ipsps_position < SP_list.Count())
                GetIPSPSData(ipsps_position);
            else
                ipsps_position = SP_list.Count() - 1;
            EnableButtons(ipsps_position);
        }

        private void bLast_Click(object sender, RoutedEventArgs e)
        {
            ipsps_position = SP_list.Count() - 1;
            GetIPSPSData(ipsps_position);
            EnableButtons(ipsps_position);
        }

        private void bPrevious_Click(object sender, RoutedEventArgs e)
        {
            ipsps_position = ipsps_position - 1;
            if (ipsps_position >= 0)
                GetIPSPSData(ipsps_position);
            else
                ipsps_position = 0;
            EnableButtons(ipsps_position);
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            //Новая точка
            if (ipsps_position == -1 && SP_list.Where(w => w.SP_code_old.Trim().ToLower() == tbSP_code.Text.Trim().ToLower()).FirstOrDefault() == null)
            {
                int visit=1;
                try { visit = Convert.ToInt16(tbVisit.Text); }
                catch { }
                Code.BaseRecord record = new BaseRecord()
                {
                    Dealer_name = tbDealername.Text,
                    Area = tbArea.Text,
                    Beeline_price = 1,
                    Base_RecStatus = BaseRecStatus.New,
                    City = tbCity.Text,
                    City_type = cbCityType.Text,
                    Comment = tbComment.Text,
                    Contact_person = tbContactPerson.Text,
                    Contact_phone = tbContactPhone.Text,
                    DW = cbDW.Text,
                    House = tbHouse.Text,
                    House_build = tbBuild.Text,
                    Megafon_price = 1,
                    Mts_price = 1,
                    RN = cbRN.Text,
                    SP_code_old = tbSP_code.Text,
                    SP_status = cbSPStatus.Text,
                    Street = tbStreet.Text,
                    Street_type = cbStreetType.Text,
                    Subway_station = cbSubway.Text,
                    Torgpred1 = String.Format("{0} {1}", USurname, UName),
                    Torgpred2 = String.Format("{0} {1}", USurname, UName),
                    ValidationStatus = new ValidationStatus(),
                    Visit_number = visit,
                    Zone = 1
                };

                if (helper.CreateUpdateSalePoint(record))
                {
                    SP_list.Add(record);
                    ipsps_position = SP_list.IndexOf(record);
                    GetIPSPSData(ipsps_position);
                    EnableButtons(ipsps_position);
                    bNewTT.Content = "Новая точка";
                    MessageBox.Show("Изменения сохранены", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                    MessageBox.Show("Ошибка сохранения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                //Обновляем новую|страую точку
                Code.BaseRecord old_record = SP_list.Where(w => w.SP_code_old.Trim().ToLower() == tbSP_code.Text.Trim().ToLower()).FirstOrDefault();
                if (ipsps_position != -1 && old_record != null)
                {
                    old_record.Dealer_name = tbDealername.Text;
                    old_record.Area = tbArea.Text;
                    old_record.City = tbCity.Text;
                    old_record.City_type = cbCityType.Text;
                    old_record.Comment = tbComment.Text;
                    old_record.Contact_person = tbContactPerson.Text;
                    old_record.Contact_phone = tbContactPhone.Text;
                    old_record.DW = cbDW.Text;
                    old_record.House = tbHouse.Text;
                    old_record.House_build = tbBuild.Text;
                    old_record.RN = cbRN.Text;
                    old_record.SP_code_old = tbSP_code.Text;
                    old_record.SP_status = cbSPStatus.Text;
                    old_record.Street = tbStreet.Text;
                    old_record.Street_type = cbStreetType.Text;
                    old_record.Subway_station = cbSubway.Text;
                    old_record.ValidationStatus = new ValidationStatus();
                    if (old_record.Base_RecStatus == BaseRecStatus.NotChanged)
                        old_record.Base_RecStatus = BaseRecStatus.Updated;
                    if (helper.CreateUpdateSalePoint(old_record))
                    {
                        EnableButtons(ipsps_position);
                        MessageBox.Show("Изменения сохранены", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                        MessageBox.Show("Ошибка сохранения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            helper.USurname = USurname;
            helper.UName = UName;
        }

        private void tbVisit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !helper.SymbolIsNumber(e.Text);
        }
    }
}

