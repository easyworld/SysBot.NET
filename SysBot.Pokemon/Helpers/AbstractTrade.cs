using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SysBot.Pokemon.Helpers
{
    /// <summary>
    /// 宝可梦抽象交易类
    /// 本类需要实现SendMessage，同时也要实现一个多参数的构造函数，
    /// 参数应该包括该类型机器人发送消息的相关信息，以便SendMessage使用
    /// 注意在实现抽象类的构造方法里一定要调用SetPokeTradeTrainerInfo和SetTradeQueueInfo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractTrade<T> where T : PKM, new()
    {
        public abstract void SendMessage(string message);//完善此方法以实现发送消息功能
        public abstract IPokeTradeNotifier<T> GetPokeTradeNotifier(T pkm, int code);//完善此方法以实现消息通知功能
        protected PokeTradeTrainerInfo userInfo = default!;
        private TradeQueueInfo<T> queueInfo = default!;

        public void SetPokeTradeTrainerInfo(PokeTradeTrainerInfo pokeTradeTrainerInfo)
        {
            userInfo = pokeTradeTrainerInfo;
        }

        public void SetTradeQueueInfo(TradeQueueInfo<T> queueInfo)
        {
            this.queueInfo = queueInfo;
        }

        public void StartTradePs(string ps)
        {
            var _ = CheckAndGetPkm(ps, out var msg, out var pkm);
            if (!_)
            {
                SendMessage(msg);
                return;
            }
            var foreign = ps.Contains("Language: ");
            StartTradeWithoutCheck(pkm, foreign);
        }
        public void StartTradeChinesePs(string chinesePs)
        {
            var ps = ShowdownTranslator<T>.Chinese2Showdown(chinesePs);
            LogUtil.LogInfo($"中文转换后ps代码:\n{ps}", nameof(AbstractTrade<T>));
            StartTradePs(ps);
        }
        public void StartTradePKM(T pkm)
        {
            var _ = CheckPkm(pkm, out var msg);
            if (!_)
            {
                SendMessage(msg);
                return;
            }

            StartTradeWithoutCheck(pkm);
        }
        public void StartTradeMultiPs(string pss)
        {
            var psList = pss.Split("\n\n").ToList();
            if (!JudgeMultiNum(psList.Count)) return;

            var pkms = GetPKMsFromPsList(psList, isChinesePS: false, out int invalidCount, out List<bool> skipAutoOTList);

            if (!JudgeInvalidCount(invalidCount, psList.Count)) return;

            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkms, code, skipAutoOTList,
                PokeRoutineType.LinkTrade, out string message);
            SendMessage(message);
        }

        public void StartTradeMultiChinesePs(string chinesePssString)
        {
            var chinesePsList = chinesePssString.Split('+').ToList();
            if (!JudgeMultiNum(chinesePsList.Count)) return;

            List<T> pkms = GetPKMsFromPsList(chinesePsList, true, out int invalidCount, out List<bool> skipAutoOTList);

            if (!JudgeInvalidCount(invalidCount, chinesePsList.Count)) return;

            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkms, code, skipAutoOTList,
                PokeRoutineType.LinkTrade, out string message);
            SendMessage(message);
        }

        public void StartTradeMultiPKM(List<T> rawPkms)
        {
            if (!JudgeMultiNum(rawPkms.Count)) return;

            List<T> pkms = new();
            List<bool> skipAutoOTList = new();
            int invalidCount = 0;
            for (var i = 0; i < rawPkms.Count; i++)
            {
                var _ = CheckPkm(rawPkms[i], out var msg);
                if (!_)
                {
                    LogUtil.LogInfo($"批量第{i + 1}只宝可梦有问题:{msg}", nameof(AbstractTrade<T>));
                    invalidCount++;
                }
                else
                {
                    LogUtil.LogInfo($"批量第{i + 1}只:{GameInfo.GetStrings("zh").Species[rawPkms[i].Species]}", nameof(AbstractTrade<T>));
                    skipAutoOTList.Add(false);
                    pkms.Add(rawPkms[i]);
                }
            }

            if (!JudgeInvalidCount(invalidCount, rawPkms.Count)) return;

            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkms, code, skipAutoOTList,
                PokeRoutineType.LinkTrade, out string message);
            SendMessage(message);
        }
        /// <summary>
        /// 根据pokemon showdown代码生成对应版本的PKM文件
        /// </summary>
        /// <param name="psList">ps代码</param>
        /// <param name="isChinesePS">是否是中文ps</param>
        /// <param name="invalidCount">不合法的宝可梦数量</param>
        /// <param name="skipAutoOTList">需要跳过自id的列表</param>
        /// <returns></returns>
        private List<T> GetPKMsFromPsList(List<string> psList, bool isChinesePS, out int invalidCount, out List<bool> skipAutoOTList)
        {
            List<T> pkms = new();
            skipAutoOTList = new List<bool>();
            invalidCount = 0;
            for (var i = 0; i < psList.Count; i++)
            {
                var ps = isChinesePS ? ShowdownTranslator<T>.Chinese2Showdown(psList[i]) : psList[i];
                var _ = CheckAndGetPkm(ps, out var msg, out var pkm);
                if (!_)
                {
                    LogUtil.LogInfo($"批量第{i + 1}只宝可梦有问题:{msg}", nameof(AbstractTrade<T>));
                    invalidCount++;
                }
                else
                {
                    LogUtil.LogInfo($"批量第{i + 1}只:\n{ps}", nameof(AbstractTrade<T>));
                    skipAutoOTList.Add(ps.Contains("Language: "));
                    pkms.Add(pkm);
                }
            }
            return pkms;
        }
        /// <summary>
        /// 判断是否符合批量规则
        /// </summary>
        /// <param name="multiNum">待计算的数量</param>
        /// <returns></returns>
        private bool JudgeMultiNum(int multiNum)
        {
            var maxPkmsPerTrade = queueInfo.Hub.Config.Trade.MaxPkmsPerTrade;
            if (maxPkmsPerTrade <= 1)
            {
                SendMessage("请联系群主将trade/MaxPkmsPerTrade配置改为大于1");
                return false;
            }
            else if (multiNum > maxPkmsPerTrade)
            {
                SendMessage($"批量交换宝可梦数量应小于等于{maxPkmsPerTrade}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 判断无效数量
        /// </summary>
        /// <param name="invalidCount"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        private bool JudgeInvalidCount(int invalidCount, int totalCount)
        {
            if (invalidCount == totalCount)
            {
                SendMessage("一个都不合法，换个屁");
                return false;
            }
            else if (invalidCount != 0)
            {
                SendMessage($"期望交换的{totalCount}只宝可梦中，有{invalidCount}只不合法，仅交换合法的{totalCount - invalidCount}只");
            }
            return true;
        }

        public void StartTradeWithoutCheck(T pkm, bool foreign = false)
        {
            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(pkm, code, foreign,
                PokeRoutineType.LinkTrade, out string message);
            SendMessage(message);
        }

        public void StartDump()
        {
            var code = queueInfo.GetRandomTradeCode();
            var __ = AddToTradeQueue(new T(), code, false,
                PokeRoutineType.Dump, out string message);
            SendMessage(message);
        }

        public bool Check(T pkm, out string msg)
        {
            try
            {
                if (!pkm.CanBeTraded())
                {
                    msg = $"取消派送, 官方禁止该宝可梦交易!";
                    return false;
                }
                if (pkm is T pk)
                {
                    var la = new LegalityAnalysis(pkm);
                    var valid = la.Valid;
                    if (valid)
                    {
                        msg = $"已加入等待队列. 如果你选宝可梦的速度太慢，你的派送请求将被取消!";
                        return true;
                    }
                    LogUtil.LogInfo($"非法原因:\n{la.Report()}", nameof(AbstractTrade<T>));
                }
                LogUtil.LogInfo($"pkm type:{pkm.GetType()}, T:{typeof(T)}", nameof(AbstractTrade<T>));
                var reason = "我没办法创造非法宝可梦";
                msg = $"{reason}";
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(AbstractTrade<T>));
                msg = $"取消派送, 发生了一个错误";
            }
            return false;
        }
        public bool CheckPkm(T pkm, out string msg)
        {
            if (!queueInfo.GetCanQueue())
            {
                msg = "对不起, 我不再接受队列请求!";
                return false;
            }
            return Check(pkm, out msg);
        }

        public bool CheckAndGetPkm(string setstring, out string msg, out T outPkm)
        {
            outPkm = new T();
            if (!queueInfo.GetCanQueue())
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

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);
                if (pkm.Nickname.ToLower() == "egg" && Breeding.CanHatchAsEgg(pkm.Species)) EggTrade(pkm, template);
                if (Check((T)pkm, out msg))
                {
                    outPkm = (T)pkm;
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(AbstractTrade<T>));
                msg = $"取消派送, 发生了一个错误";
            }

            return false;
        }

        private bool AddToTradeQueue(T pk, int code, bool skipAutoOT,
            PokeRoutineType type, out string msg)
        {
            return AddToTradeQueue(new List<T> { pk }, code, new List<bool> { skipAutoOT }, type, out msg);
        }

        private bool AddToTradeQueue(List<T> pks, int code, List<bool> skipAutoOTList,
            PokeRoutineType type, out string msg)
        {
            if (pks == null || pks.Count == 0)
            {
                msg = $"宝可梦数据为空";
                return false;
            }
            T pk = pks.First();
            var trainer = userInfo;
            var notifier = GetPokeTradeNotifier(pk, code);
            var tt = type == PokeRoutineType.SeedCheck
                ? PokeTradeType.Seed
                : (type == PokeRoutineType.Dump ? PokeTradeType.Dump : PokeTradeType.Specific);
            var detail =
                new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, true);
            detail.Context.Add("skipAutoOTList", skipAutoOTList);
            if (pks.Count > 0)
            {
                detail.Context.Add("batch", pks);
            }
            var trade = new TradeEntry<T>(detail, userInfo.ID, type, userInfo.TrainerName);

            var added = queueInfo.AddToTradeQueue(trade, userInfo.ID, false);

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                msg = $"你已经在队列中，请不要重复发送";
                return false;
            }

            var position = queueInfo.CheckPosition(userInfo.ID, type);
            //msg = $"@{name}: Added to the {type} queue, unique ID: {detail.ID}. Current Position: {position.Position}";
            msg = $"你在第{position.Position}位";

            var botct = queueInfo.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = queueInfo.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                //msg += $". Estimated: {eta:F1} minutes.";
                msg += $", 需等待约{eta:F1}分钟";
            }

            return true;
        }

        // https://github.com/Koi-3088/ForkBot.NET/blob/KoiTest/SysBot.Pokemon/Helpers/TradeExtensions.cs
        public static void EggTrade(PKM pk, IBattleTemplate template)
        {
            pk.IsNicknamed = true;
            pk.NicknameTrash.Clear();
            pk.Nickname = pk.Language switch
            {
                1 => "タマゴ",
                3 => "Œuf",
                4 => "Uovo",
                5 => "Ei",
                7 => "Huevo",
                8 => "알",
                9 or 10 => "蛋",
                _ => "Egg",
            };

            pk.IsEgg = true;
            pk.EggLocation = pk switch
            {
                PB8 => 60010,
                PK9 => 30023,
                _ => 60002, //PK8
            };

            pk.HeldItem = 0;
            pk.CurrentLevel = 1;
            pk.EXP = 0;
            pk.MetLevel = 1;
            pk.MetLocation = pk switch
            {
                PB8 => 65535,
                PK9 => 0,
                _ => 30002, //PK8
            };

            pk.CurrentHandler = 0;
            pk.OriginalTrainerFriendship = 1;
            pk.HandlingTrainerName = "";
            pk.HandlingTrainerFriendship = 0;
            pk.ClearMemories();
            pk.StatNature = pk.Nature;
            pk.SetEVs(new int[] { 0, 0, 0, 0, 0, 0 });

            MarkingApplicator.SetMarkings(pk);

            pk.ClearRelearnMoves();

            if (pk is PK8 pk8)
            {
                pk8.HandlingTrainerLanguage = 0;
                pk8.HandlingTrainerGender = 0;
                pk8.HandlingTrainerMemory = 0;
                pk8.HandlingTrainerMemoryFeeling = 0;
                pk8.HandlingTrainerMemoryIntensity = 0;
                pk8.DynamaxLevel = pk8.GetSuggestedDynamaxLevel(pk8, 0);
            }
            else if (pk is PB8 pb8)
            {
                pb8.HandlingTrainerLanguage = 0;
                pb8.HandlingTrainerGender = 0;
                pb8.HandlingTrainerMemory = 0;
                pb8.HandlingTrainerMemoryFeeling = 0;
                pb8.HandlingTrainerMemoryIntensity = 0;
                pb8.DynamaxLevel = pb8.GetSuggestedDynamaxLevel(pb8, 0);
            }
            else if (pk is PK9 pk9)
            {
                pk9.HandlingTrainerLanguage = 0;
                pk9.HandlingTrainerGender = 0;
                pk9.HandlingTrainerMemory = 0;
                pk9.HandlingTrainerMemoryFeeling = 0;
                pk9.HandlingTrainerMemoryIntensity = 0;
                pk9.ObedienceLevel = 1;
                pk9.Version = 0;
                pk9.BattleVersion = 0;
                pk9.TeraTypeOverride = (MoveType)19;
            }

            var la = new LegalityAnalysis(pk);
            var enc = la.EncounterMatch;
            pk.CurrentFriendship = enc is IHatchCycle s ? s.EggCycles : pk.PersonalInfo.HatchCycles;

            Span<ushort> relearn = stackalloc ushort[4];
            la.GetSuggestedRelearnMoves(relearn, enc);
            pk.SetRelearnMoves(relearn);

            pk.SetSuggestedMoves();

            pk.Move1_PPUps = pk.Move2_PPUps = pk.Move3_PPUps = pk.Move4_PPUps = 0;
            pk.SetMaximumPPCurrent(pk.Moves);
            pk.SetSuggestedHyperTrainingData();
            pk.SetSuggestedRibbons(template, enc, true);
        }

    }
}
