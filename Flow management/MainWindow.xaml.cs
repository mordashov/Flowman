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

            //Формирования перечня дат по которым есть нормы
            string sql = "SELECT [nrm].[nrm_dt] as [date] FROM [nrm] GROUP BY [nrm].[nrm_dt] ORDER BY [nrm].[nrm_dt] DESC ;";
            GenerateComboBoxDate(sql);
            
            //Формирование DataGrid
            DataGridReqReload();

            //Формирование списка менеджеров
            sql = "SELECT mng.mng_tn as id, mng.mng_fln as Name FROM mng; ";
            GenerateComboBoxMp(sql);


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

            //Формирую DataGrid
            DataGridReqReload();

        }

        private void DataGridReqReload()
        {
            //Формирование DataGrid
            DateTime dt = DateTime.Parse(ComboBoxDate.SelectedValue.ToString());
            string tn = "%";
            try
            {
                tn = ComboBoxMp.SelectedValue.ToString();
            }
            catch (Exception e)
            {
               //
            }
            string sql = $@"
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
                                INNER JOIN (pos INNER JOIN (ord INNER JOIN (mng INNER JOIN flw ON mng.mng_tn = flw.mng_tn) 
                                ON ord.ord_id = flw.ord_id) 
                                ON pos.pos_id = flw.pos_id) ON stf.stf_tn = flw.stf_tn
                            WHERE ord.ord_dt = #{dt:M-d-yyyy}# AND mng.mng_tn LIKE '{tn}'
                            ";
            GenerateDataGridReq(sql);
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

        private void DataGridRequests_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //Изменение формата колонки Дата в DataGridRequests
            if (e.PropertyName == "Дата")
            {
                if (e.Column is DataGridTextColumn column)
                {
                    if (column.Binding is Binding binding) binding.StringFormat = "dd.MM.yyyy";
                }
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            Order order = new Order();
            order.Show();
        }
    }
}
