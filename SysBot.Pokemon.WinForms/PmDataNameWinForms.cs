using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SysBot.Pokemon.Discord;


namespace SysBot.Pokemon.WinForms
{

    public static class PmDataNameWinForms
    {       
        public static string[] text_Species_zh = new string[] { };
        public static string[] text_Species_zh2 = new string[] { };
        public static string[] text_Species_ko = new string[] { };
        public static string[] text_Species_ja = new string[] { };
        public static string[] text_Species_it = new string[] { };
        public static string[] text_Species_fr = new string[] { };
        public static string[] text_Species_es = new string[] { };
        public static string[] text_Species_en = new string[] { };
        public static string[] text_Species_de = new string[] { };
        private static string[] textName = new string[]
        {
            "_en.txt",
            "_zh2.txt",
            "_zh.txt",
            "_ko.txt",
            "_ja.txt",
            "_it.txt",
            "_fr.txt",
            "_es.txt",
            "_de.txt"
        };
        private static string[] textDataName = new string[]
        {
            "Abilities",
            "Forms",
            "Items",
            "Moves",
            "Natures",
            "Species",
            "Types",
            "Other",
            "Ball"
        };

        public static void initialization()
        {
            string FilePath = Application.StartupPath + "\\localization\\";

            for(int i = 0; i < 3; i++)
            {
                PmDataName pdn = new PmDataName();
                foreach (string s in textDataName)
                {
                    StreamReader sr = new StreamReader(FilePath + s + "\\text_"+ s + textName[i]);
                    switch (s)
                    {
                        case "Abilities":
                            pdn.Abilities= sr.ReadToEnd().Replace("\r", "").Split('\n');
                            break;
                        case "Forms":
                            pdn.Forms = sr.ReadToEnd().Replace("\r", "").Split('\n');
                            break;
                        case "Items":
                            pdn.Items = sr.ReadToEnd().Replace("\r", "").Split('\n');
                            break;
                        case "Moves":
                            pdn.Moves = sr.ReadToEnd().Replace("\r", "").Split('\n');
                            break;
                        case "Natures":
                            pdn.Natures = sr.ReadToEnd().Replace("\r", "").Split('\n');
                            break;
                        case "Species":
                            pdn.Species = sr.ReadToEnd().Replace("\r", "").Split('\n');
                            break;
                        case "Types":
                            pdn.Types = sr.ReadToEnd().Replace("\r", "").Split('\n');
                            break;
                        case "Other":
                            pdn.Other = sr.ReadToEnd().Replace("\r","").Split('\n');
                            break;
                        case "Ball":
                            pdn.Ball = sr.ReadToEnd().Replace("\r", "").Split('\n');
                            break;
                    }
                    sr.Close();
                }
                PmDataNameDiscord.PDName.Add(pdn);
            }       
        }
    }
}
