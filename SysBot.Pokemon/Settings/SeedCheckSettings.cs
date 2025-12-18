using System.ComponentModel;

namespace SysBot.Pokemon;

public class SeedCheckSettings
{
    private const string FeatureToggle = "功能开关";
    public override string ToString() => "种子检查设置";

    [Category(FeatureToggle), Description("启用时，种子检查将返回所有可能的种子结果，而不是第一个有效匹配。")]
    public bool ShowAllZ3Results { get; set; }

    [Category(FeatureToggle), Description("允许仅返回最接近的闪光帧、第一个星星和方块闪光帧，或前三个闪光帧。")]
    public SeedCheckResults ResultDisplayMode { get; set; }
}

public enum SeedCheckResults
{
    ClosestOnly,            // Only gets the first shiny
    FirstStarAndSquare,     // Gets the first star shiny and first square shiny
    FirstThree,             // Gets the first three frames
}
