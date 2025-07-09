
using System.Windows;

namespace WpfApp2.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Config(object sender, RoutedEventArgs e)
        {
            ConfigWindow taskWindow = new ConfigWindow();
            taskWindow.Show();
        }
        private void Button_Excel(object sender, RoutedEventArgs e)
        {
            ExcelWindow taskWindow = new ExcelWindow();
            taskWindow.Show();
        }
    }
}