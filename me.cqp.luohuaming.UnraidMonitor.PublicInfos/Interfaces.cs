using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.EventArgs;
using Newtonsoft.Json;
using PropertyChanged;
using System.Collections.ObjectModel;

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

    [AddINotifyPropertyChangedInterface]
    public class UnraidCommandConfig
    {
        public bool Enabled { get; set; } = true;

        public string Command { get; set; } = "help";

        public string Description { get; set; } = "命令描述";

        // 权限控制
        public PermissionSettings Permissions { get; set; } = new PermissionSettings();

        // 参数配置
        public ParameterSettings Parameters { get; set; } = new ParameterSettings();

        // SSH 相关
        public SshCommandConfig SshConfig { get; set; } = new SshCommandConfig();

        // 响应配置
        public ResponseSettings Responses { get; set; } = new ResponseSettings();

        // 高级配置
        public int Timeout { get; set; } = 30; // 单位：秒

        public bool IsAsync { get; set; } = false;

        public LoggingConfig Logging { get; set; } = new LoggingConfig();

        // 扩展字段
        public string Category { get; set; } = "default";

        public bool RequireConfirmation { get; set; } = false;
    }

    // 权限设置类
    [AddINotifyPropertyChangedInterface]
    public class PermissionSettings
    {
        public ObservableCollection<long> AllowedUsers { get; set; } = new ObservableCollection<long>();

        public ObservableCollection<long> AllowedGroups { get; set; } = new ObservableCollection<long>();

        public bool RequireAdmin { get; set; }
    }

    // 参数配置类
    [AddINotifyPropertyChangedInterface]
    public class ParameterSettings
    {
        public bool HasParameters { get; set; }

        [JsonProperty("min_params")]
        public int MinParameters { get; set; }

        [JsonProperty("max_params")]
        public int? MaxParameters { get; set; }

        [JsonProperty("parameter_pattern")]
        public string ParameterPattern { get; set; }

        [JsonProperty("validation_rules")]
        public ObservableCollection<ParameterValidationRule> ValidationRules { get; set; } = new ObservableCollection<ParameterValidationRule>();
    }

    // SSH 命令配置
    [AddINotifyPropertyChangedInterface]
    public class SshCommandConfig
    {
        public string Command { get; set; } = string.Empty;

        public bool UseSudo { get; set; } = false;

        public string WorkingDirectory { get; set; } = "/";
    }

    // 响应配置
    [AddINotifyPropertyChangedInterface]
    public class ResponseSettings
    {
        public ResponseTemplate Success { get; set; } = new ResponseTemplate();

        public ResponseTemplate Failure { get; set; } = new ResponseTemplate();

        public string PendingMessage { get; set; } = "命令正在执行中...";
    }

    // 参数验证规则
    [AddINotifyPropertyChangedInterface]
    public class ParameterValidationRule
    {
        public int ParameterIndex { get; set; }

        public string ValueType { get; set; } = "string";

        public bool IsRequired { get; set; } = true;

        public string ValidationRegex { get; set; } = ".*";
    }

    // 响应模板
    [AddINotifyPropertyChangedInterface]
    public class ResponseTemplate
    {
        public string Message { get; set; } = "操作{status}";

        public string Format { get; set; } = "text";

        public bool IncludeCommandOutput { get; set; } = false;

        public string OutputFilterRegex { get; set; } = ".*";
    }

    // 日志配置
    [AddINotifyPropertyChangedInterface]
    public class LoggingConfig
    {
        public bool Enabled { get; set; } = true;

        public string LogLevel { get; set; } = "Info";

        public bool LogParameters { get; set; } = false;
    }
}