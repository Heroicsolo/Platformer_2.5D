using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    [RequireComponent(typeof(Button))]
    public class SkillButton : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private Image cdIndicator;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Setup(Sprite icon, string costText, UnityAction onClickedCallback)
        {
            button ??= GetComponent<Button>();

            this.icon.sprite = icon;
            costLabel.text = costText;
            cdIndicator.fillAmount = 0f;

            button.onClick.AddListener(onClickedCallback);
        }

        public void SetCooldown(float cooldownPercent)
        {
            cdIndicator.fillAmount = Mathf.Clamp01(cooldownPercent);
            button.interactable = cdIndicator.fillAmount == 0f;
        }
    }
}