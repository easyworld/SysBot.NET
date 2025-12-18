using PKHeX.Core;
using SysBot.Base;
using System.ComponentModel;

namespace SysBot.Pokemon;

public class DistributionSettings : ISynchronizationSetting
{
    private const string Distribute = "分发设置";
    private const string Synchronize = "同步设置";
    public override string ToString() => "分发交易设置";

    // Distribute

    [Category(Distribute), Description("启用时，空闲的连接交易机器人将从DistributeFolder中随机分发PKM文件。")]
    public bool DistributeWhileIdle { get; set; } = true;

    [Category(Distribute), Description("启用时，分发文件夹将随机生成文件，而不是按相同顺序。")]
    public bool Shuffled { get; set; }

    [Category(Distribute), Description("当设置为None以外的值时，随机交易将要求除了昵称匹配外还需要该宝可梦种类。")]
    public Species LedySpecies { get; set; } = Species.None;

    [Category(Distribute), Description("当设置为true时，随机Ledy昵称交换交易将退出，而不是从池中交易随机实体。")]
    public bool LedyQuitIfNoMatch { get; set; }

    [Category(Distribute), Description("分发交易连接代码。")]
    public int TradeCode { get; set; } = 7196;

    [Category(Distribute), Description("分发交易连接代码使用最小和最大范围，而不是固定交易代码。")]
    public bool RandomCode { get; set; }

    [Category(Distribute), Description("对于BDSP，分发机器人将前往特定房间并留在那里，直到机器人停止。")]
    public bool RemainInUnionRoomBDSP { get; set; } = true;

    // Synchronize

    [Category(Synchronize), Description("连接交易：使用多个分发机器人——所有机器人将同时确认其交易代码。当为Local时，所有机器人到达屏障后将继续；当为Remote时，必须有其他信号指示机器人继续。")]
    public BotSyncOption SynchronizeBots { get; set; } = BotSyncOption.LocalSync;

    [Category(Synchronize), Description("连接交易：使用多个分发机器人——一旦所有机器人准备好确认交易代码，Hub将等待X毫秒后释放所有机器人。")]
    public int SynchronizeDelayBarrier { get; set; }

    [Category(Synchronize), Description("连接交易：使用多个分发机器人——机器人在继续之前等待同步的时间（秒）。")]
    public double SynchronizeTimeout { get; set; } = 90;
}
