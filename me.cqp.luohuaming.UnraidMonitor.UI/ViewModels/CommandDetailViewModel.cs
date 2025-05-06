using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace me.cqp.luohuaming.UnraidMonitor.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class CommandDetailViewModel : INotifyPropertyChanged
    {
        public UnraidCommandConfig CurrentConfig { get; set; }

        public ICommand AddUserCommand { get; }

        public ICommand RemoveUserCommand { get; }

        public ICommand AddGroupCommand { get; }

        public ICommand RemoveGroupCommand { get; }

        public ICommand AddRuleCommand { get; }

        public ICommand RemoveRuleCommand { get; }

        public ICommand CopyPermissionFromCommandCommand { get; }

        public ICommand EditUserCommand { get; }

        public ICommand EditGroupCommand { get; }

        public ICommand EditRuleCommand { get; }

        public ICommand RemoveSelectedUserCommand { get; }

        public ICommand RemoveSelectedGroupCommand { get; }

        public ICommand RemoveSelectedRuleCommand { get; }

        public ICommand SaveCurrentCommand { get; }

        public ObservableCollection<UnraidCommandConfig> CommandList { get; set; }

        public long? SelectedUser { get; set; }

        public long? SelectedGroup { get; set; }

        public ParameterValidationRule SelectedRule { get; set; }

        public CommandDetailViewModel(ObservableCollection<UnraidCommandConfig> commandList = null)
        {
            CommandList = commandList;

            AddUserCommand = new RelayCommand<string>(userId =>
            {
                if (long.TryParse(userId, out var id) && !CurrentConfig.Permissions.AllowedUsers.Contains(id))
                {
                    CurrentConfig.Permissions.AllowedUsers.Add(id);
                }
            });
            RemoveUserCommand = new RelayCommand(() =>
            {
                if (CurrentConfig.Permissions.AllowedUsers.Count > 0)
                {
                    CurrentConfig.Permissions.AllowedUsers.RemoveAt(CurrentConfig.Permissions.AllowedUsers.Count - 1);
                }
            });
            AddGroupCommand = new RelayCommand<string>(groupId =>
            {
                if (long.TryParse(groupId, out var id) && !CurrentConfig.Permissions.AllowedGroups.Contains(id))
                {
                    CurrentConfig.Permissions.AllowedGroups.Add(id);
                }
            });
            RemoveGroupCommand = new RelayCommand(() =>
            {
                if (CurrentConfig.Permissions.AllowedGroups.Count > 0)
                {
                    CurrentConfig.Permissions.AllowedGroups.RemoveAt(CurrentConfig.Permissions.AllowedGroups.Count - 1);
                }
            });
            AddRuleCommand = new RelayCommand(() =>
            {
                CurrentConfig.Parameters.ValidationRules.Add(new ParameterValidationRule());
            });
            RemoveRuleCommand = new RelayCommand(() =>
            {
                if (CurrentConfig.Parameters.ValidationRules.Count > 0)
                {
                    CurrentConfig.Parameters.ValidationRules.RemoveAt(CurrentConfig.Parameters.ValidationRules.Count - 1);
                }
            });
            CommandList = commandList;
            CopyPermissionFromCommandCommand = new RelayCommand<UnraidCommandConfig>(cmd =>
            {
                if (cmd != null && cmd != CurrentConfig)
                {
                    CurrentConfig.Permissions.AllowedUsers.Clear();
                    foreach (var u in cmd.Permissions.AllowedUsers)
                    {
                        CurrentConfig.Permissions.AllowedUsers.Add(u);
                    }

                    CurrentConfig.Permissions.AllowedGroups.Clear();
                    foreach (var g in cmd.Permissions.AllowedGroups)
                    {
                        CurrentConfig.Permissions.AllowedGroups.Add(g);
                    }

                    CurrentConfig.Permissions.RequireAdmin = cmd.Permissions.RequireAdmin;
                    OnPropertyChanged(nameof(CurrentConfig));
                }
            });
            RemoveSelectedUserCommand = new RelayCommand(() =>
            {
                if (SelectedUser.HasValue && CurrentConfig.Permissions.AllowedUsers.Contains(SelectedUser.Value))
                {
                    CurrentConfig.Permissions.AllowedUsers.Remove(SelectedUser.Value);
                }
            });
            RemoveSelectedGroupCommand = new RelayCommand(() =>
            {
                if (SelectedGroup.HasValue && CurrentConfig.Permissions.AllowedGroups.Contains(SelectedGroup.Value))
                {
                    CurrentConfig.Permissions.AllowedGroups.Remove(SelectedGroup.Value);
                }
            });
            RemoveSelectedRuleCommand = new RelayCommand(() =>
            {
                if (SelectedRule != null && CurrentConfig.Parameters.ValidationRules.Contains(SelectedRule))
                {
                    CurrentConfig.Parameters.ValidationRules.Remove(SelectedRule);
                }
            });
            EditUserCommand = new RelayCommand(() =>
            {
                MessageBox.Show("请在左侧列表中选择要编辑的用户ID，然后在输入框中修改后删除再添加即可。", "编辑提示");
            });
            EditGroupCommand = new RelayCommand(() =>
            {
                MessageBox.Show("请在左侧列表中选择要编辑的群组ID，然后在输入框中修改后删除再添加即可。", "编辑提示");
            });
            EditRuleCommand = new RelayCommand(() =>
            {
                MessageBox.Show("请直接在表格中编辑参数规则。", "编辑提示");
            });
            SaveCurrentCommand = new RelayCommand(() =>
            {
                if (CurrentConfig == null || CommandList == null)
                {
                    return;
                }

                var idx = CommandList.IndexOf(CommandList.FirstOrDefault(c => c.Command == CurrentConfig.Command));
                if (idx >= 0)
                {
                    // 用深拷贝防止引用直接变更
                    var copy = Newtonsoft.Json.JsonConvert.DeserializeObject<UnraidCommandConfig>(Newtonsoft.Json.JsonConvert.SerializeObject(CurrentConfig));
                    CommandList[idx] = copy;
                    MessageBox.Show("本指令已保存到列表，点击左侧‘保存全部’可写入文件。", "保存", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}