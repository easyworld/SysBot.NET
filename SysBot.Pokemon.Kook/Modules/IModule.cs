using Kook.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public interface IModule
{
    //
    // 摘要:
    //     是否启用模块
    bool? IsEnable { get; set; }

    //
    // 摘要:
    //     执行器
    //
    // 参数:
    //   context:
    Task ExecuteAsync(SocketCommandContext context);
}
