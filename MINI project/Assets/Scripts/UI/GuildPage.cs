using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Guid3 §8 2026-05-27: 길드 화면 단일 진입점.
// 구매 탭 + 장착 탭 + SkillInfoPanel.
// 데이터 변경은 Guild.* / SkillSwap.* / EventBus 만 사용. UI 는 갱신만.
// fallback 금지: Inspector 미연결 시 Awake 에서 enabled = false.
/// <summary>
/// 길드 화면. AcquireType.Shop 인 스킬 구매 + 보유 스킬을 ActiveSkills 슬롯 2칸에 배치.
/// </summary>
public sealed class GuildPage : MonoBehaviour
{
    private enum GuildTab
    {
        Buy,
        Slot,
    }

    [Header("Root")]
    [SerializeField]
    private GameObject root;

    [SerializeField]
    private Button buyTabButton;

    [SerializeField]
    private Button slotTabButton;

    [SerializeField]
    private GameObject buyPanel;

    [SerializeField]
    private GameObject slotPanel;

    [Header("Buy List")]
    [SerializeField]
    private Transform buyListRoot;

    [SerializeField]
    private Button skillEntryPrefab;

    [Header("Slot Panel")]
    [SerializeField]
    private Transform ownedListRoot;

    [SerializeField]
    private Button slot1Button;

    [SerializeField]
    private Button slot2Button;

    [SerializeField]
    private TMP_Text slot1Label;

    [SerializeField]
    private TMP_Text slot2Label;

    [SerializeField]
    private Button slot1ClearButton;

    [SerializeField]
    private Button slot2ClearButton;

    [Header("Header")]
    [SerializeField]
    private TMP_Text goldLabel;

    [Header("Detail")]
    [SerializeField]
    private SkillInfoPanel infoPanel;

