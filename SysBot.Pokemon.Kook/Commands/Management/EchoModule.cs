using Kook;
using Kook.Commands;
using Kook.WebSocket;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class EchoModule : ModuleBase<SocketCommandContext>
{
    private class EchoChannel(ulong ChannelId, string ChannelName, Action<string> Action)
    {
        public readonly ulong ChannelID = ChannelId;
        public readonly string ChannelName = ChannelName;
        public readonly Action<string> Action = Action;
    }

    private static readonly Dictionary<ulong, EchoChannel> Channels = [];

    public static void RestoreChannels(KookSocketClient Kook, KookSettings cfg)
    {
        foreach (var ch in cfg.EchoChannels)
        {
            if (Kook.GetChannel(ch.ID) is ISocketMessageChannel c)
                AddEchoChannel(c, ch.ID);
        }

        EchoUtil.Echo("机器人启动时已将回声通知添加到Kook频道。");
    }

    [Command("echoHere")]
    [Summary("让机器人将特殊消息回声到该频道。")]
    [RequireSudo]
    public async Task AddEchoAsync()
    {
        var c = Context.Channel;
        var cid = c.Id;
        if (Channels.TryGetValue(cid, out _))
        {
            await ReplyTextAsync("已经在此频道通知了。").ConfigureAwait(false);
            return;
        }

        AddEchoChannel(c, cid);

        // Add to Kook global loggers (saves on program close)
        KookBotSettings.Settings.EchoChannels.AddIfNew([GetReference(Context.Channel)]);
        await ReplyTextAsync("已将回声输出添加到此频道！").ConfigureAwait(false);
    }

    private static void AddEchoChannel(ISocketMessageChannel c, ulong cid)
    {
        void Echo(string msg) => c.SendTextAsync(msg);

        Action<string> l = Echo;
        EchoUtil.Forwarders.Add(l);
        var entry = new EchoChannel(cid, c.Name, l);
        Channels.Add(cid, entry);
    }

    public static bool IsEchoChannel(ISocketMessageChannel c)
    {
        var cid = c.Id;
        return Channels.TryGetValue(cid, out _);
    }

    [Command("echoInfo")]
    [Summary("显示特殊消息（回声）的设置。")]
    [RequireSudo]
    public async Task DumpEchoInfoAsync()
    {
        foreach (var c in Channels)
            await ReplyTextAsync($"{c.Key} - {c.Value}").ConfigureAwait(false);
    }

    [Command("echoClear")]
    [Summary("清除该特定频道的特殊消息回声设置。")]
    [RequireSudo]
    public async Task ClearEchosAsync()
    {
        var id = Context.Channel.Id;
        if (!Channels.TryGetValue(id, out var echo))
        {
            await ReplyTextAsync("未在此频道回声。").ConfigureAwait(false);
            return;
        }
        EchoUtil.Forwarders.Remove(echo.Action);
        Channels.Remove(Context.Channel.Id);
        KookBotSettings.Settings.EchoChannels.RemoveAll(z => z.ID == id);
        await ReplyTextAsync($"已从频道 {Context.Channel.Name} 清除回声！").ConfigureAwait(false);
    }

    [Command("echoClearAll")]
    [Summary("清除所有特殊消息回声频道设置。")]
    [RequireSudo]
    public async Task ClearEchosAllAsync()
    {
        foreach (var l in Channels)
        {
            var entry = l.Value;
            await ReplyTextAsync($"已从频道 {entry.ChannelName} ({entry.ChannelID}) 清除回声！").ConfigureAwait(false);
            EchoUtil.Forwarders.Remove(entry.Action);
        }
        EchoUtil.Forwarders.RemoveAll(y => Channels.Select(x => x.Value.Action).Contains(y));
        Channels.Clear();
        KookBotSettings.Settings.EchoChannels.Clear();
        await ReplyTextAsync("已从所有频道清除回声！").ConfigureAwait(false);
    }

    private RemoteControlAccess GetReference(IChannel channel) => new()
    {
        ID = channel.Id,
        Name = channel.Name,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };
}
