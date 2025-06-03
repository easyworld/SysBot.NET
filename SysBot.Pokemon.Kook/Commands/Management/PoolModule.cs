using Kook;
using Kook.Commands;
using PKHeX.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

[Summary("Distribution Pool Module")]
public class PoolModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    [Command("poolReload")]
    [Summary("Reloads the bot pool from the setting's folder.")]
    [RequireSudo]
    public async Task ReloadPoolAsync()
    {
        var me = KookBot<T>.Runner;
        var hub = me.Hub;

        var pool = hub.Ledy.Pool.Reload(hub.Config.Folder.DistributeFolder);
        if (!pool)
            await ReplyTextAsync("Failed to reload from folder.").ConfigureAwait(false);
        else
            await ReplyTextAsync($"Reloaded from folder. Pool count: {hub.Ledy.Pool.Count}").ConfigureAwait(false);
    }

    [Command("pool")]
    [Summary("Displays the details of Pokémon files in the random pool.")]
    public async Task DisplayPoolCountAsync()
    {
        var me = KookBot<T>.Runner;
        var hub = me.Hub;
        var pool = hub.Ledy.Pool;
        var count = pool.Count;
        if (count is > 0 and < 20)
        {
            var lines = pool.Files.Select((z, i) => $"{i + 1:00}: {z.Key} = {(Species)z.Value.RequestInfo.Species}");
            var msg = string.Join("\n", lines);

            var card = new CardBuilder().AddModule(new SectionModuleBuilder().WithText("Pool Details"))
                .AddModule(new SectionModuleBuilder().WithText($"Count: {count}"))
                .AddModule(new SectionModuleBuilder().WithText(msg))
                .Build();
            await ReplyCardAsync(card).ConfigureAwait(false);
        }
        else
        {
            await ReplyTextAsync($"Pool Count: {count}").ConfigureAwait(false);
        }
    }
}
