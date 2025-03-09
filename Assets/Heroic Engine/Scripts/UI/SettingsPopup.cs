using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.DI;
using System;
using TMPro;
using UnityEngine;

namespace HeroicEngine.UI
{
    public class SettingsPopup : UIPart
    {
        [SerializeField] private TMP_Dropdown languageSelector;

        [Inject] private ILocalizationManager localizationManager;

        private void Start()
        {
            InjectionManager.InjectTo(this);

            var availableLanguages = localizationManager.GetAvailableLanguages();
            var languages = availableLanguages
                .ConvertAll((l) => Enum.GetName(typeof(SystemLanguage), l));

            languageSelector.ClearOptions();
            languageSelector.AddOptions(languages);

            languageSelector.value = availableLanguages.FindIndex(l => l == localizationManager.GetCurrentLanguage());
        }

        public void OnLanguageSelected()
        {
            int selectedLang = languageSelector.value;

            var languages = localizationManager.GetAvailableLanguages();

            localizationManager.SwitchLanguage(languages[selectedLang]);
        }
    }
}