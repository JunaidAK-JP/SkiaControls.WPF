﻿using SkiaSharp;
using SkiaSharpControls.Models;
using System.Collections;
using System.Collections.ObjectModel;
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
                renderer.SetGridLinesVisibility(ShowGridLines);

                UpdateValues();
                SkiaCanvas.Focus();
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



        public Action<double> HorizontalScrollBarPositionChanged
        {
            get { return (Action<double>)GetValue(HorizontalScrollBarPositionChangedProperty); }
            set { SetValue(HorizontalScrollBarPositionChangedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnItemClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalScrollBarPositionChangedProperty =
            DependencyProperty.Register(nameof(HorizontalScrollBarPositionChanged), typeof(Action<double>), typeof(SkGridView), new PropertyMetadata(default));

        public Action<double> VerticalScrollBarPositionChanged
        {
            get { return (Action<double>)GetValue(VerticalScrollBarPositionChangedProperty); }
            set { SetValue(VerticalScrollBarPositionChangedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnItemClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalScrollBarPositionChangedProperty =
            DependencyProperty.Register(nameof(VerticalScrollBarPositionChanged), typeof(Action<double>), typeof(SkGridView), new PropertyMetadata(default));

        public Action<object> OnRowRightClicked
        {
            get { return (Action<object>)GetValue(OnRowRightClickedProperty); }
            set { SetValue(OnRowRightClickedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnItemClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnRowRightClickedProperty =
            DependencyProperty.Register(nameof(OnRowRightClicked), typeof(Action<object>), typeof(SkGridView), new PropertyMetadata(default));

        public Action<object> OnRowDoubleClicked
        {
            get { return (Action<object>)GetValue(OnRowDoubleClickedProperty); }
            set { SetValue(OnRowDoubleClickedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnItemClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnRowDoubleClickedProperty =
            DependencyProperty.Register(nameof(OnRowDoubleClicked), typeof(Action<object>), typeof(SkGridView), new PropertyMetadata(default));

        public Action OnSkGridDoubleClicked
        {
            get { return (Action)GetValue(OnSkGridDoubleClickedProperty); }
            set { SetValue(OnSkGridDoubleClickedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnItemClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnSkGridDoubleClickedProperty =
            DependencyProperty.Register(nameof(OnSkGridDoubleClicked), typeof(Action), typeof(SkGridView), new PropertyMetadata(default));

        public Action<Key> OnPreviewKeyDownEvent
        {
            get { return (Action<Key>)GetValue(OnPreviewKeyDownEventProperty); }
            set { SetValue(OnPreviewKeyDownEventProperty, value); }
        }

        public static readonly DependencyProperty OnPreviewKeyDownEventProperty =
            DependencyProperty.Register(nameof(OnPreviewKeyDownEvent), typeof(Action<Key>), typeof(SkGridView), new PropertyMetadata(default));

        public Action<object, string> OnCellClicked
        {
            get { return (Action<object, string>)GetValue(OnCellClickedProperty); }
            set { SetValue(OnCellClickedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnItemClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnCellClickedProperty =
            DependencyProperty.Register(nameof(OnCellClicked), typeof(Action<object, string>), typeof(SkGridView), new PropertyMetadata(default));

        public ContextMenu HeaderContextMenu
        {
            get { return (ContextMenu)GetValue(HeaderContextMenuProperty); }
            set { SetValue(HeaderContextMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderContextMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderContextMenuProperty =
            DependencyProperty.Register(nameof(HeaderContextMenu), typeof(ContextMenu), typeof(SkGridView), new PropertyMetadata(default, OnHeaderContextMenuChanged));

        private static void OnHeaderContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView)
            {
                skGridView.DataListView.ContextMenu = e.NewValue as ContextMenu;
            }
        }

        public bool ColumnHeaderVisible
        {
            get { return (bool)GetValue(ColumnHeaderVisibleProperty); }
            set { SetValue(ColumnHeaderVisibleProperty, value); }
        }

        public static readonly DependencyProperty ColumnHeaderVisibleProperty =
            DependencyProperty.Register(
                nameof(ColumnHeaderVisible),
                typeof(bool),
                typeof(SkGridView),
                new PropertyMetadata(true, OnColumnHeaderVisibleChanged));

        private static void OnColumnHeaderVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView)
            {
                skGridView.SKGridColumnHeader.Height = new GridLength((bool)e.NewValue ? 21 : 0);
                skGridView.DataListView.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public bool HorizontalScrollBarVisible
        {
            get { return (bool)GetValue(HorizontalScrollBarVisibleProperty); }
            set { SetValue(HorizontalScrollBarVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HorizontalScrollBarVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalScrollBarVisibleProperty =
            DependencyProperty.Register(
                nameof(HorizontalScrollBarVisible),
                typeof(bool),
                typeof(SkGridView),
                new PropertyMetadata(true, OnHorizontalScrollBarVisibilityChanged));

        private static void OnHorizontalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView)
            {
                skGridView.HorizontalScrollViewer.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool VerticalScrollBarVisible
        {
            get { return (bool)GetValue(VerticalScrollBarVisibleProperty); }
            set { SetValue(VerticalScrollBarVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalScrollBarVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalScrollBarVisibleProperty =
            DependencyProperty.Register(
                nameof(VerticalScrollBarVisible),
                typeof(bool),
                typeof(SkGridView),
                new PropertyMetadata(true, OnVerticalScrollBarVisibilityChanged));

        private static void OnVerticalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView)
            {
                skGridView.VerticalScrollViewer.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ContextMenu ContextMenu
        {
            get { return (ContextMenu)GetValue(ContextMenuProperty); }
            set { SetValue(ContextMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderContextMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextMenuProperty =
            DependencyProperty.Register(nameof(ContextMenu), typeof(ContextMenu), typeof(SkGridView), new PropertyMetadata(default, OnContextMenuChanged));

        private static void OnContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView)
            {
                skGridView.skiaContainer.ContextMenu = e.NewValue as ContextMenu;
            }
        }

        public ContextMenu ItemsContextMenu
        {
            get { return (ContextMenu)GetValue(ItemsContextMenuProperty); }
            set { SetValue(ItemsContextMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsContextMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsContextMenuProperty =
            DependencyProperty.Register(nameof(ItemsContextMenu), typeof(ContextMenu), typeof(SkGridView), new PropertyMetadata(default, OnItemsContextMenuChanged));

        private static void OnItemsContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView)
            {
                skGridView.SkiaCanvas.ContextMenu = e.NewValue as ContextMenu;
            }
        }

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
                skGridView.ColumnsChanged?.Invoke();
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

        public Action ColumnsChanged
        {
            get { return (Action)GetValue(ColumnsChangedProperty); }
            set { SetValue(ColumnsChangedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnsChanged.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsChangedProperty =
            DependencyProperty.Register(nameof(ColumnsChanged), typeof(Action), typeof(SkGridView), new PropertyMetadata(default));

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


        public ObservableCollection<object> SelectedItems
        {
            get { return (ObservableCollection<object>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(ObservableCollection<object>), typeof(SkGridView), new PropertyMetadata(default, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView && e.NewValue is ObservableCollection<object>)
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

        public Func<object, SKColor> RowBorderSelector
        {
            get => (Func<object, SKColor>)GetValue(RowBorderSelectorProperty);
            set => SetValue(RowBorderSelectorProperty, value);
        }

        public static readonly DependencyProperty RowBorderSelectorProperty =
            DependencyProperty.Register(nameof(RowBorderSelector), typeof(Func<object, SKColor>), typeof(SkGridView), new PropertyMetadata(default, OnRowBorderTemplateChanged));

        private static void OnRowBorderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView && e.NewValue is Func<object, SKColor> template)
            {
                skGridView.renderer.SetRowBorderSelector(template);
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

        public bool ShowGridLines
        {
            get { return (bool)GetValue(ShowGridLinesProperty); }
            set { SetValue(ShowGridLinesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowGridLines.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowGridLinesProperty =
            DependencyProperty.Register(nameof(ShowGridLines), typeof(bool), typeof(SkGridView), new PropertyMetadata(true, OnShowGridLinesChanged));

        private static void OnShowGridLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SkGridView skGridView && e.NewValue is bool showGridLines)
            {
                skGridView.renderer.SetGridLinesVisibility(showGridLines);
            }
        }

        private Dictionary<UIElement, (bool removeOnLostFocus, bool removeOnReturn)> AddedWpfElements { get; set; } = [];

        public void AddWpfElement(FrameworkElement element, bool removeOnLostFocus = true, bool removeOnReturn = true)
        {
            VerticalScrollViewer.Value = VerticalScrollViewer.ViewportSize;

            if (VerticalScrollViewer.Track.Visibility != Visibility.Visible)
            {
                element.Margin = new Thickness(0, 0, 0, -RowHeight);
            }

            if (!AddedWpfElements.TryAdd(element, (removeOnLostFocus, removeOnReturn)))
                AddedWpfElements[element] = (removeOnLostFocus, removeOnReturn);


            skiaContainer.Children.Add(element);

            if (element is TextBox)
                element.Focus();

            element.KeyDown -= RemoveWpfElementOnEnterPressed(element);

            if (removeOnReturn)
            {
                element.KeyDown += RemoveWpfElementOnEnterPressed(element);
            }

            element.IsKeyboardFocusWithinChanged -= RemoveWpfElementOnLostFocus(element);

            if (removeOnLostFocus)
            {
                element.IsKeyboardFocusWithinChanged += RemoveWpfElementOnLostFocus(element);
            }
        }

        private DependencyPropertyChangedEventHandler RemoveWpfElementOnLostFocus(FrameworkElement element)
        {
            return (obj, e) =>
            {
                if (e.NewValue is bool isFocussed && !isFocussed)
                {
                    skiaContainer.Children.Remove(element);
                    AddedWpfElements.Remove(element);
                }
            };
        }

        private KeyEventHandler RemoveWpfElementOnEnterPressed(FrameworkElement element)
        {
            return (obj, e) =>
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    skiaContainer.Children.Remove(element);
                    AddedWpfElements.Remove(element);
                }
            };
        }

        private void RemoveWpfElements()
        {
            if (AddedWpfElements.Count > 0)
            {
                foreach (var item in AddedWpfElements.Where(x => x.Value.removeOnLostFocus))
                {
                    skiaContainer.Children.Remove(item.Key);
                    AddedWpfElements.Remove(item.Key);
                }
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
            if (ItemsSource.Cast<object>().Count() == 0)
                return;
            var point = e.GetPosition(SkiaCanvas);

            int rowIndex = (int)((point.Y + ScrollOffsetY) / RowHeight);
            double x = point.X;
            int clickedColumnIndex = (int)((point.X + ScrollOffsetX));
            var s = new List<dynamic>((IEnumerable<dynamic>)ItemsSource);

            if (SelectedItems == null)
                SelectedItems = new();


            if (rowIndex > s.Count - 1)
                return;

            foreach (var item in GV.Columns)
            {
                x -= item.Width;
                if (x <= 0)
                {
                    OnCellClicked?.Invoke(s[rowIndex], item.Header?.ToString());
                    break;
                }
            }
            if (e.LeftButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                if (SelectedItems.Any(x => x.Equals(s[rowIndex])))
                {
                    SelectedItems.Remove(s[rowIndex]);
                }
                else
                {
                    SelectedItems.Add(s[rowIndex]);
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                SelectedItems.Clear();
                SelectedItems.Add(s[rowIndex]);
                OnRowClicked?.Invoke(s[rowIndex]);
            }
            if (e.ClickCount == 2)
                OnRowDoubleClicked?.Invoke(s[rowIndex]);
            SkiaCanvas.InvalidateVisual();
            SkiaCanvas.Focus();
        }
        private void SkiaCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemsSource.Cast<object>().Count() == 0)
                return;
            // Get mouse position relative to SKElement
            var point = e.GetPosition(SkiaCanvas);

            int rowIndex = (int)((point.Y + ScrollOffsetY) / RowHeight);
            double x = point.X;
            int clickedColumnIndex = (int)((point.X + ScrollOffsetX));

            var s = new List<dynamic>((IEnumerable<dynamic>)ItemsSource);
            if (rowIndex > s.Count - 1)
                return;



            foreach (var item in GV.Columns)
            {
                x -= item.Width;
                if (x <= 0)
                {
                    OnCellClicked?.Invoke(s[rowIndex], item.Header?.ToString());
                    break;
                }
            }

            if (e.RightButton == MouseButtonState.Pressed)
                OnRowRightClicked?.Invoke(s[rowIndex]);

            RemoveWpfElements();
            SkiaCanvas.Focus();
        }
        private void OnDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            RemoveWpfElements();
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


        private void VerticalScrollViewer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RemoveWpfElements();
            ScrollOffsetY = (float)e.NewValue;
            VerticalScrollBarPositionChanged?.Invoke(e.NewValue);
            SkiaCanvas.InvalidateVisual();
        }
        private void HorizontalScrollViewer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollOffsetX = (float)(e.NewValue);
            DataListViewScroll.ScrollToHorizontalOffset(ScrollOffsetX);
            HorizontalScrollBarPositionChanged?.Invoke(e.NewValue);
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
            HorizontalScrollViewer.Maximum = TotalColsVisible.Sum() - MainGrid.ActualWidth + 10;

            VerticalScrollViewer.Minimum = 0;
            VerticalScrollViewer.ViewportSize = MainGrid.ActualHeight;
            VerticalScrollViewer.Maximum = ((TotalRows + 3.3) * RowHeight) - MainGrid.ActualHeight;

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

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SkiaCanvas.Focus();
            RemoveWpfElements();
        }

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

        public void ScrollToHorizontalOffset(double offset)
        {
            HorizontalScrollViewer.Value = offset;
        }

        private void SkiaCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            OnPreviewKeyDownEvent.Invoke(e.Key);
        }

        private void SkiaCanvas_LostFocus(object sender, RoutedEventArgs e)
        {
            if(SelectedItems.Count > 0)
            {
                SelectedItems?.Clear();
                SkiaCanvas.InvalidateVisual();
            }
        }

        private void skiaContainer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not SkiaSharp.Views.WPF.SKElement)
            {
                if (SelectedItems.Count > 0 && !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    SelectedItems.Clear();
                    SkiaCanvas.InvalidateVisual();
                }
                if (e.ClickCount == 2)
                {
                    OnSkGridDoubleClicked?.Invoke();
                }
            }
        }

        public void ScrollToVerticalOffset(double offset)
        {
            VerticalScrollViewer.Value = offset;
        }
    }
}