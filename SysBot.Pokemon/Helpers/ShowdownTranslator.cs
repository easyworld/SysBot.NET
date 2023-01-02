using PKHeX.Core;
using System.Text.RegularExpressions;

namespace SysBot.Pokemon
{
    public class ShowdownTranslator<T> where T : PKM
    {
        public static GameStrings GameStringsZh = GameInfo.GetStrings("zh");
        public static GameStrings GameStringsEn = GameInfo.GetStrings("en");
        public static string Chinese2Showdown(string zh)
        {
            string result = "";

            // 添加宝可梦
            int candidateSpecieNo = 0;
            int candidateSpecieStringLength = 0;
            for (int i = 0; i < GameStringsZh.Species.Count; i++)
            {
                if (zh.Contains(GameStringsZh.Species[i]) && GameStringsZh.Species[i].Length > candidateSpecieStringLength)
                {
                    candidateSpecieNo = i;
                    candidateSpecieStringLength = GameStringsZh.Species[i].Length;
                }
            }

            if (candidateSpecieNo > 0)
            {
                if (candidateSpecieNo == 29) result = "Nidoran-F";
                else if (candidateSpecieNo == 32) result = "Nidoran-M";
                else result += GameStringsEn.Species[candidateSpecieNo];

                zh = zh.Replace(GameStringsZh.Species[candidateSpecieNo], "");

                // 特殊性别差异
                // 29-尼多兰F，32-尼多朗M，678-超能妙喵F，876-爱管侍F，902-幽尾玄鱼F, 916-飘香豚
                if ((candidateSpecieNo is 678 or 876 or 902 or 916) && zh.Contains("母")) result += "-F";
            }
            else
            {
                return result;
            }

            // 识别未知图腾
            if (Regex.IsMatch(zh, "[A-Z?!？！]形态"))
            {
                string formsUnown = Regex.Match(zh, "([A-Z?!？！])形态").Groups?[1]?.Value ?? "";
                if (formsUnown == "？") formsUnown = "?";
                else if (formsUnown == "！") formsUnown = "!";
                result += $"-{formsUnown}";
                zh = Regex.Replace(zh, "[A-Z?!？！]形态", "");
            }

            // 识别地区形态
            if (zh.Contains("帕底亚的样子（火）形态"))
            {
                result += $"-Paldea-Fire";
                zh = zh.Replace("帕底亚的样子（火）形态", "");
            } 
            else if (zh.Contains("帕底亚的样子（水）形态"))
            {
                result += $"-Paldea-Water";
                zh = zh.Replace("帕底亚的样子（水）形态", "");
            } 
            else
            {
                for (int i = 0; i < GameStringsZh.forms.Length; i++)
                {
                    if (GameStringsZh.forms[i].Length == 0) continue;
                    if (!zh.Contains(GameStringsZh.forms[i] + "形态")) continue;
                    result += $"-{GameStringsEn.forms[i]}";
                    zh = zh.Replace(GameStringsZh.forms[i] + "形态", "");
                    break;
                }
            }
            

            // 添加性别
            if (zh.Contains("公"))
            {
                result += " (M)";
                zh = zh.Replace("公", "");
            }
            else if (zh.Contains("母"))
            {
                result += " (F)";
                zh = zh.Replace("母", "");
            }

            // 添加持有物
            if (zh.Contains("持有"))
            {
                for (int i = 0; i < GameStringsZh.Item.Count; i++)
                {
                    if (GameStringsZh.Item[i].Length == 0) continue;
                    if (!zh.Contains("持有" + GameStringsZh.Item[i])) continue;
                    result += $" @ {GameStringsEn.Item[i]}";
                    zh = zh.Replace("持有" + GameStringsZh.Item[i], "");
                    break;
                }
            }
            else if (zh.Contains("携带"))
            {
                for (int i = 0; i < GameStringsZh.Item.Count; i++)
                {
                    if (GameStringsZh.Item[i].Length == 0) continue;
                    if (!zh.Contains("携带" + GameStringsZh.Item[i])) continue;
                    result += $" @ {GameStringsEn.Item[i]}";
                    zh = zh.Replace("携带" + GameStringsZh.Item[i], "");
                    break;
                }
            }

            // 添加等级
            if (Regex.IsMatch(zh, "\\d{1,3}级"))
            {
                string level = Regex.Match(zh, "(\\d{1,3})级").Groups?[1]?.Value ?? "100";
                result += $"\nLevel: {level}";
                zh = Regex.Replace(zh, "\\d{1,3}级", "");
            }

            // 添加超极巨化
            if (typeof(T) == typeof(PK8) && zh.Contains("超极巨"))
            {
                result += "\nGigantamax: Yes";
                zh = zh.Replace("超极巨", "");
            }

            // 添加异色
            if (zh.Contains("异色"))
            {
                result += "\nShiny: Yes";
                zh = zh.Replace("异色", "");
            }
            else if (zh.Contains("闪光"))
            {
                result += "\nShiny: Yes";
                zh = zh.Replace("闪光", "");
            }
            else if (zh.Contains("星闪"))
            {
                result += "\nShiny: Star";
                zh = zh.Replace("星闪", "");
            }
            else if (zh.Contains("方闪"))
            {
                result += "\nShiny: Square";
                zh = zh.Replace("方闪", "");
            }

            // 添加头目
            if (typeof(T) == typeof(PA8) && zh.Contains("头目"))
            {
                result += "\nAlpha: Yes";
                zh = zh.Replace("头目", "");
            }

            // 添加球种
            for (int i = 0; i < GameStringsZh.balllist.Length; i++)
            {
                if (GameStringsZh.balllist[i].Length == 0) continue;
                if (!zh.Contains(GameStringsZh.balllist[i])) continue;
                var ballStr = GameStringsEn.balllist[i];
                if (typeof(T) == typeof(PA8) && ballStr is "Poké Ball" or "Great Ball" or "Ultra Ball") ballStr = "LA" + ballStr;
                result += $"\nBall: {ballStr}";
                zh = zh.Replace(GameStringsZh.balllist[i], "");
                break;
            }

            // 添加特性
            for (int i = 0; i < GameStringsZh.Ability.Count; i++)
            {
                if (GameStringsZh.Ability[i].Length == 0) continue;
                if (!zh.Contains(GameStringsZh.Ability[i] + "特性")) continue;
                result += $"\nAbility: {GameStringsEn.Ability[i]}";
                zh = zh.Replace(GameStringsZh.Ability[i] + "特性", "");
                break;
            }

            // 添加性格
            for (int i = 0; i < GameStringsZh.Natures.Count; i++)
            {
                if (GameStringsZh.Natures[i].Length == 0) continue;
                if (!zh.Contains(GameStringsZh.Natures[i])) continue;
                result += $"\n{GameStringsEn.Natures[i]} Nature";
                zh = zh.Replace(GameStringsZh.Natures[i], "");
                break;
            }

            // 添加个体值
            if (zh.ToUpper().Contains("6V"))
            {
                result += "\nIVs: 31 HP / 31 Atk / 31 Def / 31 SpA / 31 SpD / 31 Spe";
                zh = zh.Replace("6V", "");
            }
            else if (zh.ToUpper().Contains("5V0A"))
            {
                result += "\nIVs: 31 HP / 0 Atk / 31 Def / 31 SpA / 31 SpD / 31 Spe";
                zh = zh.Replace("5V0A", "");
            }
            else if (zh.ToUpper().Contains("5V0S"))
            {
                result += "\nIVs: 31 HP / 31 Atk / 31 Def / 31 SpA / 31 SpD / 0 Spe";
                zh = zh.Replace("5V0S", "");
            }
            else if (zh.ToUpper().Contains("4V0A0S"))
            {
                result += "\nIVs: 31 HP / 0 Atk / 31 Def / 31 SpA / 31 SpD / 0 Spe";
                zh = zh.Replace("4V0A0S", "");
            }

            // 添加努力值
            if (zh.Contains("努力值"))
            {
                result += "\nEVs: ";
                zh = zh.Replace("努力值", "");
                if (Regex.IsMatch(zh, "\\d{1,3}生命"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})生命").Groups?[1]?.Value ?? "";
                    result += $"{value} HP / ";
                    zh = Regex.Replace(zh, "\\d{1,3}生命", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}Hp"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})Hp").Groups?[1]?.Value ?? "";
                    result += $"{value} HP / ";
                    zh = Regex.Replace(zh, "\\d{1,3}Hp", "");
                }

