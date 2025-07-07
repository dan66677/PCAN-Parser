using WpfApp2.Models;
using WpfApp2.Services;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;


namespace WpfApp2.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private List<CanMessage> _allMessages = new List<CanMessage>();
        private string _statusText = "Готов к работе";
        private string _filterText;

        public ObservableCollection<CanMessage> Messages { get; } = new ObservableCollection<CanMessage>();

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                SetProperty(ref _filterText, value);
                ApplyFilter();
            }
        }

        public ICommand OpenFileCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public MainViewModel()
        {
            OpenFileCommand = new RelayCommand(OpenFile);
            ClearFilterCommand = new RelayCommand(ClearFilter);
        }

        private void OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PCAN TRC Files (*.trc)|*.trc|All files (*.*)|*.*",
                Title = "Открыть файл логов PCAN-View"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _allMessages = CanLogParser.ParsePcanTrcLog(openFileDialog.FileName);
                ApplyFilter();
                StatusText = $"Загружено {_allMessages.Count} сообщений из {openFileDialog.FileName}";
            }
        }

        private void ApplyFilter()
        {
            Messages.Clear();

            if (string.IsNullOrWhiteSpace(FilterText))
            {
                foreach (var msg in _allMessages)
                    Messages.Add(msg);
                StatusText = $"Показаны все сообщения ({_allMessages.Count})";
                return;
            }

            var filtered = _allMessages.Where(m =>
                m.HexId.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                m.AsciiData.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var msg in filtered)
                Messages.Add(msg);

            StatusText = $"Показано {filtered.Count} сообщений по фильтру '{FilterText}'";
        }

        private void ClearFilter()
        {
            FilterText = string.Empty;
        }
    }

    public class BaseViewModel
    {
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter) => _execute();
    }
}