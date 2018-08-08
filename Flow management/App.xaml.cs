using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Flow_management
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool showNorm = false;
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/norms")
                {
                    showNorm = true;
                }
            }

           
            if (showNorm)
            {
                Norm normWindow = new Norm();
                normWindow.Show();
            }
            else
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            
        }
    }
}
