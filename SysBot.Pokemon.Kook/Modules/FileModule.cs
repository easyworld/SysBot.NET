using Kook.Commands;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class FileModule<T> : IModule where T : PKM, new()
{
    public bool? IsEnable { get; set; } = true;

    public async Task ExecuteAsync(SocketCommandContext context)
    {
        if (context.Message.Attachments.Count == 0 || !KookMessageUtil.IsJsonArray(context.Message.Content))
        {
            return;
        }
        
        LogUtil.LogInfo("In file module", nameof(FileModule<T>));
        var attachment = context.Message.Attachments.First();
        var fileName = attachment.Filename ?? "";
        var fileSize = attachment.Size ?? 0;
        if (!FileTradeHelper<T>.ValidFileName(fileName) || !FileTradeHelper<T>.ValidFileSize(fileSize))
        {
            await context.Channel.SendTextAsync("非法文件");
            return;
        }

        List<T> pkms = default!;
        try
        {
            using var client = new HttpClient();
            byte[] data = client.GetByteArrayAsync(attachment.Url).Result;
            pkms = FileTradeHelper<T>.Bin2List(data);
        }
        catch (Exception ex)
        {
            LogUtil.LogError(ex.Message, nameof(FileModule<T>));
            return;
        }
        if (pkms.Count > 1 && pkms.Count <= FileTradeHelper<T>.MaxCountInBin)
            new KookTrade<T>(context).StartTradeMultiPKM(pkms);
        else if (pkms.Count == 1)
            new KookTrade<T>(context).StartTradePKM(pkms[0]);
        else
            await context.Channel.SendTextAsync("文件内容不正确");
    }
}
