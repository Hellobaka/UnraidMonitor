using SkiaSharp;
using System;
using System.ComponentModel;
using System.IO;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_RunningStatus : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.RunningStatus;

        public override DrawingCanvas.Layout Layout { get; set; } = DrawingCanvas.Layout.Minimal;

        public int TextSize { get; set; } = 26;

        [Bindable(BindableSupport.Yes)]
        [Description("标题")]
        public string Title { get; set; } = "";

        public bool IsTitleBold { get; set; }

        [Bindable(BindableSupport.Yes)]
        [Description("运行状态")]
        public bool Running { get; set; }

        [Bindable(BindableSupport.Yes)]
        [Description("运行中文本")]
        public string RunningText { get; set; } = "运行中";

        [Bindable(BindableSupport.Yes)]
        [Description("停止运行文本")]
        public string NotRunningText { get; set; } = "已停止";

        public bool RunningStatusHasIcon { get; set; } = true;

        [Bindable(BindableSupport.Yes)]
        [Description("图标相对路径")]
        public string ImagePath { get; set; } = "images\\default.png";

        public bool HasImage { get; set; }

        public Converter RunningConverter { get; set; }

        public override void BeforeBinding()
        {
            if (Binding.Value.ContainsKey("Title"))
            {
                Title = string.Empty;
            }
            if (Binding.Value.ContainsKey("RunningText"))
            {
                RunningText = "运行中";
            }
            if (Binding.Value.ContainsKey("NotRunningText"))
            {
                NotRunningText = "已停止";
            }
            if (Binding.Value.ContainsKey("ImagePath"))
            {
                ImagePath = "images\\default.png";
            }
            if (Binding.Value.ContainsKey("Running"))
            {
                Running = false;
            }
        }

        public override void ApplyBinding()
        {
            base.ApplyBinding();
            if (Binding == null)
            {
                return;
            }
            if (Binding.Value.TryGetValue("Title", out var data))
            {
                Title = data.FormattedString;
            }
            if (Binding.Value.TryGetValue("RunningText", out data))
            {
                RunningText = data.FormattedString;
            }
            if (Binding.Value.TryGetValue("NotRunningText", out data))
            {
                NotRunningText = data.FormattedString;
            }
            if (Binding.Value.TryGetValue("ImagePath", out data))
            {
                ImagePath = data.FormattedString;
            }
            if (Binding.Value.TryGetValue("Running", out data))
            {
                var item = data.FormattedString;
                if (RunningConverter != null)
                {
                    Running = RunningConverter.Running == item;
                }
            }
        }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            var font = Painting.CreateCustomFont(DrawingStyle.GetThemeDefaultFont(theme));
            string statusText = Running ? RunningText : NotRunningText;
            using Painting textPainting = new(500, 1000);
            var endPoint = textPainting.DrawText(Title, Painting.Anywhere, new(), SKColor.Parse(palette.TextColor), isBold: IsTitleBold, customFont: font);
            SKPoint statusPoint = new SKPoint(startPoint.X, endPoint.Y + TextSize / 2);
            if (RunningStatusHasIcon)
            {
                if (Running)
                {
                    SKPoint p1 = statusPoint;
                    SKPoint p2 = new(statusPoint.X, statusPoint.Y + TextSize);
                    SKPoint p3 = new((float)(statusPoint.X + TextSize * Math.Cos(30 * Math.PI / 180.0)), (float)(statusPoint.Y + TextSize * Math.Sin(30 * Math.PI / 180.0)));
                    using var path = new SKPath();
                    path.MoveTo(p1);
                    path.LineTo(p2);
                    path.LineTo(p3);
                    path.Close();

                    using var paint = new SKPaint()
                    {
                        Color = SKColor.Parse(palette.SuccessColor),
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill
                    };
                    textPainting.MainCanvas.DrawPath(path, paint);
                    statusPoint.X = p3.X + 15;
                }
                else
                {
                    textPainting.DrawRectangle(new()
                    {
                        Location = statusPoint,
                        Size = new(TextSize, TextSize)
                    }, SKColor.Parse(palette.FatalColor), SKColors.Transparent, 0);
                    statusPoint.X += TextSize + 15;
                }
                statusPoint.Y -= 5;
            }
            var endPoint2 = textPainting.DrawText(statusText, Painting.Anywhere, statusPoint, SKColor.Parse(palette.TextColor), customFont: font);
            textPainting.Resize((int)Math.Ceiling(Math.Max(endPoint.X, endPoint2.X)), (int)endPoint2.Y + 5);
            string pathA = Path.Combine(MainSave.AppDirectory, ImagePath);
            string pathB = Path.Combine(MainSave.ImageDirectory, ImagePath);
            string pathC = ImagePath;
            var textPoint = startPoint;
            if (HasImage)
            {
                string path = "";
                if (File.Exists(pathA))
                {
                    path = pathA;
                }
                else if (File.Exists(pathB))
                {
                    path = pathB;
                }
                else if (File.Exists(pathC))
                {
                    path = pathC;
                }
                if (!string.IsNullOrEmpty(path))
                {
                    var img = painting.LoadImage(ImagePath);
                    painting.DrawImage(img, new SKRect()
                    {
                        Location = startPoint,
                        Size = new SKSize(textPainting.Height, textPainting.Height)
                    });
                    textPoint.X += textPainting.Height + 20;
                }
            }
            painting.DrawImage(textPainting.SnapShot(), new SKRect()
            {
                Location = textPoint,
                Size = new SKSize(textPainting.Width, textPainting.Height)
            });
            return (new(textPoint.X + textPainting.Width, textPoint.Y + textPainting.Height), textPoint.X + textPainting.Width, textPainting.Height);
        }

        public static DrawingItem_RunningStatus DefaultConverter => new()
        {
        };

        public class Converter
        {
            public string Running { get; set; }

            public string NotRunning { get; set; }
        }
    }
}
