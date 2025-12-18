using Kook;
using Kook.Commands;
using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class SudoModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    [Command("blacklist")]
    [Summary("将@提到的Kook用户加入黑名单。")]
    [RequireSudo]
    // ReSharper disable once UnusedParameter.Global
    public async Task BlackListUsers([Remainder] string _)
    {
        var users = Context.Message.MentionedUsers;
        var objects = users.Select(GetReference);
        KookBotSettings.Settings.UserBlacklist.AddIfNew(objects);
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("blacklistComment")]
    [Summary("为黑名单中的Kook用户ID添加备注。")]
    [RequireSudo]
    // ReSharper disable once UnusedParameter.Global
    public async Task BlackListUsers(ulong id, [Remainder] string comment)
    {
        var obj = KookBotSettings.Settings.UserBlacklist.List.Find(z => z.ID == id);
        if (obj is null)
        {
            await ReplyTextAsync($"找不到ID为 {id} 的用户。").ConfigureAwait(false);
            return;
        }

        var oldComment = obj.Comment;
        obj.Comment = comment;
        await ReplyTextAsync($"完成。已将现有备注 ({oldComment}) 更改为 ({comment})。").ConfigureAwait(false);
    }

    [Command("unblacklist")]
    [Summary("将@提到的Kook用户从黑名单移除。")]
    [RequireSudo]
    // ReSharper disable once UnusedParameter.Global
    public async Task UnBlackListUsers([Remainder] string _)
    {
        var users = Context.Message.MentionedUsers;
        var objects = users.Select(GetReference);
        KookBotSettings.Settings.UserBlacklist.RemoveAll(z => objects.Any(o => o.ID == z.ID));
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("blacklistId")]
    [Summary("将Kook用户ID加入黑名单。（当用户不在服务器中时很有用）")]
    [RequireSudo]
    public async Task BlackListIDs([Summary("Comma Separated Kook IDs")][Remainder] string content)
    {
        var IDs = GetIDs(content);
        var objects = IDs.Select(GetReference);
        KookBotSettings.Settings.UserBlacklist.AddIfNew(objects);
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("unBlacklistId")]
    [Summary("将Kook用户ID从黑名单移除。（当用户不在服务器中时很有用）")]
    [RequireSudo]
    public async Task UnBlackListIDs([Summary("Comma Separated Kook IDs")][Remainder] string content)
    {
        var IDs = GetIDs(content);
        KookBotSettings.Settings.UserBlacklist.RemoveAll(z => IDs.Any(o => o == z.ID));
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("blacklistSummary")]
    [Alias("printBlacklist", "blacklistPrint")]
    [Summary("显示黑名单中的Kook用户列表。")]
    [RequireSudo]
    public async Task PrintBlacklist()
    {
        var lines = KookBotSettings.Settings.UserBlacklist.Summarize();
        var msg = string.Join("\n", lines);
        await ReplyTextAsync(Format.Code(msg)).ConfigureAwait(false);
    }

    [Command("banID")]
    [Summary("封禁在线用户ID。")]
    [RequireSudo]
    public async Task BanOnlineIDs([Summary("Comma Separated Online IDs")][Remainder] string content)
    {
        var IDs = GetIDs(content);
        var objects = IDs.Select(GetReference);

        var me = KookBot<T>.Runner;
        var hub = me.Hub;
        hub.Config.TradeAbuse.BannedIDs.AddIfNew(objects);
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("bannedIDComment")]
    [Summary("为封禁的在线用户ID添加备注。")]
    [RequireSudo]
    public async Task BanOnlineIDs(ulong id, [Remainder] string comment)
    {
        var me = KookBot<T>.Runner;
        var hub = me.Hub;
        var obj = hub.Config.TradeAbuse.BannedIDs.List.Find(z => z.ID == id);
        if (obj is null)
        {
            await ReplyTextAsync($"找不到在线ID为 {id} 的用户。").ConfigureAwait(false);
            return;
        }

        var oldComment = obj.Comment;
        obj.Comment = comment;
        await ReplyTextAsync($"Done. Changed existing comment ({oldComment}) to ({comment}).").ConfigureAwait(false);
    }

    [Command("unbanID")]
    [Summary("Bans online user IDs.")]
    [RequireSudo]
    public async Task UnBanOnlineIDs([Summary("Comma Separated Online IDs")][Remainder] string content)
    {
        var IDs = GetIDs(content);
        var me = KookBot<T>.Runner;
        var hub = me.Hub;
        hub.Config.TradeAbuse.BannedIDs.RemoveAll(z => IDs.Any(o => o == z.ID));
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("bannedIDSummary")]
    [Alias("printBannedID", "bannedIDPrint")]
    [Summary("Prints the list of banned online IDs.")]
    [RequireSudo]
    public async Task PrintBannedOnlineIDs()
    {
        var me = KookBot<T>.Runner;
        var hub = me.Hub;
        var lines = hub.Config.TradeAbuse.BannedIDs.Summarize();
        var msg = string.Join("\n", lines);
        await ReplyTextAsync(Format.Code(msg)).ConfigureAwait(false);
    }

    [Command("forgetUser")]
    [Alias("forget")]
    [Summary("忘记之前遇到过的用户。")]
    [RequireSudo]
    public async Task ForgetPreviousUser([Summary("Comma Separated Online IDs")][Remainder] string content)
    {
        var IDs = GetIDs(content);
        foreach (var ID in IDs)
        {
            PokeRoutineExecutorBase.PreviousUsers.RemoveAllNID(ID);
            PokeRoutineExecutorBase.PreviousUsersDistribution.RemoveAllNID(ID);
        }
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("previousUserSummary")]
    [Alias("prevUsers")]
    [Summary("显示之前遇到过的用户列表。")]
    [RequireSudo]
    public async Task PrintPreviousUsers()
    {
        bool found = false;
        var lines = PokeRoutineExecutorBase.PreviousUsers.Summarize().ToList();
        if (lines.Count != 0)
        {
            found = true;
            var msg = "之前的用户:\n" + string.Join("\n", lines);
            await ReplyTextAsync(Format.Code(msg)).ConfigureAwait(false);
        }

        lines = PokeRoutineExecutorBase.PreviousUsersDistribution.Summarize().ToList();
        if (lines.Count != 0)
        {
            found = true;
            var msg = "之前的分发用户:\n" + string.Join("\n", lines);
            await ReplyTextAsync(Format.Code(msg)).ConfigureAwait(false);
        }
        if (!found)
            await ReplyTextAsync("未找到之前的用户。").ConfigureAwait(false);
    }

    private RemoteControlAccess GetReference(IUser channel) => new()
    {
        ID = channel.Id,
        Name = channel.Username,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };

    private RemoteControlAccess GetReference(ulong id) => new()
    {
        ID = id,
        Name = "Manual",
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };

    protected static IEnumerable<ulong> GetIDs(string content)
    {
        return content.Split([",", ", ", " "], StringSplitOptions.RemoveEmptyEntries)
            .Select(z => ulong.TryParse(z, out var x) ? x : 0).Where(z => z != 0);
    }
}
