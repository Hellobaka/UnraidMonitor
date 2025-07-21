using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items
{
    public class DrawingItem_Image : DrawingItemBase
    {
        public override DrawingCanvas.Layout Layout { get; set; } = DrawingCanvas.Layout.Minimal;

        public override ItemType Type { get; set; } = ItemType.Image;

        [Bindable(BindableSupport.Yes)]
        [Description("图片相对路径")]
        public string Source { get; set; }

        public override void BeforeBinding()
        {
            Source = "";
        }

        public override float CalcHeight(DrawingStyle.Theme theme)
        {
            if (OverrideHeight == 0)
            {
                OverrideHeight = 100;
            }
            return base.CalcHeight(theme);
        }

        public override void ApplyBinding()
        {
            base.ApplyBinding();
            if (Binding == null)
            {
                return;
            }
            if (Binding.Value.TryGetValue("Source", out var data))
            {
                Source = data.FormattedString;
            }
        }

        public override (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            string pathA = Path.Combine(MainSave.ImageDirectory, Source);
            string pathB = Path.Combine(MainSave.AppDirectory, Source);
            string pathC = Source;
            string path = "";
            foreach (var item in new string[] { pathA, pathB, pathC })
            {
                if (File.Exists(item))
                {
                    path = item;
                    break;
                }
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Image source '{Source}' not found in any of the expected directories.");
            }

            painting.DrawImage(path, new SKRect
            {
                Location = new SKPoint(startPoint.X, startPoint.Y),
                Size = new SKSize(desireWidth, OverrideHeight)
            });

            return (
                endPoint: new SKPoint(startPoint.X + desireWidth, startPoint.Y + OverrideHeight),
                width: desireWidth,
                height: OverrideHeight
            );
        }

        public static DrawingItem_Image Create(string source) => new()
        {
            Source = source
        };
    }
}
