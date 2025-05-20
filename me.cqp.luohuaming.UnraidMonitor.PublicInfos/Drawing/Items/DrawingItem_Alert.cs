using SkiaSharp;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Alert : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Alert;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Fill;

        public override float FillPercentage { get; set; } = 100;

        public string Header { get; set; }

        public bool IsHeaderBold { get; set; }

        public string Content { get; set; }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, SKPoint verticalCenterPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            return base.Draw(painting, startPoint, verticalCenterPoint, desireWidth, theme, palette);
        }
    }
}
