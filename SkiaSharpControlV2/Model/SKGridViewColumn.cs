using SkiaSharpControlV2.Enum;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SkiaSharpControlV2
{
    public class SkGridViewColumn : DependencyObject, INotifyPropertyChanged
    {
        public SkGridViewColumn()
        {

        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public string? Header
        {
            get => (string?)GetValue(HeaderProperty);
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
            DependencyProperty.Register(nameof(BackColor), typeof(string), typeof(SkGridViewColumn), new PropertyMetadata(null, (s, e) => TriggerChanged(s, e)));



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

        public SKCellTemplate? CellTemplate
        {
            get => (SKCellTemplate?)GetValue(CellTemplateProperty);
            set => SetValue(CellTemplateProperty, value);
        }
        public static readonly DependencyProperty CellTemplateProperty =
            DependencyProperty.Register(nameof(CellTemplate), typeof(SKCellTemplate), typeof(SkGridViewColumn), new PropertyMetadata(null));
        private static void TriggerChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            if (s is SkGridViewColumn a) a.OnPropertyChanged(e.Property.ToString());
        }
    }
    public class SKSetter
    {
        public string Property { get; set; }
        public object Value { get; set; }
    }
    public class SKCondition
    {
        public string Binding { get; set; }
        public string Operator { get; set; } = "=";
        public object Value { get; set; }
    }

    public abstract class SKTrigger
    {
        public ObservableCollection<SKSetter> Setters { get; set; } = new();
    }
    public class SKDataTrigger : SKTrigger
    {
        public string Binding { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
    }

    public class SKMultiTrigger : SKTrigger
    {
        public ObservableCollection<SKCondition> Conditions { get; set; } = new();
    }

    [ContentProperty(nameof(Triggers))]
    public class SKCellTemplate
    {
        public string? Format { get; set; }
        public string? TextColor { get; set; }
        public ObservableCollection<SKTrigger> Triggers { get; set; } = new(); // Can be SKDataTrigger or SKMultiTrigger
    }

    //public class SkDataTemplate  : DependencyObject , INotifyPropertyChanged
    //{
    //    public string? BackColor
    //    {
    //        get => (string?)GetValue(BackColorProperty);
    //        set => SetValue(BackColorProperty, value);
    //    }
    //    public static readonly DependencyProperty BackColorProperty =
    //        DependencyProperty.Register(nameof(BackColor), typeof(string), typeof(SkGridViewColumn), new PropertyMetadata(null, (s, e) => TriggerChanged(s, e)));

    //    public string? Format
    //    {
    //        get => (string?)GetValue(FormatProperty);
    //        set => SetValue(FormatProperty, value);
    //    }
    //    public static readonly DependencyProperty FormatProperty =
    //        DependencyProperty.Register(nameof(Format), typeof(string), typeof(SkGridViewColumn), new PropertyMetadata(null, (s, e) => TriggerChanged(s, e)));

    //    public string? BorderColor
    //    {
    //        get => (string?)GetValue(BorderColorProperty);
    //        set => SetValue(BorderColorProperty, value);
    //    }
    //    public static readonly DependencyProperty BorderColorProperty =
    //        DependencyProperty.Register(nameof(BorderColor), typeof(string), typeof(SkGridViewColumn), new PropertyMetadata(null, (s, e) => TriggerChanged(s, e)));

    //    public bool ShowBorder
    //    {
    //        get => (bool)GetValue(ShowBorderProperty);
    //        set => SetValue(ShowBorderProperty, value);
    //    }
    //    public static readonly DependencyProperty ShowBorderProperty =
    //        DependencyProperty.Register(nameof(ShowBorder), typeof(bool), typeof(SkGridViewColumn), new PropertyMetadata(false, (s, e) => TriggerChanged(s, e)));

    //    public event PropertyChangedEventHandler? PropertyChanged;

    //    private void OnPropertyChanged([CallerMemberName] string? prop = null)
    //        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    //    private static void TriggerChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (s is SkDataTemplate a) a.OnPropertyChanged(e.Property.ToString());
    //    }
    //}
    public class SkGridColumnCollection : ObservableCollection<SkGridViewColumn>
    {
    }
    // ============================
    // 🎯 Trigger Evaluation Logic
    // ============================
    public static class TriggerEvaluator
    {
        //bool Evaluate(SKCondition cond, object rowData)
        //{
        //    var value = ReflectionHelper.GetPropertyValue(rowData, cond.Binding);
        //    return cond.Operator switch
        //    {
        //        SKConditionOperator.Equal => Equals(value, cond.Value),
        //        SKConditionOperator.GreaterThan => Convert.ToDouble(value) > Convert.ToDouble(cond.Value),
        //        SKConditionOperator.LessThan => Convert.ToDouble(value) < Convert.ToDouble(cond.Value),
        //        SKConditionOperator.NotEqual => !Equals(value, cond.Value),
        //        SKConditionOperator.GreaterThanOrEqual => Convert.ToDouble(value) >= Convert.ToDouble(cond.Value),
        //        SKConditionOperator.LessThanOrEqual => Convert.ToDouble(value) <= Convert.ToDouble(cond.Value),
        //        _ => false
        //    };
        //}
        //public static bool EvaluateTrigger(object item, SKCellTrigger trigger)
        //{
        //    if (trigger is SKDataTrigger dt)
        //    {
        //        var val = GetBoundValue(item, dt.Binding);
        //        return EvaluateCondition(val, dt.Condition, dt.Value);
        //    }
        //    else if (trigger is And at)
        //    {
        //        return at.Conditions.All(c => EvaluateCondition(GetBoundValue(item, c.Binding), c.ConditionType, c.Value));
        //    }
        //    else if (trigger is Or ot)
        //    {
        //        return ot.Conditions.Any(c => EvaluateCondition(GetBoundValue(item, c.Binding), c.ConditionType, c.Value));
        //    }
        //    return false;
        //}

        public static object? GetBoundValue(object dataContext, string path)
        {
            if (dataContext == null || string.IsNullOrWhiteSpace(path)) return null;
            var props = path.Split('.');
            object? current = dataContext;
            foreach (var prop in props)
            {
                if (current == null) return null;
                var propInfo = current.GetType().GetProperty(prop);
                if (propInfo == null) return null;
                current = propInfo.GetValue(current);
            }
            return current;
        }

        public static bool EvaluateCondition(object? actualValue, string op, object expectedValue)
        {
            try
            {
                double actual = Convert.ToDouble(actualValue);
                double expected = Convert.ToDouble(expectedValue);

                return op switch
                {
                    ">" => actual > expected,
                    "<" => actual < expected,
                    ">=" => actual >= expected,
                    "<=" => actual <= expected,
                    "=" or "==" => actual == expected,
                    "!=" => actual != expected,
                    _ => false
                };
            }
            catch
            {
                return actualValue?.ToString() == expectedValue?.ToString();
            }
        }
    }
}