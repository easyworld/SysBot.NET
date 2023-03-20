using System;
using System.Collections.Generic;
using System.Linq;
using Manganese.Array;
using Mirai.Net.Utils.Scaffolds;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.QQ
{
    public class MiraiQQHelper<T> where T : PKM, new()
    {
        public static void StartTrade(string ps, string qq, string nickName, string groupId)
        {
            var _ = CheckAndGetPkm(ps, qq, out var msg, out var pkm);
            if (!_)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(msg).Build());
                return;
            }
            var foreign = ps.Contains("Language: ");

            StartTradeWithoutCheck(pkm, qq, nickName, groupId, foreign);
        }

        public static void StartTrade(T pkm, string qq, string nickName, string groupId)
        {
            var _ = CheckPkm(pkm, qq, out var msg);
            if (!_)
            {
                MiraiQQBot<T>.SendGroupMessage(new MessageChainBuilder().At(qq).Plain(msg).Build());
                return;
            }

            StartTradeWithoutCheck(pkm, qq, nickName, groupId);
        }

        public static void StartTradeMultiChinese(string chinesePsRaw, string qq, string nickName, string groupId)
        {
            var chinesePss = chinesePsRaw.Split('+').ToList();
            var MaxPkmsPerTrade = MiraiQQBot<T>.Info.Hub.Config.Trade.MaxPkmsPerTrade;
            if (MaxPkmsPerTrade <= 1)
            {
                MiraiQQBot<T>.SendGroupMessage("请联系群主将trade/MaxPkmsPerTrade配置改为大于1");
                return;
            }
            else if (chinesePss.Count > MaxPkmsPerTrade)
            {
                MiraiQQBot<T>.SendGroupMessage($"批量交换宝可梦数量应小于等于{MaxPkmsPerTrade}");
                return;
            }
            List<string> msgs = new();
            List<T> pkms = new();
            List<bool> skipAutoOTList = new();
            int invalidCount = 0;
            for (var i = 0; i < chinesePss.Count; i++)
            {
                var ps = ShowdownTranslator<T>.Chinese2Showdown(chinesePss[i]);
                var _ = CheckAndGetPkm(ps, qq, out var msg, out var pkm);
                if (!_)
                {
                    LogUtil.LogInfo($"批量第{i + 1}只宝可梦有问题:{msg}", nameof(MiraiQQHelper<T>));
                    invalidCount++;
                }
                else
                {
                    LogUtil.LogInfo($"批量第{i + 1}只:\n{ps}", nameof(MiraiQQHelper<T>));
                    skipAutoOTList.Add(ps.Contains("Language: "));
                    pkms.Add(pkm);
                }
            }
            if (invalidCount == chinesePss.Count)
            {
                MiraiQQBot<T>.SendGroupMessage("一个都不合法，换个屁");
                return;
            }
            else if (invalidCount != 0)
            {
                MiraiQQBot<T>.SendGroupMessage($"期望交换的{chinesePss.Count}只宝可梦中，有{invalidCount}只不合法，仅交换合法的{pkms.Count}只");
            }

            var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkms, code, ulong.Parse(qq), nickName, groupId, skipAutoOTList,
                PokeRoutineType.LinkTrade, out string message);
            MiraiQQBot<T>.SendGroupMessage(message);
        }

        public static void StartTradeMulti(List<T> rawPkms, string qq, string nickName, string groupId)
        {
            var MaxPkmsPerTrade = MiraiQQBot<T>.Info.Hub.Config.Trade.MaxPkmsPerTrade;
            if (MaxPkmsPerTrade <= 1)
            {
                MiraiQQBot<T>.SendGroupMessage("请联系群主将trade/MaxPkmsPerTrade配置改为大于1");
                return;
            }
            else if (rawPkms.Count > MaxPkmsPerTrade)
            {
                MiraiQQBot<T>.SendGroupMessage($"批量交换宝可梦数量应小于等于{MaxPkmsPerTrade}");
                return;
            }
            List<T> pkms = new();
            List<bool> skipAutoOTList = new();
            int invalidCount = 0;
            for (var i = 0; i < rawPkms.Count; i++)
            {
                var _ = CheckPkm(rawPkms[i], qq, out var msg);
                if (!_)
                {
                    LogUtil.LogInfo($"批量第{i + 1}只宝可梦有问题:{msg}", nameof(MiraiQQHelper<T>));
                    invalidCount++;
                }
                else
                {
                    LogUtil.LogInfo($"批量第{i + 1}只:\n{GameInfo.GetStrings("zh").Species[rawPkms[i].Species]}", nameof(MiraiQQHelper<T>));
                    skipAutoOTList.Add(false);
                    pkms.Add(rawPkms[i]);
                }
            }
            if (invalidCount == rawPkms.Count)
            {
                MiraiQQBot<T>.SendGroupMessage("一个都不合法，换个屁");
                return;
            }
            else if (invalidCount != 0)
            {
                MiraiQQBot<T>.SendGroupMessage($"期望交换的{rawPkms.Count}只宝可梦中，有{invalidCount}只不合法，仅交换合法的{pkms.Count}只");
            }

            var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkms, code, ulong.Parse(qq), nickName, groupId, skipAutoOTList,
                PokeRoutineType.LinkTrade, out string message);
            MiraiQQBot<T>.SendGroupMessage(message);
        }

        public static void StartTradeMultiPs(string pssRaw, string qq, string nickName, string groupId)
        {
            var psArray = pssRaw.Split("\n\n").ToList();
            var MaxPkmsPerTrade = MiraiQQBot<T>.Info.Hub.Config.Trade.MaxPkmsPerTrade;
            if (MaxPkmsPerTrade <= 1)
            {
                MiraiQQBot<T>.SendGroupMessage("请联系群主将trade/MaxPkmsPerTrade配置改为大于1");
                return;
            }
            else if (psArray.Count > MaxPkmsPerTrade)
            {
                MiraiQQBot<T>.SendGroupMessage($"批量交换宝可梦数量应小于等于{MaxPkmsPerTrade}");
                return;
            }
            List<T> pkms = new();
            List<bool> skipAutoOTList = new();
            int invalidCount = 0;
            for (var i = 0; i < psArray.Count; i++)
            {
                var ps = psArray[i];
                var _ = CheckAndGetPkm(ps, qq, out var msg, out var pkm);
                if (!_)
                {
                    LogUtil.LogInfo($"批量第{i + 1}只宝可梦有问题:{msg}", nameof(MiraiQQHelper<T>));
                    invalidCount++;
                }
                else
                {
                    LogUtil.LogInfo($"批量第{i + 1}只:\n{ps}", nameof(MiraiQQHelper<T>));
                    skipAutoOTList.Add(ps.Contains("Language: "));
                    pkms.Add(pkm);
                }
            }
            if (invalidCount == psArray.Count)
            {
                MiraiQQBot<T>.SendGroupMessage("一个都不合法，换个屁");
                return;
            }
            else if (invalidCount != 0)
            {
                MiraiQQBot<T>.SendGroupMessage($"期望交换的{psArray.Count}只宝可梦中，有{invalidCount}只不合法，仅交换合法的{pkms.Count}只");
            }

            var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkms, code, ulong.Parse(qq), nickName, groupId, skipAutoOTList,
                PokeRoutineType.LinkTrade, out string message);
            MiraiQQBot<T>.SendGroupMessage(message);
        }

        public static void StartTradeWithoutCheck(T pkm, string qq, string nickName, string groupId, bool foreign = false)
        {
            var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code, ulong.Parse(qq), nickName, groupId, foreign,
                PokeRoutineType.LinkTrade, out string message);
            MiraiQQBot<T>.SendGroupMessage(message);
        }

        public static void StartDump(string qq, string nickName, string groupId)
        {
            var code = MiraiQQBot<T>.Info.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, ulong.Parse(qq), nickName, groupId, false,
                PokeRoutineType.Dump, out string message);
            MiraiQQBot<T>.SendGroupMessage(message);
        }

        public static bool CheckPkm(T pkm, string username, out string msg)
        {
            if (!MiraiQQBot<T>.Info.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }
            try
            {
                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, 官方禁止该宝可梦交易!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid)
                    {
                        msg =
                            $"已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                }

                var reason = "我没办法创造非法宝可梦";
                msg = $"{reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(MiraiQQBot<T>));
                msg = $"取消派送, 发生了一个错误";
            }

            return false;
        }

        public static bool CheckAndGetPkm(string setstring, string username, out string msg, out T outPkm)
        {
            outPkm = new T();
            if (!MiraiQQBot<T>.Info.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }

            var set = ShowdownUtil.ConvertToShowdown(setstring);
            if (set == null)
            {
                msg = $"取消派送, 宝可梦昵称为空.";
                return false;
            }

            var template = AutoLegalityWrapper.GetTemplate(set);
            if (template.Species < 1)
            {
                msg =
                    $"取消派送, 请使用正确的Showdown Set代码";
                return false;
            }

            if (set.InvalidLines.Count != 0)
            {
                msg =
                    $"取消派送, 非法的Showdown Set代码:\n{string.Join("\n", set.InvalidLines)}";
                return false;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);

                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, 官方禁止该宝可梦交易!";
                    return false;
                }

                if (pkm is T pk)
                {
                    var valid = new LegalityAnalysis(pkm).Valid;
                    if (valid)
                    {
                        outPkm = pk;

                        msg =
                            $"已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                }

                var reason = result == "Timeout"
                    ? "宝可梦创造超时"
                    : "我没办法创造非法宝可梦";
                msg = $"{reason}";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                LogUtil.LogSafe(ex, nameof(MiraiQQBot<T>));
                msg = $"取消派送, 发生了一个错误";
            }

            return false;
        }

        private static bool AddToTradeQueue(T pk, int code, ulong userId, string name, string groupId, bool foreign,
            PokeRoutineType type, out string msg)
        {
            return AddToTradeQueue(new List<T> { pk }, code, userId, name, groupId, new List<bool> { foreign }, type, out msg);
        }

        private static bool AddToTradeQueue(List<T> pks, int code, ulong userId, string name, string groupId, List<bool> skipAutoOTList,
            PokeRoutineType type, out string msg)
        {
            if (pks == null || pks.Count == 0)
            {
                msg = $"宝可梦数据为空";
                return false;
            }
            T pk = pks.First();
            var trainer = new PokeTradeTrainerInfo(name, userId);
            var notifier = new MiraiQQTradeNotifier<T>(pk, trainer, code, name, groupId);
            var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed : (type == PokeRoutineType.Dump ? PokeTradeType.Dump : PokeTradeType.Specific);
            var detail =
                new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, true);
            detail.Context.Add("skipAutoOTList", skipAutoOTList);
            if (pks.Count > 0)
            {
                detail.Context.Add("batch", pks);
            }
            var trade = new TradeEntry<T>(detail, userId, type, name);

            var added = MiraiQQBot<T>.Info.AddToTradeQueue(trade, userId, false);

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                msg = $"你已经在队列中，请不要重复发送";
                return false;
            }

            var position = MiraiQQBot<T>.Info.CheckPosition(userId, type);
            //msg = $"@{name}: Added to the {type} queue, unique ID: {detail.ID}. Current Position: {position.Position}";
            msg = $" 你在第{position.Position}位";

            var botct = MiraiQQBot<T>.Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = MiraiQQBot<T>.Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                //msg += $". Estimated: {eta:F1} minutes.";
                msg += $", 需等待约{eta:F1}分钟";
            }

            return true;
        }
    }
}