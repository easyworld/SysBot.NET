using Kook;
using Kook.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class HelloModule : ModuleBase<SocketCommandContext>
{
    [Command("hello")]
    [Alias("hi")]
    [Summary("向机器人打招呼并获得响应。")]
    public async Task PingAsync()
    {
        var str = KookBotSettings.Settings.HelloResponse;
        var msg = string.Format(str, $"{Context.User.KMarkdownMention}");
        await ReplyTextAsync(msg).ConfigureAwait(false);
    }
}
