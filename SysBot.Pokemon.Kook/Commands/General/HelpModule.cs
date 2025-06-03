using Kook;
using Kook.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class HelpModule(CommandService Service) : ModuleBase<SocketCommandContext>
{
    [Command("help")]
    [Summary("Lists available commands.")]
    public async Task HelpAsync()
    {
        var builder = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText("Help has arrived!"))
            .AddModule(new SectionModuleBuilder().WithText("These are the commands you can use:"));

        var mgr = KookBotSettings.Manager;
        //var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
        //var owner = app.Owner.Id;
        var uid = Context.User.Id;

        foreach (var module in Service.Modules)
        {
            string? description = null;
            HashSet<string> mentioned = [];
            foreach (var cmd in module.Commands)
            {
                var name = cmd.Name;
                if (mentioned.Contains(name))
                    continue;
                //if (cmd.Attributes.Any(z => z is RequireOwnerAttribute) && owner != uid)
                //    continue;
                if (cmd.Attributes.Any(z => z is RequireSudoAttribute) && !mgr.CanUseSudo(uid))
                    continue;

                mentioned.Add(name);
                var result = await cmd.CheckPreconditionsAsync(Context).ConfigureAwait(false);
                if (result.IsSuccess)
                    description += $"{cmd.Aliases[0]}\n";
            }
            if (string.IsNullOrWhiteSpace(description))
                continue;

            var moduleName = module.Name;
            var gen = moduleName.IndexOf('`');
            if (gen != -1)
                moduleName = moduleName[..gen];

            builder.AddModule(new SectionModuleBuilder().WithText($"**{moduleName}**\n{description}"));
        }

        await ReplyCardAsync(builder.Build()).ConfigureAwait(false);
    }

    [Command("help")]
    [Summary("Lists information about a specific command.")]
    public async Task HelpAsync([Summary("The command you want help for")] string command)
    {
        var result = Service.Search(Context, command);

        if (!result.IsSuccess)
        {
            await ReplyTextAsync($"Sorry, I couldn't find a command like **{command}**.").ConfigureAwait(false);
            return;
        }

        var builder = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText("Help has arrived!"))
            .AddModule(new SectionModuleBuilder().WithText($"Here are some commands like **{command}**:"));

        foreach (var match in result.Commands)
        {
            var cmd = match.Command;

            builder.AddModule(new SectionModuleBuilder().WithText($"**{string.Join(", ", cmd.Aliases)}**\n{GetCommandSummary(cmd)}"));
        }

        await ReplyCardAsync(builder.Build()).ConfigureAwait(false);
    }

    private static string GetCommandSummary(CommandInfo cmd)
    {
        return $"Summary: {cmd.Summary}\nParameters: {GetParameterSummary(cmd.Parameters)}";
    }

    private static string GetParameterSummary(IReadOnlyList<ParameterInfo> p)
    {
        if (p.Count == 0)
            return "None";
        return $"{p.Count}\n- " + string.Join("\n- ", p.Select(GetParameterSummary));
    }

    private static string GetParameterSummary(ParameterInfo z)
    {
        var result = z.Name;
        if (!string.IsNullOrWhiteSpace(z.Summary))
            result += $" ({z.Summary})";
        return result;
    }
}
