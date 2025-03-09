using HeroicEngine.Enums;
using HeroicEngine.Systems;
using HeroicEngine.Systems.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomEditor(typeof(UIController))]
    public class UIControllerEditor : UnityEditor.Editor
    {
        UIController myScript;
        string filePath = "Assets/Heroic Engine/Scripts/Enums/";
        string fileName = "UIPartType";
        List<string> uiPartsNames = new List<string>();
        string newPartName = "";
        string statusText = "";

        private void OnEnable()
        {
            myScript = (UIController)target;
            uiPartsNames = new List<string>(Enum.GetNames(typeof(UIPartType)));
        }

        private bool IsPartNameValid()
        {
            return !string.IsNullOrEmpty(newPartName) && char.IsLetter(newPartName[0]) && !newPartName.Contains(" ");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Register new UI Part type");
            newPartName = EditorGUILayout.TextField(newPartName);

            GUIStyle italicStyle = new GUIStyle();
            italicStyle.fontStyle = FontStyle.Italic;
            italicStyle.richText = true;

            if (GUILayout.Button("Save"))
            {
                if (!IsPartNameValid())
                {
                    statusText = "Incorrect UI Part name!".ToColorizedString(Color.red);
                }
                else if (uiPartsNames.Contains(newPartName))
                {
                    statusText = "This UI Part name is already registered!".ToColorizedString(Color.red);
                }
                else
                {
                    uiPartsNames.Add(newPartName);
                    EnumUtils.WriteToEnum(filePath, fileName, uiPartsNames);
                    statusText = "UI Part name registered.".ToColorizedString(Color.green);
                    newPartName = "";
                }
            }

            EditorGUILayout.LabelField(statusText, italicStyle);
        }
    }
}