using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace SysBot.Pokemon.Tools;

/// <summary>
/// 用于查找LZA游戏RAM偏移量的工具类
/// </summary>
public class PointerFinderLZA
{
    private readonly ISwitchConnectionAsync _connection;
    private readonly CancellationToken _token;
    
    /// <summary>
    /// LZA游戏的标题ID
    /// </summary>
    public const string TargetTitleID = "0100F43008C44000";
    
    /// <summary>
    /// 宝可梦数据大小（字节）
    /// </summary>
    public const int PokemonDataSize = 0x148;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connection">Switch连接</param>
    /// <param name="token">取消令牌</param>
    public PointerFinderLZA(ISwitchConnectionAsync connection, CancellationToken token)
    {
        _connection = connection;
        _token = token;
    }
    
    /// <summary>
    /// 查找项目根目录
    /// </summary>
    /// <param name="startDir">起始目录</param>
    /// <returns>项目根目录路径，如果未找到则返回null</returns>
    private string? FindProjectRoot(string startDir)
    {
        var currentDir = new DirectoryInfo(startDir);
        
        // 最多向上查找10层目录
        for (int i = 0; i < 10; i++)
        {
            if (currentDir == null)
                break;
            
            // 检查是否存在解决方案文件
            var solutionFiles = currentDir.GetFiles("*.sln");
            if (solutionFiles.Length > 0)
                return currentDir.FullName;
            
            // 检查是否存在SysBot.Pokemon目录
            var pokemonDir = Path.Combine(currentDir.FullName, "SysBot.Pokemon");
            if (Directory.Exists(pokemonDir))
                return currentDir.FullName;
            
            // 检查是否存在SysBot.Base目录
            var baseDir = Path.Combine(currentDir.FullName, "SysBot.Base");
            if (Directory.Exists(baseDir))
                return currentDir.FullName;
            
            // 继续向上查找
            currentDir = currentDir.Parent;
        }
        
        return null;
    }
    
