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
using SWF = System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.ComponentModel;

namespace TorgPred3.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string usurname, string uname)
        {
            InitializeComponent();
            USurname = usurname;
            UName = uname;
            //При добавлении, еще добавить ярлык иерархию на форме с соответствующим индексом
            _hviews.Add(new string[] { "City", "Street", "House", "SP_code_old" });
            _hviews.Add(new string[] { "City", "Street", "House", "House_build", "SP_code_old" });
            _hviews.Add(new string[] { "Dealer_name", "SP_code_old" });
            FillDealerTreeview(hview_index, true);
        }

        public string USurname { get; set; }
        public string UName { get; set; }

        private ObservableCollection<UserControl> _views = new ObservableCollection<UserControl>();
        public ObservableCollection<UserControl> Views { get { return _views; } }
        private ObservableCollection<Code.BaseRecord> base_recs = new ObservableCollection<Code.BaseRecord>();
        private Code.BaseHelper helper = new Code.BaseHelper();
        private Code.PriceListHelper pricelist_helper = new Code.PriceListHelper();
        private int hview_index = 1;
        private ObservableCollection<string[]> _hviews = new ObservableCollection<string[]>();

        private void FillDealerTreeview(int hview_index, bool UpdateFromDB)
        {
            if (UpdateFromDB)
                base_recs = helper.GetBaseDBData(USurname, UName);
            if (base_recs != null && base_recs.Count() > 0)
            {
                FillDealerHierarchy(tvDealerHierarchy, base_recs, _hviews.ElementAt(hview_index-1));
            }
        }

        private void bStock_Click(object sender, RoutedEventArgs e)
        {
            if (tvDealerHierarchy.Items.Count > 0)
            {
                if (PageTransition1.contentPresenter.Content == null || PageTransition1.contentPresenter.Content != null && PageTransition1.contentPresenter.Content.GetType() != typeof(Views.Invoice))
                    PageTransition1.ShowPage(GetViewManager(typeof(Views.Invoice)));
            }
            else
                MessageBox.Show("Ошибка. Обновите базу точек!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void bBase_Click(object sender, RoutedEventArgs e)
        {
            string current_dir = helper.GetWorkDir();
            Microsoft.Win32.OpenFileDialog open_doc = new Microsoft.Win32.OpenFileDialog();
            open_doc.DefaultExt = ".xlsx";
            open_doc.Filter = "Excel documents|*.xlsx;*.xls";
            open_doc.InitialDirectory = current_dir;
            bool doc_selected = (bool)open_doc.ShowDialog();
            if (doc_selected)
            {
                ObservableCollection<Code.BaseRecord> excel_base_recs = helper.LoadFromExcel(open_doc.FileName);
                if (excel_base_recs != null && excel_base_recs.Count() > 0)
                {
                    if (helper.UpdateBaseData(excel_base_recs))
                    {
                        tvDealerHierarchy.Items.Clear();
                        FillDealerTreeview(hview_index, true);
                    }
                }
            }
        }

        private void FillDealerHierarchy(TreeView outer, ObservableCollection<Code.BaseRecord> recs, string[] levels)
        {
            //Корневой элемент "все"
            TreeViewItem super_parent = new TreeViewItem();
            super_parent.Header = "Все торговые точки";
            outer.Items.Add(super_parent);
            super_parent.MouseDoubleClick += new MouseButtonEventHandler(DealerHierarchy_Click);
            super_parent.Tag = recs;
            super_parent.IsExpanded = true;

            IEnumerable<string> element = (from l in recs
                                           select l.GetType().GetProperty(levels.ElementAt(0)).GetValue(l, null).ToString()).OrderBy(p=>p).Distinct<string>();
            foreach (string level_element in element)
            {
                TreeViewItem parent = new TreeViewItem();
                parent.Header = level_element.Trim() == "" ? "Не определено" : level_element;
                super_parent.Items.Add(parent);
                parent.MouseDoubleClick += new MouseButtonEventHandler(DealerHierarchy_Click);
                ObservableCollection<Code.BaseRecord> tagger = new ObservableCollection<Code.BaseRecord>(recs.Where(l => l.GetType().GetProperty(levels.ElementAt(0)).GetValue(l, null).ToString() == level_element.ToString()));
                parent.Tag = tagger;
                FillParentNode(levels, parent, new ObservableCollection<Code.BaseRecord>(tagger), 0);
            }
        }

        private void FillParentNode(string[] levels, TreeViewItem parent, ObservableCollection<Code.BaseRecord> recs, int level)
        {
            if (level + 1 < levels.Count())
            {
                string header = parent.Header.ToString().Trim() == "Не определено" ? "" : parent.Header.ToString();

                IEnumerable<string> element = (from l in recs
                                               where l.GetType().GetProperty(levels.ElementAt(level)).GetValue(l, null).ToString().Trim() == header.Trim()
                                               select l.GetType().GetProperty(levels.ElementAt(level + 1)).GetValue(l, null).ToString().Trim()).OrderBy(o=>o).Distinct();
                foreach (string level_element in element)
                {
                    TreeViewItem tvitem = new TreeViewItem();
                    tvitem.Header = level_element.Trim() == "" ? "Не определено" : level_element;
                    parent.Items.Add(tvitem);
                    tvitem.MouseDoubleClick += new MouseButtonEventHandler(DealerHierarchy_Click);

                    ObservableCollection<Code.BaseRecord> tagger = new ObservableCollection<Code.BaseRecord>(recs.Where(l => l.GetType().GetProperty(levels.ElementAt(level + 1)).GetValue(l, null).ToString().Trim() == level_element.ToString().Trim()));
                    tvitem.Tag = tagger;

                    ObservableCollection<TreeViewItem> result = new ObservableCollection<TreeViewItem>();
                    GetTVItems(tvDealerHierarchy, base_recs, result);

                    if (levels.ElementAt(level + 1) == "SP_code_old")
                    {
                        tvitem.DataContext = tagger.FirstOrDefault();
                        if (tagger != null && tagger.ElementAt(0) != null)
                        {
                            tvitem.Header = String.Format("{0}({1})", helper.GetActualSPCode(tagger.ElementAt(0).SP_code_old, true), tagger.ElementAt(0).SP_code_old);
                        }
                    }
                    FillParentNode(levels, tvitem, new ObservableCollection<Code.BaseRecord>(tagger), level + 1);
                }
            }
        }

        private void DealerHierarchy_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is TreeViewItem
                        && (e.Source as TreeViewItem).IsSelected)
            {
                IEnumerable<Code.BaseRecord> tag = (e.Source as TreeViewItem).Tag as IEnumerable<Code.BaseRecord>;
                if (tag !=null)
                {
                    if (tag.Count() == 1)
                    {
                        PageTransition1.ShowPage(GetViewManager(typeof(Views.SalePointWork), tag));
                    }
                    else
                    {
                        if (tag.Count() > 1)
                        {
                            Views.SalePointsReport spsreport = GetViewManager(typeof(Views.SalePointsReport), tag) as Views.SalePointsReport;
                            if (spsreport != null)
                            {
                                spsreport.SPSelected -= new System.ComponentModel.PropertyChangedEventHandler(spsreport_SPSelected);
                                spsreport.SPSelected += new System.ComponentModel.PropertyChangedEventHandler(spsreport_SPSelected);
                                PageTransition1.ShowPage(spsreport);
                            }
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void spsreport_SPSelected(object sender, PropertyChangedEventArgs e)
        {
            Views.ShortSPInfo sspi = sender as Views.ShortSPInfo;
            if (sspi != null)
            {
                ObservableCollection<Code.BaseRecord> tag = new ObservableCollection<Code.BaseRecord>();
                tag.Add(sspi.DataContext as Code.BaseRecord);
                //Code.BaseRecord tag = sspi.DataContext as Code.BaseRecord;
                if (tag != null)
                {
                    PageTransition1.ShowPage(GetViewManager(typeof(Views.SalePointWork), tag));
                }
            }
        }

        private UserControl GetViewManager(Type object_type, IEnumerable<Code.BaseRecord> tag, string usurname, string uname)
        {
            UserControl view = (from v in this.Views
                                           where v.GetType() == object_type
                                           && CompareTags(v.GetType().GetProperty("SP_list").GetValue(v, null), tag)
                                           select v).FirstOrDefault();
            if (view == null)
            {
                view = Activator.CreateInstance(object_type, tag, usurname, uname) as UserControl;
                this.Views.Add(view);
                return view;
            }
            else
                return view;
        }

        private UserControl GetViewManager(Type object_type, IEnumerable<Code.BaseRecord> tag)
        {
            UserControl view = (from v in this.Views
                                where v.GetType() == object_type
                                && CompareTags(v.GetType().GetProperty("SP_list").GetValue(v, null), tag)
                                select v).FirstOrDefault();
            if (view == null)
            {
                view = Activator.CreateInstance(object_type, tag) as UserControl;
                this.Views.Add(view);
                return view;
            }
            else
                return view;
        }

        private bool CompareTags(object tag_1, object tag_2)
        {
            ObservableCollection<Code.BaseRecord> tagcast_1 = tag_1 as ObservableCollection<Code.BaseRecord>;
            ObservableCollection<Code.BaseRecord> tagcast_2 = tag_2 as ObservableCollection<Code.BaseRecord>;
            bool result = false;
            if (tagcast_1 != null && tagcast_2 != null)
                result = tagcast_1.Select(s => s.SP_code_old).OrderBy(i => i).SequenceEqual(tagcast_2.Select(s => s.SP_code_old).OrderBy(i => i));
            return result;
        }

        private UserControl GetViewManager(Type object_type)
        {
            UserControl view = (from v in this.Views
                                where v.GetType() == object_type
                                select v).FirstOrDefault();
            if (view == null)
            {
                view = Activator.CreateInstance(object_type) as UserControl;
                this.Views.Add(view);
                return view;
            }
            else
                return view;
        }

        private UserControl GetViewManager(Type object_type, object parametr)
        {
            UserControl view = (from v in this.Views
                                where v.GetType() == object_type
                                select v).FirstOrDefault();
            if (view == null)
            {
                view = Activator.CreateInstance(object_type, parametr) as UserControl;
                this.Views.Add(view);
                return view;
            }
            else
                return view;
        }

        private void cbHViews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (btSearch != null)
                btSearch.Content = "Поиск";
            if (cbHViews.SelectedItem != null &&
                cbHViews.SelectedItem.GetType() == typeof(ComboBoxItem) &&
                (cbHViews.SelectedItem as ComboBoxItem).DataContext != null)
            {
                try
                {
                    if (_hviews.Count() > 1)
                    {
                        hview_index = Convert.ToInt16((cbHViews.SelectedItem as ComboBoxItem).DataContext);
                        tvDealerHierarchy.Items.Clear();
                        FillDealerTreeview(hview_index, false);
                    }
                }
                catch
                {
                    MessageBox.Show("Не удалось установить вид иерархии точек", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    hview_index = 1;
                    tvDealerHierarchy.Items.Clear();
                    FillDealerTreeview(hview_index, false);
                }
            }
        }

        private void bStockStatus_Click(object sender, RoutedEventArgs e)
        {
            if (tvDealerHierarchy.Items.Count > 0)
            {
                if (PageTransition1.contentPresenter.Content == null
                    || PageTransition1.contentPresenter.Content != null && PageTransition1.contentPresenter.Content.GetType() != typeof(Views.StockReport))
                    PageTransition1.ShowPage(GetViewManager(typeof(Views.StockReport)));
            }
            else
                MessageBox.Show("Ошибка. Обновите базу точек!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void bPriceList_Click(object sender, RoutedEventArgs e)
        {
            string current_dir = helper.GetWorkDir();
            Microsoft.Win32.OpenFileDialog open_doc = new Microsoft.Win32.OpenFileDialog();
            open_doc.DefaultExt = ".xlsx";
            open_doc.Filter = "Excel documents|*.xlsx;*.xls";
            open_doc.InitialDirectory = current_dir;
            bool doc_selected = (bool)open_doc.ShowDialog();
            if (doc_selected)
            {
                ObservableCollection<Code.PriceListRecord> pricelist_recs = pricelist_helper.LoadFromExcel(open_doc.FileName);
                if (pricelist_recs != null && pricelist_recs.Count() > 0)
                {
                    if (pricelist_helper.UpdatePriceListData(pricelist_recs))
                    {
                        MessageBox.Show("Номенклатура обновлена", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void bAddSp_Click(object sender, RoutedEventArgs e)
        {
            if (PageTransition1.contentPresenter.Content == null || PageTransition1.contentPresenter.Content != null && PageTransition1.contentPresenter.Content.GetType() != typeof(Views.SalePointEditor))
            {
                PageTransition1.ShowPage(GetViewManager(typeof(Views.SalePointEditor), base_recs, USurname, UName));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lUser.Content = String.Format("{0} {1}",this.USurname, this.UName);
            helper.USurname = USurname;
            helper.UName = UName;
        }

        private void bDirectory_Click(object sender, RoutedEventArgs e)
        {
            string current = "";
            try { current = helper.GetWorkDir(); }
            catch { }
            using (SWF.FolderBrowserDialog dialog = new SWF.FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = true;
                if (current == "" || !Directory.Exists(current))
                    dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                else
                    dialog.SelectedPath = current;
                dialog.Description = "Выбрать папку, где будут храниться файлы.";
                //dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (Directory.Exists(dialog.SelectedPath))
                    {
                        if (helper.SetWorkDir(dialog.SelectedPath))
                        {
                            MessageBox.Show("Рабочая директория сохранена", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось запомнить путь к рабочей директории.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                        MessageBox.Show("Такой директории не существует", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            if (base_recs != null && base_recs.Count() > 0)
            {
                if (btSearch.Content.ToString() == "Поиск")
                {
                    ObservableCollection<Code.BaseRecord> founded = new ObservableCollection<Code.BaseRecord>((from sps in base_recs.OrderBy(o => o.Dealer_name)
                                               where
                                               sps.Dealer_name.Contains(tbSearch.Text)
                                               || sps.Area.Contains(tbSearch.Text)
                                               || sps.City.Contains(tbSearch.Text)
                                               || sps.Comment.Contains(tbSearch.Text)
                                               || sps.Contact_person.Contains(tbSearch.Text)
                                               || sps.Contact_phone.Contains(tbSearch.Text)
                                               || sps.DW.Contains(tbSearch.Text)
                                               || sps.House.Contains(tbSearch.Text)
                                               || sps.RN.Contains(tbSearch.Text)
                                               || sps.SP_code_new.Contains(tbSearch.Text)
                                               || sps.SP_code_next.Contains(tbSearch.Text)
                                               || sps.SP_code_old.Contains(tbSearch.Text)
                                               || sps.Street.Contains(tbSearch.Text)
                                               || sps.Subway_station.Contains(tbSearch.Text)
                                               select sps).OrderBy(o => o.Dealer_name));
                    if (founded.Count() > 0)
                    {
                        if (founded.Count() > 1)
                            btSearch.Content = "След.";
                        int index = 0;
                        btSearch.Tag = index;
                        ObservableCollection<TreeViewItem> result = new ObservableCollection<TreeViewItem>();
                        GetTVItems(tvDealerHierarchy, founded, result);
                        btSearch.DataContext = result;
                        result.FirstOrDefault().IsSelected = true;
                        ExpandTVItem(result.FirstOrDefault());
                        result.FirstOrDefault().BringIntoView();
                    }
                }
                else if (btSearch.Content.ToString() == "След.")
                {
                    ObservableCollection<TreeViewItem> result = btSearch.DataContext as ObservableCollection<TreeViewItem>;
                    int index = (int)btSearch.Tag + 1;
                    if (index >= result.Count())
                    {
                        btSearch.Content = "Поиск";
                    }
                    else
                    {
                        btSearch.Tag = index;
                        result.ElementAt(index).IsSelected = true;
                        ExpandTVItem(result.ElementAt(index));
                        result.ElementAt(index).BringIntoView();
                    }
                }
            }
        }

        private void GetTVItems(ItemsControl container, ObservableCollection<Code.BaseRecord> base_recs, ObservableCollection<TreeViewItem> result)
        {
            if (container != null)
            {
                if (base_recs.Contains(container.DataContext as Code.BaseRecord))
                    result.Add(container as TreeViewItem);
                else
                    foreach (var item in container.Items.Cast<TreeViewItem>())
                        GetTVItems(item, base_recs, result);
            }
        }

        private void ExpandTVItem(TreeViewItem selected)
        {
            if (selected != null)
            {
                selected.IsExpanded = true;
                ExpandTVItem(selected.Parent as TreeViewItem);
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            btSearch.Content = "Поиск";
        }

        private void bReports_Click(object sender, RoutedEventArgs e)
        {
            Reports report_window = new Reports(USurname, UName);
            report_window.ShowDialog();
        }
    }
}
