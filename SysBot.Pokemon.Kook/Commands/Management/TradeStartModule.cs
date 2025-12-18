using Kook;
using Kook.Commands;
using Kook.WebSocket;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class TradeStartModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private class TradeStartAction(ulong ChannelId, Action<PokeRoutineExecutorBase, PokeTradeDetail<T>> messager, string channel)
        : ChannelAction<PokeRoutineExecutorBase, PokeTradeDetail<T>>(ChannelId, messager, channel);

    private static readonly Dictionary<ulong, TradeStartAction> Channels = [];

    private static void Remove(TradeStartAction entry)
    {
        Channels.Remove(entry.ChannelID);
        KookBot<T>.Runner.Hub.Queues.Forwarders.Remove(entry.Action);
    }

#pragma warning disable RCS1158 // Static member in generic type should use a type parameter.
    public static void RestoreTradeStarting(KookSocketClient Kook)
    {
        var cfg = KookBotSettings.Settings;
        foreach (var ch in cfg.TradeStartingChannels)
        {
            if (Kook.GetChannel(ch.ID) is ISocketMessageChannel c)
                AddLogChannel(c, ch.ID);
        }

        LogUtil.LogInfo("机器人启动时已将交易开始通知添加到Kook频道。", "Kook");
    }

    public static bool IsStartChannel(ulong cid)
#pragma warning restore RCS1158 // Static member in generic type should use a type parameter.
    {
        return Channels.TryGetValue(cid, out _);
    }

    [Command("startHere")]
    [Summary("让机器人在此频道记录交易开始信息。")]
    [RequireSudo]
    public async Task AddLogAsync()
    {
        var c = Context.Channel;
        var cid = c.Id;
        if (Channels.TryGetValue(cid, out _))
        {
            await ReplyTextAsync("已经在此记录了。").ConfigureAwait(false);
            return;
        }

        AddLogChannel(c, cid);

        // Add to Kook global loggers (saves on program close)
        KookBotSettings.Settings.TradeStartingChannels.AddIfNew([GetReference(Context.Channel)]);
        await ReplyTextAsync("已将交易开始通知输出添加到此频道！").ConfigureAwait(false);
    }

    private static void AddLogChannel(ISocketMessageChannel c, ulong cid)
    {
        void Logger(PokeRoutineExecutorBase bot, PokeTradeDetail<T> detail)
        {
            if (detail.Type == PokeTradeType.Random)
                return;
            c.SendTextAsync(GetMessage(bot, detail));
        }

        Action<PokeRoutineExecutorBase, PokeTradeDetail<T>> l = Logger;
        KookBot<T>.Runner.Hub.Queues.Forwarders.Add(l);
        static string GetMessage(PokeRoutineExecutorBase bot, PokeTradeDetail<T> detail) => $"> [{DateTime.Now:hh:mm:ss}] - {bot.Connection.Label} 正在与 (ID {detail.ID}) {detail.Trainer.TrainerName} 进行交易";

        var entry = new TradeStartAction(cid, l, c.Name);
        Channels.Add(cid, entry);
    }

    [Command("startInfo")]
    [Summary("显示交易开始通知的设置。")]
    [RequireSudo]
    public async Task DumpLogInfoAsync()
    {
        foreach (var c in Channels)
            await ReplyTextAsync($"{c.Key} - {c.Value}").ConfigureAwait(false);
    }

    [Command("startClear")]
    [Summary("清除该特定频道的交易开始通知设置。")]
    [RequireSudo]
    public async Task ClearLogsAsync()
    {
        var cfg = KookBotSettings.Settings;
        if (Channels.TryGetValue(Context.Channel.Id, out var entry))
            Remove(entry);
        cfg.TradeStartingChannels.RemoveAll(z => z.ID == Context.Channel.Id);
        await ReplyTextAsync($"已从频道 {Context.Channel.Name} 清除交易开始通知！").ConfigureAwait(false);
    }

    [Command("startClearAll")]
    [Summary("清除所有交易开始通知设置。")]
    [RequireSudo]
    public async Task ClearLogsAllAsync()
    {
        foreach (var l in Channels)
        {
            var entry = l.Value;
            await ReplyTextAsync($"已从频道 {entry.ChannelName} ({entry.ChannelID}) 清除记录！").ConfigureAwait(false);
            KookBot<T>.Runner.Hub.Queues.Forwarders.Remove(entry.Action);
        }
        Channels.Clear();
        KookBotSettings.Settings.TradeStartingChannels.Clear();
        await ReplyTextAsync("已从所有频道清除交易开始通知！").ConfigureAwait(false);
    }

    private RemoteControlAccess GetReference(IChannel channel) => new()
    {
        ID = channel.Id,
        Name = channel.Name,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };
}
