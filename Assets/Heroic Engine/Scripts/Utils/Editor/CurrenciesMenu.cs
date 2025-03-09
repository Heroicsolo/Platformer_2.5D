using HeroicEngine.Enums;
using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Gameplay;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public class CurrenciesMenu : EditorWindow
    {
        const string CurrenciesCollectionPath = "Assets/Heroic Engine/Scriptables/Economics/CurrenciesCollection.asset";
        CurrenciesCollection currenciesCollection;
        string currencyName;
        string currencyTitle;
        Sprite icon;
        string filePath = "Assets/Heroic Engine/Scripts/Enums/";
        string fileName = "CurrencyType";
        string statusText;
        int initAmount;
        List<string> currencyNames = new List<string>();

        [MenuItem("Tools/HeroicEngine/Add Currency")]
        public static void ShowWindow()
        {
            GetWindow<CurrenciesMenu>("Register new currency");
        }

        private bool IsCurrencyNameValid()
        {
            return !string.IsNullOrEmpty(currencyName) && char.IsLetter(currencyName[0]) && !currencyName.Contains(" ");
        }

        private void OnGUI()
        {
            currenciesCollection ??= AssetDatabase.LoadAssetAtPath<CurrenciesCollection>(CurrenciesCollectionPath);

            currencyNames = new List<string>(Enum.GetNames(typeof(CurrencyType)));

            EditorGUILayout.LabelField("Create new currency");

            GUIStyle smallInfoStyle = new GUIStyle();
            smallInfoStyle.fontStyle = FontStyle.Italic;
            smallInfoStyle.fontSize = 9;
            smallInfoStyle.wordWrap = true;

            EditorGUILayout.LabelField("Currency name:");
            currencyName = EditorGUILayout.TextField(currencyName);
            EditorGUILayout.LabelField("Currency title:");
            currencyTitle = EditorGUILayout.TextField(currencyTitle);
            
            icon = (Sprite)EditorGUILayout.ObjectField("Currency icon", icon, typeof(Sprite), false);

            initAmount = EditorGUILayout.IntField("Initial amount", initAmount);

            GUIStyle italicStyle = new GUIStyle();
            italicStyle.fontStyle = FontStyle.Italic;
            italicStyle.richText = true;

            if (GUILayout.Button("Register"))
            {
                if (!IsCurrencyNameValid())
                {
                    statusText = "Incorrect currency type name!".ToColorizedString(Color.red);
                }
                else if (currencyNames.Contains(currencyName))
                {
                    statusText = "This currency is already registered!".ToColorizedString(Color.red);
                }
                else
                {
                    currencyNames.Add(currencyName);

                    CurrencyInfo currencyInfo = new CurrencyInfo
                    {
                        CurrencyType = currencyName,
                        Icon = icon,
                        InitialAmount = initAmount,
                        Title = currencyTitle,
                    };

                    if (currenciesCollection == null)
                    {
                        CurrenciesCollection asset = CreateInstance<CurrenciesCollection>();
                        asset.RegisterCurrency(currencyInfo);
                        AssetDatabase.CreateAsset(asset, CurrenciesCollectionPath);
                        AssetDatabase.SaveAssets();
                        currenciesCollection = asset;
                    }
                    else
                    {
                        currenciesCollection.RegisterCurrency(currencyInfo);
                    }

                    EnumUtils.WriteToEnum(filePath, fileName, currencyNames);
                    statusText = "Currency registered.".ToColorizedString(Color.green);
                    currencyName = "";
                }
            }

            EditorGUILayout.LabelField(statusText, italicStyle);
        }
    }
}