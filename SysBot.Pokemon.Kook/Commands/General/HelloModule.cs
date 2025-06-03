using Kook;
using Kook.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class HelloModule : ModuleBase<SocketCommandContext>
{
    [Command("hello")]
    [Alias("hi")]
    [Summary("Say hello to the bot and get a response.")]
    public async Task PingAsync()
    {
        var str = KookBotSettings.Settings.HelloResponse;
        var msg = string.Format(str, $"{Context.User.KMarkdownMention}");
        await ReplyTextAsync(msg).ConfigureAwait(false);
    }
}
