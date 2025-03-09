using HeroicEngine.Systems.DI;
using UnityEngine.Events;

namespace HeroicEngine.Systems.Events
{
    public interface IEventsManager : ISystem
    {
        void RegisterListener<T>(string eventType, UnityAction<T> listener);
        void RegisterListener<T1, T2>(string eventType, UnityAction<T1, T2> listener);
        void UnregisterListener<T>(string eventType, UnityAction<T> listener);
        void UnregisterListener<T1, T2>(string eventType, UnityAction<T1, T2> listener);
        void TriggerEvent<T>(string eventType, T value);
        void TriggerEvent<T1, T2>(string eventType, T1 value1, T2 value2);
        void RegisterListener(string eventType, UnityAction listener);
        void UnregisterListener(string eventType, UnityAction listener);
        void TriggerEvent(string eventType);
    }
}