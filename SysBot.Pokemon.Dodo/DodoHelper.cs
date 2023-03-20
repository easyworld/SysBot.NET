using System;
using System.Collections.Generic;
using System.Linq;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.Dodo
{
    public class DodoHelper<T> where T : PKM, new()
    {
        public static void StartTrade(string ps, string dodoId, string nickName, string channelId, string islandSourceId)
        {
            var _ = CheckAndGetPkm(ps, dodoId, out var msg, out var pkm);
            if (!_)
            {
                DodoBot<T>.SendChannelMessage(msg, channelId);
                return;
            }
            var foreign = ps.Contains("Language: ");

            StartTradeWithoutCheck(pkm, dodoId, nickName, channelId, islandSourceId, foreign);
        }

        public static void StartTrade(T pkm, string dodoId, string nickName, string channelId, string islandSourceId)
        {
            var _ = CheckPkm(pkm, dodoId, out var msg);
            if (!_)
            {
                DodoBot<T>.SendChannelMessage(msg, channelId);
                return;
            }

            StartTradeWithoutCheck(pkm, dodoId, nickName, channelId, islandSourceId);
        }

        public static void StartTradeMultiChinese(string chinesePsRaw, string dodoId, string nickName, string channelId, string islandSourceId)
        {
            var chinesePss = chinesePsRaw.Split('+').ToList();
            var MaxPkmsPerTrade = DodoBot<T>.Info.Hub.Config.Trade.MaxPkmsPerTrade;
            if (MaxPkmsPerTrade <= 1)
            {
                DodoBot<T>.SendChannelMessage("请联系群主将trade/MaxPkmsPerTrade配置改为大于1", channelId);
                return;
            }
            else if (chinesePss.Count > MaxPkmsPerTrade) 
            {
                DodoBot<T>.SendChannelMessage($"批量交换宝可梦数量应小于等于{MaxPkmsPerTrade}", channelId);
                return;
            }
            List<string> msgs = new();
            List<T> pkms = new();
            List<bool> skipAutoOTList = new();
            int invalidCount = 0;
            for (var i = 0; i < chinesePss.Count; i++)
            {
                var ps = ShowdownTranslator<T>.Chinese2Showdown(chinesePss[i]);
                var _ = CheckAndGetPkm(ps, dodoId, out var msg, out var pkm);
                if (!_)
                {
                    LogUtil.LogInfo($"批量第{i+1}只宝可梦有问题:{msg}", nameof(DodoHelper<T>));
                    invalidCount++;
                }
                else
                {
                    LogUtil.LogInfo($"批量第{i+1}只:\n{ps}", nameof(DodoHelper<T>));
                    skipAutoOTList.Add(ps.Contains("Language: "));
                    pkms.Add(pkm);
                }
            }
            if (invalidCount == chinesePss.Count)
            {
                DodoBot<T>.SendChannelMessage("一个都不合法，换个屁", channelId);
                return;
            } 
            else if (invalidCount != 0)
            {
                DodoBot<T>.SendChannelMessage($"期望交换的{chinesePss.Count}只宝可梦中，有{invalidCount}只不合法，仅交换合法的{pkms.Count}只", channelId);
            }

            var code = DodoBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkms, code, ulong.Parse(dodoId), nickName, channelId, islandSourceId, skipAutoOTList,
                PokeRoutineType.LinkTrade, out string message);
            DodoBot<T>.SendChannelMessage(message, channelId);
        }

        public static void StartTradeMultiPs(string pssRaw, string dodoId, string nickName, string channelId, string islandSourceId)
        {
            var psArray = pssRaw.Split("\n\n").ToList();
            var MaxPkmsPerTrade = DodoBot<T>.Info.Hub.Config.Trade.MaxPkmsPerTrade;
            if (MaxPkmsPerTrade <= 1)
            {
                DodoBot<T>.SendChannelMessage("请联系群主将trade/MaxPkmsPerTrade配置改为大于1", channelId);
                return;
            }
            else if (psArray.Count > MaxPkmsPerTrade)
            {
                DodoBot<T>.SendChannelMessage($"批量交换宝可梦数量应小于等于{MaxPkmsPerTrade}", channelId);
                return;
            }
            List<string> msgs = new();
            List<T> pkms = new();
            List<bool> skipAutoOTList = new();
            int invalidCount = 0;
            for (var i = 0; i < psArray.Count; i++)
            {
                var ps = psArray[i];
                var _ = CheckAndGetPkm(ps, dodoId, out var msg, out var pkm);
                if (!_)
                {
                    LogUtil.LogInfo($"批量第{i + 1}只宝可梦有问题:{msg}", nameof(DodoHelper<T>));
                    invalidCount++;
                }
                else
                {
                    LogUtil.LogInfo($"批量第{i + 1}只:\n{ps}", nameof(DodoHelper<T>));
                    skipAutoOTList.Add(ps.Contains("Language: "));
                    pkms.Add(pkm);
                }
            }
            if (invalidCount == psArray.Count)
            {
                DodoBot<T>.SendChannelMessage("一个都不合法，换个屁", channelId);
                return;
            }
            else if (invalidCount != 0)
            {
                DodoBot<T>.SendChannelMessage($"期望交换的{psArray.Count}只宝可梦中，有{invalidCount}只不合法，仅交换合法的{pkms.Count}只", channelId);
            }

            var code = DodoBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkms, code, ulong.Parse(dodoId), nickName, channelId, islandSourceId, skipAutoOTList,
                PokeRoutineType.LinkTrade, out string message);
            DodoBot<T>.SendChannelMessage(message, channelId);
        }

        public static void StartTradeWithoutCheck(T pkm, string dodoId, string nickName, string channelId, string islandSourceId, bool foreign = false)
        {
            var code = DodoBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code, ulong.Parse(dodoId), nickName, channelId, islandSourceId, foreign,
                PokeRoutineType.LinkTrade, out string message);
            DodoBot<T>.SendChannelMessage(message, channelId);
        }

        public static void StartDump(string dodoId, string nickName, string channelId)
        {
            var code = DodoBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, ulong.Parse(dodoId), nickName, channelId, "", false,
                PokeRoutineType.Dump, out string message);
            DodoBot<T>.SendChannelMessage(message, channelId);
        }

        public static bool CheckPkm(T pkm, string username, out string msg)
        {
            if (!DodoBot<T>.Info.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }
            try
            {
                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, <@!{username}>: 官方禁止该宝可梦交易!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid || DodoBot<T>.Info.Hub.Config.Legality.FileillegalMod)
                    {
                        msg =
                            $"<@!{username}> - 已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                }

                var reason = "我没办法创造非法宝可梦";
                msg = $"<@!{username}>: {reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(DodoHelper<T>));
                msg = $"取消派送, <@!{username}>: 发生了一个错误";
            }

            return false;
        }

        public static bool CheckAndGetPkm(string setstring, string username, out string msg, out T outPkm)
        {
            outPkm = new T();
            if (!DodoBot<T>.Info.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }

            var set = ShowdownUtil.ConvertToShowdown(setstring);
            if (set == null)
            {
                msg = $"取消派送, <@!{username}>: 宝可梦昵称为空.";
                return false;
            }

            var template = AutoLegalityWrapper.GetTemplate(set);
            if (template.Species < 1)
            {
                msg =
                    $"取消派送, <@!{username}>: 请使用正确的Showdown Set代码";
                return false;
            }

            if (set.InvalidLines.Count != 0)
            {
                msg =
                    $"取消派送, <@!{username}>: 非法的Showdown Set代码:\n{string.Join("\n", set.InvalidLines)}";
                return false;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);

                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, <@!{username}>: 官方禁止该宝可梦交易!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid || DodoBot<T>.Info.Hub.Config.Legality.CommandillegalMod)
                    {
                        outPkm = pk;

                        msg =
                            $"<@!{username}> - 已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                }

                var reason = result == "Timeout"
                    ? "宝可梦创造超时"
                    : "我没办法创造非法宝可梦";
                msg = $"<@!{username}>: {reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(DodoBot<T>));
                msg = $"取消派送, <@!{username}>: 发生了一个错误";
            }

            return false;
        }

        private static bool AddToTradeQueue(T pk, int code, ulong userId, string name, string channelId, string islandSourceId, bool foreign,
            PokeRoutineType type, out string msg)
        {
            return AddToTradeQueue(new List<T> { pk }, code, userId, name, channelId, islandSourceId, new List<bool>{ foreign }, type, out msg);
        }

        private static bool AddToTradeQueue(List<T> pks, int code, ulong userId, string name, string channelId, string islandSourceId, List<bool> skipAutoOTList,
            PokeRoutineType type, out string msg)
        {
            if (pks == null || pks.Count == 0)
            {
                msg = $"宝可梦数据为空";
                return false;
            }
            T pk = pks.First();
            var trainer = new PokeTradeTrainerInfo(name, userId);
            var notifier = new DodoTradeNotifier<T>(pk, trainer, code, name, channelId, islandSourceId);
            var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed : (type == PokeRoutineType.Dump ? PokeTradeType.Dump : PokeTradeType.Specific);
            var detail =
                new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, true);
            detail.Context.Add("skipAutoOTList", skipAutoOTList);
            if (pks.Count > 0)
            {
                detail.Context.Add("batch", pks);
            }
            var trade = new TradeEntry<T>(detail, userId, type, name);

            var added = DodoBot<T>.Info.AddToTradeQueue(trade, userId, false);

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                msg = $"<@!{userId}> 你已经在队列中，请不要重复发送";
                return false;
            }

            var position = DodoBot<T>.Info.CheckPosition(userId, type);
            //msg = $"@{name}: Added to the {type} queue, unique ID: {detail.ID}. Current Position: {position.Position}";
            msg = $" 你在第{position.Position}位";

            var botct = DodoBot<T>.Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = DodoBot<T>.Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                //msg += $". Estimated: {eta:F1} minutes.";
                msg += $", 需等待约{eta:F1}分钟";
            }

            return true;
        }
    }
}