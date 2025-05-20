using SkiaSharp;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_ProgressRing : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.ProgressRing;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Left;

        public double Value { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public string Header { get; set; }

        public bool IsBold { get; set; }

        public DrawingBase.Position HeaderPosition { get; set; }

        public override (SKPoint endPoint, double width, double height) Draw(Painting painting, SKPoint startPoint, double desireWidth, double desireHeight, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            return base.Draw(painting, startPoint, desireWidth, desireHeight, theme, palette);
        }
    }
}
