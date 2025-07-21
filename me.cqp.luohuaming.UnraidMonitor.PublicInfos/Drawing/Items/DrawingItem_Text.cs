using SkiaSharp;
using System;
using System.ComponentModel;
using System.Linq;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Text : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Text;

        public override DrawingCanvas.Layout Layout { get; set; } = DrawingCanvas.Layout.Minimal;

        [Bindable(BindableSupport.Yes)]
        [Description("文本内容")]
        public string Text { get; set; } = "";

        public bool IsBold { get; set; }

        public int TextSize { get; set; } = 26;

        public string OverrideFont { get; set; }

        public string OverrideColor { get; set; }

        public override void BeforeBinding()
        {
            Text = "";
        }

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
            var size = Painting.MeasureString(Text, TextSize, font);
            return (endPoint, Layout == DrawingCanvas.Layout.Minimal ? size.Width : desireWidth, size.Height);
        }

        public static DrawingItem_Text Create() => new()
        {
            Text = "请输入文本"
        };
    }
}
