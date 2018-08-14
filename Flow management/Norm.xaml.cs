using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для Norm.xaml
    /// </summary>
    public partial class Norm : Window
    {
        public Norm()
        {
            InitializeComponent();
        }

        private void GenerateNormsDep()
        {
            //Удаляю предыдущую генерацию StackPanel. Все кроме шапки
            while  (StackPanelDep.Children.Count != 1)
            {
                StackPanelDep.Children.RemoveAt(StackPanelDep.Children.Count - 1);
            }

            MsAccess acs = new MsAccess();
            DataTable dt = new DataTable();

            //{DatePickerNorm.SelectedDate:#M-d-yyyy#}
            string sql = $@"
              Select
              dep.[dep_mn] As Отдел,
              Sum(nrm.[nrm_scr]) As Норма,
              (Select
                Sum(cor2.[cor_scr]) As [Sum_cor_scr]
              From
                ((((stf stf2 Inner Join
                flw flw2 On flw2.[stf_tn] = stf2.[stf_tn]) Inner Join
                dep dep2 On dep2.[dep_id] = stf2.[dep_id]) Inner Join
                ord ord2 On ord2.[ord_id] = flw2.[ord_id]) Inner Join
                app app2 On ord2.[ord_id] = app2.[ord_id]) Inner Join
                cor cor2 On cor2.[cor_id] = app2.[cor_id]
              Where
                dep2.[dep_id] = dep.[dep_id] And
                ord2.[ord_dt] = {DatePickerNorm.SelectedDate:#M-d-yyyy#}) As Итого,
              (Select
                Count(flw1.[ord_id])
              From
                (((flw flw1 Inner Join
                stf stf1 On stf1.[stf_tn] = flw1.[stf_tn]) Inner Join
                dep dep1 On dep1.[dep_id] = stf1.[dep_id]) Inner Join
                ord ord1 On ord1.[ord_id] = flw1.[ord_id])
              Where
                dep1.[dep_id] = dep.[dep_id] And
                ord1.[ord_dt] = {DatePickerNorm.SelectedDate:#M-d-yyyy#}) As [Кол-во],
              Round(100 * (Select
                Sum(cor2.[cor_scr]) As [Sum_cor_scr]
              From
                ((((stf stf2 Inner Join
                flw flw2 On flw2.[stf_tn] = stf2.[stf_tn]) Inner Join
                dep dep2 On dep2.[dep_id] = stf2.[dep_id]) Inner Join
                ord ord2 On ord2.[ord_id] = flw2.[ord_id]) Inner Join
                app app2 On ord2.[ord_id] = app2.[ord_id]) Inner Join
                cor cor2 On cor2.[cor_id] = app2.[cor_id]
              Where
                dep2.[dep_id] = dep.[dep_id] And
                ord2.[ord_dt] = {DatePickerNorm.SelectedDate:#M-d-yyyy#}) / Sum(nrm.[nrm_scr]), 0) As Выполнение
            From
              ((((((stf Inner Join
              nrm On nrm.[stf_tn] = stf.[stf_tn]) Inner Join
              dep On dep.[dep_id] = stf.[dep_id])))))
            Where
              nrm.[nrm_dt] = {DatePickerNorm.SelectedDate:#M-d-yyyy#}
            Group By
              dep.[dep_mn], nrm.[nrm_dt], dep.[dep_id]
                ";
            dt = acs.CreateDataTable(sql);
            //Перебираю строки в DataTable
            int i = 0;
            int[] arraySum = new int[5];
            foreach (DataRow row in dt.Rows)
            {
                CreatePivotTable(StackPanelDep, arraySum, row);
            }
            CreatePivotTable(StackPanelDep, arraySum);
        }

        private void GenerateNormsMp()
        {
            
            //Удаляю предыдущую генерацию StackPanel. Все кроме шапки
            while (StackPanelMp.Children.Count != 1)
            {
                StackPanelMp.Children.RemoveAt(StackPanelMp.Children.Count - 1);
            }

            MsAccess acs = new MsAccess();
            DataTable dt = new DataTable();
            string sql = $@"
            Select
                mng.[mng_fln],
                (Select
                Sum(cor.[cor_scr]) As Итого
                From
                ((flw Inner Join
                ord On ord.[ord_id] = flw.[ord_id]) Inner Join
                app On app.[ord_id] = ord.[ord_id]) Inner Join
                cor On cor.[cor_id] = app.[cor_id]
                Where
                (ord.[ord_dt]) = {DatePickerNorm.SelectedDate:#M-d-yyyy#} And
                flw.[mng_tn] = mng.[mng_tn]) As Итого,
                (Select
                Count(ord.[ord_id]) As Итого
                From
                flw Inner Join
                ord On ord.[ord_id] = flw.[ord_id]
                Where
                (ord.[ord_dt]) = {DatePickerNorm.SelectedDate:#M-d-yyyy#} And
                flw.[mng_tn] = mng.[mng_tn]) As [Кол-во]
            From
                mng";
            dt = acs.CreateDataTable(sql);
            //Перебираю строки в DataTable
            int i = 0;
            int[] arraySum = new int[3];
            foreach (DataRow row in dt.Rows)
            {
                CreatePivotTable(StackPanelMp, arraySum, row);
            }

            CreatePivotTable(StackPanelMp, arraySum);

        }

        private void CreatePivotTable(StackPanel stackPanel, int[] arraySum, DataRow row = null)
        {
            if (stackPanel.Name == "StackPanelDep") ButtonAdd.Visibility = Visibility.Visible;

            StackPanel stackPanelRow = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 0)
            };
            //Кол-во колонок определенных в шапке таблице в xaml
            int numCols = ((StackPanel) stackPanel.Children[0]).Children.Count;

            //Перебор колонок
            for (int j = 0; j < numCols; j++)
            {
                string txt;
                FontWeight fontWeight;

                //Если в вызове нет DataRow то выводим сумму и делаем ее жирной
                if (row == null)
                {
                    txt = j == 0 ? "Итого" : arraySum[j].ToString();
                    fontWeight = FontWeight.FromOpenTypeWeight(700);
                    if (j == 1 & arraySum[j].ToString() != "0")
                    {
                        Button buttonAdd = (Button) this.FindName("ButtonAdd");
                        if (buttonAdd != null) buttonAdd.Visibility = Visibility.Hidden;
                    }
                }
                else //Иначе выводим значение DataTable
                {
                    txt = row[j].ToString();
                    fontWeight = FontWeight.FromOpenTypeWeight(1);
                }

                //Получаем ширину колонки из шапке определенной в xaml
                double width = ((TextBlock) ((StackPanel) stackPanel.Children[0]).Children[j]).Width;

                //Формируем TextBlock, который потом вложим в StackPanel
                TextBox tb = new TextBox()
                {
                    Margin = new Thickness(j == 0 ? 0 : 2, 0, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 16,
                    Width = width,
                    FontWeight = fontWeight,
                    Text = txt
                };
                //Формируем StackPanel строку
                stackPanelRow.Children.Add(tb);

                //Суммирую значения колонок, чтобы потом вывести в итоговом StackPanel
                if (j != 0 & arraySum.Length > 0)
                {
                    string itemSum = "0";
                    if (row != null) itemSum = row[j].ToString();
                    if (string.IsNullOrEmpty(itemSum)) itemSum = "0";
                    arraySum[j] += int.Parse(itemSum);
                }
            }

            //Добавляю кнопку редактирование в своднуб таблицу по нормам
            if (stackPanel.Name == "StackPanelDep")
            {
                Image ico = new Image()
                {
                    Source = (ImageSource)Resources["Edit"],
                };
                Button editButton = new Button()
                {
                    Margin = new Thickness(2, 0, 0, 0),
                    Width = 50,
                    Content = ico
                };
                stackPanelRow.Children.Add(editButton);
            }

            //Добавляю все что нагенерил в родительскую StackPanel
            stackPanel.Children.Add(stackPanelRow);
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DatePickerNorm.SelectedDate = DateTime.Now;
            GenerateNormsDep();
            GenerateNormsMp();
        }

        private void DatePickerNorm_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GenerateNormsDep();
            GenerateNormsMp();
        }

        private void DatePickerNorm_CalendarClosed(object sender, RoutedEventArgs e)
        {
            GenerateNormsDep();
            GenerateNormsMp();
        }

        private void DatePickerNorm_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateNormsDep();
            GenerateNormsMp();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {

            MsAccess acs = new MsAccess();
            string result = "0";
            string sql = $@"
                Select
                  nrm.nrm_id
                From
                  nrm
                Where
                  nrm.nrm_dt = {DatePickerNorm.SelectedDate:#M-d-yyyy#}
                ";
            result = acs.GetValueSql(sql);
            if (result != "0")
            {
                MessageBox.Show ($@"Данные на {DatePickerNorm.SelectedDate:#M-d-yyyy#} уже есть в системе");
                return;
            }
            else
            {

                sql =
                    $@"INSERT INTO nrm (nrm_dt, stf_tn, nrm_hr, nrm_scr) 
                    SELECT {DatePickerNorm.SelectedDate:#M-d-yyyy#}, stf_tn, nrm_hr, nrm_scr 
                    FROM nrm WHERE nrm_dt = (SELECT MAX(nrm_dt) FROM nrm WHERE nrm_dt <= {
                            DatePickerNorm.SelectedDate
                        :#M-d-yyyy#})";
                acs.GetValueSql(sql);
                GenerateNormsDep();
            }
        }
    }
}
