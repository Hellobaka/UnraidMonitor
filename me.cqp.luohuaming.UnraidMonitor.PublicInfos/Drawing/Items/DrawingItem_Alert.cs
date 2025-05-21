using SkiaSharp;
using System;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Alert : DrawingItemBase
    {
        public override ItemType Type { get; set; } = ItemType.Alert;

        public override DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Remaining;

        public string Header { get; set; }

        public bool IsHeaderBold { get; set; }

        public string Content { get; set; }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            CalcHeight(theme);
            return theme switch
            {
                DrawingStyle.Theme.Unraid => DrawUnraid(painting, startPoint, desireWidth, theme, palette),
                DrawingStyle.Theme.MaterialDesign3 => DrawMaterialDesign3(painting, startPoint, desireWidth, theme, palette),
                DrawingStyle.Theme.MaterialDesign2 => DrawMaterialDesign2(painting, startPoint, desireWidth, theme, palette),
                _ => DrawWinUI3(painting, startPoint, desireWidth, theme, palette),
            };
        }

        private (SKPoint endPoint, float width, float height) DrawWinUI3(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            throw new NotImplementedException();
        }

        private (SKPoint endPoint, float width, float height) DrawMaterialDesign2(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            throw new NotImplementedException();
        }

        private (SKPoint endPoint, float width, float height) DrawMaterialDesign3(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            throw new NotImplementedException();
        }

        private (SKPoint endPoint, float width, float height) DrawUnraid(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            throw new NotImplementedException();
        }
    }
}
