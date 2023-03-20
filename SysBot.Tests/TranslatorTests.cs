using FluentAssertions;
using PKHeX.Core;
using SysBot.Pokemon;
using System.Diagnostics;
using Xunit;

namespace SysBot.Tests
{
    public class TranslatorTests
    {
        static TranslatorTests() => AutoLegalityWrapper.EnsureInitialized(new LegalitySettings());

        [Theory]
        [InlineData("公肯泰罗帕底亚的样子（火）形态", "Tauros-Paldea-Fire (M)")]
        public void TestForm(string input, string output)
        {
            var result = ShowdownTranslator<PK9>.Chinese2Showdown(input);
            result.Should().Be(output);
        }

        [Theory]
        [InlineData("皮卡丘")]
        [InlineData("木木枭")]
        public void TestLegal(string input)
        {
            var setstring = ShowdownTranslator<PK9>.Chinese2Showdown(input);
            var set = ShowdownUtil.ConvertToShowdown(setstring);
            set.Should().NotBeNull();
            var template = AutoLegalityWrapper.GetTemplate(set);
            template.Species.Should().BeGreaterThan(0);
            var sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();
            var pkm = sav.GetLegal(template, out var result);
            Trace.WriteLine(result.ToString());

            pkm.CanBeTraded().Should().BeTrue();
            (pkm is PK9).Should().BeTrue();
            var valid = new LegalityAnalysis(pkm).Valid;
            valid.Should().BeTrue();
        }

    }

}
