using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using WpfApp2.Models;
using WpfApp2.ViewModels;
using WpfApp2.Views;

namespace WpfApp2
{
    public partial class ExcelWindow : Window
    {
        private List<CanMessage> _canMessages = new List<CanMessage>();
        private List<ConfigItem> _configItems = new List<ConfigItem>();
        private string _currentExcelFilePath = string.Empty;

        public ExcelWindow()
        {
            InitializeComponent();
            LoadConfigData();
            LoadCanMessages();
            UpdateSelectedFileDisplay();
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
                _configItems = new List<ConfigItem>();

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
                        _configItems.Add(item);
                    }
                }

                configDataGrid.ItemsSource = _configItems;
                ConfigComboBox.ItemsSource = _configItems;
                StatusTextBlock.Text = $"Загружено {_configItems.Count} конфигураций";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadCanMessages()
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                var viewModel = mainWindow.DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _canMessages = new List<CanMessage>(viewModel.Messages);
                    StatusTextBlock.Text = $"Загружено {_canMessages.Count} CAN сообщений";
                }
            }
        }

        private void SelectExcelFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Выберите файл Excel"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _currentExcelFilePath = openFileDialog.FileName;
                UpdateSelectedFileDisplay();
                StatusTextBlock.Text = $"Выбран файл: {Path.GetFileName(_currentExcelFilePath)}";
            }
        }

        private void UpdateSelectedFileDisplay()
        {
            if (!string.IsNullOrEmpty(_currentExcelFilePath))
            {
                SelectedFileTextBlock.Text = $"Текущий файл: {_currentExcelFilePath}";
            }
            else
            {
                SelectedFileTextBlock.Text = "Файл не выбран";
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentExcelFilePath))
            {
                MessageBox.Show("Сначала выберите или создайте файл Excel");
                return;
            }

            if (ConfigComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите конфигурацию для экспорта");
                return;
            }

            var selectedConfig = ConfigComboBox.SelectedItem as ConfigItem;
            if (selectedConfig == null) return;

            try
            {
                ExportToExcel(selectedConfig, _currentExcelFilePath);
                MessageBox.Show($"Данные '{selectedConfig.Name}' добавлены в Excel файл", "Экспорт завершен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка");
            }
        }

        private void ExportToExcel(ConfigItem config, string filePath)
        {
            // Получаем отфильтрованные сообщения
            var filteredMessages = GetFilteredMessages(config);

            if (filteredMessages.Count == 0)
            {
                throw new Exception($"Нет сообщений с ID {config.Id} (конфигурация: {config.Name})");
            }

            // Работа с Excel файлом
            using (var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook())
            {
                // Получаем или создаем лист
                IXLWorksheet worksheet = workbook.Worksheets.Count > 0 ?
                    workbook.Worksheet(1) :
                    workbook.Worksheets.Add("CAN Data");

                // Определяем последнюю использованную колонку + 1 (чтобы оставить пустой промежуток)
                int newCol = (worksheet.LastColumnUsed()?.ColumnNumber() + 2) ?? 1;

                // Записываем заголовки
                worksheet.Cell(1, newCol).Value = $"Time ({config.Name})";
                worksheet.Cell(1, newCol + 1).Value = $"Value ({config.Name})";

                // Записываем данные
                for (int i = 0; i < filteredMessages.Count; i++)
                {
                    var message = filteredMessages[i];
                    double value = CalculateValue(message, config);

                    worksheet.Cell(i + 2, newCol).Value = message.TimeOffsetMs;
                    worksheet.Cell(i + 2, newCol + 1).Value = value;
                }

                // Форматирование
                worksheet.Columns().AdjustToContents();
                worksheet.Column(newCol + 1).Style.NumberFormat.Format = "0.000";

                // Сохраняем файл
                workbook.SaveAs(filePath);
            }

            StatusTextBlock.Text = $"Добавлен параметр '{config.Name}' ({filteredMessages.Count} значений)";
        }

        private List<CanMessage> GetFilteredMessages(ConfigItem config)
        {
            uint configId = Convert.ToUInt32(config.Id, 16);
            return _canMessages
                .Where(m => m.Id == configId)
                .OrderBy(m => m.TimeOffsetMs)
                .ToList();
        }

        private double CalculateValue(CanMessage message, ConfigItem config)
        {
            byte[] byteMasks = new byte[8];
            byteMasks[0] = Convert.ToByte(config.Byte0, 16);
            byteMasks[1] = Convert.ToByte(config.Byte1, 16);
            byteMasks[2] = Convert.ToByte(config.Byte2, 16);
            byteMasks[3] = Convert.ToByte(config.Byte3, 16);
            byteMasks[4] = Convert.ToByte(config.Byte4, 16);
            byteMasks[5] = Convert.ToByte(config.Byte5, 16);
            byteMasks[6] = Convert.ToByte(config.Byte6, 16);
            byteMasks[7] = Convert.ToByte(config.Byte7, 16);

            double value = 0;
            int bitPosition = 0;

            for (int byteIndex = 0; byteIndex < message.Data.Length && byteIndex < 8; byteIndex++)
            {
                byte messageByte = message.Data[byteIndex];
                byte maskByte = byteMasks[byteIndex];

                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {
                    if ((maskByte & (1 << bitIndex)) != 0)
                    {
                        bool bitValue = (messageByte & (1 << bitIndex)) != 0;
                        if (bitValue)
                        {
                            value += Math.Pow(2, bitPosition);
                        }
                        bitPosition++;
                    }
                }
            }

            double multiplier = string.IsNullOrEmpty(config.Multiplier) ? 1 : double.Parse(config.Multiplier);
            double divider = string.IsNullOrEmpty(config.Divider) ? 1 : double.Parse(config.Divider);
            double indent = string.IsNullOrEmpty(config.Indent) ? 0 : double.Parse(config.Indent);

            return (value * multiplier / divider) + indent;
        }

        private void CreateNewFileButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Создать новый файл Excel",
                FileName = "CAN_Data_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _currentExcelFilePath = saveFileDialog.FileName;

                // Создаем новый файл
                using (var workbook = new XLWorkbook())
                {
                    workbook.AddWorksheet("CAN Data");
                    workbook.SaveAs(_currentExcelFilePath);
                }

                UpdateSelectedFileDisplay();
                StatusTextBlock.Text = $"Создан новый файл: {Path.GetFileName(_currentExcelFilePath)}";
                MessageBox.Show("Новый файл создан: " + _currentExcelFilePath);
            }
        }

        private void LoadConfigDataFromExcel(string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    _configItems = new List<ConfigItem>();

                    var rows = worksheet.RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        var item = new ConfigItem
                        {
                            Name = row.Cell(1).GetString(),
                            Id = row.Cell(2).GetString(),
                            Multiplier = row.Cell(3).GetString(),
                            Divider = row.Cell(4).GetString(),
                            Indent = row.Cell(5).GetString(),
                            Byte0 = row.Cell(6).GetString(),
                            Byte1 = row.Cell(7).GetString(),
                            Byte2 = row.Cell(8).GetString(),
                            Byte3 = row.Cell(9).GetString(),
                            Byte4 = row.Cell(10).GetString(),
                            Byte5 = row.Cell(11).GetString(),
                            Byte6 = row.Cell(12).GetString(),
                            Byte7 = row.Cell(13).GetString()
                        };
                        _configItems.Add(item);
                    }
                }

                configDataGrid.ItemsSource = _configItems;
                ConfigComboBox.ItemsSource = _configItems;
                StatusTextBlock.Text = $"Загружено {_configItems.Count} конфигураций";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных из Excel: {ex.Message}");
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
}