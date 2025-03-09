using HeroicEngine.Systems.DI;
using UnityEngine;
using UnityEngine.Events;

namespace HeroicEngine.Systems.Inputs
{
    public interface IInputManager : ISystem
    {
        Vector3 GetMovementDirection();
        void AddKeyDownListener(KeyCode key, UnityAction callback);
        void RemoveKeyDownListener(KeyCode key, UnityAction callback);
    }
}