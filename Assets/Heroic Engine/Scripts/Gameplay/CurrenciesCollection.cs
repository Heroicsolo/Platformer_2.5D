using HeroicEngine.Systems.Gameplay;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Gameplay
{
    public class CurrenciesCollection : ScriptableObject
    {
        [SerializeField] private List<CurrencyInfo> currencyInfos = new List<CurrencyInfo>();

        public List<CurrencyInfo> CurrencyInfos => currencyInfos;

        public void RegisterCurrency(CurrencyInfo currencyInfo)
        {
            currencyInfos.Add(currencyInfo);
        }
    }
}