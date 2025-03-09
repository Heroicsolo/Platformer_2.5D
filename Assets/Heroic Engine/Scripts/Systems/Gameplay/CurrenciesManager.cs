using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.UI;
using HeroicEngine.Utils.Data;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using UnityEngine;
using HeroicEngine.Enums;
using HeroicEngine.Utils;

namespace HeroicEngine.Systems.Gameplay
{
    public class CurrenciesManager : SystemBase, ICurrenciesManager
    {
        private const string CurrenciesStateKey = "CurrenciesState";
        private const string CurrencyChangedEvent = "CurrencyChanged";

        [SerializeField] private CurrenciesCollection currencies;
        [SerializeField] private bool saveAfterEachChange = true;

        [Inject] private IEventsManager eventsManager;
        [Inject] private IUIController uiController;

        private CurrenciesState currenciesState;

        public bool GetCurrencyInfo(CurrencyType currencyType, out CurrencyInfo currencyInfo)
        {
            currencyInfo = default;

            int idx = currencies.CurrencyInfos.FindIndex(ci => ci.CurrencyType == currencyType.ToString());

            if (idx >= 0)
            {
                currencyInfo = currencies.CurrencyInfos[idx];
                return true;
            }

            return false;
        }

        public void AddCurrency(CurrencyType currencyType, int amount)
        {
            int idx = currenciesState.Currencies.FindIndex(c => c.CurrencyType == currencyType);

            int newAmount = Mathf.Max(amount, 0);

            if (idx >= 0)
            {
                CurrencyState currencyState = currenciesState.Currencies[idx];
                newAmount = Mathf.Max(currencyState.Amount + amount, 0);
                currencyState.Amount = newAmount;
                currenciesState.Currencies[idx] = currencyState;
            }
            else
            {
                currenciesState.Currencies.Add(new CurrencyState
                {
                    CurrencyType = currencyType,
                    Amount = newAmount
                });
            }

            uiController.UpdateCurrencySlot(currencyType, newAmount);
            eventsManager.TriggerEvent(CurrencyChangedEvent, currencyType, newAmount);

            if (saveAfterEachChange)
            {
                SaveState();
            }
        }

        public void WithdrawCurrency(CurrencyType currencyType, int amount)
        {
            AddCurrency(currencyType, -amount);
        }

        public int GetCurrencyAmount(CurrencyType currencyType)
        {
            int idx = currenciesState.Currencies.FindIndex(c => c.CurrencyType == currencyType);

            if (idx >= 0)
            {
                return currenciesState.Currencies[idx].Amount;
            }

            return 0;
        }

        private void LoadState()
        {
            if (!DataSaver.LoadPrefsSecurely(CurrenciesStateKey, out currenciesState))
            {
                currenciesState = new CurrenciesState
                {
                    Currencies = new List<CurrencyState>(currencies.CurrencyInfos.ConvertAll(ci => new CurrencyState
                    {
                        CurrencyType = (CurrencyType)Enum.Parse(typeof(CurrencyType), ci.CurrencyType), Amount = ci.InitialAmount
                    }))
                };
            }
        }

        public void SaveState()
        {
            DataSaver.SavePrefsSecurely(CurrenciesStateKey, currenciesState);
        }

        private void Awake()
        {
            LoadState();
        }

        private void Start()
        {
            foreach (var currencyState in currenciesState.Currencies)
            {
                uiController.UpdateCurrencySlot(currencyState.CurrencyType, currencyState.Amount);
                eventsManager.TriggerEvent(CurrencyChangedEvent, currencyState.CurrencyType, currencyState.Amount);
            }
        }
    }

    [Serializable]
    public struct CurrenciesState
    {
        public List<CurrencyState> Currencies;
    }

    [Serializable]
    public struct CurrencyState
    {
        public CurrencyType CurrencyType;
        public int Amount;
    }

    [Serializable]
    public struct CurrencyInfo
    {
        [ReadonlyField] public string CurrencyType;
        public Sprite Icon;
        public string Title;
        public int InitialAmount;
    }
}