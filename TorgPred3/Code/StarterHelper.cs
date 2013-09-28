using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows;

namespace TorgPred3.Code
{
    public class StarterHelper
    {
        public string[] date_formats = { "dd.MM.yyyy HH:mm", "dd.MM.yyyy H:mm", "dd.MM.yyyy H:mm:ss", "dd.MM.yyyy", "dd.MM.yy" };
        public DB.Datafile db;
        public string dataFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + Properties.Settings.Default.datapath + @"\" + Properties.Settings.Default.datafile;

        public StarterHelper()
        {
            DefineDB();
        }

        public bool RestoreResource(string resource, string destination)
        {
            try
            {
                string[] ir = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                FileStream resourceFile = new FileStream(destination, FileMode.Create);
                byte[] b = new byte[s.Length + 1];
                s.Read(b, 0, Convert.ToInt32(s.Length));
                resourceFile.Write(b, 0, Convert.ToInt32(b.Length - 1));
                resourceFile.Flush();
                resourceFile.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool BackupDB()
        {
            bool result = false;
            try
            {
                FileInfo fi = new FileInfo(dataFile);

                string wrkdir = GetWorkDir();
                if (wrkdir != "" && !Directory.Exists(wrkdir + @"\Backup"))
                    Directory.CreateDirectory(wrkdir + @"\Backup");


                if (fi.Exists == true && fi.Length != 0 && wrkdir != "" && Directory.Exists(wrkdir + @"\Backup"))
                {
                    fi.CopyTo(String.Format(@"{0}\{1}\{2}_{3}.sdf", wrkdir, "Backup", "datafile", DateTime.Now.ToString("yyyyMMddmmss")));
                    result = true;
                }
            }
            catch { MessageBox.Show("Ошибка архивирования базы"); }
            return result;
        }

        public bool CheckDBPresence()
        {
            bool result = false;
            try
            {
                FileInfo fi = new FileInfo(dataFile);
                if (fi.Exists == false || fi.Length == 0)
                {
                    Directory.CreateDirectory(fi.DirectoryName);
                    if (RestoreResource("TorgPred3.Resources.datafile.sdf", dataFile))
                    {
                        DefineDB();
                        result = true;
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch { MessageBox.Show("1"); }
            return result;
        }

        public void DefineDB()
        {
            string password = Properties.Settings.Default.Connector.Substring(0, 7);
            string connectionstring = String.Format(@"Data Source={0};Password={1}", dataFile, password);
            if (File.Exists(dataFile))
            {
                try
                {
                    db = new DB.Datafile(connectionstring);
                }
                catch (Exception e){ MessageBox.Show(e.InnerException + "|" + e.Message); }
            }
        }

        public bool TryLogin(string surname, string name, string password)
        {
            bool result = false;
            if (surname.Trim() != "" && name.Trim() != "" && password.Trim() != "")
            {
                DB.Login login = db.Login.Where(w => w.Surname == surname && w.Name == name).FirstOrDefault();
                if (login == null)
                {
                    login = new DB.Login() { Surname = surname, Name = name, Password = password };
                    db.Login.InsertOnSubmit(login);
                    try { db.SubmitChanges(); result = true; }
                    catch { }
                }
                else
                {
                    if (login.Password == password)
                    { result = true; }

                }
            }
            return result;
        }

        public string[] GetUserInfo()
        {
            string[] array = new string[2];
            array[0] = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\TorgPred3\Surname", "", null);
            array[1] = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\TorgPred3\Name", "", null);
            return array;
        }

        public string GetWorkDir()
        {
            string result = "";
            try
            {
                result = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\TorgPred3\WorkDir", "", null);
                if (Directory.Exists(result))
                    return result;
                else
                    return "";
            }
            catch { }
            return result;
        }

        public bool SetWorkDir(string SelectedPath)
        {
            try
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TorgPred3\WorkDir", "", SelectedPath, RegistryValueKind.String);
                return true;
            }
            catch { return false; }

        }

        public bool SaveUserInfo(string surname, string name)
        {
            bool result = false;
            try
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TorgPred3\Surname", "", surname.Trim(), RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TorgPred3\Name", "", name.Trim(), RegistryValueKind.String);
                result = true;
            }
            catch { }
            return result;
        }

        public string GetActualSPCode(string sp_code_old, bool try_cut_code)
        {
            string result = "";
            result = db.Dealers.Where(w => w.Sale_point_code_old == sp_code_old).Select(s => s.Sale_point_code_next).FirstOrDefault();
            if (result != null && result.Trim() != "" && try_cut_code)
                result = result.Substring(result.Length - Math.Min(5, result.Length));
            if (result == null || (result != null && result.Trim() == ""))
            {
                result = db.Dealers.Where(w => w.Sale_point_code_old == sp_code_old).Select(s => s.Sale_point_code_new).FirstOrDefault();
                if (result != null && result.Trim() != "" && try_cut_code)
                    result = result.Substring(result.Length - Math.Min(5, result.Length));
            }
            if (result == null || (result != null && result.Trim() == ""))
                result = sp_code_old;
            return result;
        }

        public Code.Upload DbUpload2CodeUpload(DB.Uploads dbupload)
        {
            return new Code.Upload()
            {
                Document_num = dbupload.Document_num,
                ICC_id = dbupload.Barcode,
                Price = dbupload.Price,
                ValidationStatus = new ValidationStatus(),
                SP_code_old = dbupload.Sale_point_code_old,
                TP_name = dbupload.Tp_name,
                Upload_date = dbupload.Upload_date
            };
        }

        public Code.Refuse DbRefuse2CodeRefuse(DB.Refusers dbrefuse)
        {
            return new Code.Refuse()
            {
                ICC_id = dbrefuse.Barcode,
                Refuse_date = dbrefuse.Refuse_date,
                SP_code_new = dbrefuse.Sale_point_code_new,
                SP_code_old = dbrefuse.Sale_point_code_old,
                ValidationStatus = new ValidationStatus()
            };
        }

        public Code.BaseRecord DbDealer2CodeBaseRecord(DB.Dealers b)
        {
            return new Code.BaseRecord()
            {
                City = b.City,
                City_type = b.City_type,
                Dealer_name = b.Dealer_name,
                Street = b.Street,
                Street_type = b.Street_type,
                House = b.House,
                House_build = b.House_build,
                SP_code_new = b.Sale_point_code_new,
                SP_code_old = b.Sale_point_code_old,
                Area = b.Area,
                Comment = b.Add_comment,
                Contact_person = b.Contact_person,
                Contact_phone = b.Contact_phone,
                DW = b.DW,
                RN = b.RN,
                Subway_station = b.Subway_station_name,
                Visit_number = b.Visit_number,
                Zone = b.Zone_id,
                Torgpred2 = b.TorgPred2,
                Torgpred1 = b.TorgPred1,
                Mts_price = b.Mts_price_id,
                Megafon_price = b.Megafon_price_id,
                Beeline_price = b.Beeline_price_id,
                SP_code_next = b.Sale_point_code_next,
                SP_status = b.Sale_point_status,
                ValidationStatus = new ValidationStatus(),
                Base_RecStatus = (BaseRecStatus)b.Record_status,
                ADR_awr = b.Adr_awr,
                Infocart_reg = b.Infocart_registration,
                SP_desc = b.Sale_point_description,
                SP_profile_type = b.Sale_point_profile_type,
                Suplier_SP_type = b.Suplier_sale_point_type//, Comm_rate=b.Comm_rate
            };
        }

        public DB.Dealers CodeBaseRecord2DbDealer(Code.BaseRecord a)
        {
            try
            {
                return new DB.Dealers()
                {
                    Dealer_name = a.Dealer_name,
                    City = a.City,
                    City_type = a.City_type,
                    Street = a.Street,
                    Street_type = a.Street_type,
                    House = a.House,
                    House_build = a.House_build,
                    Sale_point_code_old = a.SP_code_old,
                    Sale_point_code_new = a.SP_code_new,
                    RN = a.RN,
                    DW = a.DW,
                    Contact_person = a.Contact_person,
                    Contact_phone = a.Contact_phone,
                    Add_comment = a.Comment,
                    Area = a.Area,
                    Subway_station_name = a.Subway_station,
                    Visit_number = a.Visit_number,
                    Zone_id = a.Zone,
                    Beeline_price_id = a.Beeline_price,
                    Megafon_price_id = a.Megafon_price,
                    Mts_price_id = a.Mts_price,
                    Sale_point_code_next = a.SP_code_next,
                    TorgPred1 = a.Torgpred1,
                    TorgPred2 = a.Torgpred2,
                    Sale_point_status = a.SP_status,
                    Adr_awr = a.ADR_awr,
                    //Comm_rate=a.Comm_rate,
                    Infocart_registration = a.Infocart_reg,
                    Record_status = (int)a.Base_RecStatus,
                    Sale_point_description = a.SP_desc,
                    Sale_point_profile_type = a.SP_profile_type,
                    Suplier_sale_point_type = a.Suplier_SP_type
                };
            }
            catch { return null; }
        }

        public bool SymbolIsNumber(string text)
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        public DateTime GetFirstDateOfWeek(DateTime dayInWeek, DayOfWeek firstDay, int offset = 0)
        {
            return dayInWeek.Date.AddDays(firstDay - dayInWeek.Date.DayOfWeek + offset * 7);
        }

        public DateTime GetLastDateOfWeek(DateTime dayInWeek, DayOfWeek firstDay, int offset = 0)
        {
            return dayInWeek.Date.AddDays(8 - firstDay - dayInWeek.Date.DayOfWeek + offset * 7);
        }

        public DB.Money CodeMoney2DbMoney(Code.MoneySum item, DB.Money money2update)
        {
            if (money2update == null)
            {
                money2update = new DB.Money();
            }
            money2update.Sale_point_code_old = item.SP_code_old;
            money2update.Sale_point_code_new = item.SP_code_new;
            money2update.Money_sum = item.Money_sum;
            money2update.Money_action_id = item.Money_action_id;
            money2update.Money_date = item.Money_date;
            money2update.Operation_date = DateTime.Now;
            return money2update;
        }

        public IEnumerable<Code.MoneySum> GetDebetSum(DateTime ToDate)
        {
            IEnumerable<Code.MoneySum> visits = from v in db.Visits
                                                 where v.Visit_date.Date <= ToDate.Date
                                                 group v by v.Sale_point_code_old into g
                                                 select new Code.MoneySum()
                                                 {
                                                     SP_code_old = g.Key,
                                                     Money_sum = 0,
                                                     Money_date = g.Max(s => s.Visit_date)
                                                 };

            IEnumerable<Code.MoneySum> uploads = from u in db.Uploads
                      where u.Upload_date.Date <= ToDate.Date
                      group u by u.Sale_point_code_old into last_g
                      select new Code.MoneySum()
                      {
                          SP_code_old = last_g.Key,
                          Money_sum = last_g.Sum(s => -s.Price),
                          Money_date = last_g.Max(s => s.Upload_date)
                      };

            IEnumerable<Code.MoneySum> moneis = from m in db.Money
                                                where m.Money_date.Date <= ToDate.Date
                                                group m by m.Sale_point_code_old into g
                                                select new Code.MoneySum()
                                                {
                                                    SP_code_old = g.Key,
                                                    Money_sum = g.Sum(s => s.Money_sum),
                                                    Money_date = g.Max(s => s.Money_date)
                                                };
            IEnumerable<Code.MoneySum> report = from u in uploads.Union(moneis).Union(visits)
                                                group u by u.SP_code_old into r
                                                select new Code.MoneySum()
                                                {
                                                    SP_code_old = r.Key,
                                                    Money_sum = r.Sum(s=>s.Money_sum),
                                                    Money_date = r.Max(m=>m.Money_date)
                                                };

            return report;
        }
    }
}
