using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Utils.Editor;
using HeroicEngine.Utils.Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Components.Combat
{
    public class Projectile2D : PooledObject
    {
        [Header("Movement")]
        [SerializeField][Min(0f)] private float velocity = 10f;
        [SerializeField] private bool guiding = false;
        [ConditionalHide("guiding", true, true)]
        [SerializeField][Range(0f, 1f)] private float guidingForce = 1f;
        [ConditionalHide("guiding", true, true)]
        [SerializeField][Range(0f, 180f)] private float guidingAngle = 30f;
        [ConditionalHide("guiding", true, true)]
        [SerializeField] private float guidingMaxDist = 200f;
        [ConditionalHide("guiding", true, true)]
        [SerializeField][Min(0f)] private float guidingAngularSpeed = 4f;
        [SerializeField][Min(0f)] private float lifetime = 3f;

        [Header("Damage")]
        [SerializeField][Min(0f)] private float damageMin = 0f;
        [SerializeField][Min(0f)] private float damageMax = 0f;
        [SerializeField][Min(0f)] private float explosionRadius = 1f;

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
        private float targetAngle;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(Transform target, Hittable owner = null)
        {
            InjectionManager.InjectTo(this);

            targetTransform = target;
            teamType = owner != null ? owner.TeamType : TeamType.None;
            timeToDie = lifetime;
            isLaunched = true;

            Vector2 direction = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (launchSound != null)
            {
                soundsManager.PlayClip(launchSound);
            }
        }

        public void Launch(Vector3 direction, Hittable owner = null)
        {
            InjectionManager.InjectTo(this);

            targetTransform = null;
            teamType = owner != null ? owner.TeamType : TeamType.None;
            timeToDie = lifetime;
            isLaunched = true;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

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

                List<Hittable> hittables = hittablesManager.GetOtherTeamsHittablesInRadius(transform.position, explosionRadius, teamType);

                if (hittables.Count > 0)
                {
                    hittables.ForEach(hittable => hittable.GetDamage(Random.Range(damageMin, damageMax)));
                }
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

            if (guiding && guidingForce > 0f && targetTransform != null && Vector2.Distance(targetTransform.position, transform.position) < guidingMaxDist)
            {
                Vector2 dir = (targetTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, angle)) < guidingAngle)
                {
                    float guidedAngle = Mathf.LerpAngle(transform.eulerAngles.z, angle, guidingForce);
                    transform.rotation = Quaternion.Euler(0, 0, guidedAngle);
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

            rb.velocity = transform.right * velocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
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
