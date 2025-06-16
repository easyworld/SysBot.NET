using System;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon;

public class KookSettings
{
    private const string Startup = nameof(Startup);
    private const string Operation = nameof(Operation);
    private const string Messages = nameof(Messages);
    public override string ToString() => "Kook Integration Settings";

    // Startup

    [Category(Startup), Description("Kook Bot Token")]
    public string Token { get; set; } = string.Empty;

    [Category(Startup), Description("Response Channel ID")]
    public ulong ChannelId { get; set; } = 0;

    [Category(Startup), Description("Message to test Bot alive")]
    public string AliveMsg { get; set; } = "hello";

    [Category(Operation), Description("Message sent when the Barrier is released.")]
    public string MessageStart { get; set; } = string.Empty;
}
