using SkiaSharp;
using System;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class DrawingStyle
    {
        public string Name { get; set; }

        public DrawingBase[] Content { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime ModifyTime { get; set; }

        public enum BackgroundType
        {
            Color,

            Image
        }

        public enum BackgroundImageScaleType
        {
            Center,

            Stretch,

            Tile,

            Fill
        }

        /// <summary>
        /// 纯色背景色
        /// </summary>
        public string BackgrdoundColor { get; set; }

        /// <summary>
        /// 随机背景图片
        /// </summary>
        public string[] BackgroundImages { get; set; } = [];

        /// <summary>
        /// 背景绘制模式
        /// </summary>
        public BackgroundType DrawBackgroundType { get; set; } = BackgroundType.Color;

        /// <summary>
        /// 背景图片绘制模式
        /// </summary>
        public BackgroundImageScaleType DrawBackgroundImageScaleType { get; set; } = BackgroundImageScaleType.Fill;

        /// <summary>
        /// 图片默认应用的高斯模糊
        /// </summary>
        public double BackgroundBlur { get; set; } = 0;

        /// <summary>
        /// 主内容的Padding
        /// </summary>
        public Thickness Padding { get; set; }

        /// <summary>
        /// 主内容对背景的高斯模糊
        /// </summary>
        public double ContentBlur { get; set; }

        /// <summary>
        /// 主内容容器的圆角
        /// </summary>
        public double ContentRadius { get; set; }

        public Painting Draw(int width, int height)
        {
            return null;
        }
    }
}