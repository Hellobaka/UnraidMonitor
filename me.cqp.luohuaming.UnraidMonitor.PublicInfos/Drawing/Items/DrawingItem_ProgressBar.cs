using SkiaSharp;
using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_ProgressBar : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.ProgressBar;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Remaining;

        public float Radius { get; set; }

        public float Value { get; set; }

        public float Min { get; set; }

        public float Max { get; set; }

        public override float OverrideHeight { get; set; }

        public override Thickness Margin { get; set; } = new();

        public override void ApplyBinding()
        {
            base.ApplyBinding();
            if (Binding.Value.TryGetValue("Value", out var data))
            {
                Value = (float)data.ParsedNumber;
            }
        }

        public override float CalcHeight(DrawingStyle.Theme theme)
        {
            if (OverrideHeight == 0)
            {
                OverrideHeight = theme switch
                {
                    DrawingStyle.Theme.Unraid => 34,
                    DrawingStyle.Theme.MaterialDesign3 => 8,
                    DrawingStyle.Theme.MaterialDesign2 => 8,
                    _ => 8
                };
            }

            return base.CalcHeight(theme);
        }

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

        private (SKPoint endPoint, float width, float height) DrawMaterialDesign2(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            float valueWidth = (float)(desireWidth * (Value - Min) / (Max - Min));
            float remainWidth = (float)(desireWidth - valueWidth);
            float barHeight = OverrideHeight;
            painting.DrawRectangle(new SKRect
            {
                Location = new(startPoint.X, startPoint.Y),
                Size = new(valueWidth, barHeight)
            }, SKColor.Parse(palette.AccentColor), SKColor.Empty, 0, null, 0);
            painting.DrawRectangle(new SKRect
            {
                Location = new(startPoint.X + valueWidth, startPoint.Y),
                Size = new(remainWidth, barHeight)
            }, SKColor.Parse(palette.BackgroundColor), SKColor.Empty, 0, null, 0);

            return (new(startPoint.X + desireWidth, startPoint.Y + barHeight), desireWidth, barHeight);
        }

        private (SKPoint endPoint, float width, float height) DrawMaterialDesign3(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            float valueWidth = (float)(desireWidth * (Value - Min) / (Max - Min));
            float remainWidth = (float)(desireWidth - valueWidth);
            float barHeight = OverrideHeight;
            float trackGap = barHeight / 2;
            painting.DrawRectangle(new SKRect
            {
                Location = new(startPoint.X, startPoint.Y),
                Size = new(valueWidth, barHeight)
            }, SKColor.Parse(palette.AccentColor), SKColor.Empty, 0, null, 1000);
            painting.DrawRectangle(new SKRect
            {
                Location = new(startPoint.X + trackGap + valueWidth, startPoint.Y),
                Size = new(remainWidth - trackGap, barHeight)
            }, SKColor.Parse(palette.BackgroundColor), SKColor.Empty, 0, null, 1000);
            painting.DrawRectangle(new SKRect
            {
                Location = new(startPoint.X + valueWidth + remainWidth - trackGap - trackGap / 2, startPoint.Y + (OverrideHeight / 2) - (trackGap / 2)),
                Size = new(trackGap, trackGap)
            }, SKColor.Parse(palette.AccentColor), SKColor.Empty, 0, null, 1000);

            return (new(startPoint.X + desireWidth, startPoint.Y + barHeight), desireWidth, barHeight);
        }

        private (SKPoint endPoint, float width, float height) DrawUnraid(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            SKShader shader = null;
            float valueWidth = (float)(desireWidth * (Value - Min) / (Max - Min));
            float remainWidth = (float)(desireWidth - valueWidth);
            float barHeight = OverrideHeight;
            //startPoint.Y = verticalCenterPoint.Y - barHeight / 2;
            if (!string.IsNullOrEmpty(palette.Accent2Color))
            {
                shader = SKShader.CreateLinearGradient(
                    new SKPoint(startPoint.X, startPoint.Y),
                    new SKPoint(startPoint.X + valueWidth, startPoint.Y),
                    new SKColor[] { SKColor.Parse(palette.AccentColor), SKColor.Parse(palette.Accent2Color) },
                    null,
                    SKShaderTileMode.Clamp);
            }
            painting.DrawRectangle(new SKRect
            {
                Location = new(startPoint.X, startPoint.Y),
                Size = new(valueWidth, barHeight)
            }, SKColor.Parse(palette.AccentColor), SKColor.Empty, 0, shader, 0);
            painting.DrawRectangle(new SKRect
            {
                Location = new(startPoint.X + valueWidth, startPoint.Y),
                Size = new(remainWidth, barHeight)
            }, SKColor.Parse(palette.BackgroundColor), SKColor.Empty, 0, null, 0);

            return (new(startPoint.X + desireWidth, startPoint.Y + barHeight), desireWidth, barHeight);
        }

        private (SKPoint endPoint, float width, float height) DrawWinUI3(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            float valueWidth = (float)(desireWidth * (Value - Min) / (Max - Min));
            float remainWidth = (float)(desireWidth - valueWidth);
            float barHeight = OverrideHeight;
            float trackHeight = 3;

            painting.DrawRectangle(new SKRect
            {
                Location = new(startPoint.X, startPoint.Y + barHeight / 2 - trackHeight / 2),
                Size = new(desireWidth, trackHeight)
            }, SKColor.Parse(palette.BackgroundColor), SKColor.Empty, 0, null, 1000);

            painting.DrawRectangle(new SKRect
            {
                Location = new(startPoint.X, startPoint.Y),
                Size = new(valueWidth, barHeight)
            }, SKColor.Parse(palette.AccentColor), SKColor.Empty, 0, null, 1000);

            return (new(startPoint.X + desireWidth, startPoint.Y + barHeight), desireWidth, barHeight);
        }
    }
}
