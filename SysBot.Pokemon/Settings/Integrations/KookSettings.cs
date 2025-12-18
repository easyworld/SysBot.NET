using System.ComponentModel;

namespace SysBot.Pokemon;

public class KookSettings
{
    private const string Startup = "启动设置";
    private const string Operation = "操作设置";
    private const string Channels = "频道设置";
    private const string Roles = "角色设置";
    private const string Users = "用户设置";
    public override string ToString() => "Kook集成设置";

    // Startup

    [Category(Startup), Description("机器人登录令牌。")]
    public string Token { get; set; } = string.Empty;

    [Category(Startup), Description("机器人命令前缀。")]
    public string CommandPrefix { get; set; } = "$";

    [Category(Startup), Description("机器人启动时不会加载的模块列表（逗号分隔）。")]
    public string ModuleBlacklist { get; set; } = string.Empty;

    [Category(Startup), Description("切换命令处理方式：异步或同步。")]
    public bool AsyncCommands { get; set; }

    [Category(Operation), Description("当用户向机器人打招呼时，机器人将回复的自定义消息。使用字符串格式化在回复中提及用户。")]
    public string HelloResponse { get; set; } = "Hi {0}!";

    // 白名单

    [Category(Roles), Description("具有此角色的用户可以进入交易队列。")]
    public RemoteControlAccessList RoleCanTrade { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("具有此角色的用户可以进入种子检查队列。")]
    public RemoteControlAccessList RoleCanSeedCheck { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("具有此角色的用户可以进入克隆队列。")]
    public RemoteControlAccessList RoleCanClone { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("具有此角色的用户可以进入导出队列。")]
    public RemoteControlAccessList RoleCanDump { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("具有此角色的用户可以远程控制控制台（如果作为远程控制机器人运行）。")]
    public RemoteControlAccessList RoleRemoteControl { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("具有此角色的用户可以绕过命令限制。")]
    public RemoteControlAccessList RoleSudo { get; set; } = new() { AllowIfEmpty = false };

    // 操作

    [Category(Roles), Description("具有此角色的用户可以以更好的位置加入队列。")]
    public RemoteControlAccessList RoleFavored { get; set; } = new() { AllowIfEmpty = false };

    [Category(Users), Description("具有这些用户ID的用户不能使用机器人。")]
    public RemoteControlAccessList UserBlacklist { get; set; } = new();

    [Category(Channels), Description("只有具有这些ID的频道，机器人才会响应命令。")]
    public RemoteControlAccessList ChannelWhitelist { get; set; } = new();

    [Category(Users), Description("逗号分隔的Discord用户ID，这些用户将拥有Bot Hub的sudo访问权限。")]
    public RemoteControlAccessList GlobalSudoList { get; set; } = new();

    [Category(Users), Description("禁用此项将移除全局sudo支持。")]
    public bool AllowGlobalSudo { get; set; } = true;

    [Category(Channels), Description("将回显日志机器人数据的频道ID。")]
    public RemoteControlAccessList LoggingChannels { get; set; } = new();

    [Category(Channels), Description("将记录交易开始消息的记录器频道。")]
    public RemoteControlAccessList TradeStartingChannels { get; set; } = new();

    [Category(Channels), Description("将记录特殊消息的回显频道。")]
    public RemoteControlAccessList EchoChannels { get; set; } = new();

    [Category(Operation), Description("将交易中显示的宝可梦的PKM文件返回给用户。")]
    public bool ReturnPKMs { get; set; } = true;

    [Category(Operation), Description("如果用户不允许在频道中使用给定命令，机器人会回复用户。当为false时，机器人会默默地忽略他们。")]
    public bool ReplyCannotUseCommandInChannel { get; set; } = true;

    [Category(Operation), Description("机器人会监听频道消息，每当附件中有PKM文件时（不是通过命令），都会回复ShowdownSet。")]
    public bool ConvertPKMToShowdownSet { get; set; } = true;

    [Category(Operation), Description("机器人可以在它能看到的任何频道中回复ShowdownSet，而不仅仅是机器人被白名单允许运行的频道。只有当你希望机器人在非机器人频道中提供更多实用功能时，才将此设置为true。")]
    public bool ConvertPKMReplyAnyChannel { get; set; }
}
