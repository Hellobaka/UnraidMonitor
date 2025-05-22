using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public class Tester
    {
        static void Main(string[] args)
        {
            MainSave.AppDirectory = Path.GetFullPath(".");
            MainSave.ImageDirectory = CommonHelper.GetAppImageDirectory();

            AppConfig appConfig = new(Path.Combine(MainSave.AppDirectory, "Config.json"));
            appConfig.LoadConfig();
            appConfig.EnableAutoReload();

            CommandIntervalConfig intervalConfig = new(Path.Combine(MainSave.AppDirectory, "Interval.json"));
            intervalConfig.LoadConfig();
            intervalConfig.EnableAutoReload();

            DrawTest();
        }

        static void DrawTest()
        {
            DrawingStyle style = new()
            {
                BackgroundBlur = 0,
                BackgroundColor = "#FFFFFF",
                BackgroundImages = [],
                ContentBlur = 0,
                ContentRadius = 0,
                CreateTime = DateTime.Now,
                DrawBackgroundImageScaleType = DrawingStyle.BackgroundImageScaleType.Center,
                DrawBackgroundType = DrawingStyle.BackgroundType.Color,
                ItemTheme = DrawingStyle.Theme.WinUI3,
                ModifyTime = DateTime.Now,
                Name = "Test",
                Padding = new Thickness(16),
                Palette = DrawingStyle.GetThemeDefaultColor(DrawingStyle.Theme.WinUI3, false),
                Content = [
                    new DrawingBase{
                        BackgroundBlur = 0,
                        DrawingBorder = new(),
                        DrawingLayout = DrawingBase.Layout.Percentage,
                        DrawingTitle = new()
                        {
                            HasTitle = true,
                            Text = "Qualcomm Snapdragon",
                            Bold = true,
                            TextSize = 36,
                            HasIcon = false,
                            IconMargin = new Thickness(10, 0),
                            IconPath = @"icons\icon.ico",
                            IconSize = new(40, 40),
                            //OverrideColor = "#E22829",
                            //OverrideColor2 = "#FF8C30",
                        },
                        FillPercentage = 100,
                        FixedWidth = 0,
                        Margin = Thickness.DefaultMargin,
                        Padding = Thickness.DefaultPadding,
                        Radius = 0,
                        LayoutDebug = false,
                        Content = [
                            new DrawingItem_Text(){
                                Text = "CPU Slot1:",
                            },
                            new DrawingItem_Text(){
                                Text = "Qualcomm Snapdragon 999 Extra Elite",
                                Layout = DrawingBase.Layout.Remaining,
                                IsBold = true,
                                AfterNewLine = true,
                            },
                            new DrawingItem_Text(){
                                Text = "CPU Slot2:",
                            },
                            new DrawingItem_Text(){
                                Text = "AMD Razer 9 9950X3D",
                                Layout = DrawingBase.Layout.Remaining,
                                IsBold = true,
                                AfterNewLine = true,
                            },
                            new DrawingItem_Text(){
                                Text = "CPU0:",
                                Layout = DrawingBase.Layout.Percentage,
                                FillPercentage = 10,
                                VerticalAlignment = DrawingBase.Position.Center
                            },
                            new DrawingItem_ProgressBar(){
                                Value = 20,
                                Min = 0,
                                Max = 100,
                                Layout = DrawingBase.Layout.Remaining,
                                VerticalAlignment = DrawingBase.Position.Center,
                                FillPercentage = 15,
                            },
                            new DrawingItem_Text(){
                                Text = "CPU1:",
                                Layout = DrawingBase.Layout.Percentage,
                                FillPercentage = 10,
                                VerticalAlignment = DrawingBase.Position.Center
                            },
                            new DrawingItem_ProgressBar(){
                                Value = 55,
                                Min = 0,
                                Max = 100,
                                Layout = DrawingBase.Layout.Remaining,
                                VerticalAlignment = DrawingBase.Position.Center,
                                FillPercentage = 15,
                            },
                            new DrawingItem_Text(){
                                Text = "CPU2:",
                                Layout = DrawingBase.Layout.Percentage,
                                FillPercentage = 10,
                                VerticalAlignment = DrawingBase.Position.Center
                            },
                            new DrawingItem_ProgressBar(){
                                Value = 41,
                                Min = 0,
                                Max = 100,
                                Layout = DrawingBase.Layout.Remaining,
                                VerticalAlignment = DrawingBase.Position.Center,
                                FillPercentage = 15,
                            },
                            new DrawingItem_Text(){
                                Text = "CPU3:",
                                Layout = DrawingBase.Layout.Percentage,
                                FillPercentage = 10,
                                VerticalAlignment = DrawingBase.Position.Center
                            },
                            new DrawingItem_ProgressBar(){
                                Value = 74,
                                Min = 0,
                                Max = 100,
                                Layout = DrawingBase.Layout.Remaining,
                                VerticalAlignment = DrawingBase.Position.Center,
                                FillPercentage = 15,
                                AfterNewLine = true,
                            },
                            new DrawingItem_Text(){
                                Text = "CPU4:",
                                Layout = DrawingBase.Layout.Percentage,
                                FillPercentage = 10,
                                VerticalAlignment = DrawingBase.Position.Center
                            },
                            new DrawingItem_ProgressBar(){
                                Value = 63,
                                Min = 0,
                                Max = 100,
                                Layout = DrawingBase.Layout.Remaining,
                                VerticalAlignment = DrawingBase.Position.Center,
                                FillPercentage = 40,
                            },
                            new DrawingItem_Text(){
                                Text = "CPU5:",
                                Layout = DrawingBase.Layout.Percentage,
                                FillPercentage = 10,
                                VerticalAlignment = DrawingBase.Position.Center
                            },
                            new DrawingItem_ProgressBar(){
                                Value = 5,
                                Min = 0,
                                Max = 100,
                                Layout = DrawingBase.Layout.Remaining,
                                FillPercentage = 40,
                                VerticalAlignment = DrawingBase.Position.Center,
                                AfterNewLine = true,
                            },
                            new DrawingItem_Text(){
                                Text = "CPU6:",
                                Layout = DrawingBase.Layout.Percentage,
                                FillPercentage = 10,
                                VerticalAlignment = DrawingBase.Position.Center
                            },
                            new DrawingItem_ProgressBar(){
                                Value = 88,
                                Min = 0,
                                Max = 100,
                                Layout = DrawingBase.Layout.Remaining,
                                FillPercentage = 90,
                                VerticalAlignment = DrawingBase.Position.Center,
                                AfterNewLine = true,
                            },
                            new DrawingItem_Chart(){
                                Points = MockChartValue(100),
                                AfterNewLine = true,
                                OverrideHeight = 200,
                                VerticalValueDisplayCount = 5,
                                ShowVerticalGridLine = true,
                                HorizonValueDisplayCount = 10
                            },
                            new DrawingItem_Chart(){
                                Points = MockChartValue(50),
                                AfterNewLine = true,
                                VerticalValueDisplayCount = 2,
                            },
                            new DrawingItem_Alert(){
                                AlertType = DrawingBase.AlertType.Info,
                                Header = "Info",
                                IsHeaderBold = true,
                                Content = "Alert Short Text Test",
                                AfterNewLine = true,
                            },
                            new DrawingItem_Alert(){
                                AlertType = DrawingBase.AlertType.Warning,
                                Header = "Warning",
                                IsHeaderBold = true,
                                Content = "Alert Short Text Test",
                                AfterNewLine = true,
                            },
                            new DrawingItem_Alert(){
                                AlertType = DrawingBase.AlertType.Success,
                                Header = "Success",
                                IsHeaderBold = true,
                                Content = "Alert Short Text Test",
                                AfterNewLine = true,
                            },
                            new DrawingItem_Alert(){
                                AlertType = DrawingBase.AlertType.Fatal,
                                Header = "Fatal",
                                IsHeaderBold = true,
                                Content = "Alert Short Text Test",
                                AfterNewLine = true,
                            },
                            new DrawingItem_Alert(){
                                AlertType = DrawingBase.AlertType.Info,
                                Header = "Info",
                                IsHeaderBold = false,
                                Content = @"正在尝试收集与目标为“.NETFramework,Version=v4.8”的项目“me.cqp.luohuaming.UnraidMonitor.PublicInfos”有关的包“HarfBuzzSharp.7.3.0.3”的依赖项信息
正在解析操作以卸载程序包“HarfBuzzSharp.7.3.0.3”
无法卸载“HarfBuzzSharp.7.3.0.3”，因为“SkiaSharp.HarfBuzz.2.88.9”依赖于它。
已用时间: 00:00:00.5298694",
                                AfterNewLine = true,
                            },
                        ]
                    }
                ],
            };
            var paint = style.Draw(1000);
            paint.Save("1.bmp");
            File.WriteAllText("default.style", JsonConvert.SerializeObject(style, Formatting.Indented));
        }

        static Random Random = new Random();

        static (DateTime, double)[] MockChartValue(int count)
        {
            var list = new List<(DateTime, double)>();
            for (int i = 0; i < count; i++)
            {
                list.Add((DateTime.Now.AddSeconds(-1 * (count - i)), Random.Next(0, 100)));
            }
            return list.ToArray();
        }

        static void MonitorTest()
        {
            var handler = new Linux();
            handler.StartMonitor();
            Console.ReadLine();
            // -1. 系统启动时间
            var uptime = handler.GetSystemUptime();
            Console.WriteLine("[Uptime]");
            PrintProperties(uptime);
            handler.InsertData(uptime);

            // 0. 系统信息
            var systemInfo = handler.GetSystemInfo();
            Console.WriteLine("[SystemInfo]");
            PrintProperties(systemInfo);
            handler.InsertData(systemInfo);

            // 1. CPU信息
            var cpuInfo = handler.GetCpuInfo();
            Console.WriteLine("[CPUInfo]");
            PrintProperties(cpuInfo);
            handler.InsertData(cpuInfo);

            // 2.1. UPS信息
            var ups = handler.GetUPS();
            Console.WriteLine("[UPS]");
            PrintProperties(ups);
            handler.InsertData(ups);

            // 2. CPU使用率
            var cpuUsages = handler.GetCpuUsages();
            Console.WriteLine("[CPUUsage]");
            foreach (var item in cpuUsages ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.CpuUsage>())
                PrintProperties(item);
            handler.InsertData(cpuUsages);

            // 3. 内存信息
            var memoryInfo = handler.GetMemoryInfo();
            Console.WriteLine("[MemoryInfo]");
            PrintProperties(memoryInfo);

            // 4. 主板信息
            var motherboardInfo = handler.GetMotherboardInfo();
            Console.WriteLine("[MotherboardInfo]");
            PrintProperties(motherboardInfo);

            // 5. 磁盘挂载信息
            var diskMounts = handler.GetDiskMountInfos();
            Console.WriteLine("[DiskMountInfo]");
            foreach (var item in diskMounts ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.DiskMountInfo>())
                PrintProperties(item);

            // 5.1. 磁盘信息
            var disks = handler.GetDiskInfos();
            Console.WriteLine("[DiskInfo]");
            foreach (var item in disks ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.DiskInfo>())
                PrintProperties(item);

            // 6. Docker容器
            var dockers = handler.GetDockers();
            Console.WriteLine("[Dockers]");
            foreach (var item in dockers ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.Dockers>())
                PrintProperties(item);

            // 7. 虚拟机
            var vms = handler.GetVirtualMachines();
            Console.WriteLine("[VirtualMachine]");
            foreach (var item in vms ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.VirtualMachine>())
                PrintProperties(item);

            // 8. 风扇信息
            var fans = handler.GetFanInfos();
            Console.WriteLine("[FanInfo]");
            foreach (var item in fans ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.FanInfo>())
                PrintProperties(item);

            // 9. 网卡信息
            var nics = handler.GetNetworkInterfaceInfos();
            Console.WriteLine("[NetworkInterfaceInfo]");
            foreach (var item in nics ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.NetworkInterfaceInfo>())
                PrintProperties(item);

            // 10. 网络流量
            var traffics = handler.GetNetworkTrafficInfos();
            Console.WriteLine("[NetworkTrafficInfo]");
            foreach (var item in traffics ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.NetworkTrafficInfo>())
                PrintProperties(item);

            // 11. 温度信息
            var temps = handler.GetTemperatureInfos();
            Console.WriteLine("[TemperatureInfo]");
            foreach (var item in temps ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.TemperatureInfo>())
                PrintProperties(item);

            // 12. 磁盘SMART信息
            var diskSmart = handler.GetDiskSmartInfos();
            Console.WriteLine("[DiskSmartInfo]");
            foreach (var item in diskSmart ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.DiskSmartInfo>())
            {
                PrintProperties(item);
                PrintProperties(item.Smart);
            }
        }

        static void PrintProperties(object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("  [null]");
                return;
            }
            var type = obj.GetType();
            foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                var value = prop.GetValue(obj);
                if (value is System.Collections.IEnumerable enumerable && !(value is string))
                {
                    Console.WriteLine($"  {prop.Name}:");
                    foreach (var sub in enumerable)
                    {
                        Console.WriteLine($"    - {sub}");
                    }
                }
                else
                {
                    Console.WriteLine($"  {prop.Name}: {value}");
                }
            }
            Console.WriteLine();
        }
    }
}
