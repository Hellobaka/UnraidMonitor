using Newtonsoft.Json;
using SkiaSharp;
using System.ComponentModel;
using System.Reflection;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos.Drawing
{
    public class DrawingItemBase : INotifyPropertyChanged
    {
        public enum ItemType
        {
            Text,
            ProgressBar,
            ProgressRing,
            Chart,
            Alert,
            Image,
            RunningStatus
        }

        /// <summary>
        /// 子内容的类型
        /// </summary>
        public virtual ItemType Type { get; set; } = ItemType.Text;

        /// <summary>
        /// 子内容的布局
        /// </summary>
        public virtual DrawingBase.Layout Layout { get; set; } = DrawingBase.Layout.Minimal;

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

        public virtual DrawingBase.Position VerticalAlignment { get; set; } = DrawingBase.Position.Top;

        public Binding Binding { get; set; }

        [JsonIgnore]
        public SKRect Boundary { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event MainSave.PropertyChangeEventArg OnPropertyChangedDetail;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChangedDetail?.Invoke(GetType().GetProperty(propertyName), null, GetType().GetProperty(propertyName)?.GetValue(this), null);
        }

        public void SubscribePropertyChangedEvents()
        {
            Margin.PropertyChanged -= NotifyPropertyChanged;
            Margin.PropertyChanged += NotifyPropertyChanged;
            Margin.OnPropertyChangedDetail -= Margin_NotifyPropertyChangedDetail;
            Margin.OnPropertyChangedDetail += Margin_NotifyPropertyChangedDetail;

            Padding.PropertyChanged -= NotifyPropertyChanged;
            Padding.PropertyChanged += NotifyPropertyChanged;
            Padding.OnPropertyChangedDetail -= Padding_NotifyPropertyChangedDetail;
            Padding.OnPropertyChangedDetail += Padding_NotifyPropertyChangedDetail;

            OverridePalette.OnPropertyChangedDetail -= Palette_OnPropertyChangedDetail;
            OverridePalette.OnPropertyChangedDetail += Palette_OnPropertyChangedDetail;
            OverridePalette.PropertyChanged -= NotifyPropertyChanged;
            OverridePalette.PropertyChanged += NotifyPropertyChanged;
        }

        public void UnsubscribePropertyChangedEvents()
        {
            Margin.PropertyChanged -= NotifyPropertyChanged;
            Margin.OnPropertyChangedDetail -= Margin_NotifyPropertyChangedDetail;
            Padding.PropertyChanged -= NotifyPropertyChanged;
            Padding.OnPropertyChangedDetail -= Padding_NotifyPropertyChangedDetail;
            OverridePalette.OnPropertyChangedDetail -= Palette_OnPropertyChangedDetail;
            OverridePalette.PropertyChanged -= NotifyPropertyChanged;
        }

        private void Palette_OnPropertyChangedDetail(PropertyInfo propertyInfo, PropertyInfo parentPropertyType, object newValue, object oldValue)
        {
            OnPropertyChangedDetail(propertyInfo, GetType().GetProperty(nameof(OverridePalette)), newValue, oldValue);
        }

        private void Padding_NotifyPropertyChangedDetail(PropertyInfo propertyInfo, PropertyInfo parentPropertyType, object newValue, object oldValue)
        {
            OnPropertyChangedDetail(propertyInfo, GetType().GetProperty(nameof(Padding)), newValue, oldValue);
        }

        private void Margin_NotifyPropertyChangedDetail(PropertyInfo propertyInfo, PropertyInfo parentPropertyType, object newValue, object oldValue)
        {
            OnPropertyChangedDetail(propertyInfo, GetType().GetProperty(nameof(Margin)), newValue, oldValue);
        }

        private void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
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
    }
}
