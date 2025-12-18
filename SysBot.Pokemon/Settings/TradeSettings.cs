using PKHeX.Core;
using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon;

public class TradeSettings : IBotStateSettings, ICountSettings
{
    private const string TradeCode = "交易代码";
    private const string TradeConfig = "交易配置";
    private const string Dumping = "导出设置";
    private const string Counts = "计数设置";
    public override string ToString() => "交易机器人设置";

    [Category(TradeConfig), Description("等待交易伙伴的时间（秒）。")]
    public int TradeWaitTime { get; set; } = 30;

    [Category(TradeConfig), Description("按下A键等待交易处理的最大时间（秒）。")]
    public int MaxTradeConfirmTime { get; set; } = 25;

    [Category(TradeCode), Description("最小连接代码。")]
    public int MinTradeCode { get; set; } = 0;

    [Category(TradeCode), Description("最大连接代码。")]
    public int MaxTradeCode { get; set; } = 9999_9999;

    [Category(Dumping), Description("导出交易：导出例程将在单个用户的导出次数达到最大值后停止。")]
    public int MaxDumpsPerTrade { get; set; } = 20;

    [Category(Dumping), Description("导出交易：导出例程将在交易中花费x秒后停止。")]
    public int MaxDumpTradeTime { get; set; } = 180;

    [Category(Dumping), Description("导出交易：如果启用，导出例程将向用户输出合法性检查信息。")]
    public bool DumpTradeLegalityCheck { get; set; } = true;

    [Category(TradeConfig), Description("启用时，屏幕将在机器人正常循环运行期间关闭以节省电量。")]
    public bool ScreenOff { get; set; }

    [Category(TradeConfig), Description("单次交易的最大宝可梦数量。如果此配置小于1，批量模式将关闭。")]
    public int MaxPkmsPerTrade { get; set; } = 1;
    
    [Category(TradeConfig), Description("启用时，不允许请求来自其原始环境之外的宝可梦。")]
    public bool DisallowNonNatives { get; set; } = true;

    [Category(TradeConfig), Description("启用时，不允许请求带有HOME追踪器的宝可梦。")]
    public bool DisallowTracked { get; set; } = true;

    [Category(TradeConfig), Description("启用时，如果收到会进化的宝可梦，机器人将自动取消交易。")]
    public bool DisallowTradeEvolve { get; set; } = true;

    /// <summary>
    /// Gets a random trade code based on the range settings.
    /// </summary>
    public int GetRandomTradeCode() => Util.Rand.Next(MinTradeCode, MaxTradeCode + 1);

    private int _completedSurprise;
    private int _completedDistribution;
    private int _completedTrades;
    private int _completedSeedChecks;
    private int _completedClones;
    private int _completedDumps;

    [Category(Counts), Description("已完成的惊喜交易")]
    public int CompletedSurprise
    {
        get => _completedSurprise;
        set => _completedSurprise = value;
    }

    [Category(Counts), Description("已完成的连接交易（分发）")]
    public int CompletedDistribution
    {
        get => _completedDistribution;
        set => _completedDistribution = value;
    }

    [Category(Counts), Description("已完成的连接交易（特定用户）")]
    public int CompletedTrades
    {
        get => _completedTrades;
        set => _completedTrades = value;
    }

    [Category(Counts), Description("已完成的种子检查交易")]
    public int CompletedSeedChecks
    {
        get => _completedSeedChecks;
        set => _completedSeedChecks = value;
    }

    [Category(Counts), Description("已完成的克隆交易（特定用户）")]
    public int CompletedClones
    {
        get => _completedClones;
        set => _completedClones = value;
    }

    [Category(Counts), Description("已完成的导出交易（特定用户）")]
    public int CompletedDumps
    {
        get => _completedDumps;
        set => _completedDumps = value;
    }

    [Category(Counts), Description("启用时，将在请求状态检查时发出计数。")]
    public bool EmitCountsOnStatusCheck { get; set; }

    public void AddCompletedTrade() => Interlocked.Increment(ref _completedTrades);
    public void AddCompletedSeedCheck() => Interlocked.Increment(ref _completedSeedChecks);
    public void AddCompletedSurprise() => Interlocked.Increment(ref _completedSurprise);
    public void AddCompletedDistribution() => Interlocked.Increment(ref _completedDistribution);
    public void AddCompletedDumps() => Interlocked.Increment(ref _completedDumps);
    public void AddCompletedClones() => Interlocked.Increment(ref _completedClones);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedSeedChecks != 0)
            yield return $"种子检查交易: {CompletedSeedChecks}";
        if (CompletedClones != 0)
            yield return $"克隆交易: {CompletedClones}";
        if (CompletedDumps != 0)
            yield return $"导出交易: {CompletedDumps}";
        if (CompletedTrades != 0)
            yield return $"链接交易: {CompletedTrades}";
        if (CompletedDistribution != 0)
            yield return $"分发交易: {CompletedDistribution}";
        if (CompletedSurprise != 0)
            yield return $"惊喜交易: {CompletedSurprise}";
    }
}
