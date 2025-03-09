using UnityEngine;

namespace HeroicEngine.Utils.Pooling
{
    public class PooledParticleSystem : PooledObject
    {
        private ParticleSystem particles;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (!particles.IsAlive())
            {
                PoolSystem.ReturnToPool(this);
            }
        }
    }
}