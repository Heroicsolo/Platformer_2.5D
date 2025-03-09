using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.UI;
using HeroicEngine.Utils.Data;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HeroicEngine.Enums;

namespace HeroicEngine.Systems.Gameplay
{
    public class QuestManager : SystemBase, IQuestManager
    {
        private const string QuestsStateFileName = "QuestsState";
        private const string QuestStartedEvent = "QuestStarted";
        private const string QuestCompletedEvent = "QuestCompleted";
        private const string QuestProgressMade = "QuestProgress";

        [SerializeField] private QuestsCollection questsCollection;

        [Inject] private IEventsManager eventsManager;
        [Inject] private IUIController uiController;
        [Inject] private ILocalizationManager localizationManager;
        [Inject] private ICurrenciesManager currenciesManager;
        [Inject] private IPlayerProgressionManager playerProgressionManager;

        private QuestsState questsState;
        private Dictionary<string, QuestInfo> currentQuestInfos = new Dictionary<string, QuestInfo>();

        public void RegisterQuest(QuestInfo quest, bool isInitial = false)
        {
            questsCollection.RegisterQuest(quest, isInitial);
        }

        public void StartQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                return;
            }

            if (eventsManager == null)
            {
                InjectionManager.InjectTo(this);
            }

            if (IsQuestActive(questId))
            {
                return;
            }

            QuestInfo questInfo = GetQuestInfo(questId);

            if (questInfo != null)
            {
                currentQuestInfos.Add(questId, questInfo);

                eventsManager.TriggerEvent(QuestStartedEvent, questId);
            }
        }

        public void AddProgress(QuestTaskType questTaskType, int progress)
        {
            List<string> completedQuests = new List<string>();

            currentQuestInfos.Values.ToList().ForEach(quest =>
            {
                int taskIdx = quest.GetQuestTaskIndex(questTaskType);

                if (taskIdx >= 0)
                {
                    int questIdx = questsState.QuestsStates.FindIndex(qs => qs.QuestID == quest.ID);

                    QuestTask questTask = quest.QuestTasks[taskIdx];

                    if (questIdx >= 0)
                    {
                        QuestState questState = questsState.QuestsStates[questIdx];
                        questState.QuestProgress[taskIdx] = Mathf.Min(questState.QuestProgress[taskIdx] + progress, questTask.NeededAmount);
                        questsState.QuestsStates[questIdx] = questState;
                    }
                    else
                    {
                        List<int> questProgress = new();

                        for (int i = 0; i < quest.QuestTasks.Count; i++)
                        {
                            questProgress.Add(0);
                        }

                        questProgress[taskIdx] = Mathf.Min(progress, questTask.NeededAmount);

                        questsState.QuestsStates.Add(new QuestState
                        {
                            QuestID = quest.ID,
                            QuestProgress = questProgress
                        });
                    }

                    eventsManager.TriggerEvent(QuestProgressMade, quest.ID);
                }

                if (IsQuestCompleted(quest))
                {
                    eventsManager.TriggerEvent(QuestCompletedEvent, quest.ID);

                    ShowQuestCompletePopup(quest);

                    GetQuestRewards(quest);

                    completedQuests.Add(quest.ID);

                    if (quest.NextQuestIds != null && quest.NextQuestIds.Count > 0)
                    {
                        quest.NextQuestIds.ForEach(q => StartQuest(q));
                    }
                }
            });

            completedQuests.ForEach(x => currentQuestInfos.Remove(x));

            SaveState();
        }

        public QuestInfo GetQuestInfo(string questId)
        {
            return questsCollection.QuestInfos.Find(q => q.ID == questId);
        }

        public QuestState GetQuestState(string questId)
        {
            int questIdx = questsState.QuestsStates.FindIndex(qs => qs.QuestID == questId);

            if (questIdx >= 0)
            {
                return questsState.QuestsStates[questIdx];
            }

            List<int> questProgress = new();

            QuestInfo quest = GetQuestInfo(questId);

            for (int i = 0; i < quest.QuestTasks.Count; i++)
            {
                questProgress.Add(0);
            }

            return new QuestState { QuestID = questId, QuestProgress = questProgress };
        }

        public int GetQuestTaskProgress(string questId, QuestTaskType questTaskType)
        {
            QuestState questState = GetQuestState(questId);

            QuestInfo questInfo = GetQuestInfo(questId);

            if (questInfo != null)
            {
                int taskIdx = questInfo.GetQuestTaskIndex(questTaskType);

                if (taskIdx >= 0)
                {
                    return questState.QuestProgress[taskIdx];
                }
            }

            return 0;
        }

        public bool IsQuestActive(string questId)
        {
            return currentQuestInfos.ContainsKey(questId);
        }

        public bool IsQuestCompleted(string questId)
        {
            QuestInfo questInfo = GetQuestInfo(questId);

            if (questInfo != null)
            {
                return IsQuestCompleted(questInfo);
            }

            return false;
        }

        private void GetQuestRewards(QuestInfo quest)
        {
            foreach (var reward in quest.CurrencyRewards)
            {
                currenciesManager.AddCurrency(reward.RewardType, reward.Amount);
            }

            playerProgressionManager.AddExperience(quest.ExperienceReward);
        }

        private void ShowQuestCompletePopup(QuestInfo quest)
        {
            uiController.ShowMessageBox(localizationManager.GetLocalizedString("QuestComplete"), 
                localizationManager.GetLocalizedString("QuestCompleteDesc", localizationManager.GetLocalizedString(quest.Title)));
        }

        private bool IsQuestCompleted(QuestInfo quest)
        {
            int questIdx = questsState.QuestsStates.FindIndex(qs => qs.QuestID == quest.ID);

            if (questIdx >= 0)
            {
                QuestState questState = questsState.QuestsStates[questIdx];

                for (int i = 0; i < questState.QuestProgress.Count; i++)
                {
                    if (questState.QuestProgress[i] < quest.QuestTasks[i].NeededAmount)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void Awake()
        {
            LoadState();
        }

        private void Start()
        {
            questsCollection.InitialQuests.ForEach(q => StartQuest(q.ID));
        }

        private void LoadState()
        {
            if (!DataSaver.LoadPrefsSecurely(QuestsStateFileName, out questsState))
            {
                questsState = new QuestsState
                {
                    QuestsStates = new List<QuestState>(),
                    CurrentQuestIds = new List<string>()
                };
            }
        }

        private void SaveState()
        {
            DataSaver.SavePrefsSecurely(QuestsStateFileName, questsState);
        }
    }

    [Serializable]
    public struct QuestsState
    {
        public List<QuestState> QuestsStates;
        public List<string> CurrentQuestIds;
    }

    [Serializable]
    public struct QuestState
    {
        public string QuestID;
        public List<int> QuestProgress;
    }
}