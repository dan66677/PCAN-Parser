using System.Windows;

namespace WpfApp2.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_New(object sender, RoutedEventArgs e)
        {
            Config taskWindow = new Config();
            taskWindow.Show();
        }
    }
}