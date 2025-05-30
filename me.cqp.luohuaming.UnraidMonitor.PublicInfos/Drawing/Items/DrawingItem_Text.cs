using SkiaSharp;
using System;
using System.Linq;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Text : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Text;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Minimal;

        public string Text { get; set; }

        public bool IsBold { get; set; }

        public int TextSize { get; set; } = 26;

        public string OverrideFont { get; set; }

        public string OverrideColor { get; set; }

        public override void ApplyBinding()
        {
            base.ApplyBinding();
            if (Binding == null)
            {
                return;
            }
            if (Binding.Value.TryGetValue("Text", out var data))
            {
                Text = data.FormattedString;
            }
        }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            var font = Painting.CreateCustomFont(!string.IsNullOrEmpty(OverrideFont) ? OverrideFont : DrawingStyle.GetThemeDefaultFont(theme));
            var endPoint = painting.DrawText(Text, Painting.Anywhere, startPoint, SKColor.Parse(string.IsNullOrEmpty(OverrideColor) ? palette.TextColor : OverrideColor), null, TextSize, font, IsBold);
            var size = painting.MeasureString(Text, TextSize, font);
            return (endPoint, Layout == DrawingBase.Layout.Minimal ? size.Width : desireWidth, size.Height);
        }
    }
}
