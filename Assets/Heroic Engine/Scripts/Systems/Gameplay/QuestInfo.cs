using HeroicEngine.Enums;
using HeroicEngine.Utils;
using HeroicEngine.Utils.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeroicEngine.Systems.Gameplay
{
    public class QuestInfo : ConstructableSO
    {
        [ScriptableObjectId][SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private List<QuestTask> questTasks = new List<QuestTask>();
        [SerializeField] private List<CurrencyRewardInfo> currencyRewards = new List<CurrencyRewardInfo>();
        [SerializeField] private int experienceReward;
        [SerializeField] private List<string> nextQuestIds;

        public string ID => id;
        public string Title => title;
        public string Description => description;
        public Sprite Icon => icon;
        public List<QuestTask> QuestTasks => questTasks;
        public List<CurrencyRewardInfo> CurrencyRewards => currencyRewards;
        public int ExperienceReward => experienceReward;
        public List<string> NextQuestIds => nextQuestIds;

        public int GetQuestTaskIndex(QuestTaskType taskType)
        {
            return questTasks.FindIndex(qt => qt.TaskType == taskType);
        }

        public override void Initialize()
        {
            this.title = (string)parameters[0];
            this.description = (string)parameters[1];
            this.icon = (Sprite)parameters[2];
            this.experienceReward = (int)parameters[3];
            var nextQuestsList = (QuestInfo[])parameters[5];
            this.nextQuestIds = new List<string>(nextQuestsList.Select(q => q.ID));
            var questTasksList = (QuestTask[])parameters[4];
            this.questTasks = new List<QuestTask>(questTasksList);
        }
    }

    [Serializable]
    public struct QuestTask
    {
        public QuestTaskType TaskType;
        public int NeededAmount;
    }

    [Serializable]
    public struct CurrencyRewardInfo
    {
        public CurrencyType RewardType;
        public int Amount;
    }
}