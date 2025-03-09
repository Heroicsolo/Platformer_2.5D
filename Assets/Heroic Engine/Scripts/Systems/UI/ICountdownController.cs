using HeroicEngine.Systems.DI;
using System;

namespace HeroicEngine.Systems.UI
{
    public interface ICountdownController : ISystem
    {
        /// <summary>
        /// This method starts cooldown with certain length in seconds, 
        /// it calls tickCallback every tick, endCallback in the end of countdown and cancelCallback in case if countdown is cancelled.
        /// </summary>
        /// <param name="seconds">Time length of countdown (in seconds)</param>
        /// <param name="tickCallback">Action, called every tick</param>
        /// <param name="endCallback">Action, called in the end</param>
        /// <param name="cancelCallback">Action, called if countdown is cancelled</param>
        void StartCountdown(float seconds, Action tickCallback, Action endCallback, Action cancelCallback = null);
        /// <summary>
        /// This method cancels current countdown and instantly calls cancel callback if it was assigned beforehand by StartCountdown method.
        /// </summary>
        void CancelCountdown();
    }
}