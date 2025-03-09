using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    public class ResourceBar : MonoBehaviour
    {
        [SerializeField] private Image bar;
        [SerializeField] private TextMeshProUGUI valueLabel;
        [SerializeField] [Min(0f)] private float valueChangeTime = 0.5f;

        private float currValue;
        private float maxValue;

        public void SetValue(float value, float maxValue)
        {
            this.maxValue = maxValue;
            // Animate value change
            DOTween.To(() => bar.fillAmount, x => bar.fillAmount = x, value / maxValue, valueChangeTime);
            DOTween.To(() => currValue, x => 
            {
                currValue = x;
                valueLabel.text = $"{Mathf.CeilToInt(currValue)}/{Mathf.CeilToInt(maxValue)}";
            }, value, valueChangeTime);
        }
    }
}