                if (Regex.IsMatch(zh, "\\d{1,3}攻击"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})攻击").Groups?[1]?.Value ?? "";
                    result += $"{value} Atk / ";
                    zh = Regex.Replace(zh, "\\d{1,3}攻击", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}Atk"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})Atk").Groups?[1]?.Value ?? "";
                    result += $"{value} Atk / ";
                    zh = Regex.Replace(zh, "\\d{1,3}Atk", "");
                }

                if (Regex.IsMatch(zh, "\\d{1,3}防御"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})防御").Groups?[1]?.Value ?? "";
                    result += $"{value} Def / ";
                    zh = Regex.Replace(zh, "\\d{1,3}防御", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}Def"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})Def").Groups?[1]?.Value ?? "";
                    result += $"{value} Def / ";
                    zh = Regex.Replace(zh, "\\d{1,3}Def", "");
                }

                if (Regex.IsMatch(zh, "\\d{1,3}特攻"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})特攻").Groups?[1]?.Value ?? "";
                    result += $"{value} SpA / ";
                    zh = Regex.Replace(zh, "\\d{1,3}特攻", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}SpA"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})SpA").Groups?[1]?.Value ?? "";
                    result += $"{value} SpA / ";
                    zh = Regex.Replace(zh, "\\d{1,3}SpA", "");
                }

                if (Regex.IsMatch(zh, "\\d{1,3}特防"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})特防").Groups?[1]?.Value ?? "";
                    result += $"{value} SpD / ";
                    zh = Regex.Replace(zh, "\\d{1,3}特防", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}SpD"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})SpD").Groups?[1]?.Value ?? "";
                    result += $"{value} SpD / ";
                    zh = Regex.Replace(zh, "\\d{1,3}SpD", "");
                }
                if (Regex.IsMatch(zh, "\\d{1,3}速度"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})速度").Groups?[1]?.Value ?? "";
                    result += $"{value} Spe";
                    zh = Regex.Replace(zh, "\\d{1,3}速度", "");
                }
                else if (Regex.IsMatch(zh, "\\d{1,3}Spe"))
                {
                    string value = Regex.Match(zh, "(\\d{1,3})Spe").Groups?[1]?.Value ?? "";
                    result += $"{value} Spe";
                    zh = Regex.Replace(zh, "\\d{1,3}Spe", "");
                }
                if (result.EndsWith("/ "))
                {
                    result = result.Substring(0, result.Length - 2);
                }
            }

            // 添加太晶属性
            if (typeof(T) == typeof(PK9))
            {
                for (int i = 0; i < GameStringsZh.Types.Count; i++)
                {
                    if (GameStringsZh.Types[i].Length == 0) continue;
                    if (!zh.Contains("太晶" + GameStringsZh.Types[i])) continue;
                    result += $"\nTera Type: {GameStringsEn.Types[i]}";
                    zh = zh.Replace("太晶" + GameStringsZh.Types[i], "");
                    break;
                }
            }

            // 添加体型
            if (Regex.IsMatch(zh, "\\d{1,3}身高"))
            {
                string value = Regex.Match(zh, "(\\d{1,3})身高").Groups?[1]?.Value ?? "";
                result += $"\n.HeightScalar= {value}";
                zh = Regex.Replace(zh, "\\d{1,3}身高", "");
            }
            else if (Regex.IsMatch(zh, "\\d{1,3}HeightScalar"))
            {
                string value = Regex.Match(zh, "(\\d{1,3})HeightScalar").Groups?[1]?.Value ?? "";
                result += $"\n.HeightScalar= {value}";
                zh = Regex.Replace(zh, "\\d{1,3}HeightScalar", "");
            }
            if (Regex.IsMatch(zh, "\\d{1,3}体重"))
            {
                string value = Regex.Match(zh, "(\\d{1,3})体重").Groups?[1]?.Value ?? "";
                result += $"\n.WeightScalar= {value}";
                zh = Regex.Replace(zh, "\\d{1,3}体重", "");
            }
            else if (Regex.IsMatch(zh, "\\d{1,3}WeightScalar"))
            {
                string value = Regex.Match(zh, "(\\d{1,3})WeightScalar").Groups?[1]?.Value ?? "";
                result += $"\n.WeightScalar= {value}";
                zh = Regex.Replace(zh, "\\d{1,3}WeightScalar", "");
            }
            if (Regex.IsMatch(zh, "\\d{1,3}大小"))
            {
                string value = Regex.Match(zh, "(\\d{1,3})大小").Groups?[1]?.Value ?? "";
                result += $"\n.Scale= {value}";
                zh = Regex.Replace(zh, "\\d{1,3}大小", "");
            }
            else if (Regex.IsMatch(zh, "\\d{1,3}Scale"))
            {
                string value = Regex.Match(zh, "(\\d{1,3})Scale").Groups?[1]?.Value ?? "";
                result += $"\n.Scale= {value}";
                zh = Regex.Replace(zh, "\\d{1,3}WeScale", "");
            }

            // 添加技能
            zh += "-";
            for (int moveCount = 0; moveCount < 4; moveCount++)
            {
                for (int i = 0; i < GameStringsZh.Move.Count; i++)
                {
                    if (GameStringsZh.Move[i].Length == 0) continue;
                    if (!zh.Contains("-" + GameStringsZh.Move[i] + "-")) continue;
                    result += $"\n-{GameStringsEn.Move[i]}";
                    zh = zh.Replace("-" + GameStringsZh.Move[i], "");
                    break;
                }
            }
            zh = zh.Replace("-", "");

            return result;
        }
    }
}