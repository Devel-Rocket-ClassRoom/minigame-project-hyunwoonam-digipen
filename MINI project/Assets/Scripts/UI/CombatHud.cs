using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// 전투 전용 HUD. PlayerUI에 직접 배치된 버튼/슬라이더만 사용한다.
    /// </summary>
    public sealed class CombatHud : UIPageControllerBase
    {
        /// <summary>참조 컨트롤러.</summary>
        public CombatController Controller;

        /// <summary>전투 종료 보상 화면. EndCombat에서 직접 호출.</summary>
        public CombatRewardPage RewardPage;

        [SerializeField]
        private GameObject actionPanel;

        [SerializeField]
        private Button attackButton;

        [SerializeField]
        private Button skill1Button;

        [SerializeField]
        private Button skill2Button;

        [SerializeField]
        private Button defendButton;

        [SerializeField]
        private Button[] consumableButtons = new Button[ConsumableSlots.SlotCount];

        [SerializeField]
        private TMP_Text[] consumableLabels = new TMP_Text[ConsumableSlots.SlotCount];

        [SerializeField]
        private Slider playerHpBar;

        [SerializeField]
        private Slider playerMpBar;

        [SerializeField]
        private TMP_Text playerNameLabel;

        [SerializeField]
        private TMP_Text playerLevelLabel;

        [SerializeField]
        private Slider playerExpBar;

        [SerializeField]
        private Text roundLabel;

        [SerializeField]
        private Text promptLabel;

        private Player player;
        private bool isOpen;

        public void Bind(CombatController controller, Player runtimePlayer)
        {
            Controller = controller;
            player = runtimePlayer;
            RefreshPlayerBars();
            RefreshPlayerIdentity();
            RefreshPlayerExp();
            RefreshConsumableSlots();
        }

        public bool HasRequiredReferences()
        {
            return ValidateRequiredReferences();
        }

        /// <inheritdoc/>
        public override void OnOpen()
        {
            if (!ValidateRequiredReferences())
            {
                enabled = false;
                return;
            }

            WireButtons();
            SubscribeEvents();
            RefreshPlayerBars();
            RefreshPlayerIdentity();
            RefreshPlayerExp();
            RefreshConsumableSlots();
            HidePlayerActionPanel();
            HideRoundLabel();
            isOpen = true;
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            RemoveButtonListeners();
            UnsubscribeEvents();
            HidePlayerActionPanel();
            isOpen = false;
        }

        private void Update()
        {
            if (!isOpen)
            {
                return;
            }

            RefreshPlayerBars();
            RefreshPlayerExp();
            HideRoundLabel();
        }

        /// <summary>플레이어 행동 패널 표시.</summary>
        public void ShowPlayerActionPanel(EntityBase actor)
        {
            if (!ValidateRequiredReferences())
            {
                return;
            }

            bool isPlayerTurn = actor is Player;
            actionPanel.SetActive(isPlayerTurn);
            if (!isPlayerTurn)
            {
                return;
            }

            Player runtimePlayer = actor as Player;
            attackButton.interactable = true;
            defendButton.interactable = true;
            skill1Button.interactable = CanUseSkill(runtimePlayer, 0);
            skill2Button.interactable = CanUseSkill(runtimePlayer, 1);
            RefreshConsumableSlots();
            SetConsumableButtonsInteractable(true);
            SetPrompt(Loc.Get("combat_select_action"));
        }

        /// <summary>플레이어 행동 패널 숨김.</summary>
        public void HidePlayerActionPanel()
        {
            if (actionPanel != null)
            {
                actionPanel.SetActive(false);
            }

            SetConsumableButtonsInteractable(false);
        }

        public void ShowTargetPrompt(SkillTargetType targetType)
        {
            SetPrompt(
                Loc.Get(
                    targetType == SkillTargetType.AllySingle
                        ? "combat_select_ally"
                        : "combat_select_enemy"
                )
            );
        }

        public void ClearTargetPrompt()
        {
            SetPrompt(string.Empty);
        }

        private bool ValidateRequiredReferences()
        {
            bool valid =
                actionPanel != null
                && attackButton != null
                && skill1Button != null
                && skill2Button != null
                && defendButton != null
                && consumableButtons != null
                && consumableButtons.Length == ConsumableSlots.SlotCount
                && consumableLabels != null
                && consumableLabels.Length == ConsumableSlots.SlotCount
                && playerHpBar != null
                && playerMpBar != null
                && playerNameLabel != null
                && playerLevelLabel != null
                && playerExpBar != null;

            if (!valid)
            {
                GameLog.LogError(
                    "[CombatHud] PlayerUI의 actionPanel/buttons/ItemUI/StatsPanel 직접 연결이 누락되었습니다."
                );
                return false;
            }

            for (int i = 0; i < consumableButtons.Length; i++)
            {
                if (consumableButtons[i] == null || consumableLabels[i] == null)
                {
                    GameLog.LogError("[CombatHud] ItemUI 슬롯 버튼/라벨 참조가 누락되었습니다: " + i);
                    return false;
                }
            }

            return valid;
        }

        private void WireButtons()
        {
            RemoveButtonListeners();
            attackButton.onClick.AddListener(() => Controller?.PlayerPickAttack());
            skill1Button.onClick.AddListener(() => Controller?.PlayerPickSkill(0));
            skill2Button.onClick.AddListener(() => Controller?.PlayerPickSkill(1));
            defendButton.onClick.AddListener(() => Controller?.PlayerPickDefend());

            if (consumableButtons == null)
            {
                return;
            }

            for (int i = 0; i < consumableButtons.Length; i++)
            {
                int slot = i;
                consumableButtons[i].onClick.AddListener(() => Controller?.PlayerUseItem(slot));
            }
        }

        private void RemoveButtonListeners()
        {
            attackButton?.onClick.RemoveAllListeners();
            skill1Button?.onClick.RemoveAllListeners();
            skill2Button?.onClick.RemoveAllListeners();
            defendButton?.onClick.RemoveAllListeners();
            if (consumableButtons == null)
            {
                return;
            }

            for (int i = 0; i < consumableButtons.Length; i++)
            {
                consumableButtons[i]?.onClick.RemoveAllListeners();
            }
        }

        private void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= RefreshConsumableSlots;
                gsm.Events.OnPlayerExpChanged -= RefreshPlayerExp;
                gsm.Events.OnPlayerLevelUp -= RefreshPlayerLevel;
                gsm.Events.OnInventoryChanged += RefreshConsumableSlots;
                gsm.Events.OnPlayerExpChanged += RefreshPlayerExp;
                gsm.Events.OnPlayerLevelUp += RefreshPlayerLevel;
            }
        }

        private void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= RefreshConsumableSlots;
                gsm.Events.OnPlayerExpChanged -= RefreshPlayerExp;
                gsm.Events.OnPlayerLevelUp -= RefreshPlayerLevel;
            }
        }

        private void RefreshPlayerBars()
        {
            StatBlock stats = player?.Stats;
            if (stats == null)
            {
                return;
            }

            if (playerHpBar != null)
            {
                playerHpBar.value =
                    stats.MaxHP > 0 ? Mathf.Clamp01((float)stats.CurrentHP / stats.MaxHP) : 0f;
            }

            if (playerMpBar != null)
            {
                playerMpBar.value =
                    stats.MaxMP > 0 ? Mathf.Clamp01((float)stats.CurrentMP / stats.MaxMP) : 0f;
            }
        }

        private void RefreshPlayerIdentity()
        {
            PlayerState state = TryGetPlayerState();
            string displayName =
                state != null && !string.IsNullOrEmpty(state.Name) ? state.Name
                : player != null ? player.DisplayName
                : string.Empty;
            int level =
                state != null ? state.Level
                : player != null ? player.Level
                : 1;

            playerNameLabel.text = displayName;
            playerLevelLabel.text = Loc.Format("combat_level_fmt", Mathf.Max(1, level));
        }

        private void RefreshPlayerExp()
        {
            PlayerState state = TryGetPlayerState();
            int level =
                state != null ? state.Level
                : player != null ? player.Level
                : 1;
            int current =
                state != null ? state.Exp
                : player != null ? player.CurrentExp
                : 0;
            int required = GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                ? RunProgression.RequiredExpForLevel(gsm.Data, level)
                : 0;
            RefreshPlayerExp(current, required);
        }

        private void RefreshPlayerExp(int current, int required)
        {
            playerExpBar.value = required > 0 ? Mathf.Clamp01((float)current / required) : 0f;
        }

        private void RefreshPlayerLevel(int level)
        {
            RefreshPlayerIdentity();
            RefreshPlayerExp();
        }

        private static bool CanUseSkill(Player runtimePlayer, int slotIndex)
        {
            Skill skill = runtimePlayer != null ? runtimePlayer.GetActiveSkill(slotIndex) : null;
            return skill != null && skill.CanUse(runtimePlayer);
        }

        public void RefreshConsumableSlots()
        {
            for (int i = 0; i < consumableLabels.Length; i++)
            {
                consumableLabels[i].text = FormatConsumableSlot(i);
            }
        }

        public string GetConsumableSlotLabelText(int slotIndex)
        {
            if (
                consumableLabels == null
                || slotIndex < 0
                || slotIndex >= consumableLabels.Length
                || consumableLabels[slotIndex] == null
            )
            {
                return string.Empty;
            }

            return consumableLabels[slotIndex].text;
        }

        private string FormatConsumableSlot(int slotIndex)
        {
            if (
                player?.Consumables?.SlotItemIds == null
                || slotIndex < 0
                || slotIndex >= player.Consumables.SlotItemIds.Length
            )
            {
                return Loc.Get("ui_empty");
            }

            int itemId = player.Consumables.SlotItemIds[slotIndex];
            if (itemId == 0)
            {
                return Loc.Get("ui_empty");
            }

            int count = player.Inventory != null ? player.Inventory.CountOf(itemId) : 0;
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.Data?.Items == null
                || !gsm.Data.Items.TryGetValue(itemId, out ItemData itemData)
            )
            {
                return Loc.Format("ui_missing_id_fmt", itemId);
            }

            return Loc.Get(itemData.NameKey) + "\n" + Loc.Format("ui_qty_fmt", count);
        }

        private void SetConsumableButtonsInteractable(bool playerTurn)
        {
            if (consumableButtons == null)
            {
                return;
            }

            for (int i = 0; i < consumableButtons.Length; i++)
            {
                Button button = consumableButtons[i];
                if (button == null)
                {
                    continue;
                }

                button.interactable = playerTurn && HasUsableConsumable(i);
            }
        }

        private bool HasUsableConsumable(int slotIndex)
        {
            if (
                player?.Consumables?.SlotItemIds == null
                || slotIndex < 0
                || slotIndex >= player.Consumables.SlotItemIds.Length
            )
            {
                return false;
            }

            int itemId = player.Consumables.SlotItemIds[slotIndex];
            return itemId != 0 && player.Inventory != null && player.Inventory.CountOf(itemId) > 0;
        }

        private static PlayerState TryGetPlayerState()
        {
            return GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                ? gsm.CurrentRun?.Player
                : null;
        }

        private void SetPrompt(string text)
        {
            if (promptLabel == null)
            {
                return;
            }

            promptLabel.text = text ?? string.Empty;
            promptLabel.gameObject.SetActive(!string.IsNullOrEmpty(promptLabel.text));
        }

        private void HideRoundLabel()
        {
            if (roundLabel == null)
            {
                return;
            }

            roundLabel.text = string.Empty;
            roundLabel.gameObject.SetActive(false);
        }
    }
}
