using SkiaSharp;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_ProgressBar : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.ProgressBar;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Left;

        public float Radius { get; set; }

        public double Value { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, double desireWidth, double desireHeight, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            return base.Draw(painting, startPoint, desireWidth, desireHeight, theme, palette);
        }
    }
}
