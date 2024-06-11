using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SysBot.Pokemon.Discord
{
    public class DiscordTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private SocketUser Trader { get; }
        public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }
        public readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;

        public DiscordTradeNotifier(T data, PokeTradeTrainerInfo info, int code, SocketUser trader)
        {
            Data = data;
            Info = info;
            Code = code;
            Trader = trader;
        }
		
        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            //Trader.SendMessageAsync($"正在初始化{receive}. 請輸入好密碼準備連接. 您的交換密碼是 **{Code:0000 0000}**.").ConfigureAwait(false);
			//中文化寶可夢名字
            Trader.SendMessageAsync($"正在初始化{receive}. 請輸入好密碼準備連接. 您的交換密碼是 **{Code:0000 0000}**.").ConfigureAwait(false);			
        }

        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", {name}";
            Trader.SendMessageAsync($"开始搜索了！我的暱稱是 **{routine.InGameName}**.").ConfigureAwait(false);
        }

        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            OnFinish?.Invoke(routine);
            //Trader.SendMessageAsync($"交易取消2: {msg}").ConfigureAwait(false);
			// 将枚举转化为中文
			string chineseMessage = GetChineseMessage(msg);
			Trader.SendMessageAsync($"交易取消: {chineseMessage}").ConfigureAwait(false);
        }
		private string GetChineseMessage(PokeTradeResult msg)
		{
			Dictionary<PokeTradeResult, string> messageMappings = new Dictionary<PokeTradeResult, string>
			{
				{ PokeTradeResult.NoTrainerFound, "未搜尋到用戶，請檢查網路/注意操作速度！" },
				{ PokeTradeResult.TrainerTooSlow, "用戶操作太慢，請重新發起交換！" },
				{ PokeTradeResult.TrainerLeft, "用戶中途離開，請檢查網路是否斷線" },
				{ PokeTradeResult.TrainerOfferCanceledQuick, "用戶取消交易太快" },
				{ PokeTradeResult.TrainerRequestBad, "用戶請求錯誤" },
				{ PokeTradeResult.IllegalTrade, "非法交換" },
				{ PokeTradeResult.SuspiciousActivity, "可疑交換" },
				//接续
				{ PokeTradeResult.RoutineCancel, "常規取消" },
				{ PokeTradeResult.ExceptionConnection, "異常連接" },
				{ PokeTradeResult.ExceptionInternal, "内部異常" },
				{ PokeTradeResult.RecoverStart, "重新啓動" },
				{ PokeTradeResult.RecoverPostLinkCode, "重新輸入連接密碼" },
				{ PokeTradeResult.RecoverOpenBox, "重新打開盒子" },
				{ PokeTradeResult.RecoverReturnOverworld, "重新返回初始界面" },
				{ PokeTradeResult.RecoverEnterUnionRoom, "重新進入寶可入口站" },
				// 添加更多映射
			};

			return messageMappings.TryGetValue(msg, out var mappedValue) ? mappedValue : "未知原因";
		}
		
		

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = tradedToUser != 0 ? $"交易完成！祝您與 {(Species)tradedToUser} 玩的愉快!" : "交易結束!";
            Trader.SendMessageAsync(message).ConfigureAwait(false);
            if (result.Species != 0 && Hub.Config.Discord.ReturnPKMs)
                Trader.SendPKMAsync(result, "這是您傳給我的寶可夢文件!").ConfigureAwait(false);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            Trader.SendMessageAsync(message).ConfigureAwait(false);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            if (message.ExtraInfo is SeedSearchResult r)
            {
                SendNotificationZ3(r);
                return;
            }

            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            Trader.SendMessageAsync(msg).ConfigureAwait(false);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            if (result.Species != 0 && (Hub.Config.Discord.ReturnPKMs || info.Type == PokeTradeType.Dump))
                Trader.SendPKMAsync(result, message).ConfigureAwait(false);
        }

        private void SendNotificationZ3(SeedSearchResult r)
        {
            var lines = r.ToString();

            var embed = new EmbedBuilder { Color = Color.LighterGrey };
            embed.AddField(x =>
            {
                x.Name = $"Seed: {r.Seed:X16}";
                x.Value = lines;
                x.IsInline = false;
            });
            var msg = $"Here are the details for `{r.Seed:X16}`:";
            Trader.SendMessageAsync(msg, embed: embed.Build()).ConfigureAwait(false);
        }
    }
}
