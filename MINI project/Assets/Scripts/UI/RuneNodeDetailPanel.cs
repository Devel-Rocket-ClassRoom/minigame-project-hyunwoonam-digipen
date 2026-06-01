using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tempt
{
    public sealed class RuneNodeDetailPanel : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField]
        private GameObject root;

        [Header("Labels")]
        [SerializeField]
        private TMP_Text nameLabel;

        [SerializeField]
        private TMP_Text typeLabel;

        [SerializeField]
        private TMP_Text descriptionLabel;

        [SerializeField]
        private TMP_Text runePointsLabel;

        [Header("Unlock")]
        [SerializeField]
        private Button unlockButton;

        [SerializeField]
        private TMP_Text unlockButtonLabel;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }
        }

        public void Show(
            RuneData data,
            PlayerRuneState state,
            bool unlockable,
            bool unlocked,
            bool viewOnly,
            int investedPoints,
            int requiredPoints,
            UnityAction unlockAction
        )
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            SetText(nameLabel, data != null ? data.NameKey : string.Empty);
            SetText(typeLabel, data != null ? BuildTypeLabel(data) : string.Empty);
            SetText(descriptionLabel, data != null ? BuildDescription(data) : string.Empty);
            SetText(
                runePointsLabel,
                data != null
                    ? System.Math.Max(0, investedPoints)
                        + " / "
                        + System.Math.Max(0, requiredPoints)
                    : string.Empty
            );
            ConfigureUnlockButton(unlockable, unlocked, viewOnly, unlockAction);
        }

        public void Hide()
        {
            ConfigureUnlockButton(false, false, false, null);
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void ConfigureUnlockButton(
            bool unlockable,
            bool unlocked,
            bool viewOnly,
            UnityAction unlockAction
        )
        {
            if (unlockButton == null)
            {
                return;
            }

            unlockButton.onClick.RemoveAllListeners();
            unlockButton.interactable = unlockable && !unlocked && !viewOnly;
            if (unlockButton.interactable && unlockAction != null)
            {
                unlockButton.onClick.AddListener(unlockAction);
            }

            if (unlockButtonLabel != null)
            {
                if (unlocked)
                {
                    unlockButtonLabel.text = "MASTERED";
                }
                else if (viewOnly)
                {
                    unlockButtonLabel.text = "VIEW ONLY";
                }
                else if (unlockable)
                {
                    unlockButtonLabel.text = "INVEST POINT";
                }
                else
                {
                    unlockButtonLabel.text = "LOCKED";
                }
            }
        }

        private static string BuildTypeLabel(RuneData data)
        {
            switch (data.EffectType)
            {
                case RuneEffectType.AddATK:
                case RuneEffectType.AddSPD:
                    return "OFFENSIVE RUNE";
                case RuneEffectType.AddMaxHP:
                case RuneEffectType.AddDEF:
                    return "DEFENSIVE RUNE";
                case RuneEffectType.AddMaxMP:
                    return "ARCANE RUNE";
                case RuneEffectType.UnlockSkill:
                    return "SKILL RUNE";
                default:
                    return data.ClassId.ToString().ToUpperInvariant() + " RUNE";
            }
        }

        private static string BuildDescription(RuneData data)
        {
            if (!string.IsNullOrEmpty(data.DescKey))
            {
                return data.DescKey;
            }

            if (data.EffectType == RuneEffectType.UnlockSkill)
            {
                return "Unlock skill " + (int)data.EffectValue;
            }

            return data.EffectType + " +" + (int)data.EffectValue;
        }

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }
    }
}
