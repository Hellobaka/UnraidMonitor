using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_ProgressRing : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.ProgressRing;

        public override DrawingCanvas.Layout Layout { get; set; } = DrawingCanvas.Layout.Minimal;

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

        public DrawingCanvas.Position HeaderPosition { get; set; } = DrawingCanvas.Position.Top;

        public DrawingCanvas.Position ValuePosition { get; set; } = DrawingCanvas.Position.Center;

        /// <summary>
        /// 文本外边距
        /// </summary>
        public override Thickness Padding { get; set; } = Thickness.DefaultMargin;

        public override void BeforeBinding()
        {
            Header = "";
            DisplayValue = "";
            Value = 0;
            Min = 0;
            Max = 100;
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
            if (ShowHeader && (HeaderPosition == DrawingCanvas.Position.Top || HeaderPosition == DrawingCanvas.Position.Bottom))
            {
                var size = Painting.MeasureString(Header, HeaderFontSize, font);
                extraHeight += size.Height;
            }
            if (ShowValue && (ValuePosition == DrawingCanvas.Position.Top || ValuePosition == DrawingCanvas.Position.Bottom))
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
            if (ShowHeader && HeaderPosition == DrawingCanvas.Position.Top)
            {
                startPoint.Y += Padding.Top;
                var size = Painting.MeasureString(Header, HeaderFontSize, font);
                var textCenter = new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + size.Height / 2);
                var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, startPoint.Y);
                painting.DrawText(Header, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);

                startPoint.Y += size.Height + Padding.Bottom;
            }

            if (ShowValue && ValuePosition == DrawingCanvas.Position.Top)
            {
                startPoint.Y += Padding.Top;
                var size = Painting.MeasureString(DisplayValue, HeaderFontSize, font);
                var textCenter = new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + size.Height / 2);
                var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, startPoint.Y);
                painting.DrawText(DisplayValue, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);

                startPoint.Y += size.Height;
                startPoint.Y += size.Height + Padding.Bottom;
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
                if (HeaderPosition == DrawingCanvas.Position.Bottom)
                {
                    var textCenter = new SKPoint(startPoint.X + desireWidth / 2, r.endPoint.Y + Padding.Top + size.Height / 2);
                    var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, textCenter.Y - size.Height / 2);
                    var endPoint = painting.DrawText(Header, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);

                    r.endPoint = new SKPoint(endPoint.X, endPoint.Y);
                }
                else if (HeaderPosition == DrawingCanvas.Position.Center)
                {
                    var textCenter = new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + Radius);
                    var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, textCenter.Y - size.Height / 2);
                    painting.DrawText(Header, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);
                }
            }
            if (ShowValue)
            {
                var size = Painting.MeasureString(DisplayValue, HeaderFontSize, font);
                if (ValuePosition == DrawingCanvas.Position.Bottom)
                {
                    var textCenter = new SKPoint(startPoint.X + desireWidth / 2, r.endPoint.Y + Padding.Top + size.Height / 2);
                    var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, textCenter.Y - size.Height / 2);
                    var endPoint = painting.DrawText(DisplayValue, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, HeaderFontSize, font, HeaderFontBold);

                    r.endPoint = new SKPoint(endPoint.X, endPoint.Y);
                }
                else if (ValuePosition == DrawingCanvas.Position.Center)
                {
                    var textCenter = new SKPoint(startPoint.X + desireWidth / 2, startPoint.Y + Radius);
                    var textStartPoint = new SKPoint(textCenter.X - size.Width / 2, textCenter.Y - size.Height / 2);
                    painting.DrawText(DisplayValue, Painting.Anywhere, textStartPoint, SKColor.Parse(palette.TextColor), null, ValueFontSize, font, ValueFontBold);
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
            using Painting ring = new((int)(Radius * 2 + StrokeWidth), (int)(Radius * 2 + StrokeWidth));
            ring.Clear(SKColors.Transparent);
            var center = new SKPoint(Radius + StrokeWidth / 2, Radius + StrokeWidth / 2);
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
                ring.MainCanvas.DrawCircle(center, Radius, backgroundPaint);
            }
            ring.MainCanvas.Save();
            ring.MainCanvas.Translate(center);
            ring.MainCanvas.RotateDegrees(-90);
            ring.MainCanvas.Translate(-center.X, -center.Y);
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = StrokeWidth,
                StrokeCap = IsRounded ? SKStrokeCap.Round : SKStrokeCap.Square,
                IsAntialias = true
            };

            if (!string.IsNullOrEmpty(palette.Accent2Color))
            {
                paint.Shader = SKShader.CreateSweepGradient(
                    center: center,
                    colors: new SKColor[] { SKColor.Parse(palette.Accent2Color), SKColor.Parse(palette.AccentColor), SKColor.Parse(palette.Accent2Color) },
                    colorPos: null,
                    tileMode: SKShaderTileMode.Clamp,
                    startAngle: 0,
                    endAngle: 360
                );
            }
            else
            {
                paint.Color = SKColor.Parse(palette.AccentColor);
            }

            using var path = new SKPath();
            path.AddArc(new SKRect
            {
                Location = new(StrokeWidth / 2, StrokeWidth / 2),
                Size = new(Radius * 2, Radius * 2)
            }, 0, sweepAngle);
            ring.MainCanvas.DrawPath(path, paint);
            painting.DrawImage(ring.SnapShot(), new SKRect()
            {
                Location = new SKPoint(startPoint.X + desireWidth / 2 - Radius - StrokeWidth / 2, startPoint.Y),
                Size = new SKSize(ring.Width, ring.Height)
            });
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

        public static DrawingItem_ProgressRing Create() => new()
        {
        };
    }
}
