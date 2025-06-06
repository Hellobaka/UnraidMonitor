using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Handler;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

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

            DBHelper.Init();
            var handler = new Windows();
            //MonitorTest();
            LoadStyleTest();
            //DrawTest();
        }

        static void LoadStyleTest()
        {
            var paint = DrawingStyle.LoadFromFile("default.style").Draw(1500);
            paint.Save("2.bmp");
        }

        static void DrawTest()
        {
            DrawingStyle style = new()
            {
                BackgroundBlur = 0,
                BackgroundColor = "#0E0E0E",
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
                Palette = DrawingStyle.GetThemeDefaultColor(DrawingStyle.Theme.WinUI3, true),
                Content = [
                    new DrawingBase{
                    DrawingLayout = DrawingBase.Layout.Percentage,
                    FillPercentage = 50,
                    DrawingTitle = new(){
                        HasTitle = true,
                        Text = "主板",
                        Bold = true,
                        TextSize = 46,
                    },
                    Content = [
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.MotherboardInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "制造商: {0}",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "Manufacturer"
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.MotherboardInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "型号: {0}",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "ProductName"
                                        },
                                    ] }
                                }
                            },
                        },
                    ]
                },
                new DrawingBase{
                    DrawingLayout = DrawingBase.Layout.Percentage,
                    FillPercentage = 50,
                    DrawingTitle = new(){
                        HasTitle = true,
                        Text = "系统",
                        Bold = true,
                        TextSize = 46,
                    },
                    Content = [
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.SystemInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "版本: {0} {1} {2}",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "SystemName"
                                        },
                                        new MultipleBinding{
                                            Path = "SystemEdition"
                                        },
                                        new MultipleBinding{
                                            Path = "Version"
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.SystemUptime,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "启动时间: {0}天 {1}小时 {2}分钟 {3}秒",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "UpTimeDay"
                                        },
                                        new MultipleBinding{
                                            Path = "UpTimeHour"
                                        },
                                        new MultipleBinding{
                                            Path = "UpTimeMinute"
                                        },
                                        new MultipleBinding{
                                            Path = "UpTimeSecond"
                                        },
                                    ] }
                                }
                            },
                        },
                    ]
                },
                new DrawingBase{
                    BackgroundBlur = 0,
                    DrawingBorder = new()
                    {
                        BorderColor = SKColors.White,
                        BorderRadius = 10,
                        BorderWidth = 1,
                        HasBorder = false,
                    },
                    DrawingLayout = DrawingBase.Layout.Percentage,
                    DrawingTitle = new()
                    {
                        HasTitle = true,
                        Text = "CPU Info",
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
                    Radius = 10,
                    LayoutDebug = false,
                    Content = [
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.CpuInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "型号: {0} @ {1:f2} GHz",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "Model"
                                        },
                                        new MultipleBinding{
                                            Path = "BaseSpeedGHz"
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.CpuInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "规格: {0} 核心 {1} 线程",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "PhysicalCores"
                                        },
                                        new MultipleBinding{
                                            Path = "LogicalCores"
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Text {
                            Text = "总占用率:",
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Remaining,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Total" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU1:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #1" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU2:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #2" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU3:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #3" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU4:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #4" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU5:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #5" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU6:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #6" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU7:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #7" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU8:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #8" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU9:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #9 Thread #1" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU10:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #9 Thread #2" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU11:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #10 Thread #1" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU12:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #10 Thread #2" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU13:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #11 Thread #1" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU14:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #11 Thread #2" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU15:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #12 Thread #1" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU16:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #12 Thread #2" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },

                        new DrawingItem_Text {
                            Text = "CPU17:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #13" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Text {
                            Text = "CPU18:",
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 8,
                        },
                        new DrawingItem_ProgressBar {
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Layout = DrawingBase.Layout.Percentage,
                            FillPercentage = 42,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Core #14" }
                                },
                                ItemType = ItemType.CpuUsage,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] }
                                }
                            }
                        },
                        new DrawingItem_Chart{
                            AfterNewLine = true,
                            OverrideHeight = 200,
                            Binding = new(){
                                Conditions = new(){
                                    {"CPUId", "CPU Total" }
                                },
                                ItemType = ItemType.CpuUsage,
                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"DateTime", [
                                        new MultipleBinding{
                                            Path = "DateTime"
                                        },
                                    ] },
                                    {"Points", [
                                        new MultipleBinding{
                                            Path = "TotalUsage"
                                        },
                                    ] },
                                }
                            }
                        }
                    ]
                },
                new DrawingBase{
                    DrawingTitle = new(){
                        HasTitle = true,
                        Text = "内存",
                        Bold = true,
                        TextSize = 46,
                    },
                    Content = [
                        new DrawingItem_Text{
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.MemoryInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "占用率 ({0:f2}%)：{1:f1}GB / {2:f1}GB",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "UsagePercentage"
                                        },
                                        new MultipleBinding{
                                            Path = "Used",
                                            NumberConverter = NumberConverter.BytesToMB
                                        },
                                        new MultipleBinding{
                                            Path = "Total",
                                            NumberConverter = NumberConverter.BytesToMB
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_ProgressBar{
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.MemoryInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "UsagePercentage"
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Chart{
                            AfterNewLine = true,
                            OverrideHeight = 200,
                            Binding = new(){
                                Conditions = [],
                                ItemType = ItemType.MemoryInfo,
                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"DateTime", [
                                        new MultipleBinding{
                                            Path = "DateTime"
                                        },
                                    ] },
                                    {"Points", [
                                        new MultipleBinding{
                                            Path = "UsagePercentage"
                                        },
                                    ] },
                                }
                            }
                        }

                    ]
                },
                new DrawingBase{
                    DrawingTitle = new(){
                        HasTitle = true,
                        Text = "硬盘",
                        Bold = true,
                        TextSize = 46,
                    },
                    Content = [
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.DiskInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "磁盘0：{0} 挂载于 {1}",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "Model"
                                        },
                                        new MultipleBinding{
                                            Path = "DeviceName"
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.DiskInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "运行状态：{0}",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "Running"
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.DiskInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "温度：{0:f1}°C",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "Temperature"
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Text = "SMART：Good"
                        },
                        new DrawingItem_Text{
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.DiskInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "占用率：{0:f2}GB / {1:f2}GB",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "Used",
                                            NumberConverter = NumberConverter.BytesToGB
                                        },
                                        new MultipleBinding{
                                            Path = "Total",
                                            NumberConverter = NumberConverter.BytesToGB
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_ProgressBar{
                            AfterNewLine = true,
                            VerticalAlignment = DrawingBase.Position.Center,
                            Binding = new Binding(){
                                Conditions = [],
                                ItemType = ItemType.DiskInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"Value", [
                                        new MultipleBinding{
                                            Path = "UsedPercent"
                                        },
                                    ] }
                                }
                            },
                        },
                    ]
                },
                    new DrawingBase{
                    DrawingTitle = new(){
                        HasTitle = true,
                        Text = "网络",
                        Bold = true,
                        TextSize = 46,
                    },
                    Content = [
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = new(){
                                    {"Name", "WLAN" }
                                },
                                ItemType = ItemType.NetworkTrafficInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "{0}：↑{1:f1}Mbps ↓{2:f1}Mbps",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "Name"
                                        },
                                        new MultipleBinding{
                                            Path = "TxBytes",
                                            ValueType = Drawing.ValueType.Diff,
                                            NumberConverter = NumberConverter.BytesToMB
                                        },
                                        new MultipleBinding{
                                            Path = "RxBytes",
                                            ValueType = Drawing.ValueType.Diff,
                                            NumberConverter = NumberConverter.BytesToMB
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Text{
                            AfterNewLine = true,
                            Binding = new Binding(){
                                Conditions = new(){
                                    {"Name", "以太网 2" }
                                },
                                ItemType = ItemType.NetworkTrafficInfo,

                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                StringFormat = "{0}：↑{1:f1}Mbps ↓{2:f1}Mbps",
                                BindingPath = new(){
                                    {"Text", [
                                        new MultipleBinding{
                                            Path = "Name"
                                        },
                                        new MultipleBinding{
                                            Path = "TxBytes",
                                            ValueType = Drawing.ValueType.Diff,
                                            NumberConverter = NumberConverter.BytesToMB
                                        },
                                        new MultipleBinding{
                                            Path = "RxBytes",
                                            ValueType = Drawing.ValueType.Diff,
                                            NumberConverter = NumberConverter.BytesToMB
                                        },
                                    ] }
                                }
                            },
                        },
                        new DrawingItem_Chart{
                            AfterNewLine = true,
                            OverrideHeight = 200,
                            Binding = new(){
                                Conditions = new(){
                                    {"Name", "WLAN" }
                                },
                                ItemType = ItemType.NetworkTrafficInfo,
                                FromTimeRange = TimeRange.Day,
                                FromTimeValue = 1,
                                ToTimeRange = TimeRange.Day,
                                ToTimeValue = 0,
                                BindingPath = new(){
                                    {"DateTime", [
                                        new MultipleBinding{
                                            Path = "DateTime",
                                            ValueType = Drawing.ValueType.Diff,
                                        },
                                    ] },
                                    {"Points", [
                                        new MultipleBinding{
                                            Path = "TxBytes",
                                            ValueType = Drawing.ValueType.Diff,
                                            NumberConverter = NumberConverter.BytesToMB
                                        },
                                    ] },
                                    {"Max", [
                                        new MultipleBinding{
                                            Path = "TxBytes",
                                            ValueType = Drawing.ValueType.DiffMax,
                                            NumberConverter = NumberConverter.BytesToMB
                                        },
                                    ] },
                                    {"Min", [
                                        new MultipleBinding{
                                            Path = "TxBytes",
                                            ValueType = Drawing.ValueType.DiffMin,
                                            NumberConverter = NumberConverter.BytesToMB
                                        },
                                    ] },
                                }
                            }
                        }
                    ]
                },
            ],
            };
            var paint = style.Draw(1500);
            paint.Save("1.bmp");
            File.WriteAllText("bindingtest.style", style.Serialize());
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
            var handler = HandlerBase.Instance;
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
            foreach (var item in diskSmart ?? Array.Empty<me.cqp.luohuaming.UnraidMonitor.PublicInfos.Models.DiskSmartInfo>())
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
