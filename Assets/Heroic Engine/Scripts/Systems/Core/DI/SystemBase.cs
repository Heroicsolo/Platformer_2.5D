using UnityEngine;

namespace HeroicEngine.Systems.DI
{
    public class SystemBase : MonoBehaviour, ISystem
    {
        private void Start()
        {
            InjectionManager.RegisterSystem(this);
        }

        private void OnDestroy()
        {
            InjectionManager.UnregisterSystem(this);
        }
    }
}