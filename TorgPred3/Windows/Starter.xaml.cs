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
using System.Windows.Shapes;

using System.IO;
using System.Reflection;
using SWF=System.Windows.Forms;
using Microsoft.Win32;


namespace TorgPred3.Windows
{
    /// <summary>
    /// Interaction logic for Starter.xaml
    /// </summary>
    public partial class Starter : Window
    {
        public Starter()
        {
            InitializeComponent();
        }
        private Code.StarterHelper helper = new Code.StarterHelper();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            helper.BackupDB();
            tbSurname.Text = helper.GetUserInfo()[0];
            tbName.Text = helper.GetUserInfo()[1];
            if (tbSurname.Text == "" && tbName.Text == "")
                tbSurname.Focus();
            if (!helper.CheckDBPresence())
                MessageBox.Show("Проблемы с восстановлением базы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void bLogin_Click(object sender, RoutedEventArgs e)
        {
            if (helper.CheckDBPresence())
            {
                if (helper.TryLogin(tbSurname.Text, tbName.Text, tbPassword.Text))
                {
                    tbPassword.Text = "";
                    helper.SaveUserInfo(tbSurname.Text, tbName.Text);
                    MainWindow mainwindow = new MainWindow(tbSurname.Text.Trim(), tbName.Text.Trim());
                    mainwindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Ошибка входа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Проблемы с восстановлением базы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void bDirectory_Click(object sender, RoutedEventArgs e)
        {
            string current="";
            try { current = helper.GetWorkDir(); }
            catch { }
            using (SWF.FolderBrowserDialog dialog = new SWF.FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = true;
                if (current=="" || !Directory.Exists(current))
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
    }
}
