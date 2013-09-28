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
using System.Text.RegularExpressions;

namespace TorgPred3.Views
{
    /// <summary>
    /// Interaction logic for MoneyGetter.xaml
    /// </summary>
    public partial class MoneyGetter : UserControl
    {
        public MoneyGetter(IEnumerable<Code.BaseRecord> salepoints, string report_date)
        {
            InitializeComponent();
            this.SP_list = salepoints;
            this.Report_date = report_date;
            helper.FilldgMoney(this.monies, this.SP_list, this.Report_date);
            dgMoney.ItemsSource = this.monies;
            monies.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(monies_CollectionChanged);
        }

        //Money Properties
        public IEnumerable<Code.BaseRecord> SP_list
        { get; set; }
        public string Report_date
        { get; set; }
        private ObservableCollection<Code.MoneySum> _monies = new ObservableCollection<Code.MoneySum>();
        public ObservableCollection<Code.MoneySum> monies
        { get { return _monies; } }
        private Code.MoneyHelper helper = new Code.MoneyHelper();

        protected void monies_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (Code.MoneySum s in e.NewItems)
                {
                    s.SP_code_new = this.SP_list.ElementAt(0).SP_code_new;
                    s.SP_code_old = this.SP_list.ElementAt(0).SP_code_old;
                    s.Money_date = DateTime.ParseExact(this.Report_date, helper.date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                    s.owner_collection = this.monies;
                }
            else
                if (e.Action == NotifyCollectionChangedAction.Remove)
                    foreach (Code.MoneySum item in e.OldItems)
                    {
                        helper.DeleteMoneyRecord(item);
                    }
        }

        private void dgMoney_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //Заканчиваем редактировать запись
            //Прикручиваем к каждой записи отслеживание изменений свойств
            if (e.Row.DataContext != null && e.Row.DataContext.GetType() == typeof(Code.MoneySum))
            {
                Code.MoneySum item = e.Row.DataContext as Code.MoneySum;
                if ((e.Row.DataContext as Code.MoneySum).Money_action_id == 2 || (e.Row.DataContext as Code.MoneySum).Money_date.Date.ToString("dd.MM.yyyy") != this.Report_date)
                    e.Cancel = true;
                if (item.Money_action_id != 2)
                    item.Money_action_id = 1;

                item.PropertyChanged -= new PropertyChangedEventHandler(moneyrecord_PropertyChanged);
                item.PropertyChanged += new PropertyChangedEventHandler(moneyrecord_PropertyChanged);
            }
        }

        private void moneyrecord_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Пользователь может редактировать только icc и номинал
            if (e.PropertyName == "Money_sum")
            {
                if (sender.GetType() == typeof(Code.MoneySum))
                {
                    Code.MoneySum item = sender as Code.MoneySum;
                    //Валидируем запись, будем использовать статус в валидаторе
                    if (helper.ValidateMoneyRecord(ref item, Report_date))
                    {
                        //если все ок создаем/обновляем запись в БД
                        helper.CreateOrUpdateMoneyRecord(item);
                    }
                }
            }
        }

        private void bCribMoney_Click(object sender, RoutedEventArgs e)
        {
            Windows.AdminUnlock au = new Windows.AdminUnlock();
            au.ShowDialog();
            if (au.Logined)
            {
                try
                {
                    decimal test = Convert.ToDecimal(tbCribMoney.Text);
                    Code.MoneySum item = this.monies.Where(w => w.Money_date == DateTime.ParseExact(this.Report_date, helper.date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None)
                        && w.Money_action_id == 2).FirstOrDefault();
                    if (item == null)
                    {
                        item = new Code.MoneySum()
                                                {
                                                    Money_action_id = 2
                                                };
                        item.PropertyChanged -= new PropertyChangedEventHandler(moneyrecord_PropertyChanged);
                        item.PropertyChanged += new PropertyChangedEventHandler(moneyrecord_PropertyChanged);
                        item.Money_sum = Convert.ToDecimal(tbCribMoney.Text);
                        monies.Add(item);
                    }
                    else
                    {
                        item.Money_sum = Convert.ToDecimal(tbCribMoney.Text);
                    }
                    //если все ок создаем/обновляем запись в БД
                    helper.CreateOrUpdateMoneyRecord(item);
                }
                catch { MessageBox.Show("Введено неверное значение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            else
            {
                MessageBox.Show("Функция не доступна", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void dgtcMoney_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (dgMoney.SelectedItem != null && dgMoney.SelectedItem.GetType() == typeof(Code.MoneySum))
            {
                if ((dgMoney.SelectedItem as Code.MoneySum).Money_action_id == 2 || (dgMoney.SelectedItem as Code.MoneySum).Money_date.Date.ToString("dd.MM.yyyy") != this.Report_date)
                    e.Handled = true;
            }
        }

        private void tbCribMoney_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex1 = new Regex(@"\d");
            Regex regex2 = new Regex(@"-");

            if (regex1.IsMatch(e.Text) || regex2.IsMatch(e.Text))
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void dgMoney_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                try
                {
                    IEnumerable<Code.MoneySum> money = from m in dgMoney.SelectedItems.Cast<Code.MoneySum>()
                                                       where m.Money_action_id != 1 ||
                                                       m.Money_date.ToString("dd.MM.yyyy") != this.Report_date
                                                       select m;
                    if (money.Count() > 0)
                    {
                        MessageBox.Show("Нельзя удалять данные за прошлые периоды и списание средств.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
                        e.Handled = true;
                    }
                }
                catch { }
            }
        }
    }
}
