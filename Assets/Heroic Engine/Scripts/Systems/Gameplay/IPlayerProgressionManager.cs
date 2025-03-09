using HeroicEngine.Systems.DI;
using System;

namespace HeroicEngine.Gameplay
{
    public interface IPlayerProgressionManager : ISystem
    {
        ///<summary>
        ///This method resets all player progression: level will be set to 1, current experience to 0.
        ///</summary>
        void ResetState();
        ///<summary>
        ///This method returns information about current player progression state.
        ///Information is being returned in next format: (currentLevel, currentExp, neededExpToLevelUp).
        ///</summary>
        (int, int, int) GetPlayerLevelState();
        /// <summary>
        /// This method adds a certain amount of experience.
        /// </summary>
        /// <param name="amount">Amount of experience to add</param>
        void AddExperience(int amount);
        /// <summary>
        /// This method returns left amount of experience needed for level up.
        /// </summary>
        int GetNeededExpForLevelUp();
        /// <summary>
        /// This method returns full amount of experience needed for reaching next level.
        /// </summary>
        int GetCurrentLevelMaxExp();
        /// <summary>
        /// This method returns amount of already earned experience on current level.
        /// </summary>
        int GetExpPerCurrentLevel();
    }
}