using HeroicEngine.Components;
using HeroicEngine.Utils.Math;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    public class CurrencyUISlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountLabel;

        private int currAmount;

        public void SetData(Sprite icon, int amount)
        {
            this.icon.sprite = icon;
            amountLabel.text = $"{amount.ToShortenedNumber()}";
            currAmount = amount;
        }

        public void SetAmount(int amount)
        {
            if (currAmount != amount)
            {
                amountLabel.GetComponent<LabelScaler>().SetLabelText(amount.ToShortenedNumber());
            }
            currAmount = amount;
        }
    }
}