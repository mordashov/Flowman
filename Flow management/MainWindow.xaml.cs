using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
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

        //Генерация комбобокса с месяцем
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

        //Подсчет кол-ва 
        private string Сounting(string month, string year, string mp, string sql)
        {
            int param1 = 0;
            int param2 = 0;
            int param3 = 0;

            try
            {
                if (mp != null)
                {
                    param1 = int.Parse(mp);
                }

                param2 = int.Parse(year);
                param3 = int.Parse(month);
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка преобразования параметров при подсчете заявок");
                Environment.Exit(0);
            }

            MsAccess acs = new MsAccess();
            OleDbCommand cmd = new OleDbCommand(sql);

            if (mp != null)
            {
                cmd.Parameters.Add("tn", OleDbType.Integer, 10, "tn");
                cmd.Parameters["tn"].Value = param1; 
            }

            cmd.Parameters.Add("Year", OleDbType.Integer, 4, "Year");
            cmd.Parameters["Year"].Value = param2;

            cmd.Parameters.Add("Month", OleDbType.Integer, 4, "Month");
            cmd.Parameters["Month"].Value = param3;

            return acs.GetValueCommand(cmd);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Формирование DataGrid
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

            //Формирование списка менеджеров
            sql = "SELECT mng.mng_th as id, mng.mng_fln as Name FROM mng; ";
            GenerateComboBoxMp(sql);

            //Формирование списка годов
            sql = @"SELECT Year([ord_dt]) AS Год
                    FROM ord
                    GROUP BY Year([ord_dt]);";
            GenerateComboBoxYear(sql);

            //Формирование списка менеджеров
            GenerateComboBoxMonth();

            //Подсчет кол-ва обращений
            CountRequestsMp();
        }

        private void CountRequestsMp()
        {
            //Получаю кол-во заявок по менеджеру
            string sql = @"SELECT Count(flw.flw_id) AS cnt
                FROM ord INNER JOIN flw ON ord.ord_id = flw.ord_id
                WHERE (((flw.mng_tn)=@tn) AND ((Year([ord_dt]))=@Year) AND ((Month([ord_dt]))=@Month));
                ";
            string month = ComboBoxMonth.SelectedIndex.ToString();
            string year = ComboBoxYear.SelectedValue.ToString();
            string mp = "0";
            try
            {
                mp = ComboBoxMp.SelectedValue.ToString();
            }
            catch (Exception exception)
            {
                mp = "0";
            }

            LabelRequestsCount.Content = Сounting(month, year, mp, sql);

            sql = @"SELECT Sum(cor.cor_scr) AS [Sum-cor_scr]
                    FROM (ord INNER JOIN flw ON ord.ord_id = flw.ord_id) INNER JOIN (cor INNER JOIN app ON cor.cor_id = app.cor_id) ON ord.ord_id = app.ord_id
                    WHERE (((flw.mng_tn)=@tn) AND ((Year([ord_dt]))=@Year) AND ((Month([ord_dt]))=@Month))";

            string content = Сounting(month, year, mp, sql);
            if (string.IsNullOrEmpty(content)) content = "0";
            LabelCountScr.Content = content;

            //Получаю кол-во заявок по менеджеру
            sql = @"SELECT Count(flw.flw_id) AS cnt
                FROM ord INNER JOIN flw ON ord.ord_id = flw.ord_id
                WHERE (((Year([ord_dt]))=@Year) AND ((Month([ord_dt]))=@Month));
                ";
            mp = null;
            LabelRequestsCount.Content = LabelRequestsCount.Content + "/" + Сounting(month, year, mp, sql);

            sql = @"SELECT Sum(cor.cor_scr) AS [Sum-cor_scr]
                    FROM (ord INNER JOIN flw ON ord.ord_id = flw.ord_id) INNER JOIN (cor INNER JOIN app ON cor.cor_id = app.cor_id) ON ord.ord_id = app.ord_id
                    WHERE (((Year([ord_dt]))=@Year) AND ((Month([ord_dt]))=@Month))";

            content = Сounting(month, year, mp, sql);
            if (string.IsNullOrEmpty(content)) content = "0";
            LabelCountScr.Content = LabelCountScr.Content + "/" + content;


        }

        private void ComboBoxMp_DropDownClosed(object sender, EventArgs e)
        {
            // Подсчет кол - ва обращений
            CountRequestsMp();
        }

        private void ComboBoxMp_KeyUp(object sender, KeyEventArgs e)
        {
            // Подсчет кол - ва обращений
            CountRequestsMp();
        }

        private void ComboBoxYear_DropDownClosed(object sender, EventArgs e)
        {
            // Подсчет кол - ва обращений
            CountRequestsMp();

        }

        private void ComboBoxYear_KeyUp(object sender, KeyEventArgs e)
        {
            // Подсчет кол - ва обращений
            CountRequestsMp();

        }

        private void ComboBoxMonth_DropDownClosed(object sender, EventArgs e)
        {
            // Подсчет кол - ва обращений
            CountRequestsMp();

        }

        private void ComboBoxMonth_KeyUp(object sender, KeyEventArgs e)
        {
            // Подсчет кол - ва обращений
            CountRequestsMp();

        }

        private void DataGridRequests_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
        }
    }
}
