﻿using HarfBuzzSharp;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Icons;
using SkiaSharp;
using System;
using System.ComponentModel;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Alert : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Alert;

        public override DrawingCanvas.Layout Layout { get; set; } = DrawingCanvas.Layout.Remaining;

        [Bindable(BindableSupport.Yes)]
        [Description("消息等级")]
        public DrawingCanvas.AlertType AlertType { get; set; } = DrawingCanvas.AlertType.Info;

        [Bindable(BindableSupport.Yes)]
        [Description("标题")]
        public string Header { get; set; } = "";

        public bool IsHeaderBold { get; set; }

        [Bindable(BindableSupport.Yes)]
        [Description("内容")]
        public string Content { get; set; } = "";

        public string OverrideFont { get; set; }

        public float TextSize { get; set; } = 26;

        public override Thickness Padding { get; set; } = new Thickness(16);

        private Converter AlertTypeConverter { get; set; }

        public override void BeforeBinding()
        {
            Header = string.Empty;
            Content = string.Empty;
            AlertType = DrawingCanvas.AlertType.Info;
        }

        public override void ApplyBinding()
        {
            base.ApplyBinding();
            if (Binding == null)
            {
                return;
            }
            if (Binding.Value.TryGetValue("Header", out var data))
            {
                Header = data.FormattedString;
            }
            if (Binding.Value.TryGetValue("Content", out data))
            {
                Content = data.FormattedString;
            }
            if (Binding.Value.TryGetValue("AlertType", out data))
            {
                var item = data.FormattedString;
                if (AlertTypeConverter != null)
                {
                    if (AlertTypeConverter.Info == item)
                    {
                        AlertType = DrawingCanvas.AlertType.Info;
                    }
                    else if (AlertTypeConverter.Success == item)
                    {
                        AlertType = DrawingCanvas.AlertType.Success;
                    }
                    else if (AlertTypeConverter.Warning == item)
                    {
                        AlertType = DrawingCanvas.AlertType.Warning;
                    }
                    else if (AlertTypeConverter.Fatal == item)
                    {
                        AlertType = DrawingCanvas.AlertType.Fatal;
                    }
                }
            }
        }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            CalcHeight(theme);
            return theme switch
            {
                DrawingStyle.Theme.Unraid => DrawUnraid(painting, startPoint, desireWidth, theme, palette),
                DrawingStyle.Theme.MaterialDesign3 or DrawingStyle.Theme.MaterialDesign2 => DrawMaterialDesign(painting, startPoint, desireWidth, theme, palette),
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
                DrawingCanvas.AlertType.Warning => palette.WarningIconColor,
                DrawingCanvas.AlertType.Fatal => palette.FatalIconColor,
                DrawingCanvas.AlertType.Success => palette.SuccessIconColor,
                _ => palette.InfoIconColor,
            });
            SKColor backgroundColor = SKColor.Parse(AlertType switch
            {
                DrawingCanvas.AlertType.Warning => palette.WarningColor,
                DrawingCanvas.AlertType.Fatal => palette.FatalColor,
                DrawingCanvas.AlertType.Success => palette.SuccessColor,
                _ => palette.InfoColor,
            });
            string alertChar = AlertType switch
            {
                DrawingCanvas.AlertType.Warning => "\uE814",
                DrawingCanvas.AlertType.Fatal => "\uEB90",
                DrawingCanvas.AlertType.Success => "\uEC61",
                _ => "\uF167",
            };
            using Painting content = new((int)Math.Ceiling(desireWidth), 1000);
            SKPoint currentPoint = new();
            float textHeight = 0;
            // Icon
            SKSize size = Painting.MeasureString("O", TextSize, font);
            currentPoint = content.DrawText(alertChar, Painting.Anywhere, currentPoint, iconColor, null, size.Height, iconFont, false);
            // Header
            currentPoint.X += 12;
            currentPoint.Y = 0;
            size = Painting.MeasureString(Header, TextSize, font);
            textHeight = Math.Max(textHeight, size.Height);
            currentPoint = content.DrawText(Header, Painting.Anywhere, currentPoint, SKColor.Parse(palette.TextColor), null, TextSize, font, IsHeaderBold);
            // Content
            currentPoint.X += 12;
            currentPoint.Y = 0;
            size = Painting.MeasureString(Content, TextSize, font);
            currentPoint = content.DrawText(Content, new()
            {
                Location = new(currentPoint.X, currentPoint.Y),
                Size = new(width - currentPoint.X, 1000)
            }, currentPoint, SKColor.Parse(palette.TextColor), null, TextSize, font, false);
            textHeight = Math.Max(textHeight, Math.Max(size.Height, currentPoint.Y));
            // Draw to main canvas
            // content.Resize((int)Math.Ceiling(content.Width), (int)Math.Ceiling(textHeight));
            painting.DrawRectangle(new()
            {
                Location = new(startPoint.X, startPoint.Y),
                Size = new(desireWidth, textHeight + Padding.Top + Padding.Bottom)
            }, backgroundColor, SKColors.Transparent, 0, null, 4);
            painting.DrawImage(content.SnapShot(), new()
            {
                Location = new(startPoint.X + Padding.Left, startPoint.Y + Padding.Top),
                Size = new(content.Width, content.Height)
            });
            return (new(startPoint.X + desireWidth, startPoint.Y + Padding.Top + textHeight + Padding.Bottom), desireWidth, textHeight + Padding.Top + Padding.Bottom);
        }

        private (SKPoint endPoint, float width, float height) DrawMaterialDesign(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            var font = Painting.CreateCustomFont(!string.IsNullOrEmpty(OverrideFont) ? OverrideFont : DrawingStyle.GetThemeDefaultFont(theme));
            float width = desireWidth - Padding.Left - Padding.Right;
            SKColor iconColor = SKColor.Parse(AlertType switch
            {
                DrawingCanvas.AlertType.Warning => palette.WarningIconColor,
                DrawingCanvas.AlertType.Fatal => palette.FatalIconColor,
                DrawingCanvas.AlertType.Success => palette.SuccessIconColor,
                _ => palette.InfoIconColor,
            });
            SKColor backgroundColor = SKColor.Parse(AlertType switch
            {
                DrawingCanvas.AlertType.Warning => palette.WarningColor,
                DrawingCanvas.AlertType.Fatal => palette.FatalColor,
                DrawingCanvas.AlertType.Success => palette.SuccessColor,
                _ => palette.InfoColor,
            });
            string alertSvg = AlertType switch
            {
                DrawingCanvas.AlertType.Warning => SVG.ChangeFillColor(theme == DrawingStyle.Theme.MaterialDesign2 ? SVG.MD2_Warning : SVG.MD3_Warning, iconColor.ToString()),
                DrawingCanvas.AlertType.Fatal => SVG.ChangeFillColor(theme == DrawingStyle.Theme.MaterialDesign2 ? SVG.MD2_Fatal : SVG.MD3_Fatal, iconColor.ToString()),
                DrawingCanvas.AlertType.Success => SVG.ChangeFillColor(theme == DrawingStyle.Theme.MaterialDesign2 ? SVG.MD2_Success : SVG.MD3_Success, iconColor.ToString()),
                _ => SVG.ChangeFillColor(theme == DrawingStyle.Theme.MaterialDesign2 ? SVG.MD2_Info : SVG.MD3_Info, iconColor.ToString()),
            };
            using Painting content = new((int)Math.Ceiling(desireWidth), 1000);
            SKPoint currentPoint = new();
            float textHeight = 0;
            // Icon
            SKSize size = Painting.MeasureString("O", TextSize, font);
            alertSvg = SVG.ChangeSize(alertSvg, new SKSize(size.Height, size.Height));
            content.DrawSVG(alertSvg, new(currentPoint.X, currentPoint.Y));
            currentPoint.X += size.Height;
            // Header
            currentPoint.X += 12;
            size = Painting.MeasureString(Header, TextSize, font);
            textHeight = Math.Max(textHeight, size.Height);
            currentPoint = content.DrawText(Header, Painting.Anywhere, currentPoint, iconColor, null, TextSize, font, IsHeaderBold);
            // Content
            currentPoint.X += 12;
            currentPoint.Y = 0;
            size = Painting.MeasureString(Content, TextSize, font);
            currentPoint = content.DrawText(Content, new()
            {
                Location = new(currentPoint.X, currentPoint.Y),
                Size = new(width - currentPoint.X, 1000)
            }, currentPoint, iconColor, null, TextSize, font, false);
            textHeight = Math.Max(textHeight, Math.Max(size.Height, currentPoint.Y));
            // Draw to main canvas
            // content.Resize((int)Math.Ceiling(content.Width), (int)Math.Ceiling(textHeight));
            painting.DrawRectangle(new()
            {
                Location = new(startPoint.X, startPoint.Y),
                Size = new(desireWidth, textHeight + Padding.Top + Padding.Bottom)
            }, backgroundColor, SKColors.Transparent, 0, null, 4);
            painting.DrawImage(content.SnapShot(), new()
            {
                Location = new(startPoint.X + Padding.Left, startPoint.Y + Padding.Top),
                Size = new(content.Width, content.Height)
            });
            return (new(startPoint.X + desireWidth, startPoint.Y + Padding.Top + textHeight + Padding.Bottom), desireWidth, textHeight + Padding.Top + Padding.Bottom);
        }

        private (SKPoint endPoint, float width, float height) DrawUnraid(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            var font = Painting.CreateCustomFont(!string.IsNullOrEmpty(OverrideFont) ? OverrideFont : DrawingStyle.GetThemeDefaultFont(theme));
            float width = desireWidth - Padding.Left - Padding.Right;
            SKColor iconColor = SKColor.Parse(AlertType switch
            {
                DrawingCanvas.AlertType.Warning => palette.WarningIconColor,
                DrawingCanvas.AlertType.Fatal => palette.FatalIconColor,
                DrawingCanvas.AlertType.Success => palette.SuccessIconColor,
                _ => palette.InfoIconColor,
            });
            SKColor backgroundColor = SKColor.Parse(AlertType switch
            {
                DrawingCanvas.AlertType.Warning => palette.WarningColor,
                DrawingCanvas.AlertType.Fatal => palette.FatalColor,
                DrawingCanvas.AlertType.Success => palette.SuccessColor,
                _ => palette.InfoColor,
            });
            using Painting content = new((int)Math.Ceiling(desireWidth), 1000);
            SKPoint currentPoint = new();
            float textHeight = 0;
            // Header
            var size = Painting.MeasureString(Header, TextSize, font);
            textHeight = Math.Max(textHeight, size.Height);
            currentPoint = content.DrawText(Header, Painting.Anywhere, currentPoint, iconColor, null, TextSize, font, IsHeaderBold);
            // Content
            currentPoint.X = 0;
            currentPoint.Y += size.Height / 4;
            size = Painting.MeasureString(Content, TextSize, font);
            currentPoint = content.DrawText(Content, new()
            {
                Location = new(currentPoint.X, currentPoint.Y),
                Size = new(width - currentPoint.X, 1000)
            }, currentPoint, iconColor, null, TextSize, font, false);
            textHeight = Math.Max(textHeight, Math.Max(size.Height, currentPoint.Y));
            // Draw to main canvas
            // content.Resize((int)Math.Ceiling(content.Width), (int)Math.Ceiling(textHeight));
            painting.DrawRectangle(new()
            {
                Location = new(startPoint.X, startPoint.Y),
                Size = new(desireWidth, textHeight + Padding.Top + Padding.Bottom)
            }, backgroundColor, iconColor, 1, null, 4);
            painting.DrawImage(content.SnapShot(), new()
            {
                Location = new(startPoint.X + Padding.Left, startPoint.Y + Padding.Top),
                Size = new(content.Width, content.Height)
            });
            return (new(startPoint.X + desireWidth, startPoint.Y + Padding.Top + textHeight + Padding.Bottom), desireWidth, textHeight + Padding.Top + Padding.Bottom);
        }

        public static DrawingItem_Alert Create() => new()
        {
            AlertType = DrawingCanvas.AlertType.Info,
            Header = "输入标题",
            Content = "输入内容"
        };

        public class Converter
        {
            public string Info { get; set; }

            public string Warning { get; set; }

            public string Fatal { get; set; }

            public string Success { get; set; }
        }
    }
}
