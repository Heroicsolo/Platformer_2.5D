using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    public class DialogOptionButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Button button;

        public void SetData(string text, UnityAction callback)
        {
            label.text = text;
            button.onClick.AddListener(callback);
        }
    }
}