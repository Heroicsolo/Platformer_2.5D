using HeroicEngine.Gameplay;
using HeroicEngine.Systems.DI;

namespace HeroicEngine.Systems.Gameplay
{
    public interface IRandomEventsManager : ISystem
    {
        /// <summary>
        /// This method attempts to fire random event with certain eventType. Returns true if attempt was successful and event has occurred.
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <returns>true if attempt was successful and event has occurred</returns>
        bool DoEventAttempt(string eventType);
        /// <summary>
        /// This method attempts to fire random event described in eventInfo. Returns true if attempt was successful and event has occurred.
        /// </summary>
        /// <param name="eventInfo">Event info object</param>
        /// <returns>true if attempt was successful and event has occurred</returns>
        bool DoEventAttempt(RandomEventInfo eventInfo);
        /// <summary>
        /// This method resets random event chance (cancels all modifiers applied by Bad Luck Protection and Good Luck Protection logics).
        /// </summary>
        /// <param name="eventType">Event type</param>
        void ResetEventChance(string eventType);
        /// <summary>
        /// This method returns current chance of eventType event occurance.
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <returns>Current chance of event occurance (from 0 to 1)</returns>
        float GetEventChance(string eventType);
    }
}