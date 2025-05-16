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

        public enum Layout
        {
            Fill,
            Left,
            Minimal
        }

        public virtual double FillPercentage { get; set; } = 100;

        public virtual Title DrawingTitle { get; set; }

        public virtual Layout DrawingLayout { get; set; } = Layout.Fill;

        public virtual DrawingContainer[] Containers { get; set; } = [];

        public virtual double Radius { get; set; }

        public virtual double BackgroundBlur { get; set; }

        public virtual Thickness Marging { get; set; }

        public virtual (SKPoint endPoint, double width, double height) Draw(Painting painting, SKPoint startPoint, double width, double height)
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
