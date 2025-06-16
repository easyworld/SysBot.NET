using Kook;
using Kook.Commands;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class PsModule<T> : IModule where T : PKM, new()
{
    public bool? IsEnable { get; set; } = true;

    public Task ExecuteAsync(SocketCommandContext context)
    {
        if (KookMessageUtil.IsJsonArray(context.Message.Content)) return Task.CompletedTask;

        var text = KookMessageUtil.SanitizeMessage(context.Message.Content);
        if (string.IsNullOrWhiteSpace(text)) return Task.CompletedTask;
        // 中英文判断  
        if (IsChinesePS(text))
            ProcessChinesePS(text, context);
        else if (IsPS(text))
            ProcessPS(text, context);

        return Task.CompletedTask;
    }

    private void ProcessPS(string text, SocketCommandContext context)
    {
        LogUtil.LogInfo($"收到ps代码:\n{text}", nameof(PsModule<T>));
        var pss = text.Split("\n\n");
        if (pss.Length > 1)
            new KookTrade<T>(context).StartTradeMultiPs(text);
        else
            new KookTrade<T>(context).StartTradePs(text);
    }

    private void ProcessChinesePS(string text, SocketCommandContext context)
    {
        LogUtil.LogInfo($"收到中文ps代码:\n{text}", nameof(PsModule<T>));
        var pss = text.Split("+");
        if (pss.Length > 1)
        { 
            new KookTrade<T>(context).StartTradeMultiChinesePs(text);
        }
        else
        {
            new KookTrade<T>(context).StartTradeChinesePs(text);
        }

    }

    private static bool IsChinesePS(string str)
    {
        var gameStrings = ShowdownTranslator<T>.GameStringsZh;
        for (int i = 1; i < gameStrings.Species.Count; i++)
        {
            if (str.Contains(gameStrings.Species[i]))
            {
                return true;
            }
        }
        return false;
    }
    private static bool IsPS(string str)
    {
        var gameStrings = ShowdownTranslator<T>.GameStringsEn;
        for (int i = 1; i < gameStrings.Species.Count; i++)
        {
            if (str.Contains(gameStrings.Species[i]))
            {
                return true;
            }
        }
        return false;
    }
}
