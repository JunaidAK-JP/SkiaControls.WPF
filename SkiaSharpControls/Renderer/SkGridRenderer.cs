using SkiaSharp;
using SkiaSharpControls.Models;
using SkiaSharpControls.Renderer;
using System.Collections;
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
        private Func<object, SKColor>? RowBorderSelector { get; set; }
        private Func<object, string, SkCellTemplate>? CellTemplateSelector { get; set; }
        private ScrollBar? HorizontalScrollViewer { get; set; }
        private ScrollBar? VerticalScrollViewer { get; set; }
        private bool ShowGridLines { get; set; }
         
        private readonly List<SkGridViewColumn> _visibleColumnsCache = new();

        private SKPaint SelectedRowBackgroundHighlighting = new SKPaint() { Color = SKColors.Blue,IsAntialias = true};
        private SKPaint DefaultLinePaint = new SKPaint { Color = SKColor.Parse("#ffffff"), StrokeWidth = 1 };
        private SKPaint DefaultTextForegroundPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 1 };

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

        public void SetRowBorderSelector(Func<object, SKColor> rowBorderSelector)
        {
            RowBorderSelector = rowBorderSelector;
        }

        public void SetCellTemplateSelector(Func<object, string, SkCellTemplate> cellTemplateSelector)
        {
            CellTemplateSelector = cellTemplateSelector;
        }

        public void SetGridLinesVisibility(bool showGridLines)
        {
            ShowGridLines = showGridLines;
        }

        public void Draw(SKCanvas canvas, float scrollOffsetX, float scrollOffsetY, float rowHeight, int totalRows)
        {
            int firstVisibleRow = Math.Max(0, (int)(scrollOffsetY / rowHeight));

            int firstVisibleCol = 0;
            int visibleRowCount = Math.Min((int?)(VerticalScrollViewer?.ViewportSize / rowHeight) ?? 0, totalRows - firstVisibleRow);
            int visibleColCount = 0;

            double columnSum = scrollOffsetX;
            int columnCounter = 0;
            UpdateVisibleColumns();
            var visibleColumns = _visibleColumnsCache;//(?.Where(x => x.Width > 0) ?? []).ToList();



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

            for (int row = firstVisibleRow; row < firstVisibleRow + visibleRowCount; row++)
            {
                var item = Items?.Cast<object>().ElementAt(row) ?? new List<object>();
                // float currentX = firstVisibleCol == 0 ? 0 : Columns?.Take(firstVisibleCol).Sum(x => (float)x.Width) ?? 0; // Get X position based on columns
                float currentX = 0;

                var columnList = Columns as IList<SkGridViewColumn> ?? Columns.ToList();

                for (int i = 0; i < firstVisibleCol && i < columnList.Count; i++)
                {
                    currentX += (float)columnList[i].Width;
                }
                for (int colIndex = firstVisibleCol; colIndex < visibleColCount; colIndex++)
                {
                    float GVColumnWidth = (float)visibleColumns[colIndex].Width;

                    var template = CellTemplateSelector?.Invoke(item, visibleColumns.ElementAt(colIndex).Header);
                    string value = template?.CellContent ?? "";
                    var fontPaint = template?.RendererProperties?.TextForeground ?? DefaultTextForegroundPaint;
                    var textFont = template?.RendererProperties?.TextFont ?? SymbolFont;
                    var lineColor = template?.RendererProperties?.LineBackground ?? DefaultLinePaint;

                    fontPaint.IsAntialias = true;
                    lineColor.IsAntialias = true;

                    SKColor bgColor = template?.RendererProperties?.BackgroundBrush?.Color ?? RowBackgroundSelector?.Invoke(item) ?? SKColors.AliceBlue;

                    using (var paint = new SKPaint { Color = bgColor, StrokeWidth = 1, IsAntialias = true })
                    {
                        Draw(canvas, colIndex, row, value, fontPaint, textFont, paint, template?.RendererProperties?.BorderBrush, GVColumnWidth, currentX, currentY, false, false, rowHeight, HighlightSelected(item));

                        if (template?.CustomDrawing != null)
                        {
                            template.CustomDrawing.Invoke(canvas, currentX, currentY);
                        }
                    }

                    if (ShowGridLines)
                    {
                        canvas.DrawLine(currentX + GVColumnWidth, currentY, currentX + GVColumnWidth, currentY + rowHeight, lineColor);
                        canvas.DrawLine(currentX, currentY + rowHeight, currentX + GVColumnWidth, currentY + rowHeight, lineColor);
                    }

                    currentX += GVColumnWidth;
                }

                if (RowBorderSelector != null)
                {
                    using (var paint = new SKPaint { Color = RowBorderSelector.Invoke(item), StrokeWidth = 1, IsAntialias = true })
                    {
                        DrawBorder(canvas, paint, (float)columnSum, 0, currentY, rowHeight);
                    }
                }

                currentY += rowHeight;
            }
        }
        void UpdateVisibleColumns()
        {
            _visibleColumnsCache.Clear();

            if (Columns == null) return;

            foreach (var col in Columns)
            {
                if (col.Width > 0)
                    _visibleColumnsCache.Add(col);
            }
        }
        private void Draw(SKCanvas canvas, int columnsIndex, int rowIndex, string value, SKPaint fontcolor, SKFont textFont, SKPaint backColor, SKPaint? borderColor, float width, float x, float y, bool isTextMiddle, bool isTextRight, float rowHeight, bool isselectedrow)
        {
            
            var rowBackColor = isselectedrow ? SelectedRowBackgroundHighlighting : backColor;
            DrawRect(canvas, rowIndex, x, y, rowBackColor, width, rowHeight);


            if (borderColor != null)
            {
                DrawBorder(canvas, borderColor, width, x, y, rowHeight);
            }

            DrawText(canvas, columnsIndex, rowIndex, value, fontcolor, textFont, width, x, y, isTextMiddle, isTextRight);
        }

        //private static void DrawBorder(SKCanvas canvas, SKPaint? borderColor, float width, float x, float y, float rowHeight)
        //{
        //    canvas.DrawLine(x + 1, y, x + 1, y + rowHeight, borderColor);
        //    canvas.DrawLine(x + width - 2, y, x + width - 2, y + rowHeight, borderColor);
        //    canvas.DrawLine(x, y + rowHeight - 1, x + width, y + rowHeight - 1, borderColor);
        //    canvas.DrawLine(x, y, x + width, y, borderColor);
        //}
        private static void DrawBorder(SKCanvas canvas, SKPaint borderPaint, float width, float x, float y, float rowHeight)
        {
            if (borderPaint.StrokeWidth <= 0 || width <= 0 || rowHeight <= 0)
                return;

            float left = x;
            float top = y;
            float right = x + width;
            float bottom = y + rowHeight;

            canvas.DrawRect(SKRect.Create(left, top, width, rowHeight), borderPaint);
        }

        //private void DrawText(SKCanvas canvas, int columnsIndex, int rowIndex, string value, SKPaint fontcolor, SKFont textFont, float width, float x, float y, bool isTextMiddle, bool isTextRight)
        //{
        //    if (width < 10) return;

        //    float textWidth = textFont.MeasureText(value);
        //    int maxIterations = value.Length; // Prevent infinite loop

        //    while (textWidth > (width - 10) && maxIterations > 0)
        //    {
        //        value = value.Length > 1 ? value[..^1] : "";
        //        textWidth = textFont.MeasureText(value);
        //        maxIterations--; // Reduce iteration count
        //    }

        //    float textX = x + 5;
        //    if (isTextRight)
        //        textX = x + (width - textWidth) - 5;
        //    else if (isTextMiddle)
        //        textX = x + (width - textWidth) / 2;

        //    canvas.DrawText(value, textX, y + 12, textFont, fontcolor);

        //}



        private void DrawText(SKCanvas canvas, int columnsIndex, int rowIndex, string value, SKPaint fontColor, SKFont textFont, float width, float x, float y, bool isTextMiddle, bool isTextRight)
        {
            if (width < 10 || string.IsNullOrEmpty(value))
                return;

            float maxTextWidth = width - 10;
            ReadOnlySpan<char> span = value;

            // Binary search trimming instead of one-by-one
            int left = 0;
            int right = span.Length;
            int fitLength = span.Length;

            while (left < right)
            {
                int mid = (left + right) / 2;
                var testSpan = span.Slice(0, mid);
                float testWidth = textFont.MeasureText(testSpan, out _);

                if (testWidth <= maxTextWidth)
                {
                    fitLength = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid;
                }
            }

            var finalText = span.Slice(0, fitLength).ToString(); // final trimmed string
            float finalWidth = textFont.MeasureText(finalText, out _);

            float textX = x + 5;
            if (isTextRight)
                textX = x + width - finalWidth - 5;
            else if (isTextMiddle)
                textX = x + (width - finalWidth) / 2;

            // You may want to adjust `y + 12` if font size varies
            canvas.DrawText(finalText, textX, y + 12, textFont, fontColor);
        }

        //private void DrawRect(SKCanvas canvas, int rowIndex, float x, float y, SKPaint backColor, float width, float RowHeight)
        //{
        //    if (ShowGridLines)
        //    {
        //        SKRect rect = new SKRect(x, y, x + width, y + RowHeight);
        //        canvas.DrawRect(rect, backColor);
        //    }
        //    else
        //    {
        //        SKRect rect = new SKRect(x - 1, y - 1, x + width, y + RowHeight);
        //        canvas.DrawRect(rect, backColor);
        //    }
        //}
        private void DrawRect(SKCanvas canvas, int rowIndex, float x, float y, SKPaint backColor, float width, float rowHeight)
        {
            float left = ShowGridLines ? x : x - 1;
            float top = ShowGridLines ? y : y - 1;
            float right = x + width;
            float bottom = y + rowHeight;

            canvas.DrawRect(SKRect.Create(left, top, right - left, bottom - top), backColor);
        }

        private bool HighlightSelected(object? item)
        {
            if (SelectedItems == null) 
                return false;

            foreach (var selectedItem in SelectedItems)
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