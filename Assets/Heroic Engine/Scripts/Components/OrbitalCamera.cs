using UnityEngine;

namespace HeroicEngine.Components
{
    public class OrbitalCamera : MonoBehaviour
    {
        [SerializeField] private Transform lookToObject;
        [SerializeField] private float flySpeed = 1f;

        private float initHeight;
        private float orbitRadius;
        private Vector3 centerPos;

        private void Start()
        {
            initHeight = transform.position.y;
            centerPos = lookToObject.position;
            centerPos.y = initHeight;
            orbitRadius = (transform.position - centerPos).magnitude;
        }

        private void Update()
        {
            float x = centerPos.x + orbitRadius * Mathf.Sin(flySpeed * Time.time);
            float z = centerPos.z + orbitRadius * Mathf.Cos(flySpeed * Time.time);
            transform.position = new Vector3(x, initHeight, z);
            transform.LookAt(lookToObject);
        }
    }
}