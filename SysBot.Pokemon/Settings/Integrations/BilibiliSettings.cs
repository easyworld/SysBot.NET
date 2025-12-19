using System;
using System.ComponentModel;

namespace SysBot.Pokemon;

public class BilibiliSettings
{
    private const string Startup = nameof(Startup);

    public override string ToString() => "Bilibili集成设置";

    // Startup

    [Category(Startup), Description("B站彈幕姬日志目录")]
    public string LogUrl { get; set; } = string.Empty;

    [Category(Startup), Description("直播间ID")]
    public int RoomId { get; set; } = 0;
}
