using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для Config.xaml
    /// </summary>
    public partial class Config : Window
    {
        public Config()
        {
            InitializeComponent();
        }

        public class TextBoxWriter
        {
            public static void WriteToFile(TextBox[] textBoxes, string filePath)
            {
                // Проверка входных данных
                if (textBoxes == null || textBoxes.Length == 0)
                {
                    MessageBox.Show("Нет текстовых полей для записи");
                    return;
                }

                try
                {
                    // Создаем или перезаписываем файл
                    using (StreamWriter writer = new StreamWriter(filePath, true, Encoding.UTF8))
                    {
                        foreach (TextBox textBox in textBoxes)
                        {
                            // Записываем содержимое каждого TextBox
                            writer.Write(textBox.Text);
                            writer.Write(";");
                        }
                        writer.Write("\n");
                    }

                    MessageBox.Show("Данные успешно записаны в файл!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка записи: {ex.Message}");
                }
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            // Получаем все нужные TextBox
            TextBox[] inputBoxes = { NameEntry, IdEntry, Byte };

            // Указываем путь к файлу
            string filePath = @"C:\data\config.txt";

            // Вызываем функцию записи
            TextBoxWriter.WriteToFile(inputBoxes, filePath);
        }


    }
}
