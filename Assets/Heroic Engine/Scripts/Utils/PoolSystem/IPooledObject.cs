using UnityEngine;

namespace HeroicEngine.Utils.Pooling
{
    public interface IPooledObject
    {
        string GetName();
        void SetName(string name);
        GameObject GetGameObject();
    }
}
