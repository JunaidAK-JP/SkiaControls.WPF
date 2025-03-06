using SkiaSharp;
using SkiaSharpControls.Models;
using System.Collections;

namespace SkiaSharpControls.Renderer
{
    internal class SkGridRenderer : ISkGridRenderer
    {
        public SkRendererProperties RendererProperties { get; set; } = new();

        public void Draw(SKCanvas canvas, IEnumerable itemsSource, IEnumerable<SKGridViewColumn> columns, Func<object, SKColor> rowBackgroundSelector, Func<object, string, SkCellTemplate> cellTemplateSelector)
        {
            if (itemsSource == null || columns == null || !columns.Any())
                return;

            float rowHeight = 25;
            float startX = 0;
            float startY = 0;
            float currentY = startY;

            foreach (var item in itemsSource)
            {
                SKColor bgColor = rowBackgroundSelector?.Invoke(item) ?? SKColors.AliceBlue;

                var rowPaint = new SKPaint { Color = bgColor };

                // Draw background
                canvas.DrawRect(0, currentY, 1000, rowHeight, rowPaint);

                double widthSum = columns.Sum(x => x.Width);
                float currentX = startX;

                foreach (var column in columns)
                {
                    var template = cellTemplateSelector?.Invoke(item, column.Header);

                    if (template != null)
                    {
                        var cellRendererProperties = template.RendererProperties ?? RendererProperties;

                        if (template.IsToggleButton)
                        {
                            using var arrowFont = new SKFont() { Size = 12, Typeface = SKTypeface.FromFile("TypeFaces\\seguisym.ttf") };
                            canvas.DrawText(template.IsToggleButtonOn ? "\u25BC" : "\u25B6", currentX + 5, currentY + rowHeight - 5, arrowFont, cellRendererProperties.TextForeground);
                        }
                        else
                        {
                            canvas.DrawText(template.CellContent, currentX + 5, currentY + rowHeight - 5, RendererProperties.TextFont, cellRendererProperties.TextForeground);
                        }
                    }

                    canvas.DrawLine(currentX, currentY, currentX, currentY + rowHeight, RendererProperties.LineBackground);
                    currentX += (float)column.Width;
                }

                canvas.DrawLine(startX, currentY + rowHeight, (float)(startX + widthSum), currentY + rowHeight, RendererProperties.LineBackground);
                currentY += rowHeight;
            }
        }

        public void Dispose()
        {
            RendererProperties?.TextFont?.Dispose();
            RendererProperties?.TextForeground?.Dispose();
            RendererProperties?.LineBackground?.Dispose();
        }
    }
}