using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// 스킬 정보 패널 컨텍스트.
    /// </summary>
    public enum SkillDetailContext
    {
        /// <summary>길드 구매 탭 — 구매 버튼.</summary>
        GuildBuy,

        /// <summary>길드 장착 탭의 보유 스킬 클릭 — 슬롯 1/2 장착 버튼.</summary>
        GuildSlot,

        /// <summary>읽기 전용(스탯/룬 페이지, 전투 HUD 등) — 버튼 없음.</summary>
        Readonly,
    }

    // Guid3 §7 2026-05-27: ItemInfoPanel 과 별개의 스킬 정보 패널.
    // 컨텍스트(GuildBuy / GuildSlot / Readonly) 에 따라 버튼 라벨/핸들러가 달라진다.
    // fallback 금지: Inspector 필수 참조 누락은 Awake 에서 enabled = false.
    /// <summary>
    /// 선택된 스킬의 정보(이름/설명/타입/MP/효과/쿨다운/구매가) + 컨텍스트별 액션 버튼 표시.
    /// </summary>
    public sealed class SkillInfoPanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject root;

        [SerializeField]
        private TMP_Text nameLabel;

        [SerializeField]
        private TMP_Text descLabel;

        [SerializeField]
        private TMP_Text typeLabel;

        [SerializeField]
        private TMP_Text mpCostLabel;

        [SerializeField]
        private TMP_Text effectLabel;

        [SerializeField]
        private TMP_Text cooldownLabel;

        [SerializeField]
        private TMP_Text priceLabel;

        [SerializeField]
        private TMP_Text ownedLabel;

        [SerializeField]
        private Button primaryButton;

        [SerializeField]
        private Button secondaryButton;

        [SerializeField]
        private TMP_Text primaryButtonLabel;

        [SerializeField]
        private TMP_Text secondaryButtonLabel;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            Hide();
        }

        /// <summary>스킬 정보를 컨텍스트에 맞춰 표시한다.</summary>
        public void Show(int skillId, SkillDetailContext context)
        {
            if (
                !TryGetRunData(out GameRunState run, out DataManager data)
                || !data.Skills.TryGetValue(skillId, out SkillData skill)
            )
            {
                GameLog.LogError("[SkillInfoPanel.Show] 스킬 ID 없음: " + skillId);
                Hide();
                return;
            }

            root.SetActive(true);
            SkillData displaySkill = SkillRuntimeResolver.Resolve(
                skill,
                SkillRuntimeResolver.ResolveRuneClass(run.Player),
                data
            );
            nameLabel.text = Loc.Get(skill.NameKey);
            descLabel.text = BuildDescription(displaySkill);
            typeLabel.text = skill.SkillType.ToString();
            mpCostLabel.text = Loc.Format("skill_mp_fmt", displaySkill.MpCost);
            effectLabel.text = FormatEffect(displaySkill);
            cooldownLabel.text = Loc.Format("skill_cooldown_fmt", displaySkill.CooldownRounds);

            bool owned =
                run.Player?.OwnedSkillIds != null && run.Player.OwnedSkillIds.Contains(skillId);
            ownedLabel.text = owned ? "Owned" : "Not owned";
            ClearButton(primaryButton);
            ClearButton(secondaryButton);

            switch (context)
            {
                case SkillDetailContext.GuildBuy:
                    ConfigureGuildBuy(skill, run, data, owned);
                    break;
                case SkillDetailContext.GuildSlot:
                    ConfigureGuildSlot(skill, run, data, owned);
                    break;
                case SkillDetailContext.Readonly:
                    ConfigureReadonly();
                    break;
            }
        }

        /// <summary>패널을 숨긴다.</summary>
        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            ClearButton(primaryButton);
            ClearButton(secondaryButton);
        }

        private bool ValidateReferences()
        {
            bool valid =
                root != null
                && nameLabel != null
                && descLabel != null
                && typeLabel != null
                && mpCostLabel != null
                && effectLabel != null
                && cooldownLabel != null
                && priceLabel != null
                && ownedLabel != null
                && primaryButton != null
                && secondaryButton != null
                && primaryButtonLabel != null
                && secondaryButtonLabel != null;
            if (!valid)
            {
                GameLog.LogError(
                    "[SkillInfoPanel] 필수 UI 참조가 Inspector 에 직접 할당되어 있지 않습니다."
                );
            }

            return valid;
        }

        private static bool TryGetRunData(out GameRunState run, out DataManager data)
        {
            run = null;
            data = null;
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun?.Player == null
                || gsm.Data?.Skills == null
            )
            {
                GameLog.LogError(
                    "[SkillInfoPanel] GameSystemManager / CurrentRun.Player / Data.Skills 참조가 없습니다."
                );
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            return true;
        }

        private void ConfigureGuildBuy(
            SkillData skill,
            GameRunState run,
            DataManager data,
            bool owned
        )
        {
            int price = Guild.GetSkillBuyPrice(skill.Id, run, data);
            priceLabel.gameObject.SetActive(true);
            priceLabel.text = Loc.Format("skill_buy_fmt", price);
            bool canBuy = !owned && Guild.CanBuy(skill.Id, run, data);
            ConfigureButton(
                primaryButton,
                primaryButtonLabel,
                owned ? "Owned" : "BUY",
                () =>
                {
                    if (Guild.TryBuySkill(skill.Id, run, data))
                    {
                        Show(skill.Id, SkillDetailContext.GuildBuy);
                    }
                },
                true,
                canBuy
            );
            ConfigureButton(
                secondaryButton,
                secondaryButtonLabel,
                string.Empty,
                null,
                false,
                false
            );
        }

        private void ConfigureGuildSlot(
            SkillData skill,
            GameRunState run,
            DataManager data,
            bool owned
        )
        {
            priceLabel.gameObject.SetActive(false);
            if (!owned)
            {
                ConfigureButton(primaryButton, primaryButtonLabel, "Not owned", null, true, false);
                ConfigureButton(
                    secondaryButton,
                    secondaryButtonLabel,
                    string.Empty,
                    null,
                    false,
                    false
                );
                return;
            }

            if (skill.SkillType == SkillType.Passive)
            {
                ConfigureButton(primaryButton, primaryButtonLabel, "Passive", null, true, false);
                ConfigureButton(
                    secondaryButton,
                    secondaryButtonLabel,
                    string.Empty,
                    null,
                    false,
                    false
                );
                return;
            }

            ConfigureButton(
                primaryButton,
                primaryButtonLabel,
                "Slot 1",
                () =>
                {
                    if (SkillSwap.TrySetSlot(0, skill.Id, run, data))
                    {
                        Show(skill.Id, SkillDetailContext.GuildSlot);
                    }
                },
                true,
                true
            );
            ConfigureButton(
                secondaryButton,
                secondaryButtonLabel,
                "Slot 2",
                () =>
                {
                    if (SkillSwap.TrySetSlot(1, skill.Id, run, data))
                    {
                        Show(skill.Id, SkillDetailContext.GuildSlot);
                    }
                },
                true,
                true
            );
        }

        private void ConfigureReadonly()
        {
            priceLabel.gameObject.SetActive(false);
            ConfigureButton(primaryButton, primaryButtonLabel, string.Empty, null, false, false);
            ConfigureButton(
                secondaryButton,
                secondaryButtonLabel,
                string.Empty,
                null,
                false,
                false
            );
        }

        private static string FormatEffect(SkillData skill)
        {
            if (skill == null)
            {
                return string.Empty;
            }

            if (skill.DamageScale > 0f)
                return "Damage x" + skill.DamageScale;
            if (skill.HealScale > 0f)
                return "Heal x" + skill.HealScale;
            if (skill.ShieldScale > 0f)
                return "Shield x" + skill.ShieldScale;
            if (skill.SkillType == SkillType.Passive && skill.PassiveStatType != PassiveStatType.None)
                return skill.PassiveStatType
                    + " +"
                    + skill.PassiveFlatValue
                    + " / +"
                    + UnityEngine.Mathf.RoundToInt(skill.PassivePercentValue * 100f)
                    + "%";
            return string.Empty;
        }

        private static string BuildDescription(SkillData skill)
        {
            if (skill == null)
            {
                return string.Empty;
            }

            string text = string.IsNullOrEmpty(skill.DescKey) ? Loc.Get(skill.NameKey) : Loc.Get(skill.DescKey);
            if (!string.IsNullOrEmpty(skill.RuntimeDescAppendKey))
            {
                text += "\n" + Loc.Get(skill.RuntimeDescAppendKey);
            }

            return text;
        }

        private static void ConfigureButton(
            Button button,
            TMP_Text label,
            string text,
            UnityAction action,
            bool visible,
            bool interactable
        )
        {
            if (button == null)
            {
                return;
            }

            button.gameObject.SetActive(visible);
            button.interactable = visible && interactable && action != null;
            button.onClick.RemoveAllListeners();
            if (visible && interactable && action != null)
            {
                button.onClick.AddListener(action);
            }

            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }

        private static void ClearButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }
}
