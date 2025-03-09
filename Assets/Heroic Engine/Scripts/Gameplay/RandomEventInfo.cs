using HeroicEngine.Utils;
using HeroicEngine.Utils.Data;
using HeroicEngine.Utils.Editor;
using UnityEngine;

namespace HeroicEngine.Gameplay
{
    public class RandomEventInfo : ConstructableSO
    {
        [ReadonlyField][SerializeField] private string eventType;
        [SerializeField][Range(0f, 1f)] private float chance;
        [SerializeField]
        [Tooltip("If true, system will guarantee that event with 1/N chance will occur in less than N+1 attempts\nOtherwise, it will be pure random")]
        private bool badLuckProtection = true;
        [SerializeField]
        [ConditionalHide("badLuckProtection", true, true)]
        [Tooltip("If true, system will decrease event chance when it occurs, the earlier it occurs - the bigger that decrease")]
        private bool goodLuckProtection = true;
        [SerializeField]
        [Tooltip("This sound will be played automatically when this event occurs")] 
        private AudioClip eventSound;

        public string EventType => eventType;
        public float Chance => chance;
        public bool BadLuckProtection => badLuckProtection;
        public bool GoodLuckProtection => goodLuckProtection;
        public AudioClip EventSound => eventSound;

        public override void Initialize()
        {
            this.eventType = (string)parameters[0];
            this.chance = (float)parameters[1];
            this.badLuckProtection = (bool)parameters[2];
            this.goodLuckProtection = (bool)parameters[3];
            this.eventSound = (AudioClip)parameters[4];
        }
    }
}