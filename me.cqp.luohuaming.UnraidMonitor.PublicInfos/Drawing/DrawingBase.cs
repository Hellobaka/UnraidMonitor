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

            public string OverrideColor2 { get; set; }

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
        }

        public enum Layout
        {
            Percentage,
            Remaining,
            Minimal,
            FixedWidth
        }

        public enum AlertType
        {
            Info,
            Warning,
            Fatal,
            Success
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
        public Layout DrawingLayout { get; set; } = Layout.Percentage;

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
        /// Margin
        /// </summary>
        public virtual Thickness Margin { get; set; } = Thickness.DefaultMargin;

        /// <summary>
        /// Padding
        /// </summary>
        public virtual Thickness Padding { get; set; } = Thickness.DefaultPadding;

        public bool LayoutDebug { get; set; }

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
                currentPoint = DrawTitle(painting, theme, palette, startLeft, currentPoint);
            }
            // 调用各个Item的绘制方法
            currentPoint = DrawItems(painting, currentPoint, width - Padding.Left - Padding.Right, theme, palette);
            // 绘制Border
            if (DrawingBorder != null && DrawingBorder.HasBorder)
            {
                DrawBorder(painting, startPoint, currentPoint, width);
            }

            return (new(currentPoint.X + Padding.Right, currentPoint.Y + Padding.Bottom), currentPoint.Y - startPoint.Y + Padding.Top + Padding.Bottom);
        }

        private void DrawBorder(Painting painting, SKPoint startPoint, SKPoint endPoint, float width)
        {
            painting.DrawRectangle(new SKRect
            {
                Location = startPoint,
                Size = new SKSize(width, endPoint.Y + Padding.Bottom)
            }, SKColors.Transparent, DrawingBorder.BorderColor, DrawingBorder.BorderWidth, null, DrawingBorder.BorderRadius);
        }

        private SKPoint DrawItems(Painting painting, SKPoint currentPoint, float width, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            List<List<(Painting itemCanvas, DrawingItemBase item, float width, float height)>> preDraw = [];
            List<(Painting itemCanvas, DrawingItemBase item, float width, float height)> currentLine = [];
            preDraw.Add(currentLine);
            foreach (var item in Content)
            {
                item.ApplyBinding();
                float desireWidth = width;
                // 计算宽度
                if (item.Layout == Layout.Percentage)
                {
                    desireWidth = width / 100f * item.FillPercentage;
                }
                else if (item.Layout == Layout.FixedWidth)
                {
                    desireWidth = item.FixedWidth;
                }
                else if (item.Layout == Layout.Remaining
                    || item.Layout == Layout.Minimal)
                {
                    desireWidth = width;
                }
                desireWidth -= item.Margin.Left + item.Margin.Right;
                if (item.Layout == Layout.Remaining)
                {
                    // Remaining模式需要在本行元素宽度已知之后才能绘制
                    currentLine.Add((null, item, 0, item.CalcHeight(theme)));
                    if (item.AfterNewLine)
                    {
                        currentLine = new();
                        preDraw.Add(currentLine);
                    }
                    continue;
                }
                Painting itemCanvas = new((int)Math.Ceiling(desireWidth), 1000);
                var (endPoint, actualWidth, actualHeight) = item.Draw(itemCanvas, new(), desireWidth, item.OverrideTheme == DrawingStyle.Theme.Unknown ? theme : item.OverrideTheme, item.OverridePalette ?? palette);
                itemCanvas.Resize((int)Math.Ceiling(actualWidth), (int)Math.Ceiling(actualHeight));
                currentLine.Add((itemCanvas, item, actualWidth, actualHeight));
                if (item.AfterNewLine)
                {
                    currentLine = new();
                    preDraw.Add(currentLine);
                }
            }
            SKPoint startPoint = new(currentPoint.X, currentPoint.Y);
            foreach(var line in preDraw)
            {
                if (line.Count == 0)
                {
                    continue;
                }
                var maxLeftMargin = line.Max(i => i.item.Margin.Left);
                var maxRightMargin = line.Max(i => i.item.Margin.Right);
                var maxTopMargin = line.Max(i => i.item.Margin.Top);
                var maxBottomMargin = line.Max(i => i.item.Margin.Bottom);
                var maxHeight = line.Max(i => i.height);
                var maxWidth = line.Max(i => i.width);
                var remainingCount = line.Count(i => i.item.Layout == Layout.Remaining);
                var remainingItemWidth = (width - line.Where(i => i.item.Layout != Layout.Remaining).Sum(i => i.width + i.item.Margin.Left + i.item.Margin.Right)) / remainingCount;

                currentPoint.X = startPoint.X;
                currentPoint.Y += maxTopMargin;
                foreach (var item in line)
                {
                    Painting canvas = item.itemCanvas;
                    float drawWidth = item.width;
                    float drawHeight = item.height;
                    if (item.item.Layout == Layout.Remaining)
                    {
                        float widthWithoutMargin = remainingItemWidth - item.item.Margin.Left - item.item.Margin.Right;
                        canvas = new((int)Math.Ceiling(widthWithoutMargin), 1000);
                        var (_, w, h) = item.item.Draw(canvas, new(), widthWithoutMargin, item.item.OverrideTheme == DrawingStyle.Theme.Unknown ? theme : item.item.OverrideTheme, item.item.OverridePalette ?? palette);
                        canvas.Resize((int)Math.Ceiling(w), (int)Math.Ceiling(h));
                        drawWidth = w;
                        drawHeight = h;

                        maxHeight = Math.Max(maxHeight, drawHeight);
                        maxBottomMargin = Math.Max(maxBottomMargin, item.item.Margin.Bottom);
                    }
                    SKPoint drawPoint = new(x: currentPoint.X + item.item.Margin.Left, y: currentPoint.Y + maxTopMargin + item.item.VerticalAlignment switch
                    {
                        Position.Center => maxTopMargin + (maxHeight / 2) - (item.height / 2),
                        Position.Bottom => maxHeight - item.height - item.item.Margin.Bottom,
                        _ => item.item.Margin.Top,
                    });
                    painting.DrawImage(canvas.SnapShot(), new()
                    {
                        Location = drawPoint,
                        Size = new(drawWidth, drawHeight)
                    });
                    if (LayoutDebug)
                    {
                        painting.DrawRectangle(new()
                        {
                            Location = drawPoint,
                            Size = new(drawWidth, drawHeight)
                        }, SKColors.Transparent, SKColors.White, 1, null, 0);
                        painting.DrawRectangle(new()
                        {
                            Location = new(drawPoint.X - item.item.Margin.Left, drawPoint.Y - item.item.Margin.Top),
                            Size = new(drawWidth + item.item.Margin.Left + item.item.Margin.Right, drawHeight + +item.item.Margin.Top + item.item.Margin.Bottom)
                        }, SKColors.Transparent, SKColors.IndianRed, 1, null, 0);
                    }
                    canvas.Dispose();
                    currentPoint.X += drawWidth + item.item.Margin.Left + item.item.Margin.Right;
                }
                currentPoint.Y = currentPoint.Y + maxHeight + maxBottomMargin;
            }
            currentPoint.X = startPoint.X;
            return currentPoint;
        }

        private SKPoint DrawTitle(Painting painting, DrawingStyle.Theme theme, DrawingStyle.Colors palette, float startLeft, SKPoint currentPoint)
        {
            if (DrawingTitle.HasIcon)
            {
                // 绘制Icon
                SKRect iconRect = new()
                {
                    Location = new(currentPoint.X + DrawingTitle.IconMargin.Left, currentPoint.Y + DrawingTitle.IconMargin.Top),
                    Size = DrawingTitle.IconSize
                };
                painting.DrawImage(DrawingTitle.IconPath, iconRect);
                currentPoint.X += DrawingTitle.IconSize.Width + DrawingTitle.IconMargin.Left + DrawingTitle.IconMargin.Right;
            }
            var font = Painting.CreateCustomFont(!string.IsNullOrEmpty(DrawingTitle.OverrideFont) ? DrawingTitle.OverrideFont : DrawingStyle.GetThemeDefaultFont(theme));
            var size = Painting.MeasureString(DrawingTitle.Text, DrawingTitle.TextSize, font);
            if (DrawingTitle.HasIcon)
            {
                // 垂直居中于图标
                currentPoint.Y += (DrawingTitle.IconSize.Height - size.Height) / 2 + DrawingTitle.IconMargin.Top;
            }
            SKShader shader = null;
            if (!string.IsNullOrEmpty(DrawingTitle.OverrideColor2))
            {
                shader = SKShader.CreateLinearGradient(
                    new SKPoint(currentPoint.X, currentPoint.Y),
                    new SKPoint(currentPoint.X + size.Width, currentPoint.Y),
                    new SKColor[] { SKColor.Parse(DrawingTitle.OverrideColor), SKColor.Parse(DrawingTitle.OverrideColor2) },
                    null,
                    SKShaderTileMode.Clamp);
            }
            currentPoint = painting.DrawText(DrawingTitle.Text, Painting.Anywhere
                , currentPoint
                , SKColor.Parse(!string.IsNullOrEmpty(DrawingTitle.OverrideColor) ? DrawingTitle.OverrideColor : palette.TextColor)
                , shader
                , DrawingTitle.TextSize
                , font
                , DrawingTitle.Bold);
            currentPoint.X = startLeft;
            currentPoint.Y += DrawingTitle.TitleMarginBottom;
            return currentPoint;
        }
    }
}
