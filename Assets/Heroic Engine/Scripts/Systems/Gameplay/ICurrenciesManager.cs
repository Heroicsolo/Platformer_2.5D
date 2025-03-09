using HeroicEngine.Enums;
using HeroicEngine.Systems.DI;

namespace HeroicEngine.Systems.Gameplay
{
    public interface ICurrenciesManager : ISystem
    {
        void AddCurrency(CurrencyType currencyType, int amount);
        void WithdrawCurrency(CurrencyType currencyType, int amount);
        int GetCurrencyAmount(CurrencyType currencyType);
        bool GetCurrencyInfo(CurrencyType currencyType, out CurrencyInfo currencyInfo);
        void SaveState();
    }
}