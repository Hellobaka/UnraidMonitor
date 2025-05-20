using SkiaSharp;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_ProgressRing : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.ProgressRing;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Minimal;

        public double Value { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public string Header { get; set; }

        public bool IsBold { get; set; }

        public DrawingBase.Position HeaderPosition { get; set; }

        public override Thickness Margin { get; set; } = new Thickness(10);

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            return base.Draw(painting, startPoint, desireWidth, theme, palette);
        }
    }
}
