using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeroicEngine.Systems.Inputs
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        private Vector3 movementDirection;
        private Dictionary<KeyCode, UnityAction> keyDownCallbacks = new Dictionary<KeyCode, UnityAction>();

        public void AddKeyDownListener(KeyCode key, UnityAction callback)
        {
            if (!keyDownCallbacks.ContainsKey(key))
            {
                keyDownCallbacks.Add(key, callback);
            }
            else
            {
                keyDownCallbacks[key] += callback;
            }
        }

        public void RemoveKeyDownListener(KeyCode key, UnityAction callback)
        {
            if (keyDownCallbacks.ContainsKey(key))
            {
                keyDownCallbacks[key] -= callback;
            }
        }

        public Vector3 GetMovementDirection()
        {
            return movementDirection;
        }

        void Update()
        {
            movementDirection = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                movementDirection += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                movementDirection -= Vector3.forward;
            }
            if (Input.GetKey(KeyCode.D))
            {
                movementDirection += Vector3.right;
            }
            if (Input.GetKey(KeyCode.A))
            {
                movementDirection -= Vector3.right;
            }

            movementDirection.Normalize();

            foreach (var key in keyDownCallbacks.Keys)
            {
                if (Input.GetKeyDown(key))
                {
                    keyDownCallbacks[key]?.Invoke();
                }
            }
        }
    }
}