using DG.Tweening;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Events;
using UnityEngine;
using UnityEngine.UI;

public class DamageScreenEffect : MonoBehaviour
{
    private const string PlayerDamagedEvent = "PlayerDamaged";

    [SerializeField] private Color initColor = new Color(1f, 0f, 0f, 0f);
    [SerializeField] private Color peakColor = new Color(1f, 0f, 0f, 0.15f);
    [SerializeField] [Min(0f)] private float blinkTime = 0.2f;

    [Inject] private IEventsManager eventsManager;

    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        image.color = new Color(1f, 0f, 0f, 0f);

        InjectionManager.InjectTo(this);

        eventsManager.RegisterListener(PlayerDamagedEvent, OnPlayerDamaged);
    }

    public void OnPlayerDamaged()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(peakColor, blinkTime / 2f));
        sequence.Append(image.DOColor(initColor, blinkTime / 2f));
        sequence.SetEase(Ease.Flash);
    }
}
