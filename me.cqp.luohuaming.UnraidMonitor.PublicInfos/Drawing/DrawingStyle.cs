using Newtonsoft.Json;
using PropertyChanged;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using static me.cqp.luohuaming.UnraidMonitor.PublicInfos.MainSave;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class DrawingStyle : INotifyPropertyChanged
    {
        private static Dictionary<string, DrawingStyle> StyleCache { get; set; } = [];

        public enum BackgroundImageScaleType
        {
            [Description("居中绘制")]
            Center,

            [Description("拉伸填充")]
            Stretch,

            [Description("平铺背景")]
            Tile,

            [Description("填充背景")]
            Fill
        }

        public enum BackgroundType
        {
            [Description("纯色背景")]
            Color,

            [Description("随机背景图片")]
            Image
        }

        public enum Theme
        {
            Unknown,
            WinUI3,
            Unraid,
            MaterialDesign3,
            MaterialDesign2,
        }

        /// <summary>
        /// WinUI3 Dark
        /// </summary>
        public class Colors : INotifyPropertyChanged
        {
            /// <summary>
            /// 主题色
            /// </summary>
            public string AccentColor { get; set; } = "#75B6E7";

            /// <summary>
            /// 渐变或备用颜色
            /// </summary>
            public string Accent2Color { get; set; }

            public string TextColor { get; set; } = "#FFFFFF";

            public string BackgroundColor { get; set; } = "#373737";

            public string SuccessColor { get; set; } = "#393D1B";

            public string WarningColor { get; set; } = "#433519";

            public string InfoColor { get; set; } = "#373737";

            public string FatalColor { get; set; } = "#442726";

            public string SuccessIconColor { get; set; } = "#6CCB5F";

            public string WarningIconColor { get; set; } = "#FCE100";

            public string InfoIconColor { get; set; } = "#75B6E7";

            public string FatalIconColor { get; set; } = "#FF99A4";

            public event PropertyChangedEventHandler PropertyChanged;
            public event PropertyChangeEventArg OnPropertyChangedDetail;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                OnPropertyChangedDetail?.Invoke(GetType().GetProperty(propertyName), null, GetType().GetProperty(propertyName)?.GetValue(this), null);
            }
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
        public Thickness Padding { get; set; } = Thickness.DefaultPadding;

        public Theme ItemTheme { get; set; } = Theme.WinUI3;

        public Colors Palette { get; set; } = new();

        public bool LayoutDebug { get; set; }

        public int Width { get; set; } = 1000;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangeEventArg OnPropertyChangedDetail;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChangedDetail?.Invoke(GetType().GetProperty(propertyName), null, GetType().GetProperty(propertyName)?.GetValue(this), null);
        }

        public void Init()
        {
            Palette.OnPropertyChangedDetail -= Palette_OnPropertyChangedDetail;
            Palette.OnPropertyChangedDetail += Palette_OnPropertyChangedDetail;
        }

        private void Palette_OnPropertyChangedDetail(PropertyInfo propertyInfo, PropertyInfo parentPropertyType, object newValue, object oldValue)
        {
            OnPropertyChangedDetail(propertyInfo, GetType().GetProperty(nameof(Palette)), newValue, oldValue);
        }

        public static Colors GetThemeDefaultColor(Theme theme, bool dark) => (theme, dark) switch
        {
            (Theme.WinUI3, true) => new Colors(),
            (Theme.WinUI3, false) => new Colors
            {
                AccentColor = "#005FB8",
                TextColor = "#000000",
                BackgroundColor = "#CCCCCC",
                FatalColor = "#FDE7E9",
                FatalIconColor = "#C42B1C",
                InfoColor = "#F4F4F4",
                InfoIconColor = "#005FB8",
                SuccessColor = "#DFF6DD",
                SuccessIconColor = "#0F7B0F",
                WarningColor = "#FFF4CE",
                WarningIconColor = "#9D5D00",
            },
            (Theme.Unraid, false) => new Colors
            {
                Accent2Color = "#FF8C30",
                AccentColor = "#E22829",
                TextColor = "#000000",
                BackgroundColor = "#f2f2f2",
                FatalColor = "#FF9E9E",
                FatalIconColor = "#F0000C",
                InfoColor = "#808080",
                InfoIconColor = "#42453E",
                SuccessColor = "#DFF2BF",
                SuccessIconColor = "#4F8A10",
                WarningColor = "#FEEFB3",
                WarningIconColor = "#E68A00",
            },
            (Theme.Unraid, true) => new Colors
            {
                Accent2Color = "#FF8C30",
                AccentColor = "#E22829",
                TextColor = "#FFFFFF",
                BackgroundColor = "#1b1d1b",
                FatalColor = "#FF9E9E",
                FatalIconColor = "#F0000C",
                InfoColor = "#808080",
                InfoIconColor = "#42453E",
                SuccessColor = "#4f8A10",
                SuccessIconColor = "#DFF2BF",
                WarningColor = "#FEEFB3",
                WarningIconColor = "#E68A00",
            },
            (Theme.MaterialDesign3, false) => new Colors
            {
                AccentColor = "#65558F",
                TextColor = "#000000",
                BackgroundColor = "#E8DEF8",
                FatalColor = "#FEECEB",
                FatalIconColor = "#F55D52",
                InfoColor = "#EDEBF2",
                InfoIconColor = "#65558F",
                SuccessColor = "#E4FBF1",
                SuccessIconColor = "#18D988",
                WarningColor = "#FFF4E4",
                WarningIconColor = "#FFA118",
            },
            (Theme.MaterialDesign3, true) => new Colors
            {
                AccentColor = "#D0BCFF",
                TextColor = "#FFFFFF",
                BackgroundColor = "#4A4458",
                FatalColor = "#2B2030",
                FatalIconColor = "#DF3752",
                InfoColor = "#24202E",
                InfoIconColor = "#D0BCFF",
                SuccessColor = "#202830",
                SuccessIconColor = "#38A960",
                WarningColor = "#2B272E",
                WarningIconColor = "#EAA743",
            },
            (Theme.MaterialDesign2, false) => new Colors
            {
                AccentColor = "#A298FE",
                TextColor = "#000000",
                BackgroundColor = "#DEDBF9",
                FatalColor = "#FEECEB",
                FatalIconColor = "#F55D52",
                InfoColor = "#F4F3FF",
                InfoIconColor = "#A298FE",
                SuccessColor = "#E4FBF1",
                SuccessIconColor = "#18D988",
                WarningColor = "#FFF4E4",
                WarningIconColor = "#FFA118",
            },
            (Theme.MaterialDesign2, true) => new Colors
            {
                AccentColor = "#7E6FFF",
                TextColor = "#FFFFFF",
                BackgroundColor = "#DEDBF9",
                FatalColor = "#2B2030",
                FatalIconColor = "#FE173F",
                InfoColor = "#292737",
                InfoIconColor = "#7E6FFF",
                SuccessColor = "#202830",
                SuccessIconColor = "#2FB15B",
                WarningColor = "#2B272E",
                WarningIconColor = "#FFA724",
            },
            _ => new Colors(),
        };

        public static string GetThemeDefaultFont(Theme theme) => theme switch
        {
            Theme.Unraid => "Nudista",
            Theme.MaterialDesign3 => "Google Sans",
            Theme.MaterialDesign2 => "Roboto",
            _ => "微软雅黑",
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
            float drawHeight = 1;
            // 记录模糊区域
            List<(SKPath path, float blur)> blurAreas = [];
            List<float> currentRowHeights = [];
            bool hasNewLine = false;
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
                    if (item.DrawingLayout == DrawingBase.Layout.Percentage)
                    {
                        if (fillPercentage + item.FillPercentage > 100)
                        {
                            item.FillPercentage = 100;
                            NewLine(item.Margin);
                        }
                        desireWidth = contentPainting.Width / 100f * item.FillPercentage;
                    }
                    else if (item.DrawingLayout == DrawingBase.Layout.FixedWidth)
                    {
                        desireWidth = item.FixedWidth;
                    }
                    else if (item.DrawingLayout == DrawingBase.Layout.Remaining
                        || item.DrawingLayout == DrawingBase.Layout.Minimal)
                    {
                        desireWidth = contentPainting.Width - startPoint.X;
                    }
                    desireWidth -= item.Margin.Left + item.Margin.Right;

                    var (endPoint, actualHeight) = item.Draw(contentPainting, startPoint, desireWidth, ItemTheme, Palette);
                    currentRowHeights.Add(actualHeight);
                    if (LayoutDebug)
                    {
                        contentPainting.DrawRectangle(new()
                        {
                            Location = startPoint,
                            Size = new(desireWidth, actualHeight)
                        }, SKColors.Transparent, SKColors.White, 1, null, 0);
                        contentPainting.DrawRectangle(new()
                        {
                            Location = new(startPoint.X - item.Margin.Left, startPoint.Y - item.Margin.Top),
                            Size = new(desireWidth + item.Margin.Left + item.Margin.Right, actualHeight + item.Margin.Top + item.Margin.Bottom)
                        }, SKColors.Transparent, SKColors.IndianRed, 1, null, 0);
                    }
                    // 记录子项的模糊区域
                    blurAreas.Add((Painting.CreateRoundedRectPath(new SKRect
                    {
                        Location = new(startPoint.X, startPoint.Y),
                        Size = new(desireWidth + item.Margin.Left, actualHeight + item.Margin.Top)
                    }, item.Radius), item.BackgroundBlur));
                    // 根据填充类型计算下一个开始坐标
                    switch (item.DrawingLayout)
                    {
                        default:
                        case DrawingBase.Layout.Remaining:
                            // 填充模式为剩余所有空间，换行
                            NewLine(item.Margin);
                            break;

                        case DrawingBase.Layout.Minimal:
                            // 填充模式为最小宽度，X加Margin
                            startPoint = new(endPoint.X + item.Margin.Right, startPoint.Y - item.Margin.Top);
                            break;

                        case DrawingBase.Layout.FixedWidth:
                            // 填充模式为固定宽度，X为起始坐标+Margin+Width
                            startPoint = new(startPoint.X + item.FixedWidth + item.Margin.Right, startPoint.Y - item.Margin.Top);
                            break;

                        case DrawingBase.Layout.Percentage:
                            // 填充模式为百分比宽度，若填充百分比+当前行宽度大于100，则换行
                            if (fillPercentage + item.FillPercentage >= 100)
                            {
                                NewLine(item.Margin);
                            }
                            else
                            {
                                startPoint = new(startPoint.X + desireWidth + item.Margin.Right, startPoint.Y - item.Margin.Top);
                                fillPercentage += item.FillPercentage;
                            }
                            break;
                    }
                }

                if (!hasNewLine)
                {
                    NewLine(Thickness.DefaultMargin);
                }
                void NewLine(Thickness margin)
                {
                    hasNewLine = true;
                    float maxHeight = currentRowHeights.Count > 0 ? currentRowHeights.Max() : 0;
                    drawHeight += maxHeight + margin.Bottom + margin.Top;
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
            // 根据模糊区域绘制模糊
            foreach (var (path, blur) in blurAreas)
            {
                painting.Blur(path, blur);
            }
            // 将内容画布绘制到主画布上
            painting.DrawImage(contentPainting.SnapShot(), new SKRect
            {
                Location = new(Padding.Left, Padding.Top),
                Size = new(contentPainting.Width, contentPainting.Height)
            });
            contentPainting.Dispose();
            // 返回画布
            return painting;
        }

        public string Serialize() => JsonConvert.SerializeObject(this, new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        });

        public static DrawingStyle LoadFromFile(string path)
        {
            string pathA = Path.Combine(MainSave.AppDirectory, path);
            string pathB = path;
            if (File.Exists(pathA))
            {
                path = pathA;
            }
            else if (File.Exists(pathB))
            {
                path = pathB;
            }
            else
            {
                return null;
            }
            if (StyleCache.TryGetValue(Path.GetFullPath(path), out var style))
            {
                return style;
            }

            style = JsonConvert.DeserializeObject<DrawingStyle>(File.ReadAllText(path), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
            });
            StyleCache.Add(Path.GetFullPath(path), style);
            style.Init();
            return style;
        }
    }
}