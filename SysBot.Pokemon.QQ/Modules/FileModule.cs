using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Modules;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;

namespace SysBot.Pokemon.QQ
{
    public class FileModule<T> : IModule where T : PKM, new()
    {
        public bool? IsEnable { get; set; } = true;

        public async void Execute(MessageReceiverBase @base)
        {
            QQSettings settings = MiraiQQBot<T>.Settings;

            var receiver = @base.Concretize<GroupMessageReceiver>();

            var senderQQ = receiver.Sender.Id;
            var nickname = receiver.Sender.Name;
            var groupId = receiver.Sender.Group.Id;

            var fileMessage = receiver.MessageChain.OfType<FileMessage>()?.FirstOrDefault();
            if (fileMessage == null) return;
            LogUtil.LogInfo("In file module", nameof(FileModule<T>));
            var fileName = fileMessage.Name;
            string operationType;
            if (typeof(T) == typeof(PK8) && fileName.EndsWith(".pk8", StringComparison.OrdinalIgnoreCase)) 
                operationType = "pk8";
            else if (typeof(T) == typeof(PB8) && fileName.EndsWith(".pb8", StringComparison.OrdinalIgnoreCase))
                operationType = "pb8";
            else if (typeof(T) == typeof(PA8) && fileName.EndsWith(".pa8", StringComparison.OrdinalIgnoreCase))
                operationType = "pa8";
            else if (typeof(T) == typeof(PK9) && fileName.EndsWith(".pk9", StringComparison.OrdinalIgnoreCase))
                operationType = "pk9";
            else if (typeof(T) == typeof(PK9) && fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase)) 
                operationType = "bin";
            else return;

            PKM pkm = default!;
            List<PK9> pkms = new();
            try
            {
                var f = await FileManager.GetFileAsync(groupId, fileMessage.FileId, true);
                using var client = new HttpClient();
                byte[] data = client.GetByteArrayAsync(f.DownloadInfo.Url).Result;
                switch (operationType)
                {
                    case "pk8" or "pb8" or "pk9" when data.Length != 344:
                        await MessageManager.SendGroupMessageAsync(groupId, "非法文件");
                        return;
                    case "pa8" when data.Length != 376:
                        await MessageManager.SendGroupMessageAsync(groupId, "非法文件");
                        return;
                }

                switch (operationType)
                {
                    case "pk8":
                        pkm = new PK8(data);
                        break;
                    case "pb8":
                        pkm = new PB8(data);
                        break;
                    case "pa8":
                        pkm = new PA8(data);
                        break;
                    case "pk9":
                        pkm = new PK9(data);
                        break;
                    case "bin":
                        pkms = bin2List(data);
                        break;
                    default: return;
                }

                LogUtil.LogInfo($"operationType:{operationType}", nameof(FileModule<T>));
                await FileManager.DeleteFileAsync(groupId, fileMessage.FileId);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex.Message, nameof(FileModule<T>));
                return;
            }
            if (pkms != null && pkms.Count > 0)
                MiraiQQHelper<PK9>.StartTradeMulti(pkms, senderQQ, nickname, groupId);
            else
                MiraiQQHelper<T>.StartTrade((T)pkm, senderQQ, nickname, groupId);
        }

        private static List<PK9> bin2List(byte[] bb)
        {
            int count = 344;
            int times = bb.Length % count == 0 ? (bb.Length / count) : (bb.Length / count + 1);
            List<PK9> pkms = new();
            for (var i = 0; i < times; i++)
            {
                int start = i * count;
                int end = (start + count) > bb.Length ? bb.Length : (start + count);
                PK9 pk9 = new(bb[start..end]);
                if (pk9.Valid && pk9.Species > 0) pkms.Add(pk9);
            }
            return pkms;
        }
    }
}
