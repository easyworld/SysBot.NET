using Kook.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class RequireOwnerAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        ulong ownerId = KookBotSettings.Manager.Owner;

        if (context.User.Id == ownerId)
            return Task.FromResult(PreconditionResult.FromSuccess());
        else
            return Task.FromResult(PreconditionResult.FromError("只有Bot主人可以使用此命令。"));
    }
}
