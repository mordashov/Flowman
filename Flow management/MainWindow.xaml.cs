using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
                            FROM (dep INNER JOIN stf ON dep.dep_id = stf.dep_id) 
                                INNER JOIN (pos INNER JOIN (ord INNER JOIN (mng INNER JOIN flw ON mng.mng_tn = flw.mng_tn) 
                                ON ord.ord_id = flw.ord_id) 
                                ON pos.pos_id = flw.pos_id) ON stf.stf_tn = flw.stf_tn
                            WHERE ord.ord_dt = #{dt:M-d-yyyy}# AND mng.mng_tn LIKE '{tn}'
                            ORDER BY ord.ord_id DESC
                            ";
            GenerateDataGridReq(sql);
        }

        private void ComboBoxMp_DropDownClosed(object sender, EventArgs e)
        {
            // Подсчет кол - ва обращений
            CountRequestsMp();

            ButtonCopy.Visibility = Visibility.Collapsed;

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

        private int ColumnWith(int colWith)
        {
            //Эталонная сумма всех столбцов. Это сумма ширины всех колонок заданных в list
            double AllWidth = 1140;
            //Отношение эталона к оригиналу
            double dgRat = DataGridRequests.ActualWidth / AllWidth;
            //Ширина колонки умноженное на отношение эталона к оригиналу
            double result = colWith * dgRat;
            return Convert.ToInt32(result);
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

            //Изменение ширины колонок
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("МП", ColumnWith(280)),
                new KeyValuePair<string, int>("Обращение", ColumnWith(140)),
                new KeyValuePair<string, int>("Дата", ColumnWith(100)),
                new KeyValuePair<string, int>("Сотрудник", ColumnWith(300)),
                new KeyValuePair<string, int>("Должность", ColumnWith(200)),
                new KeyValuePair<string, int>("Отдел", ColumnWith(100))
            };

            foreach (var itemPair in list)
            {
                if (e.PropertyName == itemPair.Key)
                {
                    if (e.Column is DataGridTextColumn column)
                    {
                        column.Width = itemPair.Value;
                    }
                }
            }



        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxMp.SelectedValue == null)
            {
                MessageBox.Show("Выберите менеджера потока");
                return;

            }

            Order order = new Order
            {
                DateOrder = DateTime.Parse(ComboBoxDate.SelectedValue.ToString()),
                MngTn = ComboBoxMp.SelectedValue.ToString()
            };
            order.ShowDialog();
            DataGridReqReload();
            CountRequestsMp();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            //Формирование списка менеджеров
            string sql = "SELECT mng.mng_tn as id, mng.mng_fln as Name FROM mng; ";
            GenerateComboBoxMp(sql);

            DataGridReqReload();

            //пересчет кол-ва обращений и баллов
            CountRequestsMp();
            
            //Скрываю кнопку скопировать
            ButtonCopy.Visibility = Visibility.Collapsed;
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {

            //Удаление обращения
            MsAccess acs = new MsAccess();

            DataRowView dataRow = (DataRowView)DataGridRequests.SelectedItem;
            
            //Получаю номер заявки
            string ordNum = dataRow.Row.ItemArray[1].ToString();
            
            //Получаю дату заявки
            DateTime ordDt = (DateTime)dataRow.Row.ItemArray[2];

            //Формирую предупреждающее сообщение
            MessageBoxResult msg = MessageBox.Show(
                messageBoxText: $"Будет удалено обращение {ordNum} от {ordDt:MM.dd.yyyy}\nПродолжить?"
                , caption: "Внимание"
                , button: MessageBoxButton.YesNo);
            if (msg == MessageBoxResult.No) return;

            //Удаляю заявку из трех таблиц
            OleDbConnection connection = acs.CreateConnection();
            OleDbTransaction transaction = connection.BeginTransaction();
            OleDbCommand command = connection.CreateCommand();
            command.CommandText =
                $@"DELETE flw.* FROM ord INNER JOIN flw ON ord.ord_id = flw.ord_id WHERE ord.ord_num=""{ordNum}"";";
            command.Transaction = transaction;
            int ordDelRows = 0;
            
            //Удаляю из flw
            try
            {
                ordDelRows = command.ExecuteNonQuery();
            }
            catch (Exception)
            {

                MessageBox.Show("Не могу удалить обращение из таблицы flw");
                transaction.Rollback();
                return;
            }

            command.CommandText =
                $@"DELETE app.* FROM ord INNER JOIN app ON ord.ord_id = app.ord_id WHERE ord.ord_num=""{ordNum}"";";
            command.Transaction = transaction;
            
            //Удаляю из app
            try
            {
                ordDelRows = command.ExecuteNonQuery();
            }
            catch (Exception)
            {

                MessageBox.Show("Не могу удалить содержание обращения из таблицы app");
                transaction.Rollback();
                return;
            }

            command.CommandText =
                $@"DELETE ord.* FROM ord  WHERE ord.ord_num=""{ordNum}"";";
            command.Transaction = transaction;

            //Удаляю из ord
            try
            {
                ordDelRows = command.ExecuteNonQuery();
            }
            catch (Exception)
            {

                MessageBox.Show("Не могу удалить обращение из таблицы ord");
                transaction.Rollback();
                return;
            }

            //Подтверждаю транзакцию если все в порядке
            transaction.Commit();
            connection.Close();

            //Обновление DataGrid
            DataGridReqReload();

            //Подсчет кол-ва обращений
            CountRequestsMp();

            //Запись лога
            acs.Log(ordNum, "Удаление");

        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {

            if (ComboBoxMp.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите менедежара потока");
                return;
            }

            DataRowView dataRow = (DataRowView)DataGridRequests.SelectedItem;
            string ordNum;
            try
            {
                ordNum = dataRow.Row.ItemArray[1].ToString();
            }
            catch (NullReferenceException)
            {

                return;
            }

            string sql = $@"SELECT cor.cor_id FROM ord INNER JOIN (cor INNER JOIN app ON cor.cor_id = app.cor_id) ON ord.ord_id = app.ord_id WHERE (((ord.ord_num)='{ordNum}'));";
            MsAccess acs = new MsAccess();
            DataTable dt = acs.CreateDataTable(sql);
            //Преобразую DataTable в IList<int>
            IList<int> list = dt.AsEnumerable()
                .Select(r => r.Field<int>("cor_id"))
                .ToList();

            Order order = new Order
            {
                DateOrder = DateTime.Parse(ComboBoxDate.SelectedValue.ToString()),
                MngTn = ComboBoxMp.SelectedValue.ToString(),
                //Для расстановки true в checkboxes содержания заявки
                ContentOrders = list
            };
            order.ShowDialog();
            DataGridReqReload();
            CountRequestsMp();

            //Запись лога
            acs.Log(ordNum, "Копирование");

        }

        private void DataGridRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxMp.SelectedIndex == -1 )
            {
                ButtonCopy.Visibility = Visibility.Collapsed;
            }
            else
            {
                ButtonCopy.Visibility = Visibility.Visible;
            }
            
        }
    }
}
