using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public static class DBHelper
    {
        public static LiteDatabase GetInstance() => new(Path.Combine(MainSave.AppDirectory, "history.db"));
    }
}
