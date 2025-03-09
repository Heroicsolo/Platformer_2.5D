using HeroicEngine.Gameplay;
using System.IO;
using UnityEditor;
using UnityEngine;
using HeroicEngine.Utils.Data;

namespace HeroicEngine.Utils.Editor
{
    public class RandomEventsMenu : EditorWindow
    {
        const string EventsCollectionPath = "Assets/Heroic Engine/Scriptables/RandomEvents/RandomEventsCollection.asset";
        const string RandomEventsPath = "Assets/Heroic Engine/Scriptables/RandomEvents/";
        RandomEventsCollection randomEventsCollection;
        string newEventTypeStr = "";
        float newEventChance = 0f;
        bool badLuckProtection = true;
        bool goodLuckProtection = true;
        string statusText = "";
        AudioClip audioClip;

        [MenuItem("Tools/HeroicEngine/Edit Random Events")]
        public static void ShowWindow()
        {
            GetWindow<RandomEventsMenu>("Random Events Editor");
        }

        private void OnGUI()
        {
            randomEventsCollection ??= AssetDatabase.LoadAssetAtPath<RandomEventsCollection>(EventsCollectionPath);

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
                        if (randomEventsCollection == null)
                        {
                            RandomEventsCollection collection = CreateInstance<RandomEventsCollection>();
                            RandomEventInfo randomEventInfo = collection.CreateItem($"{RandomEventsPath}{newEventTypeStr}.asset", 
                                newEventTypeStr, newEventChance, badLuckProtection, goodLuckProtection, audioClip);
                            randomEventsCollection = collection;
                            randomEventInfo.Initialize();
                        }
                        else
                        {
                            RandomEventInfo randomEventInfo = randomEventsCollection.CreateItem($"{RandomEventsPath}{newEventTypeStr}.asset", 
                                newEventTypeStr, newEventChance, badLuckProtection, goodLuckProtection, audioClip);
                            randomEventInfo.Initialize();
                        }
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