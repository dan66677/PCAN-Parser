using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WpfApp2.Models;
using WpfApp2.Services;

namespace WpfApp2
{
    public partial class ExcelWindow : Window
    {
        public ExcelWindow()
        {
            InitializeComponent();
            LoadConfigData();
        }

        private void LoadConfigData()
        {
            try
            {
                string filePath = @"C:\data\config.txt";

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл конфигурации не найден!");
                    return;
                }

                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                var data = new List<ConfigItem>();

                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length >= 13)
                    {
                        var item = new ConfigItem
                        {
                            Name = parts[0],
                            Id = parts[1],
                            Multiplier = parts[2],
                            Divider = parts[3],
                            Indent = parts[4],
                            Byte0 = parts[5],
                            Byte1 = parts[6],
                            Byte2 = parts[7],
                            Byte3 = parts[8],
                            Byte4 = parts[9],
                            Byte5 = parts[10],
                            Byte6 = parts[11],
                            Byte7 = parts[12]
                        };
                        data.Add(item);
                    }
                }

                configDataGrid.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }


        private void LoadData()
        {
            try
            {
                string filePath = @"C:\data\config.txt";

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл конфигурации не найден!");
                    return;
                }

                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                var data = new List<ConfigItem>();

                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length >= 13)
                    {
                        var item = new ConfigItem
                        {
                            Name = parts[0],
                            Id = parts[1],
                            Multiplier = parts[2],
                            Divider = parts[3],
                            Indent = parts[4],
                            Byte0 = parts[5],
                            Byte1 = parts[6],
                            Byte2 = parts[7],
                            Byte3 = parts[8],
                            Byte4 = parts[9],
                            Byte5 = parts[10],
                            Byte6 = parts[11],
                            Byte7 = parts[12]
                        };
                        data.Add(item);
                    }
                }

                configDataGrid.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void ExelButton_Click(object sender, EventArgs e)
        {

        }



    }
    

    public class ConfigItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Multiplier { get; set; }
        public string Divider { get; set; }
        public string Indent { get; set; }
        public string Byte0 { get; set; }
        public string Byte1 { get; set; }
        public string Byte2 { get; set; }
        public string Byte3 { get; set; }
        public string Byte4 { get; set; }
        public string Byte5 { get; set; }
        public string Byte6 { get; set; }
        public string Byte7 { get; set; }
    }
}