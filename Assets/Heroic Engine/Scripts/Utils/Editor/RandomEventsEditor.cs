using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Gameplay;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomEditor(typeof(RandomEventsManager))]
    public class RandomEventsEditor : UnityEditor.Editor
    {
        RandomEventsManager myScript;
        string newEventTypeStr = "";
        float newEventChance = 0f;
        bool badLuckProtection = true;
        bool goodLuckProtection = true;
        string statusText = "";
        AudioClip audioClip;

        private void OnEnable()
        {
            myScript = (RandomEventsManager)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Register new random event");

            GUIStyle smallInfoStyle = new GUIStyle();
            smallInfoStyle.fontStyle = FontStyle.Italic;
            smallInfoStyle.fontSize = 11;
            smallInfoStyle.wordWrap = true;
            smallInfoStyle.richText = true;

            EditorGUILayout.LabelField("Event name:");
            newEventTypeStr = EditorGUILayout.TextField(newEventTypeStr);
            EditorGUILayout.LabelField("Event chance:");
            newEventChance = EditorGUILayout.Slider(newEventChance, 0f, 1f);
            EditorGUILayout.LabelField("Bad luck protection:");
            EditorGUILayout.LabelField("If true, system will guarantee that event with 1/N chance will occur in less than N+1 attempts\nOtherwise, it will be pure random".ToColorizedString(Color.white), smallInfoStyle);
            badLuckProtection = EditorGUILayout.Toggle(badLuckProtection);
            if (badLuckProtection)
            {
                EditorGUILayout.LabelField("Good luck protection:");
                EditorGUILayout.LabelField("If true, system will decrease event chance when it occurs, the earlier it occurs - the bigger that decrease".ToColorizedString(Color.white), smallInfoStyle);
                goodLuckProtection = EditorGUILayout.Toggle(goodLuckProtection);
            }
            else
            {
                goodLuckProtection = false;
            }

            audioClip = (AudioClip)EditorGUILayout.ObjectField("Event sound", audioClip, typeof(AudioClip), false);

            GUIStyle italicStyle = new GUIStyle();
            italicStyle.fontStyle = FontStyle.Italic;
            italicStyle.richText = true;

            if (GUILayout.Button("Register"))
            {
                if (!string.IsNullOrEmpty(newEventTypeStr))
                {
                    if (!File.Exists($"{Application.dataPath}/Heroic Engine/Scriptables/RandomEvents/{newEventTypeStr}.asset"))
                    {
                        RandomEventInfo asset = CreateInstance<RandomEventInfo>();
                        asset.Construct(newEventTypeStr, newEventChance, badLuckProtection, goodLuckProtection, audioClip);
                        AssetDatabase.CreateAsset(asset, $"Assets/Heroic Engine/Scriptables/RandomEvents/{newEventTypeStr}.asset");
                        AssetDatabase.SaveAssets();
                        myScript.RegisterEvent(asset);
                        statusText = "Random event registered.".ToColorizedString(Color.green);
                    }
                    else
                    {
                        statusText = "This event was already registered!".ToColorizedString(Color.red);
                    }
                }
            }

            EditorGUILayout.LabelField(statusText, italicStyle);
        }
    }
}