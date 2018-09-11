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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Flow_management
{
    /// <summary>
    /// Логика взаимодействия для LogOrder.xaml
    /// </summary>
    public partial class Order : Window
    {

        private DateTime _dateOrder = DateTime.Now;
        private string _tn;
        private string _mngTn;
        private string _posNm;
        private bool _fltFirstClick = false;
        private string _filter;
        private IList<int> _contentOrders;

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

        public IList<int> ContentOrders
        {
            get => _contentOrders;
            set => _contentOrders = value;
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
                    IsReadOnly = true
                };
                tb.MouseLeave += new MouseEventHandler(TextBox_MouseOverFalse);
                tb.MouseEnter += new MouseEventHandler(TextBox_MouseOverTrue);

                ch.Tag = row[0].ToString();
                ch.Content = tb;
                ch.Width = 260;
                ch.Margin = new Thickness(0,2,0,2);
                ch.Checked += (sender, args) => { CheckBox_Checked(tb.Text); };
                ch.Unchecked += (sender, args) => { CheckBox_UnChecked(tb.Text); };
                //должно выполняться если переход на форму был сделан кнопкой Копировать
                try
                {
                    if (_contentOrders.IndexOf(int.Parse(row[0].ToString())) != -1) ch.IsChecked = true;
                }
                catch (NullReferenceException)
                {
                    // ignore                
                }

                //ChekBox добавляю в ListBox
                //ListBoxContent.Items.Insert(i, ch);
                StackPanelContent.Children.Add(ch);
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

        private void TextBox_MouseOverTrue(object sender, MouseEventArgs e)
        {
            ((TextBox) sender).Background = Brushes.LightGray; 
        }

        private void TextBox_MouseOverFalse(object sender, MouseEventArgs e)
        {
            ((TextBox) sender).ClearValue(TextBox.BackgroundProperty);
        }

        //Генерация DataGrid с сотрудниками
        private void GenerateDataGridStaff(DateTime dateOrder)
        {
            MsAccess acs = new MsAccess();

            string sql = $@"
                    SELECT nrm.stf_tn AS Номер
                    , stf.stf_fln AS ФИО
                    , pos.pos_nm AS Должность
                    , dep.dep_mn AS Отдел
                    , mde.mde_nm AS Время
                    , stf.stf_tb AS Банки
                    , COUNT(ord.ord_num) AS [Кол-во]
                    , Sum(IIf([cor].[cor_cnt]='Приоритетность для статистики',1,0)) AS Срочных 
                    , IIf(IsNull(Sum([cor].[cor_scr])) OR IsNull([nrm].[nrm_scr]) OR [nrm].[nrm_scr] = 0,0,Round(100*Sum([cor].[cor_scr])/[nrm].[nrm_scr],0)) AS Процент
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
                    ORDER BY IIf(IsNull(Sum([cor].[cor_scr])) OR IsNull([nrm].[nrm_scr]) OR [nrm].[nrm_scr] = 0,0,Round(100*Sum([cor].[cor_scr])/[nrm].[nrm_scr],0))
                    , dep.dep_mn
                    , stf.stf_fln;
                    ";
            DataTable dt = acs.CreateDataTable(sql);

            DataGridStaff.ItemsSource = dt.DefaultView;
            FilterBoxWith();
            //GetMaximumColumnWidth(DataGridStaff, 1);
        }

        //Изменение ширины texbox фильтров вместе с шириной столбцов DataGrid
        private void FilterBoxWith()
        {
            double dtGridWidth = DataGridStaff.Width;
            double wth = 0.0;
           IList<double> widthList = new List<double>()
           {
               2,
               1.5,
               1,
               0.7,
               2.5
           };

            StackPanelFlt.Width = dtGridWidth;
            StackPanelFlt.Margin = new Thickness(StackPanelContent.Width+60,5,20,5);

            for (int i = 0; i < 5; i++)
            {
                //wth = dtGridWidth * (widthList[i] / 10);
                wth = dtGridWidth * widthList[i]/10;
                DataGridStaff.Columns[i+1].Width = wth;
                ((TextBox)StackPanelFlt.Children[i]).Width = wth;
            }
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

        private void DgFilter(string filter)
        {
            DataTable dt = ((DataView)DataGridStaff.ItemsSource).ToTable();
            DataView dv = new DataView(dt);
            dv.RowFilter = filter;
            if (dv.Count == 0)
            {
                //MessageBox.Show("Не найдено ни одной строки!");
                return;
            } 
            DataGridStaff.ItemsSource = dv;
            FilterBoxWith();
        }

        private void DataGridStaff_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GetStaffName();
        }

        private void GetStaffName()
        {
            DataRowView dataRow = (DataRowView)DataGridStaff.SelectedItem;
            try
            {
                _tn = dataRow.Row.ItemArray[0].ToString();
            }
            catch (NullReferenceException)
            {

                return;
            }
            LabelWorker.Content = dataRow.Row.ItemArray[1].ToString();
            _posNm = dataRow.Row.ItemArray[2].ToString();
            if (dataRow.Row.ItemArray[10].ToString() == "0")
            {
                ButtonAdd.Content = "Норма 0";
                ButtonAdd.IsEnabled = false;
            }
            else
            {
                ButtonAdd.Content = "Назначить";
                ButtonAdd.IsEnabled = true;
            }
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

            string countOrd = acs.GetValueSql($@"SELECT flw.ord_id FROM ord INNER JOIN flw ON ord.ord_id = flw.ord_id WHERE ord.ord_num=""{ordNum}""");
            if (countOrd != "0")
            {
                MessageBox.Show("Указанный номер уже заведен в системе");
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
                foreach (var child in StackPanelContent.Children)
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

            //Запись лога
            acs.Log(ordNum, "Добавление");

            this.Close();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                DataGridStaff.Width = Width - 360;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {

            switch (this.WindowState)

            {
                case WindowState.Maximized:
                    //DataGridStaff.Height = SystemParameters.PrimaryScreenHeight - 100;
                    DataGridStaff.Width = SystemParameters.PrimaryScreenWidth - 360;
                    break;
                case WindowState.Normal:
                    //DataGridStaff.Height = SystemParameters.PrimaryScreenHeight - 100;
                    DataGridStaff.Width = this.Width - 360;
                    break;
            }

            FilterBoxWith();

        }

        private void DataGridStaff_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //Изменение формата колонки Дата в DataGridRequests
            if (e.PropertyName == "Номер")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            //Удаляю надпись номер, при получении фокуса
            if (TextBoxNumber.Text == "Номер") TextBoxNumber.Text = "";
        }

        private void TextBoxNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            //Возвращаю надпись номер, при потере фокуса
            if (TextBoxNumber.Text == "") TextBoxNumber.Text = "Номер";
        }

        private void StackPanelFlt_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!_fltFirstClick)
            {
                foreach (var child in ((StackPanel) sender).Children)
                {
                    ((TextBox) child).Text = "";
                }
                _fltFirstClick = true;
            }
        }

        private void StackPanelFlt_KeyUp(object sender, KeyEventArgs e)
        {
            IList<string> fieldsList = new List<string>()
            {
                "ФИО",
                "Должность",
                "Отдел",
                "Время",
                "Банки"
            };
            StackPanel sp = (StackPanel) sender;
            int countSpChield = sp.Children.Count;
            for (int i = 0; i < countSpChield; i++)
            {
                TextBox tb = (TextBox) sp.Children[i];
                if (tb.Text != "")
                {
                    string and;
                    and = string.IsNullOrEmpty(_filter) ? null : " AND ";
                    int cl = i + 1;
                    _filter += and + fieldsList[i] + " LIKE '%" + ((TextBox)sp.Children[i]).Text + "%'";
                }
            }
            DgFilter(_filter);
            LabelFlt.Visibility = Visibility.Visible;
            _filter = "";

        }

        private void LabelFlt_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LabelFlt.Visibility = Visibility.Collapsed;
            GenerateDataGridStaff(_dateOrder);
            _filter = "";
            _fltFirstClick = false;
            TextBoxFltFln.Text = "Фильтр по ФИО";
            TextBoxFltPos.Text = "должности";
            TextBoxFltDep.Text = "подразделению";
            TextBoxFltMode.Text = "времени";
            TextBoxFltBnk.Text = "банку";
        }

    }
}
