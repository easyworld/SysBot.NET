using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon;

public class StopConditionSettings
{
    private const string StopConditions = "停止条件";
    public override string ToString() => "停止条件设置";

    [Category(StopConditions), Description("仅在遇到此宝可梦种类时停止。如果设置为\"None\"则无限制。")]
    public Species StopOnSpecies { get; set; }

    [Category(StopConditions), Description("仅在遇到此FormID的宝可梦时停止。如果留空则无限制。")]
    public int? StopOnForm { get; set; }

    [Category(StopConditions), Description("仅在遇到指定性格的宝可梦时停止。")]
    public Nature TargetNature { get; set; } = Nature.Random;

    [Category(StopConditions), Description("接受的最小IV值，格式为HP/Atk/Def/SpA/SpD/Spe。使用\"x\"表示不检查的IV，使用\"/\"作为分隔符。")]
    public string TargetMinIVs { get; set; } = "";

    [Category(StopConditions), Description("接受的最大IV值，格式为HP/Atk/Def/SpA/SpD/Spe。使用\"x\"表示不检查的IV，使用\"/\"作为分隔符。")]
    public string TargetMaxIVs { get; set; } = "";

    [Category(StopConditions), Description("选择要停止的闪光类型。")]
    public TargetShinyType ShinyTarget { get; set; } = TargetShinyType.DisableOption;

    [Category(StopConditions), Description("允许根据最小或最大尺寸过滤停止条件。")]
    public TargetHeightType HeightTarget { get; set; } = TargetHeightType.DisableOption;

    [Category(StopConditions), Description("仅在遇到有标记的宝可梦时停止。")]
    public bool MarkOnly { get; set; }

    [Category(StopConditions), Description("要忽略的标记列表，用逗号分隔。使用完整名称，例如\"Uncommon Mark, Dawn Mark, Prideful Mark\"。")]
    public string UnwantedMarks { get; set; } = "";

    [Category(StopConditions), Description("当EncounterBot或Fossilbot找到匹配的宝可梦时，按住Capture按钮录制30秒视频。")]
    public bool CaptureVideoClip { get; set; }

    [Category(StopConditions), Description("EncounterBot或Fossilbot找到匹配的宝可梦后，在按下Capture前等待的额外时间（毫秒）。")]
    public int ExtraTimeWaitCaptureVideo { get; set; } = 10000;

    [Category(StopConditions), Description("如果设置为TRUE，则同时匹配ShinyTarget和TargetIVs设置。否则，寻找ShinyTarget或TargetIVs的任一匹配。")]
    public bool MatchShinyAndIV { get; set; } = true;

    [Category(StopConditions), Description("如果不为空，提供的字符串将被添加到结果日志消息前，以提醒您指定的人。对于Discord，使用<@userIDnumber>进行提及。")]
    public string MatchFoundEchoMention { get; set; } = string.Empty;

    public static bool EncounterFound<T>(T pk, int[] targetminIVs, int[] targetmaxIVs, StopConditionSettings settings, IReadOnlyList<string>? marklist) where T : PKM
    {
        // Match Nature and Species if they were specified.
        if (settings.StopOnSpecies != Species.None && settings.StopOnSpecies != (Species)pk.Species)
            return false;

        if (settings.StopOnForm.HasValue && settings.StopOnForm != pk.Form)
            return false;

        if (settings.TargetNature != Nature.Random && settings.TargetNature != pk.Nature)
            return false;

        // Return if it doesn't have a mark, or it has an unwanted mark.
        var unmarked = pk is IRibbonIndex m && !HasMark(m);
        var unwanted = marklist is not null && pk is IRibbonIndex m2 && settings.IsUnwantedMark(GetMarkName(m2), marklist);
        if (settings.MarkOnly && (unmarked || unwanted))
            return false;

        if (settings.ShinyTarget != TargetShinyType.DisableOption)
        {
            bool shinymatch = settings.ShinyTarget switch
            {
                TargetShinyType.AnyShiny => pk.IsShiny,
                TargetShinyType.NonShiny => !pk.IsShiny,
                TargetShinyType.StarOnly => pk.IsShiny && pk.ShinyXor != 0,
                TargetShinyType.SquareOnly => pk.ShinyXor == 0,
                TargetShinyType.DisableOption => true,
                _ => throw new ArgumentException(nameof(TargetShinyType)),
            };

            // If we only needed to match one of the criteria and it shiny match'd, return true.
            // If we needed to match both criteria, and it didn't shiny match, return false.
            if (!settings.MatchShinyAndIV && shinymatch)
                return true;
            if (settings.MatchShinyAndIV && !shinymatch)
                return false;
        }

        if (settings.HeightTarget != TargetHeightType.DisableOption && pk is PK8 p)
        {
            var value = p.HeightScalar;
            bool heightmatch = settings.HeightTarget switch
            {
                TargetHeightType.MinOnly => value is 0,
                TargetHeightType.MaxOnly => value is 255,
                TargetHeightType.MinOrMax => value is 0 or 255,
                _ => throw new ArgumentException(nameof(TargetHeightType)),
            };

            if (!heightmatch)
                return false;
        }

        // Reorder the speed to be last.
        Span<int> pkIVList = stackalloc int[6];
        pk.GetIVs(pkIVList);
        (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);

        for (int i = 0; i < 6; i++)
        {
            if (targetminIVs[i] > pkIVList[i] || targetmaxIVs[i] < pkIVList[i])
                return false;
        }
        return true;
    }

