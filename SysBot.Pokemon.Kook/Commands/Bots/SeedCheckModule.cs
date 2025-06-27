using Kook;
using Kook.Commands;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

[Summary("Queues new Seed Check trades")]
public class SeedCheckModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => KookBot<T>.Runner.Hub.Queues.Info;

    [Command("seedCheck")]
    [Alias("checkMySeed", "checkSeed", "seed", "s", "sc")]
    [Summary("Checks the seed for a Pokémon.")]
    [RequireQueueRole(nameof(KookManager.RolesSeed))]
    public Task SeedCheckAsync(int code)
    {
        var sig = Context.User.GetFavor();
        return QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.SeedCheck, PokeTradeType.Seed);
    }

    [Command("seedCheck")]
    [Alias("checkMySeed", "checkSeed", "seed", "s", "sc")]
    [Summary("Checks the seed for a Pokémon.")]
    [RequireQueueRole(nameof(KookManager.RolesSeed))]
    public Task SeedCheckAsync([Summary("Trade Code")][Remainder] string code)
    {
        int tradeCode = Util.ToInt32(code);
        var sig = Context.User.GetFavor();
        return QueueHelper<T>.AddToQueueAsync(Context, tradeCode == 0 ? Info.GetRandomTradeCode() : tradeCode, Context.User.Username, sig, new T(), PokeRoutineType.SeedCheck, PokeTradeType.Seed);
    }

    [Command("seedCheck")]
    [Alias("checkMySeed", "checkSeed", "seed", "s", "sc")]
    [Summary("Checks the seed for a Pokémon.")]
    [RequireQueueRole(nameof(KookManager.RolesSeed))]
    public Task SeedCheckAsync()
    {
        var code = Info.GetRandomTradeCode();
        return SeedCheckAsync(code);
    }

    [Command("seedList")]
    [Alias("sl", "scq", "seedCheckQueue", "seedQueue", "seedList")]
    [Summary("Prints the users in the Seed Check queue.")]
    [RequireSudo]
    public async Task GetSeedListAsync()
    {
        string msg = Info.GetTradeList(PokeRoutineType.SeedCheck);
        
        var card = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText("These are the users who are currently waiting:"))
            .AddModule(new SectionModuleBuilder().WithText("Pending Trades"))
            .AddModule(new SectionModuleBuilder().WithText(msg))
            .Build();
        await ReplyCardAsync(card).ConfigureAwait(false);
    }

    [Command("findFrame")]
    [Alias("ff", "getFrameData")]
    [Summary("Prints the next shiny frame from the provided seed.")]
    public async Task FindFrameAsync([Remainder] string seedString)
    {
        var me = KookBot<T>.Runner;
        var hub = me.Hub;

        seedString = seedString.ToLower();
        if (seedString.StartsWith("0x"))
            seedString = seedString[2..];

        var seed = Util.GetHexValue64(seedString);

        var r = new SeedSearchResult(Z3SearchResult.Success, seed, -1, hub.Config.SeedCheckSWSH.ResultDisplayMode);
        var msg = r.ToString();

        var card = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText($"Here are the details for `{seed:X16}`:"))
            .AddModule(new SectionModuleBuilder().WithText($"Seed: {seed:X16}"))
            .AddModule(new SectionModuleBuilder().WithText(msg))
            .Build();
        await ReplyCardAsync(card).ConfigureAwait(false);
    }
}
