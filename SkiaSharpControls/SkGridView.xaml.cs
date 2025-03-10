using SkiaSharp;
using SkiaSharpControls.Models;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SkiaSharpControls
{
    /// <summary>
    /// Interaction logic for SkGridView.xaml
    /// </summary>
    public partial class SkGridView : UserControl
    {
        public SkGridView()
        {
            InitializeComponent();

            DataListView.ItemsSource = new List<string>() { "" };
            renderer.SetScrollBars(HorizontalScrollViewer, VerticalScrollViewer);

            SizeChanged += (s, o) =>
            {
                UpdateValues();
            };

            Loaded += (s, o) =>
            {

                SkiaCanvas.Height = GetSkiaHeight(TotalRows);
                SkiaCanvas.Width = GetSkiaWidth();
                DataListViewScroll = FindScrollViewer(DataListView);
                AddColumnWidthChangedHandler(DataListView);

                UpdateValues();
            };
        }

        private bool IsBusy { get; set; }
        private SkGridRenderer renderer = new();

        public Action<object> OnRowClicked
        {
            get { return (Action<object>)GetValue(OnRowClickedProperty); }
            set { SetValue(OnRowClickedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnItemClick.  This enables animation, styling, binding, etc...
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


        public IEnumerable<SkGridViewColumn> Columns
        {
            get { return (IEnumerable<SkGridViewColumn>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Columns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(IEnumerable<SkGridViewColumn>), typeof(SkGridView), new PropertyMetadata(default, OnColumnsChanged));

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView && e.NewValue is IEnumerable<SkGridViewColumn> columns)
            {
                skGridView.IsBusy = true;
                skGridView.GV.Columns.CollectionChanged -= skGridView.OnColumnsReordered;
                skGridView.GV.Columns.Clear();

                foreach (var column in columns)
                {
                    skGridView.GV.Columns.Add(new GridViewColumn() { Header = column.Header, Width = column.Width });
                }

                skGridView.renderer.SetColumns(columns);
                skGridView.IsBusy = false;
                skGridView.SkiaCanvas.InvalidateVisual();
                skGridView.GV.Columns.CollectionChanged += skGridView.OnColumnsReordered;
            }
        }

        private void OnColumnsReordered(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ICollection<GridViewColumn> items)
            {
                IEnumerable<SkGridViewColumn> columns = [];

                foreach (var item in items)
                {
                    var existingItem = Columns.FirstOrDefault(x => x.Header == item.Header?.ToString());

                    columns = columns.Append(new SkGridViewColumn()
                    {
                        Header = existingItem?.Header ?? "",
                        Width = existingItem?.Width ?? 100,
                    });
                }

                Columns = columns;
                renderer.SetColumns(columns);
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
            if (d is SkGridView skGridView && e.NewValue is IEnumerable list)
            {
                if (list != null)
                {
                    var listCount = list.Cast<object>().Count();
                    skGridView.SkiaCanvas.InvalidateVisual();

                    if (listCount > 0)
                    {
                        skGridView.TotalRows = listCount;
                        skGridView.VerticalScrollViewer.Maximum = skGridView.SkiaCanvas.ActualHeight - skGridView.MainGrid.ActualHeight;
                    }
                    else
                    {
                        skGridView.TotalRows = 0;
                        skGridView.VerticalScrollViewer.Maximum = 0;
                    }
                }
                skGridView?.UpdateValues();
            }
        }

        private double GetSkiaHeight(int totalRows)
        {
            if (MainGrid.ActualHeight < (totalRows * RowHeight))
            {
                return MainGrid.ActualHeight;
            }
            return (totalRows * RowHeight);
        }


        private double GetSkiaWidth()
        {
            var totalcolumnwidth = GV.Columns.Where(x => x.Width > 0).Sum(x => x.Width);
            if (MainGrid.ActualWidth > totalcolumnwidth)
            {
                return totalcolumnwidth;
            }
            return MainGrid.ActualWidth;
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
            if (d is SkGridView skGridView && e.NewValue is IEnumerable)
            {
                skGridView.SkiaCanvas.InvalidateVisual();
            }
        }

        public Func<object, SKColor> RowBackgroundSelector
        {
            get => (Func<object, SKColor>)GetValue(RowBackgroundSelectorProperty);
            set => SetValue(RowBackgroundSelectorProperty, value);
        }

        public static readonly DependencyProperty RowBackgroundSelectorProperty =
            DependencyProperty.Register(nameof(RowBackgroundSelector), typeof(Func<object, SKColor>), typeof(SkGridView), new PropertyMetadata(default, OnRowBackgroundTemplateChanged));

        private static void OnRowBackgroundTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView && e.NewValue is Func<object, SKColor> template)
            {
                skGridView.renderer.SetRowBackgroundSelector(template);
            }
        }

        public Func<object, string, SkCellTemplate> CellTemplateSelector
        {
            get => (Func<object, string, SkCellTemplate>)GetValue(CellTemplateSelectorProperty);
            set => SetValue(CellTemplateSelectorProperty, value);
        }

        public static readonly DependencyProperty CellTemplateSelectorProperty =
            DependencyProperty.Register(nameof(CellTemplateSelector), typeof(Func<object, string, SkCellTemplate>), typeof(SkGridView), new PropertyMetadata(default, OnCellTemplateChanged));

        private static void OnCellTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView && e.NewValue is Func<object, string, SkCellTemplate> template)
            {
                skGridView.renderer.SetCellTemplateSelector(template);
            }
        }

        private void OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }

            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            SetScale(canvas);
            canvas.Save();
            canvas.Translate(-ScrollOffsetX, -ScrollOffsetY);
            Draw(canvas);
            canvas.Restore();
        }

        private void Draw(SKCanvas canvas)
        {
            renderer.UpdateItems(ItemsSource);
            renderer.UpdateSelectedItems(SelectedItems);

            renderer.Draw(canvas, ScrollOffsetX, ScrollOffsetY, RowHeight, TotalRows);
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

            int rowIndex = (int)(point.Y / RowHeight);
            double x = point.X;

            var s = new List<dynamic>((IEnumerable<dynamic>)ItemsSource);

            if (s.Count > rowIndex)
            {
                OnRowClicked?.Invoke(s[rowIndex]);
            }

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
                IEnumerable<SkGridViewColumn> columns = [];

                foreach (var item in items)
                {
                    var existingItem = Columns.FirstOrDefault(x => x.Header == item.Header?.ToString());

                    columns = columns.Append(new SkGridViewColumn()
                    {
                        Header = existingItem?.Header ?? "",
                        Width = item.Width,
                    });
                }

                Columns = columns;
            }
            UpdateValues();
        }

        public void Refresh()
        {
            TotalRows = ItemsSource.Cast<object>().Count();
            UpdateValues();
            SkiaCanvas.InvalidateVisual();
        }

        public List<string> FakeList { get; set; } = new() { "" };
        private float ScrollOffsetX = 0, ScrollOffsetY = 0;
        private int TotalRows = 0;
        private int TotalCols = 61;
        private float ColWidth = 90;
        private int RowHeight = 18;
        private ScrollViewer DataListViewScroll;
        private float DpiScalling = 0;
        private SKFont TextFont;
        private void SkiaCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemsSource == null)
                return;

            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Right)
            {

            }
        }

        private void SkiaCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var clickPosition = e.GetPosition(SkiaCanvas);
            int clickedRowIndex = (int)((clickPosition.Y + ScrollOffsetY) / RowHeight);
            int clickedColumnIndex = (int)((clickPosition.X + ScrollOffsetX));
            //if (DescriptionRowIndexList != null && DescriptionRowIndexList.Contains(clickedRowIndex) && clickedColumnIndex >= (DescriptionColumnPostion + 5) && clickedColumnIndex <= (DescriptionColumnPostion + 20))
            //{

            //    var visiblerows = ItemsSource.Where(x => x.IsVisible == true).ToList();
            //    foreach (var item in ItemsSource.Where(x => x.GroupName == visiblerows[clickedRowIndex].Description))
            //    {
            //        item.IsVisible = !visiblerows[clickedRowIndex].IsExpanded;
            //    }
            //    foreach (var item in ItemsSource.Where(x => x.GroupName == visiblerows[clickedRowIndex].Description && x.IsExtra == true))
            //    {
            //        item.IsVisible = false;
            //    }
            //    visiblerows[clickedRowIndex].IsExpanded = !visiblerows[clickedRowIndex].IsExpanded;

            //    int iteration = 0;
            //    visiblerows = ItemsSource.Where(x => x.IsVisible == true).ToList();
            //    foreach (var item in visiblerows)
            //    {
            //        if (item.IsGroup)
            //        {
            //            DescriptionRowIndexList[iteration++] = visiblerows.IndexOf(item);

            //        }
            //    }

            //    SkiaHeight = visiblerows.Count() * RowHeight;
            //    TotalRows = visiblerows.Count();
            //    VerticalScrollViewer.Maximum = SkiaHeight - MainGrid.ActualHeight;

            //}
            //if (clickedRowIndex >= 0 && clickedRowIndex < TotalRows)
            //{
            //    selectedRowIndex = clickedRowIndex;
            //    SkiaCanvas.InvalidateVisual();
            //}
        }

        private void VerticalScrollViewer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollOffsetY = (float)e.NewValue;
            SkiaCanvas.InvalidateVisual();
        }
        private void HorizontalScrollViewer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollOffsetX = (float)(e.NewValue);
            DataListViewScroll.ScrollToHorizontalOffset(ScrollOffsetX);
            SkiaCanvas.InvalidateVisual();
        }
        private void AddColumnWidthChangedHandler(System.Windows.Controls.ListView listView)
        {
            if (listView.View is GridView gridView)
            {
                foreach (var column in gridView.Columns)
                {
                    DependencyPropertyDescriptor.FromProperty(GridViewColumn.WidthProperty, typeof(GridViewColumn))
                        .AddValueChanged(column, OnColumnWidthChanged);
                }
            }
        }

        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            UpdateValues();
            SkiaCanvas.InvalidateVisual();
        }

        private static ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            if (parent is ScrollViewer)
                return (ScrollViewer)parent;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void UpdateValues()
        {
            SkiaCanvas.Height = GetSkiaHeight(TotalRows);
            SkiaCanvas.Width = GetSkiaWidth();
            var TotalColsVisible = GV.Columns.Where(x => x.Width > 0).Select(x => x.Width);
            ColWidth = (float)(TotalColsVisible.Sum() / (TotalColsVisible.Count() - 1));

            TotalCols = 0;

            HorizontalScrollViewer.Minimum = 0;
            HorizontalScrollViewer.ViewportSize = MainGrid.ActualWidth;
            HorizontalScrollViewer.Maximum = TotalColsVisible.Sum() - MainGrid.ActualWidth;

            VerticalScrollViewer.Minimum = 0;
            VerticalScrollViewer.ViewportSize = MainGrid.ActualHeight;
            VerticalScrollViewer.Maximum = ((TotalRows + 5) * RowHeight) - MainGrid.ActualHeight;

            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                var res = source.CompositionTarget.TransformToDevice.M11;
                DpiScalling = (float)res;
            }
            else
            {
                DpiScalling = GetSystemDpi();
            }
        }
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void MainGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {

                HorizontalScrollViewer.Value -= e.Delta / 1;
            }
            else
            {
                VerticalScrollViewer.Value -= e.Delta / 1;
            }
        }
    }
}