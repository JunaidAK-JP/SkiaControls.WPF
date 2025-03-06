using SkiaSharp;

namespace SkiaSharpControls.Models
{
    public class SkCellTemplate
    {
        /// <summary>
        /// Indicates if the cell should be drawn as a toggle button.
        /// </summary>
        public bool IsToggleButton { get; set; }

        public bool IsToggleButtonOn { get; set; }

        public string CellContent { get; set; } = "";

        public SkRendererProperties? RendererProperties { get; set; }
    }
}