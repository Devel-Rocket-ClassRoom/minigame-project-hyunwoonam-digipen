using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    [DisallowMultipleComponent]
    public sealed class ShortcutInfoPanelController : MonoBehaviour
    {
        public enum PageKind
        {
            Skills,
            Status,
        }

        private enum SkillListMode
        {
            Active,
            Passive,
        }

        [SerializeField]
        private PageKind pageKind;

        private readonly List<SkillRowBinding> skillRows = new List<SkillRowBinding>();
        private readonly List<SkillEntry> currentSkillEntries = new List<SkillEntry>();
        private static readonly Color32 SelectedSwitchBoxColor = new Color32(128, 26, 26, 255);
        private static readonly Color32 SelectedSwitchTextColor = new Color32(247, 112, 112, 255);
        private Button closeButton;
        private Button activeSwitchButton;
        private Button passiveSwitchButton;
        private SwitchVisualState activeSwitchVisual;
        private SwitchVisualState passiveSwitchVisual;
        private Slider expBar;
        private TMP_Text levelValueText;
        private TMP_Text expValueText;
        private TMP_Text goldValueText;
        private TMP_Text floorValueText;
        private TMP_Text detailNameText;
        private TMP_Text detailDescText;
        private TMP_Text detailNoteText;
        private TMP_Text detailEffectText;
        private Transform statsRow;
        private Transform skillListContent;
        private Action closeAction;
        private EventBus subscribedEvents;
        private SkillListMode skillListMode = SkillListMode.Active;
        private int selectedSkillId;

        public PageKind Kind
        {
            get => pageKind;
            set => pageKind = value;
        }

        private void OnEnable()
        {
            SubscribeEvents();
            Refresh();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void Show(Action onClose)
        {
            closeAction = onClose;
            CacheReferences();
            WireButtons();
            Refresh();
        }

        public void Refresh()
        {
            CacheReferences();
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun?.Player == null
            )
            {
                ClearPanel();
                return;
            }

            if (pageKind == PageKind.Status)
            {
                RefreshStatus(gsm.CurrentRun, gsm.Data);
            }
            else
            {
                RefreshSkills(gsm.CurrentRun, gsm.Data);
            }
        }

        private void CacheReferences()
        {
            closeButton = closeButton != null ? closeButton : FindButton("Header/CloseBtn");

            if (pageKind == PageKind.Status)
            {
                levelValueText = FindText("Content/LeftCol/Row1/R");
                expValueText = FindText("Content/LeftCol/Row2/R");
                expBar = FindComponent<Slider>("Content/LeftCol/ExpBar");
                statsRow = transform.Find("Content/RightCol/StatsRow");
                goldValueText = FindText("Content/RightCol/LowerGrid/RunResource/Row1/R");
                floorValueText = FindText("Content/RightCol/LowerGrid/RunResource/Row2/R");
                return;
            }

            activeSwitchButton = FindButton("Content/LeftCol/Switches/ACTIVE");
            passiveSwitchButton = FindButton("Content/LeftCol/Switches/PASSIVE");
            CacheSwitchVisuals();
            skillListContent = transform.Find("Content/LeftCol/Scroll/Viewport/ListContent");
            detailNameText = FindText("Content/RightCol/Padding/Summary/Name");
            detailDescText = FindText("Content/RightCol/Padding/Desc");
            detailNoteText = FindText("Content/RightCol/Padding/Note/Text");
            detailEffectText = FindText("Content/RightCol/Padding/EffectArea/EFFECTCard/V");
            CacheSkillRows();
        }

        private void WireButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
                closeButton.onClick.AddListener(HandleCloseClicked);
            }

            if (pageKind != PageKind.Skills)
            {
                return;
            }

            if (activeSwitchButton != null)
            {
                activeSwitchButton.onClick.RemoveListener(ShowActiveSkills);
                activeSwitchButton.onClick.AddListener(ShowActiveSkills);
            }

            if (passiveSwitchButton != null)
            {
                passiveSwitchButton.onClick.RemoveListener(ShowPassiveSkills);
                passiveSwitchButton.onClick.AddListener(ShowPassiveSkills);
            }
        }

        private void CacheSkillRows()
        {
            skillRows.Clear();
            if (skillListContent == null)
            {
                return;
            }

            for (int i = 0; i < skillListContent.childCount; i++)
            {
                Transform row = skillListContent.GetChild(i);
                TMP_Text nameText = FindText(row, "N");
                if (nameText == null)
                {
                    continue;
                }

                skillRows.Add(
                    new SkillRowBinding
                    {
                        Root = row.gameObject,
                        Button = row.GetComponent<Button>(),
                        Name = nameText,
                    }
                );
            }
        }

        private void RefreshStatus(GameRunState run, DataManager data)
        {
            PlayerState player = run.Player;
            StatBlock stats = player.Stats;
            int requiredExp = RunProgression.RequiredExpForLevel(data, player.Level);
            SetText(levelValueText, Math.Max(1, player.Level).ToString());
            SetText(expValueText, player.Exp + " / " + requiredExp);

            if (expBar != null)
            {
                expBar.minValue = 0f;
                expBar.maxValue = 1f;
                expBar.value = requiredExp > 0 ? Mathf.Clamp01((float)player.Exp / requiredExp) : 0f;
            }

            if (statsRow != null && stats != null)
            {
                RefreshStatCards(stats);
            }

            SetText(goldValueText, run.Gold + " G");
            SetText(floorValueText, FormatFloor(run));
        }

        private void RefreshStatCards(StatBlock stats)
        {
            for (int i = 0; i < statsRow.childCount; i++)
            {
                Transform card = statsRow.GetChild(i);
                TMP_Text label = FindText(card, "L");
                TMP_Text value = FindText(card, "V");
                if (label == null || value == null)
                {
                    continue;
                }

                switch ((label.text ?? string.Empty).Trim().ToUpperInvariant())
                {
                    case "HP":
                        value.text = stats.CurrentHP + " / " + stats.MaxHP;
                        break;
                    case "MP":
                        value.text = stats.CurrentMP + " / " + stats.MaxMP;
                        break;
                    case "ATK":
                        value.text = stats.ATK.ToString();
                        break;
                    case "DEF":
                        value.text = stats.DEF.ToString();
                        break;
                    case "SPD":
                        value.text = stats.SPD.ToString();
                        break;
                }
            }
        }

        private void RefreshSkills(GameRunState run, DataManager data)
        {
            BuildSkillEntries(run.Player, data, currentSkillEntries);
            if (currentSkillEntries.Count > 0 && !ContainsSkill(currentSkillEntries, selectedSkillId))
            {
                selectedSkillId = currentSkillEntries[0].SkillId;
            }

            for (int i = 0; i < skillRows.Count; i++)
            {
                SkillRowBinding row = skillRows[i];
                bool hasEntry = i < currentSkillEntries.Count;
                row.Root.SetActive(hasEntry);
                if (!hasEntry)
                {
                    continue;
                }

                SkillEntry entry = currentSkillEntries[i];
                row.SkillId = entry.SkillId;
                row.Name.text = entry.Name;
                if (row.Button != null)
                {
                    row.Button.onClick.RemoveAllListeners();
                    row.Button.onClick.AddListener(() => SelectSkill(row.SkillId));
                }
            }

            RefreshSelectedSkill(data);
            RefreshSwitchVisuals();
        }

        private void BuildSkillEntries(PlayerState player, DataManager data, List<SkillEntry> entries)
        {
            entries.Clear();
            if (player == null)
            {
                return;
            }

            if (skillListMode == SkillListMode.Active)
            {
                int[] slots = player.ActiveSlotSkillIds;
                if (slots == null)
                {
                    return;
                }

                for (int i = 0; i < slots.Length; i++)
                {
                    int skillId = slots[i];
                    if (skillId <= 0 || !TryGetSkill(skillId, data, out SkillData skill))
                    {
                        continue;
                    }

                    entries.Add(ToEntry(skill));
                }

                return;
            }

            AppendPassiveRuneSkills(player, data, entries);
        }

        private static void AppendPassiveRuneSkills(
            PlayerState player,
            DataManager data,
            List<SkillEntry> entries
        )
        {
            if (player?.Rune == null || data?.Runes == null)
            {
                return;
            }

            List<int> mastered = player.Rune.GetMasteredNodeIds();
            for (int i = 0; i < mastered.Count; i++)
            {
                if (
                    !data.Runes.TryGetValue(mastered[i], out RuneData rune)
                    || rune.EffectType != RuneEffectType.UnlockSkill
                )
                {
                    continue;
                }

                int skillId = (int)rune.EffectValue;
                if (!TryGetSkill(skillId, data, out SkillData skill) || skill.SkillType != SkillType.Passive)
                {
                    continue;
                }

                entries.Add(ToEntry(skill));
            }
        }

        private void RefreshSelectedSkill(DataManager data)
        {
            if (!TryGetSkill(selectedSkillId, data, out SkillData skill))
            {
                SetText(detailNameText, string.Empty);
                SetText(detailDescText, string.Empty);
                SetText(detailNoteText, string.Empty);
                SetText(detailEffectText, string.Empty);
                return;
            }

            SetText(detailNameText, SafeText(skill.NameKey, "Skill " + skill.Id));
            SetText(detailDescText, BuildSkillDescription(skill));
            SetText(detailNoteText, BuildSkillNote(skill));
            SetText(detailEffectText, BuildSkillEffectText(skill));
        }

        private void SelectSkill(int skillId)
        {
            selectedSkillId = skillId;
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                RefreshSelectedSkill(gsm.Data);
            }
        }

        private void ShowActiveSkills()
        {
            skillListMode = SkillListMode.Active;
            selectedSkillId = 0;
            Refresh();
        }

        private void ShowPassiveSkills()
        {
            skillListMode = SkillListMode.Passive;
            selectedSkillId = 0;
            Refresh();
        }

        private void CacheSwitchVisuals()
        {
            activeSwitchVisual = CacheSwitchVisual(activeSwitchButton, activeSwitchVisual);
            passiveSwitchVisual = CacheSwitchVisual(passiveSwitchButton, passiveSwitchVisual);
        }

        private static SwitchVisualState CacheSwitchVisual(
            Button button,
            SwitchVisualState previous
        )
        {
            if (button == null)
            {
                return previous;
            }

            Image box = button.GetComponent<Image>();
            TMP_Text label = FindText(button.transform, "T");
            if (
                previous.Initialized
                && previous.Box == box
                && previous.Label == label
            )
            {
                return previous;
            }

            return new SwitchVisualState
            {
                Box = box,
                Label = label,
                BoxColor = box != null ? box.color : Color.white,
                LabelColor = label != null ? label.color : Color.white,
                Initialized = true,
            };
        }

        private void RefreshSwitchVisuals()
        {
            ApplySwitchVisual(activeSwitchVisual, skillListMode == SkillListMode.Active);
            ApplySwitchVisual(passiveSwitchVisual, skillListMode == SkillListMode.Passive);
        }

        private static void ApplySwitchVisual(SwitchVisualState visual, bool selected)
        {
            if (!visual.Initialized)
            {
                return;
            }

            if (visual.Box != null)
            {
                visual.Box.color = selected ? SelectedSwitchBoxColor : visual.BoxColor;
            }

            if (visual.Label != null)
            {
                visual.Label.color = selected ? SelectedSwitchTextColor : visual.LabelColor;
            }
        }

        private void ClearPanel()
        {
            if (pageKind == PageKind.Status)
            {
                SetText(levelValueText, string.Empty);
                SetText(expValueText, string.Empty);
                SetText(goldValueText, string.Empty);
                SetText(floorValueText, string.Empty);
                if (expBar != null)
                {
                    expBar.value = 0f;
                }
            }
            else
            {
                currentSkillEntries.Clear();
                for (int i = 0; i < skillRows.Count; i++)
                {
                    skillRows[i].Root.SetActive(false);
                }

                SetText(detailNameText, string.Empty);
                SetText(detailDescText, string.Empty);
                SetText(detailNoteText, string.Empty);
                SetText(detailEffectText, string.Empty);
                RefreshSwitchVisuals();
            }
        }

        private void SubscribeEvents()
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return;
            }

            if (subscribedEvents == gsm.Events)
            {
                return;
            }

            UnsubscribeEvents();
            subscribedEvents = gsm.Events;
            if (subscribedEvents == null)
            {
                return;
            }

            subscribedEvents.OnGoldChanged += HandleRefreshInt;
            subscribedEvents.OnManaStoneChanged += HandleRefreshInt;
            subscribedEvents.OnDayChanged += HandleRefreshInt;
            subscribedEvents.OnPlayerLevelUp += HandleRefreshInt;
            subscribedEvents.OnPlayerExpChanged += HandleRefreshExp;
            subscribedEvents.OnEquipmentChanged += Refresh;
            subscribedEvents.OnSkillsChanged += Refresh;
            subscribedEvents.OnRuneNodeUnlocked += HandleRefreshRuneNode;
            subscribedEvents.OnRunePointsChanged += HandleRefreshInt;
            subscribedEvents.OnRuneClassChanged += HandleRefreshRuneClass;
            subscribedEvents.OnRuneReset += HandleRefreshRuneReset;
        }

        private void UnsubscribeEvents()
        {
            if (subscribedEvents == null)
            {
                return;
            }

            subscribedEvents.OnGoldChanged -= HandleRefreshInt;
            subscribedEvents.OnManaStoneChanged -= HandleRefreshInt;
            subscribedEvents.OnDayChanged -= HandleRefreshInt;
            subscribedEvents.OnPlayerLevelUp -= HandleRefreshInt;
            subscribedEvents.OnPlayerExpChanged -= HandleRefreshExp;
            subscribedEvents.OnEquipmentChanged -= Refresh;
            subscribedEvents.OnSkillsChanged -= Refresh;
            subscribedEvents.OnRuneNodeUnlocked -= HandleRefreshRuneNode;
            subscribedEvents.OnRunePointsChanged -= HandleRefreshInt;
            subscribedEvents.OnRuneClassChanged -= HandleRefreshRuneClass;
            subscribedEvents.OnRuneReset -= HandleRefreshRuneReset;
            subscribedEvents = null;
        }

        private void HandleCloseClicked()
        {
            closeAction?.Invoke();
        }

        private void HandleRefreshInt(int _)
        {
            Refresh();
        }

        private void HandleRefreshExp(int _, int __)
        {
            Refresh();
        }

        private void HandleRefreshRuneNode(int _, int __)
        {
            Refresh();
        }

        private void HandleRefreshRuneClass(RuneClass _)
        {
            Refresh();
        }

        private void HandleRefreshRuneReset(int _, int __)
        {
            Refresh();
        }

        private Button FindButton(string path)
        {
            return FindComponent<Button>(path);
        }

        private TMP_Text FindText(string path)
        {
            return FindText(transform, path);
        }

        private T FindComponent<T>(string path)
            where T : Component
        {
            Transform found = transform.Find(path);
            return found != null ? found.GetComponent<T>() : null;
        }

        private static TMP_Text FindText(Transform root, string path)
        {
            Transform found = root != null ? root.Find(path) : null;
            return found != null ? found.GetComponent<TMP_Text>() : null;
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static bool TryGetSkill(int skillId, DataManager data, out SkillData skill)
        {
            skill = null;
            return data?.Skills != null && data.Skills.TryGetValue(skillId, out skill);
        }

        private static bool ContainsSkill(List<SkillEntry> entries, int skillId)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].SkillId == skillId)
                {
                    return true;
                }
            }

            return false;
        }

        private static SkillEntry ToEntry(SkillData skill)
        {
            return new SkillEntry
            {
                SkillId = skill.Id,
                Name = SafeText(skill.NameKey, "Skill " + skill.Id),
            };
        }

        private static string BuildSkillDescription(SkillData skill)
        {
            if (!string.IsNullOrWhiteSpace(skill.DescKey))
            {
                return skill.DescKey;
            }

            if (skill.DamageScale > 0f)
            {
                return "Deal " + FormatPercent(skill.DamageScale) + " ATK damage to one selected enemy.";
            }

            if (skill.HealScale > 0f)
            {
                return "Restore " + FormatPercent(skill.HealScale) + " HP.";
            }

            if (skill.ShieldScale > 0f)
            {
                return "Gain shield based on " + FormatPercent(skill.ShieldScale) + " DEF.";
            }

            return skill.SkillType == SkillType.Passive ? "Passive effect." : "Utility skill.";
        }

        private static string BuildSkillNote(SkillData skill)
        {
            if (skill.SkillType == SkillType.Passive)
            {
                return "Passive effects are applied automatically.";
            }

            return string.Empty;
        }

        private static string BuildSkillEffectText(SkillData skill)
        {
            if (skill.DamageScale > 0f)
            {
                return "Damage " + FormatPercent(skill.DamageScale) + " ATK";
            }

            if (skill.HealScale > 0f)
            {
                return "Heal " + FormatPercent(skill.HealScale) + " HP";
            }

            if (skill.ShieldScale > 0f)
            {
                return "Shield " + FormatPercent(skill.ShieldScale) + " DEF";
            }

            return "Utility";
        }

        private static string FormatPercent(float scale)
        {
            return Mathf.RoundToInt(scale * 100f) + "%";
        }

        private static string FormatFloor(GameRunState run)
        {
            if (run == null)
            {
                return string.Empty;
            }

            return run.CurrentFloor > 0 ? run.CurrentFloor.ToString() : "Safe0";
        }

        private static string SafeText(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private sealed class SkillRowBinding
        {
            public GameObject Root;
            public Button Button;
            public TMP_Text Name;
            public int SkillId;
        }

        private struct SkillEntry
        {
            public int SkillId;
            public string Name;
        }

        private struct SwitchVisualState
        {
            public Image Box;
            public TMP_Text Label;
            public Color BoxColor;
            public Color LabelColor;
            public bool Initialized;
        }
    }
}
