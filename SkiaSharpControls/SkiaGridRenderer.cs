using SkiaSharp;
using SkiaSharpControls.Models;
using System.Collections;

namespace SkiaSharpControls
{
    internal class SkiaGridRenderer
    {
        public void Draw(SKCanvas canvas, IEnumerable itemsSource, IEnumerable selectedItems, IEnumerable<SKListViewColumn> columns, Func<object, SKColor> rowBackgroundSelector, Func<object, string, SkiaCellTemplate> cellTemplateSelector)
        {
            if (itemsSource == null || columns == null || !columns.Any())
                return;

            float rowHeight = 25;
            float startX = 0;
            float startY = 0;
            float currentY = startY;

            using (var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 11 })
            using (var selectedPaint = new SKPaint { Color = new SKColor(200, 200, 255), IsAntialias = true })
            using (var linePaint = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1 })
            {
                foreach (var item in itemsSource)
                {
                    SKColor bgColor = rowBackgroundSelector?.Invoke(item) ?? SKColors.AliceBlue;

                    var rowPaint = new SKPaint { Color = bgColor };

                    // Draw background
                    canvas.DrawRect(0, currentY, 1000, rowHeight, rowPaint);

                    double widthSum = columns.Sum(x => x.Width);

                    if (selectedItems != null)
                        HighlightSelection(canvas, selectedItems, widthSum, rowHeight, startX, currentY, selectedPaint, item);

                    float currentX = startX;
                    var type = item.GetType();

                    foreach (var column in columns)
                    {
                        var template = cellTemplateSelector?.Invoke(item, column.PropertyName);

                        if (template.IsToggleButton)
                        {
                            var textFont = new SKFont() { Size = 12, Typeface = SKTypeface.FromFile("TypeFaces\\seguisym.ttf") };
                            canvas.DrawText(template.IsToggleButtonOn ? "\u25BC" : "\u25B6", currentX + 5, currentY + rowHeight - 5, textFont, paint);
                        }
                        else
                        {
                            var property = type.GetProperty(column.PropertyName);
                            var value = property?.GetValue(item)?.ToString() ?? "";
                            canvas.DrawText(value, currentX + 5, currentY + rowHeight - 5, paint);
                        }

                        canvas.DrawLine(currentX, currentY, currentX, currentY + rowHeight, linePaint);
                        currentX += (float)column.Width;
                    }

                    canvas.DrawLine(startX, currentY + rowHeight, (float)(startX + widthSum), currentY + rowHeight, linePaint);
                    currentY += rowHeight;
                }
            }
        }

        private static void HighlightSelection(SKCanvas canvas, IEnumerable selectedItems, double columnWidthSum, float rowHeight, float startX, float currentY, SKPaint selectedPaint, object? item)
        {
            foreach (var selectedItem in selectedItems)
            {
                if (item == selectedItem)
                {
                    canvas.DrawRect(new SKRect(startX, currentY, (float)(startX + columnWidthSum), currentY + rowHeight), selectedPaint);
                    break;
                }
            }
        }
    }
}