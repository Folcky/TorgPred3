using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Controls;

namespace TorgPred3.Code
{
    public class VisitHelper : StarterHelper
    {
        public IEnumerable<ComboBoxItem> GetSPStatuses(string SP_code_old)
        {
            string current_status = db.Dealers.Where(w => w.Sale_point_code_old == SP_code_old).Select(s => s.Sale_point_status).FirstOrDefault();

            return db.Sp_statuses.Select(s => new ComboBoxItem() { Content = s.Status_name, DataContext = s, IsSelected = s.Status_name.Trim().ToUpper() == current_status.Trim().ToUpper() });
        }

        public void FillVisits(ObservableCollection<Code.Visit> visits, string SP_code_old, string Report_date)
        {
            IEnumerable<Code.Visit> visited = db.Visits.Where(p => p.Sale_point_code_old == SP_code_old && p.Visit_date > DateTime.ParseExact(Report_date, date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Date.AddDays(-90)).Select(s=>new Code.Visit(){ Visit_date=s.Visit_date});
            IEnumerable<Code.Visit> uploaded = from u in db.Uploads
                                               where u.Upload_date > DateTime.ParseExact(Report_date, date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Date.AddDays(-90)
                                               && u.Sale_point_code_old == SP_code_old
                                               group u by u.Upload_date into gu
                                               select new Code.Visit()
                                               {
                                                   Visit_date = gu.Key,
                                                   Uploads_flag = gu.Count(),
                                                   Price_sum = gu.Sum(s => s.Price)
                                               };
            IEnumerable<Code.Visit> moneid = from m in db.Money
                                               where m.Money_date > DateTime.ParseExact(Report_date, date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Date.AddDays(-90)
                                               && m.Sale_point_code_old == SP_code_old
                                               group m by m.Money_date into gm
                                               select new Code.Visit()
                                               {
                                                   Visit_date = gm.Key,
                                                   Money_sum = gm.Sum(s => s.Money_sum)
                                               };
            IEnumerable<Code.Visit> refused = from r in db.Refusers
                                               where r.Refuse_date > DateTime.ParseExact(Report_date, date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Date.AddDays(-90)
                                               && r.Sale_point_code_old == SP_code_old
                                               group r by r.Refuse_date into gr
                                               select new Code.Visit()
                                               {
                                                   Visit_date = gr.Key,
                                                   Refusers_flag = gr.Count()
                                               };
            IEnumerable<Code.Visit> result = from r in visited.Union(uploaded.Union(refused.Union(moneid)))
                                             group r by r.Visit_date into gr
                                             select new Code.Visit()
                                             {
                                                 Visit_date = gr.Key,
                                                 Refusers_flag = gr.Max(m => m.Refusers_flag),
                                                 Uploads_flag = gr.Max(m => m.Uploads_flag),
                                                 Money_sum = gr.Max(m => m.Money_sum),
                                                 Price_sum = gr.Max(m => m.Price_sum)
                                             };
            foreach (Code.Visit item in result)
            {
                visits.Add(item);
            }
            
        }

        public void TryVisitSP(ObservableCollection<Code.Visit> visits, string SP_code_old, string Report_date)
        {
            DB.Visits visited = db.Visits.Where(p => p.Sale_point_code_old == SP_code_old && p.Visit_date == DateTime.ParseExact(Report_date, date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Date).FirstOrDefault();
            if (visited == null && visits.Where(w => w.Visit_date.ToString("dd.MM.yyyy") == Report_date).FirstOrDefault() == null)
            {
                visited = new DB.Visits() { Sale_point_code_old = SP_code_old, Visit_date = DateTime.ParseExact(Report_date, date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Date };
                db.Visits.InsertOnSubmit(visited);
                try { 
                    db.SubmitChanges();
                    visits.Add(new Code.Visit()
                    {
                        Visit_date = visited.Visit_date
                    });
                }
                catch { }
            }
        }
    }
}
