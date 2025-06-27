using Kook.Commands;
using Kook.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public sealed class RequireSudoAttribute : PreconditionAttribute
{
    // Override the CheckPermissions method
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        var mgr = KookBotSettings.Manager;
        if (mgr.Config.AllowGlobalSudo && mgr.CanUseSudo(context.User.Id))
            return Task.FromResult(PreconditionResult.FromSuccess());

        // Check if this user is a Guild User, which is the only context where roles exist
        if (context.User is not SocketGuildUser gUser)
            return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));

        if (mgr.CanUseSudo(gUser.Roles.Select(z => z.Name)))
            return Task.FromResult(PreconditionResult.FromSuccess());

        // Since it wasn't, fail
        return Task.FromResult(PreconditionResult.FromError("You are not permitted to run this command."));
    }
}
