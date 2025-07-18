
using SkiaSharpControlV2.Enum;
using System.Windows;

using System.Windows.Threading;

namespace SampleApplicationV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel = new();
        public MainWindow()
        {
            DataContext = viewModel;
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += (s, e) =>
            {
                skiaGrid.Refresh();
            };
            timer.Start();
        }

        private void CopyAllData_Click(object sender, RoutedEventArgs e)
        {
           Clipboard.SetText(skiaGrid.ExportData(SKExportType.All));
        }
        private void CopySelectedData_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(skiaGrid.ExportData(SKExportType.Selected));
        }
    }
}