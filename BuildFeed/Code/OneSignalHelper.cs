using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using BuildFeed.Local;
using BuildFeed.Model;
using OneSignal.RestAPIv3.Client;
using OneSignal.RestAPIv3.Client.Resources;
using OneSignal.RestAPIv3.Client.Resources.Notifications;

namespace BuildFeed.Code
{
    public static class OneSignalHelper
    {
        public static void PushNewBuild(this OneSignalClient osc, Build build, string url)
        {
            osc.Notifications.Create(new NotificationCreateOptions
            {
                AppId = Guid.Parse(ConfigurationManager.AppSettings["push:AppId"]),
                IncludedSegments = new List<string>
                {
                    #if DEBUG
                    "Testers"
                    #else
                    "All"
                    #endif
                },
                Headings =
                {
                    { LanguageCodes.Arabic, GetNewBuildTitleForLanguage("ar") },
                    { LanguageCodes.Czech, GetNewBuildTitleForLanguage("cs") },
                    { LanguageCodes.German, GetNewBuildTitleForLanguage("de") },
                    { LanguageCodes.Greek, GetNewBuildTitleForLanguage("el") },
                    { LanguageCodes.English, GetNewBuildTitleForLanguage("en") },
                    { LanguageCodes.Spanish, GetNewBuildTitleForLanguage("es") },
                    { LanguageCodes.Persian, GetNewBuildTitleForLanguage("fa") },
                    { LanguageCodes.Finnish, GetNewBuildTitleForLanguage("fi") },
                    { LanguageCodes.French, GetNewBuildTitleForLanguage("fr") },
                    { LanguageCodes.Hebrew, GetNewBuildTitleForLanguage("he") },
                    { LanguageCodes.Croatian, GetNewBuildTitleForLanguage("hr") },
                    { LanguageCodes.Hungarian, GetNewBuildTitleForLanguage("hu") },
                    { LanguageCodes.Indonesian, GetNewBuildTitleForLanguage("id") },
                    { LanguageCodes.Italian, GetNewBuildTitleForLanguage("it") },
                    { LanguageCodes.Japanese, GetNewBuildTitleForLanguage("ja") },
                    { LanguageCodes.Korean, GetNewBuildTitleForLanguage("ko") },
                    { LanguageCodes.Lithuanian, GetNewBuildTitleForLanguage("lt") },
                    { LanguageCodes.Dutch, GetNewBuildTitleForLanguage("nl") },
                    { LanguageCodes.Polish, GetNewBuildTitleForLanguage("pl") },
                    {
                        LanguageCodes.Portuguese, GetNewBuildTitleForLanguage("pt")
                    }, // Portuguese translation has notification translation ready, Brazil is used more, but not available right now.
                    { LanguageCodes.Romanian, GetNewBuildTitleForLanguage("ro") },
                    { LanguageCodes.Russian, GetNewBuildTitleForLanguage("ru") },
                    { LanguageCodes.Slovak, GetNewBuildTitleForLanguage("sk") },
                    // no slovenian support for OneSignal?
                    { LanguageCodes.Swedish, GetNewBuildTitleForLanguage("sv") },
                    { LanguageCodes.Turkish, GetNewBuildTitleForLanguage("tr") },
                    { LanguageCodes.Ukrainian, GetNewBuildTitleForLanguage("uk") },
                    { LanguageCodes.Vietnamese, GetNewBuildTitleForLanguage("vi") },
                    { LanguageCodes.ChineseSimplified, GetNewBuildTitleForLanguage("zh-hans") },
                    { LanguageCodes.ChineseTraditional, GetNewBuildTitleForLanguage("zh-hant") }
                },
                Contents =
                {
                    { LanguageCodes.English, build.AlternateBuildString }
                },
                Url = url
            });
        }

        private static string GetNewBuildTitleForLanguage(string lang)
        {
            string localised = VariantTerms.ResourceManager.GetString(nameof(VariantTerms.Notification_NewBuild),
                CultureInfo.GetCultureInfo(lang));

            string generic =
                VariantTerms.ResourceManager.GetString(nameof(VariantTerms.Notification_NewBuild),
                    CultureInfo.InvariantCulture)
                ?? "{0}";

            return string.IsNullOrEmpty(localised)
                ? string.Format(generic, InvariantTerms.SiteName)
                : string.Format(localised, InvariantTerms.SiteName);
        }
    }
}