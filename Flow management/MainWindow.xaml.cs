using System;
using System.Collections;
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

namespace Flow_management
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Обновление DataGrid
        private void GenerateDataGridReq(string sql)
        {
            MsAccess acs = new MsAccess();
            DataGridRequests.DataContext = acs.CreateDataTable(sql);
        }

        //Генерация списка менеджеров для комбобокс
        private void GenerateComboBoxMp(string sql)
        {
            MsAccess acs = new MsAccess();
            ComboBoxMp.ItemsSource = acs.CreateDataTable(sql).DefaultView;
        }

        //Генерация списка годов для комбобокс присутствующих в потоке заявок
        private void GenerateComboBoxYear(string sql)
        {
            MsAccess acs = new MsAccess();
            ComboBoxYear.ItemsSource = acs.CreateDataTable(sql).DefaultView;
            ComboBoxYear.SelectedValue = DateTime.Now.Year.ToString();
        }

        private void GenerateComboBoxMonth()
        {
            ComboBoxMonth.Items.Add("Январь");
            ComboBoxMonth.Items.Add("Фераль");
            ComboBoxMonth.Items.Add("Март");
            ComboBoxMonth.Items.Add("Апрель");
            ComboBoxMonth.Items.Add("Май");
            ComboBoxMonth.Items.Add("Июнь");
            ComboBoxMonth.Items.Add("Июль");
            ComboBoxMonth.Items.Add("Август");
            ComboBoxMonth.Items.Add("Сентябрь");
            ComboBoxMonth.Items.Add("Октябрь");
            ComboBoxMonth.Items.Add("Ноябрь");
            ComboBoxMonth.Items.Add("Декабрь");

            ComboBoxMonth.SelectedIndex = (int)DateTime.Now.Month;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string sql = @"
                            SELECT 
                            mng.mng_fln AS МП
                            , ord.ord_num AS Обращение
                            , ord.ord_dt AS Дата
                            , stf.stf_fln AS Сотрудник
                            , pos.pos_nm AS Должность
                            , dep.dep_mn AS Отдел
                            , """" AS Норма
                            , """" AS Итого
                            , """" AS [Норма отдела]
                            , """" AS [Итого по отделу]
                            FROM (dep INNER JOIN stf ON dep.dep_id = stf.dep_id) 
                                INNER JOIN (pos INNER JOIN (ord INNER JOIN (mng INNER JOIN flw ON mng.mng_th = flw.mng_tn) 
                                ON ord.ord_id = flw.ord_id) 
                                ON pos.pos_id = flw.pos_id) ON stf.stf_tn = flw.stf_tn;
                            ";
            GenerateDataGridReq(sql);

            sql = "SELECT mng.mng_th as id, mng.mng_fln as Name FROM mng; ";
            GenerateComboBoxMp(sql);

            sql = @"SELECT Year([ord_dt]) AS Год
                    FROM ord
                    GROUP BY Year([ord_dt]);";
            GenerateComboBoxYear(sql);

            GenerateComboBoxMonth();
        }
    }
}
