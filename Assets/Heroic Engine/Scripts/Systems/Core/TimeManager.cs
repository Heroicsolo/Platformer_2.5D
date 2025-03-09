using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.ScenesManagement;
using UnityEngine;

namespace HeroicEngine.Systems
{
    public class TimeManager : SystemBase, ITimeManager
    {
        private float initialTimeScale;

        [Inject] private ScenesLoader scenesLoader;

        public void PauseGame()
        {
            if (!scenesLoader.IsSceneLoading())
            {
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            Time.timeScale = initialTimeScale;
        }

        public void SetTimeScale(float timeScale)
        {
            initialTimeScale = timeScale;
            ResumeGame();
        }

        private void Start()
        {
            initialTimeScale = Time.timeScale;
        }
    }
}
