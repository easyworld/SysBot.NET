using System.ComponentModel;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon;

public sealed class PokeTradeHubConfig : BaseConfig
{
    private const string BotTrade = "交易机器人";
    private const string BotEncounter = "遭遇机器人";
    private const string Integration = "集成设置";

    [Browsable(false)]
    public override bool Shuffled => Distribution.Shuffled;

    [Category(Operation)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public QueueSettings Queues { get; set; } = new();

    [Category(Operation), Description("为较慢的Switch增加额外时间。")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TimingSettings Timings { get; set; } = new();

    // Trade Bots

    [Category(BotTrade)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TradeSettings Trade { get; set; } = new();

    [Category(BotTrade), Description("空闲分发交易的设置。")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public DistributionSettings Distribution { get; set; } = new();

    [Category(BotTrade)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public SeedCheckSettings SeedCheckSWSH { get; set; } = new();

    [Category(BotTrade)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TradeAbuseSettings TradeAbuse { get; set; } = new();

    // Encounter Bots - For finding or hosting Pokémon in-game.

    [Category(BotEncounter)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public EncounterSettings EncounterSWSH { get; set; } = new();

    [Category(BotEncounter)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public RaidSettings RaidSWSH { get; set; } = new();

    [Category(BotEncounter), Description("遭遇机器人的停止条件。")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public StopConditionSettings StopConditions { get; set; } = new();

    // Integration

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public DiscordSettings Discord { get; set; } = new();

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TwitchSettings Twitch { get; set; } = new();

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public YouTubeSettings YouTube { get; set; } = new();

    [Category(Integration), Description("配置流媒体资源的生成。")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public StreamSettings Stream { get; set; } = new();

    [Category(Integration), Description("允许受青睐的用户以比普通用户更有利的位置加入队列。")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public FavoredPrioritySettings Favoritism { get; set; } = new();

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public QQSettings QQ { get; set; } = new();

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public BilibiliSettings Bilibili { get; set; } = new();

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public DodoSettings Dodo { get; set; } = new();

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public KookSettings Kook { get; set; } = new();
}
