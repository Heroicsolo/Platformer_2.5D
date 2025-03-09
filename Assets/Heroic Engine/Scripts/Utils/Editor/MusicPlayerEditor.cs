using HeroicEngine.Enums;
using HeroicEngine.Systems.Audio;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomEditor(typeof(MusicPlayer))]
    public class MusicPlayerEditor : UnityEditor.Editor
    {
        string musicEntryName;
        string enumPath = "Assets/Heroic Engine/Scripts/Enums/";
        string enumName = "MusicEntryType";
        List<string> musicEntriesNames;
        string statusText;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            musicEntriesNames = new List<string>(Enum.GetNames(typeof(MusicEntryType)));

            EditorGUILayout.BeginVertical(GUI.skin.box);

            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.richText = true;

            EditorGUILayout.LabelField("New music entries registration".ToColorizedString(Color.white), headerStyle);

            musicEntryName = EditorGUILayout.TextField("Music entry name", musicEntryName);

            if (GUILayout.Button("Add Music Entry"))
            {
                if (!IsEntryNameValid())
                {
                    statusText = "Incorrect entry type name!".ToColorizedString(Color.red);
                }
                else if (musicEntriesNames.Contains(musicEntryName))
                {
                    statusText = "This entry is already registered!".ToColorizedString(Color.red);
                }
                else
                {
                    musicEntriesNames.Add(musicEntryName);
                    EnumUtils.WriteToEnum(enumPath, enumName, musicEntriesNames);
                    statusText = "Music entry type registered.".ToColorizedString(Color.green);
                    musicEntryName = "";
                }
            }

            GUIStyle italicStyle = new GUIStyle();
            italicStyle.fontStyle = FontStyle.Italic;
            italicStyle.richText = true;

            EditorGUILayout.LabelField(statusText, italicStyle);

            EditorGUILayout.EndVertical();
        }

        private bool IsEntryNameValid()
        {
            return !string.IsNullOrEmpty(musicEntryName) && char.IsLetter(musicEntryName[0]) && !musicEntryName.Contains(" ");
        }
    }
}