    /// <summary>
    /// 初始化并验证连接
    /// </summary>
    /// <returns>是否成功初始化</returns>
    public async Task<bool> InitializeAsync()
    {
        Console.WriteLine("正在连接到Switch...");
        
        try
        {
            // 检查连接状态
            var titleId = await _connection.GetTitleID(_token).ConfigureAwait(false);
            Console.WriteLine($"当前运行的游戏: {titleId}");
            
            if (titleId != TargetTitleID)
            {
                Console.WriteLine($"错误: 未检测到LZA游戏。期望: {TargetTitleID}");
                return false;
            }
            
            var gameVersion = await _connection.GetGameInfo("version", _token).ConfigureAwait(false);
            Console.WriteLine($"游戏版本: {gameVersion}");
            
            var botbaseVersion = await _connection.GetBotbaseVersion(_token).ConfigureAwait(false);
            Console.WriteLine($"sys-botbase版本: {botbaseVersion}");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"连接失败: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 搜索宝可梦盒子数据指针
    /// </summary>
    /// <returns>找到的指针路径</returns>
    public async Task<List<long>?> FindBoxPokemonPointerAsync()
    {
        Console.WriteLine("正在搜索宝可梦盒子数据指针...");
        
        // 生成宝可梦数据特征
        var signature = GeneratePokemonSignature();
        
        // 搜索内存中的宝可梦数据
        var pokemonAddresses = await SearchMemoryAsync(signature, 5).ConfigureAwait(false);
        
        if (pokemonAddresses.Count == 0)
        {
            Console.WriteLine("未找到宝可梦数据");
            return null;
        }
        
        Console.WriteLine($"找到 {pokemonAddresses.Count} 个潜在的宝可梦数据地址:");
        foreach (var addr in pokemonAddresses)
            Console.WriteLine($"  0x{addr:X16}");
        
        // 选择第一个地址进行指针追踪
        var targetAddress = pokemonAddresses[0];
        Console.WriteLine($"\n正在追踪指针路径: 0x{targetAddress:X16}");
        
        // 追踪指针路径
        var pointerPath = await TracePointerPathAsync(targetAddress).ConfigureAwait(false);
        
        if (pointerPath != null)
        {
            Console.WriteLine($"找到指针路径: [{string.Join(", ", pointerPath.Select(p => $"0x{p:X}"))}]");
        }
        
        return pointerPath;
    }
    
    /// <summary>
    /// 搜索玩家状态指针
    /// </summary>
    /// <returns>找到的指针路径</returns>
    public async Task<List<long>?> FindMyStatusPointerAsync()
    {
        Console.WriteLine("正在搜索玩家状态指针...");
        
        // 玩家状态数据特征：通常包含玩家ID、性别等信息
        var signature = new byte[8];
        signature[0] = 0x01; // 玩家ID示例
        signature[1] = 0x00;
        signature[2] = 0x00;
        signature[3] = 0x00;
        signature[4] = 0x01; // 性别示例：男
        
        // 搜索内存中的玩家状态数据
        var statusAddresses = await SearchMemoryAsync(signature, 5).ConfigureAwait(false);
        
        if (statusAddresses.Count == 0)
        {
            Console.WriteLine("未找到玩家状态数据");
            return null;
        }
        
        Console.WriteLine($"找到 {statusAddresses.Count} 个潜在的玩家状态地址:");
        foreach (var addr in statusAddresses)
            Console.WriteLine($"  0x{addr:X16}");
        
        // 选择第一个地址进行指针追踪
        var targetAddress = statusAddresses[0];
        Console.WriteLine($"\n正在追踪指针路径: 0x{targetAddress:X16}");
        
        // 追踪指针路径
        var pointerPath = await TracePointerPathAsync(targetAddress).ConfigureAwait(false);
        
        if (pointerPath != null)
        {
            Console.WriteLine($"找到玩家状态指针路径: [{string.Join(", ", pointerPath.Select(p => $"0x{p:X}"))}]");
        }
        
        return pointerPath;
    }
    
    /// <summary>
    /// 搜索链接交换相关指针
    /// </summary>
    /// <returns>找到的指针路径字典</returns>
    public async Task<Dictionary<string, List<long>>?> FindTradePointersAsync()
    {
        Console.WriteLine("正在搜索链接交换相关指针...");
        var results = new Dictionary<string, List<long>>();
        
        try
        {
            // 搜索交换伙伴宝可梦数据指针
            Console.WriteLine("\n搜索交换伙伴宝可梦数据指针...");
            var partnerPokemonAddr = await SearchTradePartnerPokemonAsync().ConfigureAwait(false);
            if (partnerPokemonAddr != 0)
            {
                var pointerPath = await TracePointerPathAsync(partnerPokemonAddr).ConfigureAwait(false);
                if (pointerPath != null)
                {
                    results["LinkTradePartnerPokemonPointer"] = pointerPath;
                }
            }
            
            // 搜索交换伙伴状态指针
            Console.WriteLine("\n搜索交换伙伴状态指针...");
            var partnerStatusAddr = await SearchTradePartnerStatusAsync().ConfigureAwait(false);
            if (partnerStatusAddr != 0)
            {
                var pointerPath = await TracePointerPathAsync(partnerStatusAddr).ConfigureAwait(false);
                if (pointerPath != null)
                {
                    results["TradePartnerStatusPointer"] = pointerPath;
                }
            }
            
            // 搜索链接交换代码指针
            Console.WriteLine("\n搜索链接交换代码指针...");
            var tradeCodeAddr = await SearchTradeCodeAsync().ConfigureAwait(false);
            if (tradeCodeAddr != 0)
            {
                var pointerPath = await TracePointerPathAsync(tradeCodeAddr).ConfigureAwait(false);
                if (pointerPath != null)
                {
                    results["LinkTradeCodePointer"] = pointerPath;
                }
            }
            
        } catch (Exception ex)
        {
            Console.WriteLine($"搜索交换指针时发生错误: {ex.Message}");
        }
        
        return results.Count > 0 ? results : null;
    }
    
    /// <summary>
    /// 搜索交换伙伴宝可梦数据地址
    /// </summary>
    /// <returns>交换伙伴宝可梦数据地址，如果未找到则返回0</returns>
    private async Task<ulong> SearchTradePartnerPokemonAsync()
    {
        // 交换伙伴宝可梦数据特征：与普通宝可梦数据类似
        var signature = GeneratePokemonSignature();
        var addresses = await SearchMemoryAsync(signature, 10).ConfigureAwait(false);
        
        // 返回第一个可能的交换伙伴宝可梦地址
        return addresses.FirstOrDefault();
    }
    
    /// <summary>
    /// 搜索交换伙伴状态地址
    /// </summary>
    /// <returns>交换伙伴状态地址，如果未找到则返回0</returns>
    private async Task<ulong> SearchTradePartnerStatusAsync()
    {
        // 交换伙伴状态特征：通常包含在线状态、准备状态等
        var signature = new byte[4];
        signature[0] = 0x01; // 在线状态示例
        
        var addresses = await SearchMemoryAsync(signature, 5).ConfigureAwait(false);
        return addresses.FirstOrDefault();
    }
    
    /// <summary>
    /// 搜索交换代码地址
    /// </summary>
    /// <returns>交换代码地址，如果未找到则返回0</returns>
    private async Task<ulong> SearchTradeCodeAsync()
    {
        // 交换代码特征：6位数字，以特定格式存储
        var signature = new byte[6];
        // 设置6位数字的示例（如000001）
        signature[0] = 0x01;
        signature[1] = 0x00;
        signature[2] = 0x00;
        signature[3] = 0x00;
        signature[4] = 0x00;
        signature[5] = 0x00;
        
        var addresses = await SearchMemoryAsync(signature, 5).ConfigureAwait(false);
        return addresses.FirstOrDefault();
    }
    
    /// <summary>
    /// 内存搜索功能
    /// </summary>
    /// <param name="signature">要搜索的字节特征</param>
    /// <param name="maxResults">最大结果数量</param>
    /// <returns>找到的内存地址列表</returns>
    private async Task<List<ulong>> SearchMemoryAsync(byte[] signature, int maxResults = 10)
    {
        if (signature.Length == 0)
            return new List<ulong>();
        
        Console.WriteLine($"正在搜索内存，特征长度: {signature.Length} 字节");
        
        var results = new List<ulong>();
        const ulong searchChunkSize = 0x10000; // 64KB 搜索块
        
        // 获取主内存基地址
        var mainBase = await _connection.GetMainNsoBaseAsync(_token).ConfigureAwait(false);
        Console.WriteLine($"主内存基地址: 0x{mainBase:X16}");
        
        // 计算实际搜索范围
        var startAddress = mainBase;
        var endAddress = mainBase + 0x2000000; // 搜索主内存的前512MB
        
        Console.WriteLine($"搜索范围: 0x{startAddress:X16} - 0x{endAddress:X16}");
        
        ulong currentAddress = startAddress;
        while (currentAddress < endAddress && results.Count < maxResults)
        {
            _token.ThrowIfCancellationRequested();
            
            ulong chunkEnd = Math.Min(currentAddress + searchChunkSize, endAddress);
            int chunkSize = (int)(chunkEnd - currentAddress);
            
            try
            {
                // 读取内存块
                var memoryChunk = await _connection.ReadBytesMainAsync(currentAddress, chunkSize, _token).ConfigureAwait(false);
                
                // 在内存块中搜索特征
                var matches = SearchInChunk(memoryChunk, signature, currentAddress);
                results.AddRange(matches);
                
                // 如果找到足够的结果，提前结束
                if (results.Count >= maxResults)
                    break;
                
                // 显示进度
                double progress = (double)(currentAddress - startAddress) / (endAddress - startAddress) * 100;
                Console.Write($"搜索进度: {progress:F1}% | 找到结果: {results.Count}\r");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n读取内存失败 (0x{currentAddress:X16}): {ex.Message}");
                // 跳过无法读取的内存块
            }
            
            currentAddress += searchChunkSize;
        }
        
        Console.WriteLine($"\n搜索完成 | 总共找到: {results.Count} 个结果");
        return results.Take(maxResults).ToList();
    }
    
    /// <summary>
    /// 在内存块中搜索特征
    /// </summary>
    /// <param name="chunk">内存块数据</param>
    /// <param name="signature">搜索特征</param>
    /// <param name="baseAddress">内存块的基地址</param>
    /// <returns>找到的地址列表</returns>
    private List<ulong> SearchInChunk(byte[] chunk, byte[] signature, ulong baseAddress)
    {
        var results = new List<ulong>();
        
        for (int i = 0; i <= chunk.Length - signature.Length; i++)
        {
            bool match = true;
            
            for (int j = 0; j < signature.Length; j++)
            {
                // 允许通配符（0x00表示任意字节）
                if (signature[j] != 0x00 && chunk[i + j] != signature[j])
                {
                    match = false;
                    break;
                }
            }
            
            if (match)
            {
                ulong address = baseAddress + (uint)i;
                results.Add(address);
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// 生成宝可梦数据的搜索特征
    /// </summary>
    /// <returns>宝可梦数据特征</returns>
    private byte[] GeneratePokemonSignature()
    {
        // 宝可梦数据的特征：前4字节为宝可梦ID，接着是形态ID等
        var signature = new byte[16];
        
        // 设置宝可梦数据的标志性字段
        signature[0] = 0x01; // 宝可梦ID（示例：妙蛙种子）
        signature[1] = 0x00;
        signature[2] = 0x00;
        signature[3] = 0x00;
        
        // 其他字段使用通配符
        for (int i = 4; i < signature.Length; i++)
            signature[i] = 0x00;
        
        return signature;
    }
    
    /// <summary>
    /// 从给定地址追踪指针路径
    /// </summary>
    /// <param name="targetAddress">目标数据地址</param>
    /// <returns>指针路径</returns>
    private async Task<List<long>?> TracePointerPathAsync(ulong targetAddress)
    {
        Console.WriteLine("正在追踪指针路径...");
        
        var pointerPath = new List<long>();
        const int maxDepth = 5; // 最大指针深度
        
        // 获取主内存基地址
        var mainBase = await _connection.GetMainNsoBaseAsync(_token).ConfigureAwait(false);
        
        ulong currentAddress = targetAddress;
        int depth = 0;
        
        while (depth < maxDepth)
        {
            _token.ThrowIfCancellationRequested();
            
            // 搜索指向当前地址的指针
            var pointerAddress = await FindPointerToAddressAsync(currentAddress, mainBase).ConfigureAwait(false);
            
            if (pointerAddress == 0)
            {
                Console.WriteLine("未找到更多指针");
                break;
            }
            
            // 计算偏移量
            long offset = (long)(currentAddress - pointerAddress);
            pointerPath.Insert(0, offset);
            
            Console.WriteLine($"  深度 {depth + 1}: 指针地址 0x{pointerAddress:X16}, 偏移量 0x{offset:X}");
            
            // 更新当前地址为指针地址
            currentAddress = pointerAddress;
            depth++;
        }
        
        // 添加基地址到路径
        pointerPath.Insert(0, (long)mainBase);
        
        Console.WriteLine($"完整指针路径: [{string.Join(", ", pointerPath.Select(p => $"0x{p:X}"))}]");
        
        return pointerPath;
    }
    
    /// <summary>
    /// 查找指向目标地址的指针
    /// </summary>
    /// <param name="targetAddress">目标地址</param>
    /// <param name="searchBase">搜索基地址</param>
    /// <returns>指针地址，如果未找到则返回0</returns>
    private async Task<ulong> FindPointerToAddressAsync(ulong targetAddress, ulong searchBase)
    {
        const ulong searchRange = 0x100000; // 搜索范围：1MB
        const ulong pointerSize = 8; // 指针大小：8字节
        
        // 搜索目标地址周围的内存
        ulong startAddress = Math.Max(searchBase, targetAddress - searchRange);
        ulong endAddress = targetAddress + searchRange;
        
        Console.WriteLine($"  搜索指针范围: 0x{startAddress:X16} - 0x{endAddress:X16}");
        
        ulong currentAddress = startAddress;
        while (currentAddress < endAddress)
        {
            _token.ThrowIfCancellationRequested();
            
            try
            {
                // 读取8字节（指针大小）
                var data = await _connection.ReadBytesMainAsync(currentAddress, (int)pointerSize, _token).ConfigureAwait(false);
                
                // 转换为指针值
                ulong pointerValue = BitConverter.ToUInt64(data, 0);
                
                // 检查是否指向目标地址
                if (pointerValue == targetAddress)
                {
                    Console.WriteLine($"  找到指针: 0x{currentAddress:X16} -> 0x{targetAddress:X16}");
                    return currentAddress;
                }
            }
            catch
            {
                // 忽略读取错误
            }
            
            // 按指针大小递增
            currentAddress += pointerSize;
        }
        
        return 0;
    }
    
    /// <summary>
    /// 更新PokeDataOffsetsLZA.cs文件
    /// </summary>
    /// <param name="offsets">新的偏移量数据</param>
    /// <returns>是否成功更新</returns>
    public bool UpdateOffsetsFile(Dictionary<string, List<long>> offsets)
    {
        // 构建偏移量文件路径
        string? filePath = null;
        try
        {
            // 尝试多种方式获取项目根目录
            var baseDir = AppContext.BaseDirectory;
            var projectRoot = FindProjectRoot(baseDir);
            
            if (projectRoot != null)
            {
                filePath = Path.Combine(projectRoot, "SysBot.Pokemon", "LZA", "Vision", "PokeDataOffsetsLZA.cs");
                filePath = Path.GetFullPath(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"构建文件路径失败: {ex.Message}");
            return false;
        }
        
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Console.WriteLine($"错误: 找不到偏移量文件: {filePath}");
            return false;
        }
        
        try
        {
            var content = File.ReadAllText(filePath);
            
            // 更新各个指针
            foreach (var (name, pointer) in offsets)
            {
                content = UpdatePointerInFile(content, name, pointer);
            }
            
            // 保存更新后的文件
            File.WriteAllText(filePath, content);
            Console.WriteLine($"成功更新偏移量文件: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新文件失败: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 更新文件中的特定指针
    /// </summary>
    /// <param name="content">文件内容</param>
    /// <param name="pointerName">指针名称</param>
    /// <param name="pointer">指针路径</param>
    /// <returns>更新后的内容</returns>
    private string UpdatePointerInFile(string content, string pointerName, List<long> pointer)
    {
        var startIndex = content.IndexOf($"public IReadOnlyList<long> {pointerName}", StringComparison.Ordinal);
        if (startIndex == -1)
        {
            Console.WriteLine($"警告: 未找到指针 {pointerName}");
            return content;
        }
        
        var endIndex = content.IndexOf("];", startIndex) + 2;
        if (endIndex == 1)
        {
            Console.WriteLine($"警告: 无法确定指针 {pointerName} 的结束位置");
            return content;
        }
        
        var replacement = $"public IReadOnlyList<long> {pointerName} {{ get; }} = [{string.Join(", ", pointer.Select(p => $"0x{p:X}"))}];";
        return content.Substring(0, startIndex) + replacement + content.Substring(endIndex);
    }
}