using HeroicEngine.Components;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.UI;
using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using HeroicEngine.Enums;
using UnityEngine.UI;
using HeroicEngine.Systems.Localization;
using UnityEditor;

namespace HeroicEngine.Systems.UI
{
    public class UIController : SystemBase, IUIController
    {
        [SerializeField] private CurrencyUISlot currencySlotPrefab;
        [SerializeField] private Transform currenciesHolder;
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private ResourceBar expBar;
        [SerializeField] private LabelScaler announcementLabel;
        [SerializeField] private TextMeshProUGUI loadingLabel;
        [SerializeField] private Image loadingBar;

        [Inject] private ISoundsManager soundsManager;
        [Inject] private ICurrenciesManager currenciesManager;
        [Inject] private ILocalizationManager localizationManager;

        private Dictionary<UIPartType, List<UIPart>> uiPartsBindings = new Dictionary<UIPartType, List<UIPart>>();
        private Dictionary<CurrencyType, CurrencyUISlot> currencySlots = new Dictionary<CurrencyType, CurrencyUISlot>();
        private float announcementTimeLeft;

        public void UpdateLoadingPanel(float progress, string text)
        {
            loadingLabel.text = localizationManager.GetLocalizedString(text, Mathf.CeilToInt(100f * progress));
            loadingBar.fillAmount = progress;
        }

        public void UpdateExperiencePanel(int level, int currExp, int maxExp)
        {
            levelLabel.text = level.ToString();
            expBar.SetValue(currExp, maxExp);
        }

        public void UpdateCurrencySlot(CurrencyType currencyType, int amount)
        {
            if (currenciesManager == null)
            {
                InjectionManager.InjectTo(this);
            }

            if (currencySlots.ContainsKey(currencyType))
            {
                currencySlots[currencyType].SetAmount(amount);
            }
            else
            {
                CurrencyUISlot currencyUISlot = Instantiate(currencySlotPrefab, currenciesHolder);

                Sprite icon = null;

                if (currenciesManager.GetCurrencyInfo(currencyType, out var info))
                {
                    icon = info.Icon;
                }

                currencyUISlot.SetData(icon, amount);

                currencySlots.Add(currencyType, currencyUISlot);
            }
        }

        public void ShowMessageBox(string title, string message, string buttonText, UnityAction buttonCallback, bool pauseGame = false)
        {
            MessageBox messageBox = uiPartsBindings[UIPartType.MessageBox].First() as MessageBox;

            if (messageBox != null)
            {
                messageBox.Show(title, message, pauseGame, new MessageBoxButton { callback = buttonCallback, text = buttonText });
            }
        }

        public void ShowMessageBox(string title, string message, bool pauseGame, params MessageBoxButton[] buttons)
        {
            MessageBox messageBox = uiPartsBindings[UIPartType.MessageBox].First() as MessageBox;

            if (messageBox != null)
            {
                messageBox.Show(title, message, pauseGame, buttons);
            }
        }

        public void ShowMessageBox(string title, string message, bool pauseGame = false)
        {
            MessageBox messageBox = uiPartsBindings[UIPartType.MessageBox].First() as MessageBox;

            if (messageBox != null)
            {
                messageBox.Show(title, message, pauseGame, new MessageBoxButton { text = "OK", callback = null });
            }
        }

