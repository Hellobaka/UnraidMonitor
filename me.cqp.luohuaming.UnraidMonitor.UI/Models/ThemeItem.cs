using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Models
{
    public class ThemeItem
    {
        public string Name { get; set; }

        public string Preview { get; set; }

        public DrawingStyle.Theme Theme { get; set; }

        public bool DarkMode { get; set; }

        public override string ToString() => Name;
    }
}
