using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon;

public class LegalitySettings
{
    private string DefaultTrainerName = "SysBot";
    private const string Generate = "生成设置";
    private const string Misc = "其他设置";
    public override string ToString() => "合法性生成设置";

    // Generate
    [Category(Generate), Description("奇迹卡片的MGDB目录路径。")]
    public string MGDBPath { get; set; } = string.Empty;

    [Category(Generate), Description("包含训练师数据的PKM文件文件夹，用于生成新的PKM文件。")]
    public string GeneratePathTrainerInfo { get; set; } = string.Empty;

    [Category(Generate), Description("与提供的PKM文件不匹配时，PKM文件的默认原训练师名称。")]
    public string GenerateOT
    {
        get => DefaultTrainerName;
        set
        {
            if (!StringsUtil.IsSpammyString(value))
                DefaultTrainerName = value;
        }
    }

    [Category(Generate), Description("与提供的训练师数据文件不匹配时，请求的默认16位训练师ID（TID）。应为5位数字。")]
    public ushort GenerateTID16 { get; set; } = 12345;

    [Category(Generate), Description("与提供的训练师数据文件不匹配时，请求的默认16位秘密ID（SID）。应为5位数字。")]
    public ushort GenerateSID16 { get; set; } = 54321;

    [Category(Generate), Description("与提供的PKM文件不匹配时，PKM文件的默认语言。")]
    public LanguageID GenerateLanguage { get; set; } = LanguageID.English;

    [Category(Generate), Description("生成宝可梦时搜索遭遇的方法。\"NativeOnly\"仅搜索当前游戏对，\"NewestFirst\"从最新游戏开始搜索，\"PriorityOrder\"使用\"GameVersionPriority\"设置中指定的顺序。")]
    public GameVersionPriorityType GameVersionPriority { get; set; } = GameVersionPriorityType.NativeOnly;

    [Category(Generate), Description("指定用于生成遭遇的游戏顺序。将PrioritizeGame设置为\"true\"以启用。")]
    public List<GameVersion> PriorityOrder { get; set; } = Enum.GetValues<GameVersion>().Where(GameUtil.IsValidSavedVersion).Reverse().ToList();

    [Category(Generate), Description("为生成的宝可梦设置所有可能的合法缎带。")]
    public bool SetAllLegalRibbons { get; set; }

    [Category(Generate), Description("为生成的宝可梦设置匹配的精灵球（基于颜色）。")]
    public bool SetMatchingBalls { get; set; } = true;

    [Category(Generate), Description("如果合法，强制使用指定的精灵球。")]
    public bool ForceSpecifiedBall { get; set; } = true;

    [Category(Generate), Description("假设50级的配置是100级的竞技配置。")]
    public bool ForceLevel100for50 { get; set; }

    [Category(Generate), Description("交易必须在Switch游戏之间传输的宝可梦时需要HOME追踪器。")]
    public bool EnableHOMETrackerCheck { get; set; }

    [Category(Generate), Description("尝试宝可梦遭遇类型的顺序。")]
    public List<EncounterTypeGroup> PrioritizeEncounters { get; set; } =
    [
        EncounterTypeGroup.Egg, EncounterTypeGroup.Slot,
        EncounterTypeGroup.Static, EncounterTypeGroup.Mystery,
        EncounterTypeGroup.Trade,
    ];

    [Category(Generate), Description("为支持的游戏（仅限剑盾）添加战斗版本，用于在线竞技对战中使用前代宝可梦。")]
    public bool SetBattleVersion { get; set; }

    [Category(Generate), Description("如果提供非法配置，机器人将创建彩蛋宝可梦。")]
    public bool EnableEasterEggs { get; set; }

    [Category(Generate), Description("允许用户在Showdown Set中提交自定义OT、TID、SID和OT性别。")]
    public bool AllowTrainerDataOverride { get; set; }

    [Category(Generate), Description("允许用户使用批量编辑器命令进行进一步自定义。")]
    public bool AllowBatchCommands { get; set; } = true;

    [Category(Generate), Description("生成配置时取消前的最大秒数。这可以防止困难配置导致机器人冻结。")]
    public int Timeout { get; set; } = 15;

    // Misc

    [Category(Misc), Description("将克隆和用户请求的PKM文件的HOME追踪器归零。建议保持禁用状态，以避免创建无效的HOME数据。")]
    public bool ResetHOMETracker { get; set; } = false;

    [Category(Misc), Description("通过交易伙伴覆盖训练师数据")]
    public bool UseTradePartnerInfo { get; set; } = true;
}
