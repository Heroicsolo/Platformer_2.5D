using HeroicEngine.Systems.DI;
using System;
using UnityEngine;

namespace HeroicEngine.Systems
{
    public class DayTimeController : SystemBase, IDayTimeController
    {
        [SerializeField] private Material skyMaterial;
        [SerializeField] private Light sun;
        [SerializeField][Min(0f)] private float dayLength = 60f;
        [SerializeField][Min(0f)] private float nightLength = 60f;
        [SerializeField][Range(0f, 1f)] private float timeOfDay = 0f;

        private Transform sunTransform;
        private float fullDayLength;
        private float currentTimeOfDay;
        private float sunMoveSpeed;
        private float xAngle;

        /// <summary>
        /// Set current time of day
        /// </summary>
        /// <param name="timeOfDay">Time of day (from 0 to 1, where 0 is sunrise, 0.5 is sunset, 1 is sunrise again)</param>
        public void SetTimeOfDay(float timeOfDay)
        {
            this.timeOfDay = Mathf.Clamp01(timeOfDay);
            xAngle = Mathf.Lerp(0f, 360f, this.timeOfDay);
            sunTransform.rotation = Quaternion.Euler(xAngle, 0f, 0f);
        }

        private void Start()
        {
            sunTransform = sun.transform;
            fullDayLength = dayLength + nightLength;
            if (skyMaterial != null)
            {
                RenderSettings.skybox = skyMaterial;
            }
            RenderSettings.sun = sun;
            SetTimeOfDay(timeOfDay);
        }

        private void Update()
        {
            fullDayLength = dayLength + nightLength;

            if (xAngle < 180f)
            {
                sunMoveSpeed = fullDayLength / (2f * dayLength);
            }
            else
            {
                sunMoveSpeed = fullDayLength / (2f * nightLength);
            }

            currentTimeOfDay += sunMoveSpeed * Time.deltaTime;

            if (currentTimeOfDay > fullDayLength)
            {
                currentTimeOfDay = 0f;
            }

            SetTimeOfDay(currentTimeOfDay / fullDayLength);
        }
    }
}