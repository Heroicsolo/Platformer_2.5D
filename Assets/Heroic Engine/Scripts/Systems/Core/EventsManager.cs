using HeroicEngine.Systems.DI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace HeroicEngine.Systems.Events
{
    public class EventsManager : SystemBase, IEventsManager
    {
        private Hashtable eventHash = new();

        // Register a listener for an event
        public void RegisterListener<T>(string eventType, UnityAction<T> listener)
        {
            UnityEvent<T> thisEvent = null;

            string eventKey = GetEventKey<T>(eventType);

            if (eventHash.ContainsKey(eventKey))
            {
                thisEvent = (UnityEvent<T>)eventHash[eventKey];
                thisEvent.AddListener(listener);
                eventHash[eventType] = thisEvent;
            }
            else
            {
                thisEvent = new UnityEvent<T>();
                thisEvent.AddListener(listener);
                eventHash.Add(eventKey, thisEvent);
            }
        }

        public void RegisterListener<T1, T2>(string eventType, UnityAction<T1, T2> listener)
        {
            UnityEvent<T1, T2> thisEvent = null;

            string eventKey = GetEventKey<T1, T2>(eventType);

            if (eventHash.ContainsKey(eventKey))
            {
                thisEvent = (UnityEvent<T1, T2>)eventHash[eventKey];
                thisEvent.AddListener(listener);
                eventHash[eventType] = thisEvent;
            }
            else
            {
                thisEvent = new UnityEvent<T1, T2>();
                thisEvent.AddListener(listener);
                eventHash.Add(eventKey, thisEvent);
            }
        }

        public void RegisterListener(string eventType, UnityAction listener)
        {
            UnityEvent thisEvent = null;

            if (eventHash.ContainsKey(eventType))
            {
                thisEvent = (UnityEvent)eventHash[eventType];
                thisEvent.AddListener(listener);
                eventHash[eventType] = thisEvent;
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                eventHash.Add(eventType, thisEvent);
            }
        }

        // Unregister a listener for an event
        public void UnregisterListener<T>(string eventType, UnityAction<T> listener)
        {
            UnityEvent<T> thisEvent = null;
            string eventKey = GetEventKey<T>(eventType);
            if (eventHash.ContainsKey(eventKey))
            {
                thisEvent = (UnityEvent<T>)eventHash[eventKey];
                thisEvent.RemoveListener(listener);
                eventHash[eventType] = thisEvent;
            }
        }

        public void UnregisterListener<T1, T2>(string eventType, UnityAction<T1, T2> listener)
        {
            UnityEvent<T1, T2> thisEvent = null;
            string eventKey = GetEventKey<T1, T2>(eventType);
            if (eventHash.ContainsKey(eventKey))
            {
                thisEvent = (UnityEvent<T1, T2>)eventHash[eventKey];
                thisEvent.RemoveListener(listener);
                eventHash[eventType] = thisEvent;
            }
        }

        public void UnregisterListener(string eventType, UnityAction listener)
        {
            UnityEvent thisEvent = null;

            if (eventHash.ContainsKey(eventType))
            {
                thisEvent = (UnityEvent)eventHash[eventType];
                thisEvent.RemoveListener(listener);
                eventHash[eventType] = thisEvent;
            }
        }

        // Trigger an event (calls all listeners associated with this event)
        public void TriggerEvent<T>(string eventType, T value)
        {
            UnityEvent<T> thisEvent = null;
            string eventKey = GetEventKey<T>(eventType);
            if (eventHash.ContainsKey(eventKey))
            {
                thisEvent = (UnityEvent<T>)eventHash[eventKey];
                thisEvent.Invoke(value);
            }
        }

        public void TriggerEvent<T1, T2>(string eventType, T1 value1, T2 value2)
        {
            UnityEvent<T1, T2> thisEvent = null;
            string eventKey = GetEventKey<T1, T2>(eventType);
            if (eventHash.ContainsKey(eventKey))
            {
                thisEvent = (UnityEvent<T1, T2>)eventHash[eventKey];
                thisEvent.Invoke(value1, value2);
            }
        }

        public void TriggerEvent(string eventType)
        {
            UnityEvent thisEvent = null;

            if (eventHash.ContainsKey(eventType))
            {
                thisEvent = (UnityEvent)eventHash[eventType];
                thisEvent.Invoke();
            }
        }

        private string GetEventKey<T>(string eventName)
        {
            Type type = typeof(T);
            string key = type.ToString() + eventName;
            return key;
        }

        private string GetEventKey<T1, T2>(string eventName)
        {
            Type type1 = typeof(T1);
            Type type2 = typeof(T2);
            string key = type1.ToString() + type2.ToString() + eventName;
            return key;
        }
    }
}