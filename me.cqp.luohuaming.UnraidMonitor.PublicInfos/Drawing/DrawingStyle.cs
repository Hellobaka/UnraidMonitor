using SkiaSharp;
using System;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class DrawingStyle
    {
        public string Name { get; set; }

        public DrawingBase[] Content { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime ModifyTime { get; set; }

        public enum BackgroundType
        {
            Color,

            Image
        }

        public enum BackgroundImageScaleType
        {
            Center,

            Stretch,

            Tile,

            Fill
        }

        public SKColor BackgrdoundColor { get; set; }

        public string[] BackgroundImages { get; set; } = [];

        public BackgroundType DrawBackgroundType { get; set; } = BackgroundType.Color;

        public BackgroundImageScaleType DrawBackgroundImageScaleType { get; set; } = BackgroundImageScaleType.Fill;

        public double BackgroundBlur { get; set; } = 0;

        public Thickness Padding { get; set; }

        public double ContentBlur { get; set; }

        public double ContentRadius { get; set; }

        public Painting Draw(int width, int height)
        {
            return null;
        }
    }
}