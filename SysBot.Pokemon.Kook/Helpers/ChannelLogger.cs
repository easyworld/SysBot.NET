using System;
using Kook.WebSocket;
using SysBot.Base;

namespace SysBot.Pokemon.Kook;

public class ChannelLogger(ulong ChannelID, ISocketMessageChannel Channel) : ILogForwarder
{
    public ulong ChannelID { get; } = ChannelID;
    public string ChannelName => Channel.Name;

    public void Forward(string message, string identity)
    {
        try
        {
            var text = GetMessage(message, identity);
            Channel.SendTextAsync(text);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, identity);
        }
    }
    private static string GetMessage(ReadOnlySpan<char> msg, string identity)
        => $"> [{DateTime.Now:hh:mm:ss}] - {identity}: {msg}";
}
