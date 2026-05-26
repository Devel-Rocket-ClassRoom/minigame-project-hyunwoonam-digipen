namespace Tempt
{
    /// <summary>
    /// 전투 전용 HUD. 행동 패널(공격/스킬1/스킬2/방어 + 소모 4칸), 라운드 표시, 적 의도 표시.
    /// </summary>
    public sealed class CombatHudt : UIPageControllerBaset
    {
        /// <summary>참조 컨트롤러.</summary>
        public CombatControllert Controller;

        /// <summary>전투 종료 보상 화면. EndCombat에서 직접 호출.</summary>
        public CombatRewardPaget RewardPage;

        /// <inheritdoc/>
        public override void OnOpen()
        {
            // 동작 요약:
            // - 행동 버튼 OnClick 바인딩(Controller.PlayerPickAttack 등).
            // - 소모 4칸 버튼 바인딩(Controller.PlayerUseItem).
            // - 라운드 / 적 의도 / Player HP/MP 표시 갱신.
            //TODO: AttackButton.onClick.AddListener(() => Controller.PlayerPickAttack());
            //TODO: Skill1Button.onClick.AddListener(() => Controller.PlayerPickSkill(0));
            //TODO: Skill2Button.onClick.AddListener(() => Controller.PlayerPickSkill(1));
            //TODO: DefendButton.onClick.AddListener(() => Controller.PlayerPickDefend());
            //TODO: for (int i = 0; i < ConsumableButtons.Length; i++)
            //TODO: {
            //TODO:     int idx = i; // 람다 캡처
            //TODO:     ConsumableButtons[idx].onClick.AddListener(() => Controller.PlayerUseItem(idx));
            //TODO: }
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: PlayerHPBar.value  = (float)player.Stats.CurrentHP / player.Stats.MaxHP;
            //TODO: PlayerMPBar.value  = (float)player.Stats.CurrentMP / player.Stats.MaxMP;
            //TODO: RoundLabel.text    = "Round 1";
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 바인딩 해제.
            //TODO: AttackButton.onClick.RemoveAllListeners();
            //TODO: Skill1Button.onClick.RemoveAllListeners();
            //TODO: Skill2Button.onClick.RemoveAllListeners();
            //TODO: DefendButton.onClick.RemoveAllListeners();
            //TODO: foreach (var btn in ConsumableButtons) btn.onClick.RemoveAllListeners();
        }

        /// <summary>플레이어 행동 패널 표시.</summary>
        public void ShowPlayerActionPanel(EntityBaset actor)
        {
            // 동작 요약: actor가 Player면 활성.
            //TODO: bool isPlayer = actor is Playert;
            //TODO: ActionPanel.SetActive(isPlayer);
            //TODO: if (isPlayer)
            //TODO: {
            //TODO:     var player = (Playert)actor;
            //TODO:     Skill1Button.interactable = player.ActiveSkills.Count > 0 && player.ActiveSkills[0].CanUse(player.Stats);
            //TODO:     Skill2Button.interactable = player.ActiveSkills.Count > 1 && player.ActiveSkills[1].CanUse(player.Stats);
            //TODO: }
        }

        /// <summary>플레이어 행동 패널 숨김.</summary>
        public void HidePlayerActionPanel()
        {
            // 동작 요약: 비활성.
            //TODO: ActionPanel.SetActive(false);
        }
    }
}
