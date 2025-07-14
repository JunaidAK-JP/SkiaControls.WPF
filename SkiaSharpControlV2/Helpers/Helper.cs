
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
    }

}
