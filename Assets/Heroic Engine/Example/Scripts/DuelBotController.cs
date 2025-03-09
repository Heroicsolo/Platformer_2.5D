using HeroicEngine.AI;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeroicEngine.Examples
{
    [RequireComponent(typeof(AIBrain))]
    public class DuelBotController : DuelCharacterBase, IInjectable
    {
        private const string TaskType = "Duel_0";

        [Inject] private IEventsManager eventsManager;

        [Inject] private DuelPlayerController playerCharacter;

        private AIBrain brain;
        private List<float> duelParameters = new List<float>();
        private List<float> duelParametersForPlayer = new List<float>();
        private List<DuelTurnData> myTurns = new List<DuelTurnData>();
        private List<DuelTurnData> playerTurns = new List<DuelTurnData>();

        public void PostInject()
        {
            eventsManager.RegisterListener<int>("DuelPlayerSkillUsed", PlayerSkillUsed);
            eventsManager.RegisterListener("DuelPlayerTurnEnd", StartTurn);
            eventsManager.RegisterListener("DuelPlayerDied", OnPlayerDeath);
        }

        protected override void Start()
        {
            InjectionManager.RegisterObject(this);

            base.Start();

            brain = GetComponent<AIBrain>();

            duelParameters = new List<float>(new float[4 + Enum.GetValues(typeof(DuelParameter)).Length]);
            duelParametersForPlayer = new List<float>(new float[4 + Enum.GetValues(typeof(DuelParameter)).Length]);
        }

        private void OnPlayerDeath()
        {
            // AI wins, teach its perceptron by his moves

            foreach (var turn in myTurns)
            {
                brain.SaveSolution(TaskType, turn.TurnParameters, (float)turn.SkillNumber / skills.Count);
            }

            ResetState();
        }

        protected override void OnSkillAnimEnd()
        {
            base.OnSkillAnimEnd();
            EndTurn();
        }

        protected override void Die()
        {
            base.Die();

            // Player wins, teach its perceptron by his moves

            foreach (var turn in playerTurns)
            {
                brain.SaveSolution(TaskType, turn.TurnParameters, (float)turn.SkillNumber / skills.Count, false);
            }

            eventsManager.TriggerEvent("DuelBotDied");

            Invoke(nameof(ResetState), 3f);
        }

        private void ResetState()
        {
            ResetHealth();

            currEnergy = energy;

            RefreshHPBar();
            RefreshEPBar();

            foreach (var skill in skillsCds.Keys.ToArray())
            {
                if (skillsCds[skill] > 0)
                {
                    skillsCds[skill] = 0;
                }
            }

            if (ragdoll != null)
            {
                ragdoll.SetRagdollMode(false);
            }
        }

        private void EndTurn()
        {
            eventsManager.TriggerEvent("DuelBotTurnEnd");
        }

        private void PlayerSkillUsed(int skillNumber)
        {
            playerTurns.Add(new DuelTurnData
            {
                TurnParameters = new List<float>(duelParametersForPlayer),
                SkillNumber = skillNumber
            });
        }

        private void StartTurn()
        {
            foreach (var skill in skillsCds.Keys.ToArray())
            {
                if (skillsCds[skill] > 0)
                {
                    skillsCds[skill]--;
                }
            }

            duelParametersForPlayer[(int)DuelParameter.EnemyHealth] = GetHPPercentage();
            duelParametersForPlayer[(int)DuelParameter.EnemyEnergy] = GetEPPercentage();
            duelParametersForPlayer[(int)DuelParameter.MyHealth] = playerCharacter.GetHPPercentage();
            duelParametersForPlayer[(int)DuelParameter.MyEnergy] = playerCharacter.GetEPPercentage();
            duelParametersForPlayer[(int)DuelParameter.IsEnemyStunned] = IsStunned() ? 1 : 0;

            for (int i = 0; i < skills.Count; i++)
            {
                duelParametersForPlayer[(int)DuelParameter.IsEnemyStunned + i + 1] = playerCharacter.IsSkillOnCd(i) ? 1 : 0;
            }

            if (IsStunned())
            {
                OnSkillAnimEnd();
                EndStun();
                return;
            }

            duelParameters[(int)DuelParameter.EnemyHealth] = playerCharacter.GetHPPercentage();
            duelParameters[(int)DuelParameter.EnemyEnergy] = playerCharacter.GetEPPercentage();
            duelParameters[(int)DuelParameter.MyHealth] = GetHPPercentage();
            duelParameters[(int)DuelParameter.MyEnergy] = GetEPPercentage();
            duelParameters[(int)DuelParameter.IsEnemyStunned] = playerCharacter.IsStunned() ? 1 : 0;

            for (int i = 0; i < skills.Count; i++)
            {
                duelParameters[(int)DuelParameter.IsEnemyStunned + i + 1] = skillsCds[skills[i]] > 0 ? 1 : 0;
            }

            if (brain.FindSolution(TaskType, duelParameters, out var skillNumber))
            {
                int idx = Mathf.RoundToInt(skillNumber * 3f);

                while (skillsCds[skills[idx]] > 0)
                {
                    idx++;
                    if (idx == skills.Count)
                    {
                        idx = 0;
                    }
                }

                skills[idx].Perform(this, playerCharacter);

                skillsCds[skills[idx]] = skills[idx].Cooldown;

                myTurns.Add(new DuelTurnData
                {
                    TurnParameters = new List<float>(duelParameters),
                    SkillNumber = idx
                });
            }
        }

        private enum DuelParameter
        {
            EnemyHealth,
            EnemyEnergy,
            MyHealth,
            MyEnergy,
            IsEnemyStunned,
        }

        private struct DuelTurnData
        {
            public List<float> TurnParameters;
            public int SkillNumber;
        }
    }
}