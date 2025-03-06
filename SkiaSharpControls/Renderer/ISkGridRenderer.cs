using SkiaSharp;
using SkiaSharpControls.Models;
using System.Collections;

namespace SkiaSharpControls.Renderer
{
    public interface ISkGridRenderer : IDisposable
    {
        SkRendererProperties RendererProperties { get; set; }
        void Draw(SKCanvas canvas, IEnumerable itemsSource, IEnumerable<SKGridViewColumn> columns, Func<object, SKColor> rowBackgroundSelector, Func<object, string, SkCellTemplate> cellTemplateSelector);
    }
}