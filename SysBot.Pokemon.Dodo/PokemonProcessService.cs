using System;
using System.Net.Http;
using DoDo.Open.Sdk.Models.Bots;
using DoDo.Open.Sdk.Models.ChannelMessages;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Helpers;

namespace SysBot.Pokemon.Dodo
{
    public class PokemonProcessService<TP> : EventProcessService where TP : PKM, new()
    {
        private readonly OpenApiService _openApiService;
        private static readonly string LogIdentity = "DodoBot";
        private static readonly string Welcome = "不能识别的指令！\n1.请使用**简体中文**/英文指令交换\n2.直接拖入PKHeX生成的.pk文件交换\n3.取消排队请输入:取消\n4.查询位置请输入:位置\n5.使用帮助请输入:帮助";
        private readonly string _channelId;
        private DodoSettings _dodoSettings;
        private string _botDodoSourceId = default!;

        public PokemonProcessService(OpenApiService openApiService, DodoSettings settings)
        {
            _openApiService = openApiService;
            _channelId = settings.ChannelId;
            _dodoSettings = settings;
        }

        public override void Connected(string message)
        {
            Console.WriteLine($"{message}\n");
        }

        public override void Disconnected(string message)
        {
            Console.WriteLine($"{message}\n");
        }

        public override void Reconnected(string message)
        {
            Console.WriteLine($"{message}\n");
        }

        public override void Exception(string message)
        {
            Console.WriteLine($"{message}\n");
        }

        public override void PersonalMessageEvent<T>(
            EventSubjectOutput<EventSubjectDataBusiness<EventBodyPersonalMessage<T>>> input)
        {
            var eventBody = input.Data.EventBody;

            if (eventBody.MessageBody is MessageBodyText messageBodyText)
            {
                DodoBot<TP>.SendPersonalMessage(eventBody.DodoSourceId, $"你好", eventBody.IslandSourceId);
            }
        }

