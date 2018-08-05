using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
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

namespace Flow_management
{
    /// <summary>
    /// Логика взаимодействия для Order.xaml
    /// </summary>
    public partial class Order : Window
    {

        private DateTime _dateOrder = DateTime.Now;
        private string _tn;
        private string _mngTn;
        private string _posNm;

        public DateTime DateOrder
        {
            get => _dateOrder;
            set => _dateOrder = value;
        }

        public string MngTn
        {
            get => _mngTn;
            set => _mngTn = value;
        }

        public Order()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            DatePickerDate.SelectedDate = _dateOrder;
            GenerateListBoxContent();
            GenerateDataGridStaff(_dateOrder);
        }

        private void GenerateListBoxContent()
        {

            MsAccess acs = new MsAccess();
            //Формирую DataTable с содержанием обращения
            string sql = "SELECT cor.cor_id, cor.cor_cnt & ' (' & cor.cor_scr & ')' FROM cor";
            DataTable dt = new DataTable();
            dt = acs.CreateDataTable(sql);

            //List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();

            //Перебираю строки в DataTable
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {

                //Чекбоксы выбора содержания обращения
                CheckBox ch = new CheckBox();

                //TextBox Вложенный в ChekBox
                TextBox tb = new TextBox
                {
                    Text = row[1].ToString(),
                    TextWrapping = TextWrapping.Wrap,
                    BorderThickness = new Thickness(0,0,0,0),
                };

                ch.Tag = row[0].ToString();
                ch.Content = tb;
                ch.Width = 260;
                ch.Checked += (sender, args) => { CheckBox_Checked(tb.Text); };
                ch.Unchecked += (sender, args) => { CheckBox_UnChecked(tb.Text); };

                //ChekBox добавляю в ListBox
                ListBoxContent.Items.Insert(i, ch);

                //ListBoxContent.ItemTemplate.Template = new Thickness(0,0,0,1);

                i++;
            }

            //List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>
            //{
            //    new KeyValuePair<int, string>(0, "Первый"),
            //    new KeyValuePair<int, string>(1, "Второй"),
            //    new KeyValuePair<int, string>(2, "Третий")
            //};

        }

