using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Utils;
using HeroicEngine.Utils.Editor;
using HeroicEngine.Utils.Math;
using HeroicEngine.Utils.Pooling;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace HeroicEngine.Components.Combat
{
    public class Projectile : PooledObject
    {
        [Header("Movement")]
        [SerializeField] [Min(0f)] private float velocity = 10f;
        [SerializeField] private bool guiding = false;
        [ConditionalHide("guiding", true, true)]
        [SerializeField] [Range(0f, 1f)] private float guidingForce = 1f;
        [ConditionalHide("guiding", true, true)]
        [SerializeField] [Range(0f, 180f)] private float guidingAngle = 30f;
        [ConditionalHide("guiding", true, true)]
        [SerializeField] private float guidingMaxDist = 200f;
        [ConditionalHide("guiding", true, true)]
        [SerializeField] [Min(0f)] private float guidingAngularSpeed = 4f;
        [SerializeField] [Min(0f)] private float lifetime = 3f;

        [Header("Damage")]
        [SerializeField] [Min(0f)] private float damageMin = 0f;
        [SerializeField] [Min(0f)] private float damageMax = 0f;
        [SerializeField] [Min(0f)] private float explosionRadius = 1f;

        [Header("Effects")]
        [SerializeField] private PooledParticleSystem explosionEffect;
        [SerializeField] private AudioClip launchSound;
        [SerializeField] private AudioClip hitSound;

        [Inject] private IHittablesManager hittablesManager;
        [Inject] private ISoundsManager soundsManager;

        private Transform targetTransform;
        private TeamType teamType;
        private bool isLaunched;
        private float timeToDie;
        private Quaternion targetRot;

        /// <summary>
        /// This method launches projectile from certain owner to the certain target.
        /// If owner is not set, this projectile will be neutral (TeamType.None) and will be able to inflict damage both to player team and enemies team.
        /// </summary>
        /// <param name="target">Target transform</param>
        /// <param name="owner">Owner of this projectile</param>
        public void Launch(Transform target, Hittable owner = null)
        {
            InjectionManager.InjectTo(this);

            targetTransform = target;
            teamType = owner != null ? owner.TeamType : TeamType.None;
            timeToDie = lifetime;
            isLaunched = true;
            transform.LookAt(target);

            if (launchSound != null)
            {
                soundsManager.PlayClip(launchSound);
            }
        }

        /// <summary>
        /// This method launches projectile from certain owner to the certain world direction.
        /// If owner is not set, this projectile will be neutral (TeamType.None) and will be able to inflict damage both to player team and enemies team.
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="owner">Owner of this projectile</param>
        public void Launch(Vector3 direction, Hittable owner = null)
        {
            InjectionManager.InjectTo(this);

            targetTransform = null;
            teamType = owner != null ? owner.TeamType : TeamType.None;
            timeToDie = lifetime;
            isLaunched = true;
            transform.rotation = Quaternion.LookRotation(direction);

            if (launchSound != null)
            {
                soundsManager.PlayClip(launchSound);
            }
        }

        private void Explode()
        {
            if (explosionEffect != null)
            {
                PoolSystem.GetInstanceAtPosition(explosionEffect, explosionEffect.name, transform.position);
            }

            List<Hittable> hittables = hittablesManager.GetOtherTeamsHittablesInRadius(transform.position, explosionRadius, teamType);

            if (hittables.Count > 0)
            {
                hittables.ForEach(hittable => hittable.GetDamage(Random.Range(damageMin, damageMax)));
            }

            targetTransform = null;
            isLaunched = false;

            PoolSystem.ReturnToPool(this);
        }

        private void Update()
        {
            if (!isLaunched)
            {
                return;
            }

            if (timeToDie > 0f)
            {
                timeToDie -= Time.deltaTime;

                if (timeToDie <= 0f)
                {
                    Explode();
                    return;
                }
            }

            if (guiding && guidingForce > 0f && targetTransform == null && targetTransform.Distance(transform) < guidingMaxDist)
            {
                Vector3 dir = (targetTransform.position - transform.position).normalized;

                Vector3 localDir = transform.InverseTransformDirection(dir);

                if (localDir.z > 0f && Mathf.Atan2(localDir.z, localDir.x) < guidingAngle)
                {
                    Vector3 guidedDir = dir * guidingForce + transform.forward * (1f - guidingForce);
                    targetRot = Quaternion.LookRotation(guidedDir, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, guidingAngularSpeed * Time.deltaTime);
                }
                else
                {
                    targetTransform = null;
                }
            }
            else
            {
                targetTransform = null;
            }

            transform.Translate(Time.deltaTime * velocity * transform.forward, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            Hittable hittable = other.GetComponentInParent<Hittable>();

            if (hittable != null && hittable.TeamType != teamType)
            {
                if (!hittable.IsDead())
                {
                    hittable.GetDamage(Random.Range(damageMin, damageMax));
                }

                if (hitSound != null)
                {
                    soundsManager.PlayClip(hitSound);
                }

                Explode();
            }
            else if (hittable == null && !other.isTrigger)
            {
                if (hitSound != null)
                {
                    soundsManager.PlayClip(hitSound);
                }

                Explode();
            }
        }

        private void Start()
        {
            InjectionManager.InjectTo(this);
        }
    }
}