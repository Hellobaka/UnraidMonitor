using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Converters
{
    public class SKImageToImageSource
    {
        public static ImageSource Convert(Painting image)
        {
            using var stream = new MemoryStream(image.Encode().ToArray());
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            return bitmap;
        }
    }
}
