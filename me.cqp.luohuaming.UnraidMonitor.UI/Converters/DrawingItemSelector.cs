using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items;
using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Converters
{
    public class DrawingItemSelector : DataTemplateSelector
    {
        public DataTemplate Alert { get; set; }
       
        public DataTemplate Chart { get; set; }
       
        public DataTemplate Image { get; set; }
       
        public DataTemplate ProgressBar { get; set; }
       
        public DataTemplate ProgressRing { get; set; }
       
        public DataTemplate RunningStatus { get; set; }
       
        public DataTemplate Text { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item is DrawingItem_Alert)
            {
                return Alert;
            }
            else if(item is DrawingItem_Chart)
            {
                return Chart;
            }
            else if (item is DrawingItem_Image)
            {
                return Image;
            }
            else if (item is DrawingItem_ProgressBar)
            {
                return ProgressBar;
            }
            else if (item is DrawingItem_ProgressRing)
            {
                return ProgressRing;
            }
            else if (item is DrawingItem_RunningStatus)
            {
                return RunningStatus;
            }
            else if (item is DrawingItem_Text)
            {
                return Text;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
