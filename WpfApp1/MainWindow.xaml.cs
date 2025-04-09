using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WpfApp1.UserControls;

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
            DispatcherTimer dispatcherTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(250),
            };
            dispatcherTimer.Tick += dispatcherTimerHander;
            dispatcherTimer.Start();
        }

        private void dispatcherTimerHander(object? sender, EventArgs e)
        {
            RefreshSkia();
        }

        public void RefreshSkia()
        {
            skiaGrid.Refresh();
        }

        private void AddDummyRow(object sender, RoutedEventArgs e)
        {
            //viewModel.MyDataCollection.Add(new MyData
            //{
            //    Age = 36,
            //    Id = 7,
            //    Name = "KKK"
            //});

            var textbox = new TextBoxContainer()
            {
                Width = 50,
                Height = 18,
                AllowAlphabet = true,
                UseCaps = true,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            textbox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
                {
                    MessageBox.Show(textbox.TextContent);
                }
            };

            skiaGrid.AddWpfElement(textbox);
        }

        private void ToggleGridLines(object sender, RoutedEventArgs e)
        {
            viewModel.ShowGridLines = !viewModel.ShowGridLines;
            skiaGrid.Refresh();
        }
    }    
}