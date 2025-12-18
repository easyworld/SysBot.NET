using System.ComponentModel;
using System.IO;

namespace SysBot.Pokemon;

public class FolderSettings : IDumper
{
    private const string FeatureToggle = "功能开关";
    private const string Files = "文件设置";
    public override string ToString() => "文件夹 / 导出设置";

    [Category(FeatureToggle), Description("启用时，将所有接收到的PKM文件（交易结果）导出到DumpFolder。")]
    public bool Dump { get; set; }

    [Category(Files), Description("源文件夹：用于选择要分发的PKM文件。")]
    public string DistributeFolder { get; set; } = string.Empty;

    [Category(Files), Description("目标文件夹：用于保存所有接收到的PKM文件。")]
    public string DumpFolder { get; set; } = string.Empty;

    public void CreateDefaults(string path)
    {
        var dump = Path.Combine(path, "dump");
        Directory.CreateDirectory(dump);
        DumpFolder = dump;
        Dump = true;

        var distribute = Path.Combine(path, "distribute");
        Directory.CreateDirectory(distribute);
        DistributeFolder = distribute;
    }
}
