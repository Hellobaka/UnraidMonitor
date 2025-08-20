using me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing.Items;
using Newtonsoft.Json;
using PropertyChanged;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Reflection;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class DrawingItemBase : INotifyPropertyChanged
    {
        public enum ItemType
        {
            [Description("文本块")]
            Text,
           
            [Description("进度条")]
            ProgressBar,
           
            [Description("进度环")]
            ProgressRing,
           
            [Description("图表")]
            Chart,
            
            [Description("信息块")]
            Alert,
           
            [Description("图片")]
            Image,
            
            [Description("双行状态块")]
            RunningStatus
        }

        /// <summary>
        /// 子内容的类型
        /// </summary>
        public virtual ItemType Type { get; set; } = ItemType.Text;

        /// <summary>
        /// 子内容的布局
        /// </summary>
        public virtual DrawingCanvas.Layout Layout { get; set; } = DrawingCanvas.Layout.Minimal;

        /// <summary>
        /// 填充模式的百分比占比
        /// </summary>
        public virtual float FillPercentage { get; set; } = 100;

        /// <summary>
        /// 固定宽度的宽度数值
        /// </summary>
        public virtual float FixedWidth { get; set; } = 0;

        public virtual Thickness Margin { get; set; } = Thickness.DefaultMargin;

        public virtual Thickness Padding { get; set; } = Thickness.Empty;

        public virtual bool AfterNewLine { get; set; }

        /// <summary>
        /// 强制高度
        /// </summary>
        public virtual float OverrideHeight { get; set; }

        public virtual DrawingStyle.Theme OverrideTheme { get; set; }

        public virtual DrawingStyle.Colors OverridePalette { get; set; }

        public virtual DrawingCanvas.Position VerticalAlignment { get; set; } = DrawingCanvas.Position.Top;

        public Binding Binding { get; set; }

        [JsonIgnore]
        public SKRect Boundary { get; set; }

        [JsonIgnore]
        [DoNotNotify]
        public bool IsExpanded { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            MainSave.RaisePropertyChanged(GetType().GetProperty(propertyName), this, after, before);
        }

        public virtual void BeforeBinding()
        {
        }

        public virtual void ApplyBinding()
        {
            Binding?.Get();
        }

        public virtual float CalcHeight(DrawingStyle.Theme theme)
        {
            return OverrideHeight;
        }

        public virtual (SKPoint endPoint, float width, float height) Draw(Painting painting, SKPoint startPoint, float desireWidth, DrawingStyle.Theme theme, DrawingStyle.Colors palette)
        {
            // 计算实际高度
            // 计算结束点
            // 返回结束点和实际高度
            return (SKPoint.Empty, 0, 0);
        }

        public DrawingItemBase Clone()
        {
            string item = JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            });
            return Type switch
            {
                ItemType.Text => JsonConvert.DeserializeObject<DrawingItem_Text>(item),
                ItemType.ProgressBar => JsonConvert.DeserializeObject<DrawingItem_ProgressBar>(item),
                ItemType.ProgressRing => JsonConvert.DeserializeObject<DrawingItem_ProgressRing>(item),
                ItemType.Chart => JsonConvert.DeserializeObject<DrawingItem_Chart>(item),
                ItemType.Alert => JsonConvert.DeserializeObject<DrawingItem_Alert>(item),
                ItemType.Image => JsonConvert.DeserializeObject<DrawingItem_Image>(item),
                ItemType.RunningStatus => JsonConvert.DeserializeObject<DrawingItem_RunningStatus>(item),
                _ => throw new NotImplementedException($"不支持的绘图项类型: {Type}"),
            };
        }
    }
}
