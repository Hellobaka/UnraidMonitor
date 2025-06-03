using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_ProgressRing : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.ProgressRing;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Minimal;

        public double Value { get; set; }

        public double Min { get; set; } = 0;

        public double Max { get; set; } = 100;

        public float StrokeWidth { get; set; } = 10;

        public bool IsRounded { get; set; } = true;

        public bool ShowHeader { get; set; }

        public bool TransparentBackground { get; set; } = true;

        public string Header { get; set; }

        public bool HeaderFontBold { get; set; }

        public int HeaderFontSize { get; set; } = 30;

        public bool ShowValue { get; set; }

        public string DisplayValue { get; set; }

        public bool ValueFontBold { get; set; }

        public int ValueFontSize { get; set; } = 30;

        public float Radius { get; set; }

        public DrawingBase.Position HeaderPosition { get; set; } = DrawingBase.Position.Top;

        public DrawingBase.Position ValuePosition { get; set; } = DrawingBase.Position.Center;

        public Thickness TextMargin { get; set; } = Thickness.DefaultMargin;

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
            if (Binding.Value.TryGetValue("DisplayValue", out data))
            {
                DisplayValue = data.FormattedString;
            }
            if (Binding.Value.TryGetValue("Value", out data))
            {
                Value = (float)data.ParsedNumber;
            }
            if (Binding.Value.TryGetValue("Min", out data))
            {
                Min = (float)data.ParsedNumber;
            }
            if (Binding.Value.TryGetValue("Max", out data))
            {
                Max = (float)data.ParsedNumber;
            }
        }

        public override float CalcHeight(DrawingStyle.Theme theme)
        {
            if (Radius == 0)
            {
                Radius = 50;
            }
            float extraHeight = 0;
            var font = Painting.CreateCustomFont(DrawingStyle.GetThemeDefaultFont(theme));
            if (ShowHeader && (HeaderPosition == DrawingBase.Position.Top || HeaderPosition == DrawingBase.Position.Bottom))
            {
                var size = Painting.MeasureString(Header, HeaderFontSize, font);
                extraHeight += size.Height;
            }
            if (ShowValue && (ValuePosition == DrawingBase.Position.Top || ValuePosition == DrawingBase.Position.Bottom))
            {
                var size = Painting.MeasureString(DisplayValue, ValueFontSize, font);
                extraHeight = Math.Max(extraHeight, size.Height);
            }

            return Radius * 2 + extraHeight;
        }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            CalcHeight(theme);
            var startPointCopy = new SKPoint(startPoint.X, startPoint.Y);
            var font = Painting.CreateCustomFont(DrawingStyle.GetThemeDefaultFont(theme));
            if (ShowHeader && HeaderPosition == DrawingBase.Position.Top)
            {
                startPoint.Y += TextMargin.Top;
                var size = Painting.MeasureString(Header, HeaderFontSize, font);
                var textCenter = new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + size.Height / 2);
                var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, startPoint.Y);
                painting.DrawText(Header, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);

                startPoint.Y += size.Height + TextMargin.Bottom;
            }

            if (ShowValue && ValuePosition == DrawingBase.Position.Top)
            {
                startPoint.Y += TextMargin.Top;
                var size = Painting.MeasureString(DisplayValue, HeaderFontSize, font);
                var textCenter = new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + size.Height / 2);
                var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, startPoint.Y);
                painting.DrawText(DisplayValue, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);

                startPoint.Y += size.Height;
                startPoint.Y += size.Height + TextMargin.Bottom;
            }

            var r = theme switch
            {
                DrawingStyle.Theme.Unraid => DrawUnraid(painting, startPoint, desireWidth, theme, palette),
                DrawingStyle.Theme.MaterialDesign3 => DrawMaterialDesign3(painting, startPoint, desireWidth, theme, palette),
                DrawingStyle.Theme.MaterialDesign2 => DrawMaterialDesign2(painting, startPoint, desireWidth, theme, palette),
                _ => DrawWinUI3(painting, startPoint, desireWidth, theme, palette),
            };
            if (ShowHeader)
            {
                var size = Painting.MeasureString(Header, HeaderFontSize, font);
                if (HeaderPosition == DrawingBase.Position.Bottom)
                {
                    var textCenter = new SKPoint(startPoint.X + desireWidth / 2, r.endPoint.Y + TextMargin.Top + size.Height / 2);
                    var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, textCenter.Y - size.Height / 2);
                    var endPoint = painting.DrawText(Header, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);

                    r.endPoint = new SKPoint(endPoint.X, endPoint.Y);
                }
                else if (HeaderPosition == DrawingBase.Position.Center)
                {
                    var textCenter = new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + Radius);
                    var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, textCenter.Y - size.Height / 2);
                    painting.DrawText(Header, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);
                }
            }
            if (ShowValue)
            {
                var size = Painting.MeasureString(DisplayValue, HeaderFontSize, font);
                if (ValuePosition == DrawingBase.Position.Bottom)
                {
                    var textCenter = new SKPoint(startPoint.X + desireWidth / 2, r.endPoint.Y + TextMargin.Top + size.Height / 2);
                    var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, textCenter.Y - size.Height / 2);
                    var endPoint = painting.DrawText(DisplayValue, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);

                    r.endPoint = new SKPoint(endPoint.X, endPoint.Y);
                }
                else if (ValuePosition == DrawingBase.Position.Center)
                {
                    var textCenter = new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + Radius);
                    var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, textCenter.Y - size.Height / 2);
                    painting.DrawText(DisplayValue, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);
                }
            }

            r.height = r.endPoint.Y - startPointCopy.Y;
            return r;
        }

        private (SKPoint endPoint, float width, float height) DrawMaterialDesign2(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            if (!TransparentBackground)
            {
                using var backgroundPaint = new SKPaint
                {
                    Color = SKColor.Parse(palette.BackgroundColor),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = StrokeWidth,
                    StrokeCap = IsRounded ? SKStrokeCap.Round : SKStrokeCap.Square,
                    IsAntialias = true
                };
                painting.MainCanvas.DrawCircle(new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + Radius + StrokeWidth / 2), Radius, backgroundPaint);
            }

            using var paint = new SKPaint
            {
                Color = SKColor.Parse(palette.AccentColor),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = StrokeWidth,
                StrokeCap = IsRounded ? SKStrokeCap.Round : SKStrokeCap.Square,
                IsAntialias = true
            };
            float sweepAngle = (float)(360f * ((Value - Min) / (Max - Min)));
            using var path = new SKPath();
            path.AddArc(
                new SKRect
                {
                    Location = new(startPoint.X + desireWidth / 2 - Radius, startPoint.Y + StrokeWidth / 2),
                    Size = new(Radius * 2, Radius * 2)
                },
                -90,
                sweepAngle
            );
            painting.MainCanvas.DrawPath(path, paint);

            return (new(startPoint.X + desireWidth, startPoint.Y + Radius * 2 + StrokeWidth), desireWidth, 0);
        }

        private (SKPoint endPoint, float width, float height) DrawMaterialDesign3(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            float sweepAngle = (float)(360f * ((Value - Min) / (Max - Min)));
            float gapAngle = 11.45f;

            if (!TransparentBackground)
            {
                using var backgroundPaint = new SKPaint
                {
                    Color = SKColor.Parse(palette.BackgroundColor),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = StrokeWidth,
                    StrokeCap = IsRounded ? SKStrokeCap.Round : SKStrokeCap.Square,
                    IsAntialias = true
                };
                using var backgroundPath = new SKPath();
                backgroundPath.AddArc(
                    new SKRect
                    {
                        Location = new(startPoint.X + desireWidth / 2 - Radius, startPoint.Y + StrokeWidth / 2),
                        Size = new(Radius * 2, Radius * 2)
                    },
                    -90 + sweepAngle + gapAngle,
                    (360 - sweepAngle - gapAngle * 2)
                );
                painting.MainCanvas.DrawPath(backgroundPath, backgroundPaint);
            }

            using var paint = new SKPaint
            {
                Color = SKColor.Parse(palette.AccentColor),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = StrokeWidth,
                StrokeCap = IsRounded ? SKStrokeCap.Round : SKStrokeCap.Square,
                IsAntialias = true
            };
            using (var path = new SKPath())
            {
                path.AddArc(
                    new SKRect
                    {
                        Location = new(startPoint.X + desireWidth / 2 - Radius, startPoint.Y + StrokeWidth / 2),
                        Size = new(Radius * 2, Radius * 2)
                    },
                    -90,
                    sweepAngle
                );
                painting.MainCanvas.DrawPath(path, paint);
            }
            paint.Dispose();
            return (new(startPoint.X + desireWidth, startPoint.Y + Radius * 2 + StrokeWidth), desireWidth, 0);
        }

        private (SKPoint endPoint, float width, float height) DrawUnraid(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            float sweepAngle = (float)(360f * ((Value - Min) / (Max - Min)));
            if (!TransparentBackground)
            {
                using var backgroundPaint = new SKPaint
                {
                    Color = SKColor.Parse(palette.BackgroundColor),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = StrokeWidth,
                    StrokeCap = IsRounded ? SKStrokeCap.Round : SKStrokeCap.Square,
                    IsAntialias = true
                };
                painting.MainCanvas.DrawCircle(new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + Radius + StrokeWidth / 2), Radius, backgroundPaint);
            }
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = StrokeWidth,
                StrokeCap = IsRounded ? SKStrokeCap.Round : SKStrokeCap.Square,
                IsAntialias = true
            };
            var center = new SKPoint(startPoint.X + desireWidth / 2 - Radius, startPoint.Y + StrokeWidth / 2);
            painting.MainCanvas.Save();
            painting.MainCanvas.Translate(center);
            painting.MainCanvas.RotateDegrees(-90);
            painting.MainCanvas.Translate(-center.X, -center.Y);

            if (!string.IsNullOrEmpty(palette.Accent2Color))
            {
                paint.Shader = SKShader.CreateSweepGradient(
                    center,
                    new SKColor[] { SKColor.Parse(palette.AccentColor), SKColor.Parse(palette.Accent2Color) }
                );
            }
            else
            {
                paint.Color = SKColor.Parse(palette.AccentColor);
            }

            using (var path = new SKPath())
            {
                path.AddArc(
                    new SKRect
                    {
                        Location = center,
                        Size = new(Radius * 2, Radius * 2)
                    },
                    0,
                    sweepAngle
                );
                painting.MainCanvas.DrawPath(path, paint);
                painting.MainCanvas.Restore();
            }
            paint.Dispose();
            return (new(startPoint.X + desireWidth, startPoint.Y + Radius * 2 + StrokeWidth), desireWidth, 0);
        }

        private (SKPoint endPoint, float width, float height) DrawWinUI3(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            if (!TransparentBackground)
            {
                using var backgroundPaint = new SKPaint
                {
                    Color = SKColor.Parse(palette.BackgroundColor),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = StrokeWidth,
                    StrokeCap = IsRounded ? SKStrokeCap.Round : SKStrokeCap.Square,
                    IsAntialias = true
                };
                painting.MainCanvas.DrawCircle(new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + Radius + StrokeWidth / 2), Radius, backgroundPaint);
            }

            using var paint = new SKPaint
            {
                Color = SKColor.Parse(palette.AccentColor),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = StrokeWidth,
                StrokeCap = IsRounded ? SKStrokeCap.Round : SKStrokeCap.Square,
                IsAntialias = true
            };
            float sweepAngle = (float)(360f * ((Value - Min) / (Max - Min)));
            using (var path = new SKPath())
            {
                path.AddArc(
                    new SKRect
                    {
                        Location = new(startPoint.X + desireWidth / 2 - Radius, startPoint.Y + StrokeWidth / 2),
                        Size = new(Radius * 2, Radius * 2)
                    },
                    -90,
                    sweepAngle
                );
                painting.MainCanvas.DrawPath(path, paint);
            }
            paint.Dispose();
            return (new(startPoint.X + desireWidth, startPoint.Y + Radius * 2 + StrokeWidth), desireWidth, 0);
        }
    }
}
