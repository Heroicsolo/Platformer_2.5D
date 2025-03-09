using HeroicEngine.Systems.DI;

namespace HeroicEngine.Systems
{
    public interface ITimeManager : ISystem
    {
        void PauseGame();
        void ResumeGame();
    }
}