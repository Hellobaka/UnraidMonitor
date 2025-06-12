using me.cqp.luohuaming.UnraidMonitor.UI.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Windows
{
    /// <summary>
    /// CreateStyle.xaml 的交互逻辑
    /// </summary>
    public partial class CreateStyle : HandyControl.Controls.Window, INotifyPropertyChanged
    {
        public CreateStyle()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility Step1Visibility { get; set; } = Visibility.Visible;

        public Visibility Step2Visibility { get; set; } = Visibility.Collapsed;

        public Visibility Step3Visibility { get; set; } = Visibility.Collapsed;

        public bool CanLastStep { get; set; } = false;

        public bool CanNextStep { get; set; } = true;

        public int CurrentStep { get; set; } = 0;

        public ObservableCollection<ThemeItem> ThemeList { get; set; }

        public ThemeItem SelectedTheme { get; set; }

        public string NameInput { get; set; }

        public string WidthInput { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeList = [];
            ThemeList.Add(new ThemeItem
            {
                Name = "WinUI3 - Light",
                Preview = "pack://application:,,,/Resources/Images/WinUI3-Light.bmp"
            });
            ThemeList.Add(new ThemeItem
            {
                Name = "WinUI3 - Dark",
                Preview = "pack://application:,,,/Resources/Images/WinUI3-Dark.bmp"
            });
            ThemeList.Add(new ThemeItem
            {
                Name = "MaterialDesign2 - Light",
                Preview = "pack://application:,,,/Resources/Images/MaterialDesign2-Light.bmp"
            });
            ThemeList.Add(new ThemeItem
            {
                Name = "MaterialDesign2 - Dark",
                Preview = "pack://application:,,,/Resources/Images/MaterialDesign2-Dark.bmp"
            });
            ThemeList.Add(new ThemeItem
            {
                Name = "MaterialDesign3 - Light",
                Preview = "pack://application:,,,/Resources/Images/MaterialDesign3-Light.bmp"
            });
            ThemeList.Add(new ThemeItem
            {
                Name = "MaterialDesign3 - Dark",
                Preview = "pack://application:,,,/Resources/Images/MaterialDesign3-Dark.bmp"
            });
            ThemeList.Add(new ThemeItem
            {
                Name = "Unraid - Light",
                Preview = "pack://application:,,,/Resources/Images/Unraid-Light.bmp"
            });
            ThemeList.Add(new ThemeItem
            {
                Name = "Unraid - Dark",
                Preview = "pack://application:,,,/Resources/Images/Unraid-Dark.bmp"
            });
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LastStep_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentStep == 0)
            {
                return;
            }
            CurrentStep--;
            UpdateStepVisibility();
        }

        private void UpdateStepVisibility()
        {
            Step1Visibility = CurrentStep == 0 ? Visibility.Visible : Visibility.Collapsed;
            Step2Visibility = CurrentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            Step3Visibility = CurrentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            CanLastStep = CurrentStep > 0;
        }

        private async void NextStep_Click(object sender, RoutedEventArgs e)
        {
            var binding = StyleNameInput.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();
            binding = StyleWidthInput.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();

            if (CurrentStep == 2)
            {
                if (Validation.GetHasError(StyleNameInput) || Validation.GetHasError(StyleWidthInput))
                {
                    MainWindow.ShowError("样式名称或宽度无效，请检查输入");
                    return;
                }
                if (await MainWindow.ShowConfirmAsync("快速设置完成，点击确定以保存文件并进入工作台"))
                {
                    var dialog = new SaveFileDialog();
                    dialog.ShowDialog();
                    DialogResult = true;
                }
                return;
            }
            CurrentStep++;
            UpdateStepVisibility();
        }

        private void StyleNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var binding = StyleNameInput.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();
        }
    }
}
