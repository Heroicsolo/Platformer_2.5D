#if WEATHER_PACKAGE
using HeroicEngine.Systems.DI;

namespace HeroicEngine.Systems.Weather
{
    public interface IWeatherController : ISystem
    {
        /// <summary>
        /// This method allows to set current weather state. It can be None, RainingLight, RainingMedium or RainingHeavy.
        /// You can also set certain weather change time by fadeDuration parameter.
        /// </summary>
        /// <param name="weatherState">Needed weather state</param>
        /// <param name="fadeDuration">Time of weather change</param>
        void SetWeatherState(WeatherState weatherState, float fadeDuration = 3f);
        /// <summary>
        /// This method allows to set current wind power. It can be None, Light, Medium or Heavy.
        /// </summary>
        /// <param name="windState">Needed wind power</param>
        void SetWindState(WindState windState);
    }
}
#endif