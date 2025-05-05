using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Enum;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.EventArgs;
using Newtonsoft.Json;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public interface IOrderModel
    {
        bool ImplementFlag { get; set; }
        /// <summary>
        /// 优先级，越高越优先处理
        /// </summary>
        int Priority { get; set; }
        string GetCommand();
        bool CanExecute(string destStr);
        FunctionResult Execute(CQGroupMessageEventArgs e);
        FunctionResult Execute(CQPrivateMessageEventArgs e);
    }

    public class UnraidCommandConfig : INotifyPropertyChanged
    {
        // 基础配置
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("command")]
        public string Command { get; set; } = "help";

        [JsonProperty("description")]
        public string Description { get; set; } = "命令描述";

        // 权限控制
        [JsonProperty("permissions")]
        public PermissionSettings Permissions { get; set; } = new PermissionSettings();

        // 参数配置
        [JsonProperty("parameters")]
        public ParameterSettings Parameters { get; set; } = new ParameterSettings();

        // SSH 相关
        [JsonProperty("ssh_config")]
        public SshCommandConfig SshConfig { get; set; } = new SshCommandConfig();

        // 响应配置
        [JsonProperty("responses")]
        public ResponseSettings Responses { get; set; } = new ResponseSettings();

        // 高级配置
        [JsonProperty("timeout")]
        public int Timeout { get; set; } = 30; // 单位：秒

        [JsonProperty("is_async")]
        public bool IsAsync { get; set; } = false;

        [JsonProperty("logging")]
        public LoggingConfig Logging { get; set; } = new LoggingConfig();

        // 扩展字段
        [JsonProperty("category")]
        public string Category { get; set; } = "default";

        [JsonProperty("require_confirmation")]
        public bool RequireConfirmation { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // 权限设置类
    public class PermissionSettings : INotifyPropertyChanged
    {
        [JsonProperty("allowed_users")]
        public ObservableCollection<long> AllowedUsers { get; set; } = new ObservableCollection<long>();

        [JsonProperty("allowed_groups")]
        public ObservableCollection<long> AllowedGroups { get; set; } = new ObservableCollection<long>();

        [JsonProperty("require_admin")]
        private bool _requireAdmin = false;
        public bool RequireAdmin
        {
            get => _requireAdmin;
            set { _requireAdmin = value; OnPropertyChanged(nameof(RequireAdmin)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // 参数配置类
    public class ParameterSettings : INotifyPropertyChanged
    {
        [JsonProperty("has_parameters")]
        private bool _hasParameters = false;
        public bool HasParameters
        {
            get => _hasParameters;
            set { _hasParameters = value; OnPropertyChanged(nameof(HasParameters)); }
        }

        [JsonProperty("min_params")]
        private int _minParameters = 0;
        public int MinParameters
        {
            get => _minParameters;
            set { _minParameters = value; OnPropertyChanged(nameof(MinParameters)); }
        }

        [JsonProperty("max_params")]
        private int? _maxParameters = null;
        public int? MaxParameters
        {
            get => _maxParameters;
            set { _maxParameters = value; OnPropertyChanged(nameof(MaxParameters)); }
        }

        [JsonProperty("parameter_pattern")]
        private string _parameterPattern = ".*";
        public string ParameterPattern
        {
            get => _parameterPattern;
            set { _parameterPattern = value; OnPropertyChanged(nameof(ParameterPattern)); }
        }

        [JsonProperty("validation_rules")]
        public ObservableCollection<ParameterValidationRule> ValidationRules { get; set; } = new ObservableCollection<ParameterValidationRule>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // SSH 命令配置
    public class SshCommandConfig
    {
        [JsonProperty("command")]
        public string Command { get; set; } = string.Empty;

        [JsonProperty("sudo")]
        public bool UseSudo { get; set; } = false;

        [JsonProperty("working_directory")]
        public string WorkingDirectory { get; set; } = "/";
    }

    // 响应配置
    public class ResponseSettings
    {
        [JsonProperty("success")]
        public ResponseTemplate Success { get; set; } = new ResponseTemplate();

        [JsonProperty("failure")]
        public ResponseTemplate Failure { get; set; } = new ResponseTemplate();

        [JsonProperty("pending")]
        public string PendingMessage { get; set; } = "命令正在执行中...";
    }

    // 参数验证规则
    public class ParameterValidationRule
    {
        [JsonProperty("param_index")]
        public int ParameterIndex { get; set; }

        [JsonProperty("type")]
        public string ValueType { get; set; } = "string";

        [JsonProperty("required")]
        public bool IsRequired { get; set; } = true;

        [JsonProperty("regex")]
        public string ValidationRegex { get; set; } = ".*";
    }

    // 响应模板
    public class ResponseTemplate
    {
        [JsonProperty("message")]
        public string Message { get; set; } = "操作{status}";

        [JsonProperty("format")]
        public string Format { get; set; } = "text";

        [JsonProperty("include_output")]
        public bool IncludeCommandOutput { get; set; } = false;

        [JsonProperty("output_filter")]
        public string OutputFilterRegex { get; set; } = ".*";
    }

    // 日志配置
    public class LoggingConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("level")]
        public string LogLevel { get; set; } = "Info";

        [JsonProperty("log_parameters")]
        public bool LogParameters { get; set; } = false;
    }
}
