using Kook;
using Kook.Commands;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

[Summary("清除和切换队列功能。")]
public class QueueModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => KookBot<T>.Runner.Hub.Queues.Info;

    [Command("queueStatus")]
    [Alias("qs", "ts")]
    [Summary("查看用户在队列中的位置。")]
    public async Task GetTradePositionAsync()
    {
        var msg = $"{Context.User.KMarkdownMention}" + " - " + Info.GetPositionString(Context.User.Id);
        await ReplyTextAsync(msg).ConfigureAwait(false);
    }

    [Command("queueClear")]
    [Alias("qc", "tc")]
    [Summary("将用户从交易队列中清除。如果用户正在被处理，将不会被移除。")]
    public async Task ClearTradeAsync()
    {
        string msg = ClearTrade();
        await ReplyTextAsync(msg).ConfigureAwait(false);
    }

    [Command("queueClearUser")]
    [Alias("qcu", "tcu")]
    [Summary("将指定用户从交易队列中清除。如果用户正在被处理，将不会被移除。")]
    [RequireSudo]
    public async Task ClearTradeUserAsync([Summary("Kook用户ID")] ulong id)
    {
        string msg = ClearTrade(id);
        await ReplyTextAsync(msg).ConfigureAwait(false);
    }

    [Command("queueClearUser")]
    [Alias("qcu", "tcu")]
    [Summary("将提及的用户从交易队列中清除。如果用户正在被处理，将不会被移除。")]
    [RequireSudo]
    public async Task ClearTradeUserAsync([Summary("要清除的用户名")] string _)
    {
        foreach (var user in Context.Message.MentionedUsers)
        {
            string msg = ClearTrade(user.Id);
            await ReplyTextAsync(msg).ConfigureAwait(false);
        }
    }

    [Command("queueClearUser")]
    [Alias("qcu", "tcu")]
    [Summary("将提及的用户从交易队列中清除。如果用户正在被处理，将不会被移除。")]
    [RequireSudo]
    public async Task ClearTradeUserAsync()
    {
        var users = Context.Message.MentionedUsers;
        if (users.Count == 0)
        {
            await ReplyTextAsync("没有提及任何用户").ConfigureAwait(false);
            return;
        }
        foreach (var u in users)
            await ClearTradeUserAsync(u.Id).ConfigureAwait(false);
    }

    [Command("queueClearAll")]
    [Alias("qca", "tca")]
    [Summary("清除所有用户的交易队列。")]
    [RequireSudo]
    public async Task ClearAllTradesAsync()
    {
        Info.ClearAllQueues();
        await ReplyTextAsync("已清除队列中的所有用户。").ConfigureAwait(false);
    }

    [Command("queueToggle")]
    [Alias("qt", "tt")]
    [Summary("切换是否允许用户加入交易队列。")]
    [RequireSudo]
    public Task ToggleQueueTradeAsync()
    {
        var state = Info.ToggleQueue();
        var msg = state
            ? "现在用户可以加入交易队列。"
            : $"队列设置已更改：{Format.Bold($"在重新开启之前，用户无法加入队列。")}";

        return Context.Channel.EchoAndReply(msg);
    }

    [Command("queueMode")]
    [Alias("qm")]
    [Summary("更改队列控制方式（手动/阈值/间隔）。")]
    [RequireSudo]
    public async Task ChangeQueueModeAsync([Summary("队列模式")] QueueOpening mode)
    {
        KookBot<T>.Runner.Hub.Config.Queues.QueueToggleMode = mode;
        await ReplyTextAsync($"已将队列模式更改为 {mode}。").ConfigureAwait(false);
    }

    [Command("queueList")]
    [Alias("ql")]
    [Summary("私信发送队列中的用户列表。")]
    [RequireSudo]
    public async Task ListUserQueue()
    {
        var lines = KookBot<T>.Runner.Hub.Queues.Info.GetUserList("(ID {0}) - 代码: {1} - {2} - {3}");
        var msg = string.Join("\n", lines);
        if (msg.Length < 3)
            await ReplyTextAsync("队列为空。").ConfigureAwait(false);
        else
            await Context.User.SendTextAsync(msg).ConfigureAwait(false);
    }

    private string ClearTrade()
    {
        var userID = Context.User.Id;
        return ClearTrade(userID);
    }

    //private static string ClearTrade(string username)
    //{
    //    var result = Info.ClearTrade(username);
    //    return GetClearTradeMessage(result);
    //}

    private static string ClearTrade(ulong userID)
    {
        var result = Info.ClearTrade(userID);
        return GetClearTradeMessage(result);
    }

    private static string GetClearTradeMessage(QueueResultRemove result)
    {
        return result switch
        {
            QueueResultRemove.CurrentlyProcessing => "看起来您正在被处理中！没有从所有队列中移除。",
            QueueResultRemove.CurrentlyProcessingRemoved => "看起来您正在被处理中！",
            QueueResultRemove.Removed => "已将您从队列中移除。",
            _ => "抱歉，您当前不在队列中。",
        };
    }
}
