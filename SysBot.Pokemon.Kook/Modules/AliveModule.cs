using Kook.Commands;
using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class AliveModule<T> : IModule where T : PKM, new()
{
    public bool? IsEnable { get; set; } = true;

    public async Task ExecuteAsync(SocketCommandContext context)
    {
        KookSettings settings = KookBot<T>.Settings;

        var text = KookMessageUtil.SanitizeMessage(context.Message.Content);
        if (settings.AliveMsg == text)
        {
            await context.Channel.SendTextAsync(settings.AliveMsg);
            return;
        }
    }
}
