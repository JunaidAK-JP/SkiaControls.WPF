
using SkiaSharp;
using SkiaSharpControlV2.Enum;
using SkiaSharpControlV2.Helpers;
using SkiaSharpControlV2.Renderer;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;


namespace SkiaSharpControlV2
{
    /// <summary>
    /// Interaction logic for SkiaGridViewV2.xaml
    /// </summary>

    //[ContentProperty(nameof(Columns))]
    public partial class SkiaGridViewV2 : UserControl
    {
        private SkiaRenderer SkiaRenderer;
        #region Properties
        private float ScrollOffsetX = 0, ScrollOffsetY = 0, RowHeight = 12 + 4;
        private int TotalRows;
        private ScrollViewer DataListViewScroll { get => Helper.FindScrollViewer(DataListView); }
        private ICollectionView? _collectionView;
        private bool IsBusy { get; set; } = false;
        private float Scale { get; set; }
        #endregion Properties
        public SkiaGridViewV2()
        {
            InitializeComponent();
            DataListView.ItemsSource = new List<string> { "" };
            //DispatcherTimer tm = new DispatcherTimer()
            //{
            //    Interval = TimeSpan.FromMilliseconds(250)
            //};
            //tm.Tick += (s, e) =>
            //{
            //    var col = Columns;
            //};
            //tm.Start();
            SkiaRenderer = new();
            Loaded += (s, e) =>
            {
                SetScale();
                UpdateColumnsInDataGrid();
                UpdateScrollValues();
                UpdateSkiaGrid();
                if (Columns == null || Columns?.Count == 0)
                    TryGenerateColumns(ItemsSource);

                SkiaRenderer.UpdateItems(ItemsSource);
                SkiaRenderer.UpdateSelectedItems(SelectedItems);
                SkiaRenderer.SetColumns(Columns!);
                SkiaRenderer.SetGridLinesVisibility(true);
                SkiaRenderer.SetScrollBars(HorizontalScrollViewer, VerticalScrollViewer);
                SkiaRenderer.SetFont("Tahoma", 12);
            };
            SizeChanged += (s, o) =>
            {
                SetScale();
                UpdateSkiaGrid();
                UpdateScrollValues();

            };


        }

        #region Columns
        public SkGridColumnCollection Columns
        {
            get => (SkGridColumnCollection)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(SkGridColumnCollection), typeof(SkiaGridViewV2), new PropertyMetadata(new SkGridColumnCollection()));

        #endregion Columns

        #region GroupColumns
        public GroupDefinition GroupSettings
        {
            get => (GroupDefinition)GetValue(GroupSettingsProperty);
            set => SetValue(GroupSettingsProperty, value);
        }

        public static readonly DependencyProperty GroupSettingsProperty =
            DependencyProperty.Register(nameof(GroupSettings), typeof(GroupDefinition), typeof(SkiaGridViewV2), new PropertyMetadata(null));

        #endregion GroupColumns

        #region ItemSource
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(SkiaGridViewV2), new PropertyMetadata(default, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SkiaGridViewV2 grid)
                return;

            if (e.NewValue is IEnumerable list)
                grid.TotalRows = list.Cast<object>().Count();

            if (e.OldValue is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= grid!.OnItemsChanged!;

            if (e.NewValue is INotifyCollectionChanged newCollection)
                newCollection.CollectionChanged += grid!.OnItemsChanged!;

            grid._collectionView = CollectionViewSource.GetDefaultView(e.NewValue);

            grid.UpdateSkiaGrid();
            grid.UpdateScrollValues();

        }
        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is IEnumerable list)
                TotalRows = ItemsSource.Cast<object>().Count();


