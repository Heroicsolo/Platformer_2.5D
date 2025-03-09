using HeroicEngine.Systems.DI;
using UnityEngine;

namespace HeroicEngine.Systems.Audio
{
    public interface ISoundsManager : ISystem
    {
        void PlayClip(AudioClip clip);
        void StopAllSounds();
    }
}