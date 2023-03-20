﻿using Mirai.Net.Data.Messages;
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
using System.Reflection;

namespace SysBot.Pokemon.QQ
{
    public class PsModule<T> : IModule where T : PKM, new()
    {
        public bool? IsEnable { get; set; } = true;

        public void Execute(MessageReceiverBase @base)
        {
            var receiver = @base.Concretize<GroupMessageReceiver>();
            QQSettings settings = MiraiQQBot<T>.Settings;

            if (receiver.MessageChain.OfType<AtMessage>().All(x => x.Target != settings.QQ)) return;

            var text = receiver.MessageChain.OfType<PlainMessage>()?.FirstOrDefault()?.Text ?? "";
            if (string.IsNullOrWhiteSpace(text)) return;
            var qq = receiver.Sender.Id;
            var nickName = receiver.Sender.Name;
            var groupId = receiver.GroupId;
            // 中英文判断
            if (IsChinesePS(text))
                ProcessChinesePS(text, qq, nickName, groupId);
            else if (IsPS(text))
                ProcessPS(text, qq, nickName, groupId);
        }

        private void ProcessPS(string text, string qq, string nickName, string groupId)
        {
            LogUtil.LogInfo($"收到ps代码:\n{text}", nameof(PsModule<T>));
            var pss = text.Split("\n\n");
            if (pss.Length > 1) 
                MiraiQQHelper<T>.StartTradeMultiPs(text, qq, nickName, groupId);
            else
                MiraiQQHelper<T>.StartTrade(text, qq, nickName, groupId);
        }

        private void ProcessChinesePS(string text, string qq, string nickName, string groupId)
        {
            LogUtil.LogInfo($"收到中文ps代码:\n{text}", nameof(PsModule<T>));
            var pss = text.Split("+");
            if (pss.Length > 1)
                MiraiQQHelper<T>.StartTradeMultiChinese(text, qq, nickName, groupId);
            else
            {
                string ps = ShowdownTranslator<T>.Chinese2Showdown(text);
                LogUtil.LogInfo($"中文转换后ps代码:\n{ps}", nameof(PsModule<T>));
                MiraiQQHelper<T>.StartTrade(ps, qq, nickName, groupId);
            }
               
        }

        private static bool IsChinesePS(string str)
        {
            var gameStrings = ShowdownTranslator<T>.GameStringsZh;
            for (int i = 1; i < gameStrings.Species.Count; i++)
            {
                if (str.Contains(gameStrings.Species[i]))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsPS(string str)
        {
            var gameStrings = ShowdownTranslator<T>.GameStringsEn;
            for (int i = 1; i < gameStrings.Species.Count; i++)
            {
                if (str.Contains(gameStrings.Species[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
