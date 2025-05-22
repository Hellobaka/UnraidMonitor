using SkiaSharp;
using System;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Alert : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Alert;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Remaining;

        public DrawingBase.AlertType AlertType { get; set; } = DrawingBase.AlertType.Info;

        public string Header { get; set; }

        public bool IsHeaderBold { get; set; }

        public string Content { get; set; }

        public string OverrideFont { get; set; }

        public float TextSize { get; set; } = 26;

        public Thickness Padding { get; set; } = new Thickness(16);

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            CalcHeight(theme);
            return theme switch
            {
                DrawingStyle.Theme.Unraid => DrawUnraid(painting, startPoint, desireWidth, theme, palette),
                DrawingStyle.Theme.MaterialDesign3 => DrawMaterialDesign3(painting, startPoint, desireWidth, theme, palette),
                DrawingStyle.Theme.MaterialDesign2 => DrawMaterialDesign2(painting, startPoint, desireWidth, theme, palette),
                _ => DrawWinUI3(painting, startPoint, desireWidth, theme, palette),
            };
        }

        private (SKPoint endPoint, float width, float height) DrawWinUI3(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            var font = Painting.CreateCustomFont(!string.IsNullOrEmpty(OverrideFont) ? OverrideFont : DrawingStyle.GetThemeDefaultFont(theme));
            var iconFont = Painting.CreateCustomFont("Segoe Fluent Icons");
            float width = desireWidth - Padding.Left - Padding.Right;
            SKColor iconColor = SKColor.Parse(AlertType switch
            {
                DrawingBase.AlertType.Warning => palette.WarningIconColor,
                DrawingBase.AlertType.Fatal => palette.FatalIconColor,
                DrawingBase.AlertType.Success => palette.SuccessIconColor,
                _ => palette.InfoIconColor,
            });
            SKColor backgroundColor = SKColor.Parse(AlertType switch
            {
                DrawingBase.AlertType.Warning => palette.WarningColor,
                DrawingBase.AlertType.Fatal => palette.FatalColor,
                DrawingBase.AlertType.Success => palette.SuccessColor,
                _ => palette.InfoColor,
            });
            string alertChar = AlertType switch
            {
                DrawingBase.AlertType.Warning => "\uE814",
                DrawingBase.AlertType.Fatal => "\uEB90",
                DrawingBase.AlertType.Success => "\uEC61",
                _ => "\uF167",
            };
            using Painting content = new((int)Math.Ceiling(desireWidth), 1000);
            SKPoint currentPoint = new();
            float textHeight = 0;
            // Icon
            SKSize size = content.MeasureString("O", TextSize, font);
            currentPoint = content.DrawText(alertChar, Painting.Anywhere, currentPoint, iconColor, null, size.Height, iconFont, false);
            // Header
            currentPoint.X += 12;
            currentPoint.Y = 0;
            size = content.MeasureString(Header, TextSize, font);
            textHeight = Math.Max(textHeight, size.Height);
            currentPoint = content.DrawText(Header, Painting.Anywhere, currentPoint, SKColor.Parse(palette.TextColor), null, TextSize, font, IsHeaderBold);
            // Content
            currentPoint.X += 12;
            currentPoint.Y = 0;
            size = content.MeasureString(Content, TextSize, font);
            currentPoint = content.DrawText(Content, new()
            {
                Location = new(currentPoint.X, currentPoint.Y),
                Size = new(width - currentPoint.X, 1000)
            }, currentPoint, SKColor.Parse(palette.TextColor), null, TextSize, font, false);
            textHeight = Math.Max(textHeight, Math.Max(size.Height, currentPoint.Y));
            // Draw to main canvas
            content.Resize((int)Math.Ceiling(content.Width), (int)Math.Ceiling(textHeight));
            painting.DrawRectangle(new()
            {
                Location = new(startPoint.X, startPoint.Y),
                Size = new(desireWidth, content.Height + Padding.Top + Padding.Bottom)
            }, backgroundColor, SKColors.Transparent, 0, null, 4);
            painting.DrawImage(content.SnapShot(), new()
            {
                Location = new(startPoint.X + Padding.Left, startPoint.Y + Padding.Top),
                Size = new(content.Width, content.Height)
            });
            return (new(startPoint.X + desireWidth, startPoint.Y + Padding.Top + content.Height + Padding.Bottom), desireWidth, content.Height + Padding.Top + Padding.Bottom);
        }

        private (SKPoint endPoint, float width, float height) DrawMaterialDesign2(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            throw new NotImplementedException();
        }

        private (SKPoint endPoint, float width, float height) DrawMaterialDesign3(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            throw new NotImplementedException();
        }

        private (SKPoint endPoint, float width, float height) DrawUnraid(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            throw new NotImplementedException();
        }
    }
}
