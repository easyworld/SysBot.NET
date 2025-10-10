using Kook;
using Kook.Commands;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

[Summary("Queues new Dump trades")]
public class DumpModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => KookBot<T>.Runner.Hub.Queues.Info;

    [Command("dump")]
    [Alias("d")]
    [Summary("Dumps the Pokémon you show via Link Trade.")]
    [RequireQueueRole(nameof(KookManager.RolesDump))]
    public Task DumpAsync(int code)
    {
        var sig = Context.User.GetFavor();
        return QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.Dump, PokeTradeType.Dump);
    }

    [Command("dump")]
    [Alias("d")]
    [Summary("Dumps the Pokémon you show via Link Trade.")]
    [RequireQueueRole(nameof(KookManager.RolesDump))]
    public Task DumpAsync([Summary("Trade Code")][Remainder] string code)
    {
        int tradeCode = Util.ToInt32(code);
        var sig = Context.User.GetFavor();
        return QueueHelper<T>.AddToQueueAsync(Context, tradeCode == 0 ? Info.GetRandomTradeCode() : tradeCode, Context.User.Username, sig, new T(), PokeRoutineType.Dump, PokeTradeType.Dump);
    }

    [Command("dump")]
    [Alias("d")]
    [Summary("Dumps the Pokémon you show via Link Trade.")]
    [RequireQueueRole(nameof(KookManager.RolesDump))]
    public Task DumpAsync()
    {
        var code = Info.GetRandomTradeCode();
        return DumpAsync(code);
    }

    [Command("dumpList")]
    [Alias("dl", "dq")]
    [Summary("Prints the users in the Dump queue.")]
    [RequireSudo]
    public async Task GetListAsync()
    {
        string msg = Info.GetTradeList(PokeRoutineType.Dump);

        var card = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText("These are the users who are currently waiting:"))
            .AddModule(new SectionModuleBuilder().WithText("Pending Trades"))
            .AddModule(new SectionModuleBuilder().WithText(msg))
            .Build();
        await ReplyCardAsync(card).ConfigureAwait(false);
    }
}
