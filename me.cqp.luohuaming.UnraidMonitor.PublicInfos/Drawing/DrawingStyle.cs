using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class DrawingStyle
    {
        public enum BackgroundImageScaleType
        {
            Center,

            Stretch,

            Tile,

            Fill
        }

        public enum BackgroundType
        {
            Color,

            Image
        }

        /// <summary>
        /// 纯色背景色
        /// </summary>
        public string BackgrdoundColor { get; set; }

        /// <summary>
        /// 图片默认应用的高斯模糊
        /// </summary>
        public float BackgroundBlur { get; set; } = 0;

        /// <summary>
        /// 随机背景图片
        /// </summary>
        public string[] BackgroundImages { get; set; } = [];

        public DrawingBase[] Content { get; set; }

        /// <summary>
        /// 主内容对背景的高斯模糊
        /// </summary>
        public float ContentBlur { get; set; }

        /// <summary>
        /// 主内容容器的圆角
        /// </summary>
        public float ContentRadius { get; set; }

        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 背景图片绘制模式
        /// </summary>
        public BackgroundImageScaleType DrawBackgroundImageScaleType { get; set; } = BackgroundImageScaleType.Fill;

        /// <summary>
        /// 背景绘制模式
        /// </summary>
        public BackgroundType DrawBackgroundType { get; set; } = BackgroundType.Color;

        public DateTime ModifyTime { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 主内容的Padding
        /// </summary>
        public Thickness Padding { get; set; }

        /// <summary>
        /// 注意Dispose
        /// </summary>
        /// <param name="width">期望宽度</param>
        /// <returns></returns>
        public Painting Draw(int width)
        {
            // 创建内容画布
            using Painting contentPainting = new((int)(width - Padding.Left - Padding.Right), 10000);
            contentPainting.Clear(SKColors.Transparent);
            float drawHeight = 0;
            // 记录模糊区域
            List<(SKPath path, float blur)> blurAreas = [];
            List<float> currentRowWidths = [];
            List<float> currentRowHeights = [];
            // 绘制内容
            if (Content != null && Content.Length > 0)
            {
                SKPoint startPoint = new(0, 0);
                float desireWidth = contentPainting.Width;
                float fillPercentage = 0;
                foreach (var item in Content)
                {
                    startPoint.X += item.Margin.Left;
                    startPoint.Y += item.Margin.Top;
                    // 检查宽度是否溢出
                    if (item.DrawingLayout == DrawingBase.Layout.Fill)
                    {
                        if (fillPercentage + item.FillPercentage > 100)
                        {
                            item.FillPercentage = 0;
                        }
                        // 换行
                        NewLine(item.Margin);
                    }
                    var (endPoint, actualWidth, actualHeight) = item.Draw(contentPainting, startPoint, desireWidth);
                    currentRowHeights.Add(actualHeight);
                    currentRowWidths.Add(actualWidth);
                    // 记录本子项的模糊区域
                    blurAreas.Add((Painting.CreateRoundedRectPath(new SKRect
                    {
                        Location = new(startPoint.X + item.Margin.Left, startPoint.Y + item.Margin.Top),
                        Size = new(actualHeight, actualHeight)
                    }, item.Radius), item.BackgroundBlur));
                    // 根据填充类型计算下一个开始坐标
                    switch (item.DrawingLayout)
                    {
                        default:
                        case DrawingBase.Layout.Left:
                            // 填充模式为剩余所有空间，换行
                            NewLine(item.Margin);
                            break;

                        case DrawingBase.Layout.Minimal:
                            // 填充模式为最小宽度，X加Margin
                            startPoint = new(endPoint.X + item.Margin.Right, startPoint.Y);
                            break;

                        case DrawingBase.Layout.FixedWidth:
                            // 填充模式为固定宽度，X为起始坐标+Margin+Width
                            startPoint = new(startPoint.X + item.FixedWidth + item.Margin.Right, startPoint.Y);
                            break;

                        case DrawingBase.Layout.Fill:
                            // 填充模式为百分比宽度，若填充百分比+当前行宽度大于100，则换行
                            float fillWidth = contentPainting.Width / 100f * item.FillPercentage;
                            if (fillPercentage + item.FillPercentage >= 100)
                            {
                                NewLine(item.Margin);
                            }
                            else
                            {
                                startPoint = new(startPoint.X + fillWidth + item.Margin.Right, startPoint.Y);
                                fillPercentage += item.FillPercentage;
                            }
                            break;
                    }
                }

                void NewLine(Thickness margin)
                {
                    float maxHeight = currentRowHeights.Max();
                    startPoint = new(0, startPoint.Y + maxHeight + margin.Bottom);
                    fillPercentage = 0;
                    currentRowWidths = [];
                    currentRowHeights = [];
                }
            }
            contentPainting.Resize((int)contentPainting.Width, (int)drawHeight);

            int height = (int)(contentPainting.Height + Padding.Top + Padding.Bottom);
            // 创建背景画布
            Painting painting = new(width, height);
            // 绘制背景
            switch (DrawBackgroundType)
            {
                case BackgroundType.Color:
                    SKColor color = SKColor.Parse(BackgrdoundColor);
                    painting.Clear(color);
                    break;

                case BackgroundType.Image:
                    var filePath = BackgroundImages.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                    if (!File.Exists(filePath))
                    {
                        MainSave.CQLog.Warning("绘制背景", $"{filePath} 背景文件不存在，无法绘制");
                        break;
                    }
                    var img = painting.LoadImage(BackgroundImages[0]);
                    var imgWidth = img.Width;
                    var imgHeight = img.Height;

                    switch (DrawBackgroundImageScaleType)
                    {
                        case BackgroundImageScaleType.Center:
                            var centerPoint = new SKPoint(width / 2.0f, height / 2.0f);
                            var drawRect = new SKRect()
                            {
                                Location = new(centerPoint.X - imgWidth / 2.0f, centerPoint.Y - imgHeight / 2.0f),
                                Size = new(imgWidth, imgHeight)
                            };
                            painting.DrawImage(img, drawRect);
                            break;

                        case BackgroundImageScaleType.Stretch:
                            var stretchRect = new SKRect()
                            {
                                Location = new(0, 0),
                                Size = new(width, height)
                            };
                            painting.DrawImage(img, stretchRect);
                            break;

                        case BackgroundImageScaleType.Tile:
                            painting.FillTileBackground(img);
                            break;

                        case BackgroundImageScaleType.Fill:
                            float scale = Math.Max(width / imgWidth, height / imgHeight);
                            float scaledW = imgWidth * scale;
                            float scaledH = imgHeight * scale;

                            painting.DrawImage(img, new SKRect
                            {
                                Location = new SKPoint((width - scaledW) / 2f, (height - scaledH) / 2f),
                                Size = new SKSize(scaledW, scaledH)
                            });
                            break;

                        default:
                            break;
                    }
                    // 模糊背景
                    if (BackgroundBlur > 0)
                    {
                        painting.Blur(Painting.CreateRoundedRectPath(new SKRect { Location = new(), Size = new(width, height) }, 0), BackgroundBlur);
                    }
                    break;
                default:
                    break;
            }
            // 绘制内容容器模糊背景
            if (ContentBlur > 0)
            {
                var roundRect = Painting.CreateRoundedRectPath(new SKRect
                {
                    Location = new(Padding.Left, Padding.Top),
                    Size = new(contentPainting.Width, contentPainting.Height)
                }, ContentRadius);
                painting.Blur(roundRect, ContentBlur);
            }
            // 将内容画布绘制到主画布上
            painting.DrawImage(contentPainting.SnapShot(), new SKRect
            {
                Location = new(Padding.Left, Padding.Top),
                Size = new(contentPainting.Width, contentPainting.Height)
            });
            // 根据模糊区域绘制模糊
            foreach (var (path, blur) in blurAreas)
            {
                painting.Blur(path, blur);
            }
            // 返回画布
            return painting;
        }
    }
}