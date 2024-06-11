using PKHeX.Core;
using SysBot.Base;
using System;
using System.ComponentModel;//新增调用
using System.Collections.Generic;
using System.Linq;
using System.Reflection;//新增调用
using System.Text.RegularExpressions;

namespace SysBot.Pokemon.Dodo
{
    public class DodoTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private string Username { get; }

        private string ChannelId { get; }
        private string IslandSourceId { get; }

        public DodoTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string username, string channelId,
            string islandSourceId)
        {
            Data = data;
            Info = info;
            Code = code;
            Username = username;
            ChannelId = channelId;
            IslandSourceId = islandSourceId;
            LogUtil.LogText($"Created trade details for {Username} - {Code}");
        }

        public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            LogUtil.LogText(message);
            if (message.Contains("Found Trading Partner:"))
            {
                Regex regex = new Regex("TID: (\\d+)");
                string tid = regex.Match(message).Groups[1].ToString();
                regex = new Regex("SID: (\\d+)");
                string sid = regex.Match(message).Groups[1].ToString();
				var m1 = message.Split(':');
				if (m1.Length > 1)
				{
                    var m2 = m1[1].Split('.');
                    if (m2 != null)
                        DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(), $"找到你了{m2[0]}，你的里ID(SID7):{sid},表ID(TID7):{tid}", IslandSourceId);
                }
            }
            else if (message.StartsWith("批量"))
            {
                DodoBot<T>.SendChannelMessage(message, ChannelId);
            }
        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            string description = GetEnumDescription(msg);
			OnFinish?.Invoke(routine);
            var line = $"@{info.Trainer.TrainerName}: Trade canceled, {msg}";
            LogUtil.LogText(line);
			//中文化取消原因
			string chineseMessage;
			switch (description)
				{
					case "NoTrainerFound":
						chineseMessage = "未搜索到用户，请检查网络/注意操作速度！";
						break;
					case "TrainerTooSlow":
						chineseMessage = "用户操作太慢，请重新发起交换！";
						break;
					case "TrainerLeft":
						chineseMessage = "用户中途离开，请检查网络是否掉线";
						break;
					case "TrainerOfferCanceledQuick":
						chineseMessage = "用户取消交易太快";
						break;
					case "TrainerRequestBad":
						chineseMessage = "用户请求错误";
						break;
					case "IllegalTrade":
						chineseMessage = "非法交换";
						break;
					case "SuspiciousActivity":
						chineseMessage = "可疑交换";
						break;
						//接续
					case "RoutineCancel":
						chineseMessage = "常规取消";
						break;
					case "ExceptionConnection":
						chineseMessage = "异常连接";
						break;
					case "ExceptionInternal":
						chineseMessage = "内部异常";
						break;
					case "RecoverStart":
						chineseMessage = "重新启动";
						break;
					case "ecoverPostLinkCode":
						chineseMessage = "重新输入连接密码";
						break;
					case "RecoverOpenBox":
						chineseMessage = "重新打开盒子";
						break;
					case "RecoverReturnOverworld":
						chineseMessage = "重新返回初始界面";
						break;
					case "RecoverEnterUnionRoom":
						chineseMessage = "重新进入宝可入口站";
						break;
					// Add more cases as needed
					default:
						chineseMessage = "未知原因";
						break;
				}

			DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, $"交易取消：{chineseMessage}", ChannelId);
			
			//原代码注释掉了
            //DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, $"交易取消2：{description}", ChannelId);
            var waitUserIds = DodoBot<T>.Info.GetUserIdList(4).ToList();
            for (int i = 1; i < waitUserIds.Count(); i++)
            {
                DodoBot<T>.SendChannelAtMessage(waitUserIds[i], $"你在第{i + 1}位，还有{i}个以后就到你了！\n", ChannelId);
            }
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = $"@{info.Trainer.TrainerName}: " + (tradedToUser != 0
                ? $"Trade finished. Enjoy your {(Species) tradedToUser}!"
                : "Trade finished!");
			var text = //返回宝可梦信息
                 $"我收到精灵的种类:{ShowdownTranslator<T>.GameStringsZh.Species[result.Species]}\n" +
                 $"PID:{result.PID:X}\n" +
                 $"加密常数:{result.EncryptionConstant:X}\n" +
                 $"训练家姓名:{result.OriginalTrainerName}\n" +
                 $"训练家性别:{(result.OriginalTrainerGender == 0 ? "男" : "女")}\n" +
                 $"训练家表ID:{result.TrainerTID7}\n" +
                 $"训练家里ID:{result.TrainerSID7}";				 
            LogUtil.LogText(message);
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, "完成", ChannelId);
			DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(), text, IslandSourceId);//新增私信返回宝可梦信息
            var waitUserIds = DodoBot<T>.Info.GetUserIdList(4).ToList();
            for (int i = 1; i < waitUserIds.Count(); i++)
            {
                DodoBot<T>.SendChannelAtMessage(waitUserIds[i], $"你在第{i + 1}位，还有{i}个以后就到你了！\n", ChannelId);
            }
        }

        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            var msg =
                $"@{info.Trainer.TrainerName} (ID: {info.ID}): Initializing trade{receive} with you. Please be ready.";
            msg += $" Your trade code is: {info.Code:0000 0000}";
            LogUtil.LogText(msg);
            var text = $"\n派送:**{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}**\n密码:见私信\n状态:初始化";//新增编号
            List<T> batchPKMs = (List<T>)info.Context.GetValueOrDefault("batch", new List<T>());
            if (batchPKMs.Count > 1)
            {
                text = $"\n批量派送{batchPKMs.Count}只宝可梦\n密码:见私信\n状态:初始化";
            }
            DodoBot<T>.SendChannelAtMessage(info.Trainer.ID, text, ChannelId);
            DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(),
                $"派送:{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}\n密码:{info.Code:0000 0000}",
                IslandSourceId);
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", @{name}";
            var message = $"I'm waiting for you{trainer}! My IGN is {routine.InGameName}.";
            message += $" Your trade code is: {info.Code:0000 0000}";
            LogUtil.LogText(message);
            var text = $"派送:**{ShowdownTranslator<T>.GameStringsZh.Species[Data.Species]}**\n密码:见私信\n状态:搜索中";//新增编号
            List<T> batchPKMs = (List<T>)info.Context.GetValueOrDefault("batch", new List<T>());
            if (batchPKMs.Count > 1)
            {
                text = $"批量派送{batchPKMs.Count}只宝可梦\n密码:见私信\n状态:搜索中";
            }
            DodoBot<T>.SendChannelMessage(text, ChannelId);
            //DodoBot<T>.SendPersonalMessage(info.Trainer.ID.ToString(), $"{info.Code:0000 0000}", IslandSourceId);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            LogUtil.LogText(msg);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            var msg = $"Details for {result.FileName}: " + message;
            LogUtil.LogText(msg);
            if (result.Species != 0 && info.Type == PokeTradeType.Dump)
            {
                var text =
                    $"species:{result.Species}\npid:{result.PID}\nec:{result.EncryptionConstant}\nIVs:{string.Join(",", result.IVs)}\nisShiny:{result.IsShiny}";
                DodoBot<T>.SendChannelMessage(text, ChannelId);
            }
        }
    }
}
