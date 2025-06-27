using Kook;
using Kook.Commands;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

[Summary("Queues new Clone trades")]
public class CloneModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => KookBot<T>.Runner.Hub.Queues.Info;

    [Command("clone")]
    [Alias("c")]
    [Summary("Clones the Pokémon you show via Link Trade.")]
    [RequireQueueRole(nameof(KookManager.RolesClone))]
    public Task CloneAsync(int code)
    {
        var sig = Context.User.GetFavor();
        return QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.Clone, PokeTradeType.Clone);
    }

    [Command("clone")]
    [Alias("c")]
    [Summary("Clones the Pokémon you show via Link Trade.")]
    [RequireQueueRole(nameof(KookManager.RolesClone))]
    public Task CloneAsync([Summary("Trade Code")][Remainder] string code)
    {
        int tradeCode = Util.ToInt32(code);
        var sig = Context.User.GetFavor();
        return QueueHelper<T>.AddToQueueAsync(Context, tradeCode == 0 ? Info.GetRandomTradeCode() : tradeCode, Context.User.Username, sig, new T(), PokeRoutineType.Clone, PokeTradeType.Clone);
    }

    [Command("clone")]
    [Alias("c")]
    [Summary("Clones the Pokémon you show via Link Trade.")]
    [RequireQueueRole(nameof(KookManager.RolesClone))]
    public Task CloneAsync()
    {
        var code = Info.GetRandomTradeCode();
        return CloneAsync(code);
    }

    [Command("cloneList")]
    [Alias("cl", "cq")]
    [Summary("Prints the users in the Clone queue.")]
    [RequireSudo]
    public async Task GetListAsync()
    {
        string msg = Info.GetTradeList(PokeRoutineType.Clone);
        
        var card = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText("These are the users who are currently waiting:"))
            .AddModule(new SectionModuleBuilder().WithText("Pending Trades"))
            .AddModule(new SectionModuleBuilder().WithText(msg))
            .Build();
        await ReplyCardAsync(card).ConfigureAwait(false);
    }
}
