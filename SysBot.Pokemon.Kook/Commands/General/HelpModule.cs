using Kook;
using Kook.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class HelpModule(CommandService Service) : ModuleBase<SocketCommandContext>
{
    [Command("help")]
    [Summary("列出所有可用命令。")]
    public async Task HelpAsync()
    {
        var builder = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText("帮助已送达！"))
            .AddModule(new SectionModuleBuilder().WithText("以下是您可以使用的命令："));

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

            builder.AddModule(new SectionModuleBuilder().WithText(new KMarkdownElementBuilder($"**{moduleName}**\n{description}")));
        }

        await ReplyCardAsync(builder.Build()).ConfigureAwait(false);
    }

    [Command("help")]
    [Summary("列出特定命令的信息。")]
    public async Task HelpAsync([Summary("您需要帮助的命令")] string command)
    {
        var result = Service.Search(Context, command);

        if (!result.IsSuccess)
        {
            await ReplyTextAsync($"抱歉，我找不到像 {Format.Bold($"{command}")} 这样的命令。").ConfigureAwait(false);
            return;
        }

        var builder = new CardBuilder()
            .AddModule(new SectionModuleBuilder().WithText("帮助已送达！"))
            .AddModule(new SectionModuleBuilder().WithText(new KMarkdownElementBuilder($"以下是与 **{command}** 相关的命令：")));

        foreach (var match in result.Commands)
        {
            var cmd = match.Command;

            builder.AddModule(new SectionModuleBuilder().WithText(new KMarkdownElementBuilder($"**{string.Join(", ", cmd.Aliases)}**\n{GetCommandSummary(cmd)}")));
        }

        await ReplyCardAsync(builder.Build()).ConfigureAwait(false);
    }

    private static string GetCommandSummary(CommandInfo cmd)
    {
        return $"摘要：{cmd.Summary}\n参数：{GetParameterSummary(cmd.Parameters)}";
    }

    private static string GetParameterSummary(IReadOnlyList<ParameterInfo> p)
    {
        if (p.Count == 0)
            return "无";
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
