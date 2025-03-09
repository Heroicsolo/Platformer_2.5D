using HeroicEngine.Systems;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.UI;
using HeroicEngine.Systems.DI;
using UnityEngine;
using HeroicEngine.Enums;

namespace HeroicEngine.UI
{
    public class UIPart : MonoBehaviour
    {
        [SerializeField] private UIPartType partType;
        [SerializeField] private AudioClip showSound;

        [Inject] private IUIController uiController;
        [Inject] private ISoundsManager soundsManager;

        protected float timeToHide = 0f;

        public UIPartType PartType => partType;

        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (showSound != null)
            {
                InjectionManager.InjectTo(this);
                soundsManager.PlayClip(showSound);
            }
        }

        public void ShowTemporary(float timeLength)
        {
            Show();
            timeToHide = timeLength;
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            InjectionManager.InjectTo(this);

            // If this UI Part is child object of UIController object, UIController won't be injected,
            // so we need to find it on scene
            uiController ??= FindObjectOfType<UIController>();
            uiController.RegisterUIPart(this);

            soundsManager ??= FindObjectOfType<SoundsManager>();
        }

        private void OnDestroy()
        {
            uiController.UnregisterUIPart(this);
        }

        private void Update()
        {
            if (timeToHide > 0f)
            {
                timeToHide -= Time.deltaTime;

                if (timeToHide <= 0f)
                {
                    timeToHide = 0f;
                    Hide();
                }
            }
        }
    }
}