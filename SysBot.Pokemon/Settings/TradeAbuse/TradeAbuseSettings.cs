using System.ComponentModel;

namespace SysBot.Pokemon;

public class TradeAbuseSettings
{
    private const string Monitoring = nameof(Monitoring);
    public override string ToString() => "交易滥用监控设置";

    [Category(Monitoring), Description("当同一个人在设定值（分钟）内再次出现时，将发送通知。")]
    public double TradeCooldown { get; set; }

    [Category(Monitoring), Description("当某人忽略交易冷却时间时，回声消息将包含其Nintendo账户ID。")]
    public bool EchoNintendoOnlineIDCooldown { get; set; } = true;

    [Category(Monitoring), Description("如果不为空，当用户违反交易冷却时间时，提供的字符串将附加到回声警报中，以通知您指定的任何人。对于Discord，使用<@userIDnumber>进行提及。")]
    public string CooldownAbuseEchoMention { get; set; } = string.Empty;

    [Category(Monitoring), Description("当同一个人使用不同的Discord/Twitch账户在设定值（分钟）内再次出现时，将发送通知。")]
    public double TradeAbuseExpiration { get; set; } = 120;

    [Category(Monitoring), Description("当检测到某人使用多个Discord/Twitch账户时，回声消息将包含其Nintendo账户ID。")]
    public bool EchoNintendoOnlineIDMulti { get; set; } = true;

    [Category(Monitoring), Description("当检测到某人发送到多个游戏内账户时，回声消息将包含其Nintendo账户ID。")]
    public bool EchoNintendoOnlineIDMultiRecipients { get; set; } = true;

    [Category(Monitoring), Description("当检测到某人使用多个Discord/Twitch账户时，将采取此操作。")]
    public TradeAbuseAction TradeAbuseAction { get; set; } = TradeAbuseAction.Quit;

    [Category(Monitoring), Description("当某人因使用多个账户在游戏中被阻止时，其在线ID将被添加到BannedIDs中。")]
    public bool BanIDWhenBlockingUser { get; set; } = true;

    [Category(Monitoring), Description("如果不为空，当发现用户使用多个账户时，提供的字符串将附加到回声警报中，以通知您指定的任何人。对于Discord，使用<@userIDnumber>进行提及。")]
    public string MultiAbuseEchoMention { get; set; } = string.Empty;

    [Category(Monitoring), Description("如果不为空，当发现用户发送到游戏内多个玩家时，提供的字符串将附加到回声警报中，以通知您指定的任何人。对于Discord，使用<@userIDnumber>进行提及。")]
    public string MultiRecipientEchoMention { get; set; } = string.Empty;

    [Category(Monitoring), Description("将触发交易退出或游戏内阻止的被禁在线ID列表。")]
    public RemoteControlAccessList BannedIDs { get; set; } = new();

    [Category(Monitoring), Description("当遇到具有被禁ID的人时，在退出交易前在游戏内阻止他们。")]
    public bool BlockDetectedBannedUser { get; set; } = true;

    [Category(Monitoring), Description("如果不为空，当用户匹配被禁ID时，提供的字符串将附加到回声警报中，以通知您指定的任何人。对于Discord，使用<@userIDnumber>进行提及。")]
    public string BannedIDMatchEchoMention { get; set; } = string.Empty;

    [Category(Monitoring), Description("当检测到某人使用Ledy昵称交换进行滥用时，回声消息将包含其Nintendo账户ID。")]
    public bool EchoNintendoOnlineIDLedy { get; set; } = true;

    [Category(Monitoring), Description("如果不为空，当用户违反Ledy交易规则时，提供的字符串将附加到回声警报中，以通知您指定的任何人。对于Discord，使用<@userIDnumber>进行提及。")]
    public string LedyAbuseEchoMention { get; set; } = string.Empty;
}
