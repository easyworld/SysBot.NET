using Kook;
using Kook.WebSocket;
using PKHeX.Core;
using SysBot.Pokemon.Kook;
using System;
using System.Linq;

namespace SysBot.Pokemon.Kook;

public class KookTradeNotifier<T>(T Data, PokeTradeTrainerInfo Info, int Code, SocketUser Trader)
    : IPokeTradeNotifier<T>
    where T : PKM, new()
{
    private T Data { get; } = Data;
    private PokeTradeTrainerInfo Info { get; } = Info;
    private int Code { get; } = Code;
    private SocketUser Trader { get; } = Trader;
    public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }
    public readonly PokeTradeHub<T> Hub = KookBot<T>.Runner.Hub;

    public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
        Trader.SendTextAsync($"Initializing trade{receive}. Please be ready. Your code is {Format.Bold($"{Code: 0000 0000}")}.").ConfigureAwait(false);
    }

    public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var name = Info.TrainerName;
        var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", {name}";
        Trader.SendTextAsync($"I'm waiting for you{trainer}! Your code is {Format.Bold($"{Code:0000 0000}")}. My IGN is {Format.Bold($"{routine.InGameName}")}.").ConfigureAwait(false);
    }

    public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        OnFinish?.Invoke(routine);
        Trader.SendTextAsync($"Trade canceled: {msg}").ConfigureAwait(false);
    }

    public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
    {
        OnFinish?.Invoke(routine);
        var tradedToUser = Data.Species;
        var message = tradedToUser != 0 ? $"Trade finished. Enjoy your {(Species)tradedToUser}!" : "Trade finished!";
        Trader.SendTextAsync(message).ConfigureAwait(false);
        if (result.Species != 0 && Hub.Config.Kook.ReturnPKMs)
            Trader.SendPKMAsync(result, "Here's what you traded me!").ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
    {
        Trader.SendTextAsync(message).ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
    {
        if (message.ExtraInfo is SeedSearchResult r)
        {
            SendNotificationZ3(r);
            return;
        }

        var msg = message.Summary;
        if (message.Details.Count > 0)
            msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
        Trader.SendTextAsync(msg).ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
    {
        if (result.Species != 0 && (Hub.Config.Kook.ReturnPKMs || info.Type == PokeTradeType.Dump))
            Trader.SendPKMAsync(result, message).ConfigureAwait(false);
    }

    private void SendNotificationZ3(SeedSearchResult r)
    {
        var lines = r.ToString();

        var card = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText($"Here are the details for `{r.Seed:X16}`:"))
            .AddModule(new SectionModuleBuilder().WithText($"Seed: {r.Seed:X16}"))
            .AddModule(new SectionModuleBuilder().WithText(lines))
            .Build();
        Trader.SendCardAsync(card).ConfigureAwait(false);
    }
}
