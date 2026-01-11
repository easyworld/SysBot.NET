using Kook.Commands;
using PKHeX.Core;
using SysBot.Pokemon.Helpers;

namespace SysBot.Pokemon.Kook;

public class KookTrade<T> : AbstractTrade<T> where T : PKM, new()
{
    private readonly SocketCommandContext Context = default!;

    public KookTrade(SocketCommandContext context)
    {
        Context = context;
        SetPokeTradeTrainerInfo(new PokeTradeTrainerInfo(Context.User.Username, Context.User.Id));
        SetTradeQueueInfo(KookBot<T>.Info);
    }
    public override IPokeTradeNotifier<T> GetPokeTradeNotifier(T pkm, int code)
    {
        return new KookTradeNotifier<T>(pkm, userInfo, code, Context);
    }
    public override void SendMessage(string message)
    {
        Context.Channel.SendTextAsync($"{Context.User.KMarkdownMention} {message}");
    }
}
