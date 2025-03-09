using System;
using System.Collections;
using UnityEngine;

namespace HeroicEngine.Utils
{
    public class SlowUpdate
    {
        private MonoBehaviour owner;
        private Action action;
        private float period;

        private Coroutine coroutine;
        private bool isRunning;

        public SlowUpdate(MonoBehaviour owner, Action action, float period)
        {
            this.owner = owner;
            this.action = action;
            this.period = period;
        }

        public bool IsRunning()
        {
            return isRunning;
        }

        public void Run()
        {
            if (action != null && owner != null && owner.isActiveAndEnabled)
            {
                coroutine = owner.StartCoroutine(Updater());
                isRunning = true;
            }
        }

        public void Stop()
        {
            if (coroutine != null && owner != null)
            {
                owner.StopCoroutine(coroutine);
                isRunning = false;
            }
        }

        private IEnumerator Updater()
        {
            while (owner != null && owner.isActiveAndEnabled)
            {
                yield return new WaitForSeconds(period);
                action.Invoke();
            }
        }
    }
}