    /// <summary>현재 화면이 열려 있는가.</summary>
    public bool IsOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        WireStaticButtons();
        root.SetActive(false);
    }

    /// <summary>화면 열기 + 이벤트 구독 + Refresh.</summary>
    public void OnOpen()
    {
        if (!enabled)
        {
            return;
        }

        root.SetActive(true);
        SubscribeEvents();
        SetTab(GuildTab.Buy);
        Refresh();
    }

    /// <summary>이벤트 해제 + 화면 닫기.</summary>
    public void OnClose()
    {
        UnsubscribeEvents();
        infoPanel.Hide();
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    /// <summary>현재 PlayerState / DataManager 상태로 두 탭의 리스트와 슬롯 라벨을 새로 그린다.</summary>
    public void Refresh()
    {
        if (!TryGetRunData(out GameRunState run, out DataManager data))
        {
            return;
        }

        RebuildBuyList(run, data);
        RebuildSlotPanel(run, data);
        OnGoldChanged(run.Gold);
    }

    private bool ValidateReferences()
    {
        bool valid =
            root != null
            && buyTabButton != null
            && slotTabButton != null
            && buyPanel != null
            && slotPanel != null
            && buyListRoot != null
            && skillEntryPrefab != null
            && ownedListRoot != null
            && slot1Button != null
            && slot2Button != null
            && slot1Label != null
            && slot2Label != null
            && slot1ClearButton != null
            && slot2ClearButton != null
            && goldLabel != null
            && infoPanel != null;
        if (!valid)
        {
            GameLog.LogError(
                "[GuildPage] 필수 UI 참조가 Inspector 에 직접 할당되어 있지 않습니다."
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
                "[GuildPage] GameSystemManager / CurrentRun.Player / Data.Skills 참조가 없습니다."
            );
            return false;
        }

        run = gsm.CurrentRun;
        data = gsm.Data;
        return true;
    }

    private void SubscribeEvents()
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
        {
            gsm.Events.OnGoldChanged -= OnGoldChanged;
            gsm.Events.OnSkillsChanged -= Refresh;
            gsm.Events.OnGoldChanged += OnGoldChanged;
            gsm.Events.OnSkillsChanged += Refresh;
        }
    }

    private void UnsubscribeEvents()
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
        {
            gsm.Events.OnGoldChanged -= OnGoldChanged;
            gsm.Events.OnSkillsChanged -= Refresh;
        }
    }

    private void RebuildBuyList(GameRunState run, DataManager data)
    {
        ClearList(buyListRoot);
        var skills = new System.Collections.Generic.List<SkillData>(data.Skills.Values);
        skills.Sort((a, b) => a.Id.CompareTo(b.Id));
        foreach (SkillData skill in skills)
        {
            if (skill.AcquireType != AcquireType.Shop)
            {
                continue;
            }

            bool owned =
                run.Player.OwnedSkillIds != null && run.Player.OwnedSkillIds.Contains(skill.Id);
            int price = Guild.GetSkillBuyPrice(skill.Id, run, data);
            UIListEntryFactory.SpawnListEntry(
                skillEntryPrefab,
                buyListRoot,
                skill.NameKey + "  " + price + "G" + (owned ? "  Owned" : string.Empty),
                () => infoPanel.Show(skill.Id, SkillDetailContext.GuildBuy),
                "[GuildPage] skillEntryPrefab 하위 텍스트 참조가 없습니다."
            );
        }
    }

    private void RebuildSlotPanel(GameRunState run, DataManager data)
    {
        ClearList(ownedListRoot);
        int slot1 = SkillSwap.GetSlotSkillId(0, run);
        int slot2 = SkillSwap.GetSlotSkillId(1, run);
        slot1Label.text = SlotLabel(slot1, data);
        slot2Label.text = SlotLabel(slot2, data);
        WireSlotButton(slot1Button, slot1);
        WireSlotButton(slot2Button, slot2);
        WireClearButton(slot1ClearButton, 0, run, data);
        WireClearButton(slot2ClearButton, 1, run, data);

        if (run.Player.OwnedSkillIds == null)
        {
            return;
        }

        var owned = new System.Collections.Generic.List<int>(run.Player.OwnedSkillIds);
        owned.Sort();
        foreach (int skillId in owned)
        {
            if (
                !data.Skills.TryGetValue(skillId, out SkillData skill)
                || skill.SkillType != SkillType.Active
            )
            {
                continue;
            }

            bool equipped = slot1 == skillId || slot2 == skillId;
            UIListEntryFactory.SpawnListEntry(
                skillEntryPrefab,
                ownedListRoot,
                skill.NameKey + (equipped ? "  Equipped" : string.Empty),
                () => infoPanel.Show(skillId, SkillDetailContext.GuildSlot),
                "[GuildPage] skillEntryPrefab 하위 텍스트 참조가 없습니다."
            );
        }
    }

    private void WireStaticButtons()
    {
        buyTabButton.onClick.RemoveAllListeners();
        buyTabButton.onClick.AddListener(() =>
        {
            SetTab(GuildTab.Buy);
            Refresh();
        });
        slotTabButton.onClick.RemoveAllListeners();
        slotTabButton.onClick.AddListener(() =>
        {
            SetTab(GuildTab.Slot);
            Refresh();
        });
    }

    private void SetTab(GuildTab tab)
    {
        buyPanel.SetActive(tab == GuildTab.Buy);
        slotPanel.SetActive(tab == GuildTab.Slot);
    }

    private void OnGoldChanged(int value)
    {
        goldLabel.text = Loc.Format("shop_gold_fmt", value);
    }

    private void WireSlotButton(Button button, int skillId)
    {
        button.onClick.RemoveAllListeners();
        button.interactable = skillId != 0;
        if (skillId != 0)
        {
            button.onClick.AddListener(() =>
                infoPanel.Show(skillId, SkillDetailContext.GuildSlot)
            );
        }
    }

    private void WireClearButton(
        Button button,
        int slotIndex,
        GameRunState run,
        DataManager data
    )
    {
        button.onClick.RemoveAllListeners();
        button.interactable = SkillSwap.GetSlotSkillId(slotIndex, run) != 0;
        button.onClick.AddListener(() =>
        {
            if (SkillSwap.TryClearSlot(slotIndex, run, data))
            {
                Refresh();
            }
        });
    }

    private static string SlotLabel(int skillId, DataManager data)
    {
        if (skillId == 0)
        {
            return "(Empty)";
        }

        return data.Skills.TryGetValue(skillId, out SkillData skill)
            ? skill.NameKey
            : "Missing " + skillId;
    }

    private static void ClearList(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}
