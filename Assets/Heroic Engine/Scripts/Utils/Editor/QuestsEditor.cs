using HeroicEngine.Enums;
using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Gameplay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomEditor(typeof(QuestManager))]
    public class QuestsEditor : UnityEditor.Editor
    {
        QuestManager myScript;
        string questTitle;
        string questDesc;
        Sprite questSprite;
        string statusText;
        QuestInfo[] nextQuests = new QuestInfo[0];
        QuestTask[] tasks = new QuestTask[0];
        int expReward;
        bool isInitial;
        Vector2 scrollPosition;
        Vector2 scrollPositionTasks;
        string newTaskType;
        string taskTypesPath = "Assets/Heroic Engine/Scripts/Enums/";
        string taskTypesFileName = "QuestTaskType";
        List<string> taskTypes = new List<string>();

        private void OnEnable()
        {
            myScript = (QuestManager)target;
        }

        private bool IsTaskTypeNameValid()
        {
            return !string.IsNullOrEmpty(newTaskType) && char.IsLetter(newTaskType[0]) && !newTaskType.Contains(" ");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            taskTypes = new List<string>(Enum.GetNames(typeof(QuestTaskType)));

            EditorGUILayout.LabelField("Register new quest");

            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.fontSize = 13;
            headerStyle.richText = true;

            EditorGUILayout.LabelField("Quest title:");
            questTitle = EditorGUILayout.TextField(questTitle);
            EditorGUILayout.LabelField("Quest desc:");
            questDesc = EditorGUILayout.TextField(questDesc);

            questSprite = (Sprite)EditorGUILayout.ObjectField("Quest icon", questSprite, typeof(Sprite), false);

            EditorGUILayout.LabelField("Quest Tasks".ToColorizedString(Color.white), headerStyle);

            scrollPositionTasks = EditorGUILayout.BeginScrollView(scrollPositionTasks);

            // Display each prefab in the list
            for (int i = 0; i < tasks.Length; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.LabelField("Task type:");
                List<string> taskTypes = new List<string>(Enum.GetNames(typeof(QuestTaskType)));
                var mainTaskType = (QuestTaskType)EditorGUILayout.Popup((int)tasks[i].TaskType, taskTypes.ToArray());
                int mainTaskAmount = EditorGUILayout.IntField("Task needed amount", tasks[i].NeededAmount);
                mainTaskAmount = Mathf.Clamp(mainTaskAmount, 1, mainTaskAmount);

                // Display the prefab field
                tasks[i] = new QuestTask { TaskType = mainTaskType, NeededAmount = mainTaskAmount };

                // Add a remove button
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    ArrayUtility.RemoveAt(ref tasks, i);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Quest Task"))
            {
                ArrayUtility.Add(ref tasks, default);
            }

            if (GUILayout.Button("Clear Quest Tasks"))
            {
                tasks = new QuestTask[0];
            }

            newTaskType = EditorGUILayout.TextField("New task type", newTaskType);

            if (GUILayout.Button("Register new Task type"))
            {
                if (!IsTaskTypeNameValid())
                {
                    statusText = "Invalid task type name!".ToColorizedString(Color.red);
                }
                else if (taskTypes.Contains(newTaskType))
                {
                    statusText = "This task type already exists!".ToColorizedString(Color.yellow);
                }
                else
                {
                    taskTypes.Add(newTaskType);
                    EnumUtils.WriteToEnum(taskTypesPath, taskTypesFileName, taskTypes);
                    newTaskType = "";
                }
            }

            EditorGUILayout.Space(20);

            expReward = EditorGUILayout.IntField("Experience reward", expReward);
            expReward = Mathf.Clamp(expReward, 0, expReward);

            isInitial = EditorGUILayout.Toggle("Is available from start", isInitial);

            EditorGUILayout.LabelField("Next Quests".ToColorizedString(Color.white), headerStyle);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Display each prefab in the list
            for (int i = 0; i < nextQuests.Length; i++)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                // Display the prefab field
                nextQuests[i] = (QuestInfo)EditorGUILayout.ObjectField($"Quest {i + 1}", nextQuests[i], typeof(QuestInfo), false);

                // Add a remove button
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    ArrayUtility.RemoveAt(ref nextQuests, i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Next Quest"))
            {
                ArrayUtility.Add(ref nextQuests, null);
            }

            if (GUILayout.Button("Clear Next Quests"))
            {
                nextQuests = new QuestInfo[0];
            }

            EditorGUILayout.Space(20);

            GUIStyle italicStyle = new GUIStyle();
            italicStyle.fontStyle = FontStyle.Italic;
            italicStyle.richText = true;

            if (GUILayout.Button("Register Quest"))
            {
                if (!string.IsNullOrEmpty(questTitle))
                {
                    if (!File.Exists($"{Application.dataPath}/Heroic Engine/Scriptables/Quests/{questTitle}.asset"))
                    {
                        QuestInfo asset = CreateInstance<QuestInfo>();
                        asset.Construct(questTitle, questDesc, questSprite, expReward, tasks, nextQuests);
                        asset.Initialize();
                        AssetDatabase.CreateAsset(asset, $"Assets/Heroic Engine/Scriptables/Quests/{questTitle}.asset");
                        AssetDatabase.SaveAssets();
                        myScript.RegisterQuest(asset, isInitial);
                        statusText = "Quest registered.".ToColorizedString(Color.green);
                    }
                    else
                    {
                        statusText = "This quest was already registered!".ToColorizedString(Color.red);
                    }
                }
            }

            EditorGUILayout.LabelField(statusText, italicStyle);
        }
    }
}