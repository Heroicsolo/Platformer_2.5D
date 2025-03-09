using HeroicEngine.Utils.Pooling;
using UnityEngine;

namespace HeroicEngine.Examples
{
    [CreateAssetMenu(fileName = "NewDuelSkill", menuName = "Tools/HeroicEngine/Examples/New Duel Skill")]
    public class DuelSkillInfo : ScriptableObject
    {
        public string Title;
        public Sprite Icon;
        public AudioClip Sound;
        public AnimatorOverrideController AnimatorOverride;
        public PooledParticleSystem Particles;
        [Min(0f)] public float Damage = 0f;
        [Min(0f)] public float Healing = 0f;
        public float UsageCost = 10f;
        [Min(1)] public int Cooldown = 1;
        [Range(0f, 1f)] public float StunChance = 0f;
        [Min(0f)] public float SkillAnimLength = 1f;
        [Range(0f, 1f)] public float CritChance = 0.1f;

        public void Perform(DuelCharacterBase user, DuelCharacterBase target)
        {
            user.Heal(Healing);
            target.GetDamage(Random.value <= CritChance ? 2 * Damage : Damage);
            if (Random.value <= StunChance)
            {
                target.Stun();
            }
            user.OnSkillUsed(this);
        }
    }
}