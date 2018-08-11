﻿using System;
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
                CreatePivotTable(StackPanelMp, arraySum, row);
            }

            CreatePivotTable(StackPanelMp, arraySum);

        }

        private void CreatePivotTable(StackPanel stackPanel, int[] arraySum, DataRow row = null)
        {
            ButtonAdd.Visibility = Visibility.Visible;

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
                        Button buttonAdd = (Button)this.FindName("ButtonAdd");
                        if (buttonAdd != null) buttonAdd.Visibility = Visibility.Hidden;
                    }
                    //else
                    //{
                    //    Button buttonAdd = (Button)this.FindName("ButtonAdd");
                    //    if (buttonAdd != null) buttonAdd.Visibility = Visibility.Visible;
                    //}
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
                    if (row != null) arraySum[j] += int.Parse(row[j].ToString());
                }
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
    }
}