        public override void ChannelMessageEvent<T>(
            EventSubjectOutput<EventSubjectDataBusiness<EventBodyChannelMessage<T>>> input)
        {
            var eventBody = input.Data.EventBody;
            if (!string.IsNullOrWhiteSpace(_channelId) && eventBody.ChannelId != _channelId) return;

            if (eventBody.MessageBody is MessageBodyFile messageBodyFile)
            {
                if (!FileTradeHelper<TP>.ValidFileSize(messageBodyFile.Size ?? 0) || !FileTradeHelper<TP>.ValidFileName(messageBodyFile.Name))
                {
                    ProcessWithdraw(eventBody.MessageId);
                    DodoBot<TP>.SendChannelMessage("非法文件", eventBody.ChannelId);
                    return;
                }
                using var client = new HttpClient();
                var downloadBytes = client.GetByteArrayAsync(messageBodyFile.Url).Result;
                var pkms = FileTradeHelper<TP>.Bin2List(downloadBytes);
                ProcessWithdraw(eventBody.MessageId);
                if (pkms.Count == 1) 
                    new DodoTrade<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradePKM(pkms[0]);
                else if (pkms.Count > 1 && pkms.Count <= FileTradeHelper<TP>.MaxCountInBin) 
                    new DodoTrade<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeMultiPKM(pkms);
                else
                    DodoBot<TP>.SendChannelMessage("文件内容不正确", eventBody.ChannelId);
                return;
            }

            if (eventBody.MessageBody is not MessageBodyText messageBodyText) return;

            var content = messageBodyText.Content;

            LogUtil.LogInfo($"{eventBody.Personal.NickName}({eventBody.DodoSourceId}):{content}", LogIdentity);
            if (_botDodoSourceId == null)
            {
                _botDodoSourceId = _openApiService.GetBotInfo(new GetBotInfoInput()).DodoSourceId;
            }
            if (!content.Contains($"<@!{_botDodoSourceId}>")) return;

            content = content.Substring(content.IndexOf('>') + 1);
            //if ((typeof(TP) == typeof(PK9) || (typeof(TP) == typeof(PK8)) && content.Contains("\n\n") && ShowdownTranslator<TP>.IsPS(content)))// 仅SV支持批量，其他偷懒还没写
            //if (typeof(TP) == typeof(PK9) && content.Contains("\n\n") && ShowdownTranslator<TP>.IsPS(content))// 仅SV支持批量，其他偷懒还没写
			if (content.Contains("\n\n") && ShowdownTranslator<TP>.IsPS(content))// 已开启批量
            {
                ProcessWithdraw(eventBody.MessageId);
                new DodoTrade<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeMultiPs(content.Trim());
                return;
            }
            else if (ShowdownTranslator<TP>.IsPS(content))
            {
                ProcessWithdraw(eventBody.MessageId);
                new DodoTrade<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradePs(content.Trim());
                return;
            }
            else if (content.Trim().StartsWith("dump"))
            {
                ProcessWithdraw(eventBody.MessageId);
                new DodoTrade<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartDump();
                return;
            }
			//else if ((typeof(TP) == typeof(PK9) || (typeof(TP) == typeof(PK8)) && content.Trim().Contains('+')))// 仅SV支持批量，其他偷懒还没写
			//else if (typeof(TP) == typeof(PK9) && content.Trim().Contains('+'))// 仅SV支持批量，其他偷懒还没写
			else if (content.Trim().Contains('+'))// 已开启批量
            {
                ProcessWithdraw(eventBody.MessageId);
                new DodoTrade<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradeMultiChinesePs(content.Trim());
                return;
            }

            var ps = ShowdownTranslator<TP>.Chinese2Showdown(content);
            if (!string.IsNullOrWhiteSpace(ps))
            {
                LogUtil.LogInfo($"收到命令\n{ps}", LogIdentity);
                ProcessWithdraw(eventBody.MessageId);
                new DodoTrade<TP>(ulong.Parse(eventBody.DodoSourceId), eventBody.Personal.NickName, eventBody.ChannelId, eventBody.IslandSourceId).StartTradePs(ps);
            }
            else if (content.Contains("取消"))
            {
                var result = DodoBot<TP>.Info.ClearTrade(ulong.Parse(eventBody.DodoSourceId));
                DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId), $" {GetClearTradeMessage(result)}",
                    eventBody.ChannelId);
            }
            else if (content.Contains("位置"))
            {
                var result = DodoBot<TP>.Info.CheckPosition(ulong.Parse(eventBody.DodoSourceId));
                DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId),
                    $" {GetQueueCheckResultMessage(result)}",
                    eventBody.ChannelId);
            }
            else if (content.Contains("帮助"))
            {
                var result = DodoBot<TP>.Info.CheckPosition(ulong.Parse(eventBody.DodoSourceId));
                DodoBot<TP>.SendChannelAtMessage(ulong.Parse(eventBody.DodoSourceId),
                    $"\n1.PKHeX使用教学：https://imdodo.com/p/499922403934482432 \n2.中文指令模板：https://imdodo.com/p/499915195851087872 \n3.英文指令在线生成：https://easyworld.github.io/ps/ \n4.中英文形态字典：https://docs.qq.com/sheet/DZWNRbEN5a1JsT0F0",
                    eventBody.ChannelId);
            }
            else
            {
                DodoBot<TP>.SendChannelMessage($"{Welcome}", eventBody.ChannelId);
            }
        }

        public string GetQueueCheckResultMessage(QueueCheckResult<TP> result)
        {
            if (!result.InQueue || result.Detail is null)
                return "你不在队列里";
            var msg = $"你在第{result.Position}位";
            var pk = result.Detail.Trade.TradeData;
            if (pk.Species != 0)
                msg += $"，交换宝可梦：{ShowdownTranslator<TP>.GameStringsZh.Species[result.Detail.Trade.TradeData.Species]}";
            return msg;
        }

        private static string GetClearTradeMessage(QueueResultRemove result)
        {
            return result switch
            {
                QueueResultRemove.CurrentlyProcessing => "你正在交换中",
                QueueResultRemove.CurrentlyProcessingRemoved => "正在删除",
                QueueResultRemove.Removed => "已删除",
                _ => "你不在队列里",
            };
        }

        private void ProcessWithdraw(string messageId)
        {
            if (_dodoSettings.WithdrawTradeMessage)
            {
                DodoBot<TP>.OpenApiService.SetChannelMessageWithdraw(new SetChannelMessageWithdrawInput() { MessageId = messageId }, true);
            }  
        }

        public override void MessageReactionEvent(
            EventSubjectOutput<EventSubjectDataBusiness<EventBodyMessageReaction>> input)
        {
            // Do nothing
        }

    }
}
