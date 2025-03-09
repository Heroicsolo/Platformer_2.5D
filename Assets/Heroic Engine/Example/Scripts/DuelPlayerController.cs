using HeroicEngine.Gameplay;
using HeroicEngine.Systems;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.UI;
using HeroicEngine.UI;
using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using UnityEngine;
using HeroicEngine.Enums;

namespace HeroicEngine.Examples
{
    public class DuelPlayerController : DuelCharacterBase, IInjectable
    {
        [SerializeField] private SkillButton skillButtonPrefab;
        [SerializeField] private Transform skillButtonsHolder;

        [Inject] private IEventsManager eventsManager;
        [Inject] private IUIController uiController;
        [Inject] private ILocalizationManager localizationManager;
        [Inject] private IQuestManager questManager;
        [Inject] private IPlayerProgressionManager playerProgressionManager;
        [Inject] private ICountdownController countdownController;

        [Inject] private DuelBotController botCharacter;
        private Dictionary<DuelSkillInfo, SkillButton> skillButtons = new Dictionary<DuelSkillInfo, SkillButton>();

        public void PostInject()
        {
            eventsManager.RegisterListener("DuelBotTurnEnd", StartTurn);
            eventsManager.RegisterListener("DuelBotDied", Win);

            skillButtonsHolder.gameObject.SetActive(false);

            uiController.ShowMessageBox(localizationManager.GetLocalizedString("DuelGameTitle"),
                localizationManager.GetLocalizedString("DuelGameDescription"), "OK", ShowIntroDialog, true);
        }

        protected override void Start()
        {
            InjectionManager.RegisterObject(this);

            base.Start();
        }

        private void ShowIntroDialog()
        {
            Transform enemyTransform = botCharacter.DialogTargetTransform;

            uiController.ShowDialog(DialogPopupMode.Fullscreen, localizationManager.GetLocalizedString("DuelIntroDialogMsg"), enemyTransform, 1f, 1f,
                new DialogOption { text = localizationManager.GetLocalizedString("DuelIntroDialogOption1"), callback = StartGame });
        }

        private void StartGame()
        {
            countdownController.StartCountdown(3, null, ShowSkills);
        }

        private void ShowSkills()
        {
            skillButtonsHolder.gameObject.SetActive(true);
        }

        public bool IsSkillOnCd(int idx)
        {
            return skillsCds[skills[idx]] > 0;
        }

        protected override void InitSkills()
        {
            base.InitSkills();

            skills.ForEach(skill =>
            {
                SkillButton button = Instantiate(skillButtonPrefab, skillButtonsHolder);
                button.Setup(skill.Icon, skill.UsageCost.ToString(), () =>
                {
                    if (currEnergy >= skill.UsageCost && !isStunned)
                    {
                        skill.Perform(this, botCharacter);
                        skillsCds[skill] = skill.Cooldown;
                        eventsManager.TriggerEvent("DuelPlayerSkillUsed", skills.FindIndex(s => s == skill));
                        button.SetCooldown(1f);
                    }
                });
                skillButtons.Add(skill, button);
            });
        }

        protected override void OnSkillAnimEnd()
        {
            base.OnSkillAnimEnd();
            EndTurn();
        }

        protected override void Die()
        {
            base.Die();
            eventsManager.TriggerEvent("DuelPlayerDied");
            Invoke(nameof(Loss), 3f);
        }

        private void Win()
        {
            ResetState();
            uiController.ShowUIParts(UIPartType.VictoryScreen);
            questManager.AddProgress(QuestTaskType.GameWon, 1);
            playerProgressionManager.AddExperience(25);
        }

        private void Loss()
        {
            ResetState();
            uiController.ShowUIParts(UIPartType.FailScreen);
        }

        private void StartTurn()
        {
            foreach (var skillBtn in skillButtons)
            {
                if (skillsCds[skillBtn.Key] > 0)
                {
                    skillsCds[skillBtn.Key]--;
                    skillBtn.Value.SetCooldown((float)skillsCds[skillBtn.Key] / skillBtn.Key.Cooldown);
                }
            }

            if (isStunned)
            {
                EndTurn();
                EndStun();
            }
        }

        private void ResetState()
        {
            ResetHealth();

            currEnergy = energy;

            RefreshHPBar();
            RefreshEPBar();

            foreach (var skillBtn in skillButtons)
            {
                if (skillsCds[skillBtn.Key] > 0)
                {
                    skillsCds[skillBtn.Key] = 0;
                    skillBtn.Value.SetCooldown(0f);
                }
            }

            if (ragdoll != null)
            {
                ragdoll.SetRagdollMode(false);
            }
        }

        private void EndTurn()
        {
            eventsManager.TriggerEvent("DuelPlayerTurnEnd");
        }

        private void OnDestroy()
        {
            countdownController.CancelCountdown();
        }
    }
}