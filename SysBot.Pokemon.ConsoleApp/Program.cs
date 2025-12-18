using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Z3;
using System;
using System.IO;
using System.Text.Json;

namespace SysBot.Pokemon.ConsoleApp;

public static class Program
{
    private const string ConfigPath = "config.json";

    private static void Main(string[] args)
    {
        Console.WriteLine("正在启动...");
        if (args.Length > 1)
            Console.WriteLine("该程序不支持命令行参数。");

        if (!File.Exists(ConfigPath))
        {
            ExitNoConfig();
            return;
        }

        try
        {
            var lines = File.ReadAllText(ConfigPath);
            var cfg = JsonSerializer.Deserialize(lines, ProgramConfigContext.Default.ProgramConfig) ?? new ProgramConfig();
            PokeTradeBotSWSH.SeedChecker = new Z3SeedSearchHandler<PK8>();
            BotContainer.RunBots(cfg);
        }
        catch (Exception)
        {
            Console.WriteLine("无法使用保存的配置文件启动机器人。请从WinForms项目复制配置或删除它并重新配置。");
            Console.ReadKey();
        }
    }

    private static void ExitNoConfig()
    {
        var bot = new PokeBotState { Connection = new SwitchConnectionConfig { IP = "192.168.0.1", Port = 6000 }, InitialRoutine = PokeRoutineType.FlexTrade };
        var cfg = new ProgramConfig { Bots = [bot] };
        var created = JsonSerializer.Serialize(cfg, ProgramConfigContext.Default.ProgramConfig);
        File.WriteAllText(ConfigPath, created);
        Console.WriteLine("由于程序路径中未找到配置文件，已创建新的配置文件。请配置它并重新启动程序。");
        Console.WriteLine("建议尽可能使用GUI项目（WinForms）来配置此配置文件，因为它将帮助您正确分配值。");
        Console.WriteLine("按任意键退出。");
        Console.ReadKey();
    }
}

public static class BotContainer
{
    private static IPokeBotRunner? Environment;
    private static bool IsRunning => Environment != null;
    private static bool IsStopping;

    public static void RunBots(ProgramConfig prog)
    {
        IPokeBotRunner env = GetRunner(prog);
        foreach (var bot in prog.Bots)
        {
            bot.Initialize();
            if (!AddBot(env, bot, prog.Mode))
                Console.WriteLine($"添加机器人失败: {bot}");
        }

        LogUtil.Forwarders.Add(ConsoleForwarder.Instance);
        env.StartAll();
        Console.WriteLine($"所有机器人启动完成（数量: {prog.Bots.Length}）。");

        Environment = env;
        WaitForExit();
    }

    private static void WaitForExit()
    {
        var msg = Console.IsInputRedirected
            ? "在没有控制台输入的情况下运行。等待退出信号。"
            : "按CTRL-C停止执行。可以最小化此窗口。";
        Console.WriteLine(msg);

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            if (IsStopping)
                return; // Already stopping, don't double stop.
            // Try as best we can to shut down.
            StopProcess("检测到进程退出。正在停止所有机器人。");
        };
        Console.CancelKeyPress += (_, e) =>
        {
            if (IsStopping)
                return; // Already stopping, don't double stop.
            e.Cancel = true; // Gracefully exit after stopping all bots.
            StopProcess("检测到取消键。正在停止所有机器人。");
        };

        while (IsRunning)
            System.Threading.Thread.Sleep(1000);
    }

    private static void StopProcess(string message)
    {
        IsStopping = true;
        Console.WriteLine(message);
        Environment?.StopAll();
        Environment = null;
    }

    private static IPokeBotRunner GetRunner(ProgramConfig prog) => prog.Mode switch
    {
        ProgramMode.SWSH => new PokeBotRunnerImpl<PK8>(prog.Hub, new BotFactory8SWSH()),
        ProgramMode.BDSP => new PokeBotRunnerImpl<PB8>(prog.Hub, new BotFactory8BS()),
        ProgramMode.LA   => new PokeBotRunnerImpl<PA8>(prog.Hub, new BotFactory8LA()),
        ProgramMode.SV   => new PokeBotRunnerImpl<PK9>(prog.Hub, new BotFactory9SV()),
        ProgramMode.LZA  => new PokeBotRunnerImpl<PA9>(prog.Hub, new BotFactory9LZA()),
        _ => throw new IndexOutOfRangeException("Unsupported mode."),
    };

    private static bool AddBot(IPokeBotRunner env, PokeBotState cfg, ProgramMode mode)
    {
        if (!cfg.IsValid())
        {
            Console.WriteLine($"{cfg}的配置无效。");
            return false;
        }

        PokeRoutineExecutorBase newBot;
        try
        {
            newBot = env.CreateBotFromConfig(cfg);
        }
        catch
        {
            Console.WriteLine($"当前模式 ({mode}) 不支持这种类型的机器人 ({cfg.CurrentRoutineType}).");
            return false;
        }
        try
        {
            env.Add(newBot);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

        Console.WriteLine($"已添加: {cfg}: {cfg.InitialRoutine}");
        return true;
    }
}
