using SysBot.Base;
using SysBot.Pokemon;

namespace SysBot.Pokemon.WinForms;

/// <summary>
/// 枚举值的中文翻译扩展方法
/// </summary>
public static class EnumTranslations
{
    /// <summary>
    /// 将任务类型枚举转换为中文显示文本
    /// </summary>
    /// <param name="type">任务类型枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this PokeRoutineType type) => type switch
    {
        PokeRoutineType.Idle => "空闲",
        PokeRoutineType.SurpriseTrade => "惊喜交换",
        PokeRoutineType.FlexTrade => "灵活交换",
        PokeRoutineType.LinkTrade => "链接交换",
        PokeRoutineType.SeedCheck => "种子检查",
        PokeRoutineType.Clone => "克隆",
        PokeRoutineType.Dump => "导出",
        PokeRoutineType.RaidBot => "极巨化团体战",
        PokeRoutineType.EncounterLine => "遇敌刷闪",
        PokeRoutineType.Reset => "重置刷闪",
        PokeRoutineType.DogBot => "传说犬类",
        PokeRoutineType.EggFetch => "取蛋",
        PokeRoutineType.FossilBot => "化石复活",
        PokeRoutineType.RemoteControl => "远程控制",
        _ => type.ToString()
    };

    /// <summary>
    /// 将通信协议枚举转换为中文显示文本
    /// </summary>
    /// <param name="protocol">通信协议枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this SwitchProtocol protocol) => protocol switch
    {
        SwitchProtocol.WiFi => "无线网络",
        SwitchProtocol.USB => "USB连接",
        _ => protocol.ToString()
    };

    /// <summary>
    /// 将程序模式枚举转换为中文显示文本
    /// </summary>
    /// <param name="mode">程序模式枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this ProgramMode mode) => mode switch
    {
        ProgramMode.None => "无",
        ProgramMode.SWSH => "剑盾",
        ProgramMode.BDSP => "晶灿钻石/明亮珍珠",
        ProgramMode.LA => "阿尔宙斯",
        ProgramMode.SV => "朱紫",
        ProgramMode.LZA => "LZA",
        _ => mode.ToString()
    };

    /// <summary>
    /// 将队列权重计算模式枚举转换为中文显示文本
    /// </summary>
    /// <param name="mode">队列权重计算模式枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this FlexBiasMode mode) => mode switch
    {
        FlexBiasMode.Add => "相加",
        FlexBiasMode.Multiply => "相乘",
        _ => mode.ToString()
    };

    /// <summary>
    /// 将队列优先级模式枚举转换为中文显示文本
    /// </summary>
    /// <param name="mode">队列优先级模式枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this FlexYieldMode mode) => mode switch
    {
        FlexYieldMode.LessCheatyFirst => "优先普通用户",
        FlexYieldMode.Weighted => "加权优先级",
        _ => mode.ToString()
    };

    /// <summary>
    /// 将队列开关模式枚举转换为中文显示文本
    /// </summary>
    /// <param name="mode">队列开关模式枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this QueueOpening mode) => mode switch
    {
        QueueOpening.Manual => "手动控制",
        QueueOpening.Threshold => "阈值模式",
        QueueOpening.Interval => "定时模式",
        _ => mode.ToString()
    };

    /// <summary>
    /// 将机器人同步模式枚举转换为中文显示文本
    /// </summary>
    /// <param name="option">机器人同步模式枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this BotSyncOption option) => option switch
    {
        BotSyncOption.NoSync => "不同步",
        BotSyncOption.LocalSync => "本地同步",
        _ => option.ToString()
    };

    /// <summary>
    /// 将闪光类型目标枚举转换为中文显示文本
    /// </summary>
    /// <param name="type">闪光类型目标枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this TargetShinyType type) => type switch
    {
        TargetShinyType.DisableOption => "无限制",
        TargetShinyType.NonShiny => "仅非闪光",
        TargetShinyType.AnyShiny => "任意闪光",
        TargetShinyType.StarOnly => "仅星形闪光",
        TargetShinyType.SquareOnly => "仅方形闪光",
        _ => type.ToString()
    };

    /// <summary>
    /// 将高度目标类型枚举转换为中文显示文本
    /// </summary>
    /// <param name="type">高度目标类型枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this TargetHeightType type) => type switch
    {
        TargetHeightType.DisableOption => "无限制",
        TargetHeightType.MinOnly => "仅最小高度",
        TargetHeightType.MaxOnly => "仅最大高度",
        TargetHeightType.MinOrMax => "最小或最大高度",
        _ => type.ToString()
    };

    /// <summary>
    /// 将交易滥用操作枚举转换为中文显示文本
    /// </summary>
    /// <param name="action">交易滥用操作枚举值</param>
    /// <returns>中文显示文本</returns>
    public static string ToChinese(this TradeAbuseAction action) => action switch
    {
        TradeAbuseAction.Ignore => "忽略",
        TradeAbuseAction.Quit => "退出",
        TradeAbuseAction.BlockAndQuit => "阻止并退出",
        _ => action.ToString()
    };
}
