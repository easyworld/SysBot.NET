using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Pokemon.Bilibili;
using SysBot.Pokemon.QQ;
using SysBot.Pokemon.Dodo;

namespace SysBot.Pokemon.ConsoleApp;

/// <summary>
/// Bot Environment implementation with Integrations added.
/// </summary>
public class PokeBotRunnerImpl<T> : PokeBotRunner<T> where T : PKM, new()
{
    public PokeBotRunnerImpl(PokeTradeHub<T> hub, BotFactory<T> fac) : base(hub, fac) { }
    public PokeBotRunnerImpl(PokeTradeHubConfig config, BotFactory<T> fac) : base(config, fac) { }

    private static MiraiQQBot<T>? QQ;
    private static BilibiliLiveBot<T>? Bilibili;
    private static DodoBot<T>? Dodo;

    protected override void AddIntegrations()
    {
        AddQQBot(Hub.Config.QQ);
        AddBilibiliBot(Hub.Config.Bilibili);
        AddDodoBot(Hub.Config.Dodo);
    }

    private void AddQQBot(QQSettings config)
    {
        if (string.IsNullOrWhiteSpace(config.VerifyKey) || string.IsNullOrWhiteSpace(config.Address)) return;
        if (string.IsNullOrWhiteSpace(config.QQ) || string.IsNullOrWhiteSpace(config.GroupId)) return;
        if (QQ != null) return;
        //add qq bot
        QQ = new MiraiQQBot<T>(config, Hub);
    }

    private void AddBilibiliBot(BilibiliSettings config)
    {
        if (string.IsNullOrWhiteSpace(config.LogUrl) || config.RoomId == 0) return;
        if (Bilibili != null) return;
        Bilibili = new BilibiliLiveBot<T>(config, Hub);
    }

    private void AddDodoBot(DodoSettings config)
    {
        if (string.IsNullOrWhiteSpace(config.BaseApi) || string.IsNullOrWhiteSpace(config.ClientId) || string.IsNullOrWhiteSpace(config.Token)) return;
        if (Dodo != null) return;
        Dodo = new DodoBot<T>(config, Hub);
    }
}
