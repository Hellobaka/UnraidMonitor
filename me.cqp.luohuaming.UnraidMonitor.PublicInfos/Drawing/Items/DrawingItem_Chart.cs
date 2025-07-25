﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Chart : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Chart;

        public override DrawingCanvas.Layout Layout { get; set; } = DrawingCanvas.Layout.Remaining;

        [Bindable(BindableSupport.Yes)]
        [Description("散点")]
        public (DateTime time, double value)[] Points { get; set; } = [];

        [Bindable(BindableSupport.Yes)]
        [Description("最小值")]
        public double Min { get; set; } = 0;

        [Bindable(BindableSupport.Yes)]
        [Description("最大值")]
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

        public override void BeforeBinding()
        {
            Points = [];
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
            List<double> dataY = [];
            List<DateTime> dataX = [];
            if (Binding.Value.TryGetValue("Points", out var data))
            {
                foreach (var i in data.RawValues)
                {
                    dataY.Add(Convert.ToDouble(i));
                }
            }
            if (Binding.Value.TryGetValue("DateTime", out data))
            {
                bool isDiff = Binding.BindingPath.FirstOrDefault(x => x.Key == "DateTime").Value.FirstOrDefault().ValueType == ValueType.Diff;
                for (int i = 0; i < data.RawValues.Length; i++)
                {
                    if (isDiff && i % 2 == 1)
                    {
                        dataX.Add((DateTime)data.RawValues[i]);
                    }
                    else if(!isDiff)
                    {
                        dataX.Add((DateTime)data.RawValues[i]);
                    }
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
            if (Binding.Value.TryGetValue("Min", out data))
            {
                Min = (float)data.ParsedNumber;
            }
            if (Binding.Value.TryGetValue("Max", out data))
            {
                Max = (float)data.ParsedNumber;
            }
            Max = Math.Max(Max, Min + 0.1);
        }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            CalcHeight(theme);
            Thickness padding = new();
            var font = Painting.CreateCustomFont(!string.IsNullOrEmpty(OverrideFont) ? OverrideFont : DrawingStyle.GetThemeDefaultFont(theme));
            var size = Painting.MeasureString((Math.Round(Max, 2)).ToString(), TextSize, Painting.CreateCustomFont(DrawingStyle.GetThemeDefaultFont(theme)));
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
            for (double i = Min; i <= Max; i += (Max - Min) / (VerticalValueDisplayCount - 1))
            {
                string text = Math.Round(i, 2).ToString();
                size = Painting.MeasureString(text, TextSize, Painting.CreateCustomFont(DrawingStyle.GetThemeDefaultFont(theme)));
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
            if (Points.Length == 0)
            {
                return (new SKPoint(startPoint.X + desireWidth, startPoint.Y + OverrideHeight), desireWidth, OverrideHeight);
            }
            int dataInterval = Math.Max(Points.Length / HorizonValueDisplayCount, 1);
            for (int i = 1; i <= HorizonValueDisplayCount; i++)
            {
                string text = Points[i * dataInterval - 1].time.ToString(string.IsNullOrEmpty(OverrideFormat) ? "HH:mm:ss" : OverrideFormat);
                size = Painting.MeasureString(text, TextSize, Painting.CreateCustomFont(DrawingStyle.GetThemeDefaultFont(theme)));

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

        public static DrawingItem_Chart Create() => new()
        {
            Points = [],
        };

        private SKPoint[] ConvertDataPointToSKPoints(float chartHeight, float chartWidth, float paddingLeft, float paddingTop)
        {
            SKPoint[] skPoints = new SKPoint[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                float y = chartHeight - (float)(chartHeight * (Points[i].value - Min) / (Max - Min));

                skPoints[i] = new SKPoint(paddingLeft + (float)(i * chartWidth / (Points.Length - 1)), paddingTop + y);
            }
            return skPoints;
        }
    }
}
