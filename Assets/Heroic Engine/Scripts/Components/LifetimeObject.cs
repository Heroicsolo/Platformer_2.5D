using HeroicEngine.Utils.Pooling;
using UnityEngine;

namespace HeroicEngine.Components
{
    public class LifetimeObject : PooledObject
    {
        [SerializeField] [Min(0f)] private float lifetime = 5f;

        private float timeToDeath;

        private void OnEnable()
        {
            timeToDeath = lifetime;
        }

        private void Update()
        {
            if (timeToDeath > 0f)
            {
                timeToDeath -= Time.deltaTime;

                if (timeToDeath <= 0f)
                {
                    PoolSystem.ReturnToPool(this);
                }
            }
        }
    }
}