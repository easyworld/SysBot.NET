using Kook;
using PKHeX.Core;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public static class NetUtil
{
    public static async Task<byte[]> DownloadFromUrlAsync(string url)
    {
        using var client = new HttpClient();
        return await client.GetByteArrayAsync(url).ConfigureAwait(false);
    }

    private static readonly HashSet<Type> SupportedTypes =
    [
        typeof(PK8), typeof(PA8), typeof(PB8), typeof(PK9), typeof(PA9),
    ];

    public static async Task<Download<List<PKM>>> DownloadPKMsAsync(IAttachment att, Type type)
    {
        // 统一使用本地函数生成错误结果
        static Download<List<PKM>> Error(string name, string msg)
        {
            return new()
            {
                SanitizedFileName = name,
                ErrorMessage = $"{name}: {msg}"
            };
        }

        var fileName = Format.Sanitize(att.Filename);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new Download<List<PKM>>
            {
                SanitizedFileName = "unknown",
                ErrorMessage = "Invalid filename."
            };
        }

        if (!SupportedTypes.Contains(type))
            return Error(fileName, "Unsupported PKM type.");

        var url = att.Url;

        byte[]? buffer;
        try
        {
            buffer = await DownloadFromUrlAsync(url).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Error(fileName, $"Failed to download file: {ex.Message}");
        }

        if (buffer is null || buffer.Length == 0)
            return Error(fileName, "Empty or failed download.");

        try
        {
            // 构造泛型类型 FileTradeHelper<T>
            var helperType = typeof(FileTradeHelper<>).MakeGenericType(type);

            // 取得 ValidBinFileSize(int length) 静态方法
            var validMethod = helperType.GetMethod(
                "ValidBinFileSize",
                BindingFlags.Static | BindingFlags.Public,
                binder: null,
                types: [typeof(int)],
                modifiers: null
            );
            if (validMethod is null)
                return Error(fileName, "Helper type missing ValidBinFileSize method.");

            // 取得 Bin2List(byte[]) 静态方法
            var bin2ListMethod = helperType.GetMethod(
                "Bin2List",
                BindingFlags.Static | BindingFlags.Public,
                binder: null,
                types: [typeof(byte[])],
                modifiers: null
            );
            if (bin2ListMethod is null)
                return Error(fileName, "Helper type missing Bin2List method.");

            // 调用 ValidBinFileSize
            if (validMethod.Invoke(null, [buffer.Length]) is not bool valid)
                return Error(fileName, "Invalid return type from ValidBinFileSize.");

            if (!valid)
                return Error(fileName, "Invalid .bin attachment size.");

            // 调用 Bin2List(byte[])，应返回 IEnumerable / List<PKM 派生>
            var listObj = bin2ListMethod.Invoke(null, [buffer]);
            if (listObj is not IEnumerable enumerable)
                return Error(fileName, "Unexpected return type from Bin2List.");

            var pkms = enumerable.Cast<PKM>().ToList();

            return new Download<List<PKM>>
            {
                SanitizedFileName = fileName,
                Data = pkms,
                Success = true
            };
        }
        catch (TargetInvocationException ex)
        {
            // 反射内层异常
            return Error(fileName, $"Exception while processing file (inner): {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            return Error(fileName, $"Exception while processing file: {ex.Message}");
        }
    }

    public static async Task<Download<PKM>> DownloadPKMAsync(IAttachment att)
    {
        var result = new Download<PKM> { SanitizedFileName = Format.Sanitize(att.Filename) };
        if (!EntityDetection.IsSizePlausible(att.Size ?? 0))
        {
            result.ErrorMessage = $"{result.SanitizedFileName}: Invalid size.";
            return result;
        }

        string url = att.Url;

        // Download the resource and load the bytes into a buffer.
        var buffer = await DownloadFromUrlAsync(url).ConfigureAwait(false);
        var prefer = EntityFileExtension.GetContextFromExtension(result.SanitizedFileName);
        var pkm = EntityFormat.GetFromBytes(buffer, prefer);
        if (pkm == null)
        {
            result.ErrorMessage = $"{result.SanitizedFileName}: Invalid pkm attachment.";
            return result;
        }

        result.Data = pkm;
        result.Success = true;
        return result;
    }
}

public sealed class Download<T> where T : class
{
    public bool Success;
    public T? Data;
    public string? SanitizedFileName;
    public string? ErrorMessage;
}
