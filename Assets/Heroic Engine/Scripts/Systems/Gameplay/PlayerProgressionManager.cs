using HeroicEngine.Systems;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.UI;
using HeroicEngine.Systems.DI;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeroicEngine.Gameplay
{
    public class PlayerProgressionManager : SystemBase, IPlayerProgressionManager
    {
        private const string MainMenuSceneName = "MainMenuScene";
        private const string LevelUpEventName = "LevelUp";
        private const string ExpChangedEventName = "ExpChanged";
        private const string PlayerProgressionStateKey = "PlayerProgression";

        [Header("Progression Params")]
        [SerializeField] private PlayerProgressionParams playerProgressionParams;

        [Inject] private IEventsManager eventsManager;
        [Inject] private ISoundsManager soundsManager;
        [Inject] private IUIController uiController;

        private ProgressionState playerSaves;

        private int expPerCurrentLevel;

        public void ResetState()
        {
            playerSaves = new ProgressionState
            {
                currentLevel = 1,
                currentExp = 0,
            };

            OnExpChanged();

            SaveState();
        }

        public (int, int, int) GetPlayerLevelState()
        {
            return (playerSaves.currentLevel, playerSaves.currentExp, GetNeededExpForLevelUp());
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public int GetExpPerCurrentLevel()
        {
            return expPerCurrentLevel;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (SceneManager.GetActiveScene().name == MainMenuSceneName)
            {
                return;
            }

            expPerCurrentLevel = 0;
        }

        public void AddExperience(int amount)
        {
            playerSaves.currentExp += amount;

            expPerCurrentLevel += amount;

            int neededExp = GetCurrentLevelMaxExp();

            int expTotal = playerSaves.currentExp;
            bool lvlChanged = false;

            while (expTotal >= neededExp)
            {
                playerSaves.currentLevel++;
                lvlChanged = true;
                expTotal -= neededExp;
                neededExp = GetCurrentLevelMaxExp();
            }

            playerSaves.currentExp = expTotal;

            if (lvlChanged)
            {
                eventsManager.TriggerEvent(LevelUpEventName, playerSaves.currentLevel);

                if (playerProgressionParams.LevelUpSound != null)
                {
                    soundsManager.PlayClip(playerProgressionParams.LevelUpSound);
                }
            }

            eventsManager.TriggerEvent(ExpChangedEventName, playerSaves.currentExp, GetNeededExpForLevelUp());
            OnExpChanged();

            SaveState();
        }

        public int GetNeededExpForLevelUp()
        {
            return GetCurrentLevelMaxExp() - playerSaves.currentExp;
        }

        public int GetCurrentLevelMaxExp()
        {
            return Mathf.CeilToInt(playerProgressionParams.BaseExpForLevel * (1f + playerProgressionParams.ExpForLevelMultCoef * Mathf.Pow(playerSaves.currentLevel, playerProgressionParams.ExpForLevelDegreeCoef)));
        }

        private void OnExpChanged()
        {
            uiController.UpdateExperiencePanel(playerSaves.currentLevel, playerSaves.currentExp, GetCurrentLevelMaxExp());
        }

        private void LoadState()
        {
            string playerSavesString = PlayerPrefs.GetString(PlayerProgressionStateKey, "");

            if (!string.IsNullOrEmpty(playerSavesString))
            {
                playerSaves = JsonUtility.FromJson<ProgressionState>(playerSavesString);
            }
            else
            {
                playerSaves = new ProgressionState
                {
                    currentLevel = 1,
                    currentExp = 0,
                };
            }
        }

        private void SaveState()
        {
            string playerSavesString = JsonUtility.ToJson(playerSaves);
            PlayerPrefs.SetString(PlayerProgressionStateKey, playerSavesString);
        }

        private void Awake()
        {
            LoadState();
        }

        private void Start()
        {
            OnExpChanged();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnApplicationQuit()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SaveState();
        }
    }

    [Serializable]
    public struct ProgressionState
    {
        public int currentExp;
        public int currentLevel;
    }
}