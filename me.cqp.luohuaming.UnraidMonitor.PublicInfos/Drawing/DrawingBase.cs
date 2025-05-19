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
            public bool HasIcon { get; set; }

            public string IconPath { get; set; }

            public SKSize IconSize { get; set; }

            public Thickness IconMarging { get; set; }

            public string Text { get; set; }

            public bool Bold { get; set; }

            public int TextSize { get; set; }
        }

        public class Border
        {
            public bool HasBorder { get; set; } = false;
         
            public SKColor BorderColor { get; set; } = SKColors.Transparent;
         
            public float BorderWidth { get; set; } = 0;
         
            public float BorderRadius { get; set; } = 0;
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
        public virtual Title DrawingTitle { get; set; }

        public virtual Border DrawingBorder { get; set; }

        /// <summary>
        /// 布局选项
        /// </summary>
        public virtual Layout DrawingLayout { get; set; } = Layout.Fill;

        public virtual DrawingContainer[] Containers { get; set; } = [];

        /// <summary>
        /// 若绘制边框, 则使用的圆角
        /// </summary>
        public virtual float Radius { get; set; }

        /// <summary>
        /// 对背景的高斯模糊
        /// </summary>
        public virtual float BackgroundBlur { get; set; }

        /// <summary>
        /// 强制高度
        /// </summary>
        public virtual float ContentHeight { get; set; }

        /// <summary>
        /// Margin
        /// </summary>
        public virtual Thickness Margin { get; set; }

        public virtual (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float width)
        {
            if (painting == null)
            {
                throw new InvalidOperationException("未提供父画布，无法绘制");
            }
            // 此处根据绘制填充选项，计算绘制的宽度
            // 根据容器列表，计算绘制的高度
            // 若高度大于期望宽度，则使用实际高度，并将此高度返回重新对本行的元素重新按照此高度绘制
            // 调用各个容器的绘制方法
            // 返回绘制的结束点与实际高度
            return (SKPoint.Empty, 0, 0);
        }
    }
}
