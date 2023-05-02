using ArchiSteamFarm.Core;
using ArchiSteamFarm.NLog;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ASFBuffBot.Data;
using Newtonsoft.Json;
using System.Reflection;

namespace ASFBuffBot;

internal static class Utils
{
    /// <summary>
    /// 插件配置
    /// </summary>
    internal static PluginConfig Config { get; set; } = new();

    /// <summary>
    /// BuffCookies
    /// </summary>
    internal static CookiesStorage BuffCookies = new();

    /// <summary>
    /// 更新已就绪
    /// </summary>
    internal static bool UpdatePadding { get; set; }

    /// <summary>
    /// 更新标记
    /// </summary>
    /// <returns></returns>
    private static string UpdateFlag()
    {
        if (UpdatePadding)
        {
            return "*";
        }
        else
        {
            return "";
        }
    }

    /// <summary>
    /// 格式化返回文本
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal static string FormatStaticResponse(string message)
    {
        string flag = UpdateFlag();

        return $"<ABB{flag}> {message}";
    }

    /// <summary>
    /// 格式化返回文本
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    internal static string FormatBotResponse(this Bot bot, string message)
    {
        string flag = UpdateFlag();

        return $"<{bot.BotName}{flag}> {message}";
    }

    /// <summary>
    /// 转换SteamId
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    internal static ulong SteamId2Steam32(ulong steamId)
    {
        return steamId & 0x001111011111111;
    }

    /// <summary>
    /// 转换SteamId
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    internal static ulong Steam322SteamId(ulong steamId)
    {
        return steamId | 0x110000100000000;
    }

    internal static string GetCookiesFilePath()
    {
        string pluginFolder = Path.GetDirectoryName(MyLocation) ?? ".";
        string cookieFilePath = Path.Combine(pluginFolder, "BuffCookies.json");
        return cookieFilePath;
    }

    /// <summary>
    /// 读取Cookies
    /// </summary>
    /// <returns></returns>
    internal static async Task<bool> LoadCookiesFile()
    {
        try
        {
            string cookieFilePath = GetCookiesFilePath();
            using var fs = File.Open(cookieFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);
            string? raw = await sr.ReadLineAsync().ConfigureAwait(false);
            if (!string.IsNullOrEmpty(raw))
            {
                var json = JsonConvert.DeserializeObject<CookiesStorage>(raw);
                if (json != null)
                {
                    BuffCookies = json;
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogGenericException(ex, "读取Cookies文件出错");
            return false;
        }
    }

    /// <summary>
    /// 写入Cookies
    /// </summary>
    /// <returns></returns>
    internal static async Task<bool> SaveCookiesFile()
    {
        try
        {
            string cookieFilePath = GetCookiesFilePath();
            using var fs = File.Open(cookieFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var sw = new StreamWriter(fs);
            string json = JsonConvert.SerializeObject(BuffCookies);
            await sw.WriteAsync(json).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogGenericException(ex, "写入Cookies文件出错");
            return false;
        }
    }

    /// <summary>
    /// 获取版本号
    /// </summary>
    internal static Version MyVersion => Assembly.GetExecutingAssembly().GetName().Version ?? new Version("0");

    /// <summary>
    /// 获取插件所在路径
    /// </summary>
    internal static string MyLocation => Assembly.GetExecutingAssembly().Location;

    /// <summary>
    /// Steam商店链接
    /// </summary>
    internal static Uri SteamStoreURL => ArchiWebHandler.SteamStoreURL;

    /// <summary>
    /// Steam社区链接
    /// </summary>
    internal static Uri SteamCommunityURL = ArchiWebHandler.SteamCommunityURL;

    /// <summary>
    /// Steam API链接
    /// </summary>
    internal static Uri SteamApiURL => new("https://api.steampowered.com");

    /// <summary>
    /// 日志
    /// </summary>
    internal static ArchiLogger Logger => ASF.ArchiLogger;
}
