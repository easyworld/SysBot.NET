using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon
{
    public class PkmCalculation
    {
        public static uint GetPK9_Terastal_PID(uint seed, uint id32)
        {
            var rand = new Xoroshiro128Plus(seed);
            var ec = (uint)rand.NextInt(uint.MaxValue);
            var fakeTID = (uint)rand.NextInt();
            var pid = (uint)rand.NextInt();

            var xor = ShinyUtil.GetShinyXor(pid, fakeTID);
            if (xor < 16)
            {
                if (xor != 0) xor = 1;
                ShinyUtil.ForceShinyState(true, ref pid, id32, xor);
            }
            else
            {
                ShinyUtil.ForceShinyState(false, ref pid, id32, xor);
            }

            return pid;
        }
        public static void ClearOTTrash(PKM pokemon, string tradePartnerName)
        {
            Span<byte> trash = pokemon.OriginalTrainerTrash;
            trash.Clear();
            string name = tradePartnerName;
            int maxLength = trash.Length / 2;
            int actualLength = Math.Min(name.Length, maxLength);
            for (int i = 0; i < actualLength; i++)
            {
                char value = name[i];
                trash[i * 2] = (byte)value;
                trash[i * 2 + 1] = (byte)(value >> 8);
            }
            if (actualLength < maxLength)
            {
                trash[actualLength * 2] = 0x00;
                trash[actualLength * 2 + 1] = 0x00;
            }
        }
    }
}