        //Генерация DataGrid с сотрудниками
        private void GenerateDataGridStaff(DateTime dateOrder)
        {
            MsAccess acs = new MsAccess();

            //string sql = $@"
            //        SELECT 
            //            nrm.stf_tn AS Номер
            //            , stf.stf_fln AS ФИО
            //            , pos.pos_nm AS Должность
            //            , dep.dep_mn AS Отдел
            //            , IIf(IsNull(Sum([cor].[cor_scr])),0,Round(100*Sum([cor].[cor_scr])/[nrm].[nrm_scr],0)) AS Процент
            //            , Sum(cor.cor_scr) AS Баллы, nrm.nrm_scr AS Норма
            //        FROM (pos INNER JOIN (dep INNER JOIN stf ON dep.dep_id = stf.dep_id) ON pos.pos_id = stf.pos_id) INNER JOIN ((ord LEFT JOIN (cor RIGHT JOIN app ON cor.cor_id = app.cor_id) ON ord.ord_id = app.ord_id) RIGHT JOIN (nrm LEFT JOIN flw ON nrm.stf_tn = flw.stf_tn) ON ord.ord_id = flw.ord_id) ON stf.stf_tn = nrm.stf_tn
            //        GROUP BY 
            //            nrm.stf_tn
            //            , stf.stf_fln
            //            , pos.pos_nm
            //            , dep.dep_mn
            //            , nrm.nrm_scr
            //            , nrm.nrm_dt
            //            , ord.ord_dt
            //            , dep.dep_mn
            //            , stf.stf_fln
            //        HAVING nrm.nrm_dt = {dateOrder:#M-d-yyyy#} AND (ord.ord_dt = {dateOrder:#M-d-yyyy#} OR ord.ord_dt IS NULL)
            //        ORDER BY 
            //            IIf(IsNull(Sum([cor].[cor_scr])),0,Round(100*Sum([cor].[cor_scr])/[nrm].[nrm_scr],0))
            //            , dep.dep_mn
            //            , stf.stf_fln
            //        ";

            string sql = $@"
                    SELECT nrm.stf_tn AS Номер
                    , stf.stf_fln AS ФИО
                    , pos.pos_nm AS Должность
                    , dep.dep_mn AS Отдел, mde.mde_nm AS Время
                    , stf.stf_tb AS Банки
                    , COUNT(ord.ord_num) AS [Кол-во]
                    , IIf(IsNull(Sum([cor].[cor_scr])),0,Round(100*Sum([cor].[cor_scr])/[nrm].[nrm_scr],0)) AS Процент
                    , Sum(cor.cor_scr) AS Баллы, nrm.nrm_scr AS Норма
                    FROM mde INNER JOIN ((pos INNER JOIN (dep INNER JOIN stf ON dep.dep_id = stf.dep_id) ON pos.pos_id = stf.pos_id) INNER JOIN ((ord LEFT JOIN (cor RIGHT JOIN app ON cor.cor_id = app.cor_id) ON ord.ord_id = app.ord_id) RIGHT JOIN (nrm LEFT JOIN flw ON nrm.stf_tn = flw.stf_tn) ON ord.ord_id = flw.ord_id) ON stf.stf_tn = nrm.stf_tn) ON mde.mde_id = stf.mde_id
                    GROUP BY nrm.stf_tn
                    , stf.stf_fln
                    , pos.pos_nm
                    , dep.dep_mn
                    , mde.mde_nm
                    , stf.stf_tb
                    , nrm.nrm_scr
                    , nrm.nrm_dt
                    , ord.ord_dt
                    , dep.dep_mn
                    , stf.stf_fln
                    HAVING (((nrm.nrm_dt)={dateOrder:#M-d-yyyy#}) AND ((ord.ord_dt)={dateOrder:#M-d-yyyy#} Or (ord.ord_dt) Is Null))
                    ORDER BY IIf(IsNull(Sum([cor].[cor_scr])),0,Round(100*Sum([cor].[cor_scr])/[nrm].[nrm_scr],0))
                    , dep.dep_mn
                    , stf.stf_fln;
                    ";
            DataTable dt = acs.CreateDataTable(sql);

            DataGridStaff.ItemsSource = dt.DefaultView;
        }

        //Действие при выборе CheckBox
        private void CheckBox_Checked(string chContent)
        {
            int score = int.Parse(LabelScore.Content.ToString());
            //Кол-во баллов по обращению (вычленияю баллы которые в конце наименования в скобках)
            chContent = chContent.Substring(chContent.LastIndexOf('(') + 1, chContent.Length - chContent.LastIndexOf('(') - 2);
            score = score + int.Parse(chContent);
            LabelScore.Content = score.ToString();
        }

        //Действие при снятии галки с CheckBox
        private void CheckBox_UnChecked(string chContent)
        {
            int score = int.Parse(LabelScore.Content.ToString());
            //Кол-во баллов по обращению (вычленияю баллы которые в конце наименования в скобках)
            chContent = chContent.Substring(chContent.LastIndexOf('(') + 1, chContent.Length - chContent.LastIndexOf('(') - 2);
            score = score - int.Parse(chContent);
            LabelScore.Content = score.ToString();
        }

        private void DataGridStaff_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GetStaffName();
        }

        private void GetStaffName()
        {
            DataRowView dataRow = (DataRowView)DataGridStaff.SelectedItem;
            _tn = dataRow.Row.ItemArray[0].ToString();
            LabelWorker.Content = dataRow.Row.ItemArray[1].ToString();
            _posNm = dataRow.Row.ItemArray[2].ToString();
        }

        private void DataGridStaff_KeyUp(object sender, KeyEventArgs e)
        {
            GetStaffName();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            MsAccess acs = new MsAccess();

            string ordNum = TextBoxNumber.Text;
            if (string.IsNullOrEmpty(ordNum) || ordNum == "Номер")
            {
                MessageBox.Show("Неверно указан номер");
                return;
            }

            string worker = LabelWorker.Content.ToString();
            if (string.IsNullOrEmpty(worker) || worker == "Сотрудник")
            {
                MessageBox.Show("Неверно указан сотрудник");
                return;
            }

            DateTime dt = (DateTime) DatePickerDate.SelectedDate;

            OleDbConnection connection = acs.CreateConnection();
            OleDbTransaction transaction = connection.BeginTransaction();
            OleDbCommand command = connection.CreateCommand();
            command.CommandText =
                $@"INSERT INTO ord (ord_num, ord_dt) VALUES ('{ordNum}', {dt:#M-d-yyyy#} )";
            command.Transaction = transaction;
            int ordInsRows = 0;

            try
            {
                ordInsRows = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка вставки данных. Проверьте данные и повторите попытку.");
                return;
            }

            int insFlwRows = 0;
            if (ordInsRows == 1)
            {
                int appInsRows = 0;
                foreach (var child in ListBoxContent.Items)
                {
                    if (child is CheckBox)
                    {
                        if (((CheckBox) child).IsChecked == true)
                        {
                            string corId = ((CheckBox)child).Tag.ToString();
                            command.CommandText =
                                $@"INSERT INTO app (ord_id, cor_id) SELECT ord_id, '{corId}' FROM ord WHERE ord_num = '{ordNum}';";
                            command.Transaction = transaction;
                            appInsRows = command.ExecuteNonQuery();
                        }

                    }
                }
                if (appInsRows == 0)
                {
                    transaction.Rollback();
                    connection.Close();
                    MessageBox.Show("Выберите содержание заявки");
                    return;
                }


                command.CommandText =
                    $@"INSERT INTO flw (mng_tn, ord_id, stf_tn, pos_id) 
                        SELECT {_mngTn}
                            , ord_id
                            , {_tn}
                            , (SELECT pos.pos_id FROM pos WHERE pos.pos_nm='{_posNm}') as pos
                        FROM ord 
                        WHERE ord_num = '{ordNum}';";
                command.Transaction = transaction;
                insFlwRows = command.ExecuteNonQuery();

            }

            if (insFlwRows == 0)
            {
                transaction.Rollback();
                connection.Close();
                MessageBox.Show("Не могу привязть менеджера потока к обращению");
                return;
            }


            transaction.Commit();
            connection.Close();
            this.Close();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                DataGridStaff.Width = Width - 340;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)

            {
                case WindowState.Maximized:
                    //DataGridStaff.Height = SystemParameters.PrimaryScreenHeight - 100;
                    DataGridStaff.Width = SystemParameters.PrimaryScreenWidth - 360;
                    break;
            }
        }
    }
}
