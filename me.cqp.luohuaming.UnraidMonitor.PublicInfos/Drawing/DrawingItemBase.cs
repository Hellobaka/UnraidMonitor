using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class DrawingItemBase
    {
        public enum ItemType
        {
            Text,
            ProgressBar,
            ProgressRing,
            Chart
        }

        public virtual ItemType Type { get; set; } = ItemType.Text;

        public virtual DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Minimal;

        public virtual (SKPoint endPoint, double width, double height) Draw(Painting painting, SKPoint startPoint, double desireWidth, double desireHeight)
        {
            // 计算实际高度
            // 计算结束点
            // 返回结束点和实际高度
            return (SKPoint.Empty, 0, 0);
        }
    }
}
