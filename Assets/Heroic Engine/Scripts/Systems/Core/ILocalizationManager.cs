using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.Localization
{
    public interface ILocalizationManager : ISystem
    {
        void SwitchLanguage(SystemLanguage lang);
        string GetLocalizedString(string id);
        string GetLocalizedString<T>(string id, params T[] args);
        string GetLocalizedString<T>(string id, Color paramsColor, params T[] args);
        void ResolveTexts();
        List<SystemLanguage> GetAvailableLanguages();
        SystemLanguage GetCurrentLanguage();
    }
}