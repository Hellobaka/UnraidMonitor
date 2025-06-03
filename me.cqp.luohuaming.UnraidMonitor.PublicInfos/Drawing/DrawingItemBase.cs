using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Chart,
            Alert,
            Image
        }

        /// <summary>
        /// 子内容的类型
        /// </summary>
        public virtual ItemType Type { get; set; } = ItemType.Text;

        /// <summary>
        /// 子内容的布局
        /// </summary>
        public virtual DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Minimal;

        /// <summary>
        /// 填充模式的百分比占比
        /// </summary>
        public virtual float FillPercentage { get; set; } = 100;

        /// <summary>
        /// 固定宽度的宽度数值
        /// </summary>
        public virtual float FixedWidth { get; set; } = 0;

        public virtual Thickness Margin { get; set; } = Thickness.DefaultMargin;

        public virtual bool AfterNewLine { get; set; }

        /// <summary>
        /// 强制高度
        /// </summary>
        public virtual float OverrideHeight { get; set; }

        public virtual DrawingStyle.Theme OverrideTheme { get; set; }

        public virtual DrawingStyle.Colors OverridePalette { get; set; }

        public virtual DrawingBase.Position VerticalAlignment { get; set; } = DrawingBase.Position.Top;

        public Binding Binding { get; set; }

        public virtual void ApplyBinding()
        {
            Binding?.Get();
        }

        public virtual float CalcHeight(DrawingStyle.Theme theme)
        {
            return OverrideHeight;
        }

        public virtual (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            // 计算实际高度
            // 计算结束点
            // 返回结束点和实际高度
            return (SKPoint.Empty, 0, 0);
        }
    }
}
