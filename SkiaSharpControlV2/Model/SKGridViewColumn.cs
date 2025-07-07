using SkiaSharpControlV2.Enum;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace SkiaSharpControlV2
{
    public class SkGridViewColumn : DependencyObject,INotifyPropertyChanged
    {
        public SkGridViewColumn()
        {
            
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public string? Header
        {
            get =>  (string?)GetValue(HeaderProperty);
            set { SetValue(HeaderProperty, value); OnPropertyChanged(); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(SkGridViewColumn), new PropertyMetadata(null));

        public string BindingPath
        {
            get => (string)GetValue(BindingPathProperty);
            set => SetValue(BindingPathProperty, value);
        }

        public static readonly DependencyProperty BindingPathProperty =
            DependencyProperty.Register(nameof(BindingPath), typeof(string), typeof(SkGridViewColumn), new PropertyMetadata(null));

        public string? DisplayHeader
        {
            get => (string?)GetValue(DisplayHeaderProperty);
            set => SetValue(DisplayHeaderProperty, value);
        }

        public static readonly DependencyProperty DisplayHeaderProperty =
            DependencyProperty.Register(nameof(DisplayHeader), typeof(string), typeof(SkGridViewColumn), new PropertyMetadata(null));

        public double Width
        {
            get => (double)GetValue(WidthProperty);
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register(nameof(Width), typeof(double), typeof(SkGridViewColumn), new PropertyMetadata(100.0));



        public string? BackColor
        {
            get => (string?)GetValue(BackColorProperty);
            set => SetValue(BackColorProperty, value);
        }

        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register(nameof(BackColor), typeof(string), typeof(SkGridViewColumn), new PropertyMetadata(null, (s, e) => TriggerChanged(s,e)));



        public CellContentAlignment ContentAlignment
        {
            get => (CellContentAlignment)GetValue(ContentAlignmentProperty);
            set => SetValue(ContentAlignmentProperty, value);
        }

        public static readonly DependencyProperty ContentAlignmentProperty =
            DependencyProperty.Register(nameof(ContentAlignment), typeof(CellContentAlignment), typeof(SkGridViewColumn), new PropertyMetadata(CellContentAlignment.Left));

        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register(nameof(IsVisible), typeof(bool?), typeof(SkGridViewColumn), new PropertyMetadata(true, (s, e) => TriggerChanged(s, e)));


        public bool? CanUserResize
        {
            get => (bool?)GetValue(CanUserResizeProperty);
            set => SetValue(CanUserResizeProperty, value);
        }

        public static readonly DependencyProperty CanUserResizeProperty =
            DependencyProperty.Register(nameof(CanUserResize), typeof(bool?), typeof(SkGridViewColumn), new PropertyMetadata(true, (s, e) => TriggerChanged(s, e)));

        public bool? CanUserReorder
        {
            get => (bool?)GetValue(CanUserReorderProperty);
            set => SetValue(CanUserReorderProperty, value);
        }

        public static readonly DependencyProperty CanUserReorderProperty =
            DependencyProperty.Register(nameof(CanUserReorder), typeof(bool?), typeof(SkGridViewColumn), new PropertyMetadata(true, (s, e) => TriggerChanged(s, e)));

        public bool? CanUserSort
        {
            get => (bool?)GetValue(CanUserSortProperty);
            set => SetValue(CanUserSortProperty, value);
        }

        public static readonly DependencyProperty CanUserSortProperty =
            DependencyProperty.Register(nameof(CanUserSort), typeof(bool?), typeof(SkGridViewColumn), new PropertyMetadata(true, (s, e) => TriggerChanged(s, e)));

        public SkGridViewColumnSort GridViewColumnSort
        {
            get => (SkGridViewColumnSort)GetValue(GridViewColumnSortProperty);
            set => SetValue(GridViewColumnSortProperty, value);
        }

        public static readonly DependencyProperty GridViewColumnSortProperty =
            DependencyProperty.Register(nameof(GridViewColumnSort), typeof(SkGridViewColumnSort), typeof(SkGridViewColumn), new PropertyMetadata(SkGridViewColumnSort.None, (s, e) => TriggerChanged(s, e)));


        private static void TriggerChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            if (s is SkGridViewColumn a) a.OnPropertyChanged(e.Property.ToString());
        }
    }

    public class SkGridColumnCollection : ObservableCollection<SkGridViewColumn>
    {
    }

}