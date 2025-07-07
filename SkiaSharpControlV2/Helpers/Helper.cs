using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SkiaSharpControlV2.Helpers
{
    public static class Helper
    {
        public static SolidColorBrush GetColorBrush(string Color)
        {
            string bgcolorString = Color;
            var bgcolor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(bgcolorString);
            return new SolidColorBrush(bgcolor);
        }
    }
}
