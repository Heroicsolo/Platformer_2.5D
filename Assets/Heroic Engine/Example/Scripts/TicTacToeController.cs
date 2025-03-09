using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HeroicEngine.Enums;

namespace HeroicEngine.Examples
{
    public class TicTacToeController : MonoBehaviour
    {
        private const string _X = "X";
        private const string _O = "O";
        private const string _YourTurn = "Your turn!";
        private const string _AITurn = "AI turn...";
        private const string _YouWin = "You win!";
        private const string _AIWins = "AI wins!";
        private const string _Tie = "Tie!";

        // Check rows, columns, and diagonals
        private readonly int[,] winPatterns = new int[,] {
            {0, 1, 2}, // Row 1
            {3, 4, 5}, // Row 2
            {6, 7, 8}, // Row 3
            {0, 3, 6}, // Column 1
            {1, 4, 7}, // Column 2
            {2, 5, 8}, // Column 3
            {0, 4, 8}, // Diagonal 1
            {2, 4, 6}  // Diagonal 2
        };

        [SerializeField] private TextMeshProUGUI statusLabel;
        [SerializeField] private List<Button> buttons = new List<Button>();

        [Inject] private IEventsManager eventsManager;
        [Inject] private IQuestManager questManager;
        [Inject] private IPlayerProgressionManager playerProgressionManager;

        private List<TicTacToeSymbol> fieldState = new List<TicTacToeSymbol>();
        private List<TextMeshProUGUI> buttonsLabels = new List<TextMeshProUGUI>();
        private bool gameOver;
        private bool aiTurn;

        public List<float> GetFieldStateAsFloats()
        {
            List<float> result = new List<float>();

            fieldState.ForEach(s =>
            {
                switch (s)
                {
                    case TicTacToeSymbol.None: result.Add(0f); break;
                    case TicTacToeSymbol.O: result.Add(-1f); break;
                    case TicTacToeSymbol.X: result.Add(1f); break;
                }
            });

            return result;
        }

        public void SetField(int cell, TicTacToeSymbol symbol)
        {
            if (gameOver)
            {
                return;
            }

            fieldState[cell] = symbol;

            switch (symbol)
            {
                case TicTacToeSymbol.None:
                    buttonsLabels[cell].text = string.Empty;
                    break;
                case TicTacToeSymbol.X:
                    buttonsLabels[cell].text = _X;
                    statusLabel.text = _YourTurn;
                    aiTurn = false;
                    break;
                case TicTacToeSymbol.O:
                    buttonsLabels[cell].text = _O;
                    aiTurn = true;
                    break;
            }

            CheckField();
        }

        private void CheckField()
        {
            for (int i = 0; i < winPatterns.GetLength(0); i++)
            {
                if (fieldState[winPatterns[i, 0]] == TicTacToeSymbol.X &&
                    fieldState[winPatterns[i, 1]] == TicTacToeSymbol.X &&
                    fieldState[winPatterns[i, 2]] == TicTacToeSymbol.X)
                {
                    gameOver = true;
                    statusLabel.text = _AIWins;
                    eventsManager.TriggerEvent("TicTac_AI_Win");
                    Invoke(nameof(ResetField), 3f);
                    return;
                }

                if (fieldState[winPatterns[i, 0]] == TicTacToeSymbol.O &&
                    fieldState[winPatterns[i, 1]] == TicTacToeSymbol.O &&
                    fieldState[winPatterns[i, 2]] == TicTacToeSymbol.O)
                {
                    gameOver = true;
                    statusLabel.text = _YouWin;
                    eventsManager.TriggerEvent("TicTac_Player_Win");
                    questManager.AddProgress(QuestTaskType.GameWon, 1);
                    playerProgressionManager.AddExperience(10);
                    Invoke(nameof(ResetField), 3f);
                    return;
                }
            }

            foreach (var s in fieldState)
            {
                if (s == TicTacToeSymbol.None)
                    return;
            }

            gameOver = true;
            statusLabel.text = _Tie;
            eventsManager.TriggerEvent("TicTac_Tie");
            Invoke(nameof(ResetField), 3f);
        }

        private void Start()
        {
            InjectionManager.InjectTo(this);

            for (int i = 0; i < buttons.Count; i++)
            {
                int index = i;
                buttons[i].onClick.AddListener(() => OnButtonClick(index));
                buttonsLabels.Add(buttons[i].GetComponentInChildren<TextMeshProUGUI>());
                fieldState.Add(TicTacToeSymbol.None);
            }

            ResetField();
        }

        private void OnButtonClick(int index)
        {
            if (fieldState[index] == TicTacToeSymbol.None && !gameOver && !aiTurn)
            {
                SetField(index, TicTacToeSymbol.O);

                if (!gameOver)
                {
                    eventsManager.TriggerEvent("TicTac_Player_Turn_End", index);
                    statusLabel.text = _AITurn;
                }
            }
        }

        private void ResetField()
        {
            buttonsLabels.ForEach(lbl => lbl.text = string.Empty);
            for (int i = 0; i < fieldState.Count; i++)
            {
                fieldState[i] = TicTacToeSymbol.None;
            }
            statusLabel.text = _AITurn;
            gameOver = false;
            aiTurn = true;
            eventsManager.TriggerEvent("TicTac_Reset");
        }

        public enum TicTacToeSymbol
        {
            None,
            O,
            X
        }
    }
}