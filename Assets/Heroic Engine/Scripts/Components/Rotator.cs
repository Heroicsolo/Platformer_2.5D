using System.Collections;
using UnityEngine;

namespace HeroicEngine.Components
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] Vector3 rotationDirection;
        [SerializeField] float rotationSpeed;
        [SerializeField] bool localSpace = true;
        Transform _transform;

        Coroutine rotationCoroutine;

        private void OnEnable()
        {
            StopRotation();
            _transform = GetComponent<Transform>();
            rotationCoroutine = StartCoroutine(Rotate());
        }

        private void OnDisable()
        {
            StopRotation();
        }

        IEnumerator Rotate()
        {
            do
            {
                _transform.Rotate(rotationDirection * rotationSpeed * Time.deltaTime, localSpace ? Space.Self : Space.World);
                yield return null;
            } while (true);
        }

        void StopRotation()
        {
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        }
    }
}