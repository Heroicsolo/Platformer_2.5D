using HeroicEngine.Systems;
using HeroicEngine.Systems.DI;
using System;
using UnityEngine;

namespace HeroicEngine.UI
{
    public class InfoPopup : UIPart
    {
        [SerializeField] private bool pauseGame = false;

        [Inject] private ITimeManager timeManager;

        private Action hideCallback;

        public void SetHideCallback(Action hideCallback)
        {
            this.hideCallback = hideCallback;
        }

        public override void Show()
        {
            InjectionManager.InjectTo(this);

            base.Show();
            if (pauseGame)
            {
                timeManager.PauseGame();
            }
        }

        public override void Hide()
        {
            base.Hide();
            hideCallback?.Invoke();
            if (pauseGame)
            {
                timeManager.ResumeGame();
            }
        }
    }
}