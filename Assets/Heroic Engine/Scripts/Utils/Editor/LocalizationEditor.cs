using HeroicEngine.Systems.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomEditor(typeof(LocalizationManager))]
    public class LocalizationEditor : UnityEditor.Editor
    {
        private const char Separator = '=';
        LocalizationManager localizationManager;
        Dictionary<SystemLanguage, Dictionary<string, string>> _translations = new Dictionary<SystemLanguage, Dictionary<string, string>>();
        Dictionary<SystemLanguage, string> localizedValues = new Dictionary<SystemLanguage, string>();
        Dictionary<SystemLanguage, string> localizedValues2 = new Dictionary<SystemLanguage, string>();
        int selectedKey = 0;
        int prevKey = 0;
        int selectedLang = 0;
        string localizationKey = "";
        string localizationKey2 = "";
        string statusText;

        private void OnEnable()
        {
            localizationManager = (LocalizationManager)target;
        }

        private void AddNewLanguage(SystemLanguage lang)
        {
            string fileName = Enum.GetName(typeof(SystemLanguage), lang);
            string path = Application.dataPath + "/Heroic Engine/Resources/Localization/" + fileName + ".txt";
            File.WriteAllText(path, $"#{fileName}");
        }

        private void AddTranslation(SystemLanguage lang, string key, string translation)
        {
            if (!_translations.ContainsKey(lang))
            {
                _translations.Add(lang, new Dictionary<string, string>());
            }

            _translations[lang][key] = translation;
        }

        private void LoadTranslations()
        {
            _translations.Clear();

            List<SystemLanguage> availableLanguages = localizationManager.GetAvailableLanguages();

            foreach (var lang in availableLanguages)
            {
                string fileName = Enum.GetName(typeof(SystemLanguage), lang);
                string path = Application.dataPath + "/Heroic Engine/Resources/Localization/" + fileName + ".txt";

                if (Directory.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path);

                    foreach (var line in lines)
                    {
                        if (!line.Contains("#"))
                        {
                            if (!_translations.ContainsKey(lang))
                            {
                                _translations.Add(lang, new Dictionary<string, string>());
                            }
                            var prop = line.Split(Separator);
                            _translations[lang][prop[0]] = prop[1];
                        }
                    }
                }
            }
        }

        private void SaveTranslationsForLang(SystemLanguage lang)
        {
            string fileName = Enum.GetName(typeof(SystemLanguage), lang);
            string fileContent = $"#{fileName}";
            string path = Application.dataPath + "/Resources/Localization/" + fileName + ".txt";

            foreach (var item in _translations[lang])
            {
                fileContent += $"\n{item.Key}={item.Value}";
            }

            File.WriteAllText(path, fileContent);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LoadTranslations();

            EditorGUILayout.LabelField("Localization Key:");
            localizationKey = EditorGUILayout.TextField(localizationKey);
            List<SystemLanguage> availableLanguages = localizationManager.GetAvailableLanguages();

            foreach (var lang in availableLanguages)
            {
                EditorGUILayout.LabelField($"Translation ({lang}):");
                if (!localizedValues.ContainsKey(lang))
                {
                    localizedValues.Add(lang, "");
                }
                localizedValues[lang] = EditorGUILayout.TextField(localizedValues[lang]);
            }

            if (GUILayout.Button("Add Translation"))
            {
                if (!string.IsNullOrEmpty(localizationKey))
                {
                    foreach (var lang in availableLanguages)
                    {
                        AddTranslation(lang, localizationKey, localizedValues[lang]);
                        SaveTranslationsForLang(lang);
                    }

                    statusText = "Translation added.".ToColorizedString(Color.green);
                }
                else
                {
                    statusText = "Translation key is empty!".ToColorizedString(Color.red);
                }
            }

            GUIStyle italicStyle = new GUIStyle();
            italicStyle.fontStyle = FontStyle.Italic;
            italicStyle.richText = true;

            EditorGUILayout.LabelField(statusText, italicStyle);

            EditorGUILayout.Separator();

            if (_translations.Count > 0)
            {
                EditorGUILayout.LabelField("Localization Key:");
                List<string> keys = new List<string>(_translations.ElementAt(0).Value.Keys);
                selectedKey = EditorGUILayout.Popup(selectedKey, _translations.ElementAt(0).Value.Keys.ToArray());
                if (selectedKey != prevKey)
                {
                    foreach (var lang in localizedValues2.Keys.ToArray())
                    {
                        localizedValues2[lang] = "";
                    }
                }
                prevKey = selectedKey;
                localizationKey2 = keys[selectedKey];

                foreach (var lang in availableLanguages)
                {
                    EditorGUILayout.LabelField($"Translation ({lang}):");
                    if (!localizedValues2.ContainsKey(lang))
                    {
                        localizedValues2.Add(lang, "");
                    }
                    if (!_translations.ContainsKey(lang))
                    {
                        _translations.Add(lang, new Dictionary<string, string>());
                    }

                    string val = (!string.IsNullOrEmpty(localizedValues2[lang])) ? localizedValues2[lang]
                        : (_translations[lang].ContainsKey(localizationKey2) ? _translations[lang][localizationKey2] : "");
                    localizedValues2[lang] = GUILayout.TextField(val);
                }

                if (GUILayout.Button("Save Translation"))
                {
                    foreach (var lang in availableLanguages)
                    {
                        AddTranslation(lang, localizationKey2, localizedValues2[lang]);
                        SaveTranslationsForLang(lang);
                    }
                }

                EditorGUILayout.Separator();
            }
            EditorGUILayout.LabelField("Add localization language:");

            List<string> allLanguages = new List<string>(Enum.GetNames(typeof(SystemLanguage)));

            availableLanguages.ForEach(al => allLanguages.Remove(Enum.GetName(typeof(SystemLanguage), al)));

            selectedLang = EditorGUILayout.Popup(selectedLang, allLanguages.ToArray());
            SystemLanguage newLanguage = (SystemLanguage)selectedLang;

            if (GUILayout.Button("Add Language"))
            {
                AddNewLanguage(newLanguage);
            }
        }
    }
}
