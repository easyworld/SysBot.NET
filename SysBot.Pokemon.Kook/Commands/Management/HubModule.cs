using Kook;
using Kook.Commands;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class HubModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    [Command("status")]
    [Alias("stats")]
    [Summary("Gets the status of the bot environment.")]
    public async Task GetStatusAsync()
    {
        var me = KookBot<T>.Runner;
        var hub = me.Hub;

        var builder = new CardBuilder();

        var runner = KookBot<T>.Runner;
        var allBots = runner.Bots.ConvertAll(z => z.Bot);
        var botCount = allBots.Count;
        builder.AddModule(new SectionModuleBuilder().WithText("Bot Status"))
            .AddModule(new SectionModuleBuilder().WithText("**Summary**"))
            .AddModule(new SectionModuleBuilder().WithText($"Bot Count: {botCount}\n" +
                $"Bot State: {SummarizeBots(allBots)}\n" +
                $"Pool Count: {hub.Ledy.Pool.Count}\n"));

        var bots = allBots.OfType<ICountBot>();
        var lines = bots.SelectMany(z => z.Counts.GetNonZeroCounts()).Distinct();
        var msg = string.Join("\n", lines);
        if (string.IsNullOrWhiteSpace(msg))
            msg = "Nothing counted yet!";
        builder.AddModule(new SectionModuleBuilder().WithText("**Counts**"))
            .AddModule(new SectionModuleBuilder().WithText(msg));

        var queues = hub.Queues.AllQueues;
        int count = 0;
        foreach (var q in queues)
        {
            var c = q.Count;
            if (c == 0)
                continue;

            var nextMsg = GetNextName(q);
            builder.AddModule(new SectionModuleBuilder().WithText($"**{q.Type} Queue**"))
                .AddModule(new SectionModuleBuilder().WithText($"Next: {nextMsg}\n" +
                    $"Count: {c}"));
            count += c;
        }

        if (count == 0)
        {
            builder.AddModule(new SectionModuleBuilder().WithText("**Queues are empty.**"))
                .AddModule(new SectionModuleBuilder().WithText("Nobody in line!"));
        }

        await ReplyCardAsync(builder.Build()).ConfigureAwait(false);
    }

    private static string GetNextName(PokeTradeQueue<T> q)
    {
        var next = q.TryPeek(out var detail, out _);
        if (!next)
            return "None!";

        var name = detail.Trainer.TrainerName;

        // show detail of trade if possible
        var nick = detail.TradeData.Nickname;
        if (!string.IsNullOrEmpty(nick))
            name += $" - {nick}";
        return name;
    }

    private static string SummarizeBots(IReadOnlyCollection<RoutineExecutor<PokeBotState>> bots)
    {
        if (bots.Count == 0)
            return "No bots configured.";
        var summaries = bots.Select(z => $"- {z.GetSummary()}");
        return Environment.NewLine + string.Join(Environment.NewLine, summaries);
    }
}
