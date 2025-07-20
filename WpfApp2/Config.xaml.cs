using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp2
{
    public partial class ConfigWindow : Window
    {
        private List<Parameter> parameters = new List<Parameter>();
        private int currentParameterIndex = -1;
        private bool isUpdatingUI = false;

        public ConfigWindow()
        {
            InitializeComponent();
            ParameterList.ItemsSource = new ObservableCollection<string>();
            AddNewParameter();
        }

        public class Parameter
        {
            public string Name { get; set; } = "Новый параметр";
            public string Id { get; set; } = "0";
            public string Multiplier { get; set; } = "1";
            public string Divider { get; set; } = "1";
            public string Indent { get; set; } = "0";
            public string[] Bytes { get; set; } = new string[8] { "00", "00", "00", "00", "00", "00", "00", "00" };
        }

        private void AddNewParameter()
        {
            parameters.Add(new Parameter());
            currentParameterIndex = parameters.Count - 1;
            UpdateParameterList();
            LoadCurrentParameter();
            NameEntry.Focus();
            NameEntry.SelectAll();
            StatusText.Text = "Добавлен новый параметр";
        }

        private bool isUpdatingSelection = false;

        private void ParameterList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ParameterList.SelectedIndex != -1)
            {
                currentParameterIndex = ParameterList.SelectedIndex;
                LoadCurrentParameter();
                NameEntry.Focus();
            }
        }

        private void UpdateParameterList()
        {
            if (isUpdatingSelection) return;

            isUpdatingSelection = true;
            try
            {
                var items = new ObservableCollection<string>();
                foreach (var param in parameters)
                {
                    items.Add($"{param.Name} ({param.Id})");
                }

                ParameterList.ItemsSource = items;

                if (currentParameterIndex >= 0 && currentParameterIndex < items.Count)
                {
                    ParameterList.SelectedIndex = currentParameterIndex;
                }
                else if (items.Count > 0)
                {
                    ParameterList.SelectedIndex = 0;
                    currentParameterIndex = 0;
                }
            }
            finally
            {
                isUpdatingSelection = false;
            }
        }

        private void LoadCurrentParameter()
        {
            isUpdatingUI = true;

            try
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
            finally
            {
                isUpdatingUI = false;
            }
        }

        private void SaveCurrentParameter()
        {
            if (isUpdatingUI || currentParameterIndex < 0 || currentParameterIndex >= parameters.Count)
                return;

            try
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
                    if (byteTextBox != null)
                    {
                        string text = byteTextBox.Text;
                        if (string.IsNullOrEmpty(text) || !IsHex(text))
                        {
                            text = "00";
                            byteTextBox.Text = text;
                        }
                        param.Bytes[i] = text.PadLeft(2, '0').ToUpper();
                    }
                }

                UpdateParameterList();
                StatusText.Text = $"Параметр '{param.Name}' сохранен";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка сохранения: {ex.Message}";
            }
        }

        private bool IsHex(string input)
        {
            return Regex.IsMatch(input, @"^[0-9A-Fa-f]{1,2}$");
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
            if (isUpdatingUI) return;

            byte value = 0;

            for (int bit = 0; bit < 8; bit++)
            {
                string checkBoxName = $"{byteName}Bit{bit}";
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
                if (string.IsNullOrEmpty(hexValue)) hexValue = "00";

                byte value;
                if (!byte.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber, null, out value))
                {
                    value = 0; // Если преобразование не удалось, используем 0
                }

                for (int bit = 0; bit < 8; bit++)
                {
                    string checkBoxName = $"{byteName}Bit{bit}";
                    CheckBox checkBox = FindCheckBoxByTag(checkBoxName);

                    if (checkBox != null)
                    {
                        checkBox.IsChecked = (value & (1 << bit)) != 0;
                    }
                }
            }
            catch
            {
                // Просто устанавливаем значение 00 без рекурсии
                TextBox byteTextBox = FindTextBoxByName($"{byteName}Value");
                if (byteTextBox != null)
                {
                    byteTextBox.Text = "00";
                    // Убираем рекурсивный вызов!
                    // Вместо этого просто обновляем чекбоксы для значения 00
                    byte value = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        string checkBoxName = $"{byteName}Bit{bit}";
                        CheckBox checkBox = FindCheckBoxByTag(checkBoxName);
                        if (checkBox != null)
                        {
                            checkBox.IsChecked = (value & (1 << bit)) != 0;
                        }
                    }
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag != null)
            {
                string tag = checkBox.Tag.ToString();
                string byteName = tag.Substring(0, tag.IndexOf("Bit"));
                UpdateByteValue(byteName);
                SaveCurrentParameter();
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag != null)
            {
                string tag = checkBox.Tag.ToString();
                string byteName = tag.Substring(0, tag.IndexOf("Bit"));
                UpdateByteValue(byteName);
                SaveCurrentParameter();
            }
        }

        private void ByteValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingUI) return;

            if (sender is TextBox textBox && textBox.Tag != null)
            {
                string byteName = textBox.Tag.ToString();
                string text = textBox.Text.ToUpper();

                if (!string.IsNullOrEmpty(text) && !IsHex(text))
                {
                    // Если ввод недопустимый, оставляем только последний допустимый символ
                    text = text.Length > 0 && IsHex(text[text.Length - 1].ToString()) ?
                           text[text.Length - 1].ToString() : "";
                    textBox.Text = text;
                    textBox.CaretIndex = text.Length;
                    return;
                }

                if (text.Length == 2)
                {
                    UpdateCheckBoxesFromByteValue(byteName, text);
                    SaveCurrentParameter();
                }
            }
        }

        private void HexTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только hex-символы
            Regex regex = new Regex(@"^[0-9A-Fa-f]$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void ParameterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (isUpdatingSelection || ParameterList.SelectedIndex == -1)
                    return;

                SaveCurrentParameter();
                currentParameterIndex = ParameterList.SelectedIndex;
                LoadCurrentParameter();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SelectionChanged error: {ex.Message}");
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
                var paramName = parameters[currentParameterIndex].Name;
                var result = MessageBox.Show($"Удалить параметр '{paramName}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    parameters.RemoveAt(currentParameterIndex);
                    currentParameterIndex = Math.Min(currentParameterIndex, parameters.Count - 1);
                    LoadCurrentParameter();
                    UpdateParameterList();
                    StatusText.Text = $"Параметр '{paramName}' удален";
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveCurrentParameter();
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string parameterInfo = button.Tag.ToString();
                int index = parameters.FindIndex(p => $"{p.Name} ({p.Id})" == parameterInfo);

                if (index >= 0)
                {
                    var confirmResult = MessageBox.Show(
                        $"Удалить параметр '{parameters[index].Name}'?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo);

                    if (confirmResult == MessageBoxResult.Yes)
                    {
                        parameters.RemoveAt(index);

                        UpdateParameterList();

                        if (currentParameterIndex >= parameters.Count)
                        {
                            currentParameterIndex = parameters.Count - 1;
                        }

                        if (parameters.Count > 0)
                        {
                            LoadCurrentParameter();
                        }
                        else
                        {
                            AddNewParameter();
                        }
                    }
                }
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveCurrentParameter();
                string filePath = @"C:\data\config.txt";
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

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
                    AddNewParameter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
                AddNewParameter();
            }
        }
    }
}