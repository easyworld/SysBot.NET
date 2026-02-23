using System;

namespace SysBot.Base;

/// <summary>
/// Decodes a sys-botbase protocol message into raw data.
/// </summary>
public static class Decoder
{
    public static byte[] ConvertHexByteStringToBytes(ReadOnlySpan<byte> bytes) => Convert.FromHexString(bytes);

    public static void LoadHexBytesTo(ReadOnlySpan<byte> str, Span<byte> dest)
    {
        // The input string is 2-char hex values optionally separated.
        // The destination array should always be larger or equal than the bytes written. Let the runtime bounds check us.
        Convert.FromHexString(str, dest, out _, out _);
    }
}
