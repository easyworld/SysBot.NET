using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public class KookMessageUtil
{
    /// <summary>
    /// 去除消息中的mention信息、多余空格和特殊字符
    /// 
    public static string SanitizeMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return string.Empty;
        // Remove any leading or trailing whitespace
        message = message.Trim();
        // Replace multiple spaces with a single space
        message = System.Text.RegularExpressions.Regex.Replace(message, @"\(met\)\d+\(met\)", "").Trim();
        message = Regex.Replace(message, @"\\(.)", "$1");
        return message;
    }

    /// 判断字符串是否为JSON数组格式
    public static bool IsJsonArray(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;
        str = str.Trim();
        return str.StartsWith('[') && str.EndsWith(']');
    }
}
