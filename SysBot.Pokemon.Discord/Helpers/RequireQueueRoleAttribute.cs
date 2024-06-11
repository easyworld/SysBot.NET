using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    /// <summary>
    /// Same as <see cref="RequireRoleAccessAttribute"/> with extra consideration for bots accepting Queue requests.
    /// </summary>
    public sealed class RequireQueueRoleAttribute : PreconditionAttribute
    {
        // Create a field to store the specified name
        private readonly string _name;

        // Create a constructor so the name can be specified
        public RequireQueueRoleAttribute(string name) => _name = name;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var mgr = SysCordSettings.Manager;
            if (mgr.Config.AllowGlobalSudo && mgr.CanUseSudo(context.User.Id))
                return Task.FromResult(PreconditionResult.FromSuccess());

            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is not SocketGuildUser gUser)
                return Task.FromResult(PreconditionResult.FromError("您必須從社群頻道發送訊息才能執行此命令！"));

            var roles = gUser.Roles;
            if (mgr.CanUseSudo(roles.Select(z => z.Name)))
                return Task.FromResult(PreconditionResult.FromSuccess());

            bool canQueue = SysCordSettings.HubConfig.Queues.CanQueue;
            if (!canQueue)
                return Task.FromResult(PreconditionResult.FromError("抱歉，由於人數過多我目前不接受隊列請求！請稍後重試~"));

            if (!mgr.GetHasRoleAccess(_name, roles.Select(z => z.Name)))
                return Task.FromResult(PreconditionResult.FromError("您沒有執行此命令所需的角色！"));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