            UpdateSkiaGrid();
            UpdateScrollValues();
        }
        private void TryGenerateColumns(object itemsSource)
        {
            if (itemsSource is IEnumerable enumerable)
            {
                var itemType = itemsSource.GetType().GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .FirstOrDefault()
                    ?? enumerable.Cast<object>().FirstOrDefault()?.GetType();

                if (itemType != null && Columns.Count == 0)
                {
                    GenerateColumnsFromType(itemType);
                }
            }
        }
        private void GenerateColumnsFromType(Type modelType)
        {
            Columns.Clear();
            foreach (var prop in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Columns.Add(new SKGridViewColumn
                {
                    Header = prop.Name,
                    // BindingPath = prop.Name,
                    Width = 120,
                    IsVisible = true
                });
            }
            UpdateColumnsInDataGrid();
            UpdateScrollValues();
        }
        #endregion ItemSource

        #region SelectedItems
        public ObservableCollection<object> SelectedItems
        {
            get { return (ObservableCollection<object>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(ObservableCollection<object>), typeof(SkiaGridViewV2), new PropertyMetadata(default, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkiaGridViewV2 skGridView && e.NewValue is ObservableCollection<object>)
            {
                skGridView.SkiaCanvas.InvalidateVisual();
            }
        }
        #endregion SelectedItems


        #region PrivateMethods
        private void UpdateColumnsInDataGrid()
        {
            if (Columns != null && Columns.Count > 0)
            {
                DataListView.Columns.Clear();

                foreach (var column in Columns)
                {
                    var headerStyle = new Style(typeof(DataGridColumnHeader));
                    headerStyle.Setters.Add(new Setter(HorizontalContentAlignmentProperty, column.ContentAlignment == Enum.CellContentAlignment.Right ? HorizontalAlignment.Right :
                                                                                         column.ContentAlignment == Enum.CellContentAlignment.Left ? HorizontalAlignment.Left : HorizontalAlignment.Center));

                    headerStyle.Setters.Add(new Setter(BackgroundProperty, Helper.GetColorBrush(column.BackColor ?? "#FF3F3F3F")));


                    headerStyle.BasedOn = this.Resources["ColumnHeaderStyle"] as Style;

                    var dgColumn = new DataGridTextColumn
                    {
                        Header = column.DisplayHeader ?? column.Header,
                        Width = column.Width,
                        Visibility = !column.IsVisible ? Visibility.Collapsed : Visibility.Visible,
                        HeaderStyle = headerStyle,
                        MinWidth = 30,
                    };

                    if (column.CanUserResize.HasValue)
                        dgColumn.CanUserResize = column.CanUserResize.Value;
                    if (column.CanUserReorder.HasValue)
                        dgColumn.CanUserReorder = column.CanUserReorder.Value;
                    if (column.CanUserSort.HasValue)
                        dgColumn.CanUserSort = column.CanUserSort.Value;
                    DataListView.Columns.Add(dgColumn);


                }
                var sortColumns = Columns.Where(x => x?.GridViewColumnSort != null && x?.GridViewColumnSort != SkGridViewColumnSort.None).LastOrDefault();
                if (sortColumns != null)
                {
                    DataListView.Columns!.Where(x => x.Header as string == (sortColumns.DisplayHeader ?? sortColumns.Header)).FirstOrDefault()!
                                                    .SortDirection = sortColumns.GridViewColumnSort == SkGridViewColumnSort.Ascending ? ListSortDirection.Ascending
                                                                    : (sortColumns.GridViewColumnSort == SkGridViewColumnSort.Descending ? ListSortDirection.Descending : null);
                }

                SubscribeToColumnEvents(Columns);
                MonitorColumnResize(DataListView);
            }
        }
        private void MonitorColumnResize(DataGrid dataGrid)
        {
            try
            {
                foreach (var column in dataGrid.Columns)
                {
                    var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));

                    descriptor?.RemoveValueChanged(column, OnColumnWidthChanged);
                    descriptor?.AddValueChanged(column, OnColumnWidthChanged);
                }
            }
            catch (Exception)
            {
            }

        }
        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is DataGridTextColumn column)
                {
                    var col = Columns.FirstOrDefault(x => x.Header == column?.Header.ToString() || x.DisplayHeader == column?.Header.ToString());
                    if (col != null)
                        col.Width = column.ActualWidth;
                    UpdateScrollValues();
                }
            }
            catch (Exception)
            {

            }

        }
        private void SubscribeToColumnEvents(IEnumerable<SKGridViewColumn> columns)
        {
            foreach (var col in columns)
            {
                col.PropertyChanged -= Column_PropertyChanged;
                col.PropertyChanged += Column_PropertyChanged;
            }
        }
        private void Column_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is SKGridViewColumn column)
            {
                DataGridColumn? col = DataListView.Columns.FirstOrDefault(x => x.Header.ToString() == column?.Header?.ToString());
                if (col != null)
                {
                    if (e.PropertyName == nameof(SKGridViewColumn.IsVisible))
                    {
                        col.Visibility = !column.IsVisible ? Visibility.Collapsed : Visibility.Visible;
                        UpdateScrollValues();
                        UpdateSkiaGrid();
                    }
                    if (e.PropertyName == nameof(SKGridViewColumn.CanUserReorder))
                        col.CanUserResize = column!.CanUserResize!.Value;
                    if (e.PropertyName == nameof(SKGridViewColumn.CanUserResize))
                        col.CanUserReorder = column!.CanUserReorder!.Value;
                    if (e.PropertyName == nameof(SKGridViewColumn.CanUserSort))
                        col.CanUserSort = column!.CanUserSort!.Value;
                    if (e.PropertyName == nameof(SKGridViewColumn.GridViewColumnSort)) { }
                    if (e.PropertyName == nameof(SKGridViewColumn.Width))
                    {
                        col.Width = new DataGridLength(column.Width);
                        UpdateScrollValues();
                        UpdateSkiaGrid();
                    }
                    if (e.PropertyName == nameof(SKGridViewColumn.BackColor))
                    {
                        var newStyle = new Style(typeof(DataGridColumnHeader), col.HeaderStyle);
                        newStyle.Setters.Add(new Setter(Control.BackgroundProperty, Helper.GetColorBrush(column.BackColor ?? "#FF3F3F3F")));
                        col.HeaderStyle = newStyle;
                    }
                }

            }
        }
        private double GetSkiaHeight()
        {
            if (MainGrid.ActualHeight < (TotalRows * RowHeight))
            {
                return MainGrid.ActualHeight;
            }
            return (TotalRows * RowHeight);
        }

        private double GetSkiaWidth()
        {
            var totalcolumnwidth = GetVisibleColumnsWidth();
            if (MainGrid.ActualWidth > totalcolumnwidth)
            {
                return totalcolumnwidth;
            }
            return MainGrid.ActualWidth;
        }
        private void UpdateSkiaGrid()
        {
            SkiaCanvas.Height = GetSkiaHeight();
            SkiaCanvas.Width = GetSkiaWidth(); 
        }
        private double GetVisibleColumnsWidth() => DataListView.Columns.Where(x => x.Visibility == Visibility.Visible).Sum(x => x.Width.Value);

        private void UpdateScrollValues()
        {
            HorizontalScrollViewer.Minimum = 0;
            HorizontalScrollViewer.ViewportSize = MainGrid.ActualWidth;
            HorizontalScrollViewer.Maximum = (GetVisibleColumnsWidth() + 10) - MainGrid.ActualWidth + 10;

            VerticalScrollViewer.Minimum = 0;
            VerticalScrollViewer.ViewportSize = MainGrid.ActualHeight;
            VerticalScrollViewer.Maximum = ((TotalRows + 3.3) * RowHeight) - MainGrid.ActualHeight;
        }


        private void SetScale()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                var res = source.CompositionTarget.TransformToDevice.M11;
                Scale = (float)res;
            }
            else
            {
                Scale = Helper.GetSystemDpi();
            }
        }
        private void ApplySort(string propertyName, ListSortDirection direction)
        {
            _collectionView?.SortDescriptions.Clear();
            _collectionView?.SortDescriptions.Add(new SortDescription(propertyName, direction));
            _collectionView?.Refresh();
        }

        #endregion PrivateMethods

        private void MainGrid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
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

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //SkiaCanvas.Focus();
        }

        private void HorizontalScrollViewer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollOffsetX = (float)(e.NewValue);
            DataListViewScroll?.ScrollToHorizontalOffset(ScrollOffsetX);
        }

        private void VerticalScrollViewer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollOffsetY = (float)e.NewValue;
        }

        private void SkiaCanvas_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }

            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            canvas.Scale(Scale);
            canvas.Save();
            canvas.Translate(-ScrollOffsetX, -ScrollOffsetY);
            Draw(canvas);
            canvas.Restore();
        }
        private void Draw(SKCanvas canvas)
        {

            try
            {
                SkiaRenderer.Draw(canvas, ScrollOffsetX, ScrollOffsetY, RowHeight, TotalRows);
            }
            catch (Exception ex)
            {

            }

        }
        private void DataListView_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = false;

            var column = e.Column;
            var direction = column.SortDirection != ListSortDirection.Ascending
                ? SkGridViewColumnSort.Ascending
                : SkGridViewColumnSort.Descending;

            column.SortDirection = column.SortDirection != ListSortDirection.Ascending
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            foreach (var item in Columns.Where(x => x.GridViewColumnSort != SkGridViewColumnSort.None))
            {
                item.GridViewColumnSort = SkGridViewColumnSort.None;
            }
            var col = Columns.FirstOrDefault(x => x.Header == column?.Header.ToString() || x.DisplayHeader == column?.Header.ToString());
            col!.GridViewColumnSort = direction;
            ApplySort(col.BindingPath,
                      col.GridViewColumnSort == SkGridViewColumnSort.Ascending
                      ? ListSortDirection.Ascending
                      : ListSortDirection.Descending);
        }

        public void Refresh()
        {
            try
            {
                SkiaCanvas.InvalidateVisual();
            }
            catch (Exception ex)
            {

            }

        }
    }
}
