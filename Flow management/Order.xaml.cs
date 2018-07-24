using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }

        private void GenerateListBoxContent()
        {

        CheckBox ch = new CheckBox()
            {
                Content = "Первый",
            };
            CheckBox ch1 = new CheckBox()
            {
                Content = "Второй"
            };
            CheckBox ch2 = new CheckBox()
            {
                Content = "Третий"
            };
            ListBoxContent.Items.Insert(0, ch);
            ListBoxContent.Items.Insert(1, ch1);
            ListBoxContent.Items.Insert(2, ch2);
        }

}
}
