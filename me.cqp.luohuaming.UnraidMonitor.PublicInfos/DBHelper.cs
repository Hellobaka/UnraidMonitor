using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models;
using SqlSugar;
using System.IO;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public static class DBHelper
    {
        public static string DBPath => Path.Combine(MainSave.AppDirectory, "history.db");

        public static SqlSugarClient Instance { get; set; } = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = $"data source={DBPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = false,
            InitKeyType = InitKeyType.Attribute,
        });

        public static void Init()
        {
            Instance.DbMaintenance.CreateDatabase(DBPath);

            Instance.CodeFirst.InitTables(typeof(CpuInfo));
            Instance.CodeFirst.InitTables(typeof(CpuUsage));
            Instance.CodeFirst.InitTables(typeof(DiskInfo));
            Instance.CodeFirst.InitTables(typeof(DiskMountInfo));
            Instance.CodeFirst.InitTables(typeof(DiskSmartInfo));
            Instance.CodeFirst.InitTables(typeof(Dockers));
            Instance.CodeFirst.InitTables(typeof(FanInfo));
            Instance.CodeFirst.InitTables(typeof(MemoryInfo));
            Instance.CodeFirst.InitTables(typeof(MotherboardInfo));
            Instance.CodeFirst.InitTables(typeof(NetworkInterfaceInfo));
            Instance.CodeFirst.InitTables(typeof(NetworkTrafficInfo));
            Instance.CodeFirst.InitTables(typeof(Notification));
            Instance.CodeFirst.InitTables(typeof(SystemInfo));
            Instance.CodeFirst.InitTables(typeof(SystemUptime));
            Instance.CodeFirst.InitTables(typeof(TemperatureInfo));
            Instance.CodeFirst.InitTables(typeof(UPSStatus));
            Instance.CodeFirst.InitTables(typeof(VirtualMachine));
        }
    }
}
