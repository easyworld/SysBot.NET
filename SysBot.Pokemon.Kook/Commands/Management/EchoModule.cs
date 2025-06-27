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

        EchoUtil.Echo("Added echo notification to Kook channel(s) on Bot startup.");
    }

    [Command("echoHere")]
    [Summary("Makes the echo special messages to the channel.")]
    [RequireSudo]
    public async Task AddEchoAsync()
    {
        var c = Context.Channel;
        var cid = c.Id;
        if (Channels.TryGetValue(cid, out _))
        {
            await ReplyTextAsync("Already notifying here.").ConfigureAwait(false);
            return;
        }

        AddEchoChannel(c, cid);

        // Add to Kook global loggers (saves on program close)
        KookBotSettings.Settings.EchoChannels.AddIfNew([GetReference(Context.Channel)]);
        await ReplyTextAsync("Added Echo output to this channel!").ConfigureAwait(false);
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
    [Summary("Dumps the special message (Echo) settings.")]
    [RequireSudo]
    public async Task DumpEchoInfoAsync()
    {
        foreach (var c in Channels)
            await ReplyTextAsync($"{c.Key} - {c.Value}").ConfigureAwait(false);
    }

    [Command("echoClear")]
    [Summary("Clears the special message echo settings in that specific channel.")]
    [RequireSudo]
    public async Task ClearEchosAsync()
    {
        var id = Context.Channel.Id;
        if (!Channels.TryGetValue(id, out var echo))
        {
            await ReplyTextAsync("Not echoing in this channel.").ConfigureAwait(false);
            return;
        }
        EchoUtil.Forwarders.Remove(echo.Action);
        Channels.Remove(Context.Channel.Id);
        KookBotSettings.Settings.EchoChannels.RemoveAll(z => z.ID == id);
        await ReplyTextAsync($"Echoes cleared from channel: {Context.Channel.Name}").ConfigureAwait(false);
    }

    [Command("echoClearAll")]
    [Summary("Clears all the special message Echo channel settings.")]
    [RequireSudo]
    public async Task ClearEchosAllAsync()
    {
        foreach (var l in Channels)
        {
            var entry = l.Value;
            await ReplyTextAsync($"Echoing cleared from {entry.ChannelName} ({entry.ChannelID}!").ConfigureAwait(false);
            EchoUtil.Forwarders.Remove(entry.Action);
        }
        EchoUtil.Forwarders.RemoveAll(y => Channels.Select(x => x.Value.Action).Contains(y));
        Channels.Clear();
        KookBotSettings.Settings.EchoChannels.Clear();
        await ReplyTextAsync("Echoes cleared from all channels!").ConfigureAwait(false);
    }

    private RemoteControlAccess GetReference(IChannel channel) => new()
    {
        ID = channel.Id,
        Name = channel.Name,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };
}
