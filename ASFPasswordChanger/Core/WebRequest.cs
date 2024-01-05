using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web.Responses;
using ASFPasswordChanger.Data;
using System.Security.Cryptography;
using System.Text;

namespace ASFPasswordChanger.Core;

internal static class WebRequest
{
    internal static async Task<HtmlDocumentResponse?> FetchAccountInfo(Bot bot)
    {
        var request = new Uri(Utils.SteamStoreURL, "/account/");
        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request).ConfigureAwait(false);
        return response;
    }

    internal static async Task<string?> FetchChangeMyPassword(Bot bot)
    {
        var request = new Uri(Utils.SteamHelpURL, "/zh-cn/wizard/HelpChangePassword?redir=store/account/");
        var referer = new Uri(Utils.SteamStoreURL, "/account/");
        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request, referer: referer).ConfigureAwait(false);

        if (response?.Content != null)
        {
            var ele = response.Content.QuerySelector("#wizard_contents>div>a.help_wizard_button.help_wizard_arrow_right[href^=https]");
            return ele?.GetAttribute("href");
        }
        return null;
    }

    internal static async Task<HtmlDocumentResponse?> ChangePasswdVia2Fa(Bot bot, string uri)
    {
        var request = new Uri(uri);
        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request).ConfigureAwait(false);
        return response;
    }

    internal static Dictionary<string, string>? GeneratePayload(HtmlDocumentResponse response)
    {
        var eles = response?.Content?.QuerySelectorAll("#forgot_login_code_form>input[name][value]");

        if (eles == null)
        {
            return null;
        }

        var data = new Dictionary<string, string>
        {
            { "wizard_ajax", "1" },
            { "gamepad", "0" },
        };

        foreach (var ele in eles)
        {
            var name = ele.GetAttribute("name");
            var value = ele.GetAttribute("value");
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            {
                data.Add(name, value);
            }
        }
        return data;
    }

    internal static async Task<AjaxSendAccountRecoveryCodeResponse?> AjaxSendAccountRecoveryCode(Bot bot, Dictionary<string, string> payload)
    {
        var request = new Uri(Utils.SteamHelpURL, "/zh-cn/wizard/AjaxSendAccountRecoveryCode");
        var data = new Dictionary<string, string>
        {
            { "wizard_ajax", "1" },
            { "gamepad", "0" },
            { "s", payload.GetValueOrDefault("s", "") },
            { "method", "8" },
            { "link", "" },
        };

        var response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<AjaxSendAccountRecoveryCodeResponse>(request, data: data, session: ArchiWebHandler.ESession.Lowercase).ConfigureAwait(false);
        return response?.Content;
    }

    internal static async Task<AjaxPollAccountRecoveryConfirmationResponse?> AjaxPollAccountRecoveryConfirmation(Bot bot, Dictionary<string, string> payload)
    {
        var request = new Uri(Utils.SteamHelpURL, "/zh-cn/wizard/AjaxPollAccountRecoveryConfirmation");
        var data = new Dictionary<string, string>
        {
            { "wizard_ajax", "1" },
            { "gamepad", "0" },
            { "s", payload.GetValueOrDefault("s", "") },
            { "reset", "1" },
            { "lost", "0" },
            { "method", "8" },
            { "issueid", "406" },
        };

        var response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<AjaxPollAccountRecoveryConfirmationResponse>(request, data: data, session: ArchiWebHandler.ESession.Lowercase).ConfigureAwait(false);
        return response?.Content;
    }

    internal static async Task<HashCodeResponse?> AjaxVerifyAccountRecoveryCode(Bot bot, Dictionary<string, string> payload)
    {
        var sessionID = bot.ArchiWebHandler.WebBrowser.CookieContainer.GetCookieValue(Utils.SteamHelpURL, "sessionid");
        if (sessionID == null)
        {
            return null;
        }

        var s = payload.GetValueOrDefault("s", "");
        var account = Utils.SteamId2Steam32(bot.SteamID).ToString();

        var data = new Dictionary<string, string>
        {
            { "code", "" },
            { "s", s },
            { "account", account },
            { "reset", "1" },
            { "lost", "0" },
            { "method", "8" },
            { "issueid", "406" },
            { "wizard_ajax", "1" },
            { "gamepad", "0" },
        };

        var sb = new StringBuilder();
        foreach (var (k, v) in data)
        {
            sb.Append(string.Format("{0}={1}&", k, v));
        }
        var query = sb.ToString();

        var request = new Uri(Utils.SteamHelpURL, $"/zh-cn/wizard/AjaxVerifyAccountRecoveryCode?{query}sessionid={sessionID}");
        var referer = new Uri(Utils.SteamHelpURL, $"/zh-cn/wizard/HelpWithLoginInfoEnterCode?s={s}&account={account}&reset=1&lost=0&issueid=406");

        var response = await bot.ArchiWebHandler.UrlGetToJsonObjectWithSession<HashCodeResponse>(request, referer: referer).ConfigureAwait(false);
        return response?.Content;
    }

    internal static async Task<string?> FetchChangeMyPasswordStep2(Bot bot, string path)
    {
        var request = new Uri(Utils.SteamHelpURL, $"/zh-cn/{path}");
        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request).ConfigureAwait(false);

        if (response?.Content != null)
        {
            var ele = response.Content.QuerySelector("#wizard_contents>div>a.help_wizard_button.help_wizard_arrow_right[href^=https]");
            return ele?.GetAttribute("href");
        }
        return null;
    }

    internal static async Task<AjaxAccountRecoveryGetNextStepResponse?> AjaxAccountRecoveryGetNextStep(Bot bot, Dictionary<string, string> data)
    {
        var request = new Uri(Utils.SteamHelpURL, "/zh-cn/wizard/AjaxAccountRecoveryGetNextStep");
        var response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<AjaxAccountRecoveryGetNextStepResponse>(request, data: data, session: ArchiWebHandler.ESession.Lowercase).ConfigureAwait(false);
        return response?.Content;
    }

    internal static async Task<HtmlDocumentResponse?> FetchChangeMyPasswordStep3(Bot bot, string uri)
    {
        var request = new Uri(uri);
        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request).ConfigureAwait(false);
        return response;
    }

    internal static async Task<GetRsaKeyResponse?> GetRsaKey(Bot bot)
    {
        var request = new Uri(Utils.SteamHelpURL, "/zh-cn/login/getrsakey/");
        var data = new Dictionary<string, string>
        {
            { "username", bot.BotConfig.SteamLogin ?? "" },
        };

        var response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<GetRsaKeyResponse>(request, data: data, session: ArchiWebHandler.ESession.Lowercase).ConfigureAwait(false);

        return response?.Content;
    }

    private static string EncryptPassword(GetRsaKeyResponse rsaKey, string passwd)
    {
        var rsa1 = RSA.Create(new RSAParameters
        {
            Modulus = Convert.FromHexString(rsaKey.PublicKeyMod!),
            Exponent = Convert.FromHexString(rsaKey.PublicKeyExp!),
        });

        byte[] passwdBytes = Encoding.UTF8.GetBytes(passwd);

        var encBytes = rsa1.Encrypt(passwdBytes, RSAEncryptionPadding.Pkcs1);
        var encPasswd = Convert.ToBase64String(encBytes);

        return encPasswd;
    }

    internal static async Task<HashCodeResponse?> AjaxAccountRecoveryVerifyPassword(Bot bot, Dictionary<string, string> data, GetRsaKeyResponse rsaKey, string oldPasswd)
    {
        var request = new Uri(Utils.SteamHelpURL, "/zh-cn/wizard/AjaxAccountRecoveryVerifyPassword/");
        var encPassword = EncryptPassword(rsaKey, oldPasswd);
        var payload = new Dictionary<string, string>
        {
            { "s", data.GetValueOrDefault("s", "") },
            { "lost", "2" },
            { "reset", "1" },
            { "password", encPassword },
            { "rsatimestamp", rsaKey.TimeStamp! }
        };

        var response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<HashCodeResponse>(request, data: payload, session: ArchiWebHandler.ESession.Lowercase).ConfigureAwait(false);

        return response?.Content;
    }

    internal static async Task<HtmlDocumentResponse?> FetchChangeMyPasswordStep4(Bot bot, string path)
    {
        var request = new Uri(Utils.SteamHelpURL, $"/zh-cn/{path}");
        var response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request).ConfigureAwait(false);
        return response;
    }

    internal static async Task<AjaxCheckPasswordAvailableResponse?> AjaxCheckPasswordAvailable(Bot bot, string passwd)
    {
        var request = new Uri(Utils.SteamHelpURL, "/zh-cn/wizard/AjaxCheckPasswordAvailable/");
        var data = new Dictionary<string, string>
        {
            { "wizard_ajax", "1" },
            { "gamepad", "0" },
            { "password", passwd },
        };
        var response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<AjaxCheckPasswordAvailableResponse>(request, data: data).ConfigureAwait(false);
        return response?.Content;
    }

    internal static async Task<HashCodeResponse?> AjaxAccountRecoveryChangePassword(Bot bot, Dictionary<string, string> payload, GetRsaKeyResponse rsaKey, string oldPasswd)
    {
        var request = new Uri(Utils.SteamHelpURL, "/zh-cn/wizard/AjaxAccountRecoveryChangePassword/");
        var encPassword = EncryptPassword(rsaKey, oldPasswd);
        var data = new Dictionary<string, string>
        {
            { "wizard_ajax", "1" },
            { "gamepad", "0" },
            { "s", payload.GetValueOrDefault("s", "") },
            { "account", Utils.SteamId2Steam32(bot.SteamID).ToString() },
            { "password", encPassword },
            { "rsatimestamp", rsaKey.TimeStamp! }
        };

        var response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<HashCodeResponse>(request, data: data, session: ArchiWebHandler.ESession.Lowercase).ConfigureAwait(false);

        return response?.Content;
    }
}

