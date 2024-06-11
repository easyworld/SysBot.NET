using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;



namespace SysBot.Pokemon.Discord
{
    public class PmDataName
    {
        public string[] Abilities=new string[] { };//特性
        public string[] Forms = new string[] { };//型態
        public string[] Items = new string[] { };//道具
        public string[] Moves = new string[] { };//技能
        public string[] Natures = new string[] { };//性格
        public string[] Species = new string[] { };//種類
        public string[] Types = new string[] { };//屬性
        public string[] Other = new string[] { };
        public string[] Ball = new string[] { };
    }
    public static class PmDataNameDiscord
    {
        public static List<PmDataName> PDName = new List<PmDataName>();//0en 1cht 2chs


        public static string PmConvert(string data)
        {
            string o_data = data;
            bool SpeciesHaveValue=false;
            for (int i=1;i< PDName.Count; i++)
            {
                int SpeciesLength = 0;
                if (!SpeciesHaveValue)
                {
                    for (int j = 0; j < PDName[i].Species.Length; j++)
                    {
                        if (data.Contains("圖鑑" + j+"號"))
                        {
                            o_data = data.Replace("圖鑑" + j + "號", PDName[0].Species[j]);
                            SpeciesHaveValue = true;
                            break;
                        }
                        else if (data.Contains("图鉴" + j+ "号"))
                        {
                            o_data = data.Replace("图鉴" + j + "号", PDName[0].Species[j]);
                            SpeciesHaveValue = true;
                            break;
                        }
                        if (data.Contains(PDName[i].Species[j]))
                        {
                            if (SpeciesLength < PDName[i].Species[j].Length)
                            {
                                SpeciesLength = PDName[i].Species[j].Length;
                                o_data = data.Replace(PDName[i].Species[j], PDName[0].Species[j]);
                            }
                        }
                    }
                }
                
                data = o_data;
                int FormsLength = 0;
                for (int j = 0; j < PDName[i].Forms.Length; j++)
                {
                    if (data.Contains("-"+PDName[i].Forms[j]))
                    {
                        if (FormsLength < PDName[i].Forms[j].Length)
                        {
                            FormsLength = PDName[i].Forms[j].Length;
                            o_data = data.Replace("-" + PDName[i].Forms[j], "-" + PDName[0].Forms[j]);
                        }
                    }
                }
                data = o_data;
                for (int j = 0; j < PDName[i].Moves.Length; j++)
                {
                    if (data.Contains("-"+PDName[i].Moves[j]))
                    {
                        o_data = data.Replace("-" + PDName[i].Moves[j], "-" + PDName[0].Moves[j]);
                        data = o_data;
                    }
                }              
                int ItemsLength = 0;
                for (int j = 0; j < PDName[i].Items.Length; j++)
                {
                    if (data.Contains("@ "+ PDName[i].Items[j]))
                    {
                        if (ItemsLength < PDName[i].Items[j].Length)
                        {
                            ItemsLength = PDName[i].Items[j].Length;
                            o_data = data.Replace("@ " + PDName[i].Items[j], "@ " + PDName[0].Items[j]);
                        }
                    }
                }
                data = o_data;


                for (int j = 0; j < PDName[i].Ball.Length; j++)
                {
                    if (i == 1)
                    {
                        foreach (string c in PDName[i].Ball[j].Split(','))
                        {
                            if (data.Contains(c))
                            {
                                o_data = data.Replace(c, PDName[0].Ball[j]);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (data.Contains(PDName[i].Ball[j]))
                        {
                            o_data = data.Replace(PDName[i].Ball[j], PDName[0].Ball[j]);
                            break;
                        }
                    }
                                       
                }
                data = o_data;



                int AbilitiesLength = 0;
                for (int j = 0; j < PDName[i].Abilities.Length; j++)
                {
                    if (data.Contains(PDName[i].Abilities[j]))
                    {
                        if (AbilitiesLength < PDName[i].Abilities[j].Length)
                        {
                            AbilitiesLength = PDName[i].Abilities[j].Length;
                            o_data = data.Replace(PDName[i].Abilities[j], PDName[0].Abilities[j]);
                        }
                    }
                }
                data = o_data;
                int NaturesLength = 0;
                for (int j = 0; j < PDName[i].Natures.Length; j++)
                {
                    if (data.Contains(PDName[i].Natures[j]))
                    {
                        if (NaturesLength < PDName[i].Natures[j].Length)
                        {
                            NaturesLength = PDName[i].Natures[j].Length;
                            o_data = data.Replace(PDName[i].Natures[j], PDName[0].Natures[j]);
                        }
                    }
                }
                data = o_data;
                int TypesLength = 0;
                for (int j = 0; j < PDName[i].Types.Length; j++)
                {
                    if (data.Contains(": "+PDName[i].Types[j]))
                    {
                        if (TypesLength < PDName[i].Types[j].Length)
                        {
                            TypesLength = PDName[i].Types[j].Length;
                            o_data = data.Replace(": " + PDName[i].Types[j], ": " + PDName[0].Types[j]);
                        }
                    }
                }
                data = o_data;
                for (int j = 0; j < PDName[i].Other.Length; j++)
                {
                    if (data.Contains(PDName[i].Other[j]))
                    {
                        o_data = data.Replace(PDName[i].Other[j], PDName[0].Other[j]);
                        data = o_data;
                    }
                }
                

            }
            return o_data;
        }
    }
}
