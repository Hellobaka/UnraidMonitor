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

        public DrawingCanvas.HorizonPosition HorizonAlignment { get; set; } = DrawingCanvas.HorizonPosition.Left;

        public override void BeforeBinding()
        {
            if (Binding.Value.ContainsKey("Text"))
            {
                Text = "";
            }
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
            var size = Painting.MeasureString(Text, TextSize, font);
            if (Layout == DrawingCanvas.Layout.Minimal)
            {
                var endPoint = painting.DrawText(Text, Painting.Anywhere, startPoint, SKColor.Parse(string.IsNullOrEmpty(OverrideColor) ? palette.TextColor : OverrideColor), null, TextSize, font, IsBold);
                return (endPoint, size.Width, size.Height);
            }
            else
            {
                startPoint = HorizonAlignment switch
                {
                    DrawingCanvas.HorizonPosition.Center => new SKPoint(startPoint.X + (desireWidth / 2 - size.Width / 2), startPoint.Y),
                    DrawingCanvas.HorizonPosition.Right => new SKPoint(startPoint.X + desireWidth - size.Width - Margin.Right - Padding.Right, startPoint.Y),
                    _ => startPoint,
                };
                var endPoint = painting.DrawText(Text, Painting.Anywhere, startPoint, SKColor.Parse(string.IsNullOrEmpty(OverrideColor) ? palette.TextColor : OverrideColor), null, TextSize, font, IsBold);
                return (endPoint, desireWidth, size.Height);
            }
        }

        public static DrawingItem_Text Create() => new()
        {
            Text = "请输入文本"
        };
    }
}
