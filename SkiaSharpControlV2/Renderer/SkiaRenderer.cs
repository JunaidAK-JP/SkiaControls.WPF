
using SkiaSharp;
using SkiaSharpControlV2.Enum;
using SkiaSharpControlV2.Helpers;
using SkiaSharpControlV2.Model;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace SkiaSharpControlV2.Renderer
{
    public class SkiaRenderer
    {
        public SkiaRenderer(SkiaGridViewV2 CurrentContext)
        {
            this.CurrentContext = CurrentContext;
        }
        private SkiaGridViewV2 CurrentContext;
        private ReflectionHelper reflectionHelper = new();
        private SKFont SymbolFont { get; set; } = new() { Size = 12, Typeface = SKTypeface.FromFile("TypeFaces\\seguisym.ttf") };
        private ICollectionView? Items { get; set; }
        private IEnumerable? SelectedItems { get; set; }
        private IEnumerable<SKGridViewColumn>? Columns { get; set; } = [];
        private SKGroupDefinition? Group { get; set; }
        private ScrollBar? HorizontalScrollViewer { get; set; }
        private ScrollBar? VerticalScrollViewer { get; set; }
        private bool ShowGridLines { get; set; }



        private readonly List<SKGridViewColumn> _visibleColumnsCache = new();
        private SKPaint SelectedRowBackgroundHighlighting = new SKPaint() { Color = SKColor.Parse("#0072C6"), IsAntialias = true };
        private SKPaint SelectedRowTextColor = new SKPaint { Color = SKColors.White, StrokeWidth = 1, IsAntialias = true };
        private SKPaint GridLineColor = new SKPaint { Color = SKColors.Black, StrokeWidth = 1, IsAntialias = true };
        private SKPaint FontColor = new SKPaint { Color = SKColors.Black, StrokeWidth = 1, IsAntialias = true };
        private SKPaint RowBackgroundColor = new SKPaint { Color = SKColors.White, StrokeWidth = 1, IsAntialias = true };
        private SKPaint GroupRowBackgroundColor = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1, IsAntialias = true };
        private SKPaint GroupFontColor = new SKPaint { Color = SKColors.White, StrokeWidth = 1, IsAntialias = true };
        private SKPaint AlternatingRowBackground = null;


        private SKPaint CellBackgroundColor = new SKPaint { Color = SKColors.White, StrokeWidth = 1, IsAntialias = true };
        private SKPaint CellBorderColor = new SKPaint { Color = SKColors.Green, StrokeWidth = 1, IsAntialias = true };

        public List<GroupModel> GroupItemSource { get; set; } = new();
        public void UpdateItems(ICollectionView items)
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

        public void SetColumns(IEnumerable<SKGridViewColumn> columns)
        {
            Columns = columns;
        }
        public void SetGroup(SKGroupDefinition? group)
        {
            Group = group;
        }

        public void SetFont(string fontName, double fontSize)
        {
            SymbolFont.Typeface = SKTypeface.FromFamilyName(fontName);
            SymbolFont.Size = (float)fontSize;
        }
        public void SetGridLinesVisibility(bool showGridLines)
        {
            ShowGridLines = showGridLines;
        }
        public void SetGridLinesColor(string color)
        {
            GridLineColor.Color = SKColor.Parse(color);
        }
        public void SetGroupRowBackgroundColor(string? color)
        {
            if (color != null)
                GroupRowBackgroundColor.Color = SKColor.Parse(color);
        }
        public void SetGroupFontColor(string? color)
        {
            if (color != null)
                GroupFontColor.Color = SKColor.Parse(color);
        }
        public void SetForeground(string color)
        {
            FontColor.Color = SKColor.Parse(color);
        }
        public void SetRowBackgroundColor(string color)
        {
            RowBackgroundColor.Color = SKColor.Parse(color);
        }
        public void SetAlternatingRowBackground(string color)
        {
            if (color == null)
                AlternatingRowBackground = null;
            else
                AlternatingRowBackground = new SKPaint { Color = SKColor.Parse(color), StrokeWidth = 1, IsAntialias = true };
        }
        public void Draw(SKCanvas canvas, float scrollOffsetX, float scrollOffsetY, float rowHeight, int totalRows)
        {
            int firstVisibleRow = Math.Max(0, (int)(scrollOffsetY / rowHeight));

            int firstVisibleCol = 0;
            int visibleRowCount = Math.Min((int?)(VerticalScrollViewer?.ViewportSize / rowHeight) ?? 0, totalRows - firstVisibleRow);
            int visibleColCount = 0;

            double columnSum = scrollOffsetX;
            int columnCounter = 0;

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
            if ((firstVisibleRow + visibleRowCount) > totalRows)
                return;

            float currentY = firstVisibleRow * rowHeight;
            List<GroupModel>? GroupItems = null;
            IEnumerator<object> items = Items?.Cast<object>().Skip(firstVisibleRow).Take(visibleRowCount).GetEnumerator();
            items?.MoveNext();
            if (Group != null)
            {
                GroupItems = GroupItemSource.Where(x => x.IsGroupHeader || x.IsExpanded).ToList();
            }

            for (int row = firstVisibleRow; row < firstVisibleRow + visibleRowCount; row++)
            {
                var item = items?.Current;
                float currentX = 0;
                float currentX1 = 0;

                var columnList = visibleColumns;

                for (int i = 0; i < firstVisibleCol && i < columnList?.Count; i++)
                {
                    currentX += (float)columnList[i].Width;
                }
                currentX1 += currentX;
                for (int colIndex = firstVisibleCol; colIndex < visibleColCount; colIndex++)
                {
                    float GVColumnWidth = (float)visibleColumns[colIndex].Width;

                    if (GroupItems != null)
                    {
                        var rowcolor = row % 2 == 0 ? RowBackgroundColor : AlternatingRowBackground ?? RowBackgroundColor;
                        if (GroupItems[row].IsGroupHeader)
                        {
                            string value = Convert.ToString(GroupItems[row]?.GroupName!);
                            if (Group?.Target == visibleColumns[colIndex].Name)
                            {

                                CellContentAlignment cellContentAlignment = visibleColumns[colIndex].ContentAlignment;
                                Draw(canvas, colIndex, row, value!, GroupFontColor, SymbolFont, GroupRowBackgroundColor, null, GVColumnWidth, currentX, currentY, cellContentAlignment, rowHeight, HighlightSelected(GroupItems[row].Item));
                            }
                            else if (Group?.ToggleSymbol.TargetColumns == visibleColumns[colIndex].Name)
                            {
                                if (CurrentContext.GroupToggelDetails.ContainsKey(value!))
                                {
                                    var values = CurrentContext.GroupToggelDetails[value];
                                    values.x = currentX;
                                    values.y = currentY;
                                    values.width = GVColumnWidth;
                                    values.height = rowHeight;
                                    CurrentContext.GroupToggelDetails[value] = values;

                                }
                                Draw(canvas, colIndex, row, GroupItems[row].IsExpanded ? Group?.ToggleSymbol?.Expand : Group?.ToggleSymbol?.Collapse, GroupFontColor, SymbolFont, GroupRowBackgroundColor, null, GVColumnWidth, currentX, currentY, CellContentAlignment.Center, rowHeight, HighlightSelected(GroupItems[row].Item));
                            }
                            else
                                Draw(canvas, colIndex, row, "", GroupFontColor, SymbolFont, GroupRowBackgroundColor, null, GVColumnWidth, currentX, currentY, CellContentAlignment.Center, rowHeight, HighlightSelected(GroupItems[row].Item));
                        }
                        else
                        {
                            var value = reflectionHelper.ReadCurrentItemWithTypes(GroupItems[row].Item, visibleColumns[colIndex].BindingPath);
                            var val = Helper.ApplyFormat(value.Type, value.Value, visibleColumns[colIndex].Format, visibleColumns[colIndex].ShowBracketOnNegative);

                            CellContentAlignment cellContentAlignment = visibleColumns[colIndex].ContentAlignment;
                            Draw(canvas, colIndex, row, val, FontColor, SymbolFont, rowcolor, null, GVColumnWidth, currentX, currentY, cellContentAlignment, rowHeight, HighlightSelected(GroupItems[row].Item));
                        }
                    }
                    else
                    {
                        var value = reflectionHelper.ReadCurrentItemWithTypes(item, visibleColumns[colIndex].BindingPath); ;
                        CellContentAlignment cellContentAlignment = visibleColumns[colIndex].ContentAlignment;
                        Draw(canvas, colIndex, row, value!.Value!, FontColor, SymbolFont, CellBackgroundColor, null, GVColumnWidth, currentX, currentY, cellContentAlignment, rowHeight, HighlightSelected(item));
                    }

                    if (ShowGridLines)
                    {
                        canvas?.DrawLine(currentX + GVColumnWidth, currentY, currentX + GVColumnWidth, currentY + rowHeight, GridLineColor);
                        canvas?.DrawLine(currentX, currentY + rowHeight, currentX + GVColumnWidth, currentY + rowHeight, GridLineColor);
                    }
                    currentX += GVColumnWidth;
                }
                items?.MoveNext();
                currentY += rowHeight;
            }
        }
        public void UpdateVisibleColumns()
        {
            _visibleColumnsCache.Clear();

            if (Columns == null) return;

            foreach (var col in Columns.OrderBy(x => x.DisplayIndex).ToList())
            {
                if (col.IsVisible)
                    _visibleColumnsCache.Add(col);
            }
        }
        private void Draw(SKCanvas canvas, int columnsIndex, int rowIndex, string value, SKPaint fontcolor, SKFont textFont, SKPaint backColor, SKPaint? borderColor, float width, float x, float y, CellContentAlignment cellContentAlignment, float rowHeight, bool isselectedrow)
        {
            if (isselectedrow)
            { }
            var rowBackColor = isselectedrow ? SelectedRowBackgroundHighlighting : backColor;
            var rowTextColor = isselectedrow ? SelectedRowTextColor : fontcolor;

            DrawRect(canvas, rowIndex, x, y, rowBackColor, width, rowHeight);


            if (borderColor != null && !isselectedrow)
            {
                DrawBorder2(canvas, borderColor, width, x, y, rowHeight);
            }


            DrawText(canvas, columnsIndex, rowIndex, value, rowTextColor, textFont, width, x, y, cellContentAlignment);
        }

        private static void DrawBorder(SKCanvas canvas, SKPaint? borderColor, float width, float x, float y, float rowHeight)
        {
            canvas.DrawLine(x + 1f, y, x + 1f, y + rowHeight, borderColor);
            canvas.DrawLine(x + width - 1f, y, x + width - 1f, y + rowHeight, borderColor);
            canvas.DrawLine(x, y + rowHeight - 2f, x + width, y + rowHeight - 2f, borderColor);//bottom
            canvas.DrawLine(x, y + 0.5f, x + width, y + 0.5f, borderColor); // top
        }
        private static void DrawBorder2(SKCanvas canvas, SKPaint? borderColor, float width, float x, float y, float rowHeight)
        {
            canvas.DrawLine(x + 1f, y + 1f, x + 1f, y + rowHeight - 1f, borderColor); //left
            canvas.DrawLine(x + width - 2f, y + 1f, x + width - 2f, y + rowHeight - 2f, borderColor);//right
            canvas.DrawLine(x + 1f, y + rowHeight - 2f, x + width - 2f, y + rowHeight - 2f, borderColor);//bottom
            canvas.DrawLine(x + 1f, y + 1f, x + width - 2f, y + 1f, borderColor); // top
        }



        private void DrawText(SKCanvas canvas, int columnsIndex, int rowIndex, string value, SKPaint fontColor, SKFont textFont, float width, float x, float y, CellContentAlignment cellContentAlignment)
        {
            if (width < 10 || string.IsNullOrEmpty(value))
                return;

            float maxTextWidth = width - 10;
            ReadOnlySpan<char> span = value;
            ReadOnlySpan<char> ellipsis = "...";
            float ellipsisWidth = textFont.MeasureText(ellipsis, out _);

            int left = 0;
            int right = span.Length;
            int fitLength = span.Length;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var testSpan = span.Slice(0, mid);
                float testWidth = textFont.MeasureText(testSpan, out _);

                if (testWidth + (mid < span.Length ? ellipsisWidth : 0) <= maxTextWidth)
                {
                    fitLength = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            string finalText = span.Slice(0, fitLength).ToString();
            bool wasTrimmed = fitLength < span.Length;
            if (wasTrimmed)
                finalText += "...";

            float finalWidth = textFont.MeasureText(finalText, out _);

            float textX = x + 5;
            if (cellContentAlignment == CellContentAlignment.Right)
                textX = x + width - finalWidth - 5;
            else if (cellContentAlignment == CellContentAlignment.Center)
                textX = x + (width - finalWidth) / 2;

            canvas.DrawText(finalText, textX, y + textFont.Size, textFont, fontColor);
        }


        private void DrawRect(SKCanvas canvas, int rowIndex, float x, float y, SKPaint backColor, float width, float rowHeight)
        {
            float left = ShowGridLines ? x : x;
            float top = ShowGridLines ? y : y - 1;
            float right = x + width;
            float bottom = y + rowHeight;

            canvas.DrawRect(SKRect.Create(left, top + (0.25f), right - left, bottom - top), backColor);
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


    }



}
