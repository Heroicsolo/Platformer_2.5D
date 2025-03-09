using HeroicEngine.Systems.Audio;
using HeroicEngine.Utils;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.UI
{
    public class CountdownController : MonoBehaviour, ICountdownController
    {
        [SerializeField] private List<AudioClip> countdownSounds = new List<AudioClip>();

        [Inject] private ISoundsManager soundsManager;
        [Inject] private IUIController uiController;

        private Action tickCallback;
        private Action endCallback;
        private Action cancelCallback;
        private SlowUpdate slowUpdate;
        private float lifetime;

        public void StartCountdown(float seconds, Action tickCallback, Action endCallback, Action cancelCallback = null)
        {
            if (seconds <= 0)
            {
                return;
            }

            this.tickCallback = tickCallback;
            this.endCallback = endCallback;
            this.cancelCallback = cancelCallback;

            if (slowUpdate != null && slowUpdate.IsRunning())
            {
                slowUpdate.Stop();
            }

            slowUpdate = new SlowUpdate(this, CountdownTick, 1f);
            slowUpdate.Run();

            lifetime = seconds;

            CountdownTick();
        }

        public void CancelCountdown()
        {
            lifetime = 0f;
            uiController.HideAnnouncement();
            slowUpdate.Stop();
            cancelCallback?.Invoke();
        }

        private void CountdownTick()
        {
            int second = Mathf.RoundToInt(lifetime);
            if (countdownSounds.Count >= second && second > 0)
            {
                soundsManager.PlayClip(countdownSounds[second - 1]);
            }
            uiController.ShowAnnouncement(second.ToString());
            tickCallback?.Invoke();
        }

        private void Update()
        {
            if (lifetime > 0f)
            {
                lifetime -= Time.deltaTime;

                if (lifetime <= 0f)
                {
                    lifetime = 0f;
                    slowUpdate.Stop();
                    uiController.HideAnnouncement();
                    endCallback?.Invoke();
                }
            }
        }
    }
}