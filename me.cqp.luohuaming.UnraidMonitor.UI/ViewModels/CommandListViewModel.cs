using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.UI.Helpers;
using Microsoft.Win32;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace me.cqp.luohuaming.UnraidMonitor.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class CommandListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<UnraidCommandConfig> CommandList { get; set; } = new ObservableCollection<UnraidCommandConfig>();

        public UnraidCommandConfig SelectedCommand { get; set; }

        [OnChangedMethod(nameof(FilterCommandList))]
        public string SearchText { get; set; }

        public ICommand AddCommand { get; }

        public ICommand RemoveCommand { get; }

        public ICommand ImportCommand { get; }

        public ICommand ExportCommand { get; }

        public ICommand EditCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand MoveUpCommand { get; }

        public ICommand MoveDownCommand { get; }

        public ICommand SaveAllCommand { get; }

        private ObservableCollection<UnraidCommandConfig> _allCommands = new ObservableCollection<UnraidCommandConfig>();

        public CommandListViewModel()
        {
            // 启动时自动加载
            try
            {
                var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "commands.json");
                if (System.IO.File.Exists(path))
                {
                    var list = JsonHelper.LoadFromFile<ObservableCollection<UnraidCommandConfig>>(path);
                    if (list != null)
                    {
                        CommandList.Clear();
                        _allCommands.Clear();
                        foreach (var cmd in list)
                        {
                            CommandList.Add(cmd);
                            _allCommands.Add(cmd);
                        }
                        SelectedCommand = CommandList.FirstOrDefault();
                    }
                }
            }
            catch { }

            // 可在此处初始化数据
            AddCommand = new RelayCommand(() =>
            {
                var newCmd = new UnraidCommandConfig { Command = "新指令" };
                CommandList.Add(newCmd);
                _allCommands.Add(newCmd);
                SelectedCommand = newCmd;
            });
            RemoveCommand = new RelayCommand(() =>
            {
                if (SelectedCommand != null)
                {
                    CommandList.Remove(SelectedCommand);
                    _allCommands.Remove(SelectedCommand);
                    SelectedCommand = CommandList.FirstOrDefault();
                }
            });
            ImportCommand = new RelayCommand(() =>
            {
                var dlg = new OpenFileDialog { Filter = "JSON文件|*.json" };
                if (dlg.ShowDialog() == true)
                {
                    var list = JsonHelper.LoadFromFile<ObservableCollection<UnraidCommandConfig>>(dlg.FileName);
                    if (list != null)
                    {
                        CommandList.Clear();
                        _allCommands.Clear();
                        foreach (var cmd in list)
                        {
                            CommandList.Add(cmd);
                            _allCommands.Add(cmd);
                        }
                        SelectedCommand = CommandList.FirstOrDefault();
                    }
                }
            });
            ExportCommand = new RelayCommand(() =>
            {
                var dlg = new SaveFileDialog { Filter = "JSON文件|*.json" };
                if (dlg.ShowDialog() == true)
                {
                    JsonHelper.SaveToFile(dlg.FileName, CommandList);
                }
            });
            EditCommand = new RelayCommand(() =>
            {
                MessageBox.Show("请在右侧编辑详细内容。", "编辑");
            });
            CopyCommand = new RelayCommand(() =>
            {
                if (SelectedCommand != null)
                {
                    var copy = Newtonsoft.Json.JsonConvert.DeserializeObject<UnraidCommandConfig>(Newtonsoft.Json.JsonConvert.SerializeObject(SelectedCommand));
                    copy.Command += "_副本";
                    CommandList.Add(copy);
                    _allCommands.Add(copy);
                    SelectedCommand = copy;
                }
            });
            MoveUpCommand = new RelayCommand(() =>
            {
                if (SelectedCommand != null)
                {
                    int idx = CommandList.IndexOf(SelectedCommand);
                    if (idx > 0)
                    {
                        CommandList.Move(idx, idx - 1);
                        _allCommands.Move(idx, idx - 1);
                    }
                }
            });
            MoveDownCommand = new RelayCommand(() =>
            {
                if (SelectedCommand != null)
                {
                    int idx = CommandList.IndexOf(SelectedCommand);
                    if (idx < CommandList.Count - 1 && idx >= 0)
                    {
                        CommandList.Move(idx, idx + 1);
                        _allCommands.Move(idx, idx + 1);
                    }
                }
            });
            SaveAllCommand = new RelayCommand(() =>
            {
                var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "commands.json");
                JsonHelper.SaveToFile(path, CommandList);
                MessageBox.Show("全部指令已保存。", "保存", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void FilterCommandList()
        {
            CommandList.Clear();
            foreach (var cmd in string.IsNullOrWhiteSpace(SearchText)
                ? _allCommands
                : _allCommands.Where(c => c.Command.Contains(SearchText) || c.Description.Contains(SearchText)))
            {
                CommandList.Add(cmd);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}