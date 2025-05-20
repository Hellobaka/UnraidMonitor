using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class DrawingBase
    {
        public class Title
        {
            public bool HasTitle { get; set; }

            public bool HasIcon { get; set; }

            public string IconPath { get; set; }

            public SKSize IconSize { get; set; }

            public Thickness IconMargin { get; set; }

            public string Text { get; set; }

            public string OverrideColor { get; set; }

            public string OverrideFont { get; set; }

            public bool Bold { get; set; }

            public int TextSize { get; set; } = 14;

            public float TitleMarginBottom { get; set; } = 10;
        }

        public class Border
        {
            public bool HasBorder { get; set; } = false;

            public SKColor BorderColor { get; set; } = SKColors.Transparent;

            public float BorderWidth { get; set; } = 0;

            public float BorderRadius { get; set; } = 0;
        }

        public enum Position
        {
            Top,
            Center,
            Bottom,
            Left
        }

        public enum Layout
        {
            Fill,
            Left,
            Minimal,
            FixedWidth
        }

        /// <summary>
        /// 填充模式的百分比占比
        /// </summary>
        public virtual float FillPercentage { get; set; } = 100;

        /// <summary>
        /// 固定宽度的数值
        /// </summary>
        public virtual float FixedWidth { get; set; } = 0;

        /// <summary>
        /// 默认标题栏
        /// </summary>
        public Title DrawingTitle { get; set; } = new();

        public Border DrawingBorder { get; set; } = new();

        /// <summary>
        /// 布局选项
        /// </summary>
        public Layout DrawingLayout { get; set; } = Layout.Fill;

        public virtual DrawingItemBase[] Content { get; set; } = [];

        /// <summary>
        /// 若绘制边框, 则使用的圆角
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// 对背景的高斯模糊
        /// </summary>
        public float BackgroundBlur { get; set; }

        /// <summary>
        /// 强制高度
        /// </summary>
        public virtual float ContentHeight { get; set; }

        /// <summary>
        /// Margin
        /// </summary>
        public virtual Thickness Margin { get; set; } = Thickness.DefaultMargin;

        /// <summary>
        /// Padding
        /// </summary>
        public virtual Thickness Padding { get; set; } = Thickness.DefaultPadding;

        /// <summary>
        /// 遍历绘制所有Item
        /// </summary>
        /// <param name="painting">内容画布</param>
        /// <param name="startPoint">当前绘画起始点</param>
        /// <param name="width">可用宽度(包括Margin)</param>
        /// <param name="theme">主题</param>
        /// <param name="palette">调色板</param>
        /// <returns>绘制结束点位（右下角），实际使用宽度，实际使用高度</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual (SKPoint endPoint, float height) Draw(Painting painting, SKPoint startPoint, float width, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            if (painting == null)
            {
                throw new InvalidOperationException("未提供父画布，无法绘制");
            }
            float startLeft = (int)(startPoint.X + Padding.Left);
            float startTop = (int)(startPoint.Y + Padding.Top);

            SKPoint currentPoint = new(startLeft, startTop);
            // 绘制Title
            if (DrawingTitle != null && DrawingTitle.HasTitle)
            {
                if (DrawingTitle.HasIcon)
                {
                    // 绘制Icon
                    SKRect iconRect = new(currentPoint.X + DrawingTitle.IconMargin.Left, currentPoint.Y + DrawingTitle.IconMargin.Top, currentPoint.X + DrawingTitle.IconSize.Width, currentPoint.Y + DrawingTitle.IconSize.Height);
                    painting.DrawImage(DrawingTitle.IconPath, iconRect);
                    currentPoint.X += DrawingTitle.IconSize.Width + DrawingTitle.IconMargin.Right;
                }
                currentPoint = painting.DrawText(DrawingTitle.Text, Painting.Anywhere
                    , currentPoint
                    , SKColor.Parse(string.IsNullOrEmpty(DrawingTitle.OverrideColor) ? DrawingTitle.OverrideColor : palette.TextColor)
                    , DrawingTitle.TextSize
                    , Painting.CreateCustomFont(DrawingTitle.OverrideFont)
                    , DrawingTitle.Bold);
                currentPoint.X = startLeft;
                currentPoint.Y += DrawingTitle.TitleMarginBottom;
            }
            // 调用各个Item的绘制方法
            float fillPercentage = 0;
            SKPoint rowStartPoint = new(currentPoint.X, currentPoint.Y);
            SKPoint lastEndPoint = SKPoint.Empty;
            List<float> currentRowHeights = [];
            foreach (var item in Content)
            {
                float desireWidth = width;
                // 检查宽度是否溢出
                if (item.Layout == Layout.Fill)
                {
                    if (fillPercentage + item.FillPercentage > 100)
                    {
                        item.FillPercentage = 0;
                    }
                    // 换行
                    NewLine(item.Margin);
                    desireWidth = width / 100f * item.FillPercentage;
                }
                else if (item.Layout == Layout.FixedWidth)
                {
                    desireWidth = item.FixedWidth;
                }
                else if (item.Layout == Layout.Left
                    || item.Layout == Layout.Minimal)
                {
                    desireWidth = width - currentPoint.X - item.Margin.Right;
                }

                if (item.Layout == Layout.Fill)
                {
                    if (fillPercentage + item.FillPercentage > 100)
                    {
                        item.FillPercentage = 0;
                    }
                    // 换行
                    NewLine(item.Margin);
                }
                var (endPoint, actualWidth, actualHeight) = item.Draw(painting, currentPoint, desireWidth, ContentHeight, theme, palette);
                currentRowHeights.Add(actualHeight);
                lastEndPoint = endPoint;
                // 根据填充类型计算下一个开始坐标
                switch (item.Layout)
                {
                    default:
                    case Layout.Left:
                        // 填充模式为剩余所有空间，换行
                        NewLine(item.Margin);
                        break;

                    case Layout.Minimal:
                        // 填充模式为最小宽度，X加Margin
                        currentPoint = new(endPoint.X + item.Margin.Right, currentPoint.Y);
                        break;

                    case Layout.FixedWidth:
                        // 填充模式为固定宽度，X为起始坐标+Margin+Width
                        currentPoint = new(currentPoint.X + item.FixedWidth + item.Margin.Right, currentPoint.Y);
                        break;

                    case Layout.Fill:
                        // 填充模式为百分比宽度，若填充百分比+当前行宽度大于100，则换行
                        if (fillPercentage + item.FillPercentage >= 100)
                        {
                            NewLine(item.Margin);
                        }
                        else
                        {
                            currentPoint = new(currentPoint.X + desireWidth + item.Margin.Right, currentPoint.Y);
                            fillPercentage += item.FillPercentage;
                        }
                        break;
                }
            }
            // 绘制Border
            if (DrawingBorder != null && DrawingBorder.HasBorder)
            {
                painting.DrawRectangle(new SKRect
                {
                    Location = startPoint,
                    Size = new SKSize(width, lastEndPoint.Y + Padding.Bottom)
                }, SKColors.Transparent, DrawingBorder.BorderColor, DrawingBorder.BorderWidth, DrawingBorder.BorderRadius);
            }

            void NewLine(Thickness margin)
            {
                float maxHeight = currentRowHeights.Max();
                currentPoint = new(rowStartPoint.X, rowStartPoint.Y + maxHeight + margin.Bottom);
                fillPercentage = 0;
                currentRowHeights = [];
                rowStartPoint = new(currentPoint.X, currentPoint.Y);
            }

            return (new(lastEndPoint.X + Padding.Right, lastEndPoint.Y + Padding.Bottom), lastEndPoint.Y - startPoint.Y + Padding.Top + Padding.Bottom);
        }
    }
}
