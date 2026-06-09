// Guid3 §6 2026-05-27: ActiveSkills 슬롯 교체/해제 단일 헬퍼.
// - OwnedSkillIds 검사 + SkillType / AcquireType 검사 + 전투 중 가드.
// - PlayerState.ActiveSlotSkillIds 와 Player MonoBehaviour 의 ActiveSkills 양쪽을 동기화한다.
// - ActivePlayer (GameSystemManager) 가 null 인 비전투 상황에서는 PlayerState 만 갱신 후
//   다음 BindState 가 EntityBase.ActiveSkills 를 따라잡는다(Guid3 §10 W-G3-1 정책).
/// <summary>
/// ActiveSkills 슬롯 2칸의 장착 / 해제 단일 진입점.
/// </summary>
public static class SkillSwap
{
    /// <summary>슬롯에 보유 스킬을 장착한다. Safe UI 에서는 Player MonoBehaviour 없이 이 overload 를 사용한다.</summary>
    public static bool TrySetSlot(int slotIndex, int skillId, GameRunState run, DataManager data)
    {
        if (!TryResolveInputs(slotIndex, run, data, out PlayerState state))
        {
            return false;
        }

        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.CombatContext != null)
        {
            return false;
        }

        if (skillId == 0)
        {
            GameLog.LogError("[SkillSwap.TrySetSlot] skillId == 0. 빈 슬롯은 TryClearSlot 을 사용해야 합니다.");
            return false;
        }

        if (state.OwnedSkillIds == null || !state.OwnedSkillIds.Contains(skillId))
        {
            return false;
        }

        if (!data.Skills.TryGetValue(skillId, out SkillData skill) || skill == null)
        {
            GameLog.LogError("[SkillSwap.TrySetSlot] 스킬 ID 없음: " + skillId);
            return false;
        }

        if (skill.SkillType == SkillType.Passive || skill.AcquireType == AcquireType.MonsterOnly)
        {
            return false;
        }

        for (int i = 0; i < state.ActiveSlotSkillIds.Length; i++)
        {
            if (i != slotIndex && state.ActiveSlotSkillIds[i] == skillId)
            {
                state.ActiveSlotSkillIds[i] = 0;
                SyncActivePlayerSlot(i, null);
            }
        }

        state.ActiveSlotSkillIds[slotIndex] = skillId;
        SyncActivePlayerSlot(slotIndex, new Skill(skill));
        RaiseSkillsChanged();
        return true;
    }

    /// <summary>슬롯에 OwnedSkillIds 의 스킬을 장착. 다른 슬롯에 같은 스킬이 있으면 그 슬롯을 비운다.</summary>
    public static bool TrySetSlot(Player player, int slotIndex, int skillId)
    {
        if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
        {
            GameLog.LogError("[SkillSwap.TrySetSlot] GameSystemManager 참조가 없습니다.");
            return false;
        }

        bool changed = TrySetSlot(slotIndex, skillId, gsm.CurrentRun, gsm.Data);
        if (changed && player != null && gsm.Data.Skills.TryGetValue(skillId, out SkillData skill))
        {
            player.SetActiveSkill(slotIndex, new Skill(skill));
        }

        return changed;
    }

    /// <summary>슬롯을 비운다. 이미 0 이면 false.</summary>
    public static bool TryClearSlot(int slotIndex, GameRunState run, DataManager data)
    {
        if (!TryResolveInputs(slotIndex, run, data, out PlayerState state))
        {
            return false;
        }

        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.CombatContext != null)
        {
            return false;
        }

        if (state.ActiveSlotSkillIds[slotIndex] == 0)
        {
            return false;
        }

        state.ActiveSlotSkillIds[slotIndex] = 0;
        SyncActivePlayerSlot(slotIndex, null);
        RaiseSkillsChanged();
        return true;
    }

    /// <summary>슬롯을 비운다. 이미 0 이면 false.</summary>
    public static bool TryClearSlot(Player player, int slotIndex)
    {
        if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
        {
            GameLog.LogError("[SkillSwap.TryClearSlot] GameSystemManager 참조가 없습니다.");
            return false;
        }

        bool changed = TryClearSlot(slotIndex, gsm.CurrentRun, gsm.Data);
        if (changed && player != null)
        {
            player.SetActiveSkill(slotIndex, null);
        }

        return changed;
    }

    /// <summary>현재 슬롯에 들어 있는 SkillId. 슬롯 범위 벗어나면 0.</summary>
    public static int GetSlotSkillId(int slotIndex, GameRunState run)
    {
        if (run?.Player?.ActiveSlotSkillIds == null || slotIndex < 0 || slotIndex >= run.Player.ActiveSlotSkillIds.Length)
        {
            return 0;
        }

        return run.Player.ActiveSlotSkillIds[slotIndex];
    }

    /// <summary>현재 슬롯에 들어 있는 SkillId. 슬롯 범위 벗어나면 0.</summary>
    public static int GetSlotSkillId(Player player, int slotIndex)
    {
        return GameSystemManager.TryGetInstance(out GameSystemManager gsm) ? GetSlotSkillId(slotIndex, gsm.CurrentRun) : 0;
    }

    private static bool TryResolveInputs(int slotIndex, GameRunState run, DataManager data, out PlayerState state)
    {
        state = null;
        if (slotIndex < 0 || slotIndex >= 2)
        {
            GameLog.LogError("[SkillSwap] slotIndex 범위 오류: " + slotIndex);
            return false;
        }

        if (run?.Player == null)
        {
            GameLog.LogError("[SkillSwap] run / Player 참조가 없습니다.");
            return false;
        }

        if (data?.Skills == null)
        {
            GameLog.LogError("[SkillSwap] DataManager.Skills 참조가 없습니다.");
            return false;
        }

        state = run.Player;
        if (state.ActiveSlotSkillIds == null || state.ActiveSlotSkillIds.Length != 2)
        {
            GameLog.LogError("[SkillSwap] PlayerState.ActiveSlotSkillIds 가 없거나 길이가 2가 아닙니다.");
            return false;
        }

        return true;
    }

    private static void SyncActivePlayerSlot(int slotIndex, Skill skill)
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.ActivePlayer != null)
        {
            gsm.ActivePlayer.SetActiveSkill(slotIndex, skill);
        }
    }

    private static void RaiseSkillsChanged()
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
        {
            gsm.Events?.RaiseSkillsChanged();
        }
    }
}
