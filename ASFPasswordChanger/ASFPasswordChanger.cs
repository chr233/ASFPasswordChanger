using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ASFPasswordChanger.Data;
using System.ComponentModel;
using System.Composition;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASFPasswordChanger;

[Export(typeof(IPlugin))]
internal sealed class ASFPasswordChanger : IASF, IBotCommand2
{
    public string Name => "ASF Password Changer";
    public Version Version => Utils.MyVersion;

    private bool ASFEBridge;

    [JsonInclude]
    public static PluginConfig Config => Utils.Config;

    private Timer? StatisticTimer;

    /// <summary>
    /// ASF启动事件
    /// </summary>
    /// <param name="additionalConfigProperties"></param>
    /// <returns></returns>
    public Task OnASFInit(IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null)
    {
        PluginConfig? config = null;

        if (additionalConfigProperties != null)
        {
            foreach (var (configProperty, configValue) in additionalConfigProperties)
            {
                if (configProperty == "ASFEnhance" && configValue.ValueKind == JsonValueKind.Object)
                {
                    try
                    {
                        config = configValue.Deserialize<PluginConfig>();
                        if (config != null)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.Logger.LogGenericException(ex);
                    }
                }
            }
        }

        Utils.Config = config ?? new();

        //统计
        if (Config.Statistic)
        {
            Uri request = new("https://asfe.chrxw.com/");
            StatisticTimer = new Timer(
                async (_) =>
                {
                    await ASF.WebBrowser!.UrlGetToHtmlDocument(request).ConfigureAwait(false);
                },
                null,
                TimeSpan.FromSeconds(30),
                TimeSpan.FromHours(24)
            );
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 插件加载事件
    /// </summary>
    /// <returns></returns>
    public Task OnLoaded()
    {
        Utils.Logger.LogGenericInfo(Langs.PluginContact);
        Utils.Logger.LogGenericInfo(Langs.PluginInfo);

        var flag = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        var handler = typeof(ASFPasswordChanger).GetMethod(nameof(ResponseCommand), flag);

        const string pluginId = nameof(ASFPasswordChanger);
        const string cmdPrefix = "SAS";
        const string? repoName = null;

        ASFEBridge = AdapterBridge.InitAdapter(Name, pluginId, cmdPrefix, repoName, handler);

        if (ASFEBridge)
        {
            Utils.Logger.LogGenericDebug(Langs.ASFEnhanceRegisterSuccess);
        }
        else
        {
            Utils.Logger.LogGenericInfo(Langs.ASFEnhanceRegisterFailed);
            Utils.Logger.LogGenericWarning(Langs.PluginStandalongMode);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取插件信息
    /// </summary>
    private static string? PluginInfo => string.Format("{0} {1}", nameof(ASFPasswordChanger), Utils.MyVersion);

    /// <summary>
    /// 处理命令
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="access"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Task<string?>? ResponseCommand(Bot bot, EAccess access, string cmd, string[] args)
    {
        int argLength = args.Length;
        return argLength switch
        {
            0 => throw new InvalidOperationException(nameof(args.Length)),
            1 => cmd switch //不带参数
            {
                //PluginInfo
                "ASFPASSWORDcHANGER" or
                "APC" when access >= EAccess.Master =>
                    Task.FromResult(PluginInfo),
                //Core
                "CHANGEPASSWORD" or
                "CP" when argLength == 3 && access >= EAccess.Master =>
                    Core.Command.ResponseTest(args[1], args[2]),
                "CHANGEPASSWORD" or
                "CP" when argLength == 2 && access >= EAccess.Master =>
                    Core.Command.ResponseTest(bot, args[1]),

                _ => null,
            },
            _ => null,
        };
    }

    /// <summary>
    /// 处理命令事件
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="access"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <param name="steamId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidEnumArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamId = 0)
    {
        if (ASFEBridge)
        {
            return null;
        }

        if (!Enum.IsDefined(access))
        {
            throw new InvalidEnumArgumentException(nameof(access), (int)access, typeof(EAccess));
        }

        try
        {
            var cmd = args[0].ToUpperInvariant();

            if (cmd.StartsWith("SAS."))
            {
                cmd = cmd[4..];
            }

            var task = ResponseCommand(bot, access, cmd, args);
            if (task != null)
            {
                return await task.ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(500).ConfigureAwait(false);
                Utils.Logger.LogGenericException(ex);
            }).ConfigureAwait(false);

            return ex.StackTrace;
        }
    }
}
