using Kook.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class RequireOwnerAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        // 你的Bot主人ID（可配置到配置文件）
        ulong ownerId = KookBotSettings.Manager.Owner; // 替换为你的 KOOK 用户ID

        if (context.User.Id == ownerId)
            return PreconditionResult.FromSuccess();
        else
            return PreconditionResult.FromError("只有Bot主人可以使用此命令。");
    }
}
