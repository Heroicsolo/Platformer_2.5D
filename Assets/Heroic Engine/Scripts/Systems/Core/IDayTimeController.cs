using HeroicEngine.Systems.DI;

namespace HeroicEngine.Systems
{
    public interface IDayTimeController : ISystem
    {
        void SetTimeOfDay(float timeOfDay);
    }
}