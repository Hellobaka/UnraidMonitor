using SkiaSharp;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Text : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Text;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Minimal;

        public string Text { get; set; }

        public bool IsBold { get; set; }

        public int TextSize { get; set; }

        public string OverrideFont { get; set; }

        public string OverrideColor { get; set; }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, double desireWidth, double desireHeight, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            var font = Painting.CreateCustomFont(!string.IsNullOrEmpty(OverrideFont) ? OverrideFont : DrawingStyle.GetThemeDefaultFont(theme));
            var endPoint = painting.DrawText(Text, Painting.Anywhere, startPoint, SKColor.Parse(string.IsNullOrEmpty(OverrideColor) ? palette.TextColor : OverrideColor), TextSize, font, IsBold);
            
            return (endPoint, endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
        }
    }
}
