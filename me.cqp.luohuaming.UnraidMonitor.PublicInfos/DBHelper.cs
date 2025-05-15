using LiteDB;
using System.IO;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public static class DBHelper
    {
        public static LiteDatabase Instance { get; set; } = new(Path.Combine(MainSave.AppDirectory, "history.db"));
    }
}
