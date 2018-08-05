using System;
using System.Collections.Generic;
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

        private void CreateSetDep()
        {
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
                  (ord.[ord_dt]) = 1
                Group By
                  dep.[dep_mn], dep.[dep_id]    
                ";
        }
    }
}
