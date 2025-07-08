using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp2
{
    public partial class ConfigWindow : Window
    {
        private List<Parameter> parameters = new List<Parameter>();
        private int currentParameterIndex = -1;

        public ConfigWindow()
        {
            InitializeComponent();
            AddNewParameter();
        }

        public class Parameter
        {
            public string Name { get; set; } = "";
            public string Id { get; set; } = "";
            public string Multiplier { get; set; } = "";
            public string Divider { get; set; } = "";
            public string Indent { get; set; } = "";
            public string[] Bytes { get; set; } = new string[8] { "00", "00", "00", "00", "00", "00", "00", "00" };
        }

        private void AddNewParameter()
        {
            parameters.Add(new Parameter());
            currentParameterIndex = parameters.Count - 1;
            UpdateParameterList();
            LoadCurrentParameter();
        }

        private void UpdateParameterList()
        {
            ParameterList.Items.Clear();
            foreach (var param in parameters)
            {
                ParameterList.Items.Add($"{param.Name} ({param.Id})");
            }
            ParameterList.SelectedIndex = currentParameterIndex;
        }

        private void LoadCurrentParameter()
        {
            if (currentParameterIndex >= 0 && currentParameterIndex < parameters.Count)
            {
                var param = parameters[currentParameterIndex];
                NameEntry.Text = param.Name;
                IdEntry.Text = param.Id;
                Multiplier.Text = param.Multiplier;
                Divider.Text = param.Divider;
                Indent.Text = param.Indent;

                for (int i = 0; i < 8; i++)
                {
                    var byteTextBox = FindTextBoxByName($"Byte{i + 1}Value");
                    if (byteTextBox != null)
                    {
                        byteTextBox.Text = param.Bytes[i];
                  
                        UpdateCheckBoxesFromByteValue($"Byte{i + 1}", param.Bytes[i]);
                    }
                }
            }
        }

        private void SaveCurrentParameter()
        {
            if (currentParameterIndex >= 0 && currentParameterIndex < parameters.Count)
            {
                var param = parameters[currentParameterIndex];
                param.Name = NameEntry.Text;
                param.Id = IdEntry.Text;
                param.Multiplier = Multiplier.Text;
                param.Divider = Divider.Text;
                param.Indent = Indent.Text;

                for (int i = 0; i < 8; i++)
                {
                    var byteTextBox = FindTextBoxByName($"Byte{i + 1}Value");
                    param.Bytes[i] = string.IsNullOrEmpty(byteTextBox?.Text) ? "00" : byteTextBox.Text.PadLeft(2, '0');
                }

                UpdateParameterList();
            }
        }

        private TextBox FindTextBoxByName(string name)
        {
            return (TextBox)FindName(name);
        }

        private CheckBox FindCheckBoxByTag(string tag)
        {
            return FindVisualChildren<CheckBox>(this).FirstOrDefault(cb => cb.Tag?.ToString() == tag);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void UpdateByteValue(string byteName)
        {
            byte value = 0;

            for (int bit = 0; bit < 8; bit++)
            {
                string checkBoxName = $"{byteName}Bit{bit}"; // Изменили с (7 - bit) на bit
                CheckBox checkBox = FindCheckBoxByTag(checkBoxName);

                if (checkBox != null && checkBox.IsChecked == true)
                {
                    value |= (byte)(1 << bit);
                }
            }

            TextBox byteTextBox = FindTextBoxByName($"{byteName}Value");
            if (byteTextBox != null)
            {
                byteTextBox.Text = value.ToString("X2");
            }
        }

        private void UpdateCheckBoxesFromByteValue(string byteName, string hexValue)
        {
            try
            {
                byte value = Convert.ToByte(hexValue, 16);

                for (int bit = 0; bit < 8; bit++)
                {
                    string checkBoxName = $"{byteName}Bit{bit}"; // Изменили с (7 - bit) на bit
                    CheckBox checkBox = FindCheckBoxByTag(checkBoxName);

                    if (checkBox != null)
                    {
                        checkBox.IsChecked = (value & (1 << bit)) != 0;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Некорректное HEX-значение байта");
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag != null)
            {
                string tag = checkBox.Tag.ToString();
                string byteName = tag.Substring(0, tag.IndexOf("Bit"));
                UpdateByteValue(byteName);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag != null)
            {
                string tag = checkBox.Tag.ToString();
                string byteName = tag.Substring(0, tag.IndexOf("Bit"));
                UpdateByteValue(byteName);
            }
        }

        private void ByteValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag != null)
            {
                string byteName = textBox.Tag.ToString();
                UpdateCheckBoxesFromByteValue(byteName, textBox.Text);
            }
        }

        private void ParameterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParameterList.SelectedIndex >= 0 && ParameterList.SelectedIndex != currentParameterIndex)
            {
                SaveCurrentParameter();
                currentParameterIndex = ParameterList.SelectedIndex;
                LoadCurrentParameter();
            }
        }

        private void AddParamButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentParameter();
            AddNewParameter();
        }

        private void DeleteParamButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentParameterIndex >= 0 && parameters.Count > 1)
            {
                parameters.RemoveAt(currentParameterIndex);
                currentParameterIndex = Math.Min(currentParameterIndex, parameters.Count - 1);
                LoadCurrentParameter();
                UpdateParameterList();
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveCurrentParameter();
                string filePath = @"C:\data\config.txt";
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));

                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    foreach (var param in parameters)
                    {
                        string line = $"{param.Name};{param.Id};{param.Multiplier};{param.Divider};{param.Indent};" +
                                     $"{param.Bytes[0]};{param.Bytes[1]};{param.Bytes[2]};{param.Bytes[3]};" +
                                     $"{param.Bytes[4]};{param.Bytes[5]};{param.Bytes[6]};{param.Bytes[7]}";
                        writer.WriteLine(line);
                    }
                }

                MessageBox.Show("Конфигурация успешно сохранена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = @"C:\data\config.txt";
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл конфигурации не найден");
                    return;
                }

                parameters.Clear();
                string[] lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    string[] parts = line.Split(';');
                    if (parts.Length >= 13)
                    {
                        var param = new Parameter
                        {
                            Name = parts[0],
                            Id = parts[1],
                            Multiplier = parts[2],
                            Divider = parts[3],
                            Indent = parts[4],
                            Bytes = new string[8]
                            {
                                parts[5], parts[6], parts[7], parts[8],
                                parts[9], parts[10], parts[11], parts[12]
                            }
                        };
                        parameters.Add(param);
                    }
                }

                if (parameters.Count > 0)
                {
                    currentParameterIndex = 0;
                    LoadCurrentParameter();
                    UpdateParameterList();
                    MessageBox.Show("Конфигурация успешно загружена!");
                }
                else
                {
                    MessageBox.Show("Файл конфигурации пуст или имеет неверный формат");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }
    }
}