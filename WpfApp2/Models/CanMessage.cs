using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.Models
{
    public class CanMessage
    {
        public int MessageNumber { get; set; }       // Номер сообщения (1)
        public double TimeOffsetMs { get; set; }     // Временное смещение (мс)
        public bool IsReceived { get; set; }         // Rx/Tx
        public uint Id { get; set; }                 // CAN ID (hex)
        public byte[] Data { get; set; }             // Данные
        public int Length { get; set; }              // DLC

        // Форматированные свойства для отображения в интерфейсе
        public string HexId => $"0x{Id:X8}";
        public string AsciiData => BitConverter.ToString(Data).Replace("-", " ");
        public string Direction => IsReceived ? "Rx" : "Tx";
        public string TimeOffsetFormatted => TimeOffsetMs.ToString("0.###", CultureInfo.InvariantCulture);

    }
}
