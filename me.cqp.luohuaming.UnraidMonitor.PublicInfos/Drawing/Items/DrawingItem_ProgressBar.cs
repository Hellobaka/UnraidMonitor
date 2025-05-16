using SkiaSharp;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_ProgressBar : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.ProgressBar;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Left;

        public double Value { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public override (SKPoint endPoint, double width, double height) Draw(Painting painting, SKPoint startPoint, double desireWidth, double desireHeight)
        {
            return base.Draw(painting, startPoint, desireWidth, desireHeight);
        }
    }
}
