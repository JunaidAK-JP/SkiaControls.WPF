using SkiaSharp;
using System.Drawing;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        public void RefreshSkia()
        {
            skiaGrid.Refresh();
        }

        public static float GetSystemDpi()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return g.DpiX / 96.0f; // 96 DPI is the default (100% scaling)
            }
        }
    }    
}