        public void HideCurrentDialog()
        {
            HideUIParts(UIPartType.FullscreenDialogPopup);
            HideUIParts(UIPartType.CornerDialogPopup);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, UnityAction closeCallback = null, float appearanceTime = 1f)
        {
            UIPartType partType = dialogPopupMode == DialogPopupMode.Fullscreen ? UIPartType.FullscreenDialogPopup : UIPartType.CornerDialogPopup;
            DialogPopup dialogPopup = uiPartsBindings[partType].First() as DialogPopup;

            Vector3 savedCamPos = Camera.main.transform.position;
            Camera.main.transform.position = targetTransform.position + targetDistance * targetTransform.forward;
            Quaternion savedCamRot = Camera.main.transform.rotation;
            Camera.main.transform.LookAt(targetTransform.position);

            if (dialogPopup != null)
            {
                UnityAction fullCallback = () =>
                {
                    Camera.main.transform.position = savedCamPos;
                    Camera.main.transform.rotation = savedCamRot;
                    closeCallback();
                };
                dialogPopup.ShowMessage(message, null, fullCallback, appearanceTime);
            }
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, UnityAction closeCallback = null, float appearanceTime = 1f)
        {
            UIPartType partType = dialogPopupMode == DialogPopupMode.Fullscreen ? UIPartType.FullscreenDialogPopup : UIPartType.CornerDialogPopup;
            DialogPopup dialogPopup = uiPartsBindings[partType].First() as DialogPopup;

            if (dialogPopup != null)
            {
                dialogPopup.ShowMessage(message, avatarSprite, closeCallback, appearanceTime);
            }
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, AudioClip sound, UnityAction closeCallback = null, float appearanceTime = 1f)
        {
            if (sound != null)
            {
                soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, avatarSprite, closeCallback, appearanceTime);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, AudioClip sound, UnityAction closeCallback = null, float appearanceTime = 1f)
        {
            if (sound != null)
            {
                soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, targetTransform, targetDistance, closeCallback, appearanceTime);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, float appearanceTime = 1f, params DialogOption[] dialogOptions)
        {
            UIPartType partType = dialogPopupMode == DialogPopupMode.Fullscreen ? UIPartType.FullscreenDialogPopup : UIPartType.CornerDialogPopup;
            DialogPopup dialogPopup = uiPartsBindings[partType].First() as DialogPopup;

            if (dialogPopup != null)
            {
                dialogPopup.ShowMessage(message, avatarSprite, appearanceTime, dialogOptions);
            }
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, float appearanceTime = 1f, params DialogOption[] dialogOptions)
        {
            UIPartType partType = dialogPopupMode == DialogPopupMode.Fullscreen ? UIPartType.FullscreenDialogPopup : UIPartType.CornerDialogPopup;
            DialogPopup dialogPopup = uiPartsBindings[partType].First() as DialogPopup;

            Vector3 savedCamPos = Camera.main.transform.position;
            Camera.main.transform.position = targetTransform.position + targetDistance * targetTransform.forward;
            Quaternion savedCamRot = Camera.main.transform.rotation;
            Camera.main.transform.LookAt(targetTransform.position);

            List<DialogOption> modifiedOptions = new List<DialogOption>();

            foreach (DialogOption option in dialogOptions.ToArray())
            {
                UnityAction fullCallback = () => 
                {
                    Camera.main.transform.position = savedCamPos;
                    Camera.main.transform.rotation = savedCamRot;
                    option.callback();
                };
                modifiedOptions.Add(new DialogOption
                {
                    text = option.text,
                    callback = fullCallback
                });
            }

            if (dialogPopup != null)
            {
                dialogPopup.ShowMessage(message, null, appearanceTime, modifiedOptions.ToArray());
            }
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, AudioClip sound, float appearanceTime = 1f, params DialogOption[] dialogOptions)
        {
            if (sound != null)
            {
                soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, avatarSprite, appearanceTime, dialogOptions);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, AudioClip sound, float appearanceTime = 1f, params DialogOption[] dialogOptions)
        {
            if (sound != null)
            {
                soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, targetTransform, targetDistance, appearanceTime, dialogOptions);
        }

        public void ShowAnnouncement(string message, float showTime = 3f)
        {
            if (!string.IsNullOrEmpty(message))
            {
                announcementLabel.gameObject.SetActive(true);
                announcementLabel.SetLabelText(message);
                announcementTimeLeft = showTime;
            }
        }

        public void HideAnnouncement()
        {
            announcementLabel.gameObject.SetActive(false);
        }

        public void RegisterUIPart(UIPart part)
        {
            if (!uiPartsBindings.ContainsKey(part.PartType))
            {
                uiPartsBindings.Add(part.PartType, new List<UIPart> { part });
            }
            else if (!uiPartsBindings[part.PartType].Contains(part))
            {
                uiPartsBindings[part.PartType].Add(part);
            }
        }

        public void UnregisterUIPart(UIPart part)
        {
            if (uiPartsBindings.ContainsKey(part.PartType))
            {
                uiPartsBindings[part.PartType].Remove(part);
            }
        }

        public void ShowUIParts(UIPartType uiPart)
        {
            uiPartsBindings[uiPart].ForEach(p => p.Show());
        }

        public void HideUIParts(UIPartType uiPart)
        {
            uiPartsBindings[uiPart].ForEach(p => p.Hide());
        }

        public List<UIPart> GetUIPartsOfType(UIPartType type)
        {
            return uiPartsBindings[type];
        }

        public void OnExitButtonClicked()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        private void Awake()
        {
            FindUIParts();
        }

        private void FindUIParts()
        {
            List<UIPart> uiParts = transform.GetComponentsInChildren<UIPart>(true).ToList();

            uiParts.ForEach(part =>
            {
                RegisterUIPart(part);
            });
        }

        private void Update()
        {
            if (announcementLabel.gameObject.activeSelf && announcementTimeLeft > 0f)
            {
                announcementTimeLeft -= Time.deltaTime;

                if (announcementTimeLeft <= 0f)
                {
                    HideAnnouncement();
                }
            }
        }
    }
}