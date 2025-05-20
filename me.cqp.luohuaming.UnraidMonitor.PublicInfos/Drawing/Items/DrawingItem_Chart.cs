using SkiaSharp;
using System;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Chart : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Chart;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Remaining;

        public (DateTime time, double value)[] Points { get; set; } = [];

        public double Min { get; set; }

        public double Max { get; set; }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            return base.Draw(painting, startPoint, desireWidth, theme, palette);
        }
    }
}
