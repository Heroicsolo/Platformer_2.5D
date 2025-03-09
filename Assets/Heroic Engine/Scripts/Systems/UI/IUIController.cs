using HeroicEngine.UI;
using HeroicEngine.Systems.DI;
using UnityEngine;
using UnityEngine.Events;
using HeroicEngine.Enums;
using System.Collections.Generic;

namespace HeroicEngine.Systems.UI
{
    public interface IUIController : ISystem
    {
        #region Common Methods
        void ShowUIParts(UIPartType uiPart);
        void HideUIParts(UIPartType uiPart);
        void RegisterUIPart(UIPart part);
        void UnregisterUIPart(UIPart part);
        List<UIPart> GetUIPartsOfType(UIPartType type);
        #endregion
        #region Message Box
        /// <summary>
        /// This method shows message box with certain title and message. It also pauses the game, if pauseGame flag set to true.
        /// </summary>
        void ShowMessageBox(string title, string message, bool pauseGame = false);
        /// <summary>
        /// This method shows message box with certain title, message and buttonText on button. It also pauses the game, if pauseGame flag set to true.
        /// Also, buttonCallback is invoked if user clicks message box button.
        /// </summary>
        void ShowMessageBox(string title, string message, string buttonText, UnityAction buttonCallback, bool pauseGame = false);
        /// <summary>
        /// This method shows message box with certain title, message and buttons described by buttons parameters array.
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="message">Message</param>
        /// <param name="pauseGame">Should game be paused?</param>
        /// <param name="buttons">Buttons parameters</param>
        void ShowMessageBox(string title, string message, bool pauseGame, params MessageBoxButton[] buttons);
        #endregion
        #region Dialogs
        /// <summary>
        /// This method shows dialog with message, certain dialogPopupMode and avatarSprite (which visually represents character who says that message to player).
        /// Dialog appears in amount of time set by appearanceTime parameter and invokes closeCallback right after dialog closing.
        /// </summary>
        /// <param name="dialogPopupMode">Dialog popup mode (Fullscreen or Corner)</param>
        /// <param name="message">Dialog message</param>
        /// <param name="avatarSprite">Avatar sprite (visually represents character who says that message to player). If null, dialog will not display avatar.</param>
        /// <param name="closeCallback">Action which should be invoked after dialog popup closing</param>
        /// <param name="appearanceTime">Time of dialog appearance (in seconds)</param>
        void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, UnityAction closeCallback = null, float appearanceTime = 1f);
        /// <summary>
        /// This method shows dialog with message, certain dialogPopupMode and targetTransform (camera will be targeted to this transform from targetDistance).
        /// Dialog appears in amount of time set by appearanceTime parameter and invokes closeCallback right after dialog closing.
        /// </summary>
        /// <param name="dialogPopupMode">Dialog popup mode (Fullscreen or Corner)</param>
        /// <param name="message">Dialog message</param>
        /// <param name="targetTransform">Target transform (camera will be targeted to it)</param>
        /// <param name="targetDistance">Distance to target (camera will look to target from this distance)</param>
        /// <param name="closeCallback">Action which should be invoked after dialog popup closing</param>
        /// <param name="appearanceTime">Time of dialog appearance (in seconds)</param>
        void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, UnityAction closeCallback = null, float appearanceTime = 1f);
        /// <summary>
        /// This method shows dialog with message, certain dialogPopupMode and avatarSprite (which visually represents character who says that message to player).
        /// Dialog appears in amount of time set by appearanceTime parameter, plays certain sound and invokes closeCallback right after dialog closing.
        /// </summary>
        /// <param name="dialogPopupMode">Dialog popup mode (Fullscreen or Corner)</param>
        /// <param name="message">Dialog message</param>
        /// <param name="avatarSprite">Avatar sprite (visually represents character who says that message to player). If null, dialog will not display avatar.</param>
        /// <param name="sound">Sound to play right after dialog appearance</param>
        /// <param name="closeCallback">Action which should be invoked after dialog popup closing</param>
        /// <param name="appearanceTime">Time of dialog appearance (in seconds)</param>
        void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, AudioClip sound, UnityAction closeCallback = null, float appearanceTime = 1f);
        /// <summary>
        /// This method shows dialog with message, certain dialogPopupMode and targetTransform (camera will be targeted to this transform from targetDistance).
        /// Dialog appears in amount of time set by appearanceTime parameter, plays certain sound and invokes closeCallback right after dialog closing.
        /// </summary>
        /// <param name="dialogPopupMode">Dialog popup mode (Fullscreen or Corner)</param>
        /// <param name="message">Dialog message</param>
        /// <param name="targetTransform">Target transform (camera will be targeted to it)</param>
        /// <param name="targetDistance">Distance to target (camera will look to target from this distance)</param>
        /// <param name="sound">Sound to play right after dialog appearance</param>
        /// <param name="closeCallback">Action which should be invoked after dialog popup closing</param>
        /// <param name="appearanceTime">Time of dialog appearance (in seconds)</param>
        void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, AudioClip sound, UnityAction closeCallback = null, float appearanceTime = 1f);
        /// <summary>
        /// This method shows dialog with message, certain dialogPopupMode and avatarSprite (which visually represents character who says that message to player). Dialog appears in amount of time set by appearanceTime parameter.
        /// Dialog options are presented by dialogOptions array and contains information about available answers and their callbacks.
        /// </summary>
        /// <param name="dialogPopupMode">Dialog popup mode (Fullscreen or Corner)</param>
        /// <param name="message">Dialog message</param>
        /// <param name="avatarSprite">Avatar sprite (visually represents character who says that message to player). If null, dialog will not display avatar.</param>
        /// <param name="appearanceTime">Time of dialog appearance (in seconds)</param>
        /// <param name="dialogOptions">Dialog options (available answers)</param>
        void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, float appearanceTime = 1f, params DialogOption[] dialogOptions);
        /// <summary>
        /// This method shows dialog with message, certain dialogPopupMode and targetTransform (camera will be targeted to this transform from targetDistance). Dialog appears in amount of time set by appearanceTime parameter.
        /// Dialog options are presented by dialogOptions array and contains information about available answers and their callbacks.
        /// </summary>
        /// <param name="dialogPopupMode">Dialog popup mode (Fullscreen or Corner)</param>
        /// <param name="message">Dialog message</param>
        /// <param name="targetTransform">Target transform (camera will be targeted to it)</param>
        /// <param name="targetDistance">Distance to target (camera will look to target from this distance)</param>
        /// <param name="appearanceTime">Time of dialog appearance (in seconds)</param>
        /// <param name="dialogOptions">Dialog options (available answers)</param>
        void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, float appearanceTime = 1f, params DialogOption[] dialogOptions);
        /// <summary>
        /// This method shows dialog with message, certain dialogPopupMode and avatarSprite (which visually represents character who says that message to player). Dialog appears in amount of time set by appearanceTime parameter and plays certain sound.
        /// Dialog options are presented by dialogOptions array and contains information about available answers and their callbacks.
        /// </summary>
        /// <param name="dialogPopupMode">Dialog popup mode (Fullscreen or Corner)</param>
        /// <param name="message">Dialog message</param>
        /// <param name="avatarSprite">Avatar sprite (visually represents character who says that message to player). If null, dialog will not display avatar.</param>
        /// <param name="sound">Sound to play right after dialog appearance</param>
        /// <param name="appearanceTime">Time of dialog appearance (in seconds)</param>
        /// <param name="dialogOptions">Dialog options (available answers)</param>
        void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, AudioClip sound, float appearanceTime = 1f, params DialogOption[] dialogOptions);
        /// <summary>
        /// This method shows dialog with message, certain dialogPopupMode and targetTransform (camera will be targeted to this transform from targetDistance). Dialog appears in amount of time set by appearanceTime parameter and plays certain sound.
        /// Dialog options are presented by dialogOptions array and contains information about available answers and their callbacks.
        /// </summary>
        /// <param name="dialogPopupMode">Dialog popup mode (Fullscreen or Corner)</param>
        /// <param name="message">Dialog message</param>
        /// <param name="targetTransform">Target transform (camera will be targeted to it)</param>
        /// <param name="targetDistance">Distance to target (camera will look to target from this distance)</param>
        /// <param name="sound">Sound to play right after dialog appearance</param>
        /// <param name="appearanceTime">Time of dialog appearance (in seconds)</param>
        /// <param name="dialogOptions">Dialog options (available answers)</param>
        void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, AudioClip sound, float appearanceTime = 1f, params DialogOption[] dialogOptions);

        /// <summary>
        /// This method hides current dialog.
        /// </summary>
        void HideCurrentDialog();
        #endregion
        #region Announcements
        void ShowAnnouncement(string message, float showTime = 3f);
        void HideAnnouncement();
        #endregion

        /// <summary>
        /// This method updates currency slot with currencyType in game UI and sets certain amount of this currency in that slot.
        /// </summary>
        /// <param name="currencyType">Currency type</param>
        /// <param name="amount">Currency amount to show</param>
        void UpdateCurrencySlot(CurrencyType currencyType, int amount);
        /// <summary>
        /// This method updates information about player level and experience in UI.
        /// </summary>
        /// <param name="level">Current player level</param>
        /// <param name="currExp">Current experience</param>
        /// <param name="maxExp">Max experience on this level</param>
        void UpdateExperiencePanel(int level, int currExp, int maxExp);

        void UpdateLoadingPanel(float progress, string text);
    }
}