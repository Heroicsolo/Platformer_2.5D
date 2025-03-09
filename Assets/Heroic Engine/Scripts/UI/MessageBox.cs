using HeroicEngine.Systems;
using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    public class MessageBox : UIPart
    {
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI messageLabel;
        [SerializeField] private Transform buttonsHolder;
        [SerializeField] private Button buttonPrefab;

        [Inject] private ITimeManager timeManager;

        private bool pauseGame;
        private List<Button> activeButtons = new List<Button>();

        public void Show(string title, string message, bool pauseGame, params MessageBoxButton[] buttons)
        {
            base.Show();

            titleLabel.text = title;
            messageLabel.text = message;

            this.pauseGame = pauseGame;

            ClearButtons();

            foreach (var button in buttons)
            {
                Button newBtn = Instantiate(buttonPrefab, buttonsHolder);
                
                if (button.callback != null)
                {
                    newBtn.onClick.AddListener(button.callback);
                }

                newBtn.onClick.AddListener(() => { Hide(); });

                TMP_Text label = newBtn.GetComponentInChildren<TMP_Text>();

                if (label != null)
                {
                    label.text = button.text;
                }

                activeButtons.Add(newBtn);
            }

            if (pauseGame)
            {
                timeManager.PauseGame();
            }
        }

        public override void Hide()
        {
            base.Hide();

            if (pauseGame)
            {
                timeManager.ResumeGame();
            }
        }

        private void ClearButtons()
        {
            foreach (Button button in activeButtons.ToArray())
            {
                Destroy(button.gameObject);
            }
            activeButtons.Clear();
        }
    }

    public struct MessageBoxButton
    {
        public string text;
        public UnityAction callback;
    }
}