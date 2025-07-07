using WpfApp2.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using WpfApp2.Models;

namespace WpfApp2.Services
{
    public class CanLogParser
    {
        public static List<CanMessage> ParsePcanTrcLog(string filePath)
        {
            var messages = new List<CanMessage>();

            try
            {
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith(";"))
                        continue;

                    var message = ParsePcanTrcLine(line);
                    if (message != null)
                    {
                        messages.Add(message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения файла: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return messages;
        }

        private static CanMessage ParsePcanTrcLine(string line)
        {
            try
            {
                // Удаляем номер сообщения в скобках (например, "1)") и разделяем оставшуюся строку
                int bracketIndex = line.IndexOf(')');
                if (bracketIndex < 0)
                    return null;

                string remainingLine = line.Substring(bracketIndex + 1).Trim();
                var parts = remainingLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 5)
                    return null;

                // Парсим номер сообщения (удаляем закрывающую скобку)
                int messageNumber = int.Parse(line.Substring(0, bracketIndex).Trim());

                // Временное смещение (может быть с точкой)
                double timeOffsetMs = double.Parse(parts[0], CultureInfo.InvariantCulture);

                // Направление
                bool isReceived = parts[1].Equals("Rx", StringComparison.OrdinalIgnoreCase);

                // CAN ID
                uint id = Convert.ToUInt32(parts[2], 16);

                // Длина данных
                int length = int.Parse(parts[3]);

                // Данные (начинаются с 4-го элемента)
                byte[] data = new byte[length];
                for (int i = 0; i < length && (i + 4) < parts.Length; i++)
                {
                    data[i] = Convert.ToByte(parts[i + 4], 16);
                }

                return new CanMessage
                {
                    MessageNumber = messageNumber,
                    TimeOffsetMs = timeOffsetMs,
                    IsReceived = isReceived,
                    Id = id,
                    Data = data,
                    Length = length
                };
            }
            catch
            {
                return null;
            }
        }
    }
}