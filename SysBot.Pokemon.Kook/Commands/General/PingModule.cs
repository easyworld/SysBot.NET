using Kook.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class PingModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Summary("Makes the bot respond, indicating that it is running.")]
    public async Task PingAsync()
    {
        await ReplyTextAsync("Pong!").ConfigureAwait(false);
    }
}
