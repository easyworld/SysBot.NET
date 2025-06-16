using Kook.Commands;
using PKHeX.Core;

namespace SysBot.Pokemon.Kook;

public class CommandModule<T> : IModule where T : PKM, new()
{
    public bool? IsEnable { get; set; } = true;

    public async Task ExecuteAsync(SocketCommandContext context)
    {
        var text = KookMessageUtil.SanitizeMessage(context.Message.Content);
        if (string.IsNullOrWhiteSpace(text)) return;
        if (text.Trim().StartsWith("取消"))
        {
            var result = KookBot<T>.Info.ClearTrade(context.User.Id);
            await context.Channel.SendTextAsync($"{context.User.KMarkdownMention} {GetClearTradeMessage(result)}");
        }
        else if (text.Trim().StartsWith("位置"))
        {
            var result = KookBot<T>.Info.CheckPosition(context.User.Id);
            await context.Channel.SendTextAsync($"{context.User.KMarkdownMention} {GetQueueCheckResultMessage(result)}");
        }
    }
    private static string GetQueueCheckResultMessage(QueueCheckResult<T> result)
    {
        if (!result.InQueue || result.Detail is null)
            return "你不在队列里";
        var msg = $"你在第{result.Position}位";
        var pk = result.Detail.Trade.TradeData;
        if (pk.Species != 0)
            msg += $"，交换宝可梦：{ShowdownTranslator<T>.GameStringsZh.Species[result.Detail.Trade.TradeData.Species]}";
        return msg;
    }

    private static string GetClearTradeMessage(QueueResultRemove result)
    {
        return result switch
        {
            QueueResultRemove.CurrentlyProcessing => "你正在交换中",
            QueueResultRemove.CurrentlyProcessingRemoved => "正在删除",
            QueueResultRemove.Removed => "已删除",
            _ => "你不在队列里",
        };
    }
}
