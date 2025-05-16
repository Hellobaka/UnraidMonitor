using SkiaSharp;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Text : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Text;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Minimal;

        public string Text { get; set; }

        public bool IsBold { get; set; }

        public override (SKPoint endPoint, double width, double height) Draw(Painting painting, SKPoint startPoint, double desireWidth, double desireHeight)
        {
            return base.Draw(painting, startPoint, desireWidth, desireHeight);
        }
    }
}
