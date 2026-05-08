using FluentAssertions;
using PKHeX.Core;
using SysBot.Pokemon;
using SysBot.Pokemon.Helpers;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace SysBot.Tests;

public class TranslatorTests
{
    static TranslatorTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Theory]
    [InlineData("公肯泰罗携带大师球6V异色努力值252生命全招式异国-泰山压顶", "Tauros (M) @ Master Ball\nShiny: Yes\nIVs: 31 HP / 31 Atk / 31 Def / 31 SpA / 31 SpD / 31 Spe\nEVs: 252 HP \n.RelearnMoves=$suggestAll\nLanguage: Italian\n-Body Slam")]
    public void TestTrans(string input, string output)
    {
        var result = ShowdownTranslator<PK9>.Chinese2Showdown(input);
        result.Should().Be(output);
    }

    [Theory]
    [InlineData("异色月亮球喷火龙")]
    public void TestLegalSWSH(string input)
    {
        var setstring = input;
        if (!ShowdownTranslator<PK8>.IsPS(input))
        {
            setstring = ShowdownTranslator<PK8>.Chinese2Showdown(input);
        }

        var set = ShowdownUtil.ConvertToShowdown(setstring);
        set.Should().NotBeNull();
        var template = AutoLegalityWrapper.GetTemplate(set);
        template.Species.Should().BeGreaterThan(0);
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        var pkm = sav.GetLegal(template, out var result);
        Trace.WriteLine(result.ToString());

        if (pkm.Nickname.ToLower() == "egg" && Breeding.CanHatchAsEgg(pkm.Species)) AbstractTrade<PK8>.EggTrade(pkm, template);

        (pkm is PK8).Should().BeTrue();
        var la = new LegalityAnalysis(pkm);
        if (!la.Valid)
            Trace.WriteLine(la.Report());
        pkm.CanBeTraded(la.EncounterOriginal).Should().BeTrue();
        la.Valid.Should().BeTrue();
        pkm.IsShiny.Should().BeTrue();
    }

    [Theory]
    [InlineData("皮卡丘")]
    [InlineData("木木枭")]
    [InlineData("彩粉蝶-冰雪花纹")]
    [InlineData("公小火龙的蛋")]
    [InlineData("大剑鬼")]
    [InlineData("火暴兽")]
    [InlineData("千面避役")]
    [InlineData("泪眼蜥")]
    [InlineData("布里卡隆")]
    [InlineData("异色烈空坐")]
    [InlineData("等级球呆火鳄")]
    [InlineData("异色古剑豹")]
    [InlineData("异色故勒顿")]
    [InlineData("皮卡丘 大师球 胆小 全技能")]
    [InlineData("Sinistcha-Masterpiece @ Master Ball\r\nShiny: Yes\r\nAbility: Hospitality\r\nHardy Nature\r\nEVs: 0 HP / 0 Atk / 0 Def / 0 SpA / 0 SpD / 0 Spe \r\nIVs: 31 HP / 31 Atk / 31 Def / 31 SpA / 31 SpD / 31 Spe \r\nLanguage: ChineseS\r\n")]
    public void TestSVLegal(string input)
    {
        var setstring = input;
        if (!ShowdownTranslator<PK9>.IsPS(input))
        {
            setstring = ShowdownTranslator<PK9>.Chinese2Showdown(input);
        }
        
        var set = ShowdownUtil.ConvertToShowdown(setstring);
        set.Should().NotBeNull();
        var template = AutoLegalityWrapper.GetTemplate(set);
        template.Species.Should().BeGreaterThan(0);
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK9>();
        var pkm = sav.GetLegal(template, out var result);
        Trace.WriteLine(result.ToString());

        if (pkm.Nickname.ToLower() == "egg" && Breeding.CanHatchAsEgg(pkm.Species)) AbstractTrade<PK9>.EggTrade(pkm, template);

        
        (pkm is PK9).Should().BeTrue();
        var la = new LegalityAnalysis(pkm);
        if (!la.Valid)
            Trace.WriteLine(la.Report());
        pkm.CanBeTraded(la.EncounterOriginal).Should().BeTrue();
        la.Valid.Should().BeTrue();
    }

    [Theory]
    [InlineData("梦境球皮卡丘")]
    [InlineData("异色波尔凯尼恩")]
    public void TestLegalZA(string input)
    {
        var setstring = ShowdownTranslator<PA9>.Chinese2Showdown(input);
        var set = ShowdownUtil.ConvertToShowdown(setstring);
        set.Should().NotBeNull();
        var template = AutoLegalityWrapper.GetTemplate(set);
        template.Species.Should().BeGreaterThan(0);
        var sav = AutoLegalityWrapper.GetTrainerInfo<PA9>();
        var pkm = sav.GetLegal(template, out var result);
        Trace.WriteLine(result.ToString());

        if (pkm.Nickname.ToLower() == "egg" && Breeding.CanHatchAsEgg(pkm.Species)) AbstractTrade<PA9>.EggTrade(pkm, template);

        (pkm is PA9).Should().BeTrue();
        var la = new LegalityAnalysis(pkm);
        if (!la.Valid)
            Trace.WriteLine(la.Report());
        pkm.CanBeTraded(la.EncounterOriginal).Should().BeTrue();
        la.Valid.Should().BeTrue();
        if (input.Contains("异色"))
            pkm.IsShiny.Should().BeTrue();
    }

    [Theory]
    [InlineData("C:\\Users\\easyworld\\Downloads\\boxdata Box 2.bin")]
    public void TestLegalZAFile(string file)
    {
        var bytes = File.ReadAllBytes(file);
        FileTradeHelper<PA9>.Bin2List(bytes).ForEach(pkm =>
        {
            pkm.Should().NotBeNull();
            (pkm is PA9).Should().BeTrue();
            var la = new LegalityAnalysis(pkm);
            if (!la.Valid)
                Trace.WriteLine(la.Report());
            pkm.CanBeTraded(la.EncounterOriginal).Should().BeTrue();
            la.Valid.Should().BeTrue();
        });
    }
}
