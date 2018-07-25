using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для Order.xaml
    /// </summary>
    public partial class Order : Window
    {
        public Order()
        {
            InitializeComponent();
            GenerateListBoxContent();
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
                    TextWrapping = TextWrapping.Wrap
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
        //Действие при выборе CheckBox
        private void CheckBox_Checked(string chContent)
        {
            int score = int.Parse(LabelScore.Content.ToString());
            chContent = chContent.Substring(chContent.LastIndexOf('(') + 1, chContent.Length - chContent.LastIndexOf('(') - 2);
            score = score + int.Parse(chContent);
            LabelScore.Content = score.ToString();
        }

        //Действие при снятии галки с ChekBox
        private void CheckBox_UnChecked(string chContent)
        {
            int score = int.Parse(LabelScore.Content.ToString());
            chContent = chContent.Substring(chContent.LastIndexOf('(') + 1, chContent.Length - chContent.LastIndexOf('(') - 2);
            score = score - int.Parse(chContent);
            LabelScore.Content = score.ToString();
        }

    }
}
