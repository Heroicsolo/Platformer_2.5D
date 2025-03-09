using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Utils.Data;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.Gameplay
{
    public class RandomEventsManager : SystemBase, IRandomEventsManager
    {
        private const string StateFileName = "RandomEventsState";

        [SerializeField] private RandomEventsCollection possibleEvents;

        [Inject] private ISoundsManager soundsManager;

        private RandomEventsState randomEventsState;

        public void RegisterEvent(RandomEventInfo eventInfo)
        {
            possibleEvents.RegisterItem(eventInfo);
        }

        public float GetEventChance(string eventType)
        {
            RandomEventInfo eventInfo = possibleEvents.Items.Find(e => e.EventType == eventType);

            float modifiedChance = eventInfo.Chance;

            int stateIdx = randomEventsState.eventsStates.FindIndex(es => es.eventType == eventType);

            if (stateIdx >= 0)
            {
                modifiedChance += randomEventsState.eventsStates[stateIdx].dropChanceModifier;
            }

            return modifiedChance;
        }

        public void ResetEventChance(string eventType)
        {
            int stateIdx = randomEventsState.eventsStates.FindIndex(es => es.eventType == eventType);

            if (stateIdx >= 0)
            {
                RandomEventState eventState = new RandomEventState
                {
                    eventType = eventType,
                    dropChanceModifier = 0f
                };

                randomEventsState.eventsStates[stateIdx] = eventState;

                SaveState();
            }
        }

        public bool DoEventAttempt(RandomEventInfo eventInfo)
        {
            if (eventInfo.BadLuckProtection)
            {
                float modifiedChance = eventInfo.Chance;

                int stateIdx = randomEventsState.eventsStates.FindIndex(es => es.eventType == eventInfo.EventType);

                if (stateIdx >= 0)
                {
                    modifiedChance += randomEventsState.eventsStates[stateIdx].dropChanceModifier;
                }

                bool isSuccess = UnityEngine.Random.value <= modifiedChance;

                RandomEventState modifiedState = new RandomEventState
                {
                    eventType = eventInfo.EventType,
                    dropChanceModifier = stateIdx >= 0 ? randomEventsState.eventsStates[stateIdx].dropChanceModifier : 0f,
                };

                if (isSuccess)
                {
                    if (eventInfo.EventSound != null)
                    {
                        soundsManager.PlayClip(eventInfo.EventSound);
                    }

                    if (eventInfo.GoodLuckProtection)
                    {
                        modifiedState.dropChanceModifier = -(modifiedChance - eventInfo.Chance);
                    }
                    else
                    {
                        modifiedState.dropChanceModifier = 0f;
                    }
                }
                else
                {
                    modifiedState.dropChanceModifier += eventInfo.Chance;
                }

                if (stateIdx >= 0)
                {
                    randomEventsState.eventsStates[stateIdx] = modifiedState;
                }
                else
                {
                    randomEventsState.eventsStates.Add(modifiedState);
                }

                SaveState();

                return isSuccess;
            }

            return UnityEngine.Random.value <= eventInfo.Chance;
        }

        public bool DoEventAttempt(string eventType)
        {
            RandomEventInfo eventInfo = possibleEvents.Items.Find(e => e.EventType == eventType);

            if (eventInfo == null)
            {
                return false;
            }

            return DoEventAttempt(eventInfo);
        }

        private void LoadState()
        {
            if (!DataSaver.LoadPrefsSecurely(StateFileName, out randomEventsState))
            {
                randomEventsState.eventsStates = new List<RandomEventState>();
            }
        }

        private void SaveState()
        {
            DataSaver.SavePrefsSecurely(StateFileName, randomEventsState);
        }

        private void Awake()
        {
            LoadState();
        }

        [Serializable]
        private struct RandomEventsState
        {
            public List<RandomEventState> eventsStates;
        }

        [Serializable]
        private struct RandomEventState
        {
            public string eventType;
            public float dropChanceModifier;
        }
    }
}