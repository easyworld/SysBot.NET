using Kook;
using Kook.Commands;
using Kook.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PKHeX.Core;
using SysBot.Base;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SysBot.Pokemon.Kook;

public static class KookBotSettings
{
    public static KookManager Manager { get; internal set; } = default!;
    public static KookSettings Settings => Manager.Config;
    public static PokeTradeHubConfig HubConfig { get; internal set; } = default!;
}

public sealed class KookBot<T> where T : PKM, new()
{
    public static PokeBotRunner<T> Runner { get; private set; } = default!;
    private readonly KookSocketClient _client;
    private readonly KookManager Manager;
    public readonly PokeTradeHub<T> Hub;

    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private bool MessageChannelsLoaded { get; set; }
    public KookBot(PokeBotRunner<T> runner) {
        Runner = runner;
        Hub = runner.Hub;
        Manager = new KookManager(Hub.Config.Kook);

        KookBotSettings.Manager = Manager;
        KookBotSettings.HubConfig = Hub.Config;

        _client = new KookSocketClient(new KookSocketConfig
        {
            LogLevel = LogSeverity.Info,
            MessageCacheSize = 1000, // Adjust as needed
            AlwaysDownloadUsers = true, // Download user data for commands
            DefaultRetryMode = RetryMode.AlwaysRetry,
        });

        _commands = new CommandService(new CommandServiceConfig
        {
            DefaultRunMode = Hub.Config.Kook.AsyncCommands ? RunMode.Async : RunMode.Sync,
            LogLevel = LogSeverity.Info,
            CaseSensitiveCommands = false,
        });

        _client.Log += Log;
        _commands.Log += Log;

        _services = ConfigServices();
    }

    private static ServiceProvider ConfigServices()
    {
        var map = new ServiceCollection();
        return map.BuildServiceProvider();
    }

    private static Task Log(LogMessage msg)
    {
        var text = $"[{msg.Severity,8}] {msg.Source}: {msg.Message} {msg.Exception}";
        Console.ForegroundColor = GetTextColor(msg.Severity);
        Console.WriteLine($"{DateTime.Now,-19} {text}");
        Console.ResetColor();

        LogUtil.LogText($"KookBot: {text}");

        return Task.CompletedTask;
    }

    private static ConsoleColor GetTextColor(LogSeverity sv) => sv switch
    {
        LogSeverity.Critical => ConsoleColor.Red,
        LogSeverity.Error => ConsoleColor.Red,

        LogSeverity.Warning => ConsoleColor.Yellow,
        LogSeverity.Info => ConsoleColor.White,

        LogSeverity.Verbose => ConsoleColor.DarkGray,
        LogSeverity.Debug => ConsoleColor.DarkGray,
        _ => Console.ForegroundColor,
    };

    public async Task MainAsync(string apiToken, CancellationToken token)
    {
        await InitCommands().ConfigureAwait(false);

        await _client.LoginAsync(TokenType.Bot, apiToken).ConfigureAwait(false);
        await _client.StartAsync().ConfigureAwait(false);
        LogUtil.LogInfo("Kook机器人启动成功。", "KookBot");

        var guilds = await _client.Rest.GetGuildsAsync().ConfigureAwait(false);
        if (guilds.Count != 0)
        {
            var guild = guilds.First();
            var owner = await guild.GetOwnerAsync();
            Manager.Owner = owner.Id;
            LogUtil.LogInfo($"从服务器设置所有者 {owner.Id}: {guild.Name} (ID: {guild.Id})", "KookBot");
        }
        else
        {
            LogUtil.LogError("未找到服务器。请确保机器人已添加到至少一个服务器中。", "KookBot");
        }
        // Wait infinitely so your bot actually stays connected.
        await MonitorLogIntervalAsync(token).ConfigureAwait(false);
    }

    public async Task InitCommands()
    {
        var assembly = Assembly.GetExecutingAssembly();

        await _commands.AddModulesAsync(assembly, _services).ConfigureAwait(false);
        var genericTypes = assembly.DefinedTypes.Where(z => z.IsSubclassOf(typeof(ModuleBase<SocketCommandContext>)) && z.IsGenericType);
        foreach (var t in genericTypes)
        {
            var genModule = t.MakeGenericType(typeof(T));
            try
            {
                await _commands.AddModuleAsync(genModule, _services).ConfigureAwait(false);
            } catch (Exception ex)
            {
                LogUtil.LogError($"添加模块 {genModule.Name} 失败: {ex.Message}", "KookBot");
                // Optionally, you can log the exception or handle it as needed
            }
        }
        var modules = _commands.Modules.ToList();

        var blacklist = Hub.Config.Kook.ModuleBlacklist
            .Replace("Module", "").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(z => z.Trim()).ToList();

        foreach (var module in modules)
        {
            var name = module.Name;
            name = name.Replace("Module", "");
            var gen = name.IndexOf('`');
            if (gen != -1)
                name = name[..gen];
            if (blacklist.Any(z => z.Equals(name, StringComparison.OrdinalIgnoreCase)))
                await _commands.RemoveModuleAsync(module).ConfigureAwait(false);
        }

        // Subscribe a handler to see if a message invokes a command.
        _client.Ready += LoadLoggingAndEcho;
        _client.MessageReceived += HandleMessageAsync;
    }

