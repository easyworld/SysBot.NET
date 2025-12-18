using Kook;
using Kook.Commands;
using PKHeX.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class OwnerModule<T> : SudoModule<T> where T : PKM, new()
{
    [Command("addSudo")]
    [Summary("将@提到的用户添加到全局管理员列表")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task SudoUsers([Remainder] string _)
    {
        var users = Context.Message.MentionedUsers;
        var objects = users.Select(GetReference);
        KookBotSettings.Settings.GlobalSudoList.AddIfNew(objects);
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("removeSudo")]
    [Summary("将@提到的用户从全局管理员列表移除")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task RemoveSudoUsers([Remainder] string _)
    {
        var users = Context.Message.MentionedUsers;
        var objects = users.Select(GetReference);
        KookBotSettings.Settings.GlobalSudoList.RemoveAll(z => objects.Any(o => o.ID == z.ID));
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("addChannel")]
    [Summary("将当前频道添加到接受命令的频道列表中。")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task AddChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        KookBotSettings.Settings.ChannelWhitelist.AddIfNew([obj]);
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("removeChannel")]
    [Summary("将当前频道从接受命令的频道列表中移除。")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task RemoveChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        KookBotSettings.Settings.ChannelWhitelist.RemoveAll(z => z.ID == obj.ID);
        await ReplyTextAsync("完成。").ConfigureAwait(false);
    }

    [Command("leave")]
    [Alias("bye")]
    [Summary("离开当前服务器。")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task Leave()
    {
        await ReplyTextAsync("再见。").ConfigureAwait(false);
        await Context.Guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("leaveguild")]
    [Alias("lg")]
    [Summary("根据提供的ID离开服务器。")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task LeaveGuild(string userInput)
    {
        if (!ulong.TryParse(userInput, out ulong id))
        {
            await ReplyTextAsync("请提供有效的服务器ID。").ConfigureAwait(false);
            return;
        }

        var guild = Context.Client.Guilds.FirstOrDefault(x => x.Id == id);
        if (guild is null)
        {
            await ReplyTextAsync($"提供的输入 ({userInput}) 不是有效的服务器ID，或者机器人不在指定的服务器中。").ConfigureAwait(false);
            return;
        }

        await ReplyTextAsync($"正在离开 {guild}。").ConfigureAwait(false);
        await guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("leaveall")]
    [Summary("离开机器人当前所在的所有服务器。")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task LeaveAll()
    {
        await ReplyTextAsync("正在离开所有服务器。").ConfigureAwait(false);
        foreach (var guild in Context.Client.Guilds)
            await guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("sudoku")]
    [Alias("kill", "shutdown")]
    [Summary("关闭整个机器人进程！")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task ExitProgram()
    {
        await Context.Channel.EchoAndReply($"正在关闭... 再见！ {Format.Bold("机器人服务即将离线。")}").ConfigureAwait(false);
        Environment.Exit(0);
    }

    private RemoteControlAccess GetReference(IUser channel) => new()
    {
        ID = channel.Id,
        Name = channel.Username,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };

    private RemoteControlAccess GetReference(IChannel channel) => new()
    {
        ID = channel.Id,
        Name = channel.Name,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };
}
