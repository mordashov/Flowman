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

            while  (StackPanelDep.Children.Count != 1)
            {
                StackPanelDep.Children.RemoveAt(StackPanelDep.Children.Count - 1);
            }

            MsAccess acs = new MsAccess();
            DataTable dt = new DataTable();
            string sql = $@"
                Select
                  dep.[dep_mn] As Отдел,
                  Sum(nrm.[nrm_scr]) As Норма,
                  Sum(cor.[cor_scr]) As Итого,
                  (Select
                    Count(flw1.[ord_id])
                  From
                    ((flw flw1 Inner Join
                    stf stf1 On stf1.[stf_tn] = flw1.[stf_tn]) Inner Join
                    dep dep1 On dep1.[dep_id] = stf1.[dep_id])
                  Where
                    dep1.[dep_id] = dep.[dep_id]) As [Кол-во],
                  Round(100 * Sum(cor.[cor_scr]) / Sum(nrm.[nrm_scr]), 0) As Выполнение
                From
                  ((((((flw Inner Join
                  ord On ord.[ord_id] = flw.[ord_id]) Inner Join
                  app On app.[ord_id] = ord.[ord_id]) Inner Join
                  cor On cor.[cor_id] = app.[cor_id]) Inner Join
                  stf On stf.[stf_tn] = flw.[stf_tn]) Inner Join
                  nrm On nrm.[stf_tn] = stf.[stf_tn]) Inner Join
                  dep On dep.[dep_id] = stf.[dep_id])
                Where
                  (ord.[ord_dt]) = {DatePickerNorm.SelectedDate:#M-d-yyyy#}
                Group By
                  dep.[dep_mn], dep.[dep_id]    
                ";
            dt = acs.CreateDataTable(sql);
            //Перебираю строки в DataTable
            int i = 0;
            int[] arraySum = new int[5];
            foreach (DataRow row in dt.Rows)
            {
                CreatePivotTable(StackPanelDep, 4, arraySum, row);
            }
            CreatePivotTable(StackPanelDep, 4, arraySum);
        }

        private void GenerateNormsMp()
        {

            while (StackPanelMp.Children.Count != 1)
            {
                StackPanelMp.Children.RemoveAt(StackPanelMp.Children.Count - 1);
            }

            MsAccess acs = new MsAccess();
            DataTable dt = new DataTable();
            string sql = $@"
                Select
                    mng.[mng_fln],
                    Sum(cor.[cor_scr]) As Итого,
                    (Select
                    Count(flw1.[ord_id])
                    From
                    flw flw1 Inner Join
                    mng mng1 On mng1.[mng_tn] = flw1.[mng_tn]
                    Where
                    flw1.[mng_tn] = mng.[mng_tn]) As [Кол-во]
                From
                    ((((flw Inner Join
                    ord On ord.[ord_id] = flw.[ord_id]) Inner Join
                    app On app.[ord_id] = ord.[ord_id]) Inner Join
                    cor On cor.[cor_id] = app.[cor_id]) Inner Join
                    mng On mng.[mng_tn] = flw.[mng_tn])
                Where
                    (ord.[ord_dt]) = {DatePickerNorm.SelectedDate:#M-d-yyyy#}
                Group By
                    mng.[mng_tn], mng.[mng_fln]
                ";
            dt = acs.CreateDataTable(sql);
            //Перебираю строки в DataTable
            int i = 0;
            int[] arraySum = new int[3];
            foreach (DataRow row in dt.Rows)
            {
                CreatePivotTable(StackPanelMp, 2, arraySum, row);
            }

            CreatePivotTable(StackPanelMp, 2, arraySum);

        }

        private void CreatePivotTable(StackPanel stackPanel, int numCols, int[] arraySum, DataRow row = null)
        {
            //if (row != null) stackPanel.Children.Clear();

            StackPanel stackPanelRow = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            for (int j = 0; j <= numCols; j++)
            {
                string txt;

                if (row == null)
                {
                    txt = j == 0 ? "Итого" : arraySum[j].ToString();
                }
                else
                {
                    txt = row[j].ToString();
                }

                double width = ((TextBlock) ((StackPanel) stackPanel.Children[0]).Children[j]).Width;

                TextBox tb = new TextBox()
                {
                    Margin = new Thickness(j == 0 ? 0 : 20, 0, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 16,
                    Width = width, //j == 0 ? 320 : 100,
                    Text = txt
                };
                stackPanelRow.Children.Add(tb);
                if (j != 0 & arraySum.Length > 0)
                {
                    if (row != null) arraySum[j] += int.Parse(row[j].ToString());
                }
            }
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
    }
}
