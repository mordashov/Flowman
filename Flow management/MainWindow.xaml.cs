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

        //Генерация перечня дат, по которым есть нормы
        private void GenerateComboBoxDate(string sql)
        {
            MsAccess acs = new MsAccess();
            ComboBoxDate.ItemsSource = acs.CreateDataTable(sql).DefaultView;
            ComboBoxDate.SelectedIndex = 0;
            ComboBoxDate.ItemStringFormat = "dd.MM.yyyy";
        }

        //Подсчет кол-ва 
        private string Сounting(DateTime data, string mp, string sql)
        {
            int param1 = 0;
            DateTime param2 = DateTime.Now;

            try
            {
                if (mp != null)
                {
                    param1 = int.Parse(mp);
                }

                param2 = data;
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

            cmd.Parameters.Add("Data", OleDbType.Date, 4, "Data");
            cmd.Parameters["Data"].Value = param2;

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

            //Формирования перечня дат по которым есть нормы
            sql = "SELECT [nrm].[nrm_dt] as [date] FROM [nrm] GROUP BY [nrm].[nrm_dt] ORDER BY [nrm].[nrm_dt] DESC ;";
            GenerateComboBoxDate(sql);

            //Подсчет кол-ва обращений
            CountRequestsMp();
        }

        private void CountRequestsMp()
        {
            //Получаю кол-во заявок по менеджеру
            string sql = @"SELECT Count(flw.flw_id) AS cnt
                FROM ord INNER JOIN flw ON ord.ord_id = flw.ord_id
                WHERE flw.mng_tn=@tn AND [ord_dt]=@Data;
                ";
            DateTime date = DateTime.Parse(ComboBoxDate.SelectedValue.ToString());
            string mp = "0";
            try
            {
                mp = ComboBoxMp.SelectedValue.ToString();
            }
            catch (Exception exception)
            {
                mp = "0";
            }

            LabelRequestsCount.Content = Сounting(date, mp, sql);

            sql = @"SELECT Sum(cor.cor_scr) AS [Sum-cor_scr]
                    FROM (ord INNER JOIN flw ON ord.ord_id = flw.ord_id) INNER JOIN (cor INNER JOIN app ON cor.cor_id = app.cor_id) ON ord.ord_id = app.ord_id
                    WHERE flw.mng_tn=@tn AND [ord_dt]=@Date";

            string content = Сounting(date, mp, sql);
            if (string.IsNullOrEmpty(content)) content = "0";
            LabelCountScr.Content = content;

            //Получаю кол-во заявок по менеджеру
            sql = @"SELECT Count(flw.flw_id) AS cnt
                FROM ord INNER JOIN flw ON ord.ord_id = flw.ord_id
                WHERE [ord_dt]=@Date;
                ";
            mp = null;
            LabelRequestsCount.Content = LabelRequestsCount.Content + "/" + Сounting(date, mp, sql);

            sql = @"SELECT Sum(cor.cor_scr) AS [Sum-cor_scr]
                    FROM (ord INNER JOIN flw ON ord.ord_id = flw.ord_id) INNER JOIN (cor INNER JOIN app ON cor.cor_id = app.cor_id) ON ord.ord_id = app.ord_id
                    WHERE [ord_dt]=@Date;
                    ";
            content = Сounting(date, mp, sql);
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

        private void ComboBoxDate_DropDownClosed(object sender, EventArgs e)
        {
            // Подсчет кол - ва обращений
            CountRequestsMp();
        }

        private void ComboBoxDate_KeyUp(object sender, KeyEventArgs e)
        {
            // Подсчет кол - ва обращений
            CountRequestsMp();
        }
    }
}
