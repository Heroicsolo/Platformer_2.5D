using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Utils;
using HeroicEngine.Systems.DI;
using TMPro;
using UnityEngine;

namespace HeroicEngine.Examples
{
    public class RandomEventTest : MonoBehaviour
    {
        [SerializeField] private RandomEventInfo randEvent;
        [SerializeField] private TextMeshProUGUI debugLabel;
        [SerializeField] private TextMeshProUGUI buttonLabel;

        [Inject] private IRandomEventsManager randomEventsManager;
        [Inject] private ILocalizationManager localizationManager;

        private int attemptNumber = 1;

        private void Start()
        {
            InjectionManager.InjectTo(this);
            debugLabel.text = "";
            randomEventsManager.ResetEventChance(randEvent.EventType);
            buttonLabel.text = localizationManager.GetLocalizedString("RandEventTest", Mathf.FloorToInt(100f * randomEventsManager.GetEventChance(randEvent.EventType)));
        }

        public void DoAttempt()
        {
            if (randomEventsManager.DoEventAttempt(randEvent))
            {
                debugLabel.text = $"{localizationManager.GetLocalizedString("Attempt", attemptNumber)}: "
                    + localizationManager.GetLocalizedString("Success").ToColorizedString(Color.green);
                attemptNumber = 1;
            }
            else
            {
                debugLabel.text = $"{localizationManager.GetLocalizedString("Attempt", attemptNumber)}: "
                    + localizationManager.GetLocalizedString("Fail").ToColorizedString(Color.red);
                attemptNumber++;
            }

            buttonLabel.text = localizationManager.GetLocalizedString("RandEventTest", Mathf.FloorToInt(100f * randomEventsManager.GetEventChance(randEvent.EventType)));
        }
    }
}