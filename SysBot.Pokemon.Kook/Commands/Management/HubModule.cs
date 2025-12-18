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
    [Summary("获取机器人环境状态。")]
    public async Task GetStatusAsync()
    {
        var me = KookBot<T>.Runner;
        var hub = me.Hub;

        var builder = new CardBuilder();

        var runner = KookBot<T>.Runner;
        var allBots = runner.Bots.ConvertAll(z => z.Bot);
        var botCount = allBots.Count;
        builder.AddModule(new SectionModuleBuilder().WithText("机器人状态"))
            .AddModule(new SectionModuleBuilder().WithText(new KMarkdownElementBuilder("**摘要**")))
            .AddModule(new SectionModuleBuilder().WithText($"机器人数量: {botCount}\n" +
                $"机器人状态: {SummarizeBots(allBots)}\n" +
                $"池数量: {hub.Ledy.Pool.Count}\n"));

        var bots = allBots.OfType<ICountBot>();
        var lines = bots.SelectMany(z => z.Counts.GetNonZeroCounts()).Distinct();
        var msg = string.Join("\n", lines);
        if (string.IsNullOrWhiteSpace(msg))
            msg = "还没有统计到任何东西！";
        builder.AddModule(new SectionModuleBuilder().WithText(new KMarkdownElementBuilder("**统计**")))
            .AddModule(new SectionModuleBuilder().WithText(msg));

        var queues = hub.Queues.AllQueues;
        int count = 0;
        foreach (var q in queues)
        {
            var c = q.Count;
            if (c == 0)
                continue;

            var nextMsg = GetNextName(q);
            builder.AddModule(new SectionModuleBuilder().WithText(new KMarkdownElementBuilder($"**{q.Type} 队列**")))
                .AddModule(new SectionModuleBuilder().WithText($"下一个: {nextMsg}\n" +
                    $"数量: {c}"));
            count += c;
        }

        if (count == 0)
        {
            builder.AddModule(new SectionModuleBuilder().WithText(new KMarkdownElementBuilder("**队列为空。**")))
                .AddModule(new SectionModuleBuilder().WithText("没有排队的人！"));
        }

        await ReplyCardAsync(builder.Build()).ConfigureAwait(false);
    }

    private static string GetNextName(PokeTradeQueue<T> q)
    {
        var next = q.TryPeek(out var detail, out _);
        if (!next)
            return "没有！";

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
            return "没有配置机器人。";
        var summaries = bots.Select(z => $"- {z.GetSummary()}");
        return Environment.NewLine + string.Join(Environment.NewLine, summaries);
    }
}
