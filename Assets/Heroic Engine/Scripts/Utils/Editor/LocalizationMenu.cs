using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public class LocalizationMenu : EditorWindow
    {
        private const char Separator = '=';
        Dictionary<SystemLanguage, Dictionary<string, string>> _translations = new Dictionary<SystemLanguage, Dictionary<string, string>>();
        private List<SystemLanguage> _availableLanguages = new List<SystemLanguage>();
        int selectedKey = 0;
        int prevKey = 0;
        int selectedLang = 0;
        string localizationKey = "";
        string localizationKey2 = "";
        Dictionary<SystemLanguage, string> localizedValues = new Dictionary<SystemLanguage, string>();
        Dictionary<SystemLanguage, string> localizedValues2 = new Dictionary<SystemLanguage, string>();
        string statusText;

        [MenuItem("Tools/HeroicEngine/Edit Localizations")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationMenu>("Localization Editor");
        }

        private void OnGUI()
        {
            LoadTranslations();

            GUILayout.Label("Localization Key:");
            localizationKey = GUILayout.TextField(localizationKey);

            foreach (var lang in _availableLanguages)
            {
                GUILayout.Label($"Translation ({lang}):");
                if (!localizedValues.ContainsKey(lang))
                {
                    localizedValues.Add(lang, "");
                }
                localizedValues[lang] = GUILayout.TextField(localizedValues[lang]);
            }

            if (GUILayout.Button("Add Translation"))
            {
                if (!string.IsNullOrEmpty(localizationKey))
                {
                    foreach (var lang in _availableLanguages)
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
                GUILayout.Label("Localization Key:");
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

                foreach (var lang in _availableLanguages)
                {
                    GUILayout.Label($"Translation ({lang}):");
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
                    foreach (var lang in _availableLanguages)
                    {
                        AddTranslation(lang, localizationKey2, localizedValues2[lang]);
                        SaveTranslationsForLang(lang);
                    }
                }

                EditorGUILayout.Separator();
            }
            GUILayout.Label("Add localization language:");

            List<string> allLanguages = new List<string>(Enum.GetNames(typeof(SystemLanguage)));

            _availableLanguages.ForEach(al => allLanguages.Remove(Enum.GetName(typeof(SystemLanguage), al)));

            selectedLang = EditorGUILayout.Popup(selectedLang, allLanguages.ToArray());
            SystemLanguage newLanguage = (SystemLanguage)selectedLang;

            if (GUILayout.Button("Add Language"))
            {
                AddNewLanguage(newLanguage);
            }
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
            _availableLanguages.Clear();

            TextAsset[] files = Resources.LoadAll<TextAsset>("Localization");

            for (int i = 0; i < files.Length; i++)
            {
                SystemLanguage fileLanguage = SystemLanguage.English;

                foreach (var line in files[i].text.Split('\n'))
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

        private void SaveTranslationsForLang(SystemLanguage lang)
        {
            string fileName = Enum.GetName(typeof(SystemLanguage), lang);
            string fileContent = $"#{fileName}";
            string path = Application.dataPath + "/Heroic Engine/Resources/Localization/" + fileName + ".txt";

            foreach (var item in _translations[lang])
            {
                fileContent += $"\n{item.Key}={item.Value}";
            }

            File.WriteAllText(path, fileContent);
        }
    }
}