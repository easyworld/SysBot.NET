using System;
using System.ComponentModel;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon;

public class QueueSettings
{
    private const string FeatureToggle = "功能开关";
    private const string UserBias = "用户偏向";
    private const string TimeBias = "时间偏向";
    private const string QueueToggle = "队列开关";
    public override string ToString() => "队列加入设置";

    // General

    [Category(FeatureToggle), Description("切换用户是否可以加入队列。")]
    public bool CanQueue { get; set; } = true;

    [Category(FeatureToggle), Description("如果队列中已有这么多用户，则阻止添加更多用户。")]
    public int MaxQueueCount { get; set; } = 999;

    [Category(FeatureToggle), Description("允许用户在交易过程中退出队列。")]
    public bool CanDequeueIfProcessing { get; set; }

    [Category(FeatureToggle), Description("确定Flex模式如何处理队列。")]
    public FlexYieldMode FlexMode { get; set; } = FlexYieldMode.Weighted;

    [Category(FeatureToggle), Description("确定队列何时开启和关闭。")]
    public QueueOpening QueueToggleMode { get; set; } = QueueOpening.Threshold;

    // Queue Toggle

    [Category(QueueToggle), Description("阈值模式：将导致队列开启的用户数量。")]
    public int ThresholdUnlock { get; set; }

    [Category(QueueToggle), Description("阈值模式：将导致队列关闭的用户数量。")]
    public int ThresholdLock { get; set; } = 30;

    [Category(QueueToggle), Description("计划模式：队列开启后锁定前的秒数。")]
    public int IntervalOpenFor { get; set; } = 5 * 60;

    [Category(QueueToggle), Description("计划模式：队列关闭后解锁前的秒数。")]
    public int IntervalCloseFor { get; set; } = 15 * 60;

    // Flex Users

    [Category(UserBias), Description("根据队列中的用户数量调整交易队列的权重。")]
    public int YieldMultCountTrade { get; set; } = 100;

    [Category(UserBias), Description("根据队列中的用户数量调整种子检查队列的权重。")]
    public int YieldMultCountSeedCheck { get; set; } = 100;

    [Category(UserBias), Description("根据队列中的用户数量调整克隆队列的权重。")]
    public int YieldMultCountClone { get; set; } = 100;

    [Category(UserBias), Description("根据队列中的用户数量调整导出队列的权重。")]
    public int YieldMultCountDump { get; set; } = 100;

    // Flex Time

    [Category(TimeBias), Description("确定权重是应该添加到总权重还是乘以总权重。")]
    public FlexBiasMode YieldMultWait { get; set; } = FlexBiasMode.Multiply;

    [Category(TimeBias), Description("检查用户加入交易队列后的时间，并相应增加队列权重。")]
    public int YieldMultWaitTrade { get; set; } = 1;

    [Category(TimeBias), Description("检查用户加入种子检查队列后的时间，并相应增加队列权重。")]
    public int YieldMultWaitSeedCheck { get; set; } = 1;

    [Category(TimeBias), Description("检查用户加入克隆队列后的时间，并相应增加队列权重。")]
    public int YieldMultWaitClone { get; set; } = 1;

    [Category(TimeBias), Description("检查用户加入导出队列后的时间，并相应增加队列权重。")]
    public int YieldMultWaitDump { get; set; } = 1;

    [Category(TimeBias), Description("乘以队列中的用户数量，以估计用户将被处理的时间。")]
    public float EstimatedDelayFactor { get; set; } = 1.1f;

    private int GetCountBias(PokeTradeType type) => type switch
    {
        PokeTradeType.Seed => YieldMultCountSeedCheck,
        PokeTradeType.Clone => YieldMultCountClone,
        PokeTradeType.Dump => YieldMultCountDump,
        _ => YieldMultCountTrade,
    };

    private int GetTimeBias(PokeTradeType type) => type switch
    {
        PokeTradeType.Seed => YieldMultWaitSeedCheck,
        PokeTradeType.Clone => YieldMultWaitClone,
        PokeTradeType.Dump => YieldMultWaitDump,
        _ => YieldMultWaitTrade,
    };

    /// <summary>
    /// Gets the weight of a <see cref="PokeTradeType"/> based on the count of users in the queue and time users have waited.
    /// </summary>
    /// <param name="count">Count of users for <see cref="type"/></param>
    /// <param name="time">Next-to-be-processed user's time joining the queue</param>
    /// <param name="type">Queue type</param>
    /// <returns>Effective weight for the trade type.</returns>
    public long GetWeight(int count, DateTime time, PokeTradeType type)
    {
        var now = DateTime.Now;
        var seconds = (now - time).Seconds;

        var cb = GetCountBias(type) * count;
        var tb = GetTimeBias(type) * seconds;

        return YieldMultWait switch
        {
            FlexBiasMode.Multiply => cb * tb,
            _ => cb + tb,
        };
    }

    /// <summary>
    /// Estimates the amount of time (minutes) until the user will be processed.
    /// </summary>
    /// <param name="position">Position in the queue</param>
    /// <param name="botct">Amount of bots processing requests</param>
    /// <returns>Estimated time in Minutes</returns>
    public float EstimateDelay(int position, int botct) => (EstimatedDelayFactor * position) / botct;
}

public enum FlexBiasMode
{
    Add,
    Multiply,
}

public enum FlexYieldMode
{
    LessCheatyFirst,
    Weighted,
}

public enum QueueOpening
{
    Manual,
    Threshold,
    Interval,
}
