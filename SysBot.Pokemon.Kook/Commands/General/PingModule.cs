using Kook.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class PingModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Summary("让机器人响应，表明它正在运行。")]
    public async Task PingAsync()
    {
        await ReplyTextAsync("乒！").ConfigureAwait(false);
    }
}
