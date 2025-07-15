
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SkiaSharpControlV2.Helpers
{
    internal static class Helper
    {
        public static SolidColorBrush GetColorBrush(string Color)
        {
            string bgcolorString = Color;
            var bgcolor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(bgcolorString);
            return new SolidColorBrush(bgcolor);
        }
        public static float GetSystemDpi()
        {
            using Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            return g.DpiX / 96.0f; // 96 DPI is the default (100% scaling)
        }
        public static ScrollViewer FindScrollViewer(DependencyObject parent)
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
        public static (string? Value, Type Type) ReadCurrentItemWithTypes(object currentItem, string propertyName)
        {
            if (currentItem == null || string.IsNullOrWhiteSpace(propertyName))
                return (null, typeof(void));

            var type = currentItem.GetType();
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop == null)
                return (null, typeof(void)); // Property not found

            object? val = prop.GetValue(currentItem);
            return (val?.ToString(), prop.PropertyType);
        }
        public static string ApplyFormat(Type type, string? value, string format, bool showBracketIfNegative = false)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            Type baseType = Nullable.GetUnderlyingType(type) ?? type;

            try
            {
                switch (Type.GetTypeCode(baseType))
                {
                    case TypeCode.Double:
                        return double.TryParse(value, out var dVal)
                            ? FormatWithBracket(dVal, format, showBracketIfNegative)
                            : $"[Invalid: {value}]";

                    case TypeCode.Decimal:
                        return decimal.TryParse(value, out var decVal)
                            ? FormatWithBracket(decVal, format, showBracketIfNegative)
                            : $"[Invalid: {value}]";

                    case TypeCode.Single:
                        return float.TryParse(value, out var fVal)
                            ? FormatWithBracket(fVal, format, showBracketIfNegative)
                            : $"[Invalid: {value}]";

                    case TypeCode.Int32:
                        return int.TryParse(value, out var iVal)
                            ? FormatWithBracket(iVal, format, showBracketIfNegative)
                            : $"[Invalid: {value}]";

                    case TypeCode.Int64:
                        return long.TryParse(value, out var lVal)
                            ? FormatWithBracket(lVal, format, showBracketIfNegative)
                            : $"[Invalid: {value}]";

                    case TypeCode.DateTime:
                        return DateTime.TryParse(value, out var dtVal)
                            ? dtVal.ToString(format)
                            : $"[Invalid: {value}]";
                }

                if (baseType == typeof(TimeSpan))
                {
                    return TimeSpan.TryParse(value, out var tsVal)
                        ? tsVal.ToString(format)
                        : $"[Invalid: {value}]";
                }

                return value;
            }
            catch
            {
                return $"[Invalid: {value}]";
            }
        }
        private static string FormatWithBracket<T>(T number, string format, bool showBracket) where T : struct, IComparable
        {
            decimal val = Convert.ToDecimal(number);
            string formatted = string.IsNullOrWhiteSpace(format) ? val.ToString() : val.ToString(format);

            return showBracket && val < 0 ? $"({formatted.TrimStart('-')})" : formatted;
        }

    }

}
