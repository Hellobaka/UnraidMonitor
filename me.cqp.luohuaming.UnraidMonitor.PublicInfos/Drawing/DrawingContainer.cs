using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class DrawingContainer
    {
        public DrawingItemBase[] Content { get; set; }

        /// <summary>
        /// 占用的列数
        /// </summary>
        public int DrawColumns { get; set; } = 1;

        public (SKPoint endPoint, double height) Draw(Painting painting, SKPoint startPoint, double desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            // 调用各Item的绘制方法
            // 计算每个Item的高度
            // 根据按列的数量进行布局
            // 内容之间添加默认间距
            return (SKPoint.Empty, 0);
        }
    }
}
