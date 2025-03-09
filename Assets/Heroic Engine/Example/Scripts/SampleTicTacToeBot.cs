using HeroicEngine.AI;
using HeroicEngine.Systems.Events;
using HeroicEngine.Utils.Math;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HeroicEngine.Examples
{
    [RequireComponent(typeof(AIBrain))]
    public class SampleTicTacToeBot : MonoBehaviour
    {
        private const string TaskType = "TicTacToe_0";

        [SerializeField] private TicTacToeController ticTacToeController;
        [SerializeField] private TextMeshProUGUI perceptronLabel;

        [Inject] private IEventsManager eventsManager;

        private int nextCellIdx;
        private List<TurnData> turnsHistory = new List<TurnData>();
        private List<TurnData> playerTurnsHistory = new List<TurnData>();
        private AIBrain brain;
        private Perceptron perceptron;
        private int turnNumber;

        private void Awake()
        {
            brain = GetComponent<AIBrain>();
            brain.SubscribeToBrainInit(PrintPerceptronState);
        }

        private void Start()
        {
            InjectionManager.InjectTo(this);

            eventsManager.RegisterListener<int>("TicTac_Player_Turn_End", OnPlayerTurnEnd);
            eventsManager.RegisterListener("TicTac_AI_Win", OnWin);
            eventsManager.RegisterListener("TicTac_Player_Win", OnLoss);
            eventsManager.RegisterListener("TicTac_Tie", OnTie);
            eventsManager.RegisterListener("TicTac_Reset", OnFieldReset);

            OnFieldReset();
        }

        private void PrintPerceptronState()
        {
            perceptronLabel.text = "";
            perceptron ??= brain.GetPerceptronByTask(TaskType);

            if (perceptron != null)
            {
                for (int i = 0; i < perceptron.InputsNumber; i++)
                {
                    for (int k = 0; k < perceptron.NeuronsNumber; k++)
                    {
                        if (perceptron.Weights[k][i] >= 0f)
                        {
                            perceptronLabel.text += perceptron.Weights[k][i].ToRoundedString(2) + "  | ";
                        }
                        else
                        {
                            perceptronLabel.text += perceptron.Weights[k][i].ToRoundedString(2) + " | ";
                        }
                    }

                    perceptronLabel.text += "\n";
                }
            }
        }

        private void OnDisable()
        {
            eventsManager.UnregisterListener<int>("TicTac_Player_Turn_End", OnPlayerTurnEnd);
            eventsManager.UnregisterListener("TicTac_AI_Win", OnWin);
            eventsManager.UnregisterListener("TicTac_Player_Win", OnLoss);
            eventsManager.UnregisterListener("TicTac_Tie", OnTie);
        }

        private void OnFieldReset()
        {
            List<float> fieldState = ticTacToeController.GetFieldStateAsFloats();

            nextCellIdx = UnityEngine.Random.Range(0, 9);

            turnsHistory.Add(new TurnData
            {
                fieldState = new List<float>(fieldState),
                cellIdx = nextCellIdx,
            });

            // Wait 1 sec. and perform AI turn on field
            Invoke(nameof(DoTurn), 1f);
        }

        private void ResetState()
        {
            turnsHistory.Clear();
            playerTurnsHistory.Clear();
            turnNumber = 0;
        }

        private void OnWin()
        {
            //If AI bot wins, we train his perceptron by all his own turns

            turnsHistory.ForEach(turn =>
            {
                brain.SaveSolution(TaskType, turn.fieldState, turn.cellIdx / 8f);
            });

            // And forget last player turn as failed one

            TurnData lastPlayerTurn = playerTurnsHistory[playerTurnsHistory.Count - 1];
            brain.ForgetSolution(TaskType, lastPlayerTurn.fieldState, lastPlayerTurn.cellIdx / 8f);

            PrintPerceptronState();

            ResetState();
        }

        private void OnLoss()
        {
            //If player wins, we train his perceptron by all player's turns

            playerTurnsHistory.ForEach(turn =>
            {
                brain.SaveSolution(TaskType, turn.fieldState, turn.cellIdx / 8f, false);
            });

            // And forget our last turn as failed one

            TurnData lastTurn = turnsHistory[turnsHistory.Count - 1];
            brain.ForgetSolution(TaskType, lastTurn.fieldState, lastTurn.cellIdx / 8f);

            PrintPerceptronState();

            ResetState();
        }

        private void OnTie()
        {
            ResetState();
        }

        private void OnPlayerTurnEnd(int cellIdx)
        {
            List<float> fieldState = ticTacToeController.GetFieldStateAsFloats();

            // Getting cell index to do AI turn on board
            if (brain.FindSolution(TaskType, fieldState, out var solution))
            {
                nextCellIdx = Mathf.FloorToInt(Mathf.Clamp01(solution * 8));
            }

            // If chosen cell isn't empty, we select random one
            if (fieldState[nextCellIdx] != 0f)
            {
                List<int> indices = new List<int>();
                for (int i = 0; i < fieldState.Count; i++)
                {
                    if (fieldState[i] == 0f)
                    {
                        indices.Add(i);
                    }
                }
                nextCellIdx = indices.GetRandomElement();
            }

            // We survived 2 turns, so our last turn was effective, learn it
            if (turnNumber > 1)
            {
                TurnData lastTurn = turnsHistory[turnsHistory.Count - 1];
                brain.SaveSolution(TaskType, lastTurn.fieldState, lastTurn.cellIdx / 8f, false);
                PrintPerceptronState();
            }

            // Add this turn to our turns history
            turnsHistory.Add(new TurnData
            {
                fieldState = new List<float>(fieldState),
                cellIdx = nextCellIdx,
            });

            // Invert field state to "look" at field as player
            for (int i = 0; i < fieldState.Count; i++)
            {
                fieldState[i] *= -1;
            }

            // Add this turn to our turns history
            playerTurnsHistory.Add(new TurnData
            {
                fieldState = new List<float>(fieldState),
                cellIdx = cellIdx,
            });

            // Wait 1 sec. and perform AI turn on field
            Invoke(nameof(DoTurn), 1f);
        }

        private void DoTurn()
        {
            ticTacToeController.SetField(nextCellIdx, TicTacToeController.TicTacToeSymbol.X);
            turnNumber++;
        }

        [Serializable]
        private struct TurnData
        {
            public List<float> fieldState;
            public int cellIdx;
        }
    }
}