    private async Task HandleMessageAsync(SocketMessage arg, SocketGuildUser user, SocketTextChannel channel)
    {
        // Bail out if it's a System Message.
        if (arg is not SocketUserMessage msg)
            return;

        // We don't want the bot to respond to itself or other bots.
        if (msg.Author.Id == _client.CurrentUser?.Id || (msg.Author.IsBot ?? false))
            return;

        // Create a number to track where the prefix ends and the command begins
        int pos = 0;
        if (msg.HasStringPrefix(Hub.Config.Kook.CommandPrefix, ref pos))
        {
            bool handled = await TryHandleCommandAsync(msg, pos).ConfigureAwait(false);
            if (handled)
                return;
        }
        await TryHandleMessageAsync(msg).ConfigureAwait(false);
    }

    private async Task TryHandleMessageAsync(SocketUserMessage msg)
    {
        // should this be a service?
        if (msg.Attachments.Count > 0)
        {
            var mgr = Manager;
            var cfg = mgr.Config;
            if (cfg.ConvertPKMToShowdownSet && (cfg.ConvertPKMReplyAnyChannel || mgr.CanUseCommandChannel(msg.Channel.Id)))
            {
                foreach (var att in msg.Attachments)
                    await msg.Channel.RepostPKMAsShowdownAsync(att).ConfigureAwait(false);
            }
        }
        if (msg.MentionedUserIds.Where(id => id ==_client.CurrentUser?.Id).Any())
        {
            string commandPrefix = Manager.Config.CommandPrefix;
            await msg.Channel.SendTextAsync($"请使用 {commandPrefix}help 获取帮助");
        }
    }

    private async Task<bool> TryHandleCommandAsync(SocketUserMessage msg, int pos)
    {
        // Create a Command Context.
        var context = new SocketCommandContext(_client, msg);

        // Check Permission
        var mgr = Manager;
        if (!mgr.CanUseCommandUser(msg.Author.Id))
        {
            await msg.Channel.SendTextAsync("您没有权限使用此命令。").ConfigureAwait(false);
            return true;
        }
        if (!mgr.CanUseCommandChannel(msg.Channel.Id) && msg.Author.Id != mgr.Owner)
        {
            if (Hub.Config.Kook.ReplyCannotUseCommandInChannel)
                await msg.Channel.SendTextAsync("您不能在此频道使用该命令。").ConfigureAwait(false);
            return true;
        }

        // Execute the command. (result does not indicate a return value, 
        // rather an object stating if the command executed successfully).
        var guild = msg.Channel is SocketGuildChannel g ? g.Guild.Name : "未知服务器";
        await Log(new LogMessage(LogSeverity.Info, "Command", $"正在执行命令，来自 {guild}#{msg.Channel.Name}:@{msg.Author.Username}。内容: {msg}。ID: {msg.Author.IdentifyNumber}")).ConfigureAwait(false);
        var result = await _commands.ExecuteAsync(context, pos, _services).ConfigureAwait(false);

        if (result.Error == CommandError.UnknownCommand)
            return false;

        // Uncomment the following lines if you want the bot
        // to send a message if it failed.
        // This does not catch errors from commands with 'RunMode.Async',
        // subscribe a handler for '_commands.CommandExecuted' to see those.
        if (!result.IsSuccess)
            await msg.Channel.SendTextAsync(result.ErrorReason??"无错误原因").ConfigureAwait(false);
        return true;
    }

    private async Task MonitorLogIntervalAsync(CancellationToken token)
    {
        const int Interval = 20; // seconds
        // Check datetime for update
        while (!token.IsCancellationRequested)
        {
            var time = DateTime.Now;
            var lastLogged = LogUtil.LastLogged;

            var delta = time - lastLogged;
            var gap = TimeSpan.FromSeconds(Interval) - delta;

            if (gap <= TimeSpan.Zero)
            {
                await Task.Delay(2_000, token).ConfigureAwait(false);
                continue;
            }
            await Task.Delay(gap, token).ConfigureAwait(false);
        }
    }
    private async Task LoadLoggingAndEcho()
    {
        if (MessageChannelsLoaded)
            return;

        // Restore Echoes
        EchoModule.RestoreChannels(_client, Hub.Config.Kook);

        // Restore Logging
        LogModule.RestoreLogging(_client, Hub.Config.Kook);
        TradeStartModule<T>.RestoreTradeStarting(_client);

        // Don't let it load more than once in case of Kook hiccups.
        await Log(new LogMessage(LogSeverity.Info, "LoadLoggingAndEcho()", "日志和回声频道已加载！")).ConfigureAwait(false);
        MessageChannelsLoaded = true;
    }
}

