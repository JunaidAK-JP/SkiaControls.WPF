using SkiaSharp;
using SkiaSharpControls.Models;
using SkiaSharpControls.Renderer;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SkiaSharpControls
{
    internal class SkGridRenderer : ISkGridRenderer
    {
        private readonly SKFont SymbolFont = new() { Size = 12, Typeface = SKTypeface.FromFile("TypeFaces\\seguisym.ttf") };
        private IEnumerable? Items { get; set; }
        private IEnumerable? SelectedItems { get; set; }
        private IEnumerable<SkGridViewColumn>? Columns { get; set; } = [];
        private Func<object, SKColor>? RowBackgroundSelector { get; set; }
        private Func<object, string, SkCellTemplate>? CellTemplateSelector { get; set; }
        private ScrollBar? HorizontalScrollViewer { get; set; }
        private ScrollBar? VerticalScrollViewer { get; set; }

        public void UpdateItems(IEnumerable items)
        {
            Items = items;
        }

        public void UpdateSelectedItems(IEnumerable selectedItems)
        {
            SelectedItems = selectedItems;
        }

        public void SetScrollBars(ScrollBar horizontalScrollViewer, ScrollBar verticalScrollViewer)
        {
            HorizontalScrollViewer = horizontalScrollViewer;
            VerticalScrollViewer = verticalScrollViewer;
        }

        public void SetColumns(IEnumerable<SkGridViewColumn> columns)
        {
            Columns = columns;
        }

        public void SetRowBackgroundSelector(Func<object, SKColor> rowBackGroundSelector)
        {
            RowBackgroundSelector = rowBackGroundSelector;
        }

        public void SetCellTemplateSelector(Func<object, string, SkCellTemplate> cellTemplateSelector)
        {
            CellTemplateSelector = cellTemplateSelector;
        }

        public void Draw(SKCanvas canvas, float scrollOffsetX, float scrollOffsetY, float rowHeight, int totalRows)
        {
            int firstVisibleRow = Math.Max(0, (int)(scrollOffsetY / rowHeight));

            int firstVisibleCol = 0;
            int visibleRowCount = Math.Min((int?)(VerticalScrollViewer?.ViewportSize / rowHeight) ?? 0, totalRows - firstVisibleRow);
            int visibleColCount = 0;

            double columnSum = scrollOffsetX;
            int columnCounter = 0;
            var visibleColumns = (Columns?.Where(x => x.Width > 0) ?? []).ToList();

            foreach (var item in visibleColumns)
            {
                columnSum -= item.Width;
                if (columnSum <= 0)
                {
                    firstVisibleCol = columnCounter;
                    break;
                }
                columnCounter++;
            }

            columnSum = 0;

            for (int i = firstVisibleCol; i < visibleColumns.Count; i++)
            {
                columnSum += visibleColumns[i].Width;
                visibleColCount = i + 1;
                if (columnSum >= HorizontalScrollViewer?.ViewportSize)
                    break;
            }

            float currentY = firstVisibleRow * rowHeight;

            //var visibleColumns = Columns.Where(x => x.Width > 0).ToList();

            for (int row = firstVisibleRow; row < firstVisibleRow + visibleRowCount; row++)
            {
                var item = Items?.Cast<object>().ElementAt(row) ?? new List<object>();
                float currentX1 = firstVisibleCol == 0 ? 0 : Columns?.Take(firstVisibleCol).Sum(x => (float)x.Width) ?? 0; // Get X position based on columns

                for (int colIndex = firstVisibleCol; colIndex < visibleColCount; colIndex++)
                {
                    float GVColumnWidth = (float)visibleColumns[colIndex].Width;

                    var template = CellTemplateSelector?.Invoke(item, visibleColumns.ElementAt(colIndex).Header);
                    string value = template?.CellContent ?? "";

                    SKColor bgColor = RowBackgroundSelector?.Invoke(item) ?? SKColors.AliceBlue;
                    using (var Fontpaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 1, IsAntialias = true })
                    using (var paint = new SKPaint { Color = bgColor, StrokeWidth = 1, IsAntialias = true })
                    {
                        Draw(canvas, colIndex, row, value, Fontpaint, paint, GVColumnWidth, currentX1, currentY, false, false, rowHeight, HighlightSelection(SelectedItems, item));
                    }

                    if (true)//show gridlines
                    {
                        using (var paint = new SKPaint { Color = SKColor.Parse("#ffffff"), StrokeWidth = 1, IsAntialias = true })
                        {
                            canvas.DrawLine(currentX1 + GVColumnWidth, currentY, currentX1 + GVColumnWidth, currentY + rowHeight, paint);
                            canvas.DrawLine(currentX1, currentY + rowHeight, currentX1 + GVColumnWidth, currentY + rowHeight, paint);
                        }
                    }
                    currentX1 += GVColumnWidth;
                }
                currentY += rowHeight;
            }
        }

        private void Draw(SKCanvas canvas, int columnsIndex, int rowIndex, string value, SKPaint fontcolor, SKPaint backColor, float width, float x, float y, bool isTextMiddle, bool isTextRight, float RowHeight, bool isselectedrow)
        {
            DrawRect(canvas, rowIndex, x, y, backColor, width, RowHeight);
            DrawText(canvas, columnsIndex, rowIndex, value, fontcolor, width, x, y, isTextMiddle, isTextRight);
        }
        private void DrawText(SKCanvas canvas, int columnsIndex, int rowIndex, string value, SKPaint fontcolor, float width, float x, float y, bool isTextMiddle, bool isTextRight)
        {
            if (width < 10) return;

            float textWidth = SymbolFont.MeasureText(value);
            int maxIterations = value.Length; // Prevent infinite loop

            while (textWidth > (width - 10) && maxIterations > 0)
            {
                value = value.Length > 1 ? value[..^1] : "";
                textWidth = SymbolFont.MeasureText(value);
                maxIterations--; // Reduce iteration count
            }

            float textX = x + 5;
            if (isTextRight)
                textX = x + (width - textWidth) - 5;
            else if (isTextMiddle)
                textX = x + (width - textWidth) / 2;

            canvas.DrawText(value, textX, y + 12, SymbolFont, fontcolor);

        }

        private void DrawRect(SKCanvas canvas, int rowIndex, float x, float y, SKPaint backColor, float width, float RowHeight)
        {
            if (true)//show gridlines
            {
                SKRect rect = new SKRect(x, y, x + width, y + RowHeight);
                canvas.DrawRect(rect, backColor);
            }
            else
            {
                SKRect rect = new SKRect(x - 1, y - 1, x + width, y + RowHeight);
                canvas.DrawRect(rect, backColor);
            }
        }

        private bool HighlightSelection(IEnumerable selectedItems, object? item)
        {
            if (selectedItems == null) return false;
            foreach (var selectedItem in selectedItems)
            {
                if (item == selectedItem)
                {
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            //
        }
    }
}