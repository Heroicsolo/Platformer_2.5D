using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Gameplay;
using UnityEngine;
using UnityEngine.Events;

namespace HeroicEngine.Components.Combat
{
    public abstract class Hittable : MonoBehaviour
    {
        [Inject] private IHittablesManager hittablesManager;

        protected float currHealth;
        [SerializeField] protected float maxHealth;
        [SerializeField] protected TeamType teamType;

        protected UnityEvent<float> OnDamageGot { get; set; }
        protected UnityEvent<float> OnHealingGot { get; set; }
        protected UnityEvent OnDeath { get; set; }

        public TeamType TeamType => teamType;

        /// <summary>
        /// This method assigns hittable object to certain team.
        /// </summary>
        /// <param name="teamType">Team type</param>
        public void SetTeam(TeamType teamType)
        {
            hittablesManager.UnregisterHittable(this);
            this.teamType = teamType;
            hittablesManager.RegisterHittable(this);
        }

        /// <summary>
        /// This method returns hit transform if this hittable. By default, its own transform is returned.
        /// Can be used for targeting enemy projectiles to this hittable entity.
        /// </summary>
        /// <returns>Hit transform</returns>
        public virtual Transform GetHitTransform()
        {
            return transform;
        }

        /// <summary>
        /// This method inflicts certain amount of damage to this hittable entity.
        /// </summary>
        /// <param name="damage">Amount of damage</param>
        public void GetDamage(float damage)
        {
            if (damage <= 0f || currHealth <= 0f)
            {
                return;
            }

            currHealth -= damage;

            OnDamageGot.Invoke(damage);

            if (currHealth <= 0f)
            {
                Kill();
            }
        }

        /// <summary>
        /// This method applies certain amount of healing to this hittable entity.
        /// </summary>
        /// <param name="amount">Amount of healing</param>
        public void Heal(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            currHealth += amount;
            currHealth = Mathf.Clamp(currHealth, 0f, maxHealth);
            OnHealingGot.Invoke(amount);
        }

        /// <summary>
        /// Instantly kills hittable entity.
        /// </summary>
        public void Kill()
        {
            currHealth = 0f;
            hittablesManager.UnregisterHittable(this);
            OnDeath.Invoke();
        }

        /// <summary>
        /// Resets current health to max value.
        /// </summary>
        public void ResetHealth()
        {
            currHealth = maxHealth;
            hittablesManager.RegisterHittable(this);
        }

        /// <summary>
        /// Returns true in case if entity is dead, otherwise false.
        /// </summary>
        /// <returns>Is dead?</returns>
        public bool IsDead()
        {
            return currHealth <= 0f;
        }

        /// <summary>
        /// Returns current percentage of HP.
        /// </summary>
        /// <returns>Current percentage of HP</returns>
        public float GetHPPercentage()
        {
            return currHealth / maxHealth;
        }

        /// <summary>
        /// Returns current amount of HP.
        /// </summary>
        /// <returns>Current amount of HP</returns>
        public float GetHP()
        {
            return currHealth;
        }

        /// <summary>
        /// Returns max amount of HP.
        /// </summary>
        /// <returns>Max amount of HP</returns>
        public float GetMaxHP()
        {
            return maxHealth;
        }

        /// <summary>
        /// This method adds listener to damage got event. If entity gets damage, given listener will be invoked.
        /// </summary>
        /// <param name="onDamageGot">Listener of damage got event</param>
        public void SubscribeToDamageGot(UnityAction<float> onDamageGot)
        {
            OnDamageGot.AddListener(onDamageGot);
        }

        /// <summary>
        /// This method adds listener to healing got event. If entity gets healing, given listener will be invoked.
        /// </summary>
        /// <param name="onHealingGot">Listener of healing got event</param>
        public void SubscribeToHealingGot(UnityAction<float> onHealingGot)
        {
            OnHealingGot.AddListener(onHealingGot);
        }

        /// <summary>
        /// This method adds listener to death event. If entity dies, given listener will be invoked.
        /// </summary>
        /// <param name="onDeath">Listener of death event</param>
        public void SubscribeToDeath(UnityAction onDeath)
        {
            OnDeath.AddListener(onDeath);
        }
        public void RemoveAllListeners()
        {
            OnDamageGot.RemoveAllListeners();
            OnHealingGot.RemoveAllListeners();
            OnDeath.RemoveAllListeners();
        }

        protected virtual void Awake()
        {
            OnDamageGot = new UnityEvent<float>();
            OnHealingGot = new UnityEvent<float>();
            OnDeath = new UnityEvent();
        }

        protected virtual void Start()
        {
            InjectionManager.InjectTo(this);
            hittablesManager.RegisterHittable(this);
        }
    }

    public enum TeamType
    {
        None = 0,
        Player = 1,
        Enemies = 2
    }
}
