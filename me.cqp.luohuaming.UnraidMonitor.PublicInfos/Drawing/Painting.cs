using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class Painting : IDisposable
    {
        public Painting(int width, int height)
        {
            Width = width;
            Height = height;
            MainSurface = SKSurface.Create(new SKImageInfo(width, height));
            MainCanvas.Clear(SKColors.Transparent);

            FallbackFont = CreateCustomFont(AppConfig.FallbackFont);
        }

        public static SKTypeface CreateCustomFont(string fontPathOrName)
        {
            if (!string.IsNullOrEmpty(fontPathOrName) && FontCache.TryGetValue(fontPathOrName, out SKTypeface style))
            {
                return style;
            }
            // 路径 > 名称
            if (!string.IsNullOrEmpty(fontPathOrName) && File.Exists(fontPathOrName))
            {
                // 自定义路径不支持粗体
                var fontFile = SKTypeface.FromFile(fontPathOrName);
                if (fontFile != null)
                {
                    FontCache.Add(fontPathOrName, fontFile);
                }
                else
                {
                    return SKTypeface.Default;
                }
            }

            var font = !string.IsNullOrEmpty(fontPathOrName)
                ? SKTypeface.FromFamilyName(fontPathOrName) ?? SKTypeface.Default
                : SKTypeface.Default;
            if (font != null)
            {
                FontCache.Add(fontPathOrName, font);
                return font;
            }
            else
            {
                return SKTypeface.Default;
            }
        }

        public static SKRect Anywhere { get; set; } = new SKRect { Right = int.MaxValue, Bottom = int.MaxValue };

        public static Dictionary<string, SKTypeface> FontCache { get; set; } = [];

        public float Height { get; set; }

        public float Width { get; set; }

        private static SKPaint AntialiasPaint { get; set; } = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
        };

        private static SKFontManager FontManager { get; set; } = SKFontManager.CreateDefault();

        private SKTypeface FallbackFont { get; set; }

        private bool Disposing { get; set; }

        private SKCanvas MainCanvas => MainSurface.Canvas;

        private SKSurface MainSurface { get; set; }

        public void Clear(SKColor color)
        {
            MainCanvas.Clear(color);
        }

        public SKBitmap ConvertToBitmap(SKImage image)
        {
            return SKBitmap.FromImage(image);
        }

        /// <summary>
        /// 裁切图片为圆形
        /// </summary>
        /// <param name="image">欲进行裁切的图片</param>
        /// <param name="width">圆形直径</param>
        /// <returns></returns>
        public SKImage CreateCircularImage(SKImage image, float width)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            var imageInfo = new SKImageInfo((int)width, (int)width);
            using var surface = SKSurface.Create(imageInfo);
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            using var path = new SKPath();
            path.AddCircle(width / 2, width / 2, width / 2, SKPathDirection.CounterClockwise);
            canvas.ClipPath(path, SKClipOperation.Intersect, true);

            var srcRect = new SKRect(0, 0, image.Width, image.Height);
            var destRect = new SKRect(0, 0, width, width);

            canvas.DrawImage(image, srcRect, destRect, AntialiasPaint);
            canvas.Flush();
            return surface.Snapshot();
        }

        public SKImage CreateCircularImage(string imagePath, float width)
        {
            return CreateCircularImage(LoadImage(imagePath), width);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 在指定位置绘制指定大小的图片
        /// </summary>
        /// <param name="imagePath">欲绘制图片的文件路径</param>
        /// <param name="rect">目标尺寸、大小</param>
        public void DrawImage(string imagePath, SKRect rect)
        {
            if (!File.Exists(imagePath))
            {
                return;
            }
            using var imageStream = File.OpenRead(imagePath);
            using var codec = SKCodec.Create(imageStream);
            var bitmap = SKBitmap.Decode(codec);
            var image = SKImage.FromBitmap(bitmap);

            DrawImage(image, rect);
        }

        /// <summary>
        /// 在指定位置绘制指定大小的图片
        /// </summary>
        /// <param name="imagePath">欲绘制图片的文件路径</param>
        /// <param name="rect">目标位置、大小</param>
        public void DrawImage(SKImage image, SKRect rect)
        {
            MainCanvas.DrawImage(image, rect, AntialiasPaint);
        }

        public void DrawLine(SKPoint startPoint, SKPoint endPoint, SKColor strokeColor, float strokeWidth, SKShader shader = null)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = strokeColor,
                StrokeWidth = strokeWidth
            };
            if (shader != null)
            {
                paint.Shader = shader;
            }
            MainCanvas.DrawLine(startPoint, endPoint, paint);
        }

        public void DrawRectangle(SKRect rect, SKColor fillColor, SKColor strokeColor, float strokeWidth, SKShader shader = null, float radius = 0)
        {
            if (fillColor != SKColors.Transparent)
            {
                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                };
                if (shader == null)
                {
                    paint.Color = fillColor;
                }
                else
                {
                    paint.Shader = shader;
                }
                if (radius > 0)
                {
                    MainCanvas.DrawPath(CreateRoundedRectPath(rect, radius), paint);
                }
                else
                {
                    MainCanvas.DrawRect(rect, paint);
                }
            }

            if (strokeWidth == 0)
            {
                return;
            }
            using var strokePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = strokeColor,
                StrokeWidth = strokeWidth
            };
            if (radius > 0)
            {
                MainCanvas.DrawPath(CreateRoundedRectPath(rect, radius), strokePaint);
            }
            else
            {
                MainCanvas.DrawRect(rect, strokePaint);
            }
        }

        /// <summary>
        /// 相对区域坐标绘制
        /// </summary>
        public SKPoint DrawRelativeText(string text, SKRect area, SKPoint startPoint, SKColor color, float fontSize = 26, SKTypeface customFont = null, bool isBold = false)
        {
            return DrawText(text, area, new SKPoint { X = startPoint.X + area.Left, Y = startPoint.Y + area.Top }, color, null, fontSize, customFont, isBold);
        }

        /// <summary>
        /// 绘制文本，换行，自适应Fallback字体
        /// </summary>
        /// <param name="text">欲绘制的文本</param>
        /// <param name="area">可绘制的区域</param>
        /// <param name="startPoint">起始绝对坐标</param>
        /// <param name="color">文本颜色</param>
        /// <param name="shader">渐变颜色</param>
        /// <param name="customFont">自定义字体</param>
        /// <param name="fontSize">字体大小</param>
        /// <returns>最后一个字符的右下角坐标</returns>
        public SKPoint DrawText(string text, SKRect area, SKPoint startPoint, SKColor color, SKShader shader = null, float fontSize = 26, SKTypeface customFont = null, bool isBold = false)
        {
            var textElementEnumerator = StringInfo.GetTextElementEnumerator(text);
            float currentX = startPoint.X;
            float currentY = startPoint.Y + fontSize;
            float lineGap = fontSize / 2;
            float lineHeight = fontSize + lineGap;

            SKTypeface GetTypeface(SKTypeface baseFont, bool bold)
            {
                return baseFont != null && baseFont.IsBold == bold
                    ? baseFont
                    : SKTypeface.FromFamilyName(baseFont.FamilyName, bold ? SKFontStyle.Bold : SKFontStyle.Normal);
            }

            SKTypeface typeface = customFont != null ? GetTypeface(customFont, isBold) : null;
            SKTypeface fallbackTypeface = GetTypeface(FallbackFont, isBold) ?? FallbackFont;

            while (textElementEnumerator.MoveNext())
            {
                string textElement = textElementEnumerator.GetTextElement();
                if (textElement.Contains("\n"))
                {
                    currentX = area.Left;
                    currentY += lineHeight;

                    continue;
                }

                var enumerator = StringInfo.GetTextElementEnumerator(textElement);
                enumerator.MoveNext();
                int codepoint = char.ConvertToUtf32(textElement, enumerator.ElementIndex);

                SKTypeface currentElementTypeFace;
                if (typeface != null && typeface.ContainsGlyph(codepoint))
                {
                    currentElementTypeFace = typeface;
                }
                else if (typeface == null || (!typeface.ContainsGlyph(codepoint) && fallbackTypeface.ContainsGlyph(codepoint)))
                {
                    currentElementTypeFace = fallbackTypeface;
                }
                else
                {
                    currentElementTypeFace = FontManager.MatchCharacter(codepoint);
                    if (currentElementTypeFace == null)
                    {
                        continue;
                    }
                }
                SKPaint paint = new()
                {
                    Typeface = currentElementTypeFace,
                    TextSize = fontSize,
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                    SubpixelText = true,
                    LcdRenderText = true,
                };
                if (shader != null)
                {
                    paint.Shader = shader;
                }
                else
                {
                    paint.Color = color;
                }

                var shaper = new SKShaper(currentElementTypeFace);

                var shapedText = shaper.Shape(textElement, paint);

                if (currentX + shapedText.Width > area.Right)
                {
                    currentX = area.Left;
                    currentY += lineHeight;
                }
                if (area.Bottom != 0 && currentY > area.Bottom)

                {
                    currentY -= lineHeight;
                    break;
                }
                MainCanvas.DrawShapedText(textElement, currentX, currentY, paint);

                currentX += shapedText.Width;
            }

            return new SKPoint(currentX, currentY);
        }

        public void DrawChart(SKPoint[] points, SKRect rect, SKColor strokeColor, float strokeWidth, SKColor gradientStart, SKColor gradientEnd)
        {
            if (points.Length < 2)
            {
                return;
            }
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = strokeColor,
                StrokeWidth = strokeWidth
            };

            var path = new SKPath();
            path.MoveTo(points[0]);

            for (int i = 0; i < points.Length - 1; i++)
            {
                SKPoint p0 = i == 0 ? points[0] : points[i - 1];
                SKPoint p1 = points[i];
                SKPoint p2 = points[i + 1];
                SKPoint p3 = (i + 2 < points.Length) ? points[i + 2] : p2;

                // 控制点算法（可根据需要调整tension参数）
                float tension = 0.3f;
                SKPoint control1 = new(
                    p1.X + ((p2.X - p0.X) * tension / 3),
                    p1.Y + ((p2.Y - p0.Y) * tension / 3));

                SKPoint control2 = new(
                    p2.X - ((p3.X - p1.X) * tension / 3),
                    p2.Y - ((p3.Y - p1.Y) * tension / 3));

                path.CubicTo(control1, control2, p2);
            }
            MainCanvas.DrawPath(path, paint);

            var last = points[points.Length - 1];
            var first = points[0];
            path.LineTo(last.X, rect.Bottom);
            path.LineTo(first.X, rect.Bottom);
            path.Close();

            var shader = SKShader.CreateLinearGradient(
                rect.Location,
                new(rect.Left, rect.Bottom),
                new SKColor[] { gradientStart, gradientEnd },
                null,
                SKShaderTileMode.Clamp);

            using var fillPaint = new SKPaint
            {
                Shader = shader,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            MainCanvas.DrawPath(path, fillPaint);
        }

        public SKPoint[] ConvertDataPointToArray(List<(DateTime time, float value)> list)
        {
            List<SKPoint> points = [];
            DateTime min = list.Min(x => x.time);
            DateTime max = list.Max(x => x.time);
            float perTickWidth = Width / (max - min).Ticks;
            foreach (var item in list)
            {
                points.Add(new SKPoint(perTickWidth * (item.time - min).Ticks, item.value));
            }

            return points.ToArray();
        }

        public SKImage LoadImage(string imagePath)
        {
            using var imageStream = File.OpenRead(imagePath);
            using var codec = SKCodec.Create(imageStream);
            var bitmap = SKBitmap.Decode(codec);
            return SKImage.FromBitmap(bitmap);
        }

        public SKSize MeasureString(string text, float fontSize, SKTypeface customFont)
        {
            SKTypeface typeface;
            if (customFont != null && customFont.ContainsGlyphs(text))
            {
                typeface = customFont;
            }
            else if (FallbackFont != null && FallbackFont.ContainsGlyphs(text))
            {
                typeface = FallbackFont;
            }
            else
            {
                typeface = FontManager.MatchCharacter(text.First());
                if (typeface == null)
                {
                    return new();
                }
            }
            var paint = new SKPaint
            {
                Typeface = typeface,
                TextSize = fontSize,
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High,
            };
            var shaper = new SKShaper(typeface);
            var shapedText = shaper.Shape(text, paint);
            var metrics = paint.FontMetrics;
            return new SKSize { Width = shapedText.Width, Height = metrics.Descent - metrics.Ascent };
        }

        public void Padding(int top, int left, int right, int bottom, SKColor color)
        {
            var newSurface = SKSurface.Create(new SKImageInfo((int)(Width + left + right), (int)(Height + top + bottom)));
            SKCanvas canvas = newSurface.Canvas;
            canvas.Clear(color);

            MainSurface.Draw(canvas, top, left, AntialiasPaint);
            MainCanvas.Dispose();
            MainSurface.Dispose();

            Width += left + right;
            Height += top + bottom;

            MainSurface = newSurface;
        }

        public void Resize(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }
            var newSurface = SKSurface.Create(new SKImageInfo(width, height));
            SKCanvas canvas = newSurface.Canvas;
            canvas.Clear(SKColors.Transparent);

            MainSurface.Draw(canvas, 0, 0, AntialiasPaint);
            MainCanvas.Dispose();
            MainSurface.Dispose();

            Width = width;
            Height = height;

            MainSurface = newSurface;
        }

        public SKImage ResizeImage(SKImage image, int newWidth, int newHeight)
        {
            var scaledBitmap = new SKBitmap(newWidth, newHeight);

            using (var canvas = new SKCanvas(scaledBitmap))
            {
                var paint = new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High
                };

                var srcRect = new SKRect(0, 0, image.Width, image.Height);
                var destRect = new SKRect(0, 0, newWidth, newHeight);

                canvas.DrawImage(image, srcRect, destRect, paint);
            }

            return SKImage.FromBitmap(scaledBitmap);
        }

        public void Save(string path)
        {
            using var image = MainSurface.Snapshot();
            using var data = image.Encode();

            File.WriteAllBytes(path, data.ToArray());
        }

        public SKImage SnapShot()
        {
            return MainSurface.Snapshot();
        }

        public void FillTileBackground(SKImage img)
        {
            var shader = SKShader.CreateBitmap(ConvertToBitmap(img), SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            using var paint = new SKPaint
            {
                Shader = shader,
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High
            };
            MainCanvas.DrawRect(new SKRect(0, 0, Width, Height), paint);
        }

        /// <summary>
        /// 创建一个圆角矩形路径
        /// </summary>
        /// <param name="rect">矩形区域</param>
        /// <param name="radius">圆角半径</param>
        /// <returns>SKPath</returns>
        public static SKPath CreateRoundedRectPath(SKRect rect, float radius)
        {
            var path = new SKPath();
            path.AddRoundRect(rect, radius, radius);
            return path;
        }

        /// <summary>
        /// 对图片的指定区域进行高斯模糊
        /// </summary>
        /// <param name="path">需要模糊的区域</param>
        /// <param name="sigma">高斯模糊参数（0-3）</param>
        public void Blur(SKPath path, float sigma)
        {
            if (sigma == 0)
            {
                return;
            }
            using var snapShot = SnapShot();
            using var paint = new SKPaint();
            paint.ImageFilter = SKImageFilter.CreateBlur(sigma, sigma);

            MainCanvas.Save();
            MainCanvas.ClipPath(path, SKClipOperation.Intersect, true);
            MainCanvas.Clear(SKColors.Transparent);
            MainCanvas.DrawImage(snapShot, 0, 0, paint);
            MainCanvas.Restore();
        }

        public void DrawSVG(string svg, SKPoint point)
        {
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(svg));
            var svgImage = new SkiaSharp.Extended.Svg.SKSvg();
            svgImage.Load(stream);
            var svgPath = svgImage.Picture;
            MainCanvas.Save();
            MainCanvas.Translate(point);
            MainCanvas.DrawPicture(svgPath);
            MainCanvas.Restore();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposing)
            {
                if (disposing)
                {
                    MainSurface.Dispose();
                }
                Disposing = true;
            }
        }
    }

    public struct Thickness
    {
        public float Left { get; set; }

        public float Top { get; set; }

        public float Right { get; set; }

        public float Bottom { get; set; }

        public Thickness(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Thickness(float all)
        {
            Left = all;
            Top = all;
            Right = all;
            Bottom = all;
        }

        public Thickness(float left, float top)
        {
            Left = left;
            Top = top;
            Right = left;
            Bottom = top;
        }

        public static Thickness Empty => new(0);

        public static Thickness DefaultPadding => new(5);

        public static Thickness DefaultMargin => new(5);
    }
}