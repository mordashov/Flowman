using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Flow_management
{

    class MsAccess
    {
        private string _pathToBase = null;
        private string _connectionString = null;


        public string PathToBase
        {
            get => _pathToBase;
            set => _pathToBase = value;
        }

        public string ConnectionString
        {
            get => _connectionString;
            set => _connectionString = value;
        }

        //Проверка наличия файла
        private void CheckPathToBase(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("Файл " + path + " не найден!\nПрограмма будет закрыта");
                Environment.Exit(0);
            }
        }

        //Получение соединения с базой данных
        public OleDbConnection CreateConnection()
        {
            //Проверка наличия файла настроек
            string currentPath = Environment.CurrentDirectory + "\\settings.txt";
            CheckPathToBase(currentPath);

            //Проверка наличия файла базы данных
            var line = File.ReadLines(currentPath).Take(1);
            _pathToBase = line.First();
            CheckPathToBase(_pathToBase);

            //Подключение к базе данных
            OleDbConnection connection = new OleDbConnection();
            _connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _pathToBase;

            //_connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\Dropbox\SrOrder\ord.accdb";

            connection.ConnectionString = _connectionString;

            //Проверяю подключение
            try
            {
                connection.Open();
                return connection;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Не могу подключиться к базе данных " + _pathToBase + "\n\n" + ex.ToString());
                Environment.Exit(0);
                return null;
            }
        }

        //Получаю DataTable
        public DataTable CreateDataTable(string sql)
        {

            DataTable dataTable = new DataTable();

            OleDbConnection connection = CreateConnection();
            OleDbCommand command = new OleDbCommand(sql, connection);
            OleDbDataAdapter adapter = new OleDbDataAdapter(command);
            adapter.Fill(dataTable);
            connection.Close();

            return dataTable;
        }

        //Получаю единственное значение
        public string GetValueSql(string sql)
        {
            OleDbConnection connection = CreateConnection();
            OleDbCommand command = new OleDbCommand(sql, connection);
            string valueSql = "0";
            try
            {
                valueSql = command.ExecuteScalar().ToString();
            }
            catch (NullReferenceException)
            {
                valueSql = "0";
            }

            connection.Close();

            return valueSql;
        }

        //Получаю единственное значение
        public string GetValueCommand(OleDbCommand command)
        {
            OleDbConnection connection = CreateConnection();
            command.Connection = connection;
            string valueSql = command.ExecuteScalar().ToString();

            connection.Close();

            return valueSql;
        }
    }
}
