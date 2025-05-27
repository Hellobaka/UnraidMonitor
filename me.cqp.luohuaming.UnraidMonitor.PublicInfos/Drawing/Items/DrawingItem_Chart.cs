using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Chart : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Chart;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Remaining;

        public (DateTime time, double value)[] Points { get; set; } = [];

        public double Min { get; set; } = 0;

        public double Max { get; set; } = 100;

        public bool ShowHorizonGridLine { get; set; } = true;

        public bool ShowVerticalGridLine { get; set; } = false;

        public bool ShowHorizonValue { get; set; } = true;

        public bool ShowVerticalValue { get; set; } = true;

        public int HorizonValueDisplayCount { get; set; } = 5;

        public int VerticalValueDisplayCount { get; set; } = 3;

        public string OverrideFormat { get; set; }

        public string OverrideFont { get; set; }

        public float TextSize { get; set; } = 20;

        public override float CalcHeight(DrawingStyle.Theme theme)
        {
            if (OverrideHeight == 0)
            {
                OverrideHeight = 100;
            }
            return base.CalcHeight(theme);
        }

        public override void ApplyBinding()
        {
            base.ApplyBinding();
            List<float> dataY = [];
            List<DateTime> dataX = [];
            if (Binding.Value.TryGetValue("Points", out var data))
            {
                var item = data.FirstOrDefault();
                var itemType = item.GetType().Name;
                if (itemType != "Int32" || itemType != "Double" || itemType != "Single")
                {
                    return;
                }
                foreach(var i in data)
                {
                    dataY.Add((float)i);
                }
            }
            if (Binding.Value.TryGetValue("DateTime", out data))
            {
                var item = data.FirstOrDefault();
                var itemType = item.GetType().Name;
                if (itemType != "DateTime")
                {
                    return;
                }
                foreach(var i in data)
                {
                    dataX.Add((DateTime)i);
                }
            }
            if (dataX.Count != dataY.Count)
            {
                Debugger.Break();
            }
            for (int i = 0; i < Math.Min(dataX.Count, dataY.Count); i++)
            {
                Points = [.. Points, (dataX[i], dataY[i])];
            }
        }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            CalcHeight(theme);
            Thickness padding = new();
            var font = Painting.CreateCustomFont(!string.IsNullOrEmpty(OverrideFont) ? OverrideFont : DrawingStyle.GetThemeDefaultFont(theme));
            var size = painting.MeasureString(Max.ToString(), TextSize, Painting.CreateCustomFont(DrawingStyle.GetThemeDefaultFont(theme)));
            float textPadding = 5;
            if (ShowHorizonValue)
            {
                padding.Bottom = size.Height + textPadding;
                padding.Left = size.Width / 2 + textPadding;
            }
            if (ShowVerticalValue)
            {
                padding.Left = size.Width + textPadding;
                padding.Top = size.Height / 2 + textPadding;
                padding.Bottom = Math.Max(padding.Bottom, size.Height / 2 + textPadding);
            }
            float chartHeight = OverrideHeight - padding.Top - padding.Bottom;
            SKPoint p1 = new(startPoint.X + padding.Left, startPoint.Y + padding.Top);
            SKPoint p2 = new(startPoint.X + padding.Left, startPoint.Y + padding.Top + chartHeight);
            painting.DrawLine(p1, p2, SKColor.Parse(palette.BackgroundColor), 1);
            p1 = new(p2.X, p2.Y);
            p2 = new(p1.X + desireWidth - padding.Left, p1.Y);
            painting.DrawLine(p1, p2, SKColor.Parse(palette.BackgroundColor), 1);
            // GridLine
            for (int i = (int)Min; i <= (int)Max; i += (int)(Max - Min) / (VerticalValueDisplayCount - 1))
            {
                string text = i.ToString();
                size = painting.MeasureString(text, TextSize, Painting.CreateCustomFont(DrawingStyle.GetThemeDefaultFont(theme)));
                float y = (float)(chartHeight * (i - Min) / (Max - Min));
                p1 = new(startPoint.X + padding.Left, startPoint.Y + padding.Top + chartHeight - y);
                p2 = new(p1.X + desireWidth, p1.Y);
                if (ShowHorizonGridLine)
                {
                    painting.DrawLine(p1, p2, SKColor.Parse(palette.BackgroundColor), 1);
                }
                if (ShowVerticalValue)
                {
                    painting.DrawText(text, Painting.Anywhere, new SKPoint(p1.X - size.Width - textPadding, p1.Y - size.Height / 2), SKColor.Parse(palette.TextColor), null, TextSize, font);
                }
            }
            int dataInterval = Math.Max(Points.Length / HorizonValueDisplayCount, 1);
            for (int i = 1; i <= HorizonValueDisplayCount; i++)
            {
                string text = Points[i * dataInterval - 1].time.ToString(string.IsNullOrEmpty(OverrideFormat) ? "HH:mm:ss" : OverrideFormat);
                size = painting.MeasureString(text, TextSize, Painting.CreateCustomFont(DrawingStyle.GetThemeDefaultFont(theme)));

                p1 = new(startPoint.X + padding.Left + (desireWidth / (HorizonValueDisplayCount - 1)) * i, startPoint.Y + padding.Top + chartHeight);
                p2 = new(p1.X, p1.Y - chartHeight);
                if (ShowVerticalGridLine)
                {
                    painting.DrawLine(p1, p2, SKColor.Parse(palette.BackgroundColor), 1);
                }
                if (ShowHorizonValue)
                {
                    painting.DrawText(text, Painting.Anywhere, new SKPoint(p1.X - size.Width / 2, p1.Y + textPadding), SKColor.Parse(palette.TextColor), null, TextSize, font);
                }
            }
            // Values
            painting.DrawChart(ConvertDataPointToSKPoints(chartHeight, desireWidth - padding.Left, padding.Left, padding.Top), new()
            {
                Location = new(startPoint.X + padding.Left, startPoint.Y + padding.Top),
                Size = new SKSize(desireWidth - padding.Left, chartHeight)
            }, SKColor.Parse(palette.AccentColor), 2, SKColor.Parse(palette.AccentColor), string.IsNullOrEmpty(palette.Accent2Color) ? SKColors.Transparent : SKColor.Parse(palette.Accent2Color));
            return (new SKPoint(startPoint.X + desireWidth, startPoint.Y + OverrideHeight), desireWidth, OverrideHeight);
        }

        private SKPoint[] ConvertDataPointToSKPoints(float chartHeight, float chartWidth, float paddingLeft, float paddingTop)
        {
            SKPoint[] skPoints = new SKPoint[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                float y = (float)(chartHeight * (Points[i].value - Min) / (Max - Min));

                skPoints[i] = new SKPoint(paddingLeft + (float)(i * chartWidth / (Points.Length - 1)), paddingTop + y);
            }
            return skPoints;
        }
    }
}
