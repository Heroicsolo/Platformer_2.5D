using HeroicEngine.Systems.Events;
using HeroicEngine.Utils.Localization;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace HeroicEngine.Systems.Localization
{
    public class LocalizationManager : SystemBase, ILocalizationManager
    {
        public static readonly string LanguageSwitchEvent = "LanguageSwitched";

        [Inject] private IEventsManager eventsManager;

        private const string LanguageSettingsKey = "Language";

        private const char Separator = '=';
        private Dictionary<SystemLanguage, Dictionary<string, string>> _translations = new Dictionary<SystemLanguage, Dictionary<string, string>>();
        private List<SystemLanguage> _availableLanguages = new List<SystemLanguage>();
        private SystemLanguage _language;

        private void Awake()
        {
            LoadLocalizations();
        }

        private void LoadLocalizations()
        {
            _language = (SystemLanguage)PlayerPrefs.GetInt(LanguageSettingsKey, (int)Application.systemLanguage);
            _availableLanguages.Clear();

            var files = Resources.LoadAll<TextAsset>("Localization/");

            for (int i = 0; i < files.Length; i++)
            {
                SystemLanguage fileLanguage = SystemLanguage.English;

                string[] lines = files[i].text.Split('\n');

                foreach (var line in lines)
                {
                    if (line.Contains("#"))
                    {
                        fileLanguage = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), line.Replace("#", ""));
                        if (!_availableLanguages.Contains(fileLanguage))
                        {
                            _availableLanguages.Add(fileLanguage);
                        }
                    }
                    else
                    {
                        if (!_translations.ContainsKey(fileLanguage))
                        {
                            _translations.Add(fileLanguage, new Dictionary<string, string>());
                        }
                        var prop = line.Split(Separator);
                        _translations[fileLanguage][prop[0]] = prop[1];
                    }
                }
            }
        }

        public Dictionary<string, string> GetLocalizationData(SystemLanguage lang)
        {
            return _translations.ContainsKey(lang) ? _translations[lang] : new Dictionary<string, string>();
        }

        public List<SystemLanguage> GetAvailableLanguages()
        {
            if (_availableLanguages.Count == 0)
            {
                LoadLocalizations();
            }
            return _availableLanguages;
        }

        public SystemLanguage GetCurrentLanguage()
        {
            return _language;
        }

        public void AddTranslation(SystemLanguage lang, string key, string translation)
        {
            if (!_translations.ContainsKey(lang))
            {
                _translations.Add(lang, new Dictionary<string, string>());
            }

            _translations[lang][key] = translation;
        }

        public void SwitchLanguage(SystemLanguage lang)
        {
            _language = lang;

            PlayerPrefs.SetInt(LanguageSettingsKey, (int)_language);

            ResolveTexts();

            eventsManager.TriggerEvent(LanguageSwitchEvent, lang);
        }

        public string GetLocalizedString(string id)
        {
            if (_translations[_language].ContainsKey(id))
            {
                return Regex.Unescape(_translations[_language][id]);
            }

            return id;
        }

        public string GetLocalizedString<T>(string id, params T[] args)
        {
            string result = GetLocalizedString(id);
            for (int i = 0; i < args.Length; i++)
            {
                result = result.Replace("{" + i + "}", args[i].ToString());
            }
            return result;
        }

        public string GetLocalizedString<T>(string id, Color paramsColor, params T[] args)
        {
            string result = GetLocalizedString(id);
            for (int i = 0; i < args.Length; i++)
            {
                result = result.Replace("{" + i + "}", $"<color=\"{paramsColor.ToHexString()}\"{args[i]}</color>");
            }
            return result;
        }

        public void ResolveTexts()
        {
            var allTexts = Resources.FindObjectsOfTypeAll(typeof(LangText)) as LangText[];
            foreach (var langText in allTexts)
            {
                var text = langText.GetComponent<Text>();
                if (text != null)
                {
                    text.text = GetLocalizedString(langText.Identifier);
                }
                else
                {
                    var textMesh = langText.GetComponent<TextMesh>();
                    if (textMesh != null)
                    {
                        textMesh.text = GetLocalizedString(langText.Identifier);
                    }
                    else
                    {
                        var textMeshPro = langText.GetComponent<TMP_Text>();
                        textMeshPro.text = GetLocalizedString(langText.Identifier);
                    }
                }
            }
        }
    }
}