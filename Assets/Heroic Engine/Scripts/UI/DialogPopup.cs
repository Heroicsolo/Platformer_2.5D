using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    public class DialogPopup : UIPart, IPointerClickHandler
    {
        [SerializeField] private Image avatar;
        [SerializeField] private TextMeshProUGUI messageLabel;
        [SerializeField] [Min(1f)] private float showTime = 3f;
        [SerializeField] [Min(1)] private int textAppearanceSpeed = 10;
        [SerializeField] private DialogOptionButton optionButtonPrefab;
        [SerializeField] private Transform optionsHolder;

        private bool isAppearing = false;
        private bool canSkip = true;
        private float maxTextAppearanceTime = 1f;
        private UnityAction closeCallback;
        private List<DialogOptionButton> optionButtons = new List<DialogOptionButton>();

        public void ShowMessage(string message, Sprite avatarSprite, UnityAction closeCallback = null, float appearanceTime = 1f)
        {
            gameObject.SetActive(true);
            ClearOptions();
            canSkip = true;
            maxTextAppearanceTime = appearanceTime;
            this.closeCallback = closeCallback;
            if (avatarSprite != null)
            {
                avatar.sprite = avatarSprite;
                avatar.gameObject.SetActive(true);
            }
            else
            {
                avatar.gameObject.SetActive(false);
            }
            StopAllCoroutines();
            StartCoroutine(TextAppearance(message));
        }

        public void ShowMessage(string message, Sprite avatarSprite, float appearanceTime = 1f, params DialogOption[] dialogOptions)
        {
            gameObject.SetActive(true);
            ClearOptions();
            canSkip = dialogOptions.Length == 0;
            maxTextAppearanceTime = appearanceTime;

            if (avatarSprite != null)
            {
                avatar.sprite = avatarSprite;
                avatar.gameObject.SetActive(true);
            }
            else
            {
                avatar.gameObject.SetActive(false);
            }

            foreach (DialogOption opt in dialogOptions)
            {
                DialogOptionButton optionButton = Instantiate(optionButtonPrefab, optionsHolder);
                if (opt.callback != null)
                {
                    UnityAction fullCallback = () => { opt.callback(); Hide(); };
                    optionButton.SetData(opt.text, fullCallback);
                }
                else
                {
                    optionButton.SetData(opt.text, Hide);
                }
                //optionButton.gameObject.SetActive(false);
                optionButtons.Add(optionButton);
            }

            StopAllCoroutines();
            StartCoroutine(TextAppearance(message));
        }

        public override void Hide()
        {
            base.Hide();

            timeToHide = 0f;
            closeCallback?.Invoke();
        }

        private void ClearOptions()
        {
            foreach (var option in optionButtons.ToArray())
            {
                Destroy(option.gameObject);
            }

            optionButtons.Clear();
        }

        private IEnumerator TextAppearance(string text)
        {
            float minInterval = maxTextAppearanceTime / text.Length;
            float interval = Mathf.Min(minInterval, 1f / textAppearanceSpeed);

            messageLabel.text = "";

            int symbolsPrinted = 0;

            isAppearing = true;

            do
            {
                messageLabel.text += text[symbolsPrinted];

                symbolsPrinted++;

                yield return new WaitForSeconds(interval);
            }
            while(symbolsPrinted < text.Length && isAppearing);

            messageLabel.text = text;

            timeToHide = canSkip ? showTime : 0f;

            optionButtons.ForEach(button => button.gameObject.SetActive(true));

            isAppearing = false;
        }

        public void Skip()
        {
            if (isAppearing)
            {
                isAppearing = false;
                timeToHide = canSkip ? showTime : 0f;
            }
            else if (canSkip)
            {
                Hide();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Skip();
        }
    }

    public struct DialogOption
    {
        public string text;
        public UnityAction callback;
    }

    public enum DialogPopupMode
    {
        Fullscreen = 0,
        Corner = 1
    }
}