using System.ComponentModel;

namespace SysBot.Pokemon;

public class TimingSettings
{
    private const string OpenGame = "打开游戏";
    private const string CloseGame = "关闭游戏";
    private const string Raid = "极巨化团体战";
    private const string Misc = "其他";
    public override string ToString() => "额外时间设置";

    // Opening the game.
    [Category(OpenGame), Description("启动游戏时等待配置文件加载的额外时间（毫秒）。")]
    public int ExtraTimeLoadProfile { get; set; }

    [Category(OpenGame), Description("在标题屏幕点击A键前等待的额外时间（毫秒）。")]
    public int ExtraTimeLoadGame { get; set; } = 5000;

    [Category(OpenGame), Description("标题屏幕后等待游戏世界加载的额外时间（毫秒）。")]
    public int ExtraTimeLoadOverworld { get; set; } = 3000;

    // Closing the game.
    [Category(CloseGame), Description("按下HOME键最小化游戏后等待的额外时间（毫秒）。")]
    public int ExtraTimeReturnHome { get; set; }

    [Category(CloseGame), Description("点击关闭游戏后等待的额外时间（毫秒）。")]
    public int ExtraTimeCloseGame { get; set; }

    // Raid-specific timings.
    [Category(Raid), Description("[RaidBot] 点击巢穴后等待极巨化团体战加载的额外时间（毫秒）。")]
    public int ExtraTimeLoadRaid { get; set; }

    [Category(Raid), Description("[RaidBot] 点击\"邀请他人\"后锁定宝可梦前等待的额外时间（毫秒）。")]
    public int ExtraTimeOpenRaid { get; set; }

    [Category(Raid), Description("[RaidBot] 关闭游戏重置极巨化团体战前等待的额外时间（毫秒）。")]
    public int ExtraTimeEndRaid { get; set; }

    [Category(Raid), Description("[RaidBot] 接受好友请求后等待的额外时间（毫秒）。")]
    public int ExtraTimeAddFriend { get; set; }

    [Category(Raid), Description("[RaidBot] 删除好友后等待的额外时间（毫秒）。")]
    public int ExtraTimeDeleteFriend { get; set; }

    // Miscellaneous settings.
    [Category(Misc), Description("[剑盾/朱紫] 点击+键连接Y-Comm（剑盾）或L键连接在线（朱紫）后等待的额外时间（毫秒）。")]
    public int ExtraTimeConnectOnline { get; set; }

    [Category(Misc), Description("连接丢失后尝试重新连接到套接字的次数。设置为-1可无限尝试。")]
    public int ReconnectAttempts { get; set; } = 30;

    [Category(Misc), Description("两次重新连接尝试之间等待的额外时间（毫秒）。基础时间为30秒。")]
    public int ExtraReconnectDelay { get; set; }

    [Category(Misc), Description("[晶灿钻石/明亮珍珠] 离开联合房间后等待游戏世界加载的额外时间（毫秒）。")]
    public int ExtraTimeLeaveUnionRoom { get; set; } = 1000;

    [Category(Misc), Description("[晶灿钻石/明亮珍珠] 每次交易循环开始时等待Y菜单加载的额外时间（毫秒）。")]
    public int ExtraTimeOpenYMenu { get; set; } = 500;

    [Category(Misc), Description("[晶灿钻石/明亮珍珠] 尝试发起交易前等待联合房间加载的额外时间（毫秒）。")]
    public int ExtraTimeJoinUnionRoom { get; set; } = 500;

    [Category(Misc), Description("[朱紫] 等待宝可梦传送上门加载的额外时间（毫秒）。")]
    public int ExtraTimeLoadPortal { get; set; } = 1000;

    [Category(Misc), Description("找到交易后等待宝可梦盒加载的额外时间（毫秒）。")]
    public int ExtraTimeOpenBox { get; set; } = 1000;

    [Category(Misc), Description("交易过程中打开代码输入键盘后等待的时间。")]
    public int ExtraTimeOpenCodeEntry { get; set; } = 1000;

    [Category(Misc), Description("导航Switch菜单或输入链接代码时每次按键后等待的时间。")]
    public int KeypressTime { get; set; }

    [Category(Misc), Description("启用此选项可拒绝传入的系统更新。")]
    public bool AvoidSystemUpdate { get; set; }
}
