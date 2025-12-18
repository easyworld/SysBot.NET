using System;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace SysBot.Pokemon.Tools;

/// <summary>
/// LZA指针寻找工具的命令行入口
/// </summary>
public static class PointerFinderProgram
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== LZA游戏指针寻找工具 ===");
        Console.WriteLine("用于查找和更新LZA游戏的RAM偏移量");
        Console.WriteLine();
        
        // 解析命令行参数
        var config = ParseArgs(args);
        
        // 创建Switch连接
        var connection = CreateSwitchConnection(config);
        
        // 创建取消令牌
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        
        // 注册Ctrl+C处理
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("\n正在退出...");
            cts.Cancel();
            e.Cancel = true;
        };
        
        try
        {
            // 创建指针寻找工具实例
            var finder = new PointerFinderLZA(connection, token);
            
            // 初始化并验证连接
            if (!await finder.InitializeAsync().ConfigureAwait(false))
            {
                Console.WriteLine("初始化失败，程序将退出。");
                return;
            }
            
            Console.WriteLine();
            Console.WriteLine("初始化成功！");
            
            // 主菜单
            await ShowMainMenuAsync(finder, cts).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序出错: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            // 关闭连接
            if (connection is IDisposable disposable)
                disposable.Dispose();
        }
        
        Console.WriteLine("程序已退出。");
        Console.ReadKey();
    }
    
    /// <summary>
    /// 解析命令行参数
    /// </summary>
    /// <param name="args">命令行参数</param>
    /// <returns>配置信息</returns>
    private static FinderConfig ParseArgs(string[] args)
    {
        var config = new FinderConfig
        {
            IpAddress = "192.168.1.100",
            Port = 6000,
        };
        
        // 简单的参数解析
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg.ToLower())
            {
                case "--ip":
                case "-i":
                    if (i + 1 < args.Length)
                        config.IpAddress = args[++i];
                    break;
                case "--port":
                case "-p":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int port))
                        config.Port = port;
                    break;
            }
        }
        
        return config;
    }
    
    /// <summary>
    /// 创建Switch连接
    /// </summary>
    /// <param name="config">配置信息</param>
    /// <returns>Switch连接</returns>
    private static ISwitchConnectionAsync CreateSwitchConnection(FinderConfig config)
    {
        var switchConfig = new SwitchConnectionConfig
        {
            IP = config.IpAddress,
            Port = config.Port,
        };
        
        Console.WriteLine($"正在创建到 {config.IpAddress}:{config.Port} 的连接...");
        return SwitchSocketAsync.CreateInstance(switchConfig);
    }
    
    /// <summary>
    /// 显示主菜单
    /// </summary>
    /// <param name="finder">指针寻找工具实例</param>
    /// <param name="cts">取消令牌源</param>
    private static async Task ShowMainMenuAsync(PointerFinderLZA finder, CancellationTokenSource cts)
    {
        while (!cts.Token.IsCancellationRequested)
        {
            Console.WriteLine();
            Console.WriteLine("请选择要执行的操作:");
            Console.WriteLine("1. 搜索宝可梦盒子指针");
            Console.WriteLine("2. 搜索玩家状态指针");
            Console.WriteLine("3. 搜索交换相关指针");
            Console.WriteLine("4. 搜索所有指针");
            Console.WriteLine("5. 更新偏移量文件");
            Console.WriteLine("0. 退出程序");
            Console.WriteLine();
            Console.Write("请输入选项: ");
            
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;
            
            switch (input.Trim())
            {
                case "1":
                    await finder.FindBoxPokemonPointerAsync().ConfigureAwait(false);
                    break;
                case "2":
                    await finder.FindMyStatusPointerAsync().ConfigureAwait(false);
                    break;
                case "3":
                    await finder.FindTradePointersAsync().ConfigureAwait(false);
                    break;
                case "4":
                    await FindAllPointersAsync(finder).ConfigureAwait(false);
                    break;
                case "5":
                    // 实现更新文件的逻辑
                    await UpdateOffsetsFileAsync(finder).ConfigureAwait(false);
                    break;
                case "0":
                    cts.Cancel();
                    return;
                default:
                    Console.WriteLine("无效的选项，请重新输入。");
                    break;
            }
        }
    }
    
    /// <summary>
    /// 搜索所有指针
    /// </summary>
    /// <param name="finder">指针寻找工具实例</param>
    private static async Task FindAllPointersAsync(PointerFinderLZA finder)
    {
        Console.WriteLine("正在搜索所有指针...");
        
        await finder.FindBoxPokemonPointerAsync().ConfigureAwait(false);
        await finder.FindMyStatusPointerAsync().ConfigureAwait(false);
        await finder.FindTradePointersAsync().ConfigureAwait(false);
        
        Console.WriteLine("所有指针搜索完成！");
    }
    
    /// <summary>
    /// 更新偏移量文件
    /// </summary>
    /// <param name="finder">指针寻找工具实例</param>
    private static async Task UpdateOffsetsFileAsync(PointerFinderLZA finder)
    {
        Console.WriteLine("正在更新偏移量文件...");
        
        // 搜索所有指针
        var boxPointer = await finder.FindBoxPokemonPointerAsync().ConfigureAwait(false);
        var statusPointer = await finder.FindMyStatusPointerAsync().ConfigureAwait(false);
        var tradePointers = await finder.FindTradePointersAsync().ConfigureAwait(false);
        
        var offsets = new Dictionary<string, List<long>>();
        
        if (boxPointer != null)
            offsets["BoxStartPokemonPointer"] = boxPointer;
        
        if (statusPointer != null)
            offsets["MyStatusPointer"] = statusPointer;
        
        if (tradePointers != null)
        {
            foreach (var (name, pointer) in tradePointers)
                offsets[name] = pointer;
        }
        
        if (offsets.Count == 0)
        {
            Console.WriteLine("没有找到任何指针，无法更新文件");
            return;
        }
        
        // 确认是否更新
        Console.WriteLine($"\n找到 {offsets.Count} 个指针，是否更新偏移量文件？(y/n)");
        var input = Console.ReadLine();
        if (input?.Trim().ToLower() != "y")
        {
            Console.WriteLine("取消更新");
            return;
        }
        
        // 更新文件
        if (finder.UpdateOffsetsFile(offsets))
        {
            Console.WriteLine("偏移量文件更新成功！");
        }
        else
        {
            Console.WriteLine("偏移量文件更新失败");
        }
    }
    
    /// <summary>
    /// 指针寻找工具的配置信息
    /// </summary>
    private class FinderConfig
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 6000;
    }
}