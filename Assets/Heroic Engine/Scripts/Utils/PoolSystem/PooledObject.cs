using UnityEngine;

namespace HeroicEngine.Utils.Pooling
{
    public class PooledObject : MonoBehaviour, IPooledObject
    {
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public string GetName()
        {
            return gameObject.name;
        }

        public void SetName(string name)
        {
            gameObject.name = name;
        }
    }
}
