using Kook;
using Kook.Commands;
using Kook.WebSocket;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

[Summary("添加新的链接交换交易到队列")]
public class TradeModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => KookBot<T>.Runner.Hub.Queues.Info;

    [Command("tradeList")]
    [Alias("tl")]
    [Summary("显示交易队列中的用户列表。")]
    [RequireSudo]
    public async Task GetTradeListAsync()
    {
        string msg = Info.GetTradeList(PokeRoutineType.LinkTrade);
        
        var card = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText("以下是当前正在等待的用户："))
            .AddModule(new SectionModuleBuilder().WithText("待处理的交易"))
            .AddModule(new SectionModuleBuilder().WithText(msg))
            .Build();
        await ReplyCardAsync(card).ConfigureAwait(false);
    }

    [Command("trade")]
    [Alias("t")]
    [Summary("让机器人与您交换提供的宝可梦文件。")]
    [RequireQueueRole(nameof(KookManager.RolesTrade))]
    public Task TradeAsyncAttach([Summary("交换代码")] int code)
    {
        var sig = Context.User.GetFavor();
        return TradeAsyncAttach(code, sig, Context.User);
    }

    [Command("trade")]
    [Alias("t")]
    [Summary("让机器人与您交换从提供的Showdown Set生成的宝可梦。")]
    [RequireQueueRole(nameof(KookManager.RolesTrade))]
    public async Task TradeAsync([Summary("交换代码")] int code, [Summary("Showdown Set")][Remainder] string content)
    {
        if (ShowdownTranslator<T>.GameStringsZh.Species.Skip(1).Any(s => content.Contains(s)))
        {
            // If the content contains a Chinese Showdown Set, translate it to English.
            content = ShowdownTranslator<T>.Chinese2Showdown(content);
        }
        else
        {
            content = ReusableActions.StripCodeBlock(content);
        }

        var set = new ShowdownSet(content);
        var template = AutoLegalityWrapper.GetTemplate(set);
        if (set.InvalidLines.Count != 0 || set.Species is 0)
            {
                var sb = new StringBuilder(128);
                sb.AppendLine("无法解析Showdown Set。");
                var invalidlines = set.InvalidLines;
                if (invalidlines.Count != 0)
                {
                    var localization = BattleTemplateParseErrorLocalization.Get();
                    sb.AppendLine("检测到无效行：\n```");
                    foreach (var line in invalidlines)
                    {
                        var error = line.Humanize(localization);
                        sb.AppendLine(error);
                    }
                    sb.AppendLine("```");
                }
                if (set.Species is 0)
                    sb.AppendLine("无法识别宝可梦物种。请检查拼写。");

                var msg = sb.ToString();
                await ReplyTextAsync(msg).ConfigureAwait(false);
                return;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);
                var la = new LegalityAnalysis(pkm);
                var spec = GameInfo.Strings.Species[template.Species];
                pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
                if (pkm is not T pk || !la.Valid)
                {
                    var reason = result switch
                    {
                        "Timeout" => $"生成{spec}的配置耗时过长。",
                        "VersionMismatch" => "请求被拒绝：PKHeX和Auto-Legality Mod版本不匹配。",
                        _ => $"无法从该配置创建{spec}。",
                    };
                var imsg = $"哎呀！{reason}";
                if (result == "Failed")
                    imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm)}";
                await ReplyTextAsync(imsg).ConfigureAwait(false);
                return;
            }
            pk.ResetPartyStats();

            var sig = Context.User.GetFavor();
            await AddTradeToQueueAsync(code, Context.User.Username, pk, sig, Context.User).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(TradeModule<T>));
            var msg = $"哎呀！此Showdown Set出现了意外问题：\n```\n{string.Join("\n", set.GetSetLines())}```";
            await ReplyTextAsync(msg).ConfigureAwait(false);
        }
    }

    [Command("trade")]
    [Alias("t")]
    [Summary("让机器人与您交换从提供的Showdown Set生成的宝可梦。")]
    [RequireQueueRole(nameof(KookManager.RolesTrade))]
    public Task TradeAsync([Summary("Showdown Set")][Remainder] string content)
    {
        var code = Info.GetRandomTradeCode();
        return TradeAsync(code, content);
    }

    [Command("trade")]
    [Alias("t")]
    [Summary("让机器人与您交换提供的宝可梦文件。")]
    [RequireQueueRole(nameof(KookManager.RolesTrade))]
    public Task TradeAsyncAttach(Uri file)
    {
        var code = Info.GetRandomTradeCode();
        return TradeAsyncAttach(code);
    }

    [Command("banTrade")]
    [Alias("bt")]
    [RequireSudo]
    public async Task BanTradeAsync([Summary("在线ID")] ulong nnid, string comment)
    {
        KookBotSettings.HubConfig.TradeAbuse.BannedIDs.AddIfNew([GetReference(nnid, comment)]);
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    private RemoteControlAccess GetReference(ulong id, string comment) => new()
    {
        ID = id,
        Name = id.ToString(),
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss} ({comment})",
    };

    [Command("tradeUser")]
    [Alias("tu", "tradeOther")]
    [Summary("让机器人与被提及的用户交换提供的宝可梦文件。")]
    [RequireSudo]
    public async Task TradeAsyncAttachUser([Summary("交换代码")] int code, [Remainder] string _)
    {
        if (Context.Message.MentionedUsers.Count > 1)
        {
            await ReplyTextAsync("提及的用户过多。请一次只排队一个用户。").ConfigureAwait(false);
            return;
        }

        if (Context.Message.MentionedUsers.Count == 0)
        {
            await ReplyTextAsync("必须提及一个用户才能执行此操作。").ConfigureAwait(false);
            return;
        }

        var usr = Context.Message.MentionedUsers.ElementAt(0);
        var sig = usr.GetFavor();
        await TradeAsyncAttach(code, sig, usr).ConfigureAwait(false);
    }

    [Command("tradeUser")]
    [Alias("tu", "tradeOther")]
    [Summary("让机器人与被提及的用户交换提供的宝可梦文件。")]
    [RequireSudo]
    public Task TradeAsyncAttachUser([Remainder] string _)
    {
        var code = Info.GetRandomTradeCode();
        return TradeAsyncAttachUser(code, _);
    }

    private async Task TradeAsyncAttach(int code, RequestSignificance sig, SocketUser usr)
    {
        var attachment = Context.Message.Attachments.FirstOrDefault();
        if (attachment == default)
        {
            await ReplyTextAsync("未提供附件！").ConfigureAwait(false);
            return;
        }

        var att = await NetUtil.DownloadPKMAsync(attachment).ConfigureAwait(false);
        var pk = GetRequest(att);
        if (pk == null)
        {
            await ReplyTextAsync("提供的附件与此模块不兼容！").ConfigureAwait(false);
            return;
        }

        await AddTradeToQueueAsync(code, usr.Username, pk, sig, usr).ConfigureAwait(false);
    }

    private static T? GetRequest(Download<PKM> dl)
    {
        if (!dl.Success)
            return null;
        return dl.Data switch
        {
            null => null,
            T pk => pk,
            _ => EntityConverter.ConvertToType(dl.Data, typeof(T), out _) as T,
        };
    }

    private async Task AddTradeToQueueAsync(int code, string trainerName, T pk, RequestSignificance sig, SocketUser usr)
    {
        var la = new LegalityAnalysis(pk);
        if (!la.Valid)
        {
            // Disallow trading illegal Pokémon.
            await ReplyTextAsync($"{typeof(T).Name}附件不合法，无法交易！").ConfigureAwait(false);
            return;
        }
        var enc = la.EncounterOriginal;
        if (!pk.CanBeTraded(enc))
        {
            // Disallow anything that cannot be traded from the game (e.g. Fusions).
            await ReplyTextAsync("提供的宝可梦内容被禁止交易！").ConfigureAwait(false);
            return;
        }
        var cfg = Info.Hub.Config.Trade;
        if (cfg.DisallowNonNatives && (enc.Context != pk.Context || pk.GO))
        {
            // Allow the owner to prevent trading entities that require a HOME Tracker even if the file has one already.
            await ReplyTextAsync($"{typeof(T).Name}附件不是原生的，无法交易！").ConfigureAwait(false);
            return;
        }
        if (cfg.DisallowTracked && pk is IHomeTrack { HasTracker: true })
        {
            // Allow the owner to prevent trading entities that already have a HOME Tracker.
            await ReplyTextAsync($"{typeof(T).Name}附件被HOME跟踪，无法交易！").ConfigureAwait(false);
            return;
        }

        await QueueHelper<T>.AddToQueueAsync(Context, code, trainerName, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific, usr).ConfigureAwait(false);
    }
}
