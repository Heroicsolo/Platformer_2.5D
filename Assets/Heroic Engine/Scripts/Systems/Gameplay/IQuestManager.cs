using HeroicEngine.Enums;
using HeroicEngine.Systems.DI;

namespace HeroicEngine.Systems.Gameplay
{
    public interface IQuestManager : ISystem
    {
        /// <summary>
        /// This method starts quest by known questId. If this parameter is not correct or empty, this method will not do anything.
        /// </summary>
        /// <param name="questId">ID of quest. It must be in GUID format.</param>
        void StartQuest(string questId);
        /// <summary>
        /// This method increments progress of all active quests which have a certain task type.
        /// </summary>
        /// <param name="questTaskType">Quest task type</param>
        /// <param name="progress">Amount of task progress to add</param>
        void AddProgress(QuestTaskType questTaskType, int progress);
        /// <summary>
        /// This method returns information about quest with certain ID.
        /// </summary>
        /// <param name="questId">ID of quest. It must be in GUID format.</param>
        QuestInfo GetQuestInfo(string questId);
        /// <summary>
        /// This method returns current state of quest with certain ID. You can use this to get information about quest progress.
        /// </summary>
        /// <param name="questId">ID of quest. It must be in GUID format.</param>
        QuestState GetQuestState(string questId);
        /// <summary>
        /// This method returns current progress for task taskType for quest with questId.
        /// </summary>
        /// <param name="questId">ID of quest. It must be in GUID format.</param>
        /// <param name="taskType">Quest task type</param>
        /// <returns>Current progress of quest task</returns>
        int GetQuestTaskProgress(string questId, QuestTaskType taskType);
        /// <summary>
        /// This method returns true, if quest is completed, otherwise it returns false.
        /// </summary>
        /// <param name="questId">ID of quest. It must be in GUID format.</param>
        /// <returns>true, if quest is completed, otherwise false</returns>
        bool IsQuestCompleted(string questId);
        /// <summary>
        /// This method returns true, if quest was started and is currently running.
        /// </summary>
        /// <param name="questId">ID of quest. It must be in GUID format.</param>
        /// <returns>true, if quest is running, otherwise false</returns>
        bool IsQuestActive(string questId);
    }
}