    public static void InitializeTargetIVs(PokeTradeHubConfig config, out int[] min, out int[] max)
    {
        min = ReadTargetIVs(config.StopConditions, true);
        max = ReadTargetIVs(config.StopConditions, false);
    }

    private static int[] ReadTargetIVs(StopConditionSettings settings, bool min)
    {
        int[] targetIVs = new int[6];
        char[] split = ['/'];

        string[] splitIVs = min
            ? settings.TargetMinIVs.Split(split, StringSplitOptions.RemoveEmptyEntries)
            : settings.TargetMaxIVs.Split(split, StringSplitOptions.RemoveEmptyEntries);

        // Only accept up to 6 values.  Fill it in with default values if they don't provide 6.
        // Anything that isn't an integer will be a wild card.
        for (int i = 0; i < 6; i++)
        {
            if (i < splitIVs.Length)
            {
                var str = splitIVs[i];
                if (int.TryParse(str, out var val))
                {
                    targetIVs[i] = val;
                    continue;
                }
            }
            targetIVs[i] = min ? 0 : 31;
        }
        return targetIVs;
    }

    private static bool HasMark(IRibbonIndex pk)
    {
        for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
        {
            if (pk.GetRibbon((int)mark))
                return true;
        }
        return false;
    }

    public static ReadOnlySpan<BattleTemplateToken> TokenOrder =>
    [
        BattleTemplateToken.FirstLine,
        BattleTemplateToken.Shiny,
        BattleTemplateToken.Nature,
        BattleTemplateToken.IVs,
    ];

    public static string GetPrintName(PKM pk)
    {
        const LanguageID lang = LanguageID.English;
        var settings = new BattleTemplateExportSettings(TokenOrder, lang);
        var set = ShowdownParsing.GetShowdownText(pk, settings);

        // Since we can match on Min/Max Height for transfer to future games, display it.
        if (pk is IScaledSize p)
            set += $"\nHeight: {p.HeightScalar}";

        // Add the mark if it has one.
        if (pk is IRibbonIndex r)
        {
            var rstring = GetMarkName(r);
            if (!string.IsNullOrEmpty(rstring))
                set += $"\nPokémon has the **{GetMarkName(r)}**!";
        }
        return set;
    }

    public static void ReadUnwantedMarks(StopConditionSettings settings, out IReadOnlyList<string> marks) =>
        marks = settings.UnwantedMarks.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

    public virtual bool IsUnwantedMark(string mark, IReadOnlyList<string> marklist) => marklist.Contains(mark);

    public static string GetMarkName(IRibbonIndex pk)
    {
        for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
        {
            if (pk.GetRibbon((int)mark))
                return GameInfo.Strings.Ribbons.GetName($"Ribbon{mark}");
        }
        return "";
    }
}

public enum TargetShinyType
{
    DisableOption,  // Doesn't care
    NonShiny,       // Match nonshiny only
    AnyShiny,       // Match any shiny regardless of type
    StarOnly,       // Match star shiny only
    SquareOnly,     // Match square shiny only
}

public enum TargetHeightType
{
    DisableOption,  // Doesn't care
    MinOnly,        // 0 Height only
    MaxOnly,        // 255 Height only
    MinOrMax,       // 0 or 255 Height
}
