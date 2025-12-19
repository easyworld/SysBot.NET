using Kook;
using Kook.Commands;
using Kook.Net;
using Kook.WebSocket;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public static class QueueHelper<T> where T : PKM, new()
{
    private const uint MaxTradeCode = 9999_9999;

    public static async Task AddToQueueAsync(SocketCommandContext context, int code, string trainer, RequestSignificance sig, T trade, PokeRoutineType routine, PokeTradeType type, SocketUser trader)
    {
        if ((uint)code > MaxTradeCode)
        {
            await context.Channel.SendTextAsync("交换码应该在00000000-99999999之间！").ConfigureAwait(false);
            return;
        }

        try
        {
            const string helper = "我已将您添加到队列！当交易开始时，我会在此处发送消息通知您。";
            var test = await trader.SendTextAsync(helper).ConfigureAwait(false);

            // Try adding
            var result = AddToTradeQueue(context, trade, code, trainer, sig, routine, type, trader, out var msg);

            // Notify in channel
            await context.Channel.SendTextAsync(msg).ConfigureAwait(false);
            // Notify in PM to mirror what is said in the channel.
            await trader.SendTextAsync($"{msg}\nYour trade code will be {Format.Bold($"{code:0000 0000}")}.").ConfigureAwait(false);

            // Clean Up
            if (result)
            {
                // Delete the user's join message for privacy
                if (!context.IsPrivate)
                    await context.Message.DeleteAsync(RequestOptions.Default).ConfigureAwait(false);
            }
            else
            {
                // Delete our "I'm adding you!", and send the same message that we sent to the general channel.
                // KOOK.Net 中，无法删除私信消息，直接忽略此步骤
                // await test.DeleteAsync().ConfigureAwait(false);
            }
        }
        catch (HttpException ex)
        {
            await HandleKookExceptionAsync(context, trader, ex).ConfigureAwait(false);
        }
    }

    public static Task AddToQueueAsync(SocketCommandContext context, int code, string trainer, RequestSignificance sig, T trade, PokeRoutineType routine, PokeTradeType type)
    {
        return AddToQueueAsync(context, code, trainer, sig, trade, routine, type, context.User);
    }

    private static bool AddToTradeQueue(SocketCommandContext context, T pk, int code, string trainerName, RequestSignificance sig, PokeRoutineType type, PokeTradeType t, SocketUser trader, out string msg)
    {
        var user = trader;
        var userID = user.Id;
        var name = user.Username;

        var trainer = new PokeTradeTrainerInfo(trainerName, userID);
        var notifier = new KookTradeNotifier<T>(pk, trainer, code, user);
        var detail = new PokeTradeDetail<T>(pk, trainer, notifier, t, code, sig == RequestSignificance.Favored);
        var trade = new TradeEntry<T>(detail, userID, type, name);

        var hub = KookBot<T>.Runner.Hub;
        var Info = hub.Queues.Info;
        var added = Info.AddToTradeQueue(trade, userID, sig == RequestSignificance.Owner);

        if (added == QueueResultAdd.AlreadyInQueue)
        {
            msg = "抱歉，您已经在队列中了。";
            return false;
        }

        var position = Info.CheckPosition(userID, type);

        var ticketID = "";
        if (TradeStartModule<T>.IsStartChannel(context.Channel.Id))
            ticketID = $", unique ID: {detail.ID}";

        var pokeName = "";
        if (t == PokeTradeType.Specific && pk.Species != 0)
            pokeName = $" 接收: {GameInfo.GetStrings("zh-Hans").Species[pk.Species]}.";
        msg = $"{user.KMarkdownMention} - 已添加到 {type} 队列{ticketID}。当前位置: {position.Position}.{pokeName}";

        var botct = Info.Hub.Bots.Count;
        if (position.Position > botct)
        {
            var eta = Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
            msg += $" 预计等待时间: {eta:F1} 分钟。";
        }
        return true;
    }

    private static async Task HandleKookExceptionAsync(SocketCommandContext context, SocketUser trader, HttpException ex)
    {
        string message = string.Empty;
        switch (ex.KookCode)
        {
            case KookErrorCode.MissingPermissions:
            {
                // Check if the exception was raised due to missing "Send Messages" or "Manage Messages" permissions. Nag the bot owner if so.
                var permissions = context.Guild.CurrentUser.GetPermissions(context.Channel as IGuildChannel);
                if (!permissions.SendMessages)
                {
                    // Nag the owner in logs.
                    message = "You must grant me \"Send Messages\" permissions!";
                    Base.LogUtil.LogError(message, "QueueHelper");
                    return;
                }
                if (!permissions.ManageMessages)
                {
                    //var app = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
                    //var owner = app.Owner.Id;
                    var owner = KookBotSettings.Manager.Owner;
                    message = $"{MentionUtils.KMarkdownMentionUser(owner)} You must grant me \"Manage Messages\" permissions!";
                }
            }
                break;
            //case KookErrorCode.CannotSendMessageToUser:
            //{
            //    // The user either has DMs turned off, or Kook thinks they do.
            //    message = context.User == trader ? "You must enable private messages in order to be queued!" : "The mentioned user must enable private messages in order for them to be queued!";
            //}
            //    break;
            default:
            {
                // Send a generic error message.
                message = ex.KookCode != null ? $"Kook error {(int)ex.KookCode}: {ex.Reason}" : $"Http error {(int)ex.HttpCode}: {ex.Message}";
            }
                break;
        }
        await context.Channel.SendTextAsync(message).ConfigureAwait(false);
    }
}
