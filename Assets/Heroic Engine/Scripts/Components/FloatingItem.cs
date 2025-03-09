using UnityEngine;

namespace HeroicEngine.Components
{
    public class FloatingItem : MonoBehaviour
    {
        public float m_floatingAmplitude = 1f;
        public float m_floatingSpeed = 2f;

        private Vector3 initPos;

        void Start()
        {
            initPos = transform.localPosition;
        }

        /// <summary>
        /// This method instantly moves gameobject to initial position.
        /// </summary>
        public void Restart()
        {
            transform.localPosition = initPos;
        }

        void FixedUpdate()
        {
            transform.localPosition = initPos + transform.up * m_floatingAmplitude * Mathf.Sin(m_floatingSpeed * Time.fixedTime);
        }
    }
}