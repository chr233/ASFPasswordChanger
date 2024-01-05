using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;

namespace ASFPasswordChanger.Core;

internal static partial class Command
{
    internal static async Task<string?> ResponseTest(Bot bot, string newPasswd)
    {
        if (!bot.IsConnectedAndLoggedOn)
        {
            bot.Actions.Start();
            await Task.Delay(5000).ConfigureAwait(false);
        }
        if (!bot.IsConnectedAndLoggedOn)
        {
            return bot.FormatBotResponse(Strings.BotNotConnected);
        }

        var respAccountInfo = await WebRequest.FetchAccountInfo(bot).ConfigureAwait(false);
        var changePasswdUri = await WebRequest.FetchChangeMyPassword(bot).ConfigureAwait(false);
        if (string.IsNullOrEmpty(changePasswdUri))
        {
            return bot.FormatBotResponse("未找到改密入口");
        }

        var respWait2fa = await WebRequest.ChangePasswdVia2Fa(bot, changePasswdUri).ConfigureAwait(false);
        if (respWait2fa == null)
        {
            return bot.FormatBotResponse("ChangePasswdVia2Fa 响应为空");
        }

        var payload = WebRequest.GeneratePayload(respWait2fa);
        if (payload == null)
        {
            return bot.FormatBotResponse("解析表单数据失败");
        }

        var respSendCode = await WebRequest.AjaxSendAccountRecoveryCode(bot, payload).ConfigureAwait(false);
        if (respSendCode == null)
        {
            return bot.FormatBotResponse("SendAccountRecoveryCode 响应为空");
        }


        int tries = 5;
        bool tfaOk = false;
        while (tries-- > 0)
        {
            var (tfaSuccess, _, msg) = await bot.Actions.HandleTwoFactorAuthenticationConfirmations(true).ConfigureAwait(false);
            if (!tfaSuccess)
            {
                return bot.FormatBotResponse(string.Format("两步验证失败 {0}", msg));
            }

            var respPollConfirmation = await WebRequest.AjaxPollAccountRecoveryConfirmation(bot, payload).ConfigureAwait(false);
            if (respPollConfirmation?.Success == true)
            {
                tfaOk = true;
                break;
            }
            await Task.Delay(2000).ConfigureAwait(false);
        }

        if (!tfaOk)
        {
            return bot.FormatBotResponse("两步验证超时");
        }
        else
        {
            await Task.Delay(500).ConfigureAwait(false);
        }

        var respVerifyAccount = await WebRequest.AjaxVerifyAccountRecoveryCode(bot, payload).ConfigureAwait(false);
        if (string.IsNullOrEmpty(respVerifyAccount?.Hash))
        {
            return bot.FormatBotResponse(string.Format("两步验证遇到错误, {0}", respVerifyAccount?.ErrorMsg ?? "NULL"));
        }

        var respNextStep = await WebRequest.FetchChangeMyPasswordStep2(bot, respVerifyAccount.Hash).ConfigureAwait(false);

        var respGetNextStep = await WebRequest.AjaxAccountRecoveryGetNextStep(bot, payload).ConfigureAwait(false);
        if (string.IsNullOrEmpty(respGetNextStep?.Redirect))
        {
            return bot.FormatBotResponse("AjaxAccountRecoveryGetNextStep 响应为空");
        }

        var respStep3 = await WebRequest.FetchChangeMyPasswordStep3(bot, respGetNextStep.Redirect).ConfigureAwait(false);

        var respRsakey = await WebRequest.GetRsaKey(bot).ConfigureAwait(false);
        if (respRsakey == null || string.IsNullOrEmpty(respRsakey.PublicKeyMod) || string.IsNullOrEmpty(respRsakey.PublicKeyExp) || string.IsNullOrEmpty(respRsakey.TimeStamp))
        {
            return bot.FormatBotResponse("GetRsaKey 响应为空");
        }

        var respVerifyPasswd = await WebRequest.AjaxAccountRecoveryVerifyPassword(bot, payload, respRsakey, bot.BotConfig.SteamPassword!).ConfigureAwait(false);
        if (string.IsNullOrEmpty(respVerifyPasswd?.Hash))
        {
            return bot.FormatBotResponse(string.Format("RecoveryVerifyPassword 失败, {0}", respVerifyPasswd?.ErrorMsg ?? "NULL"));
        }

        var respStep4 = await WebRequest.FetchChangeMyPasswordStep4(bot, respVerifyPasswd.Hash).ConfigureAwait(false);

        var respPasswdAvilable = await WebRequest.AjaxCheckPasswordAvailable(bot, newPasswd).ConfigureAwait(false);
        if (respPasswdAvilable?.Available != true)
        {
            return bot.FormatBotResponse("密码不可用");
        }

        var respRsakey2 = await WebRequest.GetRsaKey(bot).ConfigureAwait(false);
        if (respRsakey2 == null || string.IsNullOrEmpty(respRsakey2.PublicKeyMod) || string.IsNullOrEmpty(respRsakey2.PublicKeyExp) || string.IsNullOrEmpty(respRsakey2.TimeStamp))
        {
            return bot.FormatBotResponse("GetRsaKey2 响应为空");
        }

        var respChangePassword = await WebRequest.AjaxAccountRecoveryChangePassword(bot, payload, respRsakey2, newPasswd).ConfigureAwait(false);
        if (string.IsNullOrEmpty(respVerifyPasswd?.Hash))
        {
            return bot.FormatBotResponse(string.Format("RecoveryChangePassword 失败, {0}", respVerifyPasswd?.ErrorMsg ?? "NULL"));
        }

        bot.Actions.Stop();

        return bot.FormatBotResponse($"修改密码成功, 机器人的新密码为 {newPasswd}");
    }

    internal static async Task<string?> ResponseTest(string botNames, string newPasswd)
    {
        if (string.IsNullOrEmpty(botNames))
        {
            throw new ArgumentNullException(nameof(botNames));
        }

        HashSet<Bot>? bots = Bot.GetBots(botNames);

        if ((bots == null) || (bots.Count == 0))
        {
            return Utils.FormatStaticResponse(string.Format(Strings.BotNotFound, botNames));
        }

        IList<string?> results = await Utilities.InParallel(bots.Select(bot => ResponseTest(bot, newPasswd))).ConfigureAwait(false);

        List<string> responses = new(results.Where(result => !string.IsNullOrEmpty(result))!);

        return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
    }
}
