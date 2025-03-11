using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public void RefreshSkia()
        {
            skiaGrid.Refresh();
        }

        private void AddDummyRow(object sender, RoutedEventArgs e)
        {
            viewModel.MyDataCollection.Add(new MyData
            {
                Age = 36,
                Id = 7,
                Name = "KKK"
            });
        }

        private void ToggleGridLines(object sender, RoutedEventArgs e)
        {
            viewModel.ShowGridLines = !viewModel.ShowGridLines;
            skiaGrid.Refresh();
        }
    }    
}