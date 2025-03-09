using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.UI;
using HeroicEngine.Systems.DI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using HeroicEngine.Enums;

namespace HeroicEngine.Systems.ScenesManagement
{
    public class ScenesLoader : SystemBase
    {
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Inject] private ILocalizationManager localizationManager;
        [Inject] private MusicPlayer musicPlayer;
        [Inject] private IUIController uiController;
        [Inject] private ITimeManager timeManager;

        private bool sceneLoading = false;

        private void Start()
        {
            localizationManager.ResolveTexts();
            musicPlayer.Play(MusicEntryType.MainMenu);
        }

        public void ToMainMenu()
        {
            uiController.HideUIParts(UIPartType.ExitButton);
            LoadSceneAsync(mainMenuSceneName, () => { uiController.ShowUIParts(UIPartType.MainMenuButtons); });
        }

        public bool IsSceneLoading()
        {
            return sceneLoading;
        }

        public void RestartScene()
        {
            uiController.HideUIParts(UIPartType.FailScreen);
            LoadSceneAsync(SceneManager.GetActiveScene().name);
        }

        public void LoadSceneAsync(string name)
        {
            LoadSceneAsync(name, null);
        }

        public void LoadSceneAsync(string name, Action callback)
        {
            if (sceneLoading)
            {
                return;
            }
            musicPlayer.Stop();
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
            StartCoroutine(LevelLoader(asyncLoad, callback));
        }

        private IEnumerator LevelLoader(AsyncOperation asyncLoad, Action callback)
        {
            timeManager.ResumeGame();

            sceneLoading = true;

            uiController.HideUIParts(UIPartType.MainMenuButtons);
            uiController.ShowUIParts(UIPartType.LoadingScreen);

            asyncLoad.allowSceneActivation = false;
            float smoothProgress = 0f;

            while (asyncLoad != null && !asyncLoad.isDone)
            {
                float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                smoothProgress = Mathf.Lerp(smoothProgress, targetProgress, Time.deltaTime * 5f);
                uiController.UpdateLoadingPanel(smoothProgress, "Loading");

                // Scene Activation Condition (Example: Wait for full load and press a key)
                if (smoothProgress >= 0.99f)
                {
                    uiController.UpdateLoadingPanel(1f, localizationManager.GetLocalizedString("PressAnyKey"));
                    if (Input.anyKeyDown)
                        asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            callback?.Invoke();

            uiController.HideUIParts(UIPartType.LoadingScreen);

            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                uiController.ShowUIParts(UIPartType.ExitButton);
            }

            if (SceneManager.GetActiveScene().name == "MainMenuScene")
            {
                musicPlayer.Play(MusicEntryType.MainMenu);
            }
            else
            {
                musicPlayer.Play(MusicEntryType.InGame);
            }

            sceneLoading = false;
        }
    }
}