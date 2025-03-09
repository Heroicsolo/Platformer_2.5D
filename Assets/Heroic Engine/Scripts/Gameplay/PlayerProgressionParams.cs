using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Gameplay
{
    [CreateAssetMenu(fileName = "PlayerProgressionParams", menuName = "Tools/HeroicEngine/Create Player Progression Params")]
    public class PlayerProgressionParams : ScriptableObject
    {
        [Header("Progression Params")]
        [SerializeField][Min(1)] private int baseExpForLevel = 100;
        [SerializeField][Min(0f)] private float expForLevelDegreeCoef = 1.5f;
        [SerializeField][Min(0f)] private float expForLevelMultCoef = 2f;

        [Header("Other")]
        [SerializeField] private AudioClip levelUpSound;

        public int BaseExpForLevel => baseExpForLevel;
        public float ExpForLevelDegreeCoef => expForLevelDegreeCoef;
        public float ExpForLevelMultCoef => expForLevelMultCoef;
        public AudioClip LevelUpSound => levelUpSound;
    }
}