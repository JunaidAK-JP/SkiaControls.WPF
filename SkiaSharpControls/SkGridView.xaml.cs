using SkiaSharp;
using SkiaSharpControls.Models;
using SkiaSharpControls.Renderer;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace SkiaSharpControls
{
    /// <summary>
    /// Interaction logic for SKListView.xaml
    /// </summary>
    public partial class SkGridView : UserControl
    {
        public SkGridView()
        {
            InitializeComponent();
            Unloaded += SkGridView_Unloaded;
        }

        private void SkGridView_Unloaded(object sender, RoutedEventArgs e)
        {
            Renderer?.Dispose();
        }

        private bool IsBusy { get; set; }
        public ISkGridRenderer Renderer { get; set; } = new SkGridRenderer();

        public SkRendererProperties DefaultRendererProperties
        {
            get { return (SkRendererProperties)GetValue(DefaultRendererPropertiesProperty); }
            set { SetValue(DefaultRendererPropertiesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultRendererProperties.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultRendererPropertiesProperty =
            DependencyProperty.Register(nameof(DefaultRendererProperties), typeof(SkRendererProperties), typeof(SkGridView), new PropertyMetadata(new SkRendererProperties(), OnRendererPropertiesChanged));

        private static void OnRendererPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView gridView)
            {
                gridView.Renderer.RendererProperties = gridView.DefaultRendererProperties;
            }
        }

        public Action<object> OnRowClicked
        {
            get { return (Action<object>)GetValue(OnRowClickedProperty); }
            set { SetValue(OnRowClickedProperty, value); }
        }
        
        public static readonly DependencyProperty OnRowClickedProperty =
            DependencyProperty.Register(nameof(OnRowClicked), typeof(Action<object>), typeof(SkGridView), new PropertyMetadata(default));


        public Action<object, string> OnCellClicked
        {
            get { return (Action<object, string>)GetValue(OnCellClickedProperty); }
            set { SetValue(OnCellClickedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnItemClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnCellClickedProperty =
            DependencyProperty.Register(nameof(OnCellClicked), typeof(Action<object, string>), typeof(SkGridView), new PropertyMetadata(default));


        public IEnumerable<SKGridViewColumn> Columns
        {
            get { return (IEnumerable<SKGridViewColumn>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Columns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(IEnumerable<SKGridViewColumn>), typeof(SkGridView), new PropertyMetadata(default, OnColumnsChanged));

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skListView && e.NewValue is IEnumerable<SKGridViewColumn> columns)
            {
                skListView.IsBusy = true;
                skListView.GV.Columns.CollectionChanged -= skListView.OnColumnsReordered;
                skListView.GV.Columns.Clear();

                foreach (var column in columns)
                {
                    skListView.GV.Columns.Add(new GridViewColumn() { Header = column.Header, Width = column.Width });
                }

                skListView.IsBusy = false;
                skListView.SkiaCanvas.InvalidateVisual();
                skListView.GV.Columns.CollectionChanged += skListView.OnColumnsReordered;
            }
        }

        private void OnColumnsReordered(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ICollection<GridViewColumn> items)
            {
                IEnumerable<SKGridViewColumn> columns = [];

                foreach (var item in items)
                {
                    var existingItem = Columns.FirstOrDefault(x => x.Header == item.Header?.ToString());

                    columns = columns.Append(new SKGridViewColumn()
                    {
                        Header = existingItem?.Header ?? "",
                        Width = existingItem?.Width ?? 100,
                    });
                }

                Columns = columns;
            }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(SkGridView), new PropertyMetadata(default, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skListView && e.NewValue is IEnumerable)
            {
                skListView.SkiaCanvas.InvalidateVisual();
            }
        }

        public IEnumerable SelectedItems
        {
            get { return (IEnumerable)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IEnumerable), typeof(SkGridView), new PropertyMetadata(default, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skListView && e.NewValue is IEnumerable)
            {
                skListView.SkiaCanvas.InvalidateVisual();
            }
        }

        public Func<object, SKColor> RowBackgroundSelector
        {
            get => (Func<object, SKColor>)GetValue(RowBackgroundSelectorProperty);
            set => SetValue(RowBackgroundSelectorProperty, value);
        }

        public static readonly DependencyProperty RowBackgroundSelectorProperty =
            DependencyProperty.Register(nameof(RowBackgroundSelector), typeof(Func<object, SKColor>), typeof(SkGridView), new PropertyMetadata(default));

        public Func<object, string, SkCellTemplate> CellTemplateSelector
        {
            get => (Func<object, string, SkCellTemplate>)GetValue(CellTemplateSelectorProperty);
            set => SetValue(CellTemplateSelectorProperty, value);
        }

        public static readonly DependencyProperty CellTemplateSelectorProperty =
            DependencyProperty.Register(nameof(CellTemplateSelector), typeof(Func<object, string, SkCellTemplate>), typeof(SkGridView), new PropertyMetadata(default));

        private void OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }

            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            SetScale(canvas);
            Renderer.Draw(canvas, ItemsSource, Columns, RowBackgroundSelector, CellTemplateSelector);
        }

        private void SetScale(SKCanvas canvas)
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                var res = source.CompositionTarget.TransformToDevice.M11;
                canvas.Scale((float)res);
            }
            else
            {
                canvas.Scale(GetSystemDpi());
            }
        }

        public static float GetSystemDpi()
        {
            using Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            return g.DpiX / 96.0f; // 96 DPI is the default (100% scaling)
        }

        private void SkiaCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Get mouse position relative to SKElement
            var point = e.GetPosition(SkiaCanvas);

            int rowIndex = (int)(point.Y / 25);
            double x = point.X;

            var s = new List<dynamic>((IEnumerable<dynamic>)ItemsSource);

            if (s.Count > rowIndex)
                OnRowClicked?.Invoke(s[rowIndex]);

            foreach (var item in GV.Columns)
            {
                x -= item.Width;
                if (x <= 0)
                {
                    OnCellClicked?.Invoke(s[rowIndex], item.Header?.ToString());
                    break;
                }
            }
        }

        private void OnDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            IsBusy = true;
        }

        private void OnDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            IsBusy = false;

            if (GV.Columns is ICollection<GridViewColumn> items)
            {
                IEnumerable<SKGridViewColumn> columns = [];

                foreach (var item in items)
                {
                    var existingItem = Columns.FirstOrDefault(x => x.Header == item.Header?.ToString());

                    columns = columns.Append(new SKGridViewColumn()
                    {
                        Header = existingItem?.Header ?? "",
                        Width = item.Width,
                    });
                }

                Columns = columns;
            }
        }

        public void Refresh()
        {
            SkiaCanvas.InvalidateVisual();
        }
    }
}