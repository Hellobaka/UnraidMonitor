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

        public enum Theme
        {
            WinUI3,
            Unraid,
            MaterialDesign3,
            MaterialDesign2,
        }

        /// <summary>
        /// WinUI3 Dark
        /// </summary>
        public class Colors
        {
            public string AccentColor { get; set; } = "#945FD7";
          
            public string TextColor { get; set; } = "#FFFFFF";
          
            public string BackgroundColor { get; set; } = "#373737";
          
            public string SuccessColor { get; set; } = "#393D1B";
          
            public string WarningColor { get; set; } = "#433519";
          
            public string InfoColor { get; set; } = "#272727";
          
            public string FatalColor { get; set; } = "#442726";
          
            public string SuccessIconColor { get; set; } = "#6CCB5F";
          
            public string WarningIconColor { get; set; } = "#FCE100";
          
            public string InfoIconColor { get; set; } = "#945FD7";
          
            public string FatalIconColor { get; set; } = "#FF99A4";
        }

        /// <summary>
        /// 纯色背景色
        /// </summary>
        public string BackgroundColor { get; set; }

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

        public Theme ItemTheme { get; set; } = Theme.WinUI3;

        public Colors Palette { get; set; } = new();

        public static Colors GetThemeDefaultColor(Theme theme, bool dark) => (theme, dark) switch
        {
            (Theme.WinUI3, true) => new Colors(),
            (Theme.WinUI3, false) => new Colors
            {
                AccentColor = "#945FD7",
                TextColor = "#000000",
                BackgroundColor = "#CCCCCC",
                FatalColor = "#FDE7E9",
                FatalIconColor = "#C42B1C",
                InfoColor = "#F4F4F4",
                InfoIconColor = "#945FD7",
                SuccessColor = "#DFF6DD",
                SuccessIconColor = "#0F7B0F",
                WarningColor = "#FFF4CE",
                WarningIconColor = "#9D5D00",
            },
            (Theme.Unraid, false) => new Colors
            {
                AccentColor = "#FA7C2F",
                TextColor = "#FFFFFF",
                BackgroundColor = "#2B2A29"
            },
            (Theme.Unraid, true) => new Colors
            {
                AccentColor = "#FA7C2F",
                TextColor = "#FFFFFF",
                BackgroundColor = "#2B2A29"
            },
            (Theme.MaterialDesign3, false) => new Colors
            {
                AccentColor = "#65558F",
                TextColor = "#000000",
                BackgroundColor = "#E8DEF8",
                FatalColor = "#FEF4F3",
                FatalIconColor = "#F55D52",
                InfoColor = "#F2F9FE",
                InfoIconColor = "#36A5F5",
                SuccessColor = "#F0FCF5",
                SuccessIconColor = "#18D988",
                WarningColor = "#FFF9F0",
                WarningIconColor = "#FFA118",
            },
            (Theme.MaterialDesign3, true) => new Colors
            {
                AccentColor = "#675496",
                TextColor = "#000000",
                BackgroundColor = "#E2E0F9",
                FatalColor = "#2B2030",
                FatalIconColor = "#DF3752",
                InfoColor = "#202439",
                InfoIconColor = "#446BC9",
                SuccessColor = "#202830",
                SuccessIconColor = "#38A960",
                WarningColor = "#2B272E",
                WarningIconColor = "#EAA743",
            },
            (Theme.MaterialDesign2, false) => new Colors
            {
                AccentColor = "#5411F5",
                TextColor = "#000000",
                BackgroundColor = "#E5E5E5",
                FatalColor = "#FEF4F3",
                FatalIconColor = "#F55D52",
                InfoColor = "#F2F9FE",
                InfoIconColor = "#36A5F5",
                SuccessColor = "#F0FCF5",
                SuccessIconColor = "#18D988",
                WarningColor = "#FFF9F0",
                WarningIconColor = "#FFA118",
            },
            (Theme.MaterialDesign2, true) => new Colors
            {
                AccentColor = "#5411F5",
                TextColor = "#000000",
                BackgroundColor = "#E5E5E5",
                FatalColor = "#2B2030",
                FatalIconColor = "#DF3752",
                InfoColor = "#202439",
                InfoIconColor = "#446BC9",
                SuccessColor = "#202830",
                SuccessIconColor = "#38A960",
                WarningColor = "#2B272E",
                WarningIconColor = "#EAA743",
            },
            _ => new Colors(),
        };

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
            List<float> currentRowHeights = [];
            // 绘制内容
            if (Content != null && Content.Length > 0)
            {
                SKPoint startPoint = new(0, 0);
                float fillPercentage = 0;
                foreach (var item in Content)
                {
                    startPoint.X += item.Margin.Left;
                    startPoint.Y += item.Margin.Top;
                    float desireWidth = contentPainting.Width;
                    // 检查宽度是否溢出
                    if (item.DrawingLayout == DrawingBase.Layout.Fill)
                    {
                        if (fillPercentage + item.FillPercentage > 100)
                        {
                            item.FillPercentage = 0;
                        }
                        // 换行
                        NewLine(item.Margin);
                        desireWidth = contentPainting.Width / 100f * item.FillPercentage;
                    }
                    else if(item.DrawingLayout == DrawingBase.Layout.FixedWidth)
                    {
                        desireWidth = item.FixedWidth;
                    }
                    else if(item.DrawingLayout == DrawingBase.Layout.Left
                        || item.DrawingLayout == DrawingBase.Layout.Minimal)
                    {
                        desireWidth = contentPainting.Width - startPoint.X - item.Margin.Right;
                    }

                    var (endPoint, actualHeight) = item.Draw(contentPainting, startPoint, desireWidth, ItemTheme, Palette);
                    currentRowHeights.Add(actualHeight);
                    // 记录子项的模糊区域
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
                            if (fillPercentage + item.FillPercentage >= 100)
                            {
                                NewLine(item.Margin);
                            }
                            else
                            {
                                startPoint = new(startPoint.X + desireWidth + item.Margin.Right, startPoint.Y);
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
                    SKColor color = SKColor.Parse(BackgroundColor);
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