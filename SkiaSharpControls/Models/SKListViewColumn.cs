namespace SkiaSharpControls.Models
{
    public class SKListViewColumn
    {
        public required string? Header { get; set; }
        public required string? PropertyName { get; set; }
        public double Width { get; set; } = 100;
    }
}