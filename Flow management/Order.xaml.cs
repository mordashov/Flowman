using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

        public DateTime DateOrder
        {
            get => _dateOrder;
            set => _dateOrder = value;
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
                    BorderThickness = new Thickness(0,0,0,1)
                };

                ch.Content = tb;
                ch.Width = 260;
                ch.Checked += (sender, args) => { CheckBox_Checked(tb.Text); };
                ch.Unchecked += (sender, args) => { CheckBox_UnChecked(tb.Text); };

                //ChekBox добавляю в ListBox
                ListBoxContent.Items.Insert(i, ch);

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

            string sql = $@"SELECT stf.stf_tn as Номер
                , stf.stf_fln as ФИО
                , pos.pos_nm as Должность
                , ROUND(100*Sum(cor.cor_scr)/nrm.nrm_scr,0) as Процент
                , Sum(cor.cor_scr) AS [Баллы]
                , nrm.nrm_scr as Норма
            FROM(stf INNER JOIN(pos INNER JOIN((ord INNER JOIN(cor INNER JOIN app ON cor.cor_id = app.cor_id) ON ord.ord_id = app.ord_id) INNER JOIN flw ON ord.ord_id = flw.ord_id) ON pos.pos_id = flw.pos_id) ON stf.stf_tn = flw.stf_tn) INNER JOIN nrm ON stf.stf_tn = nrm.stf_tn
            GROUP BY stf.stf_tn
                , stf.stf_fln
                , pos.pos_nm
                , nrm.nrm_scr
                , nrm.nrm_dt
                , ord.ord_dt
            HAVING nrm.nrm_dt = {dateOrder:#M-d-yyyy#} AND ord.ord_dt = {dateOrder:#M-d-yyyy#};
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
            string _tn = dataRow.Row.ItemArray[0].ToString();
            LabelWorker.Content = dataRow.Row.ItemArray[1].ToString();
        }

        private void DataGridStaff_KeyUp(object sender, KeyEventArgs e)
        {
            GetStaffName();
        }
    }
}
