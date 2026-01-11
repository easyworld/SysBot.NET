using Kook.Commands;
using Kook;
using Kook.WebSocket;
using PKHeX.Core;
using SysBot.Base;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Linq;

namespace SysBot.Pokemon.Kook;

public class KookBot<T> where T : PKM, new()
{
    private static PokeTradeHub<T> Hub = default!;
    internal static TradeQueueInfo<T> Info => Hub.Queues.Info;

    private readonly KookSocketClient _client;

    internal static KookSettings Settings = default!;

    private readonly List<IModule> Modules = [];

    public KookBot(PokeBotRunner<T> runner)
    {
        Hub = runner.Hub;
        
        Settings = Hub.Config.Kook;
        _client = new KookSocketClient(new KookSocketConfig
        {
            LogLevel = LogSeverity.Info,
            MessageCacheSize = 1000, // Adjust as needed
            AlwaysDownloadUsers = false, // Download user data for commands
            DefaultRetryMode = RetryMode.AlwaysRetry,
        });

        _client.Log += Log;

        Modules =
        [
            new AliveModule<T>(),
            new CommandModule<T>(),
            new FileModule<T>(),
            new PsModule<T>()
        ];
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
        _client.MessageReceived += HandleMessageAsync;

        await _client.LoginAsync(TokenType.Bot, apiToken).ConfigureAwait(false);
        await _client.StartAsync().ConfigureAwait(false);
        LogUtil.LogInfo("Kook Bot started successfully.", "KookBot");
        var channel = await _client.GetChannelAsync(Settings.ChannelId).ConfigureAwait(false);
        if (channel is ITextChannel textChannel)
        {
            if (!string.IsNullOrWhiteSpace(Settings.MessageStart))
            {
                // Send a start message to the channel
                await textChannel.SendTextAsync(Settings.MessageStart).ConfigureAwait(false);
                await Task.Delay(1_000, token).ConfigureAwait(false);
            }
            if (typeof(T) == typeof(PK8))
            {
                await textChannel.SendTextAsync("当前版本为剑盾");
            }
            else if (typeof(T) == typeof(PB8))
            {
                await textChannel.SendTextAsync("当前版本为晶灿钻石明亮珍珠");
            }
            else if (typeof(T) == typeof(PA8))
            {
                await textChannel.SendTextAsync("当前版本为阿尔宙斯");
            }
            else if (typeof(T) == typeof(PK9))
            {
                await textChannel.SendTextAsync("当前版本为朱紫");
            }
            else if (typeof(T) == typeof(PA9))
            {
                await textChannel.SendTextAsync("当前版本为Z-A");
            }
            await Task.Delay(1_000, token).ConfigureAwait(false);
        }
        
        // Wait infinitely so your bot actually stays connected.
        await MonitorLogIntervalAsync(token).ConfigureAwait(false);
    }

    private async Task HandleMessageAsync(SocketMessage arg, SocketGuildUser user, SocketTextChannel channel)
    {
        if (arg is not SocketUserMessage msg)
            return;
        // Ignore messages from the bot itself or other bots
        if (msg.Author.Id == _client.CurrentUser?.Id || (msg.Author.IsBot ?? false))
            return;
        // Ignore messages from other channels
        if (msg.Channel.Id != Settings.ChannelId)
            return;
        if (msg.MentionedUserIds.Where(id => id == _client.CurrentUser?.Id).Any() || msg.Attachments.Count > 0)
        {
            // only respond to commands if the user is not a bot
            LogUtil.LogInfo($"Received command from {user.Username} in channel {channel.Name}: {msg.Content}", "KookBot");
            // Create a Command Context.
            var context = new SocketCommandContext(_client, msg);
            await Modules.Raise(context);
        }
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
}
