using Kook.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public static class ModuleScaffold
{
    //
    // 摘要:
    //     传播订阅到模块
    //
    // 参数:
    //   modules:
    //
    //   base:
    public static async Task Raise(this List<IModule> modules, SocketCommandContext context)
    {
        foreach (IModule module in modules)
        {
            if (module.IsEnable ?? true)
            {
                await module.ExecuteAsync(context);
            }
        }
    }
}
