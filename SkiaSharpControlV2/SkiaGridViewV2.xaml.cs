
using SkiaSharp;
using SkiaSharpControlV2.Enum;
using SkiaSharpControlV2.Helpers;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        public SkiaGridViewV2()
        {
            InitializeComponent();
            //DispatcherTimer tm = new DispatcherTimer()
            //{
            //    Interval = TimeSpan.FromMilliseconds(250)
            //};
            //tm.Tick += (s, e) =>
            //{
            //    var col = Columns;
            //};
            //tm.Start();
            Loaded += (s, e) =>
            {
                UpdateColumnsInDataGrid();
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

        }
        #endregion ItemSource

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
                }
            }
            catch (Exception)
            {

            }

        }
        private void SubscribeToColumnEvents(IEnumerable<SkGridViewColumn> columns)
        {
            foreach (var col in columns)
            {
                col.PropertyChanged -= Column_PropertyChanged;
                col.PropertyChanged += Column_PropertyChanged;
            }
        }

        private void Column_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is SkGridViewColumn column)
            {
                DataGridColumn? col = DataListView.Columns.FirstOrDefault(x => x.Header == column?.Header?.ToString());
                if (col != null)
                {
                    if (e.PropertyName == nameof(SkGridViewColumn.IsVisible))
                        col.Visibility = !column.IsVisible ? Visibility.Collapsed : Visibility.Visible;
                    if (e.PropertyName == nameof(SkGridViewColumn.CanUserReorder))
                        col.CanUserResize = column.CanUserResize.Value;
                    if (e.PropertyName == nameof(SkGridViewColumn.CanUserResize))
                        col.CanUserReorder = column.CanUserReorder.Value;
                    if (e.PropertyName == nameof(SkGridViewColumn.CanUserSort))
                        col.CanUserSort = column.CanUserSort.Value;
                    if (e.PropertyName == nameof(SkGridViewColumn.GridViewColumnSort)) { }
                    if (e.PropertyName == nameof(SkGridViewColumn.BackColor))
                    {
                        
                        var newStyle = new Style(typeof(DataGridColumnHeader), col.HeaderStyle);
                        newStyle.Setters.Add(new Setter(Control.BackgroundProperty, Helper.GetColorBrush(column.BackColor ?? "#FF3F3F3F")) );

                        
                        col.HeaderStyle = newStyle;
                    }
                }

            }
        }
        #endregion PrivateMethods
    }
}
