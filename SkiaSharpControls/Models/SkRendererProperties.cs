﻿using SkiaSharp;

namespace SkiaSharpControls.Models
{
    public class SkRendererProperties
    {
        public SKFont TextFont { get; set; }
        public SKPaint TextForeground { get; set; }
        public SKPaint LineBackground { get; set; }

        public SkRendererProperties()
        {
            TextFont = new SKFont() { Size = 11, Typeface = SKTypeface.FromFamilyName("Arial") };
            TextForeground = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            LineBackground = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1 };
        